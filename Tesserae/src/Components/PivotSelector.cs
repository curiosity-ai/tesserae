using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.PivotSelector")]
    public class PivotSelector : IComponent
    {
        public delegate void PivotEventHandler<TEventArgs>(PivotSelector sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent> _navigated;

        private readonly List<Tab> _orderedTabs = new List<Tab>();
        private readonly Dropdown _dropdown;
        private readonly Stack _commands;
        private readonly HTMLElement _renderedContent;
        private readonly HTMLElement _container;

        private string _initiallySelectedID;
        private string _currentSelectedID;

        public PivotSelector()
        {
            _dropdown = Dropdown().Grow().MaxWidth(500.px()).MinWidth(new UnitSize("min(200px, 100%)"));
            _commands = HStack().WS().NoShrink();
            _renderedContent = Div(_("tss-pivot-content", role: "tabpanel"));

            var header = HStack().Children(_dropdown, _commands).WS().AlignItemsCenter().PaddingBottom(8.px());

            _container = VStack().Class("tss-pivotselector").Children(header, Raw(_renderedContent)).Render();

            _dropdown.OnInput((s, e) =>
            {
                var selected = _dropdown.SelectedItems.FirstOrDefault();
                if (selected != null)
                {
                    Select(selected.Data as string);
                }
            });
        }

        public PivotSelector OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            _beforeNavigated += onBeforeNavigate;
            return this;
        }

        public PivotSelector OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            _navigated += onNavigate;
            return this;
        }

        public PivotSelector SetCommands(params IComponent[] commands)
        {
            _commands.Children(commands);
            return this;
        }

        internal PivotSelector Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);

            UpdateDropdownItems();

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

        private void UpdateDropdownItems()
        {
            _dropdown.Items(_orderedTabs.Select(t => DropdownItem(t.CreateTitle(), t.CreateTitle()).SetData(t.Id).SelectedIf(t.Id == _currentSelectedID)).ToArray());
        }

        public PivotSelector Select(string id, bool refresh = false)
        {
            if (_currentSelectedID != id || refresh)
            {
                var tab = _orderedTabs.FirstOrDefault(t => t.Id == id);
                if (tab is object)
                {
                    var pbne = new PivotBeforeNavigateEvent(_currentSelectedID, id);
                    _beforeNavigated?.Invoke(this, pbne);
                    if (pbne.Canceled)
                    {
                        UpdateDropdownItems();
                        return this;
                    }

                    _currentSelectedID = id;

                    UpdateDropdownItems();

                    ClearChildren(_renderedContent);
                    _renderedContent.appendChild(tab.RenderContent());

                    var pne = new PivotNavigateEvent(_currentSelectedID, id);
                    _navigated?.Invoke(this, pne);
                }
            }
            return this;
        }

        public HTMLElement Render()
        {
            if (_currentSelectedID is null && _initiallySelectedID is object)
            {
                Select(_initiallySelectedID);
            }
            return _container;
        }

        internal sealed class Tab
        {
            public Tab(string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
            {
                Id = id;
                _canCacheContent = cached;
                _contentCreator = contentCreator;
                _titleCreator = titleCreator;
            }

            private Func<IComponent> _titleCreator;
            private Func<IComponent> _contentCreator;
            private HTMLElement _content;
            private readonly bool _canCacheContent;

            public string Id { get; }

            public IComponent CreateTitle() => _titleCreator();

            public HTMLElement RenderContent()
            {
                if (_canCacheContent && _content is object)
                {
                    return _content;
                }
                else
                {
                    _content = _contentCreator().Render();
                    return _content;
                }
            }
        }

        public sealed class PivotNavigateEvent : PivotEvent
        {
            internal PivotNavigateEvent(string currentPivot, string targetPivot) : base(currentPivot, targetPivot) { }
        }

        public class PivotBeforeNavigateEvent : PivotEvent
        {
            internal PivotBeforeNavigateEvent(string currentPivot, string targetPivot) : base(currentPivot, targetPivot) => Canceled = false;
            internal bool Canceled { get; private set; }
            public void Cancel() => Canceled = true;
        }

        public abstract class PivotEvent
        {
            internal PivotEvent(string currentPivot, string targetPivot)
            {
                CurrentPivot = currentPivot;
                TargetPivot = targetPivot;
            }
            public string CurrentPivot { get; }
            public string TargetPivot { get; }
        }
    }
}
