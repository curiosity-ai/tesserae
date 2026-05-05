using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Tree")]
    public sealed class Tree : ComponentBase<Tree, HTMLUListElement>, IContainer<Tree.Item, Tree.Item>
    {
        private readonly List<Item> _children = new List<Item>();
        private bool _selectionEnabled = false;

        public Tree()
        {
            InnerElement = Ul(_("tss-tree", role: "tree"));

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
                document.body.addEventListener("keyup", onKeyUp);

                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    document.body.removeEventListener("keydown", onKeyDown);
                    document.body.removeEventListener("keyup", onKeyUp);
                });
            });
        }

        public override HTMLElement Render() => InnerElement;

        public Item SelectedItem { get; private set; }

        public event ComponentEventHandler<Tree, Item> SelectedItemChanged;

        public Tree OnSelected(ComponentEventHandler<Tree, Item> onSelected)
        {
            SelectedItemChanged += onSelected;
            return this;
        }

        public Tree SelectionEnabled(bool enabled = true)
        {
            _selectionEnabled = enabled;
            if (enabled)
            {
                InnerElement.classList.add("tss-tree-selection-enabled");
            }
            else
            {
                InnerElement.classList.remove("tss-tree-selection-enabled");
            }

            foreach (var item in _children)
            {
                item.SelectionEnabled = enabled;
            }

            return this;
        }

        public void Add(Item component)
        {
            component.SelectionEnabled = _selectionEnabled;
            _children.Add(component);
            InnerElement.appendChild(component.Render());
            component.InternalSelectedItem += OnItemSelected;

            if (component.IsSelected)
            {
                if (SelectedItem != null) SelectedItem.IsSelected = false;
                SelectedItem = component;
            }

            if (component.SelectedChild != null)
            {
                if (SelectedItem != null) SelectedItem.IsSelected = false;
                SelectedItem = component.SelectedChild;
            }
        }

        public void Clear()
        {
            _children.Clear();
            ClearChildren(InnerElement);
        }

        public void Replace(Item newComponent, Item oldComponent)
        {
            var index = _children.IndexOf(oldComponent);
            if (index >= 0)
            {
                newComponent.SelectionEnabled = _selectionEnabled;
                _children[index] = newComponent;
                InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
                newComponent.InternalSelectedItem += OnItemSelected;

                if (newComponent.IsSelected)
                {
                    if (SelectedItem != null) SelectedItem.IsSelected = false;
                    SelectedItem = newComponent;
                }
            }
        }

        private void OnItemSelected(Item sender)
        {
            foreach (var c in _children)
            {
                c.UnselectRecursively(sender);
            }

            SelectedItem = sender;
            SelectedItemChanged?.Invoke(this, sender);
        }

        public Tree Items(params Item[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        [H5.Name("tss.Tree.Item")]
        public class Item : ComponentBase<Item, HTMLLIElement>, IContainer<Item, Item>
        {
            internal event ComponentEventHandler<Item> InternalSelectedItem;
            private event ComponentEventHandler<Item> SelectedItem;
            private event ComponentEventHandler<Item> ExpandedItem;
            private event ComponentEventHandler<Item> CollapsedItem;

            private readonly HTMLDivElement    _headerDiv;
            private readonly HTMLElement       _chevronSpan;
            private          HTMLElement       _iconSpan;
            private readonly HTMLElement       _checkboxSpan;
            private readonly HTMLSpanElement   _textSpan;
            private readonly HTMLUListElement  _childContainer;

            private readonly List<Item> _childItems = new List<Item>();
            private bool _isExpanded;
            private bool _isSelected;
            private bool _selectionEnabled;

            internal Item SelectedChild { get; private set; }

            internal bool SelectionEnabled
            {
                get => _selectionEnabled;
                set
                {
                    _selectionEnabled = value;
                    foreach (var child in _childItems)
                    {
                        child.SelectionEnabled = value;
                    }
                }
            }

            public Item(string text = null, string icon = null)
            {
                _chevronSpan    = I(_("tss-tree-chevron " + UIcons.AngleRight.ToString()));
                _textSpan       = Span(_("tss-tree-text", text: text));
                _childContainer = Ul(_("tss-tree-children", role: "group"));
                _checkboxSpan   = I(_("tss-tree-checkbox " + UIcons.Square.ToString()));

                _headerDiv = Div(_("tss-tree-item-content"), _chevronSpan, _checkboxSpan);

                if (!string.IsNullOrEmpty(icon))
                {
                    _iconSpan = I(_("tss-tree-icon " + icon));
                    _headerDiv.appendChild(_iconSpan);
                }

                _headerDiv.appendChild(_textSpan);

                InnerElement = Li(_("tss-tree-item", role: "treeitem"), _headerDiv, _childContainer);
                InnerElement.setAttribute("aria-expanded",  "false");
                InnerElement.setAttribute("aria-selected",  "false");

                _headerDiv.onclick = ClickHandler;
                _chevronSpan.onclick = ChevronClickHandler;
                _checkboxSpan.onclick = CheckboxClickHandler;

                UpdateChevronVisibility();
            }

            public string Text
            {
                get => _textSpan.innerText;
                set => _textSpan.innerText = value;
            }

            public string Icon
            {
                get => _iconSpan?.className.Replace("tss-tree-icon ", "");
                set
                {
                    if (string.IsNullOrEmpty(value))
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
                            _iconSpan = I(_("tss-tree-icon " + value));
                            _headerDiv.insertBefore(_iconSpan, _textSpan);
                        }
                        else
                        {
                            _iconSpan.className = "tss-tree-icon " + value;
                        }
                    }
                }
            }

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
                            _chevronSpan.classList.remove(UIcons.AngleRight.ToString());
                            _chevronSpan.classList.add(UIcons.AngleDown.ToString());
                            ExpandedItem?.Invoke(this);
                        }
                        else
                        {
                            InnerElement.classList.remove("tss-expanded");
                            _chevronSpan.classList.remove(UIcons.AngleDown.ToString());
                            _chevronSpan.classList.add(UIcons.AngleRight.ToString());
                            CollapsedItem?.Invoke(this);
                        }
                    }
                }
            }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (value != _isSelected)
                    {
                        _isSelected = value;
                        InnerElement.setAttribute("aria-selected", _isSelected ? "true" : "false");
                        if (_isSelected)
                        {
                            _headerDiv.classList.add("tss-selected");
                            _checkboxSpan.classList.replace(UIcons.Square.ToString(), UIcons.Checkbox.ToString());
                            InternalSelectedItem?.Invoke(this);
                            SelectedItem?.Invoke(this);
                        }
                        else
                        {
                            _headerDiv.classList.remove("tss-selected");
                            _checkboxSpan.classList.replace(UIcons.Checkbox.ToString(), UIcons.Square.ToString());
                        }
                    }
                }
            }

            public bool HasChildren => _childItems.Count > 0 || _childContainer.hasChildNodes();

            public override HTMLElement Render() => InnerElement;

            public void Add(Item component)
            {
                _childItems.Add(component);
                _childContainer.appendChild(component.Render());
                UpdateChevronVisibility();
                component.InternalSelectedItem += OnChildSelected;

                if (component.IsSelected)
                {
                    InternalSelectedItem?.Invoke(component);

                    if (SelectedChild != null) SelectedChild.IsSelected = false;
                    SelectedChild = component;
                }

                if (component.SelectedChild != null)
                {
                    InternalSelectedItem?.Invoke(component.SelectedChild);

                    if (SelectedChild != null) SelectedChild.IsSelected = false;
                    SelectedChild = component.SelectedChild;
                }
            }

            private void OnChildSelected(Item sender)
            {
                InternalSelectedItem?.Invoke(sender);
            }

            public void Clear()
            {
                _childItems.Clear();
                ClearChildren(_childContainer);
                UpdateChevronVisibility();
            }

            public void Replace(Item newComponent, Item oldComponent)
            {
                var index = _childItems.IndexOf(oldComponent);
                if (index >= 0)
                {
                    _childItems[index] = newComponent;
                    _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
                    newComponent.InternalSelectedItem += OnChildSelected;

                    if (newComponent.IsSelected)
                    {
                        InternalSelectedItem?.Invoke(newComponent);
                    }
                }
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
                    IsSelected = false;

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

            public Item Items(params Item[] children)
            {
                children.ForEach(x => Add(x));
                return this;
            }

            public Item ItemsAsync(Func<Task<Item[]>> childrenAsync)
            {
                bool alreadyRun = false;
                _chevronSpan.classList.add("tss-has-children"); // Show chevron indicating there *might* be children or it's expandable

                ExpandedItem += s =>
                {
                    if (!alreadyRun)
                    {
                        alreadyRun = true;

                        var loading = new Item("Loading...", UIcons.Spinner.ToString());
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

            public Item Expanded(bool isExpanded = true)
            {
                IsExpanded = isExpanded;
                return this;
            }

            public Item Selected(bool isSelected = true)
            {
                IsSelected = isSelected;
                return this;
            }

            public Item OnSelected(ComponentEventHandler<Item> onSelected)
            {
                SelectedItem += onSelected;
                return this;
            }

            public Item OnExpanded(ComponentEventHandler<Item> onExpanded)
            {
                ExpandedItem += onExpanded;
                return this;
            }

            public Item OnCollapsed(ComponentEventHandler<Item> onCollapsed)
            {
                CollapsedItem += onCollapsed;
                return this;
            }

            private void ClickHandler(MouseEvent e)
            {
                StopEvent(e);

                if (SelectionEnabled && e.shiftKey)
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

                if (SelectionEnabled)
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