using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class Sidebar : IComponent
    {
        private event OnBeforeSelectHandler BeforeSelect;
        public delegate bool OnBeforeSelectHandler(Item willBeSelected, Item currentlySelected);

        private readonly HTMLElement _sidebarContainer;
        private readonly HTMLElement _contentContainer;
        private readonly HTMLElement _container;
        private readonly List<Item> _items = new List<Item>();
        private ResizeObserver _resizeObserver;
        
        public Sidebar()
        {
            _sidebarContainer = Div(_("tss-sidebar"));
            _contentContainer = Div(_("tss-sidebar-content"));
            _container = Div(_("tss-sidebar-host"), _sidebarContainer, _contentContainer);
            Width = Size.Medium;
        }

        public Sidebar Primary()
        {
            _sidebarContainer.classList.add("tss-sidebar-primary");
            return this;
        }

        public bool IsLight
        {
            get => _sidebarContainer.classList.contains("tss-light");
            set
            {
                if (value) _sidebarContainer.classList.add("tss-light");
                else _sidebarContainer.classList.remove("tss-light");
            }
        }

        public Size Width
        {
            get
            {
                if (_sidebarContainer.classList.contains("tss-small"))
                    return Size.Small;
                else if (_sidebarContainer.classList.contains("tss-medium"))
                    return Size.Medium;
                else
                    return Size.Large;
            }
            set
            {
                if (value == Size.Small)
                {
                    _sidebarContainer.classList.add("tss-small");
                    _sidebarContainer.classList.remove("tss-medium");
                }
                else if (value == Size.Medium)
                {
                    _sidebarContainer.classList.add("tss-medium");
                    _sidebarContainer.classList.remove("tss-small");
                }
                else
                {
                    _sidebarContainer.classList.remove("tss-small");
                    _sidebarContainer.classList.remove("tss-medium");
                }
            }
        }

        public bool IsVisible
        {
            get => !_container.classList.contains("tss-hidden");
            set
            {
                if (value) _container.classList.remove("tss-hidden");
                else _container.classList.add("tss-hidden");
            }
        }

        public bool IsAlwaysOpen
        {
            get => _container.classList.contains("tss-open");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-open");
                    EnableResizeMonitor();
                }
                else
                {
                    _container.classList.remove("tss-open");
                }
                RecomputeContainerMargin();
            }
        }

        public Sidebar SetContent(IComponent content)
        {
            ClearChildren(_contentContainer);
            _contentContainer.appendChild(content.Render());
            return this;
        }

        public Sidebar ContentPadding(UnitSize padding)
        {
            _sidebarContainer.style.padding = padding.ToString();
            return this;
        }

        public Sidebar ContentPadding(UnitSize top, UnitSize bottom, UnitSize left, UnitSize right)
        {
            _sidebarContainer.style.paddingTop = top.ToString();
            _sidebarContainer.style.paddingBottom = bottom.ToString();
            _sidebarContainer.style.paddingLeft = left.ToString();
            _sidebarContainer.style.paddingRight = right.ToString();
            return this;
        }

        public Sidebar Light()
        {
            IsLight = true;
            return this;
        }
        public Sidebar Small()
        {
            Width = Size.Small;
            return this;
        }

        public Sidebar Large()
        {
            Width = Size.Large;
            return this;
        }

        public Sidebar AlwaysOpen()
        {
            IsAlwaysOpen = true;
            return this;
        }

        public Sidebar Clear()
        {
            _items.Clear();
            ClearChildren(_sidebarContainer);
            return this;
        }

        public Sidebar Brand(IComponent brand)
        {
            if (_sidebarContainer.childElementCount == 0)
            {
                _sidebarContainer.appendChild(brand.Render());
            }
            else
            {
                _sidebarContainer.insertBefore(brand.Render(), _sidebarContainer.firstElementChild);
            }
            return this;
        }

        public Sidebar Add(Item item)
        {
            item.parent = this;
            _items.Add(item);
            _sidebarContainer.appendChild(item.Render());
            return this;
        }

        public Sidebar OnBeforeSelect(OnBeforeSelectHandler onBeforeSelect)
        {
            BeforeSelect += onBeforeSelect;
            return this;
        }

        public HTMLElement Render() => _container;

        private void SelectItem(Item item)
        {
            foreach (var i in _items)
            {
                if (i != item)
                {
                    i.IsSelected = false;
                }
            }
        }

        private void EnableResizeMonitor()
        {
            if (_resizeObserver is null)
            {
                _resizeObserver = new ResizeObserver();
                _resizeObserver.Observe(_sidebarContainer);
                _resizeObserver.OnResize = RecomputeContainerMargin;
            }
        }

        private void RecomputeContainerMargin()
        {
            if (IsAlwaysOpen)
            {
                var rect = (DOMRect)_sidebarContainer.getBoundingClientRect();
                _contentContainer.style.marginLeft = rect.width.px().ToString();
            }
            else
            {
                _contentContainer.style.marginLeft = "";
            }
        }

        private bool OnBeforeSelect(Item willBeSelected)
        {
            var currentlySelected = _items.Where(i => i.IsSelected).FirstOrDefault();

            if (BeforeSelect is object)
            {
                return BeforeSelect(willBeSelected, currentlySelected);
            }
            else
            {
                return true;
            }
        }

        public sealed class Item : IComponent, IHasForegroundColor, IHasBackgroundColor
        {
            private HTMLElement _container;
            private HTMLElement _label;
            private readonly HTMLElement _icon;
            private bool _isSelectable = true;
            private bool _hasOnClick = false;
            private bool _hasOnSelect = false;
            private bool _onBottom = false;
            internal Sidebar parent;

            public bool IsEnabled
            {
                get => !_container.classList.contains("tss-disabled");
                set { if (value) _container.classList.add("tss-disabled"); else _container.classList.remove("tss-disabled"); }
            }

            public bool IsLarge
            {
                get => !_container.classList.contains("tss-extrapadding");
                set { if (value) _container.classList.add("tss-extrapadding"); else _container.classList.remove("tss-extrapadding"); }
            }

            public string Foreground { get => _container.style.color; set => _container.style.color = value; }
            public string Background { get => _container.style.background; set => _container.style.background = value; }

            public bool IsSelectable
            {
                get => _isSelectable;
                set
                {
                    _isSelectable = value;
                    if (!_isSelectable)
                    {
                        _container.classList.remove("tss-selected");
                        _container.classList.add("tss-nonselectable");
                    }
                    else
                    {
                        _container.classList.remove("tss-nonselectable");
                    }
                }
            }

            private event SidebarItemHandler ClickedItem;
            private event SidebarItemHandler SelectedItem;

            public delegate void SidebarItemHandler(Item sender);

            public bool IsSelected
            {
                get => IsSelectable && _container.classList.contains("tss-selected");
                set
                {
                    if (!IsSelectable) return;

                    var changed = value != IsSelected;

                    if (value)
                    {
                        _container.classList.add("tss-selected");
                        if (changed)
                        {
                            parent?.SelectItem(this);
                        }
                    }
                    else
                    {
                        _container.classList.remove("tss-selected");
                    }
                }
            }

     
            public Item(string text, IComponent icon, string href = null)
            {
                _icon = icon.Render();
                CreateSelf(Span(_("tss-sidebar-label", text: text)), href);
            }

            public Item(string text, string icon, string href = null)
            {
                _icon = I(_(icon));
                CreateSelf(Span(_("tss-sidebar-label", text: text)), href);
            }

            public Item(IComponent content, string href = null)
            {
                _icon = null;
                CreateSelf(content, href);
            }

            public Item(IComponent content, IComponent icon, string href = null)
            {
                _icon = icon.Render();
                CreateSelf(content.Render(), href);
            }

            public Item(IComponent content, string icon, string href = null)
            {
                _icon = I(_(icon));
                CreateSelf(content.Render(), href);
            }

            private void CreateSelf(HTMLElement text, string href)
            {
                _label = text;

                if (string.IsNullOrEmpty(href))
                {
                    _container = Div(_("tss-sidebar-item"), Div(_("tss-sidebar-icon"), _icon), _label);
                }
                else
                {
                    _container = A(_("tss-sidebar-item", href: href), Div(_("tss-sidebar-icon"), _icon), _label);
                }

                AppendOnClick(href);
            }

            private void CreateSelf(IComponent component, string href)
            {
                _label = null;

                if (string.IsNullOrEmpty(href))
                {
                    _container = Div(_("tss-sidebar-item"), component.Render());
                }
                else
                {
                    _container = A(_("tss-sidebar-item", href: href), component.Render());
                }

                AppendOnClick(href);
            }

            private void AppendOnClick(string href)
            {
                _container.onclick = (e) =>
                {
                    if (_hasOnClick || _hasOnSelect)
                    {
                        StopEvent(e);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(href))
                        {
                            StopEvent(e);
                            Router.Navigate(href);
                        }
                    }

                    ClickedItem?.Invoke(this);

                    if (!IsSelectable)
                    {
                        return;
                    }

                    if (parent is object)
                    {
                        if (!parent.OnBeforeSelect(this))
                        {
                            return;
                        }
                    }

                    IsSelected = true;

                    SelectedItem?.Invoke(this);
                };
            }

            public Item SetIcon(string icon)
            {
                if (_icon is null) return this;
                _icon.className = icon;
                return this;
            }

            public Item SetIcon(LineAwesome icon)
            {
                if (_icon is null) return this;
                _icon.className = $"{LineAwesomeWeight.Light} {icon}";
                return this;
            }

            public Item SetText(string text)
            {
                if (_label is null) return this;
                _label.textContent = text;
                return this;
            }

            public Item Selected()
            {
                IsSelected = true;
                return this;
            }

            public Item SelectedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    IsSelected = true;
                }
                return this;
            }

            public Item Large()
            {
                IsLarge = true;
                return this;
            }

            public Item NonSelectable()
            {
                IsSelectable = false;
                return this;
            }

            public Item Spacer()
            {
                _container.classList.add("tss-sidebar-spacer");
                return this;
            }

            public Item OnSelect(SidebarItemHandler onSelect)
            {
                _hasOnSelect = true;
                SelectedItem += onSelect;
                return this;
            }

            public Item OnClick(SidebarItemHandler onClick)
            {
                _hasOnClick = true;
                ClickedItem += onClick;
                return this;
            }

            public HTMLElement Render() => _container;
        }

        public enum Size
        {
            Small,
            Medium,
            Large
        }
    }
}