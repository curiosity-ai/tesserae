using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Command component for use within a Sidebar, typically appearing as a small action button.
    /// </summary>
    public class SidebarCommand : IComponent
    {
        private readonly Button               _button;
        private          Action<Button>       _tooltip;
        private          Func<ISidebarItem[]> _menuGenerator;
        private          bool                 _hookParentContextMenu;

        internal bool ShouldHookToContextMenu => _hookParentContextMenu;

        public SidebarCommand(UIcons icon, UIconsWeight weight = UIconsWeight.Regular) : this(Button().SetIcon(icon, weight: weight)) { }
        public SidebarCommand(Emoji  icon) : this(Button().SetIcon(icon)) { }

        public SidebarCommand(ISidebarIcon image)
        {
            _button = Button().ReplaceContent(image).Class("tss-sidebar-command");
        }

        private SidebarCommand(Button buttonWithIcon)
        {
            _button = buttonWithIcon.Class("tss-sidebar-command");
        }

        /// <summary>
        /// Sets the foreground color of the command button.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Foreground(string color)
        {
            _button.Foreground(color);
            return this;
        }

        /// <summary>
        /// Configures the command to hook into the parent's context menu event.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand HookToParentContextMenu()
        {
            _hookParentContextMenu = true;
            return this;
        }

        /// <summary>
        /// Sets the background color of the command button.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Background(string color)
        {
            _button.Background(color);
            return this;
        }

        /// <summary>
        /// Sets the command to use the default style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Default()
        {
            _button.IsPrimary = false;
            return this;
        }

        /// <summary>
        /// Sets the command to use the primary style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Primary()
        {
            _button.IsPrimary = true;
            return this;
        }

        /// <summary>
        /// Sets the command to use the success style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Success()
        {
            _button.IsSuccess = true;
            return this;
        }

        /// <summary>
        /// Sets the command to use the danger style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Danger()
        {
            _button.IsDanger = true;
            return this;
        }

        /// <summary>
        /// Sets a tooltip for the command.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets a tooltip component for the command.
        /// </summary>
        /// <param name="tooltip">The tooltip component.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Tooltip(IComponent tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets a tooltip generator function for the command.
        /// </summary>
        /// <param name="tooltip">The tooltip generator function.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand Tooltip(Func<IComponent> tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Configures the command to show a menu when clicked.
        /// </summary>
        /// <param name="generator">A function that generates the sidebar items for the menu.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand OnClickMenu(Func<ISidebarItem[]> generator)
        {
            _menuGenerator = generator;
            _button.OnClick(() => ShowMenu());
            return this;
        }

        /// <summary>
        /// Shows the associated menu for the command.
        /// </summary>
        public void ShowMenu()
        {
            if (_menuGenerator is null) throw new NullReferenceException("Need to configure the menu first");

            var items = _menuGenerator();

            var menuDiv = Div(_("tss-sidebar-menu"));

            foreach (var item in items)
            {
                if (item is SidebarCommands sbc)
                {
                    menuDiv.appendChild(sbc.RenderOpenFull().Render());
                }
                else
                {
                    menuDiv.appendChild(item.RenderOpen().Render());
                }
            }

            DomObserver.WhenMounted(menuDiv, () =>
            {
                _button.Render().parentElement.classList.add("tss-sidebar-command-menu-is-open");

                DomObserver.WhenRemoved(menuDiv, () =>
                {
                    _button.Render().parentElement.classList.remove("tss-sidebar-command-menu-is-open");
                    RefreshTooltip();
                });
            });

            Tippy.ShowFor(_button.Render(), menuDiv, out Action hide, placement: TooltipPlacement.BottomStart, maxWidth: 500, delayHide: 1000, theme: "tss-sidebar-tippy");

        }

        /// <summary>
        /// Programmatically raises the click event.
        /// </summary>
        /// <param name="mouseEvent">The mouse event arguments.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand RaiseOnClick(MouseEvent mouseEvent)
        {
            _button.RaiseOnClick(mouseEvent);
            return this;
        }

        /// <summary>
        /// Programmatically raises the context menu event.
        /// </summary>
        /// <param name="mouseEvent">The mouse event arguments.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand RaiseOnContextMenu(MouseEvent mouseEvent)
        {
            _button.RaiseOnContextMenu(mouseEvent);
            return this;
        }

        /// <summary>
        /// Adds a click event handler.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand OnClick(Action action)
        {
            _button.OnClick(action);
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand OnContextMenu(Action action)
        {
            _button.OnContextMenu(action);
            return this;
        }

        /// <summary>
        /// Adds a click event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand OnClick(Action<Button, MouseEvent> action)
        {
            _button.OnClick((b, e) => action(b, e));
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand OnContextMenu(Action<Button, MouseEvent> action)
        {
            _button.OnContextMenu((b, e) => action(b, e));
            return this;
        }

        /// <summary>
        /// Sets the icon for the command.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="weight">The weight of the icon.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand SetIcon(UIcons icon, string color = "", UIconsWeight weight = UIconsWeight.Regular)
        {
            _button.SetIcon(icon, color, weight: weight);
            return this;
        }

        /// <summary>
        /// Sets an emoji icon for the command.
        /// </summary>
        /// <param name="icon">The emoji icon.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarCommand SetIcon(Emoji icon)
        {
            _button.SetIcon(icon);
            return this;
        }

        internal void RefreshTooltip() => _tooltip?.Invoke(_button);

        /// <summary>
        /// Renders the sidebar command.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => _button.Render();
    }
}