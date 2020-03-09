using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Sidebar : IComponent
    {
        public enum Size
        {
            Small,
            Medium,
            Large
        }

        private HTMLElement _sidebarContainer;
        private HTMLElement _contentContainer;
        private HTMLElement _container;
        private List<Item> _items = new List<Item>();
        private ResizeObserver _resizeObserver;

        public event OnBeforeSelectHandler onBeforeSelect;
        public delegate bool OnBeforeSelectHandler(Item willBeSelected, Item currentlySelected);

        public bool IsLight
        {
            get => _sidebarContainer.classList.contains("light");
            set
            {
                if (value) _sidebarContainer.classList.add("light");
                else _sidebarContainer.classList.remove("light");
            }
        }

        public Size Width
        {
            get
            {

                if (_sidebarContainer.classList.contains("small"))
                {
                    return Size.Small;
                }
                else if (_sidebarContainer.classList.contains("medium"))
                {
                    return Size.Medium;
                }
                else
                {
                    return Size.Large;
                }
            }
            set
            {
                if (value == Size.Small)
                {
                    _sidebarContainer.classList.add("small");
                    _sidebarContainer.classList.remove("medium");
                }
                else if (value == Size.Medium)
                {
                    _sidebarContainer.classList.add("medium");
                    _sidebarContainer.classList.remove("small");
                }
                else
                {
                    _sidebarContainer.classList.remove("small");
                    _sidebarContainer.classList.remove("medium");
                }
            }
        }

        public bool IsVisible
        {
            get => !_container.classList.contains("hidden");
            set
            {
                if (value) _container.classList.remove("hidden");
                else _container.classList.add("hidden");
            }
        }

        public bool IsAlwaysOpen
        {
            get => _container.classList.contains("open");
            set
            {
                if (value)
                {
                    _container.classList.add("open");
                    EnableResizeMonitor();
                }
                else
                {
                    _container.classList.remove("open");
                }
                RecomputeContainerMargin();
            }
        }


        public Sidebar()
        {
            _sidebarContainer = Div(_("tss-sidebar"));
            _contentContainer = Div(_("tss-sidebar-content"));
            _container = Div(_("tss-sidebar-host"), _sidebarContainer, _contentContainer);
            Width = Size.Medium;
        }

        public Sidebar SetContent(IComponent content)
        {
            ClearChildren(_contentContainer);
            _contentContainer.appendChild(content.Render());
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
            if(_sidebarContainer.childElementCount == 0)
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
            this.onBeforeSelect += onBeforeSelect;
            return this;
        }

        public HTMLElement Render()
        {
            return _container;
        }

        private void SelectItem(Item item)
        {
            foreach(var i in _items)
            {
                if(i != item)
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
            if(IsAlwaysOpen)
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

            if (onBeforeSelect is object)
            {
                return onBeforeSelect(willBeSelected, currentlySelected);
            }
            else
            {
                return true;
            }
        }

        public class Item : IComponent
        {
            protected HTMLElement _container;
            private HTMLSpanElement _label;
            private HTMLElement _icon;
            private bool _isSelectable = true;
            internal Sidebar parent;

            public bool IsEnabled
            {
                get => !_container.classList.contains("disabled");
                set { if(value) _container.classList.add("disabled"); else _container.classList.remove("disabled"); }
            }

            public bool IsLarge
            {
                get => !_container.classList.contains("extrapadding");
                set { if(value) _container.classList.add("extrapadding"); else _container.classList.remove("extrapadding"); }
            }

            public bool IsSelectable
            {
                get => _isSelectable;
                set
                {
                    _isSelectable = value;
                    if(!_isSelectable)
                    {
                        _container.classList.remove("selected");
                        _container.classList.add("nonselectable");
                    }
                    else
                    {
                        _container.classList.remove("nonselectable");
                    }
                }
            }

            public event SidebarItemSelectedHandler onSelected;
            public delegate void SidebarItemSelectedHandler(Item sender);

            public bool IsSelected
            {
                get => IsSelectable ? _container.classList.contains("selected") : false;
                set
                {
                    if (!IsSelectable) return;

                    var changed = value != IsSelected;

                    if (value)
                    {
                        _container.classList.add("selected");
                        if (changed)
                        {
                            parent?.SelectItem(this);
                        }
                    }
                    else
                    {
                        _container.classList.remove("selected");
                    }
                }
            }

            public Item(string text, IComponent icon) : this(text, "")
            {
                _icon = icon.Render();
                CreateSelf(text);
            }

            public Item(string text, string icon)
            {
                _icon = I(_(icon));
                CreateSelf(text);
            }

            private void CreateSelf(string text)
            {
                _label = Span(_("tss-sidebar-label", text: text));
                _container = Div(_("tss-sidebar-item"), Div(_("tss-sidebar-icon"), _icon), _label);

                _container.onclick = (e) =>
                {
                    StopEvent(e);
                    if (parent is object)
                    {
                        if (!parent.OnBeforeSelect(this))
                        {
                            return;
                        }
                    }
                    IsSelected = true;
                    onSelected?.Invoke(this);
                };
            }

            public Item SetIcon(string icon)
            {
                if (_icon is null) return this;
                _icon.className = icon;
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

            public Item OnSelect(SidebarItemSelectedHandler onSelect)
            {
                onSelected += onSelect;
                return this;
            }

            public HTMLElement Render()
            {
                return _container;
            }
        }
    }
}
