using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// How many items of a <see cref="Tree"/> can be selected at once, and with which gestures.
    /// </summary>
    public enum TreeSelectionMode
    {
        /// <summary>Items cannot be selected.</summary>
        None,
        /// <summary>One item at a time: selecting an item unselects whatever was selected before.</summary>
        Single,
        /// <summary>
        /// Any number of items, with the gestures of a search-results list: the checkbox toggles one item,
        /// ctrl (or cmd) clicking a row does the same, and shift-clicking a row selects everything between it
        /// and the last item the user picked.
        /// </summary>
        Multiple
    }

    /// <summary>
    /// A vertically-stacked tree view with expand / collapse, keyboard navigation, selection and arbitrary item
    /// rendering. The keyboard follows the WAI-ARIA tree pattern: the arrow keys, Home and End move between the
    /// rows on screen, Right and Left open, close and step in and out of folders, Enter does what a click on the
    /// row does, Space selects the focused row (Shift+Space ranges to it in a multiple selection), Shift with the
    /// arrows extends a multiple selection, Ctrl+A selects everything, * opens a row's siblings, and typing
    /// focuses the next row starting with what was typed.
    /// </summary>
    [Transpose.Name("tss.Tree")]
    public sealed class Tree : ComponentBase<Tree, HTMLUListElement>, IContainer<Tree.Item, Tree.Item>, IObservableComponent<Tree.Item>
    {
        private readonly List<Item>               _children      = new List<Item>();
        private readonly SettableObservable<Item> _observable    = new SettableObservable<Item>();
        private          TreeSelectionMode        _selectionMode = TreeSelectionMode.None;
        private          bool                     _cascade;
        private          Item                     _anchor;
        private          int                      _updateDepth;
        private          bool                     _updatePending;
        private          Func<Item, bool>         _filter;
        private          Item                     _focused;
        private          string                   _typeAhead     = "";
        private          double                   _typeAheadAt;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Tree()
        {
            InnerElement = Ul(Att("tss-tree", role: "tree"));

            // Keyboard focus moves between the rows (a roving tab stop): the row that last had focus is the one
            // Tab lands on while focus is inside the tree, and from outside the tree itself is the tab stop and
            // hands focus straight on to a row. The tree's own stop steps aside while a row holds focus, so
            // Shift+Tab from a row leaves the tree instead of landing on the tree and bouncing back in.
            InnerElement.tabIndex = 0;
            InnerElement.addEventListener("focus",    _ => FocusTabStop());
            InnerElement.addEventListener("focusin",  e => OnFocusIn(e));
            InnerElement.addEventListener("focusout", e => OnFocusOut(e));
            InnerElement.addEventListener("keydown",  e => OnKeyDown(e.As<KeyboardEvent>()));

            DomObserver.WhenMounted(InnerElement, () =>
            {
                var onKeyDown = new Action<Event>(e =>
                {
                    var kbEvent = e.As<KeyboardEvent>();

                    if (kbEvent.shiftKey)
                    {
                        InnerElement.classList.add("tss-tree-shift-pressed");
                    }
                });

                var onKeyUp = new Action<Event>(e =>
                {
                    var kbEvent = e.As<KeyboardEvent>();

                    if (!kbEvent.shiftKey)
                    {
                        InnerElement.classList.remove("tss-tree-shift-pressed");
                    }
                });

                document.body.addEventListener("keydown", onKeyDown);
                document.body.addEventListener("keyup",   onKeyUp);

                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    document.body.removeEventListener("keydown", onKeyDown);
                    document.body.removeEventListener("keyup",   onKeyUp);
                });
            });
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Gets the currently selected item - the last one the user picked when several are selected.
        /// </summary>
        public Item SelectedItem { get; private set; }

        /// <summary>
        /// Returns every selected item, in the order they appear in the tree.
        /// </summary>
        public Item[] SelectedItems => Flatten(includeCollapsed: true).Where(i => i.IsSelected).ToArray();

        /// <summary>
        /// Returns how many items of the tree can be selected at once.
        /// </summary>
        public TreeSelectionMode SelectionMode => _selectionMode;

        /// <summary>
        /// Returns a value indicating whether selecting an item also selects everything below it.
        /// </summary>
        public bool IsCascading => _cascade;

        /// <summary>
        /// Raised when selected item changed occurs.
        /// </summary>
        public event ComponentEventHandler<Tree, Item> SelectedItemChanged;

        /// <summary>
        /// Raised when any item is selected or unselected, with everything that is selected afterwards.
        /// </summary>
        public event ComponentEventHandler<Tree, Item[]> SelectionChanged;

        /// <summary>
        /// Registers a callback invoked when the selected event fires.
        /// </summary>
        public Tree OnSelected(ComponentEventHandler<Tree, Item> onSelected)
        {
            SelectedItemChanged += onSelected;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked whenever the selection changes, with every selected item. A gesture
        /// that moves several items at once - a range, or a cascade into a folder's contents - runs it once.
        /// </summary>
        public Tree OnSelectionChanged(ComponentEventHandler<Tree, Item[]> onSelectionChanged)
        {
            SelectionChanged += onSelectionChanged;
            return this;
        }

        /// <summary>
        /// Switches the tree to a compact density, matching the row height, font size and indentation of a code
        /// editor's file explorer.
        /// </summary>
        public Tree Compact(bool compact = true)
        {
            if (compact)
            {
                InnerElement.classList.add("tss-tree-compact");
            }
            else
            {
                InnerElement.classList.remove("tss-tree-compact");
            }

            return this;
        }

        /// <summary>
        /// Enables or disables single-item selection on the tree. Shorthand for
        /// <see cref="Selectable(TreeSelectionMode)"/> with <see cref="TreeSelectionMode.Single"/>.
        /// </summary>
        public Tree SelectionEnabled(bool enabled = true) => Selectable(enabled ? TreeSelectionMode.Single : TreeSelectionMode.None);

        /// <summary>
        /// Makes the items of the tree selectable, as many at a time as the given mode allows. In
        /// <see cref="TreeSelectionMode.Multiple"/> every item shows a checkbox, ctrl (or cmd) clicking a row
        /// toggles it, and shift-clicking one selects everything between it and the last item picked.
        /// </summary>
        public Tree Selectable(TreeSelectionMode mode = TreeSelectionMode.Multiple)
        {
            _selectionMode = mode;

            InnerElement.UpdateClassIf(mode != TreeSelectionMode.None, "tss-tree-selection-enabled");
            InnerElement.UpdateClassIf(mode == TreeSelectionMode.Multiple, "tss-tree-selection-multiple");

            if (mode == TreeSelectionMode.Multiple)
            {
                InnerElement.setAttribute("aria-multiselectable", "true");
            }
            else
            {
                InnerElement.removeAttribute("aria-multiselectable");
            }

            if (mode == TreeSelectionMode.None)
            {
                ClearSelection();
            }

            return this;
        }

        /// <summary>
        /// Takes selection away again, unselecting whatever was selected.
        /// </summary>
        public Tree NotSelectable() => Selectable(TreeSelectionMode.None);

        /// <summary>
        /// Makes an item's selection carry to everything below it: selecting a folder selects every item
        /// inside it, unselecting it unselects them, and a folder only some of whose contents are selected is
        /// drawn as partially selected.
        /// </summary>
        public Tree CascadeSelection(bool cascade = true)
        {
            _cascade = cascade;

            if (cascade) RefreshCascadeState();

            return this;
        }

        /// <summary>
        /// Unselects every item of the tree.
        /// </summary>
        public Tree ClearSelection()
        {
            BeginSelectionUpdate();

            foreach (var item in Flatten(includeCollapsed: true))
            {
                item.SetSelected(false, cascade: false);
            }

            _anchor = null;

            EndSelectionUpdate();

            return this;
        }

        /// <summary>
        /// Selects every selectable item of the tree.
        /// </summary>
        public Tree SelectAll()
        {
            if (_selectionMode != TreeSelectionMode.Multiple) return this;

            BeginSelectionUpdate();

            foreach (var item in Flatten(includeCollapsed: true))
            {
                item.SetSelected(true, cascade: false);
            }

            RefreshCascadeState();

            EndSelectionUpdate();

            return this;
        }

        /// <summary>
        /// Re-reads every folder's state from its contents, deepest first.
        /// </summary>
        private void RefreshCascadeState()
        {
            if (!_cascade) return;

            BeginSelectionUpdate();

            foreach (var child in _children)
            {
                child.RefreshCascadeSubtree();
            }

            EndSelectionUpdate();
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(Item component)
        {
            component.AttachTo(this, null);
            _children.Add(component);
            InnerElement.appendChild(component.Render());
            component.InternalSelectionChanged += OnItemSelectionChanged;

            if (_cascade)
            {
                BeginSelectionUpdate();
                component.RefreshCascadeSubtree();
                EndSelectionUpdate();
            }

            if (component.IsSelected)
            {
                if (_selectionMode != TreeSelectionMode.Multiple && SelectedItem != null) SelectedItem.IsSelected = false;
                SelectedItem      = component;
                _observable.Value = component;
            }

            if (component.SelectedChild != null)
            {
                if (_selectionMode != TreeSelectionMode.Multiple && SelectedItem != null) SelectedItem.IsSelected = false;
                SelectedItem = component.SelectedChild;
            }

            if (_filter is object) component.ApplyFilter(_filter);
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public void Clear()
        {
            _children.Clear();
            _anchor      = null;
            SelectedItem = null;
            ClearChildren(InnerElement);
        }

        /// <summary>
        /// Shows only the items <paramref name="predicate"/> accepts, plus whatever leads to them: an item that
        /// matches keeps its subtree as it was, an ancestor of a match stays visible and is opened so the match can be
        /// seen, and everything else is hidden. The filter is a view over the tree, not a change to it - items added
        /// while it is active are filtered as they arrive, and <see cref="ClearFilter"/> puts every item back,
        /// including the expansion state the user had before the filter opened folders on their behalf. Opening a
        /// folder for the filter raises no <c>OnExpanded</c>, so code persisting what the user expanded is not
        /// misled by it.
        /// </summary>
        public Tree Filter(Func<Item, bool> predicate)
        {
            if (predicate is null) return ClearFilter();

            _filter = predicate;

            foreach (var child in _children)
            {
                child.ApplyFilter(predicate);
            }

            return this;
        }

        /// <summary>
        /// Shows only the items whose text contains <paramref name="text"/>, ignoring case, and the folders leading
        /// to them. An empty text clears the filter.
        /// </summary>
        public Tree Filter(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return ClearFilter();

            var term = text.Trim().ToLowerInvariant();

            return Filter(item => (item.Text ?? "").ToLowerInvariant().Contains(term));
        }

        /// <summary>
        /// Removes the filter set by <see cref="Filter(Func{Item, bool})"/>: every item is shown again, and the
        /// folders the filter opened return to how the user had left them.
        /// </summary>
        public Tree ClearFilter()
        {
            if (_filter is null) return this;

            _filter = null;

            foreach (var child in _children)
            {
                child.ClearFilter();
            }

            return this;
        }

        /// <summary>
        /// Whether a filter set by <see cref="Filter(Func{Item, bool})"/> is in force.
        /// </summary>
        public bool IsFiltered => _filter is object;

        internal void ReapplyFilter(Item root)
        {
            if (_filter is object) root.ApplyFilter(_filter);
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(Item newComponent, Item oldComponent)
        {
            var index = _children.IndexOf(oldComponent);

            if (index >= 0)
            {
                newComponent.AttachTo(this, null);
                _children[index] = newComponent;
                InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
                newComponent.InternalSelectionChanged += OnItemSelectionChanged;

                if (newComponent.IsSelected)
                {
                    if (_selectionMode != TreeSelectionMode.Multiple && SelectedItem != null) SelectedItem.IsSelected = false;
                    SelectedItem = newComponent;
                }
            }
        }

        private void OnItemSelectionChanged(Item sender, bool isSelected)
        {
            if (isSelected)
            {
                if (_selectionMode != TreeSelectionMode.Multiple)
                {
                    foreach (var c in _children)
                    {
                        c.UnselectRecursively(sender);
                    }
                }

                SelectedItem      = sender;
                _observable.Value = sender;
                SelectedItemChanged?.Invoke(this, sender);
            }

            RaiseSelectionChanged();
        }

        /// <summary>
        /// Returns an observable that tracks the currently-selected tree item.
        /// </summary>
        public IObservable<Item> AsObservable() => _observable;

        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
        public Tree Items(params Item[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        // A gesture moves several items at once - a range, or a folder cascading into its contents - and each
        // of them reports its own change. The depth counter holds the tree-wide event back until the whole
        // gesture has run, so a handler that reads SelectedItems never sees a half-applied selection.
        internal void BeginSelectionUpdate() => _updateDepth++;

        internal void EndSelectionUpdate()
        {
            _updateDepth--;

            if (_updateDepth > 0) return;

            _updateDepth = 0;

            if (_updatePending)
            {
                _updatePending = false;
                SelectionChanged?.Invoke(this, SelectedItems);
            }
        }

        private void RaiseSelectionChanged()
        {
            if (_updateDepth > 0)
            {
                _updatePending = true;
                return;
            }

            SelectionChanged?.Invoke(this, SelectedItems);
        }

        /// <summary>
        /// Toggles the given item, and makes it the anchor the next shift-click ranges from.
        /// </summary>
        internal void ToggleSelection(Item item)
        {
            if (_selectionMode == TreeSelectionMode.None || !item.IsSelectable) return;

            BeginSelectionUpdate();

            item.SetSelected(!item.IsSelected, cascade: _cascade);
            _anchor = item;

            EndSelectionUpdate();
        }

        /// <summary>
        /// Selects everything between the anchor - the last item the user picked - and the given item,
        /// unselecting whatever falls outside it, the way a shift-click through a list of search results does.
        /// The range runs over the rows that are actually on screen, so a collapsed folder counts as one row
        /// (and, when the tree cascades, brings its contents with it).
        /// </summary>
        internal void SelectRangeTo(Item item)
        {
            if (_selectionMode != TreeSelectionMode.Multiple || !item.IsSelectable) return;

            var visible = Flatten(includeCollapsed: false);
            var to      = visible.IndexOf(item);

            if (to < 0) return;

            var from = _anchor is object ? visible.IndexOf(_anchor) : -1;

            if (from < 0)
            {
                //Nothing to range from yet: this click becomes the anchor, the way the first shift-click on a
                //list of search results does.
                _anchor = item;
                ToggleSelection(item);
                return;
            }

            if (from > to)
            {
                var swap = from;
                from     = to;
                to       = swap;
            }

            BeginSelectionUpdate();

            for (var i = 0; i < visible.Count; i++)
            {
                visible[i].SetSelected(i >= from && i <= to, cascade: _cascade);
            }

            EndSelectionUpdate();
        }

        /// <summary>
        /// Moves keyboard focus to the given item - the row the arrow keys then move from - opening the folders
        /// above it so it can be seen.
        /// </summary>
        public Tree FocusItem(Item item)
        {
            if (item is null || item.IsFilteredOut) return this;

            for (var parent = item.Parent; parent is object; parent = parent.Parent)
            {
                parent.IsExpanded = true;
            }

            item.Render().focus();

            return this;
        }

        /// <summary>The rows the keyboard moves through: those on screen, in the order they are drawn.</summary>
        private List<Item> NavigableItems() => Flatten(includeCollapsed: false);

        private Item ItemOf(EventTarget target)
        {
            if (target is null) return null;

            foreach (var item in Flatten(includeCollapsed: true))
            {
                if (item.Render() == target) return item;
            }

            return null;
        }

        // Focus arriving on the tree itself (Tab from outside, or a click on its empty space) goes on to the row
        // that had it last, else to the selected row, else to the first one.
        private void FocusTabStop()
        {
            var visible = NavigableItems();

            if (visible.Count == 0) return;

            var target = _focused is object && visible.Contains(_focused) ? _focused : null;

            if (target is null) target = visible.FirstOrDefault(i => i.IsSelected);
            if (target is null) target = visible[0];

            target.Render().focus();
        }

        private void OnFocusIn(Event e)
        {
            var item = ItemOf(e.target);

            if (item is null) return;

            if (_focused is object && _focused != item)
            {
                _focused.Render().tabIndex = -1;
                _focused.ShowFocus(false);
            }

            _focused              = item;
            item.Render().tabIndex = 0;
            InnerElement.tabIndex  = -1;

            // A row reached from the keyboard shows its commands, so Tab can go on to them. It is a class rather
            // than :focus-visible because the commands must stay on screen while focus is between the row and its
            // button: the browser checks the button can take focus after the row has already let go of it.
            item.ShowFocus(item.Render().matches(":focus-visible"));
        }

        private void OnFocusOut(Event e)
        {
            var next = e.As<FocusEvent>().relatedTarget;

            if (_focused is object && (next is null || !_focused.OwnsRowElement(next.As<HTMLElement>()))) _focused.ShowFocus(false);

            if (next is object && InnerElement.contains(next.As<HTMLElement>())) return;

            // Leaving the tree: it becomes the tab stop again, so the way back in never depends on a row that may
            // have been collapsed, filtered out or removed in the meantime.
            if (_focused is object) _focused.Render().tabIndex = -1;

            InnerElement.tabIndex = 0;
        }

        private void OnKeyDown(KeyboardEvent e)
        {
            // Only keys pressed on a row itself: a command button or anything else inside the row keeps its own keys.
            var current = ItemOf(e.target);

            if (current is null || e.altKey) return;

            var visible = NavigableItems();
            var index   = visible.IndexOf(current);

            if (index < 0) return;

            var ctrl     = e.ctrlKey || e.metaKey;
            var multiple = _selectionMode == TreeSelectionMode.Multiple;

            switch (e.key)
            {
                case "ArrowDown":
                    MoveFocus(current, visible, index + 1, extend: multiple && e.shiftKey);
                    break;

                case "ArrowUp":
                    MoveFocus(current, visible, index - 1, extend: multiple && e.shiftKey);
                    break;

                case "Home":
                    MoveFocus(current, visible, 0, extend: multiple && e.shiftKey && ctrl);
                    break;

                case "End":
                    MoveFocus(current, visible, visible.Count - 1, extend: multiple && e.shiftKey && ctrl);
                    break;

                case "ArrowRight":
                    if (!current.IsExpandable) break;

                    if (!current.IsExpanded)
                    {
                        current.IsExpanded = true;
                    }
                    else if (index + 1 < visible.Count && visible[index + 1].Parent == current)
                    {
                        visible[index + 1].Render().focus();
                    }

                    break;

                case "ArrowLeft":
                    if (current.IsExpandable && current.IsExpanded)
                    {
                        current.IsExpanded = false;
                    }
                    else if (current.Parent is object)
                    {
                        current.Parent.Render().focus();
                    }

                    break;

                case "Enter":
                    current.Activate();
                    break;

                case " ":
                    if (_selectionMode == TreeSelectionMode.None || !current.IsSelectable) return;

                    if (multiple && e.shiftKey)
                    {
                        SelectRangeTo(current);
                    }
                    else
                    {
                        ToggleSelection(current);
                    }

                    break;

                case "*":
                    foreach (var sibling in current.Parent is object ? current.Parent.Children : _children.ToArray())
                    {
                        if (sibling.IsExpandable && !sibling.IsFilteredOut) sibling.IsExpanded = true;
                    }

                    break;

                default:
                    if (ctrl && (e.key == "a" || e.key == "A"))
                    {
                        if (!multiple) return;

                        SelectAll();
                        break;
                    }

                    if (ctrl || e.key is null || e.key.Length != 1 || !TypeAhead(current, visible, e.key)) return;

                    break;
            }

            e.preventDefault();
            e.stopPropagation();
        }

        // Moves focus to the row at the given index, clamped to the tree. Extending - Shift held in a multiple
        // selection - selects everything from the anchor to that row, the way a shift-click does; with nothing
        // picked yet, the range starts at the row focus is leaving.
        private void MoveFocus(Item current, List<Item> visible, int index, bool extend)
        {
            if (index < 0) index = 0;
            if (index >= visible.Count) index = visible.Count - 1;

            var target = visible[index];

            if (extend)
            {
                if (_anchor is null || !visible.Contains(_anchor)) _anchor = current;

                SelectRangeTo(target);
            }

            target.Render().focus();
        }

        // Typing a row's first letters focuses the next row whose text starts with them; letters typed in quick
        // succession build up one search, a pause starts a new one.
        private bool TypeAhead(Item current, List<Item> visible, string key)
        {
            var now = Transpose.Core.es5.Date.now();

            _typeAhead   = now - _typeAheadAt > 500 ? key : _typeAhead + key;
            _typeAheadAt = now;

            if (string.IsNullOrWhiteSpace(_typeAhead)) return false;

            var term  = _typeAhead.ToLowerInvariant();
            var start = visible.IndexOf(current) + (_typeAhead.Length == 1 ? 1 : 0);

            for (var i = 0; i < visible.Count; i++)
            {
                var candidate = visible[(start + i) % visible.Count];

                if ((candidate.Text ?? "").ToLowerInvariant().StartsWith(term))
                {
                    candidate.Render().focus();
                    return true;
                }
            }

            return true;
        }

        private List<Item> Flatten(bool includeCollapsed)
        {
            var items = new List<Item>();

            foreach (var child in _children)
            {
                child.Flatten(items, includeCollapsed);
            }

            return items;
        }

        [Transpose.Name("tss.Tree.Item")]
        public class Item : ComponentBase<Item, HTMLLIElement>, IContainer<Item, Item>
        {
            internal event Action<Item, bool>          InternalSelectionChanged;
            private event  ComponentEventHandler<Item> SelectedItem;
            private event  ComponentEventHandler<Item> ExpandedItem;
            private event  ComponentEventHandler<Item> CollapsedItem;
            private event  Action<Item, bool>          SelectionChangedItem;

            private readonly HTMLDivElement   _headerDiv;
            private readonly HTMLElement      _chevronSpan;
            private          HTMLElement      _iconSpan;
            private          UIcons?          _icon;
            private readonly HTMLElement      _checkboxSpan;
            private readonly HTMLSpanElement  _textSpan;
            private readonly HTMLDivElement   _commandsDiv;
            private readonly HTMLUListElement _childContainer;
            private readonly TreeCommand[]    _commands;

            private readonly List<Item> _childItems = new List<Item>();
            private          bool       _isExpanded;
            private          bool       _isSelected;
            private          bool       _isPartiallySelected;
            private          bool       _isSelectable = true;
            private          bool       _filteredOut;
            private          bool?      _expandedBeforeFilter;
            private          string     _iconColor = "";
            private          Tree       _tree;

            internal Item SelectedChild { get; private set; }

            /// <summary>Gets the item this one hangs from, or null when it is a root of the tree.</summary>
            public Item Parent { get; private set; }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Item(string text = null, UIcons? icon = null, params TreeCommand[] commands)
            {
                _chevronSpan    = I(Att("tss-tree-chevron " + UIcons.AngleRight.ToCssClass()));
                _textSpan = Span(Att("tss-tree-text",   text: text));
                _childContainer = Ul(Att("tss-tree-children", role: "group"));
                _checkboxSpan   = I(Att("tss-tree-checkbox " + UIcons.Square.ToCssClass()));
                _commands       = commands ?? new TreeCommand[0];

                _headerDiv = Div(Att("tss-tree-item-content"), _chevronSpan, _checkboxSpan);

                if (icon.HasValue)
                {
                    _icon     = icon.Value;
                    _iconSpan = I(Att("tss-tree-icon " + icon.Value.ToCssClass()));
                    _headerDiv.appendChild(_iconSpan);
                }

                _headerDiv.appendChild(_textSpan);

                _commandsDiv = Div(Att("tss-tree-commands"));

                if (_commands.Length > 0)
                {
                    foreach (var c in _commands)
                    {
                        _commandsDiv.appendChild(c.Render());
                    }
                }

                _headerDiv.appendChild(_commandsDiv);

                InnerElement = Li(Att("tss-tree-item", role: "treeitem"), _headerDiv, _childContainer);
                InnerElement.setAttribute("aria-selected", "false");
                InnerElement.tabIndex = -1;

                _headerDiv.onclick    = ClickHandler;
                _chevronSpan.onclick  = ChevronClickHandler;
                _checkboxSpan.onclick = CheckboxClickHandler;

                AttachContextMenu();

                var hookContextMenu = _commands.FirstOrDefault(c => c.ShouldHookToContextMenu);

                if (hookContextMenu is object)
                {
                    OnContextMenu((_, e) => hookContextMenu.RaiseOnClick(e));
                }

                UpdateChevronVisibility();
            }

            /// <summary>
            /// Configures the item's commands to always be visible (rather than only on hover).
            /// </summary>
            public Item CommandsAlwaysVisible(bool alwaysVisible = true)
            {
                if (alwaysVisible)
                {
                    _headerDiv.classList.add("tss-tree-commands-always-visible");
                }
                else
                {
                    _headerDiv.classList.remove("tss-tree-commands-always-visible");
                }

                return this;
            }

            /// <summary>
            /// Colours the item's icon - a danger tint on a file that fails to compile, say - leaving the text
            /// alone. Pass null or an empty string to go back to the theme's colour. An item created without
            /// an icon has nothing to colour until <see cref="Icon"/> gives it one; the colour is kept for then.
            /// </summary>
            public Item IconColor(string color)
            {
                _iconColor = color ?? "";

                if (_iconSpan is object) _iconSpan.style.color = _iconColor;

                return this;
            }

            /// <summary>
            /// Gets or sets the text shown in the component.
            /// </summary>
            public string Text
            {
                get => _textSpan.innerText;
                set => _textSpan.innerText = value;
            }

            /// <summary>
            /// Gets or sets the icon shown by the component.
            /// </summary>
            public UIcons? Icon
            {
                get => _icon;
                set
                {
                    _icon = value;

                    if (!value.HasValue)
                    {
                        if (_iconSpan != null)
                        {
                            _headerDiv.removeChild(_iconSpan);
                            _iconSpan = null;
                        }
                    }
                    else
                    {
                        if (_iconSpan == null)
                        {
                            _iconSpan = I(Att("tss-tree-icon " + value.Value.ToCssClass()));
                            _iconSpan.style.color = _iconColor;
                            _headerDiv.insertBefore(_iconSpan, _textSpan);
                        }
                        else
                        {
                            _iconSpan.className = "tss-tree-icon " + value.Value.ToCssClass();
                        }
                    }
                }
            }

            /// <summary>
            /// Returns a value indicating whether the component is expanded.
            /// </summary>
            public bool IsExpanded
            {
                get => _isExpanded;
                set => SetExpanded(value, raiseEvents: true);
            }

            private void SetExpanded(bool value, bool raiseEvents)
            {
                if (value == _isExpanded) return;

                _isExpanded = value;
                UpdateAriaExpanded();

                if (_isExpanded)
                {
                    InnerElement.classList.add("tss-expanded");
                    _chevronSpan.classList.remove(UIcons.AngleRight.ToCssClass());
                    _chevronSpan.classList.add(UIcons.AngleDown.ToCssClass());

                    if (raiseEvents) ExpandedItem?.Invoke(this);
                }
                else
                {
                    InnerElement.classList.remove("tss-expanded");
                    _chevronSpan.classList.remove(UIcons.AngleDown.ToCssClass());
                    _chevronSpan.classList.add(UIcons.AngleRight.ToCssClass());

                    if (raiseEvents) CollapsedItem?.Invoke(this);
                }
            }

            /// <summary>
            /// Whether the item is hidden by the tree's <see cref="Tree.Filter(Func{Item, bool})"/>: neither it nor
            /// anything below it matched.
            /// </summary>
            public bool IsFilteredOut => _filteredOut;

            // Hides what neither matches nor leads to a match, and opens the folders on the way to one. Returns whether
            // the item stays visible, which is what its parent needs to decide about itself. The expansion the user had
            // is kept from the first pass of a filter, so clearing it can put the folders back - a later pass does not
            // overwrite it, or a search that narrows over several keystrokes would remember its own openings.
            internal bool ApplyFilter(Func<Item, bool> predicate)
            {
                var matches      = predicate(this);
                var childVisible = false;

                foreach (var child in _childItems)
                {
                    if (child.ApplyFilter(predicate)) childVisible = true;
                }

                var visible = matches || childVisible;

                SetFilteredOut(!visible);

                if (childVisible && !matches)
                {
                    if (!_expandedBeforeFilter.HasValue) _expandedBeforeFilter = _isExpanded;

                    SetExpanded(true, raiseEvents: false);
                }

                return visible;
            }

            internal void ClearFilter()
            {
                SetFilteredOut(false);

                if (_expandedBeforeFilter.HasValue)
                {
                    SetExpanded(_expandedBeforeFilter.Value, raiseEvents: false);
                    _expandedBeforeFilter = null;
                }

                foreach (var child in _childItems)
                {
                    child.ClearFilter();
                }
            }

            private void SetFilteredOut(bool filteredOut)
            {
                if (filteredOut == _filteredOut) return;

                _filteredOut = filteredOut;

                if (filteredOut)
                {
                    InnerElement.classList.add("tss-tree-filtered-out");
                }
                else
                {
                    InnerElement.classList.remove("tss-tree-filtered-out");
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the component is selected. On a tree that cascades
            /// (<see cref="Tree.CascadeSelection(bool)"/>) setting it carries to everything below the item.
            /// </summary>
            public bool IsSelected
            {
                get => _isSelected;
                set => SetSelected(value, cascade: _tree is object && _tree.IsCascading);
            }

            /// <summary>
            /// Returns a value indicating whether only some of the items below this one are selected. Only a
            /// tree that cascades (<see cref="Tree.CascadeSelection(bool)"/>) ever reports this.
            /// </summary>
            public bool IsPartiallySelected => _isPartiallySelected;

            /// <summary>
            /// Returns a value indicating whether the item can be selected at all. An item that cannot is
            /// skipped by a range, by a cascade, and by <see cref="Tree.SelectAll"/>, and shows no checkbox.
            /// </summary>
            public bool IsSelectable => _isSelectable;

            /// <summary>
            /// Says whether the item can be selected. Use it for a row that is there to be read rather than
            /// picked - a file with nothing to import, a folder no action applies to.
            /// </summary>
            public Item Selectable(bool selectable = true)
            {
                _isSelectable = selectable;

                InnerElement.UpdateClassIfNot(selectable, "tss-tree-item-not-selectable");

                if (!selectable && _isSelected)
                {
                    SetSelected(false, cascade: false);
                }

                return this;
            }

            /// <summary>
            /// Returns a value indicating whether the component has the given children.
            /// </summary>
            public bool HasChildren => _childItems.Count > 0 || _childContainer.hasChildNodes();

            /// <summary>Returns the items hanging from this one.</summary>
            public Item[] Children => _childItems.ToArray();

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public override HTMLElement Render() => InnerElement;

            /// <summary>
            /// Adds the given item to the component.
            /// </summary>
            public void Add(Item component)
            {
                _childItems.Add(component);
                _childContainer.appendChild(component.Render());
                UpdateChevronVisibility();
                component.AttachTo(_tree, this);
                component.InternalSelectionChanged += OnChildSelectionChanged;

                if (component.IsSelected)
                {
                    InternalSelectionChanged?.Invoke(component, true);

                    if (SelectedChild != null && !(_tree is object && _tree.SelectionMode == TreeSelectionMode.Multiple)) SelectedChild.IsSelected = false;
                    SelectedChild = component;
                }

                if (component.SelectedChild != null)
                {
                    InternalSelectionChanged?.Invoke(component.SelectedChild, true);

                    if (SelectedChild != null && !(_tree is object && _tree.SelectionMode == TreeSelectionMode.Multiple)) SelectedChild.IsSelected = false;
                    SelectedChild = component.SelectedChild;
                }

                RefreshSelectionFromChildren();

                // A child arriving under a filtered tree can turn a hidden folder into a visible one, so the
                // whole branch is judged again from its root.
                if (_tree is object && _tree.IsFiltered) _tree.ReapplyFilter(Root());
            }

            private Item Root()
            {
                var item = this;

                while (item.Parent is object)
                {
                    item = item.Parent;
                }

                return item;
            }

            private void OnChildSelectionChanged(Item sender, bool isSelected)
            {
                InternalSelectionChanged?.Invoke(sender, isSelected);
            }

            /// <summary>
            /// Clears the component's current state.
            /// </summary>
            public void Clear()
            {
                _childItems.Clear();
                SelectedChild = null;
                ClearChildren(_childContainer);
                UpdateChevronVisibility();
            }

            /// <summary>
            /// Replaces an existing item with a new one.
            /// </summary>
            public void Replace(Item newComponent, Item oldComponent)
            {
                var index = _childItems.IndexOf(oldComponent);

                if (index >= 0)
                {
                    _childItems[index] = newComponent;
                    _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
                    newComponent.AttachTo(_tree, this);
                    newComponent.InternalSelectionChanged += OnChildSelectionChanged;

                    if (newComponent.IsSelected)
                    {
                        InternalSelectionChanged?.Invoke(newComponent, true);
                    }
                }
            }

            internal void AttachTo(Tree tree, Item parent)
            {
                _tree  = tree;
                Parent = parent;

                foreach (var child in _childItems)
                {
                    child.AttachTo(tree, this);
                }
            }

            /// <summary>
            /// Rows that are not on screen - inside a collapsed folder, or hidden by a filter - are left out unless
            /// <paramref name="includeCollapsed"/> asks for every item.
            /// </summary>
            internal void Flatten(List<Item> into, bool includeCollapsed)
            {
                if (!includeCollapsed && _filteredOut) return;

                into.Add(this);

                if (!includeCollapsed && !_isExpanded) return;

                foreach (var child in _childItems)
                {
                    child.Flatten(into, includeCollapsed);
                }
            }

            /// <summary>
            /// Sets the item's selection, optionally carrying it to everything below, and updates the
            /// ancestors' partial state on the way back up.
            /// </summary>
            internal void SetSelected(bool value, bool cascade)
            {
                if (value && !_isSelectable) return;

                _tree?.BeginSelectionUpdate();

                ApplySelected(value);

                if (cascade)
                {
                    foreach (var child in _childItems)
                    {
                        child.SetSelected(value, cascade: true);
                    }

                    Parent?.RefreshSelectionFromChildren();
                }

                _tree?.EndSelectionUpdate();
            }

            private void ApplySelected(bool value)
            {
                if (value == _isSelected && !_isPartiallySelected) return;

                var wasSelected = _isSelected;

                _isSelected          = value;
                _isPartiallySelected = false;

                InnerElement.setAttribute("aria-selected", _isSelected ? "true" : "false");
                _headerDiv.UpdateClassIf(_isSelected, "tss-selected");
                _headerDiv.classList.remove("tss-partially-selected");
                UpdateCheckbox();

                if (_isSelected == wasSelected) return;

                if (_isSelected) SelectedItem?.Invoke(this);

                SelectionChangedItem?.Invoke(this, _isSelected);
                InternalSelectionChanged?.Invoke(this, _isSelected);
            }

            /// <summary>
            /// Re-reads this item's state from its children - all selected, none, or some - and carries the
            /// answer further up.
            /// </summary>
            internal void RefreshSelectionFromChildren()
            {
                if (_tree is null || !_tree.IsCascading) return;

                RecomputeFromChildren();

                Parent?.RefreshSelectionFromChildren();
            }

            /// <summary>
            /// Re-reads the whole subtree's state, deepest item first, so a tree built bottom-up shows the
            /// right folder states the moment it is added.
            /// </summary>
            internal void RefreshCascadeSubtree()
            {
                foreach (var child in _childItems)
                {
                    child.RefreshCascadeSubtree();
                }

                RecomputeFromChildren();
            }

            /// <summary>
            /// A folder with nothing selectable inside it keeps whatever state it had.
            /// </summary>
            private void RecomputeFromChildren()
            {
                if (!_isSelectable) return;

                var selectable = _childItems.Where(c => c.IsSelectable).ToArray();

                if (selectable.Length == 0) return;

                var selected  = selectable.Count(c => c.IsSelected);
                var partial   = selectable.Any(c => c.IsPartiallySelected);

                if (selected == selectable.Length && !partial)
                {
                    ApplySelected(true);
                }
                else if (selected > 0 || partial)
                {
                    ApplyPartiallySelected();
                }
                else
                {
                    ApplySelected(false);
                }
            }

            private void ApplyPartiallySelected()
            {
                var wasSelected = _isSelected;

                _isSelected          = false;
                _isPartiallySelected = true;

                InnerElement.setAttribute("aria-selected", "false");
                _headerDiv.classList.remove("tss-selected");
                _headerDiv.classList.add("tss-partially-selected");
                UpdateCheckbox();

                if (wasSelected)
                {
                    SelectionChangedItem?.Invoke(this, false);
                    InternalSelectionChanged?.Invoke(this, false);
                }
            }

            private void UpdateCheckbox()
            {
                var icon = _isSelected
                    ? UIcons.Checkbox
                    : (_isPartiallySelected ? UIcons.SquareMinus : UIcons.Square);

                _checkboxSpan.className = "tss-tree-checkbox " + icon.ToCssClass();
            }

            internal void UnselectRecursively(Item sender)
            {
                if (this == sender)
                {
                    foreach (var child in _childItems)
                    {
                        child.UnselectRecursively(sender);
                    }
                }
                else if (!_childItems.Any(l => l.IsOrHasChild(sender)))
                {
                    SetSelected(false, cascade: false);

                    foreach (var child in _childItems)
                    {
                        child.UnselectRecursively(sender);
                    }
                }
                else
                {
                    // A folder on the way to the new selection keeps its own state, but whatever else is selected
                    // inside it - a sibling of the new item, say - is let go like everything else.
                    foreach (var child in _childItems)
                    {
                        child.UnselectRecursively(sender);
                    }
                }
            }

            private bool IsOrHasChild(Item sender) => this == sender || _childItems.Any(l => l.IsOrHasChild(sender));

            /// <summary>
            /// Whether the item can be opened: it has children, or loads them when it is first expanded.
            /// </summary>
            internal bool IsExpandable => HasChildren || _chevronSpan.classList.contains("tss-has-children");

            /// <summary>
            /// Runs what a plain click on the row runs - opening or closing it, and its <c>OnClick</c>. This is
            /// what Enter does to the focused row.
            /// </summary>
            internal void Activate() => _headerDiv.click();

            internal void ShowFocus(bool show) => _headerDiv.UpdateClassIf(show, "tss-tree-keyboard-focus");

            /// <summary>Whether the element is this row - the item itself or anything on its own line, such as a command.</summary>
            internal bool OwnsRowElement(HTMLElement element) => element == InnerElement || _headerDiv.contains(element);

            private void UpdateChevronVisibility()
            {
                if (HasChildren)
                {
                    _chevronSpan.classList.add("tss-has-children");
                }
                else
                {
                    _chevronSpan.classList.remove("tss-has-children");
                }

                UpdateAriaExpanded();
            }

            // Only a row that can be opened says whether it is: a leaf carrying aria-expanded="false" is read out
            // as a collapsed folder.
            private void UpdateAriaExpanded()
            {
                if (IsExpandable)
                {
                    InnerElement.setAttribute("aria-expanded", _isExpanded ? "true" : "false");
                }
                else
                {
                    InnerElement.removeAttribute("aria-expanded");
                }
            }

            /// <summary>
            /// Adds the given items to the component.
            /// </summary>
            public Item Items(params Item[] children)
            {
                children.ForEach(x => Add(x));
                return this;
            }

            /// <summary>
            /// Asynchronously loads the child items of this tree node from the supplied factory.
            /// </summary>
            public Item ItemsAsync(Func<Task<Item[]>> childrenAsync)
            {
                bool alreadyRun = false;
                _chevronSpan.classList.add("tss-has-children"); // Show chevron indicating there *might* be children or it's expandable
                UpdateAriaExpanded();

                ExpandedItem += s =>
                {
                    if (!alreadyRun)
                    {
                        alreadyRun = true;

                        var loading = new Item("Loading...", UIcons.Spinner);
                        Add(loading);

                        Task.Run(async () =>
                        {
                            var children = await childrenAsync();
                            Clear();
                            children.ForEach(x => Add(x));
                        }).FireAndForget();
                    }
                };

                return this;
            }

            /// <summary>
            /// Expands the component.
            /// </summary>
            public Item Expanded(bool isExpanded = true)
            {
                IsExpanded = isExpanded;
                return this;
            }

            /// <summary>
            /// Marks the component as selected.
            /// </summary>
            public Item Selected(bool isSelected = true)
            {
                IsSelected = isSelected;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the selected event fires.
            /// </summary>
            public Item OnSelected(ComponentEventHandler<Item> onSelected)
            {
                SelectedItem += onSelected;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked whenever the item is selected or unselected, with the new state.
            /// </summary>
            public Item OnSelectionChanged(Action<Item, bool> onSelectionChanged)
            {
                SelectionChangedItem += onSelectionChanged;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the expanded event fires.
            /// </summary>
            public Item OnExpanded(ComponentEventHandler<Item> onExpanded)
            {
                ExpandedItem += onExpanded;
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the collapsed event fires.
            /// </summary>
            public Item OnCollapsed(ComponentEventHandler<Item> onCollapsed)
            {
                CollapsedItem += onCollapsed;
                return this;
            }

            private void ClickHandler(MouseEvent e)
            {
                StopEvent(e);

                var mode = _tree is object ? _tree.SelectionMode : TreeSelectionMode.None;

                if (mode == TreeSelectionMode.Multiple && _isSelectable)
                {
                    //The gestures of a search-results list: ctrl (or cmd) picks one more, shift picks
                    //everything up to here, and a plain click is left to the row's own handler.
                    if (e.ctrlKey || e.metaKey)
                    {
                        _tree.ToggleSelection(this);
                        return;
                    }

                    if (e.shiftKey)
                    {
                        _tree.SelectRangeTo(this);
                        return;
                    }
                }
                else if (mode == TreeSelectionMode.Single && e.shiftKey)
                {
                    IsSelected = !IsSelected;
                }

                // Clicking the item content toggles expansion if it has children
                if (IsExpandable)
                {
                    IsExpanded = !IsExpanded;
                }

                RaiseOnClick(e);
            }

            private void CheckboxClickHandler(MouseEvent e)
            {
                StopEvent(e);

                var mode = _tree is object ? _tree.SelectionMode : TreeSelectionMode.None;

                if (mode == TreeSelectionMode.None || !_isSelectable) return;

                if (mode == TreeSelectionMode.Multiple && e.shiftKey)
                {
                    _tree.SelectRangeTo(this);
                    return;
                }

                if (_tree is object)
                {
                    _tree.ToggleSelection(this);
                }
                else
                {
                    IsSelected = !IsSelected;
                }
            }

            private void ChevronClickHandler(MouseEvent e)
            {
                StopEvent(e);

                if (IsExpandable)
                {
                    IsExpanded = !IsExpanded;
                }
            }
        }
    }
}
