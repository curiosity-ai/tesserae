using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarBadge : IComponent
    {
        private readonly Button         _button;
        private          Action<Button> _tooltip;

        public SidebarBadge(string badge, UIcons badgeIcon, TextSize iconSize = TextSize.Tiny)
        {
            _button = Button().Class("tss-sidebar-command").Class("tss-sidebar-badge");

            _button.SetIcon(badgeIcon, size: iconSize);

            if (!string.IsNullOrWhiteSpace(badge))
            {
                _button.SetText(badge);
            }
        }

        public SidebarBadge(string badge, UIcons badgeIcon)
        {
            _button = Button().Class("tss-sidebar-command").Class("tss-sidebar-badge").SetIcon(badgeIcon);

            if (!string.IsNullOrWhiteSpace(badge))
            {
                _button.SetText(badge);
            }
        }

        public SidebarBadge(string badge)
        {
            _button = Button().SetText(badge).Class("tss-sidebar-command").Class("tss-sidebar-badge");

            if (string.IsNullOrWhiteSpace(badge))
            {
                _button.Collapse();
            }
        }

        public SidebarBadge(ISidebarIcon image)
        {
            _button = Button().ReplaceContent(image).Class("tss-sidebar-command").Class("tss-sidebar-badge");
        }

        public SidebarBadge SemiTransparent()
        {
            _button.Class("tss-sidebar-badge-transparent-icon");
            return this;
        }

        public SidebarBadge Foreground(string color)
        {
            _button.Foreground(color);
            return this;
        }

        public SidebarBadge Background(string color)
        {
            _button.Background(color);
            return this;
        }

        public SidebarBadge Default()
        {
            _button.IsPrimary = false;
            return this;
        }

        public SidebarBadge Primary()
        {
            _button.IsPrimary = true;
            return this;
        }

        public SidebarBadge Success()
        {
            _button.IsSuccess = true;
            return this;
        }

        public SidebarBadge Danger()
        {
            _button.IsDanger = true;
            return this;
        }

        public SidebarBadge Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        public SidebarBadge Tooltip(IComponent tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        public SidebarBadge Tooltip(Func<IComponent> tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        public SidebarBadge SetIcon(UIcons icon, string color = "", UIconsWeight weight = UIconsWeight.Regular)
        {
            _button.SetIcon(icon, color, weight: weight);
            return this;
        }

        public SidebarBadge SetIcon(Emoji icon)
        {
            _button.SetIcon(icon);
            return this;
        }

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

        public HTMLElement Render() => _button.Render();
    }
}