using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ContextMenu")]
    public sealed partial class ContextMenu : Layer<ContextMenu>, IContainer<ContextMenu, ContextMenu.Item>
    {
        private readonly HTMLElement    _childContainer;
        private          HTMLDivElement _modalOverlay;
        private          HTMLDivElement _popup;

        private const int    DELAY = 200;
        private       double _timeoutId;

        private Point2D _previousMouseCoords;
        private Point2D _currentMouseCoords;

        // Extreme coordinates that form a box of the context menu + submenu. Used to check if the mouse moved out.
        private Point2D _extremeCoordsTopLeft;
        private Point2D _extremeCoordsBottomRight;

        private Point2D _menuElementCoordsTopLeft;

        private Point2D _activeSubMenuTopLeftCoords;
        private Point2D _activeSubMenuBottomLeftCoords;

        private Item        _activeMenuItem;
        private ContextMenu _activeSubMenu;

        private event ComponentEventHandler<Item, MouseEvent> ItemClick;

        private List<Item> _items = new List<Item>();

        public ContextMenu()
        {
            InnerElement    = Div(_("tss-contextmenu"));
            _childContainer = Div(_());
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
            _items.Add(component);
            _childContainer.appendChild(component.Render());

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

        private void SaveMouseCoords(double x, double y)
        {
            _previousMouseCoords.x = _currentMouseCoords.x;
            _previousMouseCoords.y = _currentMouseCoords.y;
            _currentMouseCoords.x  = x;
            _currentMouseCoords.y  = y;
        }

        private void OnWindowMouseMove(Event evnt)
        {
            var e = (MouseEvent)evnt;
            SaveMouseCoords(e.pageX, e.pageY);

            if (_activeMenuItem != null)
            {
                HideSubMenuIfCompletelyOutside();
            }
        }

        private void HideSubMenuIfCompletelyOutside()
        {
            if ((_currentMouseCoords.x    < _extremeCoordsTopLeft.x
                 || _currentMouseCoords.x > _extremeCoordsBottomRight.x
                 || _currentMouseCoords.y < _extremeCoordsTopLeft.y
                 || _currentMouseCoords.y > _extremeCoordsBottomRight.y) && !_items.Any(i => i.CurrentlyMouseovered)
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
                _modalOverlay.addEventListener("contextmenu", _ => Hide());
                _popup       = Div(_("tss-contextmenu-popup"), _childContainer);
                _contentHtml = Div(_(),                        _modalOverlay, _popup);
            }

            _popup.style.height = "unset";
            _popup.style.left   = "-1000px";
            _popup.style.top    = "-1000px";

            base.Show();

            if (!_popup.classList.contains("tss-no-focus")) _popup.classList.add("tss-no-focus");

            var popupRect = (ClientRect)_popup.getBoundingClientRect();
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

            _extremeCoordsTopLeft.x     = x;
            _extremeCoordsTopLeft.y     = y;
            _extremeCoordsBottomRight.x = x + popupRect.width;
            _extremeCoordsBottomRight.y = y + popupRect.height;

            PossiblySetupSubMenuHooks();
        }

        public void ShowFor(HTMLElement element, int distanceX = 1, int distanceY = 1) => ShowFor(element, distanceX, distanceY, false);

        private void ShowFor(HTMLElement element, int distanceX, int distanceY, bool asSubMenu)
        {
            if (asSubMenu)
            {
                _popup       = Div(_("tss-contextmenu-popup"), _childContainer);
                _contentHtml = Div(_(),                        _popup);
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

            ClientRect parentRect = (ClientRect)element.getBoundingClientRect();
            var        popupRect  = (ClientRect)_popup.getBoundingClientRect();

            var x = parentRect.left   + distanceX;
            var y = parentRect.bottom + distanceY;

            _popup.style.left     = x                + "px";
            _popup.style.top      = y                + "px";
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

            _extremeCoordsTopLeft.x     = x;
            _extremeCoordsTopLeft.y     = y;
            _extremeCoordsBottomRight.x = y + popupRect.width;
            _extremeCoordsBottomRight.y = y + popupRect.height;


            PossiblySetupSubMenuHooks();
        }

        private void CancelPendingMenuItemActivations()
        {
            if (_timeoutId > 0)
            {
                clearTimeout(_timeoutId);
            }
        }

        private double CalculateSlope(Point2D a, Point2D b)
        {
            return (b.y - a.y) / (b.x - a.x);
        }

        private Point2D CalculateTopLeftCoords(HTMLElement element)
        {
            var    rect = (DOMRect)element.getBoundingClientRect();
            double topX = rect.left + (window.pageXOffset != 0 ? window.pageXOffset : document.documentElement.scrollLeft);
            double topY = rect.top  + (window.pageYOffset != 0 ? window.pageYOffset : document.documentElement.scrollTop);
            return new Point2D(topX, topY);
        }

        private (Point2D topleft, Point2D bottomLEft, int width, int height) CalculateTopAndBottomLeftCoords(HTMLElement element)
        {
            var    rect = (DOMRect)element.getBoundingClientRect();
            double topX = rect.left + (window.pageXOffset != 0 ? window.pageXOffset : document.documentElement.scrollLeft);
            double topY = rect.top  + (window.pageYOffset != 0 ? window.pageYOffset : document.documentElement.scrollTop);
            return (new Point2D(topX, topY), new Point2D(topX, topY + element.offsetHeight), element.offsetWidth, element.offsetHeight);
        }

        private void ActivateMenuItem(Item menuItem)
        {
            _activeMenuItem = menuItem ?? throw new ArgumentNullException();

            if (_activeMenuItem.HasSubMenu)
            {
                var menuItemElement = menuItem.Render();

                _activeSubMenu = menuItem._subMenu;

                var selfRect = (ClientRect)menuItemElement.getBoundingClientRect();


                _activeSubMenu.ShowFor(menuItemElement, (int)selfRect.width, (int)-selfRect.height, asSubMenu: true);
                menuItemElement.classList.add("tss-selected");
                int activeSubMenuWidth;
                int _activeSubMenuHeight;

                (_activeSubMenuTopLeftCoords, _activeSubMenuBottomLeftCoords, activeSubMenuWidth, _activeSubMenuHeight) = CalculateTopAndBottomLeftCoords(_activeSubMenu._popup);


                _extremeCoordsTopLeft.x     = _menuElementCoordsTopLeft.x;
                _extremeCoordsTopLeft.y     = _menuElementCoordsTopLeft.y;
                _extremeCoordsBottomRight.x = _activeSubMenuTopLeftCoords.x + activeSubMenuWidth;
                _extremeCoordsBottomRight.y = _activeSubMenuTopLeftCoords.y + _activeSubMenuHeight;
            }
        }

        private bool ShouldChangeActiveMenuItem()
        {

            var shouldChange = _activeMenuItem                                       == null
             || CalculateSlope(_previousMouseCoords, _activeSubMenuTopLeftCoords)    < CalculateSlope(_currentMouseCoords, _activeSubMenuTopLeftCoords)
             || CalculateSlope(_previousMouseCoords, _activeSubMenuBottomLeftCoords) > CalculateSlope(_currentMouseCoords, _activeSubMenuBottomLeftCoords);
            return shouldChange;
        }

        private void DeactivateActiveMenuItem()
        {
            if (_activeMenuItem != null)
            {
                _activeMenuItem.HideSubmenus();
                _activeMenuItem = null;
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
                _timeoutId = window.setTimeout(args =>
                {
                    if (_items.Any(i => i.CurrentlyMouseovered))
                    {
                        CancelPendingMenuItemActivations();
                        DeactivateActiveMenuItem();
                        ActivateMenuItem(_items.First(i => i.CurrentlyMouseovered));
                    }
                }, DELAY);
            }
        }

        private void PossiblySetupSubMenuHooks()
        {
            if (_items.Any(i => i.HasSubMenu))
            {
                window.addEventListener("mousemove", OnWindowMouseMove);
                _menuElementCoordsTopLeft = CalculateTopLeftCoords(_popup);

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
            var ev = e.As<KeyboardEvent>();

            if (ev.keyCode == 38)
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
            else if (ev.keyCode == 40) // down arrow
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
            else if (ev.keyCode == 27) // Esc
            {
                Hide();
            }
        }

        private struct Point2D
        {
            public Point2D(double x, double y)
            {
                this.x = x;
                this.y = y;

            }
            public double x;
            public double y;
        }
    }
}