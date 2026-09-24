using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A menu surface composed of clickable items, optional headers, dividers and arbitrarily deep
    /// nested submenus. Built on top of the <see cref="Tesserae.Popover"/> primitive, so positioning,
    /// click-outside dismissal and animation are inherited from the shared popover machinery.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Menu"/> is the recommended primitive for application menus, dropdown menus and
    /// action menus shown in response to a button click or other explicit user gesture. For the
    /// classic right-click context menu use the existing <see cref="ContextMenu"/> component.
    /// </para>
    /// <para>
    /// Submenus open on hover and follow the pointer's intent: a pointer that is heading from a row
    /// towards the submenu it opened may cross the rows below it without switching, a pointer that
    /// comes to rest on another row switches to that row, and coming back over the row that opened
    /// the submenu leaves it exactly as it was. The keyboard walks the same tree - arrows move,
    /// ArrowRight and Enter open a submenu, ArrowLeft and Escape close one level - and a menu item's
    /// submenu may itself contain items with submenus, and so on.
    /// </para>
    /// <example>
    /// <code>
    /// var fileMenu = UI.Menu()
    ///     .Items(
    ///         UI.MenuHeader("File"),
    ///         UI.MenuItem("New",    UIcons.Plus).OnClick(() => New()),
    ///         UI.MenuItem("Open…",  UIcons.FolderOpen).OnClick(() => Open()),
    ///         UI.MenuDivider(),
    ///         UI.MenuItem("Export").SubMenu(
    ///             UI.Menu().Items(
    ///                 UI.MenuItem("PDF").OnClick(() => ExportPdf()),
    ///                 UI.MenuItem("CSV").OnClick(() => ExportCsv()))));
    ///
    /// var trigger = UI.Button("File").OnClick(b => fileMenu.ShowFor(b));
    /// </code>
    /// </example>
    /// </remarks>
    [Transpose.Name("tss.Menu")]
    public sealed class Menu
    {
        // How long an open submenu is held while the pointer is on another row: the time it has to
        // reach the submenu it is heading for, and the time a pointer parked on a plain row waits
        // before that row wins. The same order as the platform menus (macOS and GTK both sit near it).
        private const int SWITCH_DELAY = 300;

        private readonly List<Item>       _items = new List<Item>();
        private readonly Popover          _popover;
        private          TooltipPlacement _placement = TooltipPlacement.BottomStart;
        private          Menu             _parent;
        private          Menu             _openChild;
        private          Item             _openChildItem;
        private          Action           _onHidden;
        private          HTMLElement      _container;

        // Pointer intent, all in viewport coordinates: the two most recent positions seen over this
        // menu, the row the pointer is on, and the row waiting for the switch delay to run out.
        private double _lastX, _lastY, _prevX, _prevY;
        private bool   _hasPointer;
        private Item   _hoveredItem;
        private Item   _pendingItem;
        private double _switchTimeout;

        private readonly Action<Event> _onKeyDown;
        private readonly Action<Event> _onMouseMove;

        /// <summary>Creates a new, empty menu.</summary>
        public Menu()
        {
            _onKeyDown   = (e) => OnKeyDown((KeyboardEvent)e);
            _onMouseMove = (e) => OnMouseMove((MouseEvent)e);

            _popover = new Popover()
                .Placement(_placement)
                .HideOnClickOutside(true)
                .HideOnEscape(false) // Escape is handled here, one level at a time, rather than by every open popover at once
                .OnHidden(OnPopoverHidden);
        }

        /// <summary>
        /// Adds a single item to this menu. Most code prefers the params overload
        /// <see cref="Items(Item[])"/> together with the <c>UI.MenuItem(...)</c> / <c>UI.MenuHeader(...)</c>
        /// factory helpers.
        /// </summary>
        public Menu Add(Item item)
        {
            if (item is null) return this;
            item._parent = this;
            _items.Add(item);
            return this;
        }

        /// <summary>Adds the given items to this menu, in order.</summary>
        public Menu Items(params Item[] items)
        {
            if (items == null) return this;
            foreach (var item in items) Add(item);
            return this;
        }

        /// <summary>
        /// Sets the preferred placement of this menu relative to its anchor. Defaults to
        /// <see cref="TooltipPlacement.BottomStart"/>, which mirrors the standard dropdown-menu placement
        /// of platforms like Windows and macOS.
        /// </summary>
        public Menu Placement(TooltipPlacement placement)
        {
            _placement = placement;
            _popover.Placement(placement);
            return this;
        }

        /// <summary>Registers a callback that fires after this menu has been hidden.</summary>
        public Menu OnHidden(Action onHidden)
        {
            _onHidden += onHidden;
            return this;
        }

        /// <summary>Gets a value indicating whether the menu is currently displayed.</summary>
        public bool IsVisible => _popover.IsVisible;

        /// <summary>
        /// Shows the menu anchored to the rendered element of <paramref name="anchor"/>.
        /// </summary>
        public Menu ShowFor(IComponent anchor)
        {
            if (anchor is null) return this;
            return ShowFor(anchor.Render());
        }

        /// <summary>Shows the menu anchored to the given DOM element.</summary>
        public Menu ShowFor(HTMLElement anchor)
        {
            if (anchor is null) return this;

            if (_parent is object)
            {
                // Already open from this very row: leave it alone. Re-showing would rebuild the popover
                // and close everything below it, which is what used to happen each time the pointer
                // came back over the row that opened the submenu.
                if (_parent._openChild == this && IsVisible) return this;

                // Close any sibling menu opened from the same parent before showing ourselves
                if (_parent._openChild is object && _parent._openChild != this)
                {
                    _parent._openChild.Hide();
                }
                _parent._openChild = this;

                // A submenu lives inside its parent's popover, and a press on the parent's rows is not a
                // reason for it to go: the parent's click-outside closes the whole tree, a plain row
                // activating closes it too, and the pointer logic above decides everything in between.
                _popover.HideOnClickOutside(false);
            }

            _popover.Content(BuildContent());
            _popover.ShowFor(anchor);

            _hasPointer  = false;
            _hoveredItem = null;
            _pendingItem = null;

            _container?.addEventListener("mousemove", _onMouseMove);

            // One keyboard handler for the whole tree, on the root: it always acts on the deepest open level.
            if (_parent is null) document.addEventListener("keydown", _onKeyDown);

            return this;
        }

        /// <summary>Hides the menu (and any of its open submenus) if currently visible.</summary>
        public void Hide()
        {
            CancelPendingSwitch();
            _openChild?.Hide();
            _openChild     = null;
            _openChildItem = null;
            _popover.Hide();
        }

        // Called from the leaf level when an item is activated — closes the entire menu stack.
        internal void HideAllUpwards()
        {
            var top = this;
            while (top._parent is object) top = top._parent;
            top.Hide();
        }

        private void OnPopoverHidden()
        {
            CancelPendingSwitch();

            // Cascade-close any submenu that may still be open
            _openChild?.Hide();
            _openChild     = null;
            _openChildItem = null;

            _container?.removeEventListener("mousemove", _onMouseMove);
            _container = null;

            if (_parent is null) document.removeEventListener("keydown", _onKeyDown);

            foreach (var item in _items) item.SetOpen(false);

            if (_parent is object && _parent._openChild == this)
            {
                _parent._openChild     = null;
                _parent._openChildItem?.SetOpen(false);
                _parent._openChildItem = null;
            }

            _onHidden?.Invoke();
        }

        private IComponent BuildContent()
        {
            var container = Div(Att("tss-menu", role: "menu"));
            foreach (var item in _items)
            {
                container.appendChild(item.RenderInMenu(this));
            }
            _container = container;
            return new Raw(container);
        }

        // ---- Pointer intent -----------------------------------------------------------------------

        private void RecordPointer(MouseEvent e)
        {
            if (_hasPointer)
            {
                _prevX = _lastX;
                _prevY = _lastY;
            }
            else
            {
                _prevX = e.clientX;
                _prevY = e.clientY;
            }

            _lastX      = e.clientX;
            _lastY      = e.clientY;
            _hasPointer = true;
        }

        private bool HasDirection => _hasPointer && (_prevX != _lastX || _prevY != _lastY);

        private void OnMouseMove(MouseEvent e)
        {
            RecordPointer(e);

            // A row is waiting for the pointer to make up its mind: the moment it stops heading for the
            // open submenu, that row wins - a pointer moving straight down the list should not wait.
            if (_pendingItem is object && HasDirection && !IsHeadingTowardsOpenChild())
            {
                var pending = _pendingItem;
                CancelPendingSwitch();
                SettleOn(pending);
            }
        }

        internal void OnRowEnter(Item item, MouseEvent e)
        {
            // mouseenter arrives before the mousemove of the same pointer update, so read the position
            // off it: a fast flick otherwise reaches the row with no direction to judge it by.
            RecordPointer(e);

            _hoveredItem = item;

            if (_openChild is object && _openChildItem == item)
            {
                // Back over the row whose submenu is open: nothing to change, and nothing to schedule.
                CancelPendingSwitch();
                return;
            }

            if (_openChild is object && _openChild.IsVisible && (!HasDirection || IsHeadingTowardsOpenChild()))
            {
                // Give the pointer time to reach the submenu it is aiming for - or, with no movement
                // to judge yet, to show where it is going. If it is still on this row when the time is
                // up, it meant this row after all.
                CancelPendingSwitch();
                _pendingItem   = item;
                _switchTimeout = window.setTimeout(_ =>
                {
                    _pendingItem = null;
                    if (_hoveredItem is object) SettleOn(_hoveredItem);
                }, SWITCH_DELAY);
                return;
            }

            CancelPendingSwitch();
            SettleOn(item);
        }

        internal void OnRowLeave(Item item)
        {
            if (_hoveredItem == item) _hoveredItem = null;
        }

        private void SettleOn(Item item)
        {
            if (item.HasSubMenu)
            {
                item.OpenSubMenu(focusFirst: false);
            }
            else if (_openChild is object)
            {
                // A plain row took over: the submenu that was open beside another row goes away.
                _openChild.Hide();
            }
        }

        private void CancelPendingSwitch()
        {
            if (_switchTimeout > 0) window.clearTimeout(_switchTimeout);
            _switchTimeout = 0;
            _pendingItem   = null;
        }

        // Whether the last movement points into the open submenu: the pointer is between the two rays
        // from its previous position to the near corners of the submenu, and got closer to it.
        private bool IsHeadingTowardsOpenChild()
        {
            if (!_hasPointer || _openChild is null || _openChild._container is null) return false;
            if (_prevX == _lastX && _prevY == _lastY) return false;

            var rect  = _openChild._container.getBoundingClientRect().As<DOMRect>();
            var nearX = _lastX < rect.left ? rect.left : (_lastX > rect.right ? rect.right : _lastX);

            if (nearX == _lastX) return true; // already inside the submenu's horizontal span

            var closer = Math.Abs(_lastX - nearX) < Math.Abs(_prevX - nearX);

            if (!closer) return false;

            var toTop    = Cross(nearX - _prevX, rect.top    - _prevY, _lastX - _prevX, _lastY - _prevY);
            var toBottom = Cross(nearX - _prevX, rect.bottom - _prevY, _lastX - _prevX, _lastY - _prevY);

            // Between the two rays means the two cross products have opposite signs (or one is zero).
            return toTop * toBottom <= 0;
        }

        private static double Cross(double ax, double ay, double bx, double by) => ax * by - ay * bx;

        // ---- Keyboard -----------------------------------------------------------------------------

        private Menu Deepest()
        {
            var menu = this;
            while (menu._openChild is object && menu._openChild.IsVisible) menu = menu._openChild;
            return menu;
        }

        private void OnKeyDown(KeyboardEvent e)
        {
            if (e.ctrlKey || e.altKey || e.metaKey) return;

            var menu = Deepest();

            switch (e.key)
            {
                case "ArrowDown":  menu.FocusRelative(+1); break;
                case "ArrowUp":    menu.FocusRelative(-1); break;
                case "Home":       menu.FocusEdge(first: true);  break;
                case "End":        menu.FocusEdge(first: false); break;
                case "ArrowRight":
                {
                    var row = menu.FocusedRow();
                    if (row is object && row.HasSubMenu) row.OpenSubMenu(focusFirst: true);
                    break;
                }
                case "ArrowLeft":
                    if (menu._parent is object) menu.CloseAndFocusParentRow();
                    break;
                case "Escape":
                    if (menu._parent is object) menu.CloseAndFocusParentRow();
                    else Hide();
                    break;
                default:
                    return;
            }

            e.preventDefault();
            e.stopPropagation();
        }

        private void CloseAndFocusParentRow()
        {
            var row = _parent?._openChildItem;
            Hide();
            row?.Focus();
        }

        private List<Item> FocusableRows() => _items.Where(i => i.IsFocusable).ToList();

        private Item FocusedRow() => _items.FirstOrDefault(i => i.IsFocused);

        private void FocusRelative(int delta)
        {
            var rows = FocusableRows();

            if (rows.Count == 0) return;

            var index = -1;

            for (var i = 0; i < rows.Count; i++)
            {
                if (rows[i].IsFocused) { index = i; break; }
            }

            if (index < 0)
            {
                FocusEdge(first: delta > 0);
                return;
            }

            index = (index + delta + rows.Count) % rows.Count;
            rows[index].Focus();
        }

        private void FocusEdge(bool first)
        {
            var rows = FocusableRows();

            if (rows.Count == 0) return;

            (first ? rows[0] : rows[rows.Count - 1]).Focus();
        }

        /// <summary>
        /// Specifies what role a <see cref="Item"/> plays inside its parent menu.
        /// </summary>
        public enum ItemType
        {
            /// <summary>A normal, clickable item.</summary>
            Item,
            /// <summary>A non-interactive header label used to group nearby items.</summary>
            Header,
            /// <summary>A thin horizontal divider used to visually separate groups of items.</summary>
            Divider
        }

        /// <summary>
        /// A single entry inside a <see cref="Menu"/>. Items may be normal clickable rows, section headers,
        /// dividers, or rows that fan out into a nested submenu.
        /// </summary>
        [Transpose.Name("tss.Menu.Item")]
        public sealed class Item
        {
            private readonly string      _text;
            private readonly UIcons?     _icon;
            private          Menu        _subMenu;
            private          Action      _onClick;
            private          bool        _disabled;
            private          ItemType    _type = ItemType.Item;
            internal         Menu        _parent;
            private          HTMLElement _rendered;

            /// <summary>Creates a new clickable item with the given label.</summary>
            public Item(string text) { _text = text; }

            /// <summary>Creates a new clickable item with the given label and a leading icon.</summary>
            public Item(string text, UIcons icon) { _text = text; _icon = icon; }

            /// <summary>Marks this item as a non-interactive section header.</summary>
            public Item Header()  { _type = ItemType.Header;  return this; }

            /// <summary>Marks this item as a thin horizontal divider.</summary>
            public Item Divider() { _type = ItemType.Divider; return this; }

            /// <summary>Gets or sets whether this item is enabled (clickable and focusable).</summary>
            public Item Disabled(bool disabled = true) { _disabled = disabled; return this; }

            /// <summary>Registers the click handler invoked when the user activates this item.</summary>
            public Item OnClick(Action onClick) { _onClick = onClick; return this; }

            /// <summary>
            /// Attaches a nested <see cref="Menu"/> to this item. When the user hovers the item, presses
            /// ArrowRight or Enter on it, the submenu opens beside it. Submenus may be nested arbitrarily deep.
            /// </summary>
            public Item SubMenu(Menu submenu)
            {
                _subMenu = submenu;
                if (submenu is object) submenu._parent = _parent;
                return this;
            }

            internal bool HasSubMenu  => _subMenu is object;
            internal bool IsFocusable => _type == ItemType.Item && !_disabled && _rendered is object;
            internal bool IsFocused   => _rendered is object && document.activeElement == _rendered;

            internal void Focus() => _rendered?.focus();

            internal void SetOpen(bool open)
            {
                if (_rendered is null || !HasSubMenu) return;

                _rendered.classList.toggle("tss-menu-item-open", open);
                _rendered.setAttribute("aria-expanded", open ? "true" : "false");
            }

            internal HTMLElement RenderInMenu(Menu parent)
            {
                _parent = parent;
                if (_subMenu is object) _subMenu._parent = parent;

                switch (_type)
                {
                    case ItemType.Divider: return Div(Att("tss-menu-divider", role: "separator"));
                    case ItemType.Header:  return Div(Att("tss-menu-header", role: "presentation"), Span(Att(text: _text)));
                }

                var classes = "tss-menu-item" + (_disabled ? " tss-disabled" : "") + (_subMenu is object ? " tss-menu-has-submenu" : "");
                var label   = Span(Att("tss-menu-item-label", text: _text));
                var row     = _icon.HasValue
                    ? Div(Att(classes, role: "menuitem"), I(_icon.Value, cssClass: "tss-menu-item-icon"), label)
                    : Div(Att(classes, role: "menuitem"), label);

                if (_subMenu is object)
                {
                    row.appendChild(I(UIcons.AngleRight, cssClass: "tss-menu-item-submenu-icon"));
                    row.setAttribute("aria-haspopup", "menu");
                    row.setAttribute("aria-expanded", "false");
                }

                if (!_disabled)
                {
                    row.tabIndex = 0;
                    row.addEventListener("mouseenter", (e) => _parent?.OnRowEnter(this, (MouseEvent)e));
                    row.addEventListener("mouseleave", (_) => _parent?.OnRowLeave(this));
                    row.addEventListener("click", (_) => Activate());
                    row.addEventListener("keydown", (e) =>
                    {
                        var ke = (KeyboardEvent)e;
                        if (ke.key == "Enter" || ke.key == " ") { Activate(); ke.preventDefault(); ke.stopPropagation(); }
                    });
                }
                else
                {
                    row.tabIndex = -1;
                    row.setAttribute("aria-disabled", "true");
                }

                _rendered = row;
                return row;
            }

            internal void OpenSubMenu(bool focusFirst)
            {
                if (_subMenu is null || _rendered is null) return;

                var alreadyOpen = _parent is object && _parent._openChild == _subMenu && _subMenu.IsVisible;

                if (!alreadyOpen)
                {
                    // Start-aligned with this row and touching the menu it comes out of: -5 undoes the
                    // menu's top padding and border so the submenu's first row lines up with this one.
                    _subMenu.Placement(TooltipPlacement.RightStart);
                    _subMenu._popover.Offset(-5, 0);
                    _subMenu.ShowFor(_rendered);

                    if (_parent is object)
                    {
                        _parent._openChildItem?.SetOpen(false);
                        _parent._openChildItem = this;
                    }

                    SetOpen(true);
                }

                if (focusFirst) _subMenu.FocusEdge(first: true);
            }

            private void Activate()
            {
                if (_disabled) return;
                if (_subMenu is object)
                {
                    OpenSubMenu(focusFirst: true);
                    return;
                }
                _onClick?.Invoke();
                _parent?.HideAllUpwards();
            }
        }
    }
}
