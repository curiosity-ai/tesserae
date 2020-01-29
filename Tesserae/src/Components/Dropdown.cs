using static Tesserae.UI;
using static Retyped.dom;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Retyped;

namespace Tesserae.Components
{
    public class Dropdown : Layer, IContainer<Dropdown, Dropdown.Item>, ICanValidate<Dropdown>
    {
        private readonly HTMLElement _childContainer;

        private readonly HTMLDivElement _container;
        private readonly HTMLSpanElement _errorSpan;

        private HTMLDivElement _spinner;

        private bool _isChanged;

        private List<Item> _selectedChildren;

        public Dropdown()
        {
            InnerElement = Div(_("tss-dropdown"));
            _errorSpan = Span(_("tss-dropdown-error"));

            _container = Div(_("tss-dropdown-container"), InnerElement, _errorSpan);

            _childContainer = Div(_());

            InnerElement.onclick = (e) =>
            {
                if (!IsVisible) Show();
            };
            _selectedChildren = new List<Item>();
        }

        public SelectMode Mode
        {
            get { return _childContainer.classList.contains("tss-dropdown-multi") ? SelectMode.Multi : SelectMode.Single; }
            set
            {
                if (value != Mode)
                {
                    if (value == SelectMode.Single)
                    {
                        _childContainer.classList.remove("tss-dropdown-multi");

                    }
                    else
                    {
                        _childContainer.classList.add("tss-dropdown-multi");
                    }
                }
            }
        }

        public Item[] SelectedItems { get { return _selectedChildren.ToArray(); } }

        public string SelectedText
        {
            get
            {
                return string.Join(", ", _selectedChildren.Select(x => x.Text));
            }
        }

        public string Error
        {
            get { return _errorSpan.innerText; }
            set
            {
                if (_errorSpan.innerText != value)
                {
                    _errorSpan.innerText = value;
                }
            }
        }

        public bool IsInvalid
        {
            get { return _container.classList.contains("invalid"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        _container.classList.add("invalid");
                    }
                    else
                    {
                        _container.classList.remove("invalid");
                    }
                }
            }
        }

        public bool IsEnabled
        {
            get { return !InnerElement.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
                {
                    if (value)
                    {
                        InnerElement.classList.remove("disabled");
                    }
                    else
                    {
                        InnerElement.classList.add("disabled");
                    }
                }
            }
        }

        public bool IsRequired
        {
            get { return _container.classList.contains("tss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        _container.classList.add("tss-required");
                    }
                    else
                    {
                        _container.classList.remove("tss-required");
                    }
                }
            }
        }
        public Func<Task<Item[]>> ItemsSource { get; set; }

        public void Clear()
        {
            ClearChildren(_childContainer);
        }

        public void Replace(Item newComponent, Item oldComponent)
        {
            _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
        }

        public void Remove(NavLink oldComponent)
        {
            _childContainer.removeChild(oldComponent.Render());
        }

        public void Add(Item component)
        {
            _childContainer.appendChild(component.Render());
            component.OnSelect += OnItemSelected;

            if (component.IsSelected)
            {

                OnItemSelected(null, component);
            }
        }

        public override HTMLElement Render()
        {
            return _container;
        }

        public override void Show()
        {
            if (_ContentHtml == null)
            {
                _ContentHtml = Div(_("tss-dropdown-popup"), _childContainer);
                if (ItemsSource != null)
                {
                    _spinner = Div(_("tss-spinner"));
                    _container.appendChild(_spinner);
                    _container.style.pointerEvents = "none";
                    Task.Run(async () =>
                    {
                        this.Items(await ItemsSource());
                        Show();
                        _container.removeChild(_spinner);
                        _container.style.pointerEvents = "unset";
                    });
                    return;
                }
            }

            _ContentHtml.style.height = "unset";
            _ContentHtml.style.left = "-1000px";
            _ContentHtml.style.top = "-1000px";

            base.Show();

            _isChanged = false;

            if (!_ContentHtml.classList.contains("no-focus")) _ContentHtml.classList.add("no-focus");

            ClientRect rect = (ClientRect)InnerElement.getBoundingClientRect();
            var contentRect = (ClientRect)_ContentHtml.getBoundingClientRect();
            _ContentHtml.style.left = rect.left + "px";
            _ContentHtml.style.top = rect.bottom - 1 + "px";
            _ContentHtml.style.width = rect.width + "px";

            if (window.innerHeight - rect.bottom - 1 < contentRect.height)
            {
                var top = rect.top - contentRect.height;
                if (top < 0)
                {
                    if (rect.top > window.innerHeight - rect.bottom - 1)
                    {
                        _ContentHtml.style.top = "1px";
                        _ContentHtml.style.height = rect.top - 1 + "px";
                    }
                    else
                    {
                        _ContentHtml.style.height = window.innerHeight - rect.bottom - 1 + "px";
                    }
                }
                else
                {
                    _ContentHtml.style.top = top + "px";
                }
            }

            window.setTimeout((e) =>
            {
                document.addEventListener("click", OnWindowClick);
                document.addEventListener("dblclick", OnWindowClick);
                document.addEventListener("contextmenu", OnWindowClick);
                document.addEventListener("wheel", OnWindowClick);
                document.addEventListener("keydown", OnPopupKeyDown);
                if (_selectedChildren.Count > 0) _selectedChildren[_selectedChildren.Count - 1].Render().focus();
            }, 100);
        }

        public override void Hide()
        {
            document.removeEventListener("click", OnWindowClick);
            document.removeEventListener("dblclick", OnWindowClick);
            document.removeEventListener("contextmenu", OnWindowClick);
            document.removeEventListener("wheel", OnWindowClick);
            document.removeEventListener("keydown", OnPopupKeyDown);
            base.Hide();
            if (_isChanged) RaiseOnChange(this);
        }

        public void Attach(EventHandler<Event> handler, Validation.Mode mode)
        {
            if (mode == Validation.Mode.OnBlur)
            {
                onChange += (s, e) => handler(this, e);
            }
            else
            {
                onInput += (s, e) => handler(this, e);
            }
        }

        private void OnWindowClick(Event e)
        {
            if (e.srcElement != _childContainer && !_childContainer.contains(e.srcElement)) Hide();
        }

        private void OnItemSelected(object sender, Item e)
        {
            if (Mode == SelectMode.Single && !e.IsSelected) return;

            if (Mode == SelectMode.Single)
            {
                if (_selectedChildren.Count > 0)
                {
                    foreach (var selectedChild in _selectedChildren)
                    {
                        selectedChild.IsSelected = false;
                    }

                    _selectedChildren.Clear();
                }

                _selectedChildren.Add(e);

                Hide();
            }
            else
            {
                if (_selectedChildren.Contains(e))
                {
                    _selectedChildren.Remove(e);
                }
                else
                {
                    _selectedChildren.Add(e);
                }

                _isChanged = true;
            }

            InnerElement.innerText = SelectedText;
            RaiseOnInput(this);
        }

        private void OnPopupKeyDown(Event e)
        {
            var ev = e as KeyboardEvent;
            if (ev.key == "ArrowUp")
            {
                if (_ContentHtml.classList.contains("no-focus")) _ContentHtml.classList.remove("no-focus");
                if (document.activeElement != null && _childContainer.contains(document.activeElement))
                {
                    var el = (_childContainer.children.TakeWhile(x => !x.Equals(document.activeElement)).LastOrDefault(x => (x as HTMLElement).tabIndex != -1) as HTMLElement);
                    if (el != null) el.focus();
                    else (_childContainer.children.Last(x => (x as HTMLElement).tabIndex != -1) as HTMLElement).focus();
                }
                else
                {
                    (_childContainer.children.Last(x => (x as HTMLElement).tabIndex != -1) as HTMLElement).focus();
                }
            }
            else if (ev.key == "ArrowDown")
            {
                if (_ContentHtml.classList.contains("no-focus")) _ContentHtml.classList.remove("no-focus");
                if (document.activeElement != null && _childContainer.contains(document.activeElement))
                {
                    var el = (_childContainer.children.SkipWhile(x => !x.Equals(document.activeElement)).Skip(1).FirstOrDefault(x => (x as HTMLElement).tabIndex != -1) as HTMLElement);
                    if (el != null) el.focus();
                    else (_childContainer.children.First(x => (x as HTMLElement).tabIndex != -1) as HTMLElement).focus();
                }
                else
                {
                    (_childContainer.children.First(x => (x as HTMLElement).tabIndex != -1) as HTMLElement).focus();
                }
            }
        }

        public enum SelectMode
        {
            Single,
            Multi
        }

        public enum ItemType
        {
            Item,
            Header,
            Divider
        }

        public class Item : ComponentBase<Item, HTMLButtonElement>
        {
            public Item(string text = string.Empty)
            {
                InnerElement = Button(_("tss-dropdown-item", text: text));
                InnerElement.addEventListener("click", OnItemClick);
                InnerElement.addEventListener("mouseover", OnItemMouseOver);
            }

            public event EventHandler<Item> OnSelect;

            public ItemType Type
            {
                get
                {
                    if (InnerElement.classList.contains("tss-dropdown-item")) return ItemType.Item;
                    if (InnerElement.classList.contains("tss-dropdown-header")) return ItemType.Header;
                    return ItemType.Divider;
                }

                set
                {
                    if (value != Type)
                    {
                        InnerElement.classList.remove($"tss-dropdown-{Type.ToString().ToLower()}");
                        InnerElement.classList.add($"tss-dropdown-{value.ToString().ToLower()}");

                        if (value == ItemType.Item) InnerElement.tabIndex = 0;
                        else InnerElement.tabIndex = -1;
                    }
                }
            }

            public bool IsEnabled
            {
                get { return !InnerElement.classList.contains("disabled"); }
                set
                {
                    if (value != IsEnabled)
                    {
                        if (value)
                        {
                            InnerElement.classList.remove("disabled");
                            if (Type == ItemType.Item) InnerElement.tabIndex = 0;
                        }
                        else
                        {
                            InnerElement.classList.add("disabled");
                            InnerElement.tabIndex = -1;
                        }
                    }
                }
            }

            public bool IsSelected
            {
                get { return InnerElement.classList.contains("selected"); }
                set
                {
                    if (value != IsSelected)
                    {
                        if (value) InnerElement.classList.add("selected");
                        else InnerElement.classList.remove("selected");
                        if (OnSelect != null) OnSelect(this, this);
                    }
                }
            }

            public string Text
            {
                get { return InnerElement.innerText; }
                set { InnerElement.innerText = value; }
            }

            public override HTMLElement Render()
            {
                return InnerElement;
            }

            private void OnItemClick(Event e)
            {
                if (Type == ItemType.Item)
                {
                    if (InnerElement.parentElement.classList.contains("tss-dropdown-multi")) IsSelected = !IsSelected;
                    else IsSelected = true;
                }
            }

            private void OnItemMouseOver(Event ev)
            {
                if (Type == ItemType.Item) InnerElement.focus();
            }
        }
    }

    public static class DropdownExtensions
    {
        public static Dropdown Single(this Dropdown dropdown)
        {
            dropdown.Mode = Dropdown.SelectMode.Single;
            return dropdown;
        }

        public static Dropdown Multi(this Dropdown dropdown)
        {
            dropdown.Mode = Dropdown.SelectMode.Multi;
            return dropdown;
        }

        public static Dropdown Items(this Dropdown container, params Dropdown.Item[] children)
        {
            children.ForEach(x => container.Add(x));
            return container;
        }
        public static Dropdown Items(this Dropdown container, Func<Task<Dropdown.Item[]>> itemsSource)
        {
            container.ItemsSource = itemsSource;
            return container;
        }

        public static Dropdown Disabled(this Dropdown dropdown)
        {
            dropdown.IsEnabled = false;
            return dropdown;
        }

        public static Dropdown Required(this Dropdown dropdown)
        {
            dropdown.IsRequired = true;
            return dropdown;
        }

        public static Dropdown.Item Header(this Dropdown.Item item)
        {
            item.Type = Dropdown.ItemType.Header;
            return item;
        }
        public static Dropdown.Item Divider(this Dropdown.Item item)
        {
            item.Type = Dropdown.ItemType.Divider;
            return item;
        }
        public static Dropdown.Item Disabled(this Dropdown.Item item)
        {
            item.IsEnabled = false;
            return item;
        }

        public static Dropdown.Item Selected(this Dropdown.Item item)
        {
            item.IsSelected = true;
            return item;
        }
    }
}
