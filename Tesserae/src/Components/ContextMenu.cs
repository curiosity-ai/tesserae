using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed partial class ContextMenu : Layer<ContextMenu>, IContainer<ContextMenu, ContextMenu.Item>
    {

        private struct Point2d
        {
            public Point2d(double x, double y)
            {
                this.x = x;
                this.y = y;

            }
            public double x;
            public double y;
        }

        private readonly HTMLElement    _childContainer;
        private          HTMLDivElement _modalOverlay;
        private          HTMLDivElement _popup;


        //Submenu handling
        private const int delay = 200;
        private double timeoutId;
        private Point2d previousMouseCoordinates;
        private Point2d currentMouseCoordinates;

        // Coords that box the context menu + submenu. Used to check if the mouse moved out.
        private Point2d extremeCoordinatesTopLeft;
        private Point2d extremeCoordinatesBottomRight;

        private Point2d menuElementCoordinatesTopLeft;

        private Point2d activeSubMenuTopLeftCoordinates;
        private Point2d activeSubMenuBottomLeftCoordinates;
        private int activeSubMenuWidth;
        private int activeSubMenuHeight;

        private Item activeMenuItem;
        private ContextMenu activeSubMenu;
        private double possiblyDeactivateActiveMenuItemTimout;

        private double printLocationTimout;

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

            component.OnClick((s, e) =>
            {
                ItemClick?.Invoke(s, e);
                Hide();
            });
        }
        
        public override HTMLElement Render()
        {
            throw new NotImplementedException();
        }

        public override ContextMenu Show()
        {
            throw new NotImplementedException();
        }

        private void SaveMouseCoordinates(double x, double y)
        {
            previousMouseCoordinates.x = currentMouseCoordinates.x;
            previousMouseCoordinates.y = currentMouseCoordinates.y;
            currentMouseCoordinates.x = x;
            currentMouseCoordinates.y = y;
        }

        private void OnWindowMouseMove(Event evnt)
        {
            var e = (MouseEvent) evnt;
            SaveMouseCoordinates(e.pageX, e.pageY);

            if (activeMenuItem != null)
            {
                HideSubMenuIfCompletelyOutside();
            }
        }

        private void HideSubMenuIfCompletelyOutside()
        {
            if ((currentMouseCoordinates.x < extremeCoordinatesTopLeft.x
              || currentMouseCoordinates.x > extremeCoordinatesBottomRight.x
              || currentMouseCoordinates.y < extremeCoordinatesTopLeft.y
              || currentMouseCoordinates.y > extremeCoordinatesBottomRight.y) && !_items.Any(i => i.currentlyMouseovered)
            )
            {
                CancelPendingMenuItemActivations();
                DeactivateActiveMenuItem();
            }
        }

        private void OnItemClick(ComponentEventHandler<Item, MouseEvent> componentEventHandler)
        {
            ItemClick += componentEventHandler;
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

            extremeCoordinatesTopLeft.x = x;
            extremeCoordinatesTopLeft.y = y;
            extremeCoordinatesBottomRight.x = x + popupRect.width;
            extremeCoordinatesBottomRight.y = y + popupRect.height;

            PossiblySetupSubMenuHooks();
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

            var x = parentRect.left + distanceX;
            var y = parentRect.bottom + distanceY;

            _popup.style.left     = x + "px";
            _popup.style.top      = y + "px";
            _popup.style.minWidth = parentRect.width + "px";

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

            extremeCoordinatesTopLeft.x = x;
            extremeCoordinatesTopLeft.y = y;
            extremeCoordinatesBottomRight.x = y + popupRect.width;
            extremeCoordinatesBottomRight.y = y + popupRect.height;


            PossiblySetupSubMenuHooks();
        }

        private void CancelPendingMenuItemActivations()
        {
            if (timeoutId > 0)
            {
                clearTimeout(timeoutId);
            }
        }

        private double CalculateSlope(Point2d a, Point2d b)
        {
            return (b.y - a.y) / (b.x - a.x);
        }

        private Point2d CalculateTopLeftCoords(HTMLElement element)
        {
            var rect = (DOMRect) element.getBoundingClientRect();
            double topX = rect.left + (window.pageXOffset != 0 ? window.pageXOffset : document.documentElement.scrollLeft);
            double topY = rect.top + (window.pageYOffset != 0 ? window.pageYOffset : document.documentElement.scrollTop);
            return new Point2d(topX, topY);
        }

        private (Point2d topleft, Point2d bottomLEft, int width, int height) CalculateTopAndBottomLeftCoords(HTMLElement element)
        {
            var rect = (DOMRect) element.getBoundingClientRect();
            double topX = rect.left + (window.pageXOffset != 0 ? window.pageXOffset : document.documentElement.scrollLeft);
            double topY = rect.top + (window.pageYOffset != 0 ? window.pageYOffset : document.documentElement.scrollTop);
            return (new Point2d(topX, topY), new Point2d(topX, topY + element.offsetHeight), element.offsetWidth, element.offsetHeight);
        }

        private void ActivateMenuItem(Item menuItem)
        {
            activeMenuItem = menuItem ?? throw new ArgumentNullException();

            if (activeMenuItem.HasSubMenu)
            {
                var menuItemElement = menuItem.Render();

                activeSubMenu = menuItem._subMenu;

                var selfRect = (ClientRect) menuItemElement.getBoundingClientRect();


                activeSubMenu.ShowFor(menuItemElement, (int) selfRect.width, (int) -selfRect.height, asSubMenu: true);
                menuItemElement.classList.add("tss-selected");


                (activeSubMenuTopLeftCoordinates, activeSubMenuBottomLeftCoordinates, activeSubMenuWidth, activeSubMenuHeight) = CalculateTopAndBottomLeftCoords(activeSubMenu._popup);


                extremeCoordinatesTopLeft.x = menuElementCoordinatesTopLeft.x;
                extremeCoordinatesTopLeft.y = menuElementCoordinatesTopLeft.y;
                extremeCoordinatesBottomRight.x = activeSubMenuTopLeftCoordinates.x + activeSubMenuWidth;
                extremeCoordinatesBottomRight.y = activeSubMenuTopLeftCoordinates.y + activeSubMenuHeight;
            }
        }

        private bool ShouldChangeActiveMenuItem()
        {

            var shouldChange = activeMenuItem == null
                            || CalculateSlope(previousMouseCoordinates, activeSubMenuTopLeftCoordinates) < CalculateSlope(currentMouseCoordinates, activeSubMenuTopLeftCoordinates)
                            || CalculateSlope(previousMouseCoordinates, activeSubMenuBottomLeftCoordinates) > CalculateSlope(currentMouseCoordinates, activeSubMenuBottomLeftCoordinates);
            return shouldChange;
        }

        private void DeactivateActiveMenuItem()
        {
            if (activeMenuItem != null)
            {
                activeMenuItem.HideSubmenus();
                activeMenuItem = null;
            }
        }

        private bool PossiblyActivateMenuItem(Item menuItem)
        {
            CancelPendingMenuItemActivations();
            if (!ShouldChangeActiveMenuItem())
            {
                return false;
            }
            else
            {
                DeactivateActiveMenuItem();
                ActivateMenuItem(menuItem);
                return true;
            }
        }
        
        private void OnMenuItemMouseEnter(Item item)
        {
            if (!PossiblyActivateMenuItem(item))
            {
                timeoutId = window.setTimeout(args =>
                {
                    if (_items.Any(i => i.currentlyMouseovered))
                    {
                        CancelPendingMenuItemActivations();
                        DeactivateActiveMenuItem();
                        ActivateMenuItem(_items.First(i => i.currentlyMouseovered));
                    }
                }, delay);
            }
        }

        private void PossiblySetupSubMenuHooks()
        {
            if (_items.Any(i => i.HasSubMenu))
            {
                window.addEventListener("mousemove", OnWindowMouseMove);
                menuElementCoordinatesTopLeft = CalculateTopLeftCoords(_popup);
                foreach (var item in _items)
                {
                    item.HookMouseEnter(OnMenuItemMouseEnter);
                }
            }
        }

        public override void Hide(Action onHidden = null)
        {
            window.removeEventListener("mousemove", OnWindowMouseMove);
            document.removeEventListener("keydown", OnPopupKeyDown);
            base.Hide(onHidden);
            foreach (var item in _items)
            {
                item.HideSubmenus();
                item.UnHookMouseEnter(OnMenuItemMouseEnter);
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