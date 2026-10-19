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
    /// The asynchronous check most such fields want is <see cref="Input{TInput}.CheckAsync"/>, shared
    /// with every input.
    /// </remarks>
    [Transpose.Name("tss.AutoCompleteBox")]
    public sealed class AutoCompleteBox : Input<AutoCompleteBox>, IRoundedStyle
    {
        private static int _instanceCounter;

        private const int MaxRows = 50;

        private readonly HTMLElement _list;
        private readonly Popover     _popover;
        private readonly string      _listId;

        private string[]                        _suggestions = new string[0];
        private Func<Task<string[]>>            _suggestionsLoader;
        private Task<string[]>                  _suggestionsLoading;
        private bool                            _picking;

        private readonly List<HTMLElement> _rows    = new List<HTMLElement>();
        private readonly List<string>      _visible = new List<string>();
        private int                        _highlighted = -1;

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

            Render().classList.add("tss-autocomplete");

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
                // A pick sets the text too; the list it just closed stays closed.
                if (!_picking && document.activeElement == InnerElement) ShowSuggestionsAsync().FireAndForget();
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

        /// <summary>Gets or sets whether the box is read-only; a read-only box shows no suggestions.</summary>
        public bool IsReadOnly
        {
            get => InnerElement.hasAttribute("readonly");
            set
            {
                if (value) InnerElement.setAttribute("readonly", string.Empty);
                else       InnerElement.removeAttribute("readonly");
            }
        }

        /// <summary>Sets the box as read-only.</summary>
        public AutoCompleteBox ReadOnly()
        {
            IsReadOnly = true;
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
            if (!IsEnabled || IsReadOnly) return;

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
            if (string.IsNullOrWhiteSpace(query)) return all.Take(MaxRows).ToList();

            var prefix   = new List<string>();
            var contains = new List<string>();

            foreach (var s in all)
            {
                var index = s.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                if (index == 0) prefix.Add(s);
                else if (index > 0) contains.Add(s);
            }

            return prefix.Concat(contains).Take(MaxRows).ToList();
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
                    // Tab leaves the box as usual; a row the user arrowed to is a choice they made, so it is
                    // picked on the way out. With none highlighted the typed text is kept.
                    if (_popover.IsVisible && _highlighted >= 0 && _highlighted < _visible.Count)
                    {
                        Pick(_visible[_highlighted]);
                        break;
                    }

                    _popover.Hide();
                    break;
                }
            }
        }

        private void Pick(string value)
        {
            _popover.Hide();

            _picking = true;
            try     { Text = value; } // Raises the input event, which schedules the check
            finally { _picking = false; }
            RaiseOnChange(null);
        }

        private static string[] Distinct(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>()).Where(v => !string.IsNullOrEmpty(v)).Distinct().ToArray();
        }
    }
}
