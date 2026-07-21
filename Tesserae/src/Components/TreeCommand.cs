using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Command component for use within a Tree item, typically appearing as a small action button.
    /// </summary>
    public class TreeCommand : IComponent
    {
        private readonly Button         _button;
        private readonly IComponent     _renderedComponent;
        private          Action<Button> _tooltip;
        private          Func<TreeCommand[]> _menuGenerator;
        private          bool           _hookParentContextMenu;

        internal bool ShouldHookToContextMenu => _hookParentContextMenu;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TreeCommand(UIcons icon, UIconsWeight weight = UIconsWeight.Regular) : this(null, Button().SetIcon(icon, weight: weight)) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TreeCommand(string href, UIcons icon, UIconsWeight weight = UIconsWeight.Regular) : this(href, Button().SetIcon(icon, weight: weight)) { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TreeCommand(Emoji  icon) : this(null, Button().SetIcon(icon)) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TreeCommand(string href, Emoji  icon) : this(href, Button().SetIcon(icon)) { }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TreeCommand(ISidebarIcon image) : this(null, image) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TreeCommand(string href, ISidebarIcon image)
        {
            _button = Button().ReplaceContent(image).Class("tss-tree-command");
            _renderedComponent = string.IsNullOrWhiteSpace(href) ? (IComponent)_button : UI.Link(href, _button, noUnderline: true);
        }

        private TreeCommand(Button buttonWithIcon) : this(null, buttonWithIcon) { }
        private TreeCommand(string href, Button buttonWithIcon)
        {
            _button = buttonWithIcon.Class("tss-tree-command");
            _renderedComponent = string.IsNullOrWhiteSpace(href) ? (IComponent)_button : UI.Link(href, _button, noUnderline: true);
        }

        /// <summary>
        /// Sets the foreground color of the command button.
        /// </summary>
        public TreeCommand Foreground(string color)
        {
            _button.Foreground(color);
            return this;
        }

        /// <summary>
        /// Configures the command to hook into the parent tree item's context menu event.
        /// </summary>
        public TreeCommand HookToParentContextMenu()
        {
            _hookParentContextMenu = true;
            return this;
        }

        /// <summary>
        /// Sets the background color of the command button.
        /// </summary>
        public TreeCommand Background(string color)
        {
            _button.Background(color);
            return this;
        }

        /// <summary>
        /// Sets the command to use the default style.
        /// </summary>
        public TreeCommand Default()
        {
            _button.IsPrimary = false;
            return this;
        }

        /// <summary>
        /// Sets the command to use the primary style.
        /// </summary>
        public TreeCommand Primary()
        {
            _button.IsPrimary = true;
            return this;
        }

        /// <summary>
        /// Sets the command to use the success style.
        /// </summary>
        public TreeCommand Success()
        {
            _button.IsSuccess = true;
            return this;
        }

        /// <summary>
        /// Sets the command to use the danger style.
        /// </summary>
        public TreeCommand Danger()
        {
            _button.IsDanger = true;
            return this;
        }

        /// <summary>
        /// Sets a tooltip for the command.
        /// </summary>
        public TreeCommand Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets a tooltip component for the command.
        /// </summary>
        public TreeCommand Tooltip(IComponent tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets a tooltip generator function for the command.
        /// </summary>
        public TreeCommand Tooltip(Func<IComponent> tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Configures the command to show a menu when clicked.
        /// </summary>
        /// <param name="generator">A function that generates the tree commands for the menu.</param>
        public TreeCommand OnClickMenu(Func<TreeCommand[]> generator)
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

            var menuDiv = Div(Att("tss-tree-menu"));

            foreach (var item in items)
            {
                menuDiv.appendChild(item.Render());
            }

            DomObserver.WhenMounted(menuDiv, () =>
            {
                _button.Render().parentElement.classList.add("tss-tree-command-menu-is-open");

                DomObserver.WhenRemoved(menuDiv, () =>
                {
                    _button.Render().parentElement.classList.remove("tss-tree-command-menu-is-open");
                    RefreshTooltip();
                });
            });

            Tippy.ShowFor(_button.Render(), menuDiv, out Action hide, placement: TooltipPlacement.BottomStart, maxWidth: 500, delayHide: 1000, theme: "tss-tree-tippy");
        }

        /// <summary>
        /// Programmatically raises the click event.
        /// </summary>
        public TreeCommand RaiseOnClick(MouseEvent mouseEvent)
        {
            _button.RaiseOnClick(mouseEvent);
            return this;
        }

        /// <summary>
        /// Programmatically raises the context menu event.
        /// </summary>
        public TreeCommand RaiseOnContextMenu(MouseEvent mouseEvent)
        {
            _button.RaiseOnContextMenu(mouseEvent);
            return this;
        }

        /// <summary>
        /// Adds a click event handler.
        /// </summary>
        public TreeCommand OnClick(Action action)
        {
            _button.OnClick(action);
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler.
        /// </summary>
        public TreeCommand OnContextMenu(Action action)
        {
            _button.OnContextMenu(action);
            return this;
        }

        /// <summary>
        /// Adds a click event handler with button and mouse event arguments.
        /// </summary>
        public TreeCommand OnClick(Action<Button, MouseEvent> action)
        {
            _button.OnClick((b, e) => action(b, e));
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler with button and mouse event arguments.
        /// </summary>
        public TreeCommand OnContextMenu(Action<Button, MouseEvent> action)
        {
            _button.OnContextMenu((b, e) => action(b, e));
            return this;
        }

        /// <summary>
        /// Sets the icon for the command.
        /// </summary>
        public TreeCommand SetIcon(UIcons icon, string color = "", UIconsWeight weight = UIconsWeight.Regular)
        {
            _button.SetIcon(icon, color, weight: weight);
            return this;
        }

        /// <summary>
        /// Sets an emoji icon for the command.
        /// </summary>
        public TreeCommand SetIcon(Emoji icon)
        {
            _button.SetIcon(icon);
            return this;
        }

        /// <summary>
        /// Sets the text label for the command.
        /// </summary>
        public TreeCommand SetText(string text)
        {
            _button.SetText(text);
            return this;
        }

        internal void RefreshTooltip() => _tooltip?.Invoke(_button);

        /// <summary>
        /// Renders the tree command.
        /// </summary>
        public HTMLElement Render() => _renderedComponent.Render();
    }
}
