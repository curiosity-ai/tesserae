using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarButton : ISidebarItem
    {
        private readonly Button                   _closedButton;
        private readonly Button                   _openButton;
        private readonly IComponent               _open;
        private readonly SidebarCommand[]         _commands;
        private          Action<IComponent>       _tooltipClosed;
        private readonly ISidebarIcon             _image;
        private          Action<IComponent>       _tooltipOpen;
        private readonly SettableObservable<bool> _selected;

        private event Action<HTMLElement> _onRendered;

        public bool IsSelected { get { return _selected.Value; } set { _selected.Value = value; } }

        public IComponent CurrentRendered => _closedButton.IsMounted() ? _closedButton : _open;

        public SidebarButton(LineAwesome icon, string text, params SidebarCommand[] commands) : this($"{LineAwesomeWeight.Light} {icon}", text, commands) { }

        public SidebarButton(LineAwesome icon, LineAwesomeWeight weight, string text, params SidebarCommand[] commands) : this($"{weight} {icon}", text, commands) { }

        public SidebarButton(Emoji icon, string text, params SidebarCommand[] commands) : this($"ec {icon}", text, commands) { }

        public SidebarButton(string icon, string text, params SidebarCommand[] commands)
        {
            _selected = new SettableObservable<bool>(false);
            _tooltipClosed = (b) => b.Tooltip(text);
            _closedButton = Button().Class("tss-sidebar-btn").SetIcon(icon);

            _openButton = Button(text).SetIcon(icon).Class("tss-sidebar-btn");

            _commands = commands;

            _open = Wrap(_openButton);


            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closedButton.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closedButton.RemoveClass("tss-sidebar-selected");
                    _open.RemoveClass("tss-sidebar-selected");
                }
            });
        }

        private IComponent Wrap(Button button)
        {
            var div = Div(_("tss-sidebar-btn-open"));
            div.appendChild(button.Render());

            if (_commands is object && _commands.Length > 0)
            {
                var divCmd = Div(_("tss-sidebar-commands"));
                div.appendChild(divCmd);

                foreach (var c in _commands)
                {
                    divCmd.appendChild(c.Render());
                }
            }
            return Raw(div);
        }

        public SidebarButton(ISidebarIcon image, string text, params SidebarCommand[] commands)
        {
            _selected = new SettableObservable<bool>(false);

            _tooltipClosed = (b) => b.Tooltip(text);

            _image = image;

            _closedButton = Button().Class("tss-sidebar-btn").ReplaceContent(image);

            _openButton = Button(text).ReplaceContent(Raw(Div(_("tss-btn-with-image"), image.Clone().Render(), Span(_(text: text))))).Class("tss-sidebar-btn");
            _open = Wrap(_openButton);

            _commands = commands;

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closedButton.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closedButton.RemoveClass("tss-sidebar-selected");
                    _open.RemoveClass("tss-sidebar-selected");
                }
            });

            IComponent Wrap(Button button)
            {
                var div = Div(_("tss-sidebar-btn-open"));
                div.appendChild(button.Render());

                if (commands.Length > 0)
                {
                    var divCmd = Div(_("tss-sidebar-commands"));
                    div.appendChild(divCmd);

                    foreach (var c in commands)
                    {
                        divCmd.appendChild(c.Render());
                    }
                }
                return Raw(div);
            }
        }

        public SidebarButton ClearProgress()
        {
            _openButton.Render().style.background = "";
            _closedButton.Render().style.background = "";
            return this;
        }
        public SidebarButton Progress(float progress)
        {
            var p = $"linear-gradient(to right, rgba(var(--tss-primary-background-color-root),0.2), rgba(var(--tss-primary-background-color-root),0.2) {progress * 100:0.0}%, transparent 0)";
            _openButton.Render().style.background = p;
            _closedButton.Render().style.background = p;
            return this;
        }


        public SidebarButton SetText(string text)
        {
            if (_image is object)
            {
                _openButton.ReplaceContent(Raw(Div(_("tss-btn-with-image"), _image.Clone().Render(), Span(_(text: text)))));
            }
            else
            {
                _openButton.SetText(text);
            }
            return this;
        }

        public SidebarButton CommandsAlwaysVisible()
        {
            _open.Class("tss-sidebar-commands-always-open");
            return this;
        }

        public SidebarButton Light()
        {
            _open.Class("tss-sidebar-btn-light");
            _closedButton.Class("tss-sidebar-btn-light");
            return this;
        }

        public SidebarButton Danger()
        {
            _openButton.Danger();
            _closedButton.Danger();
            return this;
        }

        public SidebarButton Default()
        {
            _openButton.IsPrimary = false;
            _closedButton.IsPrimary = false;
            return this;
        }

        public SidebarButton Success()
        {
            _openButton.Success();
            _closedButton.Success();
            return this;
        }

        public SidebarButton Primary()
        {
            _openButton.Primary();
            _closedButton.Primary();
            return this;
        }

        public SidebarButton Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return this;
        }

        public SidebarButton Tooltip(string text)
        {
            _tooltipClosed = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return this;
        }

        public SidebarButton Tooltip(IComponent tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return this;
        }

        public SidebarButton Tooltip(Func<IComponent> tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Right);
            _tooltipClosed(_closedButton);
            return this;
        }

        public SidebarButton OpenedTooltip(string text)
        {
            _tooltipOpen = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipOpen(_openButton);
            return this;
        }

        public SidebarButton OpenedTooltip(IComponent tooltip)
        {
            _tooltipOpen = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            _tooltipOpen(_openButton);
            return this;
        }

        public SidebarButton OpenedTooltip(Func<IComponent> tooltip)
        {
            _tooltipOpen = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Right);
            _tooltipOpen(_openButton);
            return this;
        }

        public SidebarButton OnClick(Action action)
        {
            _closedButton.OnClick(action);
            _openButton.OnClick(action);
            return this;
        }

        public SidebarButton OnOpenIconClick(Action<HTMLElement, MouseEvent> action)
        {
            _openButton.OnIconClick(action);
            _openButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        public SidebarButton Id(string id)
        {
            _open.Id(id);
            _closedButton.Id(id);
            return this;

        }

        public SidebarButton OnOpenIconClick(Action action)
        {
            _openButton.OnIconClick((_, __) => action());
            _openButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        public SidebarButton OnContextMenu(Action action)
        {
            _closedButton.OnContextMenu(action);
            _openButton.OnContextMenu(action);
            return this;
        }

        public SidebarButton OnClick(Action<Button, MouseEvent> action)
        {
            _closedButton.OnClick((b, e) => action(b, e));
            _openButton.OnClick((b,   e) => action(b, e));
            return this;
        }

        public SidebarButton OnContextMenu(Action<Button, MouseEvent> action)
        {
            _closedButton.OnContextMenu((b, e) => action(b, e));
            _openButton.OnContextMenu((b,   e) => action(b, e));
            return this;
        }

        public SidebarButton SetIcon(string icon, string color = "")
        {
            _closedButton.SetIcon(icon, color);
            _openButton.SetIcon(icon, color);
            return this;
        }

        public SidebarButton SetIcon(LineAwesome icon, string color = "", LineAwesomeWeight weight = LineAwesomeWeight.Light)
        {
            _closedButton.SetIcon(icon, color, weight: weight);
            _openButton.SetIcon(icon, color, weight: weight);
            return this;
        }

        public SidebarButton SetIcon(Emoji icon)
        {
            _closedButton.SetIcon(icon);
            _openButton.SetIcon(icon);
            return this;
        }

        public SidebarButton Collapse()
        {
            _closedButton.Collapse();
            _open.Collapse();
            return this;
        }

        public SidebarButton Show()
        {
            _closedButton.Show();
            _open.Show();
            return this;
        }


        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }
        public IComponent RenderClosed()
        {
            _tooltipClosed?.Invoke(_closedButton);
            _onRendered?.Invoke(_closedButton.Render());
            return _closedButton;
        }

        public IComponent RenderOpen()
        {
            foreach (var c in _commands) c.RefreshTooltip();
            _tooltipOpen?.Invoke(_openButton);
            _onRendered?.Invoke(_open.Render());
            return _open;
        }
    }
}