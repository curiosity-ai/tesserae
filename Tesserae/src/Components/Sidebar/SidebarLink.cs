using System;
using System.Linq;
using H5.Core;
using TNT;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An anchor-based sidebar item for navigation scenarios.
    /// Unlike <see cref="SidebarButton"/>, this component relies on native link behavior.
    /// </summary>
    public class SidebarLink : ISearchableSidebarItem
    {
        private readonly Link                     _closedLink;
        private readonly Link                     _openLink;
        private readonly IComponent               _open;
        private readonly SidebarCommand[]         _commands;
        private readonly SidebarBadge             _badge;
        private readonly SettableObservable<bool> _selected;
        private readonly string                   _text;

        private Action<IComponent> _tooltipClosed;
        private Action<IComponent> _tooltipOpen;

        private event Action<HTMLElement> _onRendered;

        /// <summary>Gets or sets whether the link is currently selected.</summary>
        public bool IsSelected { get { return _selected.Value; } set { _selected.Value = value; if (value) CurrentRendered.ScrollIntoView(); } }

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _closedLink.IsMounted() ? (IComponent)_closedLink : _open;

        /// <summary>Gets the full identifier of the link.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets the own identifier of the link.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the link's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }

        public SidebarLink(string identifier, Emoji icon, string text, string url, params SidebarCommand[] commands)
            : this(identifier, text, url, null, Icon(icon), Icon(icon), commands) { }

        public SidebarLink(string identifier, Emoji icon, string text, string url, SidebarBadge badge, params SidebarCommand[] commands)
            : this(identifier, text, url, badge, Icon(icon), Icon(icon), commands) { }

        public SidebarLink(string identifier, UIcons icon, string text, string url, params SidebarCommand[] commands)
            : this(identifier, icon, UIconsWeight.Regular, text, url, null, commands) { }

        public SidebarLink(string identifier, UIcons icon, string text, string url, SidebarBadge badge, params SidebarCommand[] commands)
            : this(identifier, icon, UIconsWeight.Regular, text, url, badge, commands) { }

        public SidebarLink(string identifier, UIcons icon, UIconsWeight weight, string text, string url, params SidebarCommand[] commands)
            : this(identifier, icon, weight, text, url, null, commands) { }

        public SidebarLink(string identifier, UIcons icon, UIconsWeight weight, string text, string url, SidebarBadge badge, params SidebarCommand[] commands)
            : this(identifier, text, url, badge, Icon(icon, weight: weight), Icon(icon, weight: weight), commands) { }

        public SidebarLink(string identifier, ISidebarIcon image, string text, string url, params SidebarCommand[] commands)
            : this(identifier, image, text, url, null, commands) { }

        public SidebarLink(string identifier, ISidebarIcon image, string text, string url, SidebarBadge badge, params SidebarCommand[] commands)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Navigation URL cannot be null or whitespace.", nameof(url));
            }

            Identifier     = identifier;
            _text          = text;
            _selected      = new SettableObservable<bool>(false);
            _tooltipClosed = (c) => c.Tooltip(text, placement: TooltipPlacement.Right);

            var closedContent = Raw(image.Clone().Render());
            var openContent = Raw(Div(_("tss-btn-with-image"), image.Clone().Render(), Span(_(text: text))));

            _closedLink = CreateStyledSidebarLink(url, closedContent, identifier);
            _openLink   = CreateStyledSidebarLink(url, openContent, identifier);

            _commands = commands;
            _badge    = badge;
            _open     = Wrap(_openLink);

            var hookContextMenu = _commands.FirstOrDefault(c => c.ShouldHookToContextMenu);
            if (hookContextMenu is object)
            {
                OnContextMenu((_, e) => hookContextMenu.RaiseOnClick(e));
            }

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closedLink.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closedLink.RemoveClass("tss-sidebar-selected");
                    _open.RemoveClass("tss-sidebar-selected");
                }
            });
        }

        private SidebarLink(string identifier, string text, string url, SidebarBadge badge, IComponent closedIcon, IComponent openIcon, params SidebarCommand[] commands)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Navigation URL cannot be null or whitespace.", nameof(url));
            }

            Identifier     = identifier;
            _text          = text;
            _selected      = new SettableObservable<bool>(false);
            _tooltipClosed = (c) => c.Tooltip(text, placement: TooltipPlacement.Right);

            _closedLink = CreateStyledSidebarLink(url, closedIcon, identifier);
            _openLink   = CreateStyledSidebarLink(url, Raw(Div(_("tss-btn-with-image"), openIcon.Render(), Span(_(text: text)))), identifier);

            _commands = commands;
            _badge    = badge;
            _open     = Wrap(_openLink);

            var hookContextMenu = _commands.FirstOrDefault(c => c.ShouldHookToContextMenu);
            if (hookContextMenu is object)
            {
                OnContextMenu((_, e) => hookContextMenu.RaiseOnClick(e));
            }

            _selected.Observe(isSelected =>
            {
                if (isSelected)
                {
                    _closedLink.Class("tss-sidebar-selected");
                    _open.Class("tss-sidebar-selected");
                }
                else
                {
                    _closedLink.RemoveClass("tss-sidebar-selected");
                    _open.RemoveClass("tss-sidebar-selected");
                }
            });
        }

        private static Link CreateStyledSidebarLink(string url, IComponent content, string identifier)
        {
            return Link(url, content, noUnderline: true)
               .Class("tss-sidebar-btn")
               .Class("tss-btn")
               .Class("tss-btn-default")
               .Class("tss-default-component-margin")
               .Id(identifier);
        }

        private IComponent Wrap(Link link)
        {
            var div = Div(_("tss-sidebar-btn-open"));
            div.appendChild(link.Render());

            if (_badge is object)
            {
                var divBadges = Div(_("tss-sidebar-badges"));
                div.appendChild(divBadges);
                divBadges.appendChild(_badge.Render());
            }

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

        public void Show()
        {
            _closedLink.Show();
            _openLink.Show();
        }

        public void Collapse()
        {
            _closedLink.Collapse();
            _openLink.Collapse();
        }

        public SidebarLink NotSortable()
        {
            _closedLink.Class("tss-sortable-disable");
            _open.Class("tss-sortable-disable");
            return this;
        }

        public SidebarLink Selected(bool isSelected = true)
        {
            IsSelected = isSelected;
            return this;
        }

        public SidebarLink Tooltip(string text)
        {
            _tooltipClosed = (c) => c.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipClosed(_closedLink);
            return this;
        }

        public SidebarLink OpenedTooltip(string text)
        {
            _tooltipOpen = (c) => c.Tooltip(text, placement: TooltipPlacement.Right);
            _tooltipOpen(_openLink);
            return this;
        }

        public SidebarLink CommandsAlwaysVisible()
        {
            _open.Class("tss-sidebar-commands-always-open");
            return this;
        }

        public SidebarLink OnClick(Action action)
        {
            _closedLink.OnClick(action);
            _openLink.OnClick(action);
            return this;
        }

        public SidebarLink OnContextMenu(Action<Link, MouseEvent> action)
        {
            _closedLink.Render().addEventListener("contextmenu", ev =>
            {
                var e = ev.As<MouseEvent>();
                StopEvent(e);
                action(_closedLink, e);
            });

            _openLink.Render().addEventListener("contextmenu", ev =>
            {
                var e = ev.As<MouseEvent>();
                StopEvent(e);
                action(_openLink, e);
            });
            return this;
        }

        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }

        public IComponent RenderClosed()
        {
            _onRendered?.Invoke(_closedLink.Render());
            _closedLink.RemoveTooltip();

            DomObserver.WhenMounted(_closedLink.Render(), () =>
            {
                window.setTimeout(_ => { _tooltipClosed?.Invoke(_closedLink); }, Sidebar.SIDEBAR_TRANSITION_TIME);
            });

            return _closedLink;
        }

        public IComponent RenderOpen()
        {
            foreach (var c in _commands) c.RefreshTooltip();
            _tooltipOpen?.Invoke(_openLink);
            _onRendered?.Invoke(_open.Render());
            return _open;
        }

        public bool Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Show();
                return true;
            }

            if (_text.ToLower().Contains(searchTerm.ToLower()))
            {
                Show();
                return true;
            }

            Collapse();
            return false;
        }
    }
}
