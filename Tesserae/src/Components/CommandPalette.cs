using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A keyboard-driven full-screen command launcher (Ctrl/Cmd-K style) that lets users search and invoke
    /// application commands.
    /// </summary>
    [Transpose.Name("tss.CommandPalette")]
    public sealed class CommandPalette : Layer<CommandPalette>
    {
        private readonly HTMLDivElement _overlay;
        private readonly HTMLDivElement _positioner;
        private readonly HTMLDivElement _animator;
        private readonly HTMLDivElement _searchContainer;
        private readonly HTMLInputElement _searchInput;
        private readonly HTMLDivElement _breadcrumbs;
        private readonly HTMLButtonElement _backButton;
        private readonly HTMLSpanElement _pathText;
        private readonly HTMLDivElement _results;
        private readonly HTMLDivElement _emptyState;

        private readonly List<CommandPaletteAction> _actions = new List<CommandPaletteAction>();
        private readonly Dictionary<string, CommandPaletteAction> _actionLookup = new Dictionary<string, CommandPaletteAction>();

        // One entry per navigable row, actions and host results alike, so the arrow keys walk the list the
        // user sees rather than the actions inside it.
        private readonly List<PaletteEntry> _entries = new List<PaletteEntry>();
        private readonly List<HTMLElement> _entryElements = new List<HTMLElement>();

        private readonly List<CommandPaletteResult> _hostResults = new List<CommandPaletteResult>();
        private Func<string, Task<IEnumerable<CommandPaletteResult>>> _search;
        private int _searchDebounceMs = 200;
        private double _searchTimer = -1;
        private int _searchGeneration;

        private string _currentParentId;
        private int _activeIndex = -1;
        private HTMLElement _focusBeforeShow;

        /// <summary>How long <see cref="Layer{T}"/> takes to fade a hidden layer out before removing it.</summary>
        private const int LAYER_FADE_OUT_MS = 150;

        private sealed class PaletteEntry
        {
            public CommandPaletteAction Action { get; set; }
            public CommandPaletteResult Result { get; set; }
        }

        private readonly Action<Event> _globalKeyDownHandler;
        private bool _globalListenerActive;

        /// <summary>
        /// Raised when action executed occurs.
        /// </summary>
        public event Action<CommandPaletteAction> ActionExecuted;

        /// <summary>
        /// Raised when one of the host's own rows is activated - see <see cref="SetResults(IEnumerable{CommandPaletteResult})"/>.
        /// </summary>
        public event Action<CommandPaletteResult> ResultActivated;

        /// <summary>
        /// Enables the global shortcut on the component.
        /// </summary>
        public bool EnableGlobalShortcut { get; set; } = true;
        /// <summary>
        /// Enables the global action shortcuts on the component.
        /// </summary>
        public bool EnableGlobalActionShortcuts { get; set; } = true;
        /// <summary>
        /// Hides the on action.
        /// </summary>
        public bool HideOnAction { get; set; } = true;

        /// <summary>
        /// Key (combined with Ctrl/Cmd) that toggles the palette globally. Case-insensitive. Defaults to "k".
        /// </summary>
        public string GlobalShortcutKey { get; set; } = "k";

        /// <summary>
        /// Creates a CommandPalette whose global Ctrl/Cmd keyboard listener is bound
        /// to the lifetime of <paramref name="host"/>: the listener is attached when
        /// <paramref name="host"/> first mounts to the DOM and detached when it is
        /// removed. This prevents the palette from leaking listeners (and continuing
        /// to respond to its shortcut) after the owning view has been navigated away.
        /// </summary>
        public CommandPalette(IComponent host, IEnumerable<CommandPaletteAction> actions = null)
        {
            if (host is null) throw new ArgumentNullException(nameof(host));

            _searchInput = UI.TextBox(Att("tss-commandpalette-search", type: "search", placeholder: "Type a command"));
            _searchInput.setAttribute("aria-label", "Command palette search");
            _searchInput.addEventListener("input", _ => RefreshResults());
            _searchInput.addEventListener("keydown", e => HandleSearchKeyDown(e.As<KeyboardEvent>()));
            _searchInput.addEventListener("blur", e =>
            {
                if(_searchInput.IsMounted())
                {
                    StopEvent(e);
                    _searchInput.focus();
                }
            });
            _backButton = UI.Button(Att("tss-commandpalette-back tss-fontweight-semibold", type: "button", title: "Go Back"),
                                    Div(Att("tss-commandpalette-icon"), I(Att($"tss-commandpalette-icon-item {UIcons.AngleLeft}"))));

            _backButton.addEventListener("click", e =>
            {
                StopEvent(e);
                NavigateBack();
            });

            _pathText = Span(Att("tss-commandpalette-path tss-fontweight-semibold"));
            _breadcrumbs = Div(Att("tss-commandpalette-breadcrumbs"), _backButton, _pathText);

            _searchContainer = Div(Att("tss-commandpalette-search-container"), _breadcrumbs, _searchInput);
            _results = Div(Att("tss-commandpalette-results", role: "listbox"));
            _emptyState = Div(Att("tss-commandpalette-empty", text: "No results"));

            _animator = Div(Att("tss-commandpalette-animator"), _searchContainer, _results, _emptyState);
            _positioner = Div(Att("tss-commandpalette-positioner"), _animator);
            _overlay = Div(Att("tss-commandpalette-overlay"));
            _overlay.addEventListener("click", e =>
            {
                StopEvent(e);
                Hide();
            });

            _contentHtml = Div(Att("tss-commandpalette-container"), _overlay, _positioner);

            SetActions(actions);

            _globalKeyDownHandler = HandleGlobalKeyDown;
            host.WhenMounted(() =>
            {
                if (_globalListenerActive) return;
                window.addEventListener("keydown", _globalKeyDownHandler);
                _globalListenerActive = true;
                host.WhenRemoved(() =>
                {
                    if (!_globalListenerActive) return;
                    window.removeEventListener("keydown", _globalKeyDownHandler);
                    _globalListenerActive = false;
                    if (IsVisible) Hide();
                });
            });

            InnerElement    = _contentHtml;

        }

        /// <summary>
        /// Gets or sets the placeholder text shown when the component is empty.
        /// </summary>
        public string Placeholder
        {
            get => _searchInput.placeholder;
            set => _searchInput.placeholder = value ?? string.Empty;
        }

        /// <summary>
        /// What is said when there is nothing to show. Defaults to "No results", which is right for a
        /// palette that filters a list it already has; a palette that goes and searches usually wants to
        /// say what to do instead ("Type to search").
        /// </summary>
        public string EmptyText
        {
            get => _emptyState.innerText;
            set => _emptyState.innerText = value ?? string.Empty;
        }

        /// <summary>
        /// Sets the actions of the component.
        /// </summary>
        public CommandPalette SetActions(IEnumerable<CommandPaletteAction> actions)
        {
            _actions.Clear();
            if (actions != null)
            {
                _actions.AddRange(actions.Where(a => a != null));
            }
            RebuildLookup();
            RefreshResults();
            return this;
        }

        /// <summary>
        /// Adds the given action to the component.
        /// </summary>
        public CommandPalette AddAction(CommandPaletteAction action)
        {
            if (action == null)
            {
                return this;
            }

            _actions.Add(action);
            if (!string.IsNullOrWhiteSpace(action.Id))
            {
                _actionLookup[action.Id] = action;
            }
            RefreshResults();
            return this;
        }

        /// <summary>
        /// Puts rows of the host's own at the top of the palette, above its actions - search results drawn
        /// as <see cref="OmniResult{T}"/>s, recent files, anything a list of actions can't say. Each carries
        /// what Enter does with it, and they take part in the arrow-key walk like any other row.
        /// <para>
        /// These are shown as given: the palette does not filter them, because the host that produced them
        /// for a query already knows which ones answer it. Use <see cref="OnSearch"/> to have them refreshed
        /// as the query changes, or call this whenever the host has new ones.
        /// </para>
        /// </summary>
        public CommandPalette SetResults(IEnumerable<CommandPaletteResult> results)
        {
            _hostResults.Clear();

            if (results != null) _hostResults.AddRange(results.Where(r => r != null));

            RenderEntries();

            return this;
        }

        /// <summary>Puts one row of the host's own at the top of the palette. See <see cref="SetResults"/>.</summary>
        public CommandPalette SetResults(params CommandPaletteResult[] results) => SetResults((IEnumerable<CommandPaletteResult>)results);

        /// <summary>
        /// Asks the host for the rows to show, every time the query changes and once when the palette opens.
        /// The call is debounced, and an answer that arrives after a newer query was typed is dropped, so a
        /// slow search can never overwrite a faster one behind it.
        /// </summary>
        /// <param name="search">Given the current query, the rows to show. Null clears the search.</param>
        /// <param name="debounceMs">How long typing has to stop before the search runs.</param>
        public CommandPalette OnSearch(Func<string, Task<IEnumerable<CommandPaletteResult>>> search, int debounceMs = 200)
        {
            _search           = search;
            _searchDebounceMs = debounceMs < 0 ? 0 : debounceMs;

            if (_search is null)
            {
                CancelPendingSearch();
                SetResults((IEnumerable<CommandPaletteResult>)null);
            }
            else if (IsVisible)
            {
                RunSearch(CurrentQuery);
            }

            return this;
        }

        /// <summary>What is typed in the palette's search box right now, trimmed.</summary>
        public string CurrentQuery => _searchInput.value?.Trim() ?? string.Empty;

        /// <summary>
        /// Opens the component.
        /// </summary>
        public CommandPalette Open()
        {
            Show();
            return this;
        }

        /// <summary>
        /// Closes the component.
        /// </summary>
        public CommandPalette Close()
        {
            Hide();
            return this;
        }

        /// <summary>
        /// Toggles the component's state.
        /// </summary>
        public CommandPalette Toggle()
        {
            if (IsVisible) Hide();
            else Show();
            return this;
        }

        /// <summary>
        /// Shows the component.
        /// </summary>
        public override CommandPalette Show()
        {
            //Whatever had the focus gets it back when the palette closes - see Hide.
            _focusBeforeShow = document.activeElement.As<HTMLElement>();

            base.Show();
            ResetState();
            window.setTimeout(_ => _searchInput.focus(), 0);
            return this;
        }

        /// <summary>
        /// Hides the component.
        /// </summary>
        public override void Hide(Action onHidden = null)
        {
            var restoreTo = _focusBeforeShow;

            _focusBeforeShow = null;

            base.Hide(onHidden);

            //The palette holds the focus while it is open and its search box leaves the document with it,
            //so closing it leaves the page with nothing focused - and the browser then takes the next
            //Ctrl+K for itself and drops the caret in the address bar. The document has to be given the
            //focus back, actively, once the layer is actually gone: the layer fades out on a timer, so
            //doing it only now would put the focus on something that is about to be removed.
            RestoreFocus(restoreTo);

            window.setTimeout(_ => RestoreFocus(restoreTo), 0);
            window.setTimeout(_ => RestoreFocus(restoreTo), LAYER_FADE_OUT_MS);
        }

        private void RestoreFocus(HTMLElement restoreTo)
        {
            var active = document.activeElement.As<HTMLElement>();

            //Something outside the palette has the focus - a modal an activated row opened, or the page
            //itself, already restored by an earlier pass - and it is more entitled to it than this.
            if (active is object && active != document.body && !_contentHtml.contains(active)) return;

            if (restoreTo is object && restoreTo != document.body && document.body.contains(restoreTo))
            {
                restoreTo.focus();
                return;
            }

            //Nothing to hand it back to, so the document holds it itself. body only accepts the focus once
            //it is focusable, and the window has to be told separately, or the page keeps reporting a
            //focused body while the browser goes on treating the keystrokes as its own.
            if (document.body.tabIndex < 0) document.body.tabIndex = -1;

            window.focus();
            document.body.focus();
        }

        private void ResetState()
        {
            _searchInput.value = string.Empty;
            _currentParentId = null;
            RefreshResults();
        }

        private void RebuildLookup()
        {
            _actionLookup.Clear();
            foreach (var action in _actions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.Id))
                {
                    continue;
                }
                _actionLookup[action.Id] = action;
            }
        }

        private void HandleGlobalKeyDown(Event ev)
        {
            if (!EnableGlobalShortcut && !EnableGlobalActionShortcuts)
            {
                return;
            }
            var e = ev.As<KeyboardEvent>();

            var target = (e.target is object ? e.target : e.srcElement).As<HTMLElement>();
            if (target != null && (target.isContentEditable || target.tagName == "INPUT" || target.tagName == "TEXTAREA" || target.tagName == "SELECT"))
            {
                return;
            }

            if (EnableGlobalShortcut && !string.IsNullOrEmpty(GlobalShortcutKey)
                && string.Equals(e.key, GlobalShortcutKey, StringComparison.OrdinalIgnoreCase)
                && (e.metaKey || e.ctrlKey))
            {
                StopEvent(e);
                Toggle();
                return;
            }

            if (!EnableGlobalActionShortcuts || IsVisible)
            {
                return;
            }

            var action = FindShortcutAction(e.key);
            if (action != null)
            {
                StopEvent(e);
                ExecuteAction(action);
            }
        }

        private void HandleSearchKeyDown(KeyboardEvent e)
        {
            if (e.key == "ArrowDown" || (e.ctrlKey && e.key == "n"))
            {
                StopEvent(e);
                MoveActive(1);
                return;
            }

            if (e.key == "ArrowUp" || (e.ctrlKey && e.key == "p"))
            {
                StopEvent(e);
                MoveActive(-1);
                return;
            }

            if (e.key == "Enter" | e.key == "Tab")
            {
                StopEvent(e);
                ActivateSelected();
                return;
            }

            if (e.key == "Escape")
            {
                StopEvent(e);
                
                if(_breadcrumbs.style.display != "none")
                {
                    NavigateBack();
                }
                else
                {
                    Hide();
                }
                return;
            }

            if (e.key == "Backspace" && string.IsNullOrEmpty(_searchInput.value) && !string.IsNullOrEmpty(_currentParentId))
            {
                StopEvent(e);
                NavigateBack();
                return;
            }
        }

        private void NavigateBack()
        {
            if (string.IsNullOrEmpty(_currentParentId))
            {
                return;
            }

            var current = _actionLookup.ContainsKey(_currentParentId) ? _actionLookup[_currentParentId] : null;
            _currentParentId = current?.ParentId;
            _searchInput.value = string.Empty;
            RefreshResults();
            window.setTimeout(_ => _searchInput.focus(), 0);
        }

        private void ActivateSelected()
        {
            if (_activeIndex < 0 || _activeIndex >= _entries.Count)
            {
                return;
            }

            var entry = _entries[_activeIndex];

            if (entry.Action is object) ExecuteAction(entry.Action);
            else                        ActivateResult(entry.Result);
        }

        private void ActivateResult(CommandPaletteResult result)
        {
            if (result is null || result.Activate is null) return;

            result.Activate.Invoke();
            ResultActivated?.Invoke(result);

            if (HideOnAction) Hide();
        }

        private void ExecuteAction(CommandPaletteAction action)
        {
            if (action == null || !action.IsEnabled)
            {
                return;
            }

            if (HasChildren(action))
            {
                _currentParentId = action.Id;
                _searchInput.value = string.Empty;
                RefreshResults();
                return;
            }

            action.Perform?.Invoke();
            ActionExecuted?.Invoke(action);
            if (HideOnAction)
            {
                Hide();
            }
        }

        private bool HasChildren(CommandPaletteAction action)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.Id))
            {
                return false;
            }

            return _actions.Any(a => a != null && a.ParentId == action.Id && a.IsVisible);
        }

        private void MoveActive(int delta)
        {
            if (_entryElements.Count == 0)
            {
                _activeIndex = -1;
                return;
            }

            var nextIndex = _activeIndex + delta;
            if (nextIndex < 0)
            {
                nextIndex = _entryElements.Count - 1;
            }
            else if (nextIndex >= _entryElements.Count)
            {
                nextIndex = 0;
            }

            SetActiveIndex(nextIndex);
        }

        private void SetActiveIndex(int index)
        {
            if (_entryElements.Count == 0)
            {
                _activeIndex = -1;
                return;
            }

            if (_activeIndex >= 0 && _activeIndex < _entryElements.Count)
            {
                _entryElements[_activeIndex].classList.remove("tss-active");
            }

            _activeIndex = index;
            if (_activeIndex >= 0 && _activeIndex < _entryElements.Count)
            {
                var activeEl = _entryElements[_activeIndex];
                activeEl.classList.add("tss-active");
                try
                {
                    activeEl.scrollIntoViewIfNeeded();
                }
                catch
                {
                    activeEl.scrollIntoView();
                }
            }
        }

        private void RefreshResults()
        {
            RenderEntries();
            ScheduleSearch();
        }

        private void RenderEntries()
        {
            var query   = CurrentQuery;
            var actions = FilterActions(query).ToList();

            _results.RemoveChildElements();
            _entries.Clear();
            _entryElements.Clear();

            string lastSection = null;

            //The host's own rows lead: they answer the query itself, where an action only offers to go
            //somewhere that might.
            foreach (var result in _hostResults)
            {
                lastSection = AppendSection(result.Section, lastSection);

                var item = BuildResultElement(result, _entryElements.Count);

                _results.appendChild(item);
                _entries.Add(new PaletteEntry { Result = result });
                _entryElements.Add(item);
            }

            foreach (var action in actions)
            {
                lastSection = AppendSection(action.Section, lastSection);

                var item = BuildActionElement(action, _entryElements.Count);

                _results.appendChild(item);
                _entries.Add(new PaletteEntry { Action = action });
                _entryElements.Add(item);
            }

            _emptyState.style.display = _entries.Count == 0 ? "block" : "none";
            UpdateBreadcrumbs();
            SetActiveIndex(_entries.Count > 0 ? 0 : -1);
        }

        private string AppendSection(string section, string lastSection)
        {
            if (string.IsNullOrWhiteSpace(section) || section == lastSection) return lastSection;

            _results.appendChild(Div(Att("tss-commandpalette-section", text: section)));

            return section;
        }

        private void ScheduleSearch()
        {
            if (_search is null) return;

            CancelPendingSearch();

            var query = CurrentQuery;

            if (_searchDebounceMs == 0)
            {
                RunSearch(query);
                return;
            }

            _searchTimer = window.setTimeout(_ =>
            {
                _searchTimer = -1;
                RunSearch(query);
            }, _searchDebounceMs);
        }

        private void CancelPendingSearch()
        {
            if (_searchTimer < 0) return;

            window.clearTimeout(_searchTimer);
            _searchTimer = -1;
        }

        private void RunSearch(string query)
        {
            var search = _search;

            if (search is null) return;

            //An answer to a query the user has already typed past is thrown away rather than replacing the
            //rows a later, faster search already put up.
            var generation = ++_searchGeneration;

            RunSearchAsync(search, query, generation).FireAndForget();
        }

        private async Task RunSearchAsync(Func<string, Task<IEnumerable<CommandPaletteResult>>> search, string query, int generation)
        {
            var results = await search(query);

            if (generation != _searchGeneration) return;

            SetResults(results);
        }

        private IEnumerable<CommandPaletteAction> FilterActions(string query)
        {
            IEnumerable<CommandPaletteAction> candidates;
            if (string.IsNullOrEmpty(query))
            {
                candidates = _actions.Where(a => a != null && a.IsVisible && IsParentMatch(a.ParentId, _currentParentId));
            }
            else
            {
                candidates = _actions.Where(a => a != null && a.IsVisible && MatchesQuery(a, query));
            }

            return candidates;
        }

        private bool IsParentMatch(string actionParent, string currentParent)
        {
            if (string.IsNullOrWhiteSpace(actionParent) && string.IsNullOrWhiteSpace(currentParent))
            {
                return true;
            }

            return string.Equals(actionParent, currentParent, StringComparison.Ordinal);
        }

        private bool MatchesQuery(CommandPaletteAction action, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            var haystack = string.Join(" ", new[] { action.Name, action.Subtitle, action.Keywords, action.Section }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLower();
            var terms = query.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return terms.All(term => haystack.Contains(term));
        }

        private HTMLElement BuildActionElement(CommandPaletteAction action, int index)
        {
            var iconContainer = Div(Att("tss-commandpalette-icon"));
            if (action.Icon.HasValue)
            {
                iconContainer.appendChild(I(Att($"tss-commandpalette-icon-item {action.Icon.Value}")));
            }
            else
            {
                iconContainer.appendChild(Span(Att("tss-commandpalette-icon-placeholder", text: "•")));
            }

            var title = Div(Att("tss-commandpalette-title", text: action.Name));
            var content = Div(Att("tss-commandpalette-content"), title);

            if (!string.IsNullOrWhiteSpace(action.Subtitle))
            {
                content.appendChild(Div(Att("tss-commandpalette-subtitle", text: action.Subtitle)));
            }

            var shortcuts = Div(Att("tss-commandpalette-shortcuts"));
            if (action.Shortcut != null && action.Shortcut.Length > 0)
            {
                foreach (var shortcut in action.Shortcut)
                {
                    shortcuts.appendChild(Span(Att("tss-commandpalette-shortcut", text: shortcut)));
                }
            }

            if (HasChildren(action))
            {
                shortcuts.appendChild(Span(Att("tss-commandpalette-submenu", text: "›")));
            }

            var item = Div(Att("tss-commandpalette-item", role: "option"), iconContainer, content, shortcuts);
            if (!action.IsEnabled)
            {
                item.classList.add("tss-disabled");
            }

            item.addEventListener("mousemove", _ => SetActiveIndex(index));
            item.addEventListener("click", e =>
            {
                StopEvent(e);
                ExecuteAction(action);
            });

            return item;
        }

        private HTMLElement BuildResultElement(CommandPaletteResult result, int index)
        {
            var item = Div(Att("tss-commandpalette-item tss-commandpalette-result", role: "option"));

            if (result.Component is object) item.appendChild(result.Component.Render());

            item.addEventListener("mousemove", _ => SetActiveIndex(index));

            //A row of the host's own usually answers its own click - an OmniResult opens what it stands for -
            //so the palette only steps in when it was given something to do, and gets out of the way after
            //either, because a palette that stays open over what it just opened is in the way.
            item.addEventListener("click", e =>
            {
                if (result.Activate is object)
                {
                    StopEvent(e);
                    ActivateResult(result);
                    return;
                }

                if (HideOnAction) Hide();
            });

            return item;
        }

        private CommandPaletteAction FindShortcutAction(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var normalized = key.ToLower();
            foreach (var action in _actions)
            {
                if (action == null || !action.IsVisible || !action.IsEnabled || action.Shortcut == null)
                {
                    continue;
                }

                if (action.Shortcut.Any(s => string.Equals(s, normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    return action;
                }
            }

            return null;
        }

        private void UpdateBreadcrumbs()
        {
            if (string.IsNullOrWhiteSpace(_currentParentId))
            {
                _breadcrumbs.style.display = "none";
                _pathText.innerText = string.Empty;
                return;
            }

            var names = new List<string>();
            var cursor = _currentParentId;
            while (!string.IsNullOrWhiteSpace(cursor) && _actionLookup.ContainsKey(cursor))
            {
                var action = _actionLookup[cursor];
                names.Add(action.Name);
                cursor = action.ParentId;
            }

            names.Reverse();
            _pathText.innerText = string.Join(" / ", names);
            _breadcrumbs.style.display = "flex";
        }
    }

    /// <summary>
    /// A row of the host's own in a <see cref="CommandPalette"/> - a search result drawn as an
    /// <see cref="OmniResult{T}"/>, a recent file, a preview card - rather than one of the palette's actions.
    /// </summary>
    [Transpose.Name("tss.CommandPaletteResult")]
    public sealed class CommandPaletteResult
    {
        /// <summary>
        /// A row and what Enter does with it. With no <paramref name="activate"/> the row is only clickable,
        /// which is what a component that already answers its own click (an <see cref="OmniResult{T}"/> with
        /// an <c>OpenWith</c>) wants.
        /// </summary>
        public CommandPaletteResult(IComponent component, Action activate = null)
        {
            Component = component;
            Activate  = activate;
        }

        /// <summary>The row itself.</summary>
        public IComponent Component { get; }

        /// <summary>What Enter - and a click, when it is given - does with the row.</summary>
        public Action Activate { get; }

        /// <summary>The heading this row sits under, when a palette groups its rows.</summary>
        public string Section { get; set; }
    }

    [Transpose.Name("tss.CommandPaletteAction")]
    public sealed class CommandPaletteAction
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CommandPaletteAction(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Sets the DOM id of the component.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        public string Subtitle { get; set; }
        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        public string Keywords { get; set; }
        /// <summary>
        /// Gets or sets the section.
        /// </summary>
        public string Section { get; set; }
        /// <summary>
        /// Gets or sets the parent id.
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// Gets or sets the icon shown by the component.
        /// </summary>
        public UIcons? Icon { get; set; }
        /// <summary>
        /// Gets or sets the shortcut.
        /// </summary>
        public string[] Shortcut { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        /// <summary>
        /// Gets a value indicating whether the component is currently visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
        /// <summary>
        /// Gets or sets the perform.
        /// </summary>
        public Action Perform { get; set; }
    }
}
