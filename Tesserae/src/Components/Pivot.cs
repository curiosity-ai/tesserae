using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Pivot")]
    public sealed class Pivot : IComponent, ISpecialCaseStyling
    {
        public delegate void                                      PivotEventHandler<TEventArgs>(Pivot sender, TEventArgs e);

        private event PivotEventHandler<PivotBeforeNavigateEvent> _beforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent>       _navigated;
        private readonly List<Tab>                    _orderedTabs    = new List<Tab>();
        private readonly Dictionary<Tab, HTMLElement> _renderedTitles = new Dictionary<Tab, HTMLElement>();
        private readonly HTMLElement _renderedTabs;
        private readonly HTMLElement _renderedContent;
        private readonly HTMLElement _line;
        private readonly HTMLDivElement _scrollCommands;
        private          string      _initiallySelectedID;
        private          string      _currentSelectedID;
        private          bool        _isRendered = false;
        public           string      SelectedTab => _currentSelectedID ?? _initiallySelectedID;
        private HTMLElement _selectedNav;
        private HTMLElement _hoveredNav;
        private double _t0 = 0;
        private double _currentWidth = 0;
        private double _currentLeft = 0;
        private double _targetWidth;
        private double _targetLeft;
        private double _left0;
        private bool _firstRender = false;
        private ResizeObserver _ro;
        private readonly Button _moreBtn;


        public Pivot()
        {
            _moreBtn          = Button().SetIcon(UIcons.MenuDots).NoMinSize().W(32).HS().NoPadding().Class("tss-pivot-titlebar-more").OnClick(() => ShowMoreTabs());
            _line             = Div(_("tss-pivot-line"));
            _renderedTabs     = Div(_("tss-pivot-titlebar"));
            _renderedContent  = Div(_("tss-pivot-content"));
            StylingContainer = Div(_("tss-pivot"), _renderedTabs, _line, _renderedContent);
        }

        public HTMLElement StylingContainer { get; }

        public bool PropagateToStackItemParent => true;

        public Pivot Justified()
        {
            _renderedTabs.style.justifyContent = "space-between";
            return this;
        }

        public Pivot Centered()
        {
            _renderedTabs.style.justifyContent = "center";
            return this;
        }


        public Pivot HideIfSingle()
        {
            _renderedTabs.classList.add("tss-pivot-titlebar-hide-if-single");
            return this;
        }

        internal Pivot Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            _orderedTabs.Add(tab);
            var title = tab.RenderTitle();
            _renderedTitles.Add(tab, title);
            AttachEvents(tab.Id, title);
            _renderedTabs.appendChild(title);

            if (_isRendered && _orderedTabs.Count == 1)
            {
                Select(tab.Id);
            }

            RefreshTabsOverflow();

            return this;
        }

        private void RefreshTabsOverflow(HTMLElement willSelect = null)
        {
            if (!StylingContainer.IsMounted()) return; //Nothing to do yet
            
            if (_moreBtn.IsMounted())
            {
                _moreBtn.Render().remove();
            }

            var moreWidth = 40;
            var maxWidth = _renderedTabs.getBoundingClientRect().As<DOMRect>().width - moreWidth;
            double curWidth = 0;
            bool hasMore = false;
            
            var orderedTitles = _orderedTabs.Select(t => _renderedTitles[t]).ToArray();

            foreach (var rendered in orderedTitles)
            {
                rendered.classList.remove("tss-pivot-titlebar-hidden-overflow");
            }

            orderedTitles = orderedTitles.OrderBy(e => (e == willSelect  || (willSelect is null && e.classList.contains("tss-pivot-selected-title"))) ? 0 : 1).ToArray();

            foreach (var rendered in orderedTitles)
            {
                curWidth += rendered.getBoundingClientRect().As<DOMRect>().width;
                if (curWidth > maxWidth)
                {
                    rendered.classList.add("tss-pivot-titlebar-hidden-overflow");
                    hasMore = true;
                }
                else
                {
                    rendered.classList.remove("tss-pivot-titlebar-hidden-overflow");
                }
            }

            if (hasMore)
            {
                _renderedTabs.appendChild(_moreBtn.Render());
            }
        }

        private void ShowMoreTabs()
        {
            var elementsToClone = _renderedTabs.querySelectorAll(".tss-pivot-titlebar-hidden-overflow").Select(n => n.As<HTMLElement>()).ToArray();
            var cloned = elementsToClone.Select(n => n.cloneNode(true).As<HTMLElement>()).ToArray();
            var items = new ContextMenu.Item[cloned.Length];
            for (int i = 0; i < cloned.Length; i++)
            {
                var iLocal = i;
                HTMLElement c = cloned[i];
                c.classList.remove("tss-pivot-titlebar-hidden-overflow");
                items[i] = ContextMenuItem(Raw(c)).OnClick(() =>
                {
                    RefreshTabsOverflow(elementsToClone[iLocal]);
                    elementsToClone[iLocal].click();
                });
            }

            ContextMenu().Items(items).ShowFor(_moreBtn);
        }

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

        public Pivot OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            _beforeNavigated += onBeforeNavigate;
            return this;
        }

        public Pivot OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            _navigated += onNavigate;
            return this;
        }

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

            HTMLElement content = Div(_());
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

            ClearChildren(_renderedContent);
            _renderedContent.appendChild(content);

            _currentSelectedID = tab.Id;
            UpdateTitleStyles(title);
            TriggerAnimation();

            var pne = new PivotNavigateEvent(_currentSelectedID, tab.Id);

            _navigated?.Invoke(this, pne);

            return this;
        }

        private void UpdateTitleStyles(HTMLElement title)
        {
            foreach (var v in _renderedTitles.Values)
            {
                if (v == title)
                {
                    v.classList.add("tss-pivot-selected-title");
                }
                else
                {
                    v.classList.remove("tss-pivot-selected-title");
                }
            }
            _selectedNav = title;
        }

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

                _ro = new ResizeObserver();
                _ro.Observe(StylingContainer);
                _ro.OnResize = () =>
                {
                    RefreshTabsOverflow(); TriggerAnimation();
                } ;

                DomObserver.WhenMounted(StylingContainer, () =>
                {
                    RefreshTabsOverflow();
                    TriggerAnimation();
                    //Also do on a timeout to account for animations on modals
                    window.setTimeout((_) => {
                        RefreshTabsOverflow();
                        TriggerAnimation();
                    }, 1000);
                });
                
                DomObserver.WhenRemoved(StylingContainer, () => _ro.StopObserving(StylingContainer));
            }

            return StylingContainer;
        }

        private void TriggerAnimation()
        {
            _t0 = -1;
            window.requestAnimationFrame((t) => AnimateLine(t));
        }

        private void AnimateLine(double time)
        {
            if (_t0 < 0)
            {
                var target = _hoveredNav ?? _selectedNav;

                if (target is null) { return; }

                _t0 = time;
                var r = target.getBoundingClientRect().As<DOMRect>();
                _left0       = _renderedTabs.getBoundingClientRect().As<DOMRect>().left;
                _targetWidth = r.width;
                _targetLeft  = r.left;
            }

            var f = (time - _t0) / 500; //500ms animation

            if (_firstRender)
            {
                f            = 1;
                _firstRender = false;
            }

            if (f > 1)
            {
                f = 1;
            }

            _currentWidth          += (_targetWidth - _currentWidth) * f;
            _currentLeft           += (_targetLeft  - _currentLeft)  * f;
            _line.style.width      =  _currentWidth          + "px";
            _line.style.marginLeft =  (_currentLeft - _left0) + "px";

            if (Math.Abs(_currentLeft - _targetLeft) > 1e-5 || Math.Abs(_currentWidth - _targetWidth) > 1e-5)
            {
                window.requestAnimationFrame((t) => AnimateLine(t));
            }
        }

        internal sealed class Tab
        {
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

            public string Id { get; }

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

            public void Cancel() => Canceled = true;
        }

        public abstract class PivotEvent
        {
            internal PivotEvent(string currentPivot, string targetPivot)
            {
                CurrentPivot = currentPivot;
                TargetPivot  = targetPivot;
            }
            public string CurrentPivot { get; }
            public string TargetPivot  { get; }
        }
    }
}
