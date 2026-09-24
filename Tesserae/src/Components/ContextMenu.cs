using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A right-click / hover-driven popup menu with support for items, headers, dividers and arbitrarily deep nested
    /// submenus.
    /// </summary>
    [Transpose.Name("tss.ContextMenu")]
    public sealed partial class ContextMenu : Layer<ContextMenu>, IContainer<ContextMenu, ContextMenu.Item>
    {
        private readonly HTMLElement    _childContainer;
        private          HTMLDivElement _modalOverlay;
        private          HTMLDivElement _popup;

        // How long an open submenu is held while the pointer rests on another row of this menu - the time
        // it has to reach the submenu it was heading for. The same order as the platform menus.
        private const int    DELAY = 300;
        private       double _timeoutId;

        // Viewport coordinates of the two most recent pointer positions, for reading where it is heading.
        private Point2D _previousMouseCoords;
        private Point2D _currentMouseCoords;

        private Item        _activeMenuItem;
        private ContextMenu _activeSubMenu;

        // Set on a menu shown as a submenu: the menu and the row it came out of, so closing it from the
        // keyboard can hand the focus back.
        private ContextMenu _parentMenu;
        private Item        _parentItem;

        private readonly Action<Event> _onWindowMouseMoveAction;
        private readonly Action<Event> _onPopupKeyDownAction;

        private event Action _onHide;

        private event ComponentEventHandler<Item, MouseEvent> ItemClick;

        private List<Item> _items = new List<Item>();

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ContextMenu()
        {
            InnerElement    = Div(Att("tss-contextmenu"));
            _childContainer = Div(Att());

            _onWindowMouseMoveAction = (ev) => OnWindowMouseMove(ev);
            _onPopupKeyDownAction    = (ev) => OnPopupKeyDown(ev);
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public void Clear()
        {
            ClearChildren(_childContainer);
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(Item newComponent, Item oldComponent)
        {
            _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(Item component)
        {
            _items.Add(component);
            _childContainer.appendChild(component.Render());

            component.OnClick((s, e) =>
            {
                ItemClick?.Invoke(s, e);
                Hide();
            }, clearPrevious: false);
        }

        /// <summary>
        /// Registers a callback invoked when the hide event fires.
        /// </summary>
        public ContextMenu OnHide(Action onHidden)
        {
            _onHide += onHidden;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows the component.
        /// </summary>
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
            SaveMouseCoords(e.clientX, e.clientY);

            if (_activeMenuItem != null)
            {
                HideSubMenuIfCompletelyOutside();
            }

            // A row is waiting for the pointer to show where it is going: the moment it is not heading
            // for the open submenu, that row wins - a pointer moving down the list should not wait.
            if (_timeoutId > 0 && HasDirection && !IsHeadingTowardsActiveSubMenu())
            {
                var hovered = _items.FirstOrDefault(i => i.CurrentlyMouseovered);

                if (hovered is object && hovered != _activeMenuItem)
                {
                    CancelPendingMenuItemActivations();
                    DeactivateActiveMenuItem();
                    ActivateMenuItem(hovered);
                }
            }
        }

        private bool HasDirection => _previousMouseCoords.x != _currentMouseCoords.x || _previousMouseCoords.y != _currentMouseCoords.y;

        // Whether the pointer is over this menu's popup or over any menu open below it. A third level
        // sits beside the second, outside any box drawn from this menu to its own submenu - which is
        // what used to close the whole branch the moment the pointer reached it.
        private bool ContainsPointer(Point2D p)
        {
            if (_popup is object && IsInside(p, _popup.getBoundingClientRect().As<DOMRect>())) return true;

            return _activeSubMenu is object && _activeSubMenu.IsVisible && _activeSubMenu.ContainsPointer(p);
        }

        private static bool IsInside(Point2D p, DOMRect rect) => p.x >= rect.left && p.x <= rect.right && p.y >= rect.top && p.y <= rect.bottom;

        private void HideSubMenuIfCompletelyOutside()
        {
            if (!ContainsPointer(_currentMouseCoords) && !_items.Any(i => i.CurrentlyMouseovered))
            {
                CancelPendingMenuItemActivations();
                DeactivateActiveMenuItem();
            }
        }

        private void OnItemClick(ComponentEventHandler<Item, MouseEvent> componentEventHandler, bool clearPrevious = true)
        {
            if (ItemClick != null && clearPrevious)
            {
                foreach (Delegate d in ItemClick.GetInvocationList())
                {
                    ItemClick -= (ComponentEventHandler<Item, MouseEvent>)d;
                }
            }
            ItemClick += componentEventHandler;
        }

        /// <summary>
        /// Shows the for.
        /// </summary>
        public void ShowFor(IComponent component, int distanceX = 1, int distanceY = 1)
        {
            ShowFor(component.Render(), distanceX, distanceY);
            component.WhenMounted(() => component.WhenRemoved(() => Hide()));
        }

        private void EnsureRootPopup()
        {
            if (_contentHtml == null)
            {
                _modalOverlay = Div(Att("tss-contextmenu-overlay"));
                _modalOverlay.addEventListener("click", e => { StopEvent(e);  Hide(); });
                _modalOverlay.addEventListener("contextmenu", e => { StopEvent(e);  Hide(); });
                _popup       = Div(Att("tss-contextmenu-popup", role: "menu"), _childContainer);
                _contentHtml = Div(Att(),                        _modalOverlay, _popup);
            }
        }

        private void AfterShown()
        {
            window.setTimeout((e) =>
            {
                document.addEventListener("keydown", _onPopupKeyDownAction);
            }, 100);

            PossiblySetupSubMenuHooks();
        }

        /// <summary>
        /// Shows the at.
        /// </summary>
        public void ShowAt(int x, int y, int minWidth)
        {
            EnsureRootPopup();

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

            AfterShown();
        }

        /// <summary>
        /// Shows the for.
        /// </summary>
        public void ShowFor(HTMLElement element, int distanceX = 1, int distanceY = 1)
        {
            EnsureRootPopup();

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

            AfterShown();
        }

        // Opens this menu beside the row of another menu that it belongs to. Beside means touching the
        // parent popup's right edge, top-aligned with the row; to the left of the parent when there is no
        // room on the right, and never on top of it, which is where the old flip put it - it tested the
        // room against the row's width twice and then measured the popup before its minimum width applied.
        private void ShowAsSubMenu(ContextMenu parentMenu, Item parentItem)
        {
            _parentMenu = parentMenu;
            _parentItem = parentItem;

            _popup       = Div(Att("tss-contextmenu-popup", role: "menu"), _childContainer);
            _contentHtml = Div(Att(),                        _popup);

            _popup.style.height = "unset";
            _popup.style.width  = "unset";
            _popup.style.left   = "-1000px";
            _popup.style.top    = "-1000px";

            base.Show();

            if (!_popup.classList.contains("tss-no-focus")) _popup.classList.add("tss-no-focus");

            var itemRect   = parentItem.Render().getBoundingClientRect().As<DOMRect>();
            var parentRect = parentMenu._popup.getBoundingClientRect().As<DOMRect>();

            _popup.style.minWidth = itemRect.width + "px";

            var popupRect = _popup.getBoundingClientRect().As<DOMRect>();
            var width     = popupRect.width;
            var height    = popupRect.height;

            var x = parentRect.right;

            if (x + width > window.innerWidth)
            {
                x = parentRect.left - width;

                if (x < 0) x = Math.Max(0, window.innerWidth - width);
            }

            var y = itemRect.top;

            if (height > window.innerHeight)
            {
                y                   = 0;
                _popup.style.height = window.innerHeight + "px";
            }
            else if (y + height > window.innerHeight)
            {
                y = window.innerHeight - height;
            }

            _popup.style.left = x + "px";
            _popup.style.top  = y + "px";

            AfterShown();
        }

        private void CancelPendingMenuItemActivations()
        {
            if (_timeoutId > 0)
            {
                clearTimeout(_timeoutId);
            }
            _timeoutId = 0;
        }

        private void ActivateMenuItem(Item menuItem)
        {
            _activeMenuItem = menuItem ?? throw new ArgumentNullException();

            if (_activeMenuItem.HasSubMenu)
            {
                _activeSubMenu = menuItem._subMenu;
                _activeSubMenu.ShowAsSubMenu(this, menuItem);
                menuItem.Render().classList.add("tss-selected");
                menuItem.Render().setAttribute("aria-expanded", "true");
            }
        }

        // Whether the last pointer movement points into the open submenu: between the two rays from the
        // previous position to the near corners of the submenu, and closer to it than before.
        private bool IsHeadingTowardsActiveSubMenu()
        {
            if (_activeSubMenu is null || !_activeSubMenu.IsVisible || _activeSubMenu._popup is null) return false;

            var prev = _previousMouseCoords;
            var cur  = _currentMouseCoords;

            if (prev.x == cur.x && prev.y == cur.y) return false;

            var rect  = _activeSubMenu._popup.getBoundingClientRect().As<DOMRect>();
            var nearX = cur.x < rect.left ? rect.left : (cur.x > rect.right ? rect.right : cur.x);

            if (nearX == cur.x) return true;

            if (Math.Abs(cur.x - nearX) >= Math.Abs(prev.x - nearX)) return false;

            var toTop    = Cross(nearX - prev.x, rect.top    - prev.y, cur.x - prev.x, cur.y - prev.y);
            var toBottom = Cross(nearX - prev.x, rect.bottom - prev.y, cur.x - prev.x, cur.y - prev.y);

            return toTop * toBottom <= 0;
        }

        private static double Cross(double ax, double ay, double bx, double by) => ax * by - ay * bx;

        private void DeactivateActiveMenuItem()
        {
            if (_activeMenuItem != null)
            {
                _activeMenuItem.HideSubmenus();
                _activeMenuItem = null;
                _activeSubMenu  = null;
            }
        }

        private bool PossiblyActivateMenuItem(Item menuItem)
        {
            CancelPendingMenuItemActivations();

            // Back over the row whose submenu is open: leave it as it is. Tearing it down to show it
            // again is what made the submenu fade in anew every time the pointer crossed its row.
            if (menuItem == _activeMenuItem) return true;

            // Heading for the open submenu, or arrived too fast to tell: hold the submenu and let the
            // next movement (or the delay) decide.
            if (_activeMenuItem is object && _activeSubMenu is object && _activeSubMenu.IsVisible && (!HasDirection || IsHeadingTowardsActiveSubMenu()))
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
                    _timeoutId = 0;

                    var hovered = _items.FirstOrDefault(i => i.CurrentlyMouseovered);

                    if (hovered is object && hovered != _activeMenuItem)
                    {
                        DeactivateActiveMenuItem();
                        ActivateMenuItem(hovered);
                    }
                }, DELAY);
            }
        }

        private void PossiblySetupSubMenuHooks()
        {
            if (_items.Any(i => i.HasSubMenu))
            {
                window.addEventListener("mousemove", _onWindowMouseMoveAction);

                foreach (var item in _items)
                {
                    item.HookMouseEnter(OnMenuItemMouseEnter);
                }
            }
        }

        /// <summary>
        /// Hides the component.
        /// </summary>
        public override void Hide(Action onHidden = null)
        {
            CancelPendingMenuItemActivations();
            window.removeEventListener("mousemove", _onWindowMouseMoveAction);
            document.removeEventListener("keydown", _onPopupKeyDownAction);

            base.Hide(() => { _onHide?.Invoke(); onHidden?.Invoke(); });

            foreach (var item in _items)
            {
                item.HideSubmenus();
                item.UnHookMouseEnter(OnMenuItemMouseEnter);
            }

            _activeMenuItem = null;
            _activeSubMenu  = null;
        }

        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
        public ContextMenu Items(params Item[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        private List<HTMLElement> FocusableRows() => _childContainer.children.Select(c => (HTMLElement)c).Where(c => c.tabIndex != -1).ToList();

        private HTMLElement FocusedRow()
        {
            var active = document.activeElement;

            return active is object && _childContainer.contains(active) ? FocusableRows().FirstOrDefault(r => r == active || r.contains(active)) : null;
        }

        private void FocusRelative(int delta)
        {
            var rows = FocusableRows();

            if (rows.Count == 0) return;

            var focused = FocusedRow();
            var index   = focused is null ? -1 : rows.IndexOf(focused);

            if (index < 0)
            {
                (delta > 0 ? rows[0] : rows[rows.Count - 1]).focus();
                return;
            }

            rows[(index + delta + rows.Count) % rows.Count].focus();
        }

        private void FocusFirstRow()
        {
            var rows = FocusableRows();

            if (rows.Count > 0) rows[0].focus();
        }

        // Closes this submenu from the keyboard and puts the focus back on the row it came out of.
        private void CloseSubMenuToParent()
        {
            var parentMenu = _parentMenu;
            var parentItem = _parentItem;

            parentMenu?.CancelPendingMenuItemActivations();
            parentMenu?.DeactivateActiveMenuItem();
            parentItem?.Render().focus();
        }

        private void OnPopupKeyDown(Event e)
        {
            var ev = e.As<KeyboardEvent>();

            if (ev.ctrlKey || ev.altKey || ev.metaKey) return;

            // Every open level listens; only the deepest one acts, so the arrows do not move the focus
            // in two menus at once.
            if (_activeSubMenu is object && _activeSubMenu.IsVisible) return;

            switch (ev.key)
            {
                case "ArrowDown": FocusRelative(+1); break;
                case "ArrowUp":   FocusRelative(-1); break;
                case "Home":
                    FocusFirstRow();
                    break;
                case "End":
                {
                    var rows = FocusableRows();
                    if (rows.Count > 0) rows[rows.Count - 1].focus();
                    break;
                }
                case "ArrowRight":
                {
                    var focused = FocusedRow();
                    var item    = focused is null ? null : _items.FirstOrDefault(i => i.Render() == focused);

                    if (item is object && item.HasSubMenu)
                    {
                        CancelPendingMenuItemActivations();
                        if (_activeMenuItem != item)
                        {
                            DeactivateActiveMenuItem();
                            ActivateMenuItem(item);
                        }
                        _activeSubMenu?.FocusFirstRow();
                    }
                    break;
                }
                case "ArrowLeft":
                    if (_parentMenu is object) CloseSubMenuToParent();
                    break;
                case "Escape":
                    if (_parentMenu is object) CloseSubMenuToParent();
                    else Hide();
                    break;
                default:
                    return;
            }

            if (_contentHtml.classList.contains("tss-no-focus")) _contentHtml.classList.remove("tss-no-focus");
            if (_popup.classList.contains("tss-no-focus")) _popup.classList.remove("tss-no-focus");

            ev.preventDefault();
        }

        private struct Point2D
        {
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Point2D(double x, double y)
            {
                this.x = x;
                this.y = y;

            }
            /// <summary>
            /// The X coordinate.
            /// </summary>
            public double x;
            /// <summary>
            /// The Y coordinate.
            /// </summary>
            public double y;
        }
    }
}
