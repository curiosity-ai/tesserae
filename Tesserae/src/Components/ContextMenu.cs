using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed partial class ContextMenu : Layer<ContextMenu>, IContainer<ContextMenu, ContextMenu.Item>
    {
        private readonly HTMLElement    _childContainer;
        private          HTMLDivElement _modalOverlay;
        private          HTMLDivElement _popup;

        private event ComponentEventHandler<Item, MouseEvent> ItemClick;


        private List<Item> _items = new List<Item>();

        public ContextMenu()
        {
            InnerElement    = Div(_("tss-contextmenu"));
            _childContainer = Div(_());
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
            _items.Add(component);
            ScrollBar.GetCorrectContainer(_childContainer).appendChild(component.Render());

            component.CloseOtherSubmenus += CloseOtherSubmenus;
            component.OnClick((s, e) =>
            {
                ItemClick?.Invoke(s, e);
                Hide();
            });
        }

        private void CloseOtherSubmenus(Item sender)
        {
            foreach (var item in _items)
            {
                if (item != sender)
                {
                    item.HideSubmenus();
                }
            }
        }

        private void HideIfNotHovered()
        {
            var anyItemHovered = false;

            foreach (var child in ScrollBar.GetCorrectContainer(_childContainer).children)
            {
                anyItemHovered = child.matches(":hover");
                if (anyItemHovered) break;
            }

            if (!anyItemHovered) Hide();
        }

        private void OnItemClick(ComponentEventHandler<Item, MouseEvent> componentEventHandler)
        {
            ItemClick += componentEventHandler;
        }


        public override HTMLElement Render()
        {
            throw new NotImplementedException();
        }

        public override ContextMenu Show()
        {
            throw new NotImplementedException();
        }

        public void ShowFor(IComponent component, int distanceX = 1, int distanceY = 1)
        {
            ShowFor(component.Render(), distanceX, distanceY);
        }

        public void ShowAt(int x, int y, int minWidth)
        {
            if (_contentHtml == null)
            {
                _modalOverlay = Div(_("tss-contextmenu-overlay"));
                _modalOverlay.addEventListener("click", _ => Hide());
                _popup       = Div(_("tss-contextmenu-popup"), _childContainer);
                _contentHtml = Div(_(),                        _modalOverlay, _popup);
            }

            _popup.style.height = "unset";
            _popup.style.left   = "-1000px";
            _popup.style.top    = "-1000px";

            base.Show();

            if (!_popup.classList.contains("tss-no-focus")) _popup.classList.add("tss-no-focus");

            var popupRect = (ClientRect) _popup.getBoundingClientRect();
            _popup.style.left     = x        + "px";
            _popup.style.top      = y        + "px";
            _popup.style.minWidth = minWidth + "px";

            //TODO: CHECK THIS LOGIC

            if (window.innerHeight - y - 1 < popupRect.height)
            {
                var top = y - popupRect.height;
                if (top < 0)
                {
                    if (y > window.innerHeight - y - 1)
                    {
                        _popup.style.top    = "1px";
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

            if (window.innerWidth - x - 1 < popupRect.width)
            {
                var left = x - popupRect.width;
                if (left < 0)
                {
                    if (x > window.innerWidth - x - 1)
                    {
                        _popup.style.left  = "1px";
                        _popup.style.width = x - 1 + "px";
                    }
                    else
                    {
                        _popup.style.width = window.innerWidth - x - 1 + "px";
                    }
                }
                else
                {
                    _popup.style.left = left + "px";
                }
            }

            window.setTimeout((e) =>
            {
                document.addEventListener("keydown", OnPopupKeyDown);
            }, 100);
        }

        public void ShowFor(HTMLElement element, int distanceX = 1, int distanceY = 1) => ShowFor(element, distanceX, distanceY, false);

        private void ShowFor(HTMLElement element, int distanceX, int distanceY, bool asSubMenu)
        {
            if (asSubMenu)
            {
                _popup       = Div(_("tss-contextmenu-popup"), _childContainer);
                _contentHtml = Div(_(),                        _modalOverlay, _popup);
            }
            else
            {
                if (_contentHtml == null)
                {
                    _modalOverlay = Div(_("tss-contextmenu-overlay"));
                    _modalOverlay.addEventListener("click", _ => Hide());
                    _popup       = Div(_("tss-contextmenu-popup"), _childContainer);
                    _contentHtml = Div(_(),                        _modalOverlay, _popup);
                }
            }


            _popup.style.height = "unset";
            _popup.style.left   = "-1000px";
            _popup.style.top    = "-1000px";

            base.Show();

            if (!_popup.classList.contains("tss-no-focus")) _popup.classList.add("tss-no-focus");

            ClientRect parentRect = (ClientRect) element.getBoundingClientRect();
            var        popupRect  = (ClientRect) _popup.getBoundingClientRect();

            _popup.style.left     = parentRect.left   + distanceX + "px";
            _popup.style.top      = parentRect.bottom + distanceY + "px";
            _popup.style.minWidth = parentRect.width  + "px";


            //TODO: CHECK THIS LOGIC

            if (window.innerHeight - parentRect.bottom - distanceY < popupRect.height)
            {
                var top = parentRect.top - popupRect.height;
                if (top < 0)
                {
                    if (parentRect.top > window.innerHeight - parentRect.bottom - distanceY)
                    {
                        _popup.style.top    = "1px";
                        _popup.style.height = parentRect.top - distanceY + "px";
                    }
                    else
                    {
                        _popup.style.height = window.innerHeight - parentRect.bottom - distanceY + "px";
                    }
                }
                else
                {
                    _popup.style.top = top + "px";
                }
            }

            if (window.innerWidth - parentRect.right - distanceX < popupRect.width)
            {
                var left = parentRect.right - popupRect.width;
                if (left < 0)
                {
                    if (parentRect.left > window.innerWidth - parentRect.right - distanceX)
                    {
                        _popup.style.left  = "1px";
                        _popup.style.width = parentRect.left - distanceX + "px";
                    }
                    else
                    {
                        _popup.style.width = window.innerWidth - parentRect.right - distanceX + "px";
                    }
                }
                else
                {
                    _popup.style.left = left + "px";
                }
            }

            window.setTimeout((e) =>
            {
                document.addEventListener("keydown", OnPopupKeyDown);
            }, 100);
        }

        public override void Hide(Action onHidden = null)
        {
            document.removeEventListener("keydown", OnPopupKeyDown);
            base.Hide(onHidden);
            foreach (var item in _items)
            {
                item.HideSubmenus();
            }
        }

        public ContextMenu Items(params Item[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        private void OnPopupKeyDown(Event e)
        {
            var ev = e as KeyboardEvent;
            if (ev.key == "ArrowUp")
            {
                if (_contentHtml.classList.contains("tss-no-focus")) _contentHtml.classList.remove("tss-no-focus");
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
                if (_contentHtml.classList.contains("tss-no-focus")) _contentHtml.classList.remove("tss-no-focus");
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
    }
}