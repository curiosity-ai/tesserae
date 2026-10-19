using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single-line text input that suggests values as the user types and can check the typed value
    /// asynchronously. The text stays free: a suggestion is a shortcut, not a constraint, which is what
    /// sets it apart from a searchable <see cref="Dropdown"/>.
    /// </summary>
    /// <remarks>
    /// Suggestions come from a fixed list or from a loader that runs once, on first use, and is cached
    /// until <see cref="ReloadSuggestions"/>; either way they are filtered here, prefix matches first.
    /// The check (<see cref="CheckAsync"/>) runs debounced after every edit and once for the initial
    /// text, and only the answer for the text currently in the box is shown. Its result is drawn on a
    /// line of its own under the input, apart from <see cref="Input{TInput}.Error"/>, so a synchronous
    /// <c>.Validation(...)</c> on the same box keeps working.
    /// </remarks>
    [Transpose.Name("tss.AutoCompleteBox")]
    public sealed class AutoCompleteBox : Input<AutoCompleteBox>, IRoundedStyle
    {
        private static int _instanceCounter;

        private readonly HTMLElement _list;
        private readonly HTMLElement _status;
        private readonly HTMLElement _statusIcon;
        private readonly HTMLElement _statusText;
        private readonly Popover     _popover;
        private readonly string      _listId;

        private string[]                        _suggestions = new string[0];
        private Func<Task<string[]>>            _suggestionsLoader;
        private Task<string[]>                  _suggestionsLoading;
        private int                             _maxSuggestions = 50;
        private Action<AutoCompleteBox, string> _onSuggestionSelected;

        private readonly List<HTMLElement> _rows    = new List<HTMLElement>();
        private readonly List<string>      _visible = new List<string>();
        private int                        _highlighted = -1;

        private Func<string, Task<AutoCompleteCheck>> _check;
        private int                                   _checkDebounceMs = 400;
        private double                                _checkTimeout;
        private int                                   _checkRequest;

        /// <summary>Creates the box with an optional initial text.</summary>
        public AutoCompleteBox(string text = string.Empty) : base("text", text)
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
            InnerElement.setAttribute("autocomplete", "off");
            InnerElement.setAttribute("role",              "combobox");
            InnerElement.setAttribute("aria-autocomplete", "list");
            InnerElement.setAttribute("aria-expanded",     "false");

            _listId = "tss-autocomplete-list-" + (++_instanceCounter);
            InnerElement.setAttribute("aria-controls", _listId);

            _list = Div(Att("tss-autocomplete-list", id: _listId, role: "listbox"));

            // Keeps the focus in the input while a row (or the list's scrollbar) is pressed, so picking
            // a suggestion does not blur the box and close the list before the click lands.
            _list.addEventListener("mousedown", e => e.preventDefault());

            _statusIcon = I(Att("tss-autocomplete-status-icon"));
            _statusText = Span(Att("tss-autocomplete-status-text"));
            _status     = Div(Att("tss-autocomplete-status"), _statusIcon, _statusText);
            _status.setAttribute("aria-live", "polite");

            var container = Render();
            container.classList.add("tss-autocomplete");
            container.appendChild(_status);

            _popover = new Popover(Raw(_list))
               .Placement(TooltipPlacement.BottomStart)
               .Offset(0, 4)
               .OnHidden(() =>
                {
                    InnerElement.setAttribute("aria-expanded", "false");
                    InnerElement.removeAttribute("aria-activedescendant");
                    _highlighted = -1;
                });

            SubscribeInputUpdated((_, __) =>
            {
                if (document.activeElement == InnerElement) ShowSuggestionsAsync().FireAndForget();
                ScheduleCheck();
            });

            SubscribeReceivedFocus((_, __) => ShowSuggestionsAsync().FireAndForget());
            SubscribeClicked((_, __) => { if (!_popover.IsVisible) ShowSuggestionsAsync().FireAndForget(); });
            SubscribeLostFocus((_, __) => _popover.Hide());
            SubscribeKeyDown((_, e) => OnKeyDown(e));
        }

        /// <summary>Gets or sets the placeholder text.</summary>
        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        /// <summary>Sets the placeholder text.</summary>
        public AutoCompleteBox SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary>Sets a fixed list of suggestions, replacing any loader set before.</summary>
        public AutoCompleteBox Suggestions(params string[] suggestions)
        {
            _suggestions        = Distinct(suggestions);
            _suggestionsLoader  = null;
            _suggestionsLoading = null;
            RefreshIfOpen();
            return this;
        }

        /// <summary>
        /// Sets a loader for the suggestions. It runs the first time the list is needed (on focus or the
        /// first keystroke) and its answer is kept until <see cref="ReloadSuggestions"/>; the component
        /// filters it as the user types, so the loader returns everything it has.
        /// </summary>
        public AutoCompleteBox Suggestions(Func<Task<string[]>> loader)
        {
            _suggestionsLoader  = loader;
            _suggestionsLoading = null;
            _suggestions        = new string[0];
            return this;
        }

        /// <summary>
        /// Drops the cached answer of the suggestions loader, so the next use loads it again - after a
        /// setting the list depends on has changed. Refreshes the list at once if it is open.
        /// </summary>
        public AutoCompleteBox ReloadSuggestions()
        {
            _suggestionsLoading = null;
            RefreshIfOpen();
            return this;
        }

        /// <summary>Caps how many suggestions the list shows at once. Defaults to 50.</summary>
        public AutoCompleteBox MaxSuggestions(int max)
        {
            _maxSuggestions = Math.Max(1, max);
            return this;
        }

        /// <summary>
        /// Called when the user picks a suggestion (click or Enter), after the text has been set. The
        /// box also raises its change event then, so a handler on <c>OnChange</c> sees a pick and an
        /// edit alike.
        /// </summary>
        public AutoCompleteBox OnSuggestionSelected(Action<AutoCompleteBox, string> onSelected)
        {
            _onSuggestionSelected += onSelected;
            return this;
        }

        /// <summary>
        /// Checks the text asynchronously after each edit, debounced by <paramref name="debounceMs"/>,
        /// and shows the answer under the box: <see cref="AutoCompleteCheck.Valid"/>,
        /// <see cref="AutoCompleteCheck.Warning"/>, <see cref="AutoCompleteCheck.Invalid"/>, or
        /// <see cref="AutoCompleteCheck.None"/> for nothing. An empty box is not checked. The initial
        /// text is checked straight away.
        /// </summary>
        public AutoCompleteBox CheckAsync(Func<string, Task<AutoCompleteCheck>> check, int debounceMs = 400)
        {
            _check           = check;
            _checkDebounceMs = Math.Max(0, debounceMs);
            Recheck();
            return this;
        }

        /// <summary>Runs the check now for the current text, without waiting for the debounce.</summary>
        public AutoCompleteBox Recheck()
        {
            window.clearTimeout(_checkTimeout);
            RunCheckAsync().FireAndForget();
            return this;
        }

        /// <summary>The answer of the last check that completed for the current text, or <c>null</c>.</summary>
        public AutoCompleteCheck LastCheck { get; private set; }

        private void ScheduleCheck()
        {
            if (_check is null) return;

            window.clearTimeout(_checkTimeout);
            _checkRequest++;

            if (string.IsNullOrWhiteSpace(Text))
            {
                ShowCheck(null);
                return;
            }

            _checkTimeout = window.setTimeout(_ => RunCheckAsync().FireAndForget(), _checkDebounceMs);
        }

        private async Task RunCheckAsync()
        {
            if (_check is null) return;

            var request = ++_checkRequest;
            var text    = Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                ShowCheck(null);
                return;
            }

            ShowChecking();

            var result = await _check(text);

            // A later edit started its own check; its answer is the one the box should show.
            if (request != _checkRequest) return;

            ShowCheck(result);
        }

        private void ShowChecking()
        {
            _status.className      = "tss-autocomplete-status tss-autocomplete-status-checking";
            _statusIcon.className  = "tss-autocomplete-status-icon " + Icon.Transform(UIcons.Spinner, UIconsWeight.Regular);
            _statusText.innerText  = "Checking…";
            _status.title          = "";
        }

        private void ShowCheck(AutoCompleteCheck result)
        {
            LastCheck = result;

            if (result is null || result.State == AutoCompleteCheckState.None)
            {
                _status.className     = "tss-autocomplete-status";
                _statusIcon.className = "tss-autocomplete-status-icon";
                _statusText.innerText = "";
                _status.title         = "";
                return;
            }

            UIcons icon;
            string state;

            switch (result.State)
            {
                case AutoCompleteCheckState.Valid:   icon = UIcons.CheckCircle;     state = "valid";   break;
                case AutoCompleteCheckState.Warning: icon = UIcons.TriangleWarning; state = "warning"; break;
                default:                             icon = UIcons.CrossCircle;     state = "invalid"; break;
            }

            _status.className     = "tss-autocomplete-status tss-autocomplete-status-visible tss-autocomplete-status-" + state;
            _statusIcon.className = "tss-autocomplete-status-icon " + Icon.Transform(icon, UIconsWeight.Regular);
            _statusText.innerText = result.Message ?? "";
            _status.title         = result.Message ?? "";
        }

        private async Task<string[]> GetSuggestionsAsync()
        {
            if (_suggestionsLoader is null) return _suggestions;

            if (_suggestionsLoading is null)
            {
                var loading = _suggestionsLoader();
                _suggestionsLoading = loading;
                var loaded = Distinct(await loading);

                // A reload or another loader replaced this one while it ran.
                if (_suggestionsLoading != loading) return _suggestions;

                _suggestions = loaded;
            }
            else
            {
                await _suggestionsLoading;
            }

            return _suggestions;
        }

        private void RefreshIfOpen()
        {
            if (_popover.IsVisible) ShowSuggestionsAsync().FireAndForget();
        }

        private async Task ShowSuggestionsAsync()
        {
            if (!IsEnabled || InnerElement.hasAttribute("readonly")) return;

            var all   = await GetSuggestionsAsync();
            var query = Text ?? "";

            // The focus may have moved on while a loader ran.
            if (document.activeElement != InnerElement)
            {
                _popover.Hide();
                return;
            }

            var matches = Filter(all, query);

            // A list holding only what is already typed has nothing to offer.
            if (matches.Count == 0 || (matches.Count == 1 && string.Equals(matches[0], query, StringComparison.Ordinal)))
            {
                _popover.Hide();
                return;
            }

            RenderRows(matches, query);

            var width = (int)Math.Ceiling(InnerElement.getBoundingClientRect().As<DOMRect>().width);
            _list.style.width = width + "px";

            if (_popover.IsVisible)
            {
                Tippy.CheckRepositionNeeded(_list);
            }
            else
            {
                _popover.MaxWidth(Math.Max(width, 1)).ShowFor(InnerElement);
                InnerElement.setAttribute("aria-expanded", _popover.IsVisible ? "true" : "false");
            }
        }

        private List<string> Filter(string[] all, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return all.Take(_maxSuggestions).ToList();

            var prefix   = new List<string>();
            var contains = new List<string>();

            foreach (var s in all)
            {
                var index = s.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                if (index == 0) prefix.Add(s);
                else if (index > 0) contains.Add(s);
            }

            return prefix.Concat(contains).Take(_maxSuggestions).ToList();
        }

        private void RenderRows(List<string> matches, string query)
        {
            ClearChildren(_list);
            _rows.Clear();
            _visible.Clear();
            _highlighted = -1;
            InnerElement.removeAttribute("aria-activedescendant");

            for (var i = 0; i < matches.Count; i++)
            {
                var value = matches[i];
                var row   = Div(Att("tss-autocomplete-item", id: _listId + "-" + i, role: "option"));
                row.setAttribute("aria-selected", "false");
                AppendHighlighted(row, value, query);

                var index = i;
                row.addEventListener("click",     _ => Pick(value));
                row.addEventListener("mousemove", _ => { if (_highlighted != index) Highlight(index, scroll: false); });

                _list.appendChild(row);
                _rows.Add(row);
                _visible.Add(value);
            }
        }

        private static void AppendHighlighted(HTMLElement row, string value, string query)
        {
            var index = string.IsNullOrEmpty(query) ? -1 : value.IndexOf(query, StringComparison.OrdinalIgnoreCase);

            if (index < 0)
            {
                row.appendChild(document.createTextNode(value));
                return;
            }

            if (index > 0) row.appendChild(document.createTextNode(value.Substring(0, index)));
            row.appendChild(Span(Att("tss-autocomplete-match", text: value.Substring(index, query.Length))));
            if (index + query.Length < value.Length) row.appendChild(document.createTextNode(value.Substring(index + query.Length)));
        }

        private void Highlight(int index, bool scroll = true)
        {
            if (_highlighted >= 0 && _highlighted < _rows.Count)
            {
                _rows[_highlighted].classList.remove("tss-autocomplete-item-highlight");
                _rows[_highlighted].setAttribute("aria-selected", "false");
            }

            _highlighted = index;

            if (index < 0 || index >= _rows.Count)
            {
                InnerElement.removeAttribute("aria-activedescendant");
                return;
            }

            var row = _rows[index];
            row.classList.add("tss-autocomplete-item-highlight");
            row.setAttribute("aria-selected", "true");
            InnerElement.setAttribute("aria-activedescendant", row.id);

            if (scroll) row.scrollIntoView(new ScrollIntoViewOptions { block = ScrollLogicalPosition.nearest });
        }

        private void OnKeyDown(KeyboardEvent e)
        {
            switch (e.key)
            {
                case "ArrowDown":
                case "ArrowUp":
                {
                    e.preventDefault();

                    if (!_popover.IsVisible)
                    {
                        ShowSuggestionsAsync().FireAndForget();
                        return;
                    }

                    if (_rows.Count == 0) return;

                    var step = e.key == "ArrowDown" ? 1 : -1;
                    var next = _highlighted < 0 ? (step > 0 ? 0 : _rows.Count - 1) : (_highlighted + step + _rows.Count) % _rows.Count;
                    Highlight(next);
                    break;
                }
                case "Enter":
                {
                    if (_popover.IsVisible && _highlighted >= 0 && _highlighted < _visible.Count)
                    {
                        e.preventDefault();
                        Pick(_visible[_highlighted]);
                    }
                    break;
                }
                case "Tab":
                {
                    _popover.Hide();
                    break;
                }
            }
        }

        private void Pick(string value)
        {
            _popover.Hide();

            Text = value; // Raises the input event, which schedules the check
            RaiseOnChange(null);
            _onSuggestionSelected?.Invoke(this, value);
        }

        private static string[] Distinct(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>()).Where(v => !string.IsNullOrEmpty(v)).Distinct().ToArray();
        }
    }

    /// <summary>What an <see cref="AutoCompleteBox"/> check says about the text in the box.</summary>
    [Transpose.Name("tss.AutoCompleteCheckState")]
    public enum AutoCompleteCheckState
    {
        /// <summary>Nothing to say; the status line stays empty.</summary>
        None,
        /// <summary>The value is known to be good.</summary>
        Valid,
        /// <summary>The value may be wrong, but is allowed.</summary>
        Warning,
        /// <summary>The value is wrong.</summary>
        Invalid,
    }

    /// <summary>The answer of an <see cref="AutoCompleteBox.CheckAsync"/> callback.</summary>
    [Transpose.Name("tss.AutoCompleteCheck")]
    public sealed class AutoCompleteCheck
    {
        private AutoCompleteCheck(AutoCompleteCheckState state, string message)
        {
            State   = state;
            Message = message;
        }

        /// <summary>The verdict.</summary>
        public AutoCompleteCheckState State   { get; }
        /// <summary>The message shown next to the verdict's icon, or <c>null</c>.</summary>
        public string                 Message { get; }

        /// <summary>No verdict: clears the status line.</summary>
        public static AutoCompleteCheck None()                       => new AutoCompleteCheck(AutoCompleteCheckState.None,    null);
        /// <summary>The value is good.</summary>
        public static AutoCompleteCheck Valid(string message = null) => new AutoCompleteCheck(AutoCompleteCheckState.Valid,   message);
        /// <summary>The value may be wrong, but is allowed.</summary>
        public static AutoCompleteCheck Warning(string message)      => new AutoCompleteCheck(AutoCompleteCheckState.Warning, message);
        /// <summary>The value is wrong.</summary>
        public static AutoCompleteCheck Invalid(string message)      => new AutoCompleteCheck(AutoCompleteCheckState.Invalid, message);
    }
}
