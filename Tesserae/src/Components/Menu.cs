using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
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
    /// classic right-click context menu (which uses precise mouse-tracking to keep submenus open
    /// while the cursor traces toward them) use the existing <see cref="ContextMenu"/> component.
    /// </para>
    /// <para>
    /// Unlike the legacy <see cref="ContextMenu"/>, <see cref="Menu"/> imposes no artificial limit
    /// on submenu depth — a menu item's submenu may itself contain items with submenus, and so on.
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
    [H5.Name("tss.Menu")]
    public sealed class Menu
    {
        private readonly List<Item>     _items = new List<Item>();
        private readonly Popover        _popover;
        private          TooltipPlacement _placement = TooltipPlacement.BottomStart;
        private          Menu             _parent;
        private          Menu             _openChild;
        private          Action           _onHidden;

        /// <summary>Creates a new, empty menu.</summary>
        public Menu()
        {
            _popover = new Popover()
                .Placement(_placement)
                .HideOnClickOutside(true)
                .HideOnEscape(true)
                .OnHidden(() =>
                {
                    // Cascade-close any submenu that may still be open
                    _openChild?.Hide();
                    _openChild = null;
                    _onHidden?.Invoke();
                });
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

            // Close any sibling menu opened from the same parent before showing ourselves
            if (_parent is object && _parent._openChild is object && _parent._openChild != this)
            {
                _parent._openChild.Hide();
            }
            if (_parent is object) _parent._openChild = this;

            _popover.Content(BuildContent());
            _popover.ShowFor(anchor);
            return this;
        }

        /// <summary>Hides the menu (and any of its open submenus) if currently visible.</summary>
        public void Hide()
        {
            _openChild?.Hide();
            _openChild = null;
            _popover.Hide();
        }

        // Called from the leaf level when an item is activated — closes the entire menu stack.
        internal void HideAllUpwards()
        {
            var top = this;
            while (top._parent is object) top = top._parent;
            top.Hide();
        }

        private IComponent BuildContent()
        {
            var container = Div(_("tss-menu"));
            foreach (var item in _items)
            {
                container.appendChild(item.RenderInMenu(this));
            }
            return new Raw(container);
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
        [H5.Name("tss.Menu.Item")]
        public sealed class Item
        {
            private readonly string  _text;
            private readonly UIcons? _icon;
            private          Menu    _subMenu;
            private          Action  _onClick;
            private          bool    _disabled;
            private          ItemType _type = ItemType.Item;
            internal         Menu    _parent;
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
            /// Attaches a nested <see cref="Menu"/> to this item. When the user hovers or focuses the item,
            /// the submenu opens beside it. Submenus may be nested arbitrarily deep.
            /// </summary>
            public Item SubMenu(Menu submenu)
            {
                _subMenu = submenu;
                if (submenu is object) submenu._parent = _parent;
                return this;
            }

            internal HTMLElement RenderInMenu(Menu parent)
            {
                _parent = parent;
                if (_subMenu is object) _subMenu._parent = parent;

                switch (_type)
                {
                    case ItemType.Divider: return Div(_("tss-menu-divider"));
                    case ItemType.Header:  return Div(_("tss-menu-header"), Span(_(text: _text)));
                }

                var classes = "tss-menu-item" + (_disabled ? " tss-disabled" : "") + (_subMenu is object ? " tss-menu-has-submenu" : "");
                var label   = Span(_("tss-menu-item-label", text: _text));
                var row     = _icon.HasValue
                    ? Div(_(classes), I(_icon.Value, cssClass: "tss-menu-item-icon"), label)
                    : Div(_(classes), label);

                if (_subMenu is object)
                {
                    row.appendChild(I(UIcons.AngleRight, cssClass: "tss-menu-item-submenu-icon"));
                    row.addEventListener("mouseenter", (_) => OpenSubMenu());
                    row.addEventListener("focus",      (_) => OpenSubMenu());
                }

                if (!_disabled)
                {
                    row.tabIndex = 0;
                    row.addEventListener("click", (_) => Activate());
                    row.addEventListener("keydown", (e) =>
                    {
                        var ke = (KeyboardEvent)e;
                        if (ke.key == "Enter" || ke.key == " ") { Activate(); ke.preventDefault(); }
                    });
                }
                else
                {
                    row.tabIndex = -1;
                }

                _rendered = row;
                return row;
            }

            private void OpenSubMenu()
            {
                if (_subMenu is null || _rendered is null) return;
                _subMenu.Placement(TooltipPlacement.RightStart);
                _subMenu.ShowFor(_rendered);
            }

            private void Activate()
            {
                if (_disabled) return;
                if (_subMenu is object)
                {
                    OpenSubMenu();
                    return;
                }
                _onClick?.Invoke();
                _parent?.HideAllUpwards();
            }
        }
    }
}
