using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SidebarSegmentedPivot")]
    public sealed class SidebarSegmentedPivot : ISearchableSidebarItem
    {
        private readonly List<Tab> _orderedTabs = new List<Tab>();

        private readonly HTMLElement _renderedTabsOpen;
        private readonly HTMLElement _renderedTabsClosed;
        private readonly HTMLElement _renderedContentOpen;
        private readonly HTMLElement _renderedContentClosed;

        private readonly HTMLElement _openContainer;
        private readonly HTMLElement _closedContainer;

        private string _initiallySelectedID;
        private string _currentSelectedID;

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered => _openContainer.IsMounted() ? Raw(_openContainer) : Raw(_closedContainer);

        public string Identifier { get; private set; }

        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        public SidebarSegmentedPivot(string identifier)
        {
            Identifier = identifier;

            _renderedTabsOpen = Div(_("tss-segmentedpivot-titlebar tss-sidebar-segmentedpivot-open", role: "tablist"));
            _renderedContentOpen = Div(_("tss-segmentedpivot-content", role: "tabpanel"));

            _renderedTabsClosed = Div(_("tss-segmentedpivot-titlebar tss-sidebar-segmentedpivot-closed", role: "tablist"));
            _renderedContentClosed = Div(_("tss-segmentedpivot-content", role: "tabpanel"));

            var wrapperOpen = Div(_("tss-segmentedpivot-wrapper tss-sidebar-segmentedpivot-wrapper-open"), _renderedTabsOpen);
            _openContainer = Div(_("tss-segmentedpivot"), wrapperOpen, _renderedContentOpen);

            var wrapperClosed = Div(_("tss-segmentedpivot-wrapper tss-sidebar-segmentedpivot-wrapper-closed"), _renderedTabsClosed);
            _closedContainer = Div(_("tss-segmentedpivot"), wrapperClosed, _renderedContentClosed);
        }

        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
            foreach (var tab in _orderedTabs)
            {
                foreach(var item in tab.Items)
                {
                    item.AddGroupIdentifier(Identifier);
                }
            }
        }

        public SidebarSegmentedPivot Add(string id, Func<IComponent> titleCreator, params ISidebarItem[] items)
        {
            var tab = new Tab(id, titleCreator, items);
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);

            foreach (var item in items)
            {
                item.AddGroupIdentifier(Identifier);
            }

            // Open tab title
            var titleContainerOpen = Div(_("tss-segmentedpivot-tab"));
            titleContainerOpen.tabIndex = 0;
            AttachEvents(titleContainerOpen, tab.Id);
            titleContainerOpen.appendChild(tab.CreateTitle().Render());
            _renderedTabsOpen.appendChild(titleContainerOpen);

            // Closed tab title
            var titleContainerClosed = Div(_("tss-segmentedpivot-tab"));
            titleContainerClosed.tabIndex = 0;
            AttachEvents(titleContainerClosed, tab.Id);

            titleContainerClosed.appendChild(tab.CreateTitle().Render());

            _renderedTabsClosed.appendChild(titleContainerClosed);

            tab.OpenTitle = titleContainerOpen;
            tab.ClosedTitle = titleContainerClosed;

            if (_currentSelectedID is null && _initiallySelectedID == tab.Id)
            {
                Select(tab.Id);
            }
            else if (_currentSelectedID is object)
            {
                Select(_currentSelectedID, refresh: true);
            }

            return this;
        }

        private void AttachEvents(HTMLElement el, string id)
        {
            el.onkeydown = (e) =>
            {
                if (e.key == " ") StopEvent(e);
            };
            el.onkeyup = (e) =>
            {
                if (e.key == "Enter" || e.key == " ")
                {
                    StopEvent(e);
                    Select(id);
                }
            };
            el.onclick = (e) =>
            {
                StopEvent(e);
                Select(id);
            };
        }

        public SidebarSegmentedPivot Select(string id, bool refresh = false)
        {
            if (_currentSelectedID != id || refresh)
            {
                var tab = _orderedTabs.FirstOrDefault(t => t.Id == id);
                if (tab is object)
                {
                    _currentSelectedID = id;

                    foreach (var t in _orderedTabs)
                    {
                        if (t.Id == id)
                        {
                            t.OpenTitle.classList.add("tss-segmentedpivot-selected");
                            t.ClosedTitle.classList.add("tss-segmentedpivot-selected");
                        }
                        else
                        {
                            t.OpenTitle.classList.remove("tss-segmentedpivot-selected");
                            t.ClosedTitle.classList.remove("tss-segmentedpivot-selected");
                        }
                    }

                    ClearChildren(_renderedContentOpen);
                    ClearChildren(_renderedContentClosed);

                    foreach(var item in tab.Items)
                    {
                        _renderedContentOpen.appendChild(item.RenderOpen().Render());
                        _renderedContentClosed.appendChild(item.RenderClosed().Render());
                    }
                }
            }
            return this;
        }

        private void ClearChildren(HTMLElement element)
        {
            while(element.lastChild is object)
            {
                element.removeChild(element.lastChild);
            }
        }

        public void Collapse()
        {
            _openContainer.classList.add("tss-collapse");
            _closedContainer.classList.add("tss-collapse");
        }

        public void Show()
        {
            _openContainer.classList.remove("tss-collapse");
            _closedContainer.classList.remove("tss-collapse");
        }

        public IComponent RenderClosed()
        {
            if (_currentSelectedID is null && _initiallySelectedID is object)
            {
                Select(_initiallySelectedID);
            }
            return Raw(_closedContainer);
        }

        public IComponent RenderOpen()
        {
            if (_currentSelectedID is null && _initiallySelectedID is object)
            {
                Select(_initiallySelectedID);
            }
            return Raw(_openContainer);
        }

        public bool Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Show();
                var currentTab = _orderedTabs.FirstOrDefault(t => t.Id == _currentSelectedID);
                if (currentTab != null)
                {
                    foreach(var item in currentTab.Items)
                    {
                        if(item is ISearchableSidebarItem s) s.Search(searchTerm);
                        else item.Show();
                    }
                }
                return true;
            }

            var tab = _orderedTabs.FirstOrDefault(t => t.Id == _currentSelectedID);
            if (tab is null) return false;

            bool anyChildMatch = false;
            foreach (var item in tab.Items)
            {
                if (item is ISearchableSidebarItem s)
                {
                    if (s.Search(searchTerm)) anyChildMatch = true;
                }
                else
                {
                    item.Collapse();
                }
            }

            if (anyChildMatch)
            {
                Show();
                return true;
            }

            Collapse();
            return false;
        }

        internal sealed class Tab
        {
            public Tab(string id, Func<IComponent> titleCreator, ISidebarItem[] items)
            {
                Id = id;
                _titleCreator = titleCreator;
                Items = items;
            }

            private Func<IComponent> _titleCreator;
            public string Id { get; }
            public ISidebarItem[] Items { get; }
            public HTMLElement OpenTitle { get; set; }
            public HTMLElement ClosedTitle { get; set; }

            public IComponent CreateTitle() => _titleCreator();
        }
    }
}
