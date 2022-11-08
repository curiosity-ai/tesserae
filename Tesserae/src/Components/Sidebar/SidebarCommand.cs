using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarCommand : IComponent
    {
        private readonly Button               _button;
        private          Action<Button>       _tooltip;
        private          Func<ISidebarItem[]> _menuGenerator;
        private bool _badge;

        internal bool IsBadge => _badge;

        public SidebarCommand(LineAwesome icon, LineAwesomeWeight weight = LineAwesomeWeight.Light) : this($"{weight} {icon}") { }
        public SidebarCommand(Emoji       icon) : this($"ec {icon}") { }

        public SidebarCommand(string badge, string background, string foreground) 
        {
            _badge = true;
            _button = Button().SetText(badge).Class("tss-sidebar-command").Foreground(foreground).Background(background);
        }

        public SidebarCommand(ISidebarIcon image)
        {
            _button = Button().ReplaceContent(image).Class("tss-sidebar-command");
        }

        public SidebarCommand(string icon)
        {
            _button = Button().SetIcon(icon).Class("tss-sidebar-command");
        }

        public SidebarCommand Foreground(string color)
        {
            _button.Foreground(color);
            return this;
        }

        public SidebarCommand Background(string color)
        {
            _button.Background(color);
            return this;
        }

        public SidebarCommand Default()
        {
            _button.IsPrimary = false;
            return this;
        }

        public SidebarCommand Primary()
        {
            _button.IsPrimary = true;
            return this;
        }

        public SidebarCommand Success()
        {
            _button.IsSuccess = true;
            return this;
        }

        public SidebarCommand Danger()
        {
            _button.IsDanger = true;
            return this;
        }

        public SidebarCommand Tooltip(string text)
        {
            _tooltip = (b) => b.Tooltip(text, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        public SidebarCommand Tooltip(IComponent tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        public SidebarCommand Tooltip(Func<IComponent> tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Top);
            RefreshTooltip();
            return this;
        }

        public SidebarCommand OnClickMenu(Func<ISidebarItem[]> generator)
        {
            _menuGenerator = generator;
            _button.OnClick(() => ShowMenu());
            return this;
        }

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

        public SidebarCommand OnClick(Action action)
        {
            _button.OnClick(action);
            return this;
        }

        public SidebarCommand OnContextMenu(Action action)
        {
            _button.OnContextMenu(action);
            return this;
        }

        public SidebarCommand OnClick(Action<Button, MouseEvent> action)
        {
            _button.OnClick((b, e) => action(b, e));
            return this;
        }

        public SidebarCommand OnContextMenu(Action<Button, MouseEvent> action)
        {
            _button.OnContextMenu((b, e) => action(b, e));
            return this;
        }

        public SidebarCommand SetIcon(string icon, string color = "")
        {
            _button.SetIcon(icon, color);
            return this;
        }

        public SidebarCommand SetIcon(LineAwesome icon, string color = "", LineAwesomeWeight weight = LineAwesomeWeight.Light)
        {
            _button.SetIcon(icon, color, weight: weight);
            return this;
        }

        public SidebarCommand SetIcon(Emoji icon)
        {
            _button.SetIcon(icon);
            return this;
        }

        public SidebarCommand SetBadge(string badge)
        {
            if (!_badge)
            {
                throw new Exception("Only supported for badges");
            }
            _button.SetText(badge);
            return this;
        }

        internal void RefreshTooltip() => _tooltip?.Invoke(_button);

        public HTMLElement Render() => _button.Render();
    }
}