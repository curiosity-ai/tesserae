using static Tesserae.UI;
using static Retyped.dom;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Retyped;

namespace Tesserae.Components
{
    public class ContextMenu : Layer, IContainer<ContextMenu, ContextMenu.Item>
    {
        private readonly HTMLElement _childContainer;
        private HTMLDivElement _modalOverlay;
        private HTMLDivElement _popup;

        public ContextMenu()
        {
            InnerElement = Div(_("tss-contextmenu"));
            _childContainer = Div(_());

            InnerElement.onclick = (e) =>
            {
                if (!IsVisible) Show();
            };
        }

        public void Clear()
        {
            ClearChildren(ScrollBar.GetCorrectContainer(_childContainer));
        }

        public void Replace(Item newComponent, Item oldComponent)
        {
            ScrollBar.GetCorrectContainer(_childContainer).replaceChild(newComponent.Render(), oldComponent.Render());
        }

        public void Add(Item component)
        {
            ScrollBar.GetCorrectContainer(_childContainer).appendChild(component.Render());
            component.OnClick((s,e) => Hide());
        }

        public override HTMLElement Render()
        {
            throw new NotImplementedException();
        }

        public override void Show()
        {
            throw new NotImplementedException();
        }

        public void ShowFor(IComponent component)
        {
            ShowFor(component.Render());
        }

        public void ShowAt(int x, int y, int minWidth)
        {
            if (_contentHtml == null)
            {
                _modalOverlay = Div(_("tss-contextmenu-overlay"));
                _modalOverlay.addEventListener("click", (_) => Hide());
                _popup = Div(_("tss-contextmenu-popup"), _childContainer);
                _contentHtml = Div(_(), _modalOverlay, _popup);
            }

            _popup.style.height = "unset";
            _popup.style.left = "-1000px";
            _popup.style.top = "-1000px";

            base.Show();

            if (!_popup.classList.contains("no-focus")) _popup.classList.add("no-focus");

            var contentRect = (ClientRect)_popup.getBoundingClientRect();
            _popup.style.left = x + "px";
            _popup.style.top = y  + "px";
            _popup.style.minWidth = minWidth + "px";

            //TODO: CHECK THIS LOGIC

            if (window.innerHeight - y - 1 < contentRect.height)
            {
                var top = y - contentRect.height;
                if (top < 0)
                {
                    if (y > window.innerHeight - y - 1)
                    {
                        _popup.style.top = "1px";
                        _popup.style.height = y - 1 + "px";
                    }
                    else
                    {
                        _popup.style.height = window.innerHeight - y - 1 + "px";
                    }
                }
                else
                {
                    _popup.style.top = top + "px";
                }
            }

            window.setTimeout((e) =>
            {
                //document.addEventListener("click", OnWindowClick);
                //document.addEventListener("dblclick", OnWindowClick);
                //document.addEventListener("contextmenu", OnWindowClick);
                //document.addEventListener("wheel", OnWindowClick);
                document.addEventListener("keydown", OnPopupKeyDown);
            }, 100);
        }
        public void ShowFor(HTMLElement element)
        {
            if (_contentHtml == null)
            {
                _modalOverlay = Div(_("tss-contextmenu-overlay"));
                _modalOverlay.addEventListener("click", (_) => Hide());
                _popup = Div(_("tss-contextmenu-popup"), _childContainer);
                _contentHtml = Div(_(), _modalOverlay, _popup);
            }

            _popup.style.height = "unset";
            _popup.style.left = "-1000px";
            _popup.style.top = "-1000px";

            base.Show();

            if (!_popup.classList.contains("no-focus")) _popup.classList.add("no-focus");

            ClientRect rect = (ClientRect)element.getBoundingClientRect();
            var contentRect = (ClientRect)_popup.getBoundingClientRect();
            _popup.style.left = rect.left + "px";
            _popup.style.top = rect.bottom - 1 + "px";
            _popup.style.minWidth = rect.width + "px";


            //TODO: CHECK THIS LOGIC

            if (window.innerHeight - rect.bottom - 1 < contentRect.height)
            {
                var top = rect.top - contentRect.height;
                if (top < 0)
                {
                    if (rect.top > window.innerHeight - rect.bottom - 1)
                    {
                        _popup.style.top = "1px";
                        _popup.style.height = rect.top - 1 + "px";
                    }
                    else
                    {
                        _popup.style.height = window.innerHeight - rect.bottom - 1 + "px";
                    }
                }
                else
                {
                    _popup.style.top = top + "px";
                }
            }

            window.setTimeout((e) =>
            {
                //document.addEventListener("click", OnWindowClick);
                //document.addEventListener("dblclick", OnWindowClick);
                //document.addEventListener("contextmenu", OnWindowClick);
                //document.addEventListener("wheel", OnWindowClick);
                document.addEventListener("keydown", OnPopupKeyDown);
            }, 100);
        }

        public override void Hide(Action onHidden = null)
        {
            //document.removeEventListener("click", OnWindowClick);
            //document.removeEventListener("dblclick", OnWindowClick);
            //document.removeEventListener("contextmenu", OnWindowClick);
            //document.removeEventListener("wheel", OnWindowClick);
            document.removeEventListener("keydown", OnPopupKeyDown);
            base.Hide(onHidden);
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

        public ContextMenu Items(params ContextMenu.Item[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        private void OnWindowClick(Event e)
        {
            if (e.srcElement != _childContainer && !_childContainer.contains(e.srcElement)) Hide();
        }

        private void OnPopupKeyDown(Event e)
        {
            var ev = e as KeyboardEvent;
            if (ev.key == "ArrowUp")
            {
                if (_contentHtml.classList.contains("no-focus")) _contentHtml.classList.remove("no-focus");
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
                if (_contentHtml.classList.contains("no-focus")) _contentHtml.classList.remove("no-focus");
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
                InnerElement = Button(_("tss-contextmenu-item", text: text));
                AttachClick();
                InnerElement.addEventListener("mouseover", OnItemMouseOver);
            }

            public Item(IComponent component)
            {
                InnerElement = Button(_("tss-contextmenu-item"));
                InnerElement.appendChild(component.Render());
                AttachClick();
                InnerElement.addEventListener("mouseover", OnItemMouseOver);
            }


            public ItemType Type
            {
                get
                {
                    if (InnerElement.classList.contains("tss-contextmenu-item")) return ItemType.Item;
                    if (InnerElement.classList.contains("tss-contextmenu-header")) return ItemType.Header;
                    return ItemType.Divider;
                }
                set
                {
                    InnerElement.classList.remove($"tss-contextmenu-{Type.ToString().ToLower()}");
                    InnerElement.classList.add($"tss-contextmenu-{value.ToString().ToLower()}");

                    if (value == ItemType.Item) InnerElement.tabIndex = 0;
                    else InnerElement.tabIndex = -1;
                }
            }

            public bool IsEnabled
            {
                get => !InnerElement.classList.contains("disabled");
                set
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

            public string Text
            {
                get => InnerElement.innerText;
                set => InnerElement.innerText = value;
            }

            public override HTMLElement Render()
            {
                return InnerElement;
            }

            public ContextMenu.Item Header()
            {
                Type = ContextMenu.ItemType.Header;
                return this;
            }
            public ContextMenu.Item Divider()
            {
                Type = ContextMenu.ItemType.Divider;
                return this;
            }
            public ContextMenu.Item Disabled()
            {
                IsEnabled = false;
                return this;
            }

            public new ContextMenu.Item OnClick(ComponentEventHandler<MouseEvent>  e)
            {
                if (Type == ItemType.Item)
                {
                    onClick += e;
                }
                return this;
            }

            private void OnItemMouseOver(Event ev)
            {
                if (Type == ItemType.Item) InnerElement.focus();
            }
        }
    }
}
