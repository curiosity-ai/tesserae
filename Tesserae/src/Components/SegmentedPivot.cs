using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A pill-style pivot variant where the tabs share a connected segmented-control look.
    /// </summary>
    [H5.Name("tss.SegmentedPivot")]
    public sealed class SegmentedPivot : IComponent
    {
        public delegate void PivotEventHandler<TEventArgs>(SegmentedPivot sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent> _navigated;

        private readonly List<Tab> _orderedTabs = new List<Tab>();
        private readonly Dictionary<Tab, HTMLElement> _renderedTitles = new Dictionary<Tab, HTMLElement>();

        private readonly HTMLElement _renderedTabs;
        private readonly HTMLElement _renderedContent;
        private readonly HTMLElement _container;

        private string _initiallySelectedID;
        private string _currentSelectedID;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SegmentedPivot()
        {
            _renderedTabs = Div(_("tss-segmentedpivot-titlebar", role: "tablist"));
            var wrapper = Div(_("tss-segmentedpivot-wrapper"), _renderedTabs);
            _renderedContent = Div(_("tss-segmentedpivot-content", role: "tabpanel"));

            _container = Div(_("tss-segmentedpivot"), wrapper, _renderedContent);
        }

        /// <summary>
        /// Registers a callback invoked when the before navigate event fires.
        /// </summary>
        public SegmentedPivot OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            _beforeNavigated += onBeforeNavigate;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the navigate event fires.
        /// </summary>
        public SegmentedPivot OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            _navigated += onNavigate;
            return this;
        }

        internal SegmentedPivot Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);

            var titleContainer = Div(_("tss-segmentedpivot-tab"));
            titleContainer.tabIndex = 0;
            titleContainer.onkeydown = (e) =>
            {
                if (e.key == " ")
                {
                    StopEvent(e);
                }
            };
            titleContainer.onkeyup = (e) =>
            {
                if (e.key == "Enter" || e.key == " ")
                {
                    StopEvent(e);
                    Select(tab.Id);
                }
            };
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
        public SegmentedPivot Select(string id, bool refresh = false)
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
                            kvp.Value.classList.add("tss-segmentedpivot-selected");
                        }
                        else
                        {
                            kvp.Value.classList.remove("tss-segmentedpivot-selected");
                        }
                    }

                    ClearChildrenExceptCached();

                    var content = tab.RenderContent();
                    if (tab.KeepCached)
                    {
                        content.classList.add("tss-segmentedpivot-keep-cached");
                        content.classList.remove("tss-segmentedpivot-cached-hidden");
                    }

                    _renderedContent.appendChild(content);

                    var pne = new PivotNavigateEvent(_currentSelectedID, id);
                    _navigated?.Invoke(this, pne);
                }
            }
            return this;
        }

        private void ClearChildrenExceptCached()
        {
            foreach (var el in _renderedContent.children)
            {
                if (el.classList.contains("tss-segmentedpivot-keep-cached"))
                {
                    el.classList.add("tss-segmentedpivot-cached-hidden");
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
