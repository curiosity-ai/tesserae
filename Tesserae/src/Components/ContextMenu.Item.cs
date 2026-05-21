using H5;
using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A right-click / hover-driven popup menu with support for items, headers, dividers and arbitrarily deep nested
    /// submenus.
    /// </summary>
    public sealed partial class ContextMenu
    {

        [Enum(Emit.StringName)] //Don't change the emit type without updating the FromClassList method
        public enum ItemType
        {
            [Name("tss-contextmenu-item")]    Item,
            [Name("tss-contextmenu-header")]  Header,
            [Name("tss-contextmenu-divider")] Divider
        }

        public class Item : ComponentBase<Item, HTMLElement>
        {
            private readonly HTMLElement              _innerComponent;
            internal         ContextMenu              _subMenu;
            private event ComponentEventHandler<Item> PossiblyOpenSubMenu;
            internal bool                             CurrentlyMouseovered = false;

            /// <summary>
            /// Returns a value indicating whether the component has the given sub menu.
            /// </summary>
            public bool HasSubMenu => _subMenu != null;

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Item(string text = string.Empty)
            {
                _innerComponent = null;
                InnerElement    = Button(_("tss-contextmenu-item", text: text));
                AttachClick();
                InnerElement.addEventListener("mouseenter", OnItemMouseEnter);
                InnerElement.addEventListener("mouseleave", OnItemMouseLeave);
            }

            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Item(IComponent component)
            {
                if (component is ITextFormating itf && (itf is Button || itf is Link))
                {
                    itf.SetTextAlign(TextAlign.Left);
                }

                _innerComponent = component.Render();
                InnerElement    = Div(_("tss-contextmenu-item"), _innerComponent);
                InnerElement.appendChild(_innerComponent);
                AttachClick();
                InnerElement.addEventListener("mouseenter", OnItemMouseEnter);
                InnerElement.addEventListener("mouseleave", OnItemMouseLeave);
            }

            /// <summary>
            /// Gets or sets the type of the item.
            /// </summary>
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
                    InnerElement.classList.remove(Type.ToString());
                    InnerElement.classList.add(value.ToString());

                    if (value == ItemType.Item) InnerElement.tabIndex = 0;
                    else InnerElement.tabIndex                        = -1;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the component is interactive (enabled).
            /// </summary>
            public bool IsEnabled
            {
                get => !InnerElement.classList.contains("tss-disabled");
                set
                {
                    if (value)
                    {
                        InnerElement.classList.remove("tss-disabled");
                        if (Type == ItemType.Item) InnerElement.tabIndex = 0;
                    }
                    else
                    {
                        InnerElement.classList.add("tss-disabled");
                        InnerElement.tabIndex = -1;
                    }
                }
            }

            /// <summary>
            /// Gets or sets the text shown in the component.
            /// </summary>
            public string Text
            {
                get => InnerElement.innerText;
                set => InnerElement.innerText = value;
            }

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public override HTMLElement Render()
            {
                return InnerElement;
            }

            /// <summary>
            /// Configures the component to header.
            /// </summary>
            public Item Header()
            {
                Type = ItemType.Header;
                return this;
            }

            /// <summary>
            /// Configures the component to divider.
            /// </summary>
            public Item Divider()
            {
                Type = ItemType.Divider;
                return this;
            }

            /// <summary>
            /// Disables the component.
            /// </summary>
            public Item Disabled(bool value = true)
            {
                IsEnabled = !value;
                return this;
            }

            /// <summary>
            /// Attaches a nested <see cref="ContextMenu"/> to this item. The submenu opens beside the item
            /// when the user hovers over it. Submenus may themselves contain items with submenus — the
            /// existing mouse-tracking and hide-cascade machinery operates recursively, so arbitrarily deep
            /// menu trees are supported. (For application-style dropdown menus opened via an explicit
            /// trigger, consider the newer <see cref="Menu"/> component, which is built on
            /// <see cref="Popover"/> instead of bespoke mouse-tracking.)
            /// </summary>
            /// <param name="cm">The submenu to attach. May itself contain items with further submenus.</param>
            public Item SubMenu(ContextMenu cm)
            {
                _subMenu = cm;
                InnerElement.appendChild(I(_($"{UIcons.AngleRight} tss-contextmenu-submenu-button-icon")));
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the click event fires.
            /// </summary>
            public override Item OnClick(ComponentEventHandler<Item, MouseEvent> e, bool clearPrevious = true)
            {
                if (Type == ItemType.Item)
                {
                    if (HasSubMenu)
                    {
                        _subMenu.OnItemClick(e, clearPrevious);
                    }
                    else
                    {
                        Clicked += e;

                        if (_innerComponent is object)
                        {
                            if (clearPrevious)
                            {
                                _innerComponent.onclick = (e2) =>
                                {
                                    if (_innerComponent.tagName != "A" || string.IsNullOrWhiteSpace(_innerComponent.As<HTMLAnchorElement>().href))
                                    {
                                        StopEvent(e2); //Stop double calling the click handler for anything but links
                                    }

                                    e.Invoke(this, e2);
                                };
                            }
                            else
                            {
                                _innerComponent.onclick += (e2) =>
                                {
                                    if (_innerComponent.tagName != "A" || string.IsNullOrWhiteSpace(_innerComponent.As<HTMLAnchorElement>().href))
                                    {
                                        StopEvent(e2); //Stop double calling the click handler for anything but links
                                    }

                                    e.Invoke(this, e2);
                                };
                            }
                        }
                    }
                }

                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the click event fires.
            /// </summary>
            public Item OnClick(Action action, bool clearPrevious = true) => OnClick((_, __) => action.Invoke(), clearPrevious);

            /// <summary>
            /// Hides the submenus.
            /// </summary>
            public void HideSubmenus()
            {
                if (_subMenu != null)
                {
                    _subMenu.Hide();
                    InnerElement.classList.remove("tss-selected");
                }
            }

            internal void HookMouseEnter(ComponentEventHandler<Item> mouseEventCallback)
            {
                PossiblyOpenSubMenu += mouseEventCallback;
            }

            internal void UnHookMouseEnter(ComponentEventHandler<Item> mouseEventCallback)
            {
                PossiblyOpenSubMenu -= mouseEventCallback;
            }

            private void OnItemMouseEnter(Event mouseEvent)
            {
                if (mouseEvent is MouseEvent e)
                {
                    if (Type == ItemType.Item)
                    {
                        InnerElement.focus();
                        CurrentlyMouseovered = true;
                        PossiblyOpenSubMenu?.Invoke(this);
                    }
                }
            }

            private void OnItemMouseLeave(Event mouseEvent)
            {
                if (mouseEvent is MouseEvent e)
                {
                    CurrentlyMouseovered = false;
                }
            }
        }
    }
}