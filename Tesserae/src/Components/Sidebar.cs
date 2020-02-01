using System;
using System.Collections.Generic;
using System.Linq;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Sidebar : IComponent
    {
        private HTMLElement _sidebarContainer;
        private HTMLElement _contentContainer;
        private HTMLElement _container;
        private List<Item> _items = new List<Item>();

        public event OnBeforeSelectHandler onBeforeSelect;
        public delegate bool OnBeforeSelectHandler(Item willBeSelected, Item currentlySelected);

        public bool IsLight { get { return _sidebarContainer.classList.contains("light"); } set { if (value) _sidebarContainer.classList.add("light"); else _sidebarContainer.classList.remove("light"); } }

        public Sidebar()
        {
            _sidebarContainer = Div(_("tss-sidebar"));
            _contentContainer = Div(_("tss-sidebar-content"));
            _container = Div(_("tss-sidebar-host"), _sidebarContainer, _contentContainer);
        }

        public Sidebar Content(IComponent content)
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

        public Sidebar Clear()
        {
            _items.Clear();
            ClearChildren(_sidebarContainer);
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

        private bool OnBeforeSelect(Item willBeSelected)
        {
            var currentlySelected = _items.Where(i => i.IsSelected).FirstOrDefault();
            if (currentlySelected == willBeSelected) return false;

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
            private bool _isSelectable;
            internal Sidebar parent;

            public bool IsEnabled
            {
                get { return !_container.classList.contains("disabled"); }
                set { if(value) _container.classList.add("disabled"); else _container.classList.remove("disabled"); }
            }

            public bool IsLarge
            {
                get { return !_container.classList.contains("extrapadding"); }
                set { if(value) _container.classList.add("extrapadding"); else _container.classList.remove("extrapadding"); }
            }

            public bool IsSelectable
            {
                get { return _isSelectable; }
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
                get { return IsSelectable ? _container.classList.contains("selected") : false; }
                set
                {
                    if (!IsSelectable) return;
                    if (IsSelected != value)
                    {
                        if (value)
                        {
                            _container.classList.add("selected");
                            parent?.SelectItem(this);
                        }
                        else
                        {
                            _container.classList.remove("selected");
                        }
                    }
                }
            }

            public Item(string text, IComponent icon) : this(text, "")
            {
                var newIcon = icon.Render();
                _container.replaceChild(newIcon, _icon);
                _icon = newIcon;
            }

            public Item(string text, string icon)
            {
                _label = Span(_("tss-sidebar-label", text: text));
                _icon = I(_(icon));
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