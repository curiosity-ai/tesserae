using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
    [H5.Name("tss.Sidebar")]
    public sealed class Sidebar : IComponent
    {
        private readonly ObservableList<ISidebarItem> _header;
        private readonly ObservableList<ISidebarItem> _middle;
        private readonly ObservableList<ISidebarItem> _footer;
        private readonly SettableObservable<bool> _closed;
        private readonly Stack _sidebar;

        public bool IsClosed {  get { return _closed.Value; } set { _closed.Value = value; } }

        public Sidebar()
        {
            _header = new ObservableList<ISidebarItem>();
            _middle = new ObservableList<ISidebarItem>();
            _footer = new ObservableList<ISidebarItem>();
            _closed = new SettableObservable<bool>(false);
            _sidebar = VStack().Class("tss-sidebar");

            _closed.Observe(isClosed =>
            {
                if (isClosed)
                {
                    _sidebar.Class("tss-sidebar-closed");
                }
                else
                {
                    _sidebar.RemoveClass("tss-sidebar-closed");
                }
            });

            var combined = new CombinedObservable<IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, bool>(_header, _middle, _footer, _closed);

            combined.ObserveFutureChanges(content => RenderSidebar(content.first, content.second, content.third, content.forth));
        }

        private void RenderSidebar(IReadOnlyList<ISidebarItem> header, IReadOnlyList<ISidebarItem> middle, IReadOnlyList<ISidebarItem> footer, bool closed)
        {
            _sidebar.Children(VStack().Class("tss-sidebar-header").WS().NoShrink().Children(header.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                              VStack().Class("tss-sidebar-middle").WS().H(10).Grow().ScrollY().Children(middle.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                              VStack().Class("tss-sidebar-footer").WS().NoShrink().Children(footer.Select(si => closed ? si.RenderClosed() : si.RenderOpen()))
                );
        }

        public Sidebar Closed(bool isClosed = true)
        {
            _closed.Value = isClosed;
            return this;
        }

        public Sidebar Toggle()
        {
            _closed.Value = !_closed.Value;
            return this;
        }

        public Sidebar AddHeader(ISidebarItem item)  { _header.Add(item); return this; }
        public Sidebar AddContent(ISidebarItem item) { _middle.Add(item); return this; }
        public Sidebar AddFooter(ISidebarItem item)  { _footer.Add(item); return this; }

        public void Clear() { ClearHeader(); ClearContent(); ClearFooter(); }
        public void ClearHeader() => _header.Clear();
        public void ClearContent() => _middle.Clear();
        public void ClearFooter() => _footer.Clear();

        public HTMLElement Render() => _sidebar.Render();
    }

    public interface ISidebarItem 
    {
        IComponent RenderClosed();
        IComponent RenderOpen();
        bool IsSelected { get; set; }
        IComponent CurrentRendered { get; }
    }

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


    public class SidebarCommands : ISidebarItem
    {
        private readonly SidebarCommand[] _commands;

        public SidebarCommands(params SidebarCommand[] commands)
        {
            _commands = commands;
        }

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered { get; private set; }

        public IComponent RenderOpen()
        {
            var div = Div(_("tss-sidebar-commands-line"));
            var divWrapped = Raw(div);

            DomObserver.WhenMounted(div, () =>
            {
                _commands[0].RefreshTooltip();
                div.appendChild(_commands[0].Render());

                window.setTimeout((__) =>
                {
                    HTMLDivElement stack = null;
                    var rect = div.getBoundingClientRect().As<DOMRect>();
                    int max = (int)Math.Floor(rect.width / (48));

                    for (int i = 1; i < _commands.Length; i++)
                    {
                        var command = _commands[i];
                        command.RefreshTooltip();
                        if (i < max)
                        {
                            div.appendChild(command.Render());
                        }
                        else
                        {
                            if (stack is null) stack = Div(_("tss-sidebar-commands-line-extra"));
                            stack.appendChild(command.Render());
                        }
                    }

                    if (stack is object)
                    {
                        divWrapped.Tooltip(Raw(stack), true, placement: TooltipPlacement.Right, delayHide: 500);
                    }
                }, 400); //Need to be after the animation
            });
            CurrentRendered = divWrapped;
            return divWrapped;
        }

        public IComponent RenderClosed()
        {
            var div = Div(_("tss-sidebar-commands-line tss-sidebar-commands-line-closed"));
            var divWrapped = Raw(div);

            DomObserver.WhenMounted(div, () =>
            {
                HTMLDivElement stack = null;
                int max = 1;

                for (int i = 0; i < _commands.Length; i++)
                {
                    var command = _commands[i];
                    command.RefreshTooltip();
                    if (i < max)
                    {
                        div.appendChild(command.Render());
                    }
                    else
                    {
                        if (stack is null) stack = Div(_("tss-sidebar-commands-line-extra"));
                        stack.appendChild(command.Render());
                    }
                }

                if (stack is object)
                {
                    divWrapped.Tooltip(Raw(stack), true, placement: TooltipPlacement.Right, delayHide: 500);
                }
            });

            CurrentRendered = divWrapped;
            return divWrapped;
        }
    }

    public class SidebarCommand : IComponent
    {
        private readonly Button _button;
        private Action<Button> _tooltip;

        public SidebarCommand(LineAwesome icon, LineAwesomeWeight weight = LineAwesomeWeight.Light) : this($"{weight} {icon}") {}
        public SidebarCommand(Emoji icon) : this($"ec {icon}") {}

        public SidebarCommand(ImageIcon image)
        {
            _button = Button().ReplaceContent(image).Class("tss-sidebar-command");
        }

        public SidebarCommand(string icon)
        {
            _button = Button().SetIcon(icon).Class("tss-sidebar-command");
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
            return this;
        }

        public SidebarCommand Tooltip(IComponent tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip, placement: TooltipPlacement.Top);
            return this;
        }

        public SidebarCommand Tooltip(Func<IComponent> tooltip)
        {
            _tooltip = (b) => b.Tooltip(tooltip(), placement: TooltipPlacement.Top);
            return this;
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

        internal void RefreshTooltip() => _tooltip?.Invoke(_button);

        public HTMLElement Render() => _button.Render();
    }


    public class SidebarNav : ISidebarItem
    {
        private readonly string _text;
        private readonly Button _closedHeader;
        private readonly HTMLElement _openHeader;
        private readonly Button _arrow;
        private readonly Button _openHeaderButton;
        private readonly ObservableList<ISidebarItem> _items;
        private readonly SettableObservable<bool> _collapsed;
        private readonly SettableObservable<bool> _selected;
        private readonly Func<IComponent> _closedContent;
        private readonly Func<IComponent> _openContent;
        private SidebarCommand[] _commands;

        public bool IsCollapsed { get { return _collapsed.Value; } set { _collapsed.Value = value; } }
        public bool IsSelected  { get { return _selected.Value; }  set { _selected.Value = value; } }

        private IComponent _lastClosed;
        private IComponent _lastOpen;

        public IComponent CurrentRendered => (_lastClosed is object && _lastClosed.IsMounted()) ? _lastClosed : _lastOpen;

        public SidebarNav(Emoji icon, string text, bool initiallyCollapsed, params SidebarCommand[] commands) : this($"ec {icon}", text, initiallyCollapsed, commands) { }
        public SidebarNav(LineAwesome icon, string text, bool initiallyCollapsed, params SidebarCommand[] commands) : this($"{LineAwesomeWeight.Light} {icon}", text, initiallyCollapsed, commands) { }
        public SidebarNav(LineAwesome icon, LineAwesomeWeight weight, string text, bool initiallyCollapsed, params SidebarCommand[] commands) : this($"{weight} {icon}", text, initiallyCollapsed, commands) { }

        public SidebarNav(string icon, string text, bool initiallyCollapsed, params SidebarCommand[] commands)
        {
            _text = text;
            _closedHeader = Button().SetIcon(icon).Class("tss-sidebar-nav-header").Class("tss-sidebar-btn");
            _openHeader = Div(_("tss-sidebar-nav-header tss-sidebar-btn-open"));

            _arrow     = Button().Class("tss-sidebar-nav-arrow").Fade();

            _openHeaderButton = Button(text).SetIcon(icon).Class("tss-sidebar-nav-button");
            _openHeader.appendChild(_openHeaderButton.Render());
            _openHeader.appendChild(_arrow.Render());

            _commands = commands;

            if (commands.Length > 0)
            {
                var divCmd = Div(_("tss-sidebar-commands"));
                _openHeader.appendChild(divCmd);
                foreach (var c in commands)
                {
                    divCmd.appendChild(c.Render());
                }
            }

            _items = new ObservableList<ISidebarItem>();
            _collapsed = new SettableObservable<bool>(initiallyCollapsed);
            _selected  = new SettableObservable<bool>(false);

            _closedContent = () => Defer(_items, (items) => RenderClosed(items).AsTask());
            _openContent   = () => Defer(_items, (items) => RenderOpened(items).AsTask());

            _arrow.OnClick(() =>
            {
                _collapsed.Value = !_collapsed.Value;
            });
        }

        public SidebarNav Collapsed(bool isCollapsed = true)
        {
            _collapsed.Value = isCollapsed;
            return this;
        }

        public SidebarNav Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return this;
        }

        public SidebarNav Toggle()
        {
            _collapsed.Value = !_collapsed.Value;
            return this;
        }

        public SidebarNav OnClick(Action action)
        {
            _closedHeader.OnClick(action);
            _openHeaderButton.OnClick(action);
            return this;
        }

        public SidebarNav OnClick(Action<SidebarNav> action)
        {
            _closedHeader.OnClick(() => action(this));
            _openHeaderButton.OnClick(() => action(this));
            return this;
        }

        public SidebarNav OnContextMenu(Action action)
        {
            _closedHeader.OnContextMenu(action);
            _openHeaderButton.OnContextMenu(action);
            return this;
        }

        public SidebarNav OnClick(Action<Button, MouseEvent> action)
        {
            _closedHeader.OnClick((b, e) => action(b, e));
            _openHeaderButton.OnClick((b, e) => action(b, e));
            return this;
        }

        public SidebarNav OnContextMenu(Action<Button, MouseEvent> action)
        {
            _closedHeader.OnContextMenu((b, e) => action(b, e));
            _openHeaderButton.OnContextMenu((b, e) => action(b, e));
            return this;
        }

        private IComponent RenderOpened(IReadOnlyList<ISidebarItem> items)
        {
            if (items.Count > 0)
            {
                _arrow.Show();
            }
            else
            {
                _arrow.Fade();
            }


            foreach (var c in _commands) c.RefreshTooltip();

            var nav = Div(_("tss-sidebar-nav"));
            nav.appendChild(_openHeader);
            nav.appendChild(VStack().Class("tss-sidebar-nav-children").Children(items.Select(i => i.RenderOpen())).Render());

            CollapsedChanged(_collapsed.Value);
            SelectedChanged(_selected.Value);

            DomObserver.WhenMounted(nav ,() =>
            {
                _collapsed.Observe(CollapsedChanged);
                _selected.Observe(SelectedChanged);
                DomObserver.WhenRemoved(nav, () => { _collapsed.StopObserving(CollapsedChanged); _selected.StopObserving(SelectedChanged); });
            });

            var comp = Raw(nav);
            _lastOpen= comp;
            return comp;

            void CollapsedChanged(bool isCollapsed)
            {
                if (isCollapsed)
                {
                    nav.classList.remove("tss-sidebar-nav-open");
                    _arrow.Tooltip("Expand".t(), placement: TooltipPlacement.Top);
                }
                else
                {
                    nav.classList.add("tss-sidebar-nav-open");
                    _arrow.Tooltip("Collapse".t(), placement: TooltipPlacement.Top);
                }
            }

            void SelectedChanged(bool isSelected)
            {
                if (isSelected)
                {
                    nav.classList.add("tss-sidebar-selected");
                }
                else
                {
                    nav.classList.remove("tss-sidebar-selected");
                }
            }
        }

        private IComponent RenderClosed(IReadOnlyList<ISidebarItem> items)
        {
            _closedHeader.Tooltip(_text, placement: TooltipPlacement.Top);

            var nav = Div(_("tss-sidebar-nav"));
            nav.appendChild(_closedHeader.Render());
            nav.appendChild(VStack().Class("tss-sidebar-nav-children").Children(items.Select(i => i.RenderClosed())).Render());

            CollapsedChanged(_collapsed.Value);
            SelectedChanged(_selected.Value);

            DomObserver.WhenMounted(nav, () =>
            {
                _collapsed.Observe(CollapsedChanged);
                _selected.Observe(SelectedChanged);
                DomObserver.WhenRemoved(nav, () => { _collapsed.StopObserving(CollapsedChanged); _selected.StopObserving(SelectedChanged); });
            });

            var comp = Raw(nav);
            _lastClosed = comp;
            return comp;

            void CollapsedChanged(bool isCollapsed)
            {
                if (isCollapsed)
                {
                    nav.classList.remove("tss-sidebar-nav-open");
                    _arrow.Tooltip("Expand".t(), placement: TooltipPlacement.Top);
                }
                else
                {
                    nav.classList.add("tss-sidebar-nav-open");
                    _arrow.Tooltip("Collapse".t(), placement: TooltipPlacement.Top);
                }
            }

            void SelectedChanged(bool isSelected)
            {
                if (isSelected)
                {
                    nav.classList.add("tss-sidebar-selected");
                }
                else
                {
                    nav.classList.remove("tss-sidebar-selected");
                }
            }
        }


        public void Clear()
        {
            _items.Clear();
        }

        public void Add(ISidebarItem item)
        {
            _items.Add(item);
        }

        public IComponent RenderClosed() => _closedContent();

        public IComponent RenderOpen() => _openContent();

    }
}