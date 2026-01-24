using System;
using System.Collections.Generic;
using System.Linq;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.CommandPalette")]
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
        private readonly List<CommandPaletteAction> _visibleActions = new List<CommandPaletteAction>();
        private readonly List<HTMLElement> _actionElements = new List<HTMLElement>();

        private string _currentParentId;
        private int _activeIndex = -1;

        public event Action<CommandPaletteAction> ActionExecuted;

        public bool EnableGlobalShortcut { get; set; } = true;
        public bool EnableGlobalActionShortcuts { get; set; } = true;
        public bool HideOnAction { get; set; } = true;

        public CommandPalette(IEnumerable<CommandPaletteAction> actions = null)
        {
            _searchInput = TextBox(_("tss-commandpalette-search", type: "search", placeholder: "Type a command"));
            _searchInput.setAttribute("aria-label", "Command palette search");
            _searchInput.addEventListener("input", _ => RefreshResults());
            _searchInput.addEventListener("keydown", e => HandleSearchKeyDown(e.As<KeyboardEvent>()));

            _backButton = Button(_("tss-commandpalette-back", type: "button"), Span(_(text: "Back")));
            _backButton.addEventListener("click", e =>
            {
                StopEvent(e);
                NavigateBack();
            });

            _pathText = Span(_("tss-commandpalette-path"));
            _breadcrumbs = Div(_("tss-commandpalette-breadcrumbs"), _backButton, _pathText);

            _searchContainer = Div(_("tss-commandpalette-search-container"), _breadcrumbs, _searchInput);
            _results = Div(_("tss-commandpalette-results", role: "listbox"));
            _emptyState = Div(_("tss-commandpalette-empty", text: "No results"));

            _animator = Div(_("tss-commandpalette-animator"), _searchContainer, _results, _emptyState);
            _positioner = Div(_("tss-commandpalette-positioner"), _animator);
            _overlay = Div(_("tss-commandpalette-overlay"));
            _overlay.addEventListener("click", e =>
            {
                StopEvent(e);
                Hide();
            });

            _contentHtml = Div(_("tss-commandpalette-layer"), _overlay, _positioner);

            SetActions(actions);
            window.addEventListener("keydown", e => HandleGlobalKeyDown(e.As<KeyboardEvent>()));
        }

        public string Placeholder
        {
            get => _searchInput.placeholder;
            set => _searchInput.placeholder = value ?? string.Empty;
        }

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

        public CommandPalette Open()
        {
            Show();
            return this;
        }

        public CommandPalette Close()
        {
            Hide();
            return this;
        }

        public CommandPalette Toggle()
        {
            if (IsVisible) Hide();
            else Show();
            return this;
        }

        public override CommandPalette Show()
        {
            base.Show();
            ResetState();
            window.setTimeout(_ => _searchInput.focus(), 0);
            return this;
        }

        public override void Hide(Action onHidden = null)
        {
            base.Hide(onHidden);
            ResetState();
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

        private void HandleGlobalKeyDown(KeyboardEvent e)
        {
            if (!EnableGlobalShortcut && !EnableGlobalActionShortcuts)
            {
                return;
            }

            var target = (e.target is object ? e.target : e.srcElement).As<HTMLElement>();
            if (target != null && (target.isContentEditable || target.tagName == "INPUT" || target.tagName == "TEXTAREA" || target.tagName == "SELECT"))
            {
                return;
            }

            if (EnableGlobalShortcut && (e.key == "k" || e.key == "K") && (e.metaKey || e.ctrlKey))
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

            if (e.key == "Enter")
            {
                StopEvent(e);
                ActivateSelected();
                return;
            }

            if (e.key == "Escape")
            {
                StopEvent(e);
                Hide();
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
            if (_activeIndex < 0 || _activeIndex >= _visibleActions.Count)
            {
                return;
            }

            var action = _visibleActions[_activeIndex];
            ExecuteAction(action);
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
            if (_actionElements.Count == 0)
            {
                _activeIndex = -1;
                return;
            }

            var nextIndex = _activeIndex + delta;
            if (nextIndex < 0)
            {
                nextIndex = _actionElements.Count - 1;
            }
            else if (nextIndex >= _actionElements.Count)
            {
                nextIndex = 0;
            }

            SetActiveIndex(nextIndex);
        }

        private void SetActiveIndex(int index)
        {
            if (_actionElements.Count == 0)
            {
                _activeIndex = -1;
                return;
            }

            if (_activeIndex >= 0 && _activeIndex < _actionElements.Count)
            {
                _actionElements[_activeIndex].classList.remove("tss-active");
            }

            _activeIndex = index;
            if (_activeIndex >= 0 && _activeIndex < _actionElements.Count)
            {
                var activeEl = _actionElements[_activeIndex];
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
            var query = _searchInput.value?.Trim() ?? string.Empty;
            var actions = FilterActions(query).ToList();

            _results.RemoveChildElements();
            _visibleActions.Clear();
            _actionElements.Clear();

            string lastSection = null;
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (!string.IsNullOrWhiteSpace(action.Section) && action.Section != lastSection)
                {
                    _results.appendChild(Div(_("tss-commandpalette-section", text: action.Section)));
                    lastSection = action.Section;
                }

                var item = BuildActionElement(action, i);
                _results.appendChild(item);
                _visibleActions.Add(action);
                _actionElements.Add(item);
            }

            _emptyState.style.display = _visibleActions.Count == 0 ? "block" : "none";
            UpdateBreadcrumbs();
            SetActiveIndex(_visibleActions.Count > 0 ? 0 : -1);
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

            var haystack = string.Join(" ", new[] { action.Name, action.Subtitle, action.Keywords, action.Section }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();
            var terms = query.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return terms.All(term => haystack.Contains(term));
        }

        private HTMLElement BuildActionElement(CommandPaletteAction action, int index)
        {
            var iconContainer = Div(_("tss-commandpalette-icon"));
            if (action.Icon.HasValue)
            {
                iconContainer.appendChild(I(_($"tss-commandpalette-icon-item ec {action.Icon.Value}")));
            }
            else if (!string.IsNullOrWhiteSpace(action.IconClass))
            {
                iconContainer.appendChild(I(_($"tss-commandpalette-icon-item {action.IconClass}")));
            }
            else
            {
                iconContainer.appendChild(Span(_("tss-commandpalette-icon-placeholder", text: "•")));
            }

            var title = Div(_("tss-commandpalette-title", text: action.Name));
            var content = Div(_("tss-commandpalette-content"), title);

            if (!string.IsNullOrWhiteSpace(action.Subtitle))
            {
                content.appendChild(Div(_("tss-commandpalette-subtitle", text: action.Subtitle)));
            }

            var shortcuts = Div(_("tss-commandpalette-shortcuts"));
            if (action.Shortcut != null && action.Shortcut.Length > 0)
            {
                foreach (var shortcut in action.Shortcut)
                {
                    shortcuts.appendChild(Span(_("tss-commandpalette-shortcut", text: shortcut)));
                }
            }

            if (HasChildren(action))
            {
                shortcuts.appendChild(Span(_("tss-commandpalette-submenu", text: "›")));
            }

            var item = Div(_("tss-commandpalette-item", role: "option"), iconContainer, content, shortcuts);
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

        private CommandPaletteAction FindShortcutAction(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var normalized = key.ToLowerInvariant();
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

    [H5.Name("tss.CommandPaletteAction")]
    public sealed class CommandPaletteAction
    {
        public CommandPaletteAction(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public string Keywords { get; set; }
        public string Section { get; set; }
        public string ParentId { get; set; }
        public string IconClass { get; set; }
        public UIcons? Icon { get; set; }
        public string[] Shortcut { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public Action Perform { get; set; }
    }
}
