using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed partial class ContextMenu
    {
        public enum ItemType
        {
            Item,
            Header,
            Divider
        }

        public class Item : ComponentBase<Item, HTMLElement>
        {
            private readonly HTMLElement _innerComponent;
            internal ContextMenu _subMenu;
            private event ComponentEventHandler<Item> PossiblyOpenSubMenu;
            internal bool currentlyMouseovered = false;

            public bool HasSubMenu => _subMenu != null;

            public Item(string text = string.Empty)
            {
                _innerComponent = null;
                InnerElement    = Button(_("tss-contextmenu-item", text: text));
                AttachClick();
                InnerElement.addEventListener("mouseenter", OnItemMouseEnter);
                InnerElement.addEventListener("mouseleave", OnItemMouseLeave);
            }

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
                    else InnerElement.tabIndex                        = -1;
                }
            }

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

            public string Text
            {
                get => InnerElement.innerText;
                set => InnerElement.innerText = value;
            }

            public override HTMLElement Render()
            {
                return InnerElement;
            }

            public Item Header()
            {
                Type = ItemType.Header;
                return this;
            }

            public Item Divider()
            {
                Type = ItemType.Divider;
                return this;
            }

            public Item Disabled(bool value = true)
            {
                IsEnabled = !value;
                return this;
            }

            public Item SubMenu(ContextMenu cm)
            {
                _subMenu = cm;
                if (cm._items.Any(i => i.HasSubMenu))
                {
                    //TODO implement submenu of submenus (bad ux though)
                    throw new InvalidOperationException("Sub menus of submenus currently not supported");
                }

                InnerElement.appendChild(I(_("las la-angle-right tss-contextmenu-submenu-button-icon")));
                return this;
            }

            public new Item OnClick(ComponentEventHandler<Item, MouseEvent> e)
            {
                if (Type == ItemType.Item)
                {
                    if (HasSubMenu)
                    {
                        _subMenu.OnItemClick(e);
                    }
                    else
                    {
                        Clicked += e;
                        if (_innerComponent is object)
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

                return this;
            }


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
                        currentlyMouseovered = true;
                        PossiblyOpenSubMenu?.Invoke(this);
                    }
                }
            }

            private void OnItemMouseLeave(Event mouseEvent)
            {
                if (mouseEvent is MouseEvent e)
                {
                    currentlyMouseovered = false;
                }
            }
        }
    }
}