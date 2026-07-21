using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A standalone tab-selector strip that drives an external <see cref="Pivot"/>, used when the strip needs to be
    /// positioned independently of its panels.
    /// </summary>
    [Transpose.Name("tss.PivotSelector")]
    public class PivotSelector : IComponent, ISpecialCaseStyling, IBindableComponent<string>
    {
        public delegate void PivotEventHandler<TEventArgs>(PivotSelector sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent>       _navigated;

        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        private readonly List<Tab>   _orderedTabs = new List<Tab>();
        private readonly Dropdown    _dropdown;
        private readonly Stack       _commands;
        private readonly HTMLElement _renderedContent;

        private string _initiallySelectedID;
        private string _currentSelectedID;

        /// <summary>
        /// Gets the HTMLElement that should receive styling. Exposing the root as the
        /// styling container (via <see cref="ISpecialCaseStyling"/>) lets sizing helpers
        /// like .S() / .Grow() write directly onto the selector instead of an extra
        /// wrapper when it is placed inside a Stack or Grid, matching the pivot family.
        /// </summary>
        public HTMLElement StylingContainer { get; }

        /// <summary>
        /// Gets whether styling should propagate to the stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public PivotSelector()
        {
            _dropdown        = Dropdown().Grow().MaxWidth(500.px()).MinWidth(new UnitSize("min(200px, 100%)"));
            _commands        = HStack().WS().NoShrink();
            _renderedContent = Div(_("tss-pivotselector-content", role: "tabpanel"));

            var header = HStack().Children(_dropdown, _commands).WS().AlignItemsCenter().PaddingBottom(8.px());

            // Build the root as a flex column with the header and content pane as direct
            // children (rather than wrapping them through a Stack), so the content pane is
            // a real flex child that grows to fill and lays its active tab out like a Stack.
            StylingContainer = Div(_("tss-pivotselector"), header.Render(), _renderedContent);

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

                    _observable.Value = _currentSelectedID;

                    var pne = new PivotNavigateEvent(_currentSelectedID, id);
                    _navigated?.Invoke(this, pne);
                }
            }
            return this;
        }

        /// <summary>
        /// Returns an observable that tracks the id of the currently-selected tab.
        /// </summary>
        public IObservable<string> AsObservable() => _observable;

        /// <summary>
        /// Programmatically selects a tab by id as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            Select(value);
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
            return StylingContainer;
        }

        internal sealed class Tab
        {
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Tab(string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
            {
                Id               = id;
                _canCacheContent = cached;
                _contentCreator  = contentCreator;
                _titleCreator    = titleCreator;
            }

            private          Func<IComponent> _titleCreator;
            private          Func<IComponent> _contentCreator;
            private          HTMLElement      _content;
            private readonly bool             _canCacheContent;

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
                TargetPivot  = targetPivot;
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