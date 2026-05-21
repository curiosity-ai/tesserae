using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A standalone tab-selector strip that drives an external <see cref="Pivot"/>, used when the strip needs to be
    /// positioned independently of its panels.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
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
                    Select(selected.GetDataAs<string>());
                }
            });
        }

        /// <summary>
        /// Registers a callback invoked when the before navigate event fires.
        /// </summary>
        public PivotSelector OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            _beforeNavigated += onBeforeNavigate;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the navigate event fires.
        /// </summary>
        public PivotSelector OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            _navigated += onNavigate;
            return this;
        }

        /// <summary>
        /// Sets the commands of the component.
        /// </summary>
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

        /// <summary>
        /// Configures the component to select.
        /// </summary>
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

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
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
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
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

            /// <summary>
            /// Sets the DOM id of the component.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Configures the create title on the component.
            /// </summary>
            public IComponent CreateTitle() => _titleCreator();

            /// <summary>
            /// Renders the content.
            /// </summary>
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
            /// <summary>
            /// Cancels the component's current operation.
            /// </summary>
            public void Cancel() => Canceled = true;
        }

        public abstract class PivotEvent
        {
            internal PivotEvent(string currentPivot, string targetPivot)
            {
                CurrentPivot = currentPivot;
                TargetPivot = targetPivot;
            }
            /// <summary>
            /// Gets or sets the current pivot.
            /// </summary>
            public string CurrentPivot { get; }
            /// <summary>
            /// Gets or sets the target pivot.
            /// </summary>
            public string TargetPivot { get; }
        }
    }
}
