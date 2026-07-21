using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A horizontal tabbed surface with one tab visible at a time.
    /// </summary>
    [Transpose.Name("tss.Pivot")]
    public sealed class Pivot : IComponent, ISpecialCaseStyling, IBindableComponent<string>
    {
        public delegate void PivotEventHandler<TEventArgs>(Pivot sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent>       _navigated;

        private readonly SettableObservable<string>   _observable     = new SettableObservable<string>();
        private readonly List<Tab>                    _orderedTabs    = new List<Tab>();
        private readonly Dictionary<Tab, HTMLElement> _renderedTitles = new Dictionary<Tab, HTMLElement>();
        private readonly HTMLElement                  _renderedTabs;
        private readonly HTMLElement                  _renderedContent;
        private readonly HTMLElement                  _line;
        private readonly HTMLElement                  _scroller;
        private readonly HTMLElement                  _titlebarWrapper;
        private          string                       _initiallySelectedID;
        private          string                       _currentSelectedID;
        private          bool                         _isRendered   = false;
        private          bool                         _hideIfSingle = false;
        /// <summary>
        /// Gets or sets the selected tab.
        /// </summary>
        public string SelectedTab => _currentSelectedID ?? _initiallySelectedID;
        private          HTMLElement    _selectedNav;
        private          HTMLElement    _hoveredNav;
        private          bool           _firstRender = false;
        private          ResizeObserver _ro;
        private readonly Button         _moreBtn;
        private readonly Button         _scrollLeftBtn;
        private readonly Button         _scrollRightBtn;

        private Action<Event> _tabSwitchHandler;

        // Fraction of the scroller's visible width to move per scroll-button click.
        private const double ScrollButtonStep = 0.7;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Pivot()
        {
            _scrollLeftBtn  = Button().SetIcon(UIcons.AngleLeft).NoMinSize().HS().NoPadding().NoMargin().Class("tss-pivot-titlebar-scroll-left").OnClick(() => ScrollByAmount(-_scroller.clientWidth * ScrollButtonStep));
            _scrollRightBtn = Button().SetIcon(UIcons.AngleRight).NoMinSize().HS().NoPadding().NoMargin().Class("tss-pivot-titlebar-scroll-right").OnClick(() => ScrollByAmount(_scroller.clientWidth * ScrollButtonStep));
            _moreBtn        = Button().SetIcon(UIcons.MenuDots).NoMinSize().HS().NoPadding().NoMargin().Class("tss-pivot-titlebar-more").OnClick(() => ShowAllTabs());

            _renderedTabs = Div(Att("tss-pivot-titlebar", role: "tablist"));
            _line         = Div(Att("tss-pivot-line"));
            _renderedTabs.appendChild(_line); // line sits inside titlebar so it scrolls with tabs
            _scroller = Div(Att("tss-pivot-titlebar-scroller"), _renderedTabs);

            _titlebarWrapper = Div(Att("tss-pivot-titlebar-wrapper"),
                _scrollLeftBtn.Render(),
                _scroller,
                _scrollRightBtn.Render(),
                _moreBtn.Render());
            _renderedContent = Div(Att("tss-pivot-content", role: "tabpanel"));
            StylingContainer = Div(Att("tss-pivot"), _titlebarWrapper, _renderedContent);

            AttachScrollerEvents();
        }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer { get; }

        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => true;

        /// <summary>
        /// Configures the component to justified.
        /// </summary>
        public Pivot Justified()
        {
            _renderedTabs.style.justifyContent = "space-between";
            return this;
        }

        /// <summary>
        /// Configures the component to centered.
        /// </summary>
        public Pivot Centered()
        {
            _renderedTabs.style.justifyContent = "center";
            return this;
        }


        /// <summary>
        /// Hides the if single.
        /// </summary>
        public Pivot HideIfSingle()
        {
            _hideIfSingle = true;
            UpdateTitlebarVisibility();
            return this;
        }

        /// <summary>
        /// Enables Ctrl+Alt+Right / Ctrl+Alt+Left to cycle through tabs in order
        /// while the pivot is mounted and focus is inside its container (or no
        /// element is focused). Safe to call multiple times — the handler is
        /// only registered once.
        /// </summary>
        public Pivot EnableCtrlTabSwitching()
        {
            if (_tabSwitchHandler is object) return this;

            _tabSwitchHandler = ev =>
            {
                if (!StylingContainer.IsMounted()) return;
                if (_orderedTabs.Count <= 1) return;

                var ke = ev.As<KeyboardEvent>();
                if (!ke.ctrlKey || !ke.altKey || ke.shiftKey || ke.metaKey) return;
                if (ke.key != "ArrowRight" && ke.key != "ArrowLeft") return;

                // Scope to focus within this pivot so multiple pivots on the
                // same page don't fight, while still firing when nothing is
                // focused (i.e. the user hasn't clicked anything yet).
                var active = document.activeElement;
                if (active is object && active != document.body && !StylingContainer.contains(active)) return;

                StopEvent(ev);
                NavigateBy(ke.key == "ArrowLeft" ? -1 : 1);
            };

            window.addEventListener("keydown", _tabSwitchHandler);
            return this;
        }

        private void NavigateBy(int delta)
        {
            if (_orderedTabs.Count == 0) return;

            int currentIdx                 = _orderedTabs.FindIndex(t => t.Id == _currentSelectedID);
            if (currentIdx < 0) currentIdx = 0;

            int n       = _orderedTabs.Count;
            int nextIdx = ((currentIdx + delta) % n + n) % n;
            if (nextIdx == currentIdx) return;

            Select(_orderedTabs[nextIdx].Id);
        }

        /// <summary>
        /// Refreshes the pivot sizes.
        /// </summary>
        public void RefreshPivotSizes()
        {
            UpdateScrollState();
            TriggerAnimation();
        }

        internal Pivot Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);
            var title = tab.RenderTitle();

            if (tab.Closeable)
            {
                var closeIcon = I(Att("tss-pivot-tab-close tss-fontsize-tiny " + UIcons.Cross.ToString(), ariaLabel: "Close tab"));

                closeIcon.onclick = (e) =>
                {
                    StopEvent(e);
                    RequestCloseTab(tab);
                };
                title.appendChild(closeIcon);
                title.classList.add("tss-pivot-tab-closeable");
            }
            title.setAttribute("role",          "tab");
            title.setAttribute("aria-selected", "false");
            title.tabIndex = -1;
            _renderedTitles.Add(tab, title);
            AttachEvents(tab.Id, title);
            _renderedTabs.insertBefore(title, _line); // keep _line as last child

            if (_isRendered && _orderedTabs.Count == 1)
            {
                Select(tab.Id);
            }

            UpdateTitlebarVisibility();
            UpdateScrollState();

            return this;
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

            // ARIA tablist keyboard navigation. Arrow keys move and activate the
            // adjacent tab, Home/End jump to the first/last tab.
            _renderedTabs.addEventListener("keydown", e =>
            {
                var ke = e.As<KeyboardEvent>();

                if (ke.key == "ArrowLeft" || ke.key == "ArrowRight" || ke.key == "Home" || ke.key == "End")
                {
                    StopEvent(e);
                    NavigateWithKey(ke.key);
                }
            });
        }

        private void NavigateWithKey(string key)
        {
            if (_orderedTabs.Count == 0) return;

            int currentIdx                 = _orderedTabs.FindIndex(t => t.Id == _currentSelectedID);
            if (currentIdx < 0) currentIdx = 0;

            int nextIdx = currentIdx;

            switch (key)
            {
                case "ArrowLeft": nextIdx  = (currentIdx - 1 + _orderedTabs.Count) % _orderedTabs.Count; break;
                case "ArrowRight": nextIdx = (currentIdx + 1) % _orderedTabs.Count; break;
                case "Home": nextIdx       = 0; break;
                case "End": nextIdx        = _orderedTabs.Count - 1; break;
            }

            if (nextIdx != currentIdx)
            {
                var nextTab = _orderedTabs[nextIdx];
                Select(nextTab.Id);

                if (_renderedTitles.TryGetValue(nextTab, out var nextTitle))
                {
                    nextTitle.focus();
                }
            }
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
            _moreBtn.Render().style.display = _orderedTabs.Count > 0 ? "" : "none";
        }

        private void UpdateTitlebarVisibility()
        {
            var hide = _hideIfSingle && _orderedTabs.Count <= 1;
            _titlebarWrapper.style.display = hide ? "none" : "";
        }

        private void ShowAllTabs()
        {
            var items = new ContextMenu.Item[_orderedTabs.Count];

            for (int i = 0; i < _orderedTabs.Count; i++)
            {
                var tab   = _orderedTabs[i];
                var title = _renderedTitles[tab];
                var clone = title.cloneNode(true).As<HTMLElement>();
                clone.classList.remove("tss-pivot-titlebar-hidden-overflow");

                // The cloned close icon is non-functional inside the menu; strip it
                // so the user gets a clean title row.
                foreach (var x in clone.querySelectorAll(".tss-pivot-tab-close").ToList())
                {
                    x.As<HTMLElement>().remove();
                }
                var id = tab.Id;
                items[i] = ContextMenuItem(Raw(clone)).OnClick(() => Select(id));
            }
            ContextMenu().Items(items).ShowFor(_moreBtn);
        }

        /// <summary>
        /// Invoked by the tab's close button. When the tab supplies an
        /// <see cref="Tab.OnBeforeClose"/> guard (e.g. to confirm discarding
        /// unsaved changes), it is awaited first and the tab is only removed
        /// if the guard resolves <c>true</c>. Tabs without a guard close
        /// immediately, exactly as before.
        /// </summary>
        private void RequestCloseTab(Tab tab)
        {
            if (tab.OnBeforeClose is null)
            {
                RemoveTab(tab.Id);
                return;
            }

            RequestCloseTabAsync(tab).FireAndForget();
        }

        private async Task RequestCloseTabAsync(Tab tab)
        {
            var canClose = await tab.OnBeforeClose();

            if (canClose)
            {
                RemoveTab(tab.Id);
            }
        }

        /// <summary>
        /// Removes the given tab from the component.
        /// </summary>
        public void RemoveTab(string id)
        {
            var tab = _orderedTabs.FirstOrDefault(t => t.Id == id);

            if (tab is object)
            {
                _orderedTabs.Remove(tab);

                if (_renderedTitles.TryGetValue(tab, out var renderedTitle))
                {
                    _renderedTabs.removeChild(renderedTitle);
                    _renderedTitles.Remove(tab);
                }

                if (_currentSelectedID == id)
                {
                    if (_isRendered && _orderedTabs.Count > 0)
                    {
                        Select(_orderedTabs.First().Id);
                    }
                }

                if (_initiallySelectedID == id)
                {
                    _initiallySelectedID = null;
                }

                tab.OnClosed?.Invoke();

                UpdateTitlebarVisibility();
                UpdateScrollState();
            }
        }

        private void AttachEvents(string id, HTMLElement title)
        {
            title.onclick = (e) =>
            {
                StopEvent(e);
                Select(id);
            };

            title.onmouseover = e =>
            {
                _hoveredNav = title;
                TriggerAnimation();
            };

            title.onmouseleave = e =>
            {
                if (_hoveredNav == title)
                {
                    _hoveredNav = null;
                    TriggerAnimation();
                }
            };
        }

        /// <summary>
        /// Registers a callback invoked when the before navigate event fires.
        /// </summary>
        public Pivot OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            _beforeNavigated += onBeforeNavigate;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the navigate event fires.
        /// </summary>
        public Pivot OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            _navigated += onNavigate;
            return this;
        }

        /// <summary>
        /// Configures the component to select.
        /// </summary>
        public Pivot Select(string id, bool refresh = false)
        {
            if (_currentSelectedID != id || refresh)
            {
                var tab = _orderedTabs.FirstOrDefault(t => t.Id == id);

                if (tab is object)
                {
                    Select(tab);
                }
            }
            return this;
        }

        private Pivot Select(Tab tab)
        {
            if (!_isRendered)
            {
                _initiallySelectedID = tab.Id;
                return this;
            }

            var pbne = new PivotBeforeNavigateEvent(_currentSelectedID, tab.Id);

            _beforeNavigated?.Invoke(this, pbne);

            if (pbne.Canceled) return this;

            var title = _renderedTitles[tab];

            HTMLElement content = Div(Att());
            content.style.width     = "100%";
            content.style.minHeight = "100%";

            try
            {
                content = tab.RenderContent();
            }
            catch (Exception E)
            {
                content.textContent = E.ToString();
            }

            ClearChildrenExceptCached();

            if (tab.KeepCached)
            {
                content.classList.add("tss-pivot-keep-cached");
                content.classList.remove("tss-pivot-cached-hidden");
            }

            _renderedContent.appendChild(content);

            _currentSelectedID = tab.Id;
            UpdateTitleStyles(title);
            ScrollIntoView(title);
            TriggerAnimation();

            _observable.Value = _currentSelectedID;

            var pne = new PivotNavigateEvent(_currentSelectedID, tab.Id);

            _navigated?.Invoke(this, pne);

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
                if (el.classList.contains("tss-pivot-keep-cached"))
                {
                    el.classList.add("tss-pivot-cached-hidden");
                }
                else
                {
                    _renderedContent.removeChild(el);
                }
            }
        }

        private void UpdateTitleStyles(HTMLElement title)
        {
            foreach (var v in _renderedTitles.Values)
            {
                if (v == title)
                {
                    v.classList.add("tss-pivot-selected-title");
                    v.setAttribute("aria-selected", "true");
                    v.tabIndex = 0;
                }
                else
                {
                    v.classList.remove("tss-pivot-selected-title");
                    v.setAttribute("aria-selected", "false");
                    v.tabIndex = -1;
                }
            }
            _selectedNav = title;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            if (!_isRendered)
            {
                _isRendered = true; //Sets before calling Select, so it does its thing

                if (_initiallySelectedID != _currentSelectedID)
                {
                    _firstRender = true;
                    Select(_initiallySelectedID);
                }

                _ro = new ResizeObserver((entries, obs) =>
                {
                    UpdateScrollState();
                    TriggerAnimation();
                });

                _ro.observe(StylingContainer);

                DomObserver.WhenMounted(StylingContainer, () =>
                {
                    UpdateScrollState();
                    if (_selectedNav != null) ScrollIntoView(_selectedNav);
                    TriggerAnimation();

                    //Also do on a timeout to account for animations on modals
                    window.setTimeout((_) =>
                    {
                        UpdateScrollState();
                        if (_selectedNav != null) ScrollIntoView(_selectedNav);
                        TriggerAnimation();
                    }, 1000);
                });

                DomObserver.WhenRemoved(StylingContainer, () => _ro.unobserve(StylingContainer));
            }

            return StylingContainer;
        }

        private void TriggerAnimation()
        {
            var target = _hoveredNav ?? _selectedNav;
            if (target is null) return;

            // offsetLeft/offsetWidth are relative to the titlebar, which is the line's
            // offsetParent (it's position: relative). This stays correct regardless of
            // scroll position. CSS transitions on .tss-pivot-line interpolate width/left.
            if (_firstRender)
            {
                // Snap to position on first render so the line doesn't animate in from 0/0.
                _line.classList.add("tss-pivot-line-instant");
                _line.style.width = target.offsetWidth + "px";
                _line.style.left  = target.offsetLeft + "px";
                // Read offsetWidth to flush the layout before re-enabling the transition.
                var _flush = _line.offsetWidth;
                _line.classList.remove("tss-pivot-line-instant");
                _firstRender = false;
                return;
            }

            _line.style.width = target.offsetWidth + "px";
            _line.style.left  = target.offsetLeft + "px";
        }

        internal sealed class Tab
        {
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Tab(string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false, bool closeable = false, Action onClosed = null, Func<Task<bool>> onBeforeClose = null)
            {
                Id               = id;
                _canCacheContent = cached;
                _contentCreator  = contentCreator;
                _titleCreator    = titleCreator;
                Closeable        = closeable;
                OnClosed         = onClosed;
                OnBeforeClose    = onBeforeClose;
            }

            internal bool             KeepCached    => _canCacheContent;
            internal bool             Closeable     { get; }
            internal Action           OnClosed      { get; }
            internal Func<Task<bool>> OnBeforeClose { get; }

            private          Func<IComponent> _titleCreator;
            private          Func<IComponent> _contentCreator;
            private          HTMLElement      _content;
            private readonly bool             _canCacheContent;

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

            /// <summary>
            /// Sets the DOM id of the component.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Renders the title.
            /// </summary>
            public HTMLElement RenderTitle() => _titleCreator().Render();
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