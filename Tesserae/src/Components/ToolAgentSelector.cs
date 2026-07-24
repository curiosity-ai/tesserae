using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A trigger button with a searchable popup for enabling agents and tools, shown grouped under "Agents" and
    /// "Tools" sections. Selecting items updates a count badge on the trigger. Designed to be dropped into one of
    /// <see cref="OmniBox.FooterItems"/>'s slots, and can also be driven inline (see <see cref="ShowInlineAt"/>) to
    /// back an "@mention" style picker inside a chat input.
    /// </summary>
    [Transpose.Name("tss.ToolAgentSelector")]
    public sealed class ToolAgentSelector : Layer<ToolAgentSelector>
    {
        private readonly HTMLElement _triggerLabel;
        private readonly HTMLElement _badge;

        private readonly SearchBox _searchBox;
        private readonly HTMLElement _agentsSection;
        private readonly HTMLElement _toolsSection;
        private readonly HTMLElement _agentsList;
        private readonly HTMLElement _toolsList;
        private readonly HTMLElement _emptyState;
        private readonly HTMLDivElement _popup;

        private readonly Action<Event> _onWindowClickAction;

        private readonly List<Item> _agents = new List<Item>();
        private readonly List<Item> _tools  = new List<Item>();
        private readonly List<Item> _visible = new List<Item>();

        private int  _highlightIndex = -1;
        private bool _isInline;

        /// <summary>
        /// Raised whenever an item's selected state changes.
        /// </summary>
        public event Action<ToolAgentSelector> SelectionChanged;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ToolAgentSelector(string label = "Tools", UIcons icon = UIcons.Tools)
        {
            _onWindowClickAction = (ev) => OnWindowClick(ev);

            _badge = Span(Att("tss-toolagent-badge"));
            _badge.style.display = "none";

            _triggerLabel = Span(Att("tss-toolagent-trigger-label", text: label));

            InnerElement = Div(Att("tss-toolagent-trigger", role: "button", ariaLabel: label),
                I(icon, cssClass: "tss-toolagent-trigger-icon"),
                _triggerLabel,
                I(UIcons.AngleDown, cssClass: "tss-toolagent-trigger-chevron"),
                _badge);
            InnerElement.tabIndex = 0;

            InnerElement.addEventListener("click", e =>
            {
                StopEvent(e);
                Toggle();
            });

            InnerElement.addEventListener("keydown", e =>
            {
                var ke = e.As<KeyboardEvent>();
                if (ke.key == "Enter" || ke.key == " ")
                {
                    StopEvent(e);
                    Toggle();
                }
            });

            _searchBox = new SearchBox("Search tools & agents").SearchAsYouType();
            _searchBox.OnSearch((s, text) => ApplyFilter(text));
            _searchBox.OnKeyDown((s, e) => HandlePopupKeyDown(e));

            _agentsList = Div(Att("tss-toolagent-list"));
            _toolsList  = Div(Att("tss-toolagent-list"));

            _agentsSection = Div(Att("tss-toolagent-section"), Div(Att("tss-toolagent-section-title", text: "Agents")), _agentsList);
            _toolsSection  = Div(Att("tss-toolagent-section"), Div(Att("tss-toolagent-section-title", text: "Tools")), _toolsList);

            _emptyState = Div(Att("tss-toolagent-empty", text: "No matches"));
            _emptyState.style.display = "none";

            var scrollArea = Div(Att("tss-toolagent-scroll"), _agentsSection, _toolsSection, _emptyState);

            _popup = Div(Att("tss-toolagent-popup"), _searchBox.Render(), scrollArea);

            UpdateSectionVisibility();
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            DomObserver.WhenMounted(InnerElement, () =>
            {
                DomObserver.WhenRemoved(InnerElement, () => Hide());
            });
            return InnerElement;
        }

        /// <summary>
        /// Sets the items shown under the "Agents" section (replaces any previously set).
        /// </summary>
        public ToolAgentSelector Agents(params Item[] items)
        {
            SetItems(_agents, _agentsList, items);
            return this;
        }

        /// <summary>
        /// Sets the items shown under the "Tools" section (replaces any previously set).
        /// </summary>
        public ToolAgentSelector Tools(params Item[] items)
        {
            SetItems(_tools, _toolsList, items);
            return this;
        }

        /// <summary>
        /// Hides item descriptions, rendering a denser list of icon + title rows only.
        /// </summary>
        public ToolAgentSelector Compact(bool value = true)
        {
            _popup.UpdateClassIf(value, "tss-toolagent-compact");
            return this;
        }

        /// <summary>
        /// Registers a callback invoked whenever the selection changes.
        /// </summary>
        public ToolAgentSelector OnChange(Action<ToolAgentSelector> handler)
        {
            SelectionChanged += handler;
            return this;
        }

        /// <summary>
        /// Gets every currently selected item, agents first, then tools.
        /// </summary>
        public Item[] SelectedItems => _agents.Concat(_tools).Where(i => i.IsSelected).ToArray();

        /// <summary>
        /// Gets the number of currently selected items.
        /// </summary>
        public int SelectedCount => _agents.Count(i => i.IsSelected) + _tools.Count(i => i.IsSelected);

        private void SetItems(List<Item> target, HTMLElement container, Item[] items)
        {
            foreach (var existing in target)
            {
                existing.SelectionChanged -= OnItemSelectionChanged;
            }

            target.Clear();
            ClearChildren(container);

            foreach (var item in items ?? Array.Empty<Item>())
            {
                target.Add(item);
                container.appendChild(item.Render());
                item.SelectionChanged += OnItemSelectionChanged;
            }

            UpdateSectionVisibility();
            UpdateBadge();
        }

        private void OnItemSelectionChanged(Item item)
        {
            UpdateBadge();
            SelectionChanged?.Invoke(this);
        }

        private void UpdateBadge()
        {
            var count = SelectedCount;
            if (count > 0)
            {
                _badge.innerText = count > 99 ? "99+" : count.ToString();
                _badge.style.display = "flex";
            }
            else
            {
                _badge.style.display = "none";
            }
        }

        /// <summary>
        /// Shows the popup anchored below the trigger button.
        /// </summary>
        public override ToolAgentSelector Show()
        {
            EnsurePopupLayer();

            _isInline = false;
            _searchBox.Render().style.display = "";
            InnerElement.classList.add("tss-toolagent-trigger-active");

            base.Show();
            PositionPopupNearTrigger();
            ResetFilter();

            // The short delay is required: the click that opens the popup also gives the (focusable, for
            // keyboard activation) trigger element the browser's default focus, which is only committed a few
            // milliseconds after the click handler returns - focusing the search box any earlier (even on the
            // very next macrotask) loses that race and the trigger keeps focus instead.
            DomObserver.WhenMounted(_popup, () => window.setTimeout(_ => _searchBox.Focus(), 100));

            return this;
        }

        /// <summary>
        /// Shows the popup anchored at an explicit viewport position (for example, next to a text caret), without
        /// moving focus or toggling the trigger button's pressed state. Used to back an "@mention" style picker.
        /// </summary>
        public ToolAgentSelector ShowInlineAt(double clientX, double clientY)
        {
            EnsurePopupLayer();

            _isInline = true;
            _searchBox.Render().style.display = "none";
            InnerElement.classList.remove("tss-toolagent-trigger-active");

            base.Show();
            PositionPopupAt(clientX, clientY);
            ApplyFilter(string.Empty);

            return this;
        }

        /// <summary>
        /// Hides the popup.
        /// </summary>
        public override void Hide(Action onHidden = null)
        {
            InnerElement.classList.remove("tss-toolagent-trigger-active");
            _isInline = false;
            base.Hide(onHidden);
        }

        private void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }

        private void EnsurePopupLayer()
        {
            if (_contentHtml != null) return;

            _contentHtml = Div(Att("tss-toolagent-layer"), _popup);
            _contentHtml.addEventListener("click",       _onWindowClickAction);
            _contentHtml.addEventListener("contextmenu", _onWindowClickAction);
        }

        private void OnWindowClick(Event e)
        {
            if (e.srcElement != _popup && !_popup.contains(e.srcElement))
            {
                Hide();
            }
        }

        private void PositionPopupNearTrigger()
        {
            _popup.style.visibility = "hidden";
            _popup.style.top  = "-1000px";
            _popup.style.left = "-1000px";

            var rect      = InnerElement.getBoundingClientRect().As<DOMRect>();
            var popupRect = _popup.getBoundingClientRect().As<DOMRect>();

            var top = rect.bottom + 4;
            if (window.innerHeight - rect.bottom - 4 < popupRect.height && rect.top > popupRect.height)
            {
                top = rect.top - popupRect.height - 4;
            }

            var left = rect.left;
            if (left + popupRect.width + 8 > window.innerWidth)
            {
                left = Math.Max(8, window.innerWidth - popupRect.width - 8);
            }

            _popup.style.top        = top  + "px";
            _popup.style.left       = left + "px";
            _popup.style.minWidth   = rect.width + "px";
            _popup.style.visibility = "visible";
        }

        private void PositionPopupAt(double clientX, double clientY)
        {
            _popup.style.visibility = "hidden";
            _popup.style.top  = "-1000px";
            _popup.style.left = "-1000px";
            _popup.style.minWidth = "";

            var popupRect = _popup.getBoundingClientRect().As<DOMRect>();

            var top = clientY;
            if (window.innerHeight - clientY < popupRect.height && clientY > popupRect.height)
            {
                top = clientY - popupRect.height - 20;
            }

            var left = clientX;
            if (left + popupRect.width + 8 > window.innerWidth)
            {
                left = Math.Max(8, window.innerWidth - popupRect.width - 8);
            }

            _popup.style.top        = top  + "px";
            _popup.style.left       = left + "px";
            _popup.style.visibility = "visible";
        }

        /// <summary>
        /// Filters the visible items by the given free-text query (matches title or description). Used both by the
        /// built-in search box and externally, to drive the popup from an inline "@" mention as the user types.
        /// </summary>
        public ToolAgentSelector Filter(string text)
        {
            if (!_isInline)
            {
                _searchBox.Text = text ?? string.Empty;
            }
            ApplyFilter(text ?? string.Empty);
            return this;
        }

        private void ResetFilter()
        {
            _searchBox.Text = string.Empty;
            ApplyFilter(string.Empty);
        }

        private void ApplyFilter(string text)
        {
            var terms = (text ?? string.Empty).Trim().ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in _agents.Concat(_tools))
            {
                item.SetVisible(terms.Length == 0 || terms.All(t => item.SearchText.Contains(t)));
            }

            UpdateSectionVisibility();
            RebuildVisibleList();
        }

        private void UpdateSectionVisibility()
        {
            var anyAgentsVisible = _agents.Any(i => i.IsVisible);
            var anyToolsVisible  = _tools.Any(i => i.IsVisible);

            _agentsSection.style.display = _agents.Count > 0 && anyAgentsVisible ? "" : "none";
            _toolsSection.style.display  = _tools.Count  > 0 && anyToolsVisible  ? "" : "none";
            _emptyState.style.display    = (_agents.Count + _tools.Count == 0) || (!anyAgentsVisible && !anyToolsVisible) ? "block" : "none";
        }

        private void RebuildVisibleList()
        {
            _visible.Clear();
            _visible.AddRange(_agents.Concat(_tools).Where(i => i.IsVisible));

            foreach (var item in _visible)
            {
                item.SetHighlighted(false);
            }

            _highlightIndex = -1;
        }

        private void HandlePopupKeyDown(KeyboardEvent e)
        {
            if (e.key == "ArrowDown")
            {
                StopEvent(e);
                MoveHighlight(1);
            }
            else if (e.key == "ArrowUp")
            {
                StopEvent(e);
                MoveHighlight(-1);
            }
            else if (e.key == "Enter" || e.key == "Tab")
            {
                StopEvent(e);
                ActivateHighlighted();
            }
            else if (e.key == "Escape")
            {
                StopEvent(e);
                Hide();
            }
        }

        /// <summary>
        /// Moves the keyboard-navigation highlight forward (positive) or backward (negative) among the currently
        /// visible items, wrapping around at either end.
        /// </summary>
        public void MoveHighlight(int direction)
        {
            if (_visible.Count == 0)
            {
                _highlightIndex = -1;
                return;
            }

            if (_highlightIndex < 0)
            {
                _highlightIndex = direction > 0 ? 0 : _visible.Count - 1;
            }
            else
            {
                _highlightIndex = (_highlightIndex + direction + _visible.Count) % _visible.Count;
            }

            for (var i = 0; i < _visible.Count; i++)
            {
                _visible[i].SetHighlighted(i == _highlightIndex);
            }

            _visible[_highlightIndex].ScrollIntoView();
        }

        /// <summary>
        /// Toggles the selection of the currently highlighted item (defaulting to the first visible item if none is
        /// highlighted yet). Returns <c>true</c> if an item was toggled.
        /// </summary>
        public bool ActivateHighlighted()
        {
            if (_visible.Count == 0) return false;

            if (_highlightIndex < 0 || _highlightIndex >= _visible.Count)
            {
                _highlightIndex = 0;
            }

            _visible[_highlightIndex].Toggle();
            return true;
        }

        /// <summary>
        /// A single selectable agent or tool row, with a checkbox, an optional leading icon, a title and an
        /// optional description.
        /// </summary>
        public sealed class Item : IComponent
        {
            private readonly HTMLElement _root;
            private readonly HTMLInputElement _input;
            private readonly HTMLElement _description;
            private readonly string _searchText;

            /// <summary>
            /// Gets the identifier of this item.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Gets or sets an arbitrary payload associated with this item.
            /// </summary>
            public object Tag { get; set; }

            internal event Action<Item> SelectionChanged;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Item(string id, string title, string description = null, UIcons? icon = null)
            {
                Id = id;

                _input = CheckBox(Att("tss-toolagent-item-input"));
                var checkmark = Span(Att("tss-toolagent-item-checkmark"));

                var iconContainer = Div(Att("tss-toolagent-item-icon"));
                if (icon.HasValue)
                {
                    iconContainer.appendChild(I(icon.Value));
                }
                else
                {
                    iconContainer.style.display = "none";
                }

                var titleEl = Div(Att("tss-toolagent-item-title", text: title ?? string.Empty));
                _description = Div(Att("tss-toolagent-item-description", text: description ?? string.Empty));
                if (string.IsNullOrEmpty(description))
                {
                    _description.style.display = "none";
                }

                var content = Div(Att("tss-toolagent-item-content"), titleEl, _description);

                _root = Label(Att("tss-toolagent-item"), _input, checkmark, iconContainer, content);

                _input.addEventListener("change", _ => SelectionChanged?.Invoke(this));

                _searchText = ((title ?? string.Empty) + " " + (description ?? string.Empty)).ToLower();
            }

            /// <summary>
            /// Gets or sets whether this item is selected.
            /// </summary>
            public bool IsSelected
            {
                get => _input.@checked;
                set
                {
                    if (_input.@checked == value) return;
                    _input.@checked = value;
                    SelectionChanged?.Invoke(this);
                }
            }

            /// <summary>
            /// Marks the item as selected.
            /// </summary>
            public Item Selected(bool value = true)
            {
                IsSelected = value;
                return this;
            }

            internal bool IsVisible => _root.style.display != "none";

            internal string SearchText => _searchText;

            internal void SetVisible(bool value) => _root.style.display = value ? "" : "none";

            internal void SetHighlighted(bool value) => _root.UpdateClassIf(value, "tss-toolagent-item-highlight");

            internal void ScrollIntoView()
            {
                try { _root.scrollIntoViewIfNeeded(); }
                catch { _root.scrollIntoView(); }
            }

            internal void Toggle() => IsSelected = !IsSelected;

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public HTMLElement Render() => _root;
        }
    }
}
