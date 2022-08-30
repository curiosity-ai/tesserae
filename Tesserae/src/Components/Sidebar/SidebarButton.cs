using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarButton : ISidebarItem
    {
        private readonly Button _closed;
        private readonly Button _openButton;
        private readonly IComponent _open;
        private readonly SidebarCommand[] _commands;
        private Action<IComponent> _tooltipClosed;
        private readonly ImageIcon _image;
        private Action<IComponent> _tooltipOpen;
        private readonly SettableObservable<bool> _selected;

        public bool IsSelected { get { return _selected.Value; } set { _selected.Value = value; } }

        public IComponent CurrentRendered => _closed.IsMounted() ? _closed : _open;

        public SidebarButton(LineAwesome icon, string text, params SidebarCommand[] commands) : this($"{LineAwesomeWeight.Light} {icon}", text, commands) { }

        public SidebarButton(LineAwesome icon, LineAwesomeWeight weight, string text, params SidebarCommand[] commands) : this($"{weight} {icon}", text, commands) { }

        public SidebarButton(Emoji icon, string text, params SidebarCommand[] commands) : this($"ec {icon}", text, commands) { }

        public SidebarButton(string icon, string text, params SidebarCommand[] commands)
        {
            _selected = new SettableObservable<bool>(false);
            _tooltipClosed = (b) => b.Tooltip(text);
            _closed = Button().Class("tss-sidebar-btn").SetIcon(icon);
            
            _openButton = Button(text).SetIcon(icon).Class("tss-sidebar-btn");
            
            _commands = commands;

            _open   = Wrap(_openButton);
            

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closed.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closed.RemoveClass("tss-sidebar-selected");
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

        public SidebarButton(ImageIcon image, string text, params SidebarCommand[] commands)
        {
            _selected = new SettableObservable<bool>(false);
            
            _tooltipClosed = (b) => b.Tooltip(text);

            _image = image;

            _closed = Button().Class("tss-sidebar-btn").ReplaceContent(image);

            _openButton = Button(text).ReplaceContent(Raw(Div(_("tss-btn-with-image"), image.Clone().Render(), Span(_(text: text))))).Class("tss-sidebar-btn");
            _open = Wrap(_openButton);

            _commands = commands;

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closed.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closed.RemoveClass("tss-sidebar-selected");
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
            _closed.Class("tss-sidebar-btn-light");
            return this;
        }

        public SidebarButton Danger()
        {
            _openButton.Danger();
            _closed.Danger();
            return this;
        }

        public SidebarButton Success()
        {
            _openButton.Success();
            _closed.Success();
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
            return this;
        }

        public SidebarButton Tooltip(IComponent tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            return this;
        }

        public SidebarButton Tooltip(Func<IComponent> tooltip)
        {
            _tooltipClosed = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Right);
            return this;
        }

        public SidebarButton OpenedTooltip(string text)
        {
            _tooltipOpen = (b) => b.Tooltip(text, placement: TooltipPlacement.Right);
            return this;
        }

        public SidebarButton OpenedTooltip(IComponent tooltip)
        {
            _tooltipOpen = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Right);
            return this;
        }

        public SidebarButton OpenedTooltip(Func<IComponent> tooltip)
        {
            _tooltipOpen = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Right);
            return this;
        }

        public SidebarButton OnClick(Action action)
        {
            _closed.OnClick(action);
            _openButton.OnClick(action);
            return this;
        }

        public SidebarButton OnContextMenu(Action action)
        {
            _closed.OnContextMenu(action);
            _openButton.OnContextMenu(action);
            return this;
        }

        public SidebarButton OnClick(Action<Button, MouseEvent> action)
        {
            _closed.OnClick((b, e) => action(b, e));
            _openButton.OnClick((b, e) => action(b, e));
            return this;
        }

        public SidebarButton OnContextMenu(Action<Button, MouseEvent> action)
        {
            _closed.OnContextMenu((b, e) => action(b, e));
            _openButton.OnContextMenu((b, e) => action(b, e));
            return this;
        }

        public SidebarButton SetIcon(string icon, string color = "")
        {
            _closed.SetIcon(icon, color);
            _openButton.SetIcon(icon, color);
            return this;
        }

        public SidebarButton SetIcon(LineAwesome icon, string color = "", LineAwesomeWeight weight = LineAwesomeWeight.Light)
        {
            _closed.SetIcon(icon, color, weight: weight);
            _openButton.SetIcon(icon, color, weight: weight);
            return this;
        }

        public SidebarButton SetIcon(Emoji icon)
        {
            _closed.SetIcon(icon);
            _openButton.SetIcon(icon);
            return this;
        }


        public IComponent RenderClosed()
        {
            _tooltipClosed?.Invoke(_closed);
            return _closed;
        }

        public IComponent RenderOpen()
        {
            foreach (var c in _commands) c.RefreshTooltip();
            _tooltipOpen?.Invoke(_openButton);
            return _open;
        }
    }
}