﻿using static Tesserae.UI;
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
            ClearChildren(_childContainer);
        }

        public void Replace(Item newComponent, Item oldComponent)
        {
            _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
        }
        
        public void Add(Item component)
        {
            _childContainer.appendChild(component.Render());
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

        public void ShowFor(HTMLElement element)
        {
            if (_contentHtml == null)
            {
                _contentHtml = Div(_("tss-contextmenu-popup"), _childContainer);
            }

            _contentHtml.style.height = "unset";
            _contentHtml.style.left = "-1000px";
            _contentHtml.style.top = "-1000px";

            base.Show();

            if (!_contentHtml.classList.contains("no-focus")) _contentHtml.classList.add("no-focus");

            ClientRect rect = (ClientRect)element.getBoundingClientRect();
            var contentRect = (ClientRect)_contentHtml.getBoundingClientRect();
            _contentHtml.style.left = rect.left + "px";
            _contentHtml.style.top = rect.bottom - 1 + "px";
            _contentHtml.style.minWidth = rect.width + "px";

            if (window.innerHeight - rect.bottom - 1 < contentRect.height)
            {
                var top = rect.top - contentRect.height;
                if (top < 0)
                {
                    if (rect.top > window.innerHeight - rect.bottom - 1)
                    {
                        _contentHtml.style.top = "1px";
                        _contentHtml.style.height = rect.top - 1 + "px";
                    }
                    else
                    {
                        _contentHtml.style.height = window.innerHeight - rect.bottom - 1 + "px";
                    }
                }
                else
                {
                    _contentHtml.style.top = top + "px";
                }
            }

            window.setTimeout((e) =>
            {
                document.addEventListener("click", OnWindowClick);
                document.addEventListener("dblclick", OnWindowClick);
                document.addEventListener("contextmenu", OnWindowClick);
                document.addEventListener("wheel", OnWindowClick);
                document.addEventListener("keydown", OnPopupKeyDown);
            }, 100);
        }

        public override void Hide(Action onHidden = null)
        {
            document.removeEventListener("click", OnWindowClick);
            document.removeEventListener("dblclick", OnWindowClick);
            document.removeEventListener("contextmenu", OnWindowClick);
            document.removeEventListener("wheel", OnWindowClick);
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
                    if (value != Type)
                    {
                        InnerElement.classList.remove($"tss-contextmenu-{Type.ToString().ToLower()}");
                        InnerElement.classList.add($"tss-contextmenu-{value.ToString().ToLower()}");

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

            public string Text
            {
                get { return InnerElement.innerText; }
                set { InnerElement.innerText = value; }
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

            public ContextMenu.Item OnClick(ComponentEventHandler<MouseEvent>  e)
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