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
    /// rendering.
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

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Tree()
        {
            InnerElement = Ul(Att("tss-tree", role: "tree"));

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
                InnerElement.setAttribute("aria-expanded", "false");
                InnerElement.setAttribute("aria-selected", "false");

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
                set
                {
                    if (value != _isExpanded)
                    {
                        _isExpanded = value;
                        InnerElement.setAttribute("aria-expanded", _isExpanded ? "true" : "false");

                        if (_isExpanded)
                        {
                            InnerElement.classList.add("tss-expanded");
                            _chevronSpan.classList.remove(UIcons.AngleRight.ToCssClass());
                            _chevronSpan.classList.add(UIcons.AngleDown.ToCssClass());
                            ExpandedItem?.Invoke(this);
                        }
                        else
                        {
                            InnerElement.classList.remove("tss-expanded");
                            _chevronSpan.classList.remove(UIcons.AngleDown.ToCssClass());
                            _chevronSpan.classList.add(UIcons.AngleRight.ToCssClass());
                            CollapsedItem?.Invoke(this);
                        }
                    }
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

            internal void Flatten(List<Item> into, bool includeCollapsed)
            {
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
            }

            private bool IsOrHasChild(Item sender) => this == sender || _childItems.Any(l => l.IsOrHasChild(sender));

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
                if (HasChildren || _chevronSpan.classList.contains("tss-has-children"))
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

                if (HasChildren || _chevronSpan.classList.contains("tss-has-children"))
                {
                    IsExpanded = !IsExpanded;
                }
            }
        }
    }
}
