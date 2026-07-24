using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A pill-style pivot variant where the tabs share a connected segmented-control look.
    /// </summary>
    [Transpose.Name("tss.SegmentedPivot")]
    public sealed class SegmentedPivot : IComponent, ISpecialCaseStyling, IBindableComponent<string>
    {
        public delegate void PivotEventHandler<TEventArgs>(SegmentedPivot sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent>       _navigated;

        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        private readonly List<Tab>                    _orderedTabs    = new List<Tab>();
        private readonly Dictionary<Tab, HTMLElement> _renderedTitles = new Dictionary<Tab, HTMLElement>();

        private readonly HTMLElement _renderedTabs;
        private readonly HTMLElement _renderedContent;
        private readonly HTMLElement _scroller;
        private readonly HTMLElement _titlebarWrapper;

        private readonly Button _moreBtn;
        private readonly Button _scrollLeftBtn;
        private readonly Button _scrollRightBtn;

        private string         _initiallySelectedID;
        private string         _currentSelectedID;
        private bool           _isRendered = false;
        private ResizeObserver _ro;

        // Fraction of the scroller's visible width to move per scroll-button click.
        private const double ScrollButtonStep = 0.7;

        /// <summary>
        /// Gets or sets the selected tab.
        /// </summary>
        public string SelectedTab => _currentSelectedID ?? _initiallySelectedID;

        /// <summary>
        /// Gets the HTMLElement that should receive styling. Exposing the root as the
        /// styling container (via <see cref="ISpecialCaseStyling"/>) lets sizing helpers
        /// like .S() / .Grow() write directly onto the pivot instead of an extra
        /// wrapper when it is placed inside a Stack or Grid, matching Pivot.
        /// </summary>
        public HTMLElement StylingContainer { get; }

        /// <summary>
        /// Gets whether styling should propagate to the stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SegmentedPivot()
        {
            _scrollLeftBtn  = Button().SetIcon(UIcons.AngleLeft).NoMinSize().HS().NoPadding().NoMargin().Class("tss-segmentedpivot-titlebar-scroll-left").OnClick(() => ScrollByAmount(-_scroller.clientWidth * ScrollButtonStep));
            _scrollRightBtn = Button().SetIcon(UIcons.AngleRight).NoMinSize().HS().NoPadding().NoMargin().Class("tss-segmentedpivot-titlebar-scroll-right").OnClick(() => ScrollByAmount(_scroller.clientWidth * ScrollButtonStep));
            _moreBtn        = Button().SetIcon(UIcons.MenuDots).NoMinSize().HS().NoPadding().NoMargin().Class("tss-segmentedpivot-titlebar-more").OnClick(() => ShowAllTabs());

            _renderedTabs = Div(Att("tss-segmentedpivot-titlebar", role: "tablist"));
            _scroller     = Div(Att("tss-segmentedpivot-titlebar-scroller"), _renderedTabs);

            _titlebarWrapper = Div(Att("tss-segmentedpivot-titlebar-wrapper"),
                _scrollLeftBtn.Render(),
                _scroller,
                _scrollRightBtn.Render(),
                _moreBtn.Render());

            _renderedContent = Div(Att("tss-segmentedpivot-content", role: "tabpanel"));

            StylingContainer = Div(Att("tss-segmentedpivot"), _titlebarWrapper, _renderedContent);

            AttachScrollerEvents();
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

        /// <summary>
        /// Refreshes the pivot sizes, re-evaluating whether the scroll and overflow
        /// controls should be visible. Useful after the pivot's container changes size
        /// in a way a <see cref="ResizeObserver"/> can't observe.
        /// </summary>
        public void RefreshPivotSizes()
        {
            UpdateScrollState();
        }

        internal SegmentedPivot Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);

            var titleContainer = Div(Att("tss-segmentedpivot-tab"));
            titleContainer.tabIndex = 0;
            titleContainer.setAttribute("role", "tab");

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

            UpdateScrollState();

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

                    HTMLElement selectedTitle = null;

                    foreach (var kvp in _renderedTitles)
                    {
                        if (kvp.Key.Id == id)
                        {
                            kvp.Value.classList.add("tss-segmentedpivot-selected");
                            kvp.Value.setAttribute("aria-selected", "true");
                            selectedTitle = kvp.Value;
                        }
                        else
                        {
                            kvp.Value.classList.remove("tss-segmentedpivot-selected");
                            kvp.Value.setAttribute("aria-selected", "false");
                        }
                    }

                    ClearChildrenExceptCached();

                    // Append the tab's content directly, exactly like Pivot does. The
                    // content pane (.tss-segmentedpivot-content) is itself a flex column
                    // — like a Stack with a single always-visible child — so panel-filling
                    // content sizes correctly against it whether it fills via .S()/.HS(),
                    // .Grow(), or its own height:100%. Wrapping the content in an extra
                    // tss-stack-item (height:auto) instead broke the percentage-height
                    // chain and collapsed such content, so we no longer do that.
                    var content = tab.RenderContent();

                    if (tab.KeepCached)
                    {
                        content.classList.add("tss-segmentedpivot-keep-cached");
                        content.classList.remove("tss-segmentedpivot-cached-hidden");
                    }

                    _renderedContent.appendChild(content);

                    if (selectedTitle is object) ScrollIntoView(selectedTitle);

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

        private void AttachScrollerEvents()
        {
            // Convert vertical wheel motion (typical mouse) into horizontal scrolling.
            // This listener calls preventDefault() so it must be non-passive; mark it
            // explicitly so the browser doesn't have to guess.
            _scroller.addEventListener("wheel", (Action<Event>)(e =>
            {
                var we = e.As<WheelEvent>();

                if (Math.Abs(we.deltaY) > Math.Abs(we.deltaX))
                {
                    _scroller.scrollLeft += we.deltaY;
                    e.preventDefault();
                }
            }), new AddEventListenerOptions { passive = false });

            _scroller.addEventListener("scroll", e => UpdateScrollButtons());
        }

        private void ScrollByAmount(double dx)
        {
            _scroller.scrollLeft += dx;
        }

        private void UpdateScrollState()
        {
            if (!StylingContainer.IsMounted()) return;
            UpdateScrollButtons();
            UpdateMoreVisibility();
        }

        private void UpdateScrollButtons()
        {
            var canScrollLeft  = _scroller.scrollLeft > 0;
            var canScrollRight = _scroller.scrollLeft + _scroller.clientWidth < _scroller.scrollWidth - 1; // -1 for sub-pixel rounding
            _scrollLeftBtn.Render().style.display  = canScrollLeft ? "" : "none";
            _scrollRightBtn.Render().style.display = canScrollRight ? "" : "none";
        }

        private void UpdateMoreVisibility()
        {
            // Only surface the overflow (⋯) menu when segments are actually clipped,
            // i.e. the strip content is wider than the visible scroller. Driven by the
            // ResizeObserver so it re-evaluates whenever the available width changes.
            var overflows = _scroller.scrollWidth > _scroller.clientWidth + 1; // +1 for sub-pixel rounding
            _moreBtn.Render().style.display = (_orderedTabs.Count > 0 && overflows) ? "" : "none";
        }

        private void ShowAllTabs()
        {
            var items = new ContextMenu.Item[_orderedTabs.Count];

            for (int i = 0; i < _orderedTabs.Count; i++)
            {
                var tab   = _orderedTabs[i];
                var title = _renderedTitles[tab];
                var clone = title.cloneNode(true).As<HTMLElement>();
                clone.classList.remove("tss-segmentedpivot-selected");
                var id = tab.Id;
                items[i] = ContextMenuItem(Raw(clone)).OnClick(() => Select(id));
            }
            ContextMenu().Items(items).ShowFor(_moreBtn);
        }

        private void ScrollIntoView(HTMLElement target)
        {
            if (!_scroller.IsMounted()) return;
            var tabLeft   = (double)target.offsetLeft;
            var tabRight  = tabLeft + target.offsetWidth;
            var viewLeft  = _scroller.scrollLeft;
            var viewRight = viewLeft + _scroller.clientWidth;

            if (tabLeft < viewLeft)
            {
                _scroller.scrollLeft = tabLeft;
            }
            else if (tabRight > viewRight)
            {
                _scroller.scrollLeft = tabRight - _scroller.clientWidth;
            }
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

            if (!_isRendered)
            {
                _isRendered = true;

                _ro = new ResizeObserver((entries, obs) => UpdateScrollState());
                _ro.observe(StylingContainer);

                DomObserver.WhenMounted(StylingContainer, () =>
                {
                    UpdateScrollState();

                    // Also do on a timeout to account for animations on modals.
                    window.setTimeout((_) => UpdateScrollState(), 1000);
                });

                DomObserver.WhenRemoved(StylingContainer, () => _ro.unobserve(StylingContainer));
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
            /// Renders the content, reusing the cached element when the tab is cacheable
            /// so its rendered state survives tab switches.
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
