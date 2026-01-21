using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Badge component for sidebar items, often used to display counts or status indicators.
    /// </summary>
    public class SidebarBadge : IComponent
    {
        private readonly Button         _button;
        private          Action<Button> _tooltip;

        /// <summary>
        /// Initializes a new instance of the SidebarBadge class.
        /// </summary>
        /// <param name="badge">The text content of the badge.</param>
        /// <param name="badgeIcon">An optional icon for the badge.</param>
        /// <param name="iconSize">The size of the icon.</param>
        public SidebarBadge(string badge, UIcons badgeIcon, TextSize iconSize = TextSize.Tiny)
        {
            _button = Button().Class("tss-sidebar-command").Class("tss-sidebar-badge");

            _button.SetIcon(badgeIcon, size: iconSize);

            if (!string.IsNullOrWhiteSpace(badge))
            {
                _button.SetText(badge);
            }
        }

        /// <summary>
        /// Initializes a new instance of the SidebarBadge class.
        /// </summary>
        /// <param name="badge">The text content of the badge.</param>
        /// <param name="badgeIcon">An optional icon for the badge.</param>
        public SidebarBadge(string badge, UIcons badgeIcon)
        {
            _button = Button().Class("tss-sidebar-command").Class("tss-sidebar-badge").SetIcon(badgeIcon);

            if (!string.IsNullOrWhiteSpace(badge))
            {
                _button.SetText(badge);
            }
        }

        /// <summary>
        /// Initializes a new instance of the SidebarBadge class.
        /// </summary>
        /// <param name="badge">The text content of the badge.</param>
        public SidebarBadge(string badge)
        {
            _button = Button().SetText(badge).Class("tss-sidebar-command").Class("tss-sidebar-badge");

            if (string.IsNullOrWhiteSpace(badge))
            {
                _button.Collapse();
            }
        }

        /// <summary>
        /// Initializes a new instance of the SidebarBadge class with an image.
        /// </summary>
        /// <param name="image">The image icon.</param>
        public SidebarBadge(ISidebarIcon image)
        {
            _button = Button().ReplaceContent(image).Class("tss-sidebar-command").Class("tss-sidebar-badge");
        }

        /// <summary>
        /// Sets the badge icon to be semi-transparent.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge SemiTransparent()
        {
            _button.Class("tss-sidebar-badge-transparent-icon");
            return this;
        }

        /// <summary>
        /// Sets the foreground color of the badge.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Foreground(string color)
        {
            _button.Foreground(color);
            return this;
        }

        /// <summary>
        /// Sets the background color of the badge.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Background(string color)
        {
            _button.Background(color);
            return this;
        }

        /// <summary>
        /// Sets the badge to use the default style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Default()
        {
            _button.IsPrimary = false;
            return this;
        }

        /// <summary>
        /// Sets the badge to use the primary style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Primary()
        {
            _button.IsPrimary = true;
            return this;
        }

        /// <summary>
        /// Sets the badge to use the success style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Success()
        {
            _button.IsSuccess = true;
            return this;
        }

        /// <summary>
        /// Sets the badge to use the danger style.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Danger()
        {
            _button.IsDanger = true;
            return this;
        }

        /// <summary>
        /// Sets a tooltip for the badge.
        /// </summary>
        /// <param name="text">The tooltip text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets a tooltip component for the badge.
        /// </summary>
        /// <param name="tooltip">The tooltip component.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Tooltip(IComponent tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets a tooltip generator function for the badge.
        /// </summary>
        /// <param name="tooltip">The tooltip generator function.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge Tooltip(Func<IComponent> tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        /// <summary>
        /// Sets the icon for the badge.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="weight">The weight of the icon.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge SetIcon(UIcons icon, string color = "", UIconsWeight weight = UIconsWeight.Regular)
        {
            _button.SetIcon(icon, color, weight: weight);
            return this;
        }

        /// <summary>
        /// Sets an emoji icon for the badge.
        /// </summary>
        /// <param name="icon">The emoji icon.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge SetIcon(Emoji icon)
        {
            _button.SetIcon(icon);
            return this;
        }

        /// <summary>
        /// Sets the text content of the badge.
        /// </summary>
        /// <param name="badge">The badge text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarBadge SetBadge(string badge)
        {
            _button.SetText(badge);

            if (string.IsNullOrEmpty(badge))
            {
                _button.Collapse();
            }
            else
            {
                _button.Show();
            }

            return this;
        }

        internal void RefreshTooltip() => _tooltip?.Invoke(_button);

        /// <summary>
        /// Renders the sidebar badge.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => _button.Render();
    }
}