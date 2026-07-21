using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A pivot variant that styles each tab as a card, used for dashboard-style switching between rich panels.
    /// </summary>
    [Transpose.Name("tss.CardPivot")]
    public sealed class CardPivot : IComponent, ISpecialCaseStyling, IBindableComponent<string>
    {
        public delegate void PivotEventHandler<TEventArgs>(CardPivot sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent>       _navigated;

        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        private readonly List<Tab>                    _orderedTabs    = new List<Tab>();
        private readonly Dictionary<Tab, HTMLElement> _renderedTitles = new Dictionary<Tab, HTMLElement>();

        private readonly HTMLElement _renderedTabs;
        private readonly HTMLElement _renderedContent;

        private string _initiallySelectedID;
        private string _currentSelectedID;

        /// <summary>
        /// Gets the HTMLElement that should receive styling. Exposing the root as the
        /// styling container (via <see cref="ISpecialCaseStyling"/>) lets sizing helpers
        /// like .S() / .Grow() write directly onto the pivot instead of an extra wrapper
        /// when it is placed inside a Stack or Grid, matching Pivot and SegmentedPivot.
        /// </summary>
        public HTMLElement StylingContainer { get; }

        /// <summary>
        /// Gets whether styling should propagate to the stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CardPivot()
        {
            _renderedTabs    = Div(_("tss-cardpivot-titlebar", role: "tablist"));
            _renderedContent = Div(_("tss-cardpivot-content",  role: "tabpanel"));

            StylingContainer = Div(_("tss-cardpivot"), _renderedTabs, _renderedContent);
        }

        /// <summary>
        /// Registers a callback invoked when the before navigate event fires.
        /// </summary>
        public CardPivot OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            _beforeNavigated += onBeforeNavigate;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the navigate event fires.
        /// </summary>
        public CardPivot OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            _navigated += onNavigate;
            return this;
        }

        internal CardPivot Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);

            var titleContainer = Div(_("tss-cardpivot-tab"));

            titleContainer.onclick = (e) =>
            {
                StopEvent(e);
                Select(tab.Id);
            };

            var titleComponent = tab.CreateTitle();
            titleContainer.appendChild(titleComponent.Render());

            _renderedTitles.Add(tab, titleContainer);
            _renderedTabs.appendChild(titleContainer);

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

        /// <summary>
        /// Configures the component to select.
        /// </summary>
        public CardPivot Select(string id, bool refresh = false)
        {
            if (_currentSelectedID != id || refresh)
            {
                var tab = _orderedTabs.FirstOrDefault(t => t.Id == id);

                if (tab is object)
                {
                    var pbne = new PivotBeforeNavigateEvent(_currentSelectedID, id);
                    _beforeNavigated?.Invoke(this, pbne);
                    if (pbne.Canceled) return this;

                    _currentSelectedID = id;

                    foreach (var kvp in _renderedTitles)
                    {
                        if (kvp.Key.Id == id)
                        {
                            kvp.Value.classList.add("tss-cardpivot-selected");
                        }
                        else
                        {
                            kvp.Value.classList.remove("tss-cardpivot-selected");
                        }
                    }

                    ClearChildrenExceptCached();

                    var content = tab.RenderContent();

                    if (tab.KeepCached)
                    {
                        content.classList.add("tss-cardpivot-keep-cached");
                        content.classList.remove("tss-cardpivot-cached-hidden");
                    }

                    _renderedContent.appendChild(content);

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

        private void ClearChildrenExceptCached()
        {
            foreach (var el in _renderedContent.children)
            {
                if (el.classList.contains("tss-cardpivot-keep-cached"))
                {
                    el.classList.add("tss-cardpivot-cached-hidden");
                }
                else
                {
                    _renderedContent.removeChild(el);
                }
            }
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

            internal bool KeepCached => _canCacheContent;
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