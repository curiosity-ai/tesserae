using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
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
            _openHeader = Div(_("tss-sidebar-nav-header tss-sidebar-btn-open tss-sidebar-nav-header-empty"));

            _arrow     = Button().Class("tss-sidebar-nav-arrow");

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

        public SidebarNav ShowDotIfEmpty()
        {
            _openHeader.classList.add("tss-sidebar-nav-header-dot-if-empty");
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
                _openHeader.classList.remove("tss-sidebar-nav-header-empty");
            }
            else
            {
                _openHeader.classList.add("tss-sidebar-nav-header-empty");
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