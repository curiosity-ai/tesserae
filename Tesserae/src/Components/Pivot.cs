﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class Pivot : IComponent, ISpecialCaseStyling
    {
        private event PivotEventHandler<PivotBeforeNavigateEvent> BeforeNavigated;
        private event PivotEventHandler<PivotNavigateEvent> Navigated;
        public delegate void PivotEventHandler<TEventArgs>(Pivot sender, TEventArgs e);

        private readonly List<Tab> OrderedTabs = new List<Tab>();
        private readonly Dictionary<Tab, HTMLElement> RenderedTitles = new Dictionary<Tab, HTMLElement>();

        private readonly HTMLElement RenderedTabs;
        private readonly HTMLElement RenderedContent;
        private readonly HTMLElement Line;
        private string _initiallySelectedID;
        private string _currentSelectedID;
        private bool _isRendered = false;
        public string SelectedTab => _currentSelectedID ?? _initiallySelectedID;

        public Pivot()
        {
            Line = Div(_("tss-pivot-line"));
            RenderedTabs = Div(_("tss-pivot-titlebar"));
            RenderedContent = Div(_("tss-pivot-content"));
            StylingContainer = Div(_("tss-pivot"), RenderedTabs, Line, RenderedContent);
        }

        public HTMLElement StylingContainer { get; }

        public bool PropagateToStackItemParent => true;

        public Pivot Justified()
        {
            RenderedTabs.style.justifyContent = "space-between";
            return this;
        }
        public Pivot HideIfSingle()
        {
            RenderedTabs.classList.add("tss-pivot-titlebar-hide-if-single");
            return this;
        }

        internal Pivot Add(Tab tab)
        {
            if (_initiallySelectedID is null) _initiallySelectedID = tab.Id;
            OrderedTabs.Add(tab);
            var title = tab.RenderTitle();
            RenderedTitles.Add(tab, title);
            AttachEvents(tab.Id, title);
            RenderedTabs.appendChild(title);
            return this;
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
                HoveredNav = title;
                TriggerAnimation();
            };

            title.onmouseleave = e =>
            {
                if (HoveredNav == title)
                {
                    HoveredNav = null;
                    TriggerAnimation();
                }
            };
        }

        public Pivot OnBeforeNavigate(PivotEventHandler<PivotBeforeNavigateEvent> onBeforeNavigate)
        {
            BeforeNavigated += onBeforeNavigate;
            return this;
        }

        public Pivot OnNavigate(PivotEventHandler<PivotNavigateEvent> onNavigate)
        {
            Navigated += onNavigate;
            return this;
        }

        public Pivot Select(string id, bool refresh = false)
        {
            if (_currentSelectedID != id || refresh)
            {
                var tab = OrderedTabs.FirstOrDefault(t => t.Id == id);
                Select(tab);
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

            BeforeNavigated?.Invoke(this, pbne);

            if (pbne.Canceled) return this;

            var title = RenderedTitles[tab];

            HTMLElement content = Div(_());
            content.style.width = "100%";
            content.style.minHeight = "100%";

            try
            {
                content = tab.RenderContent();
            }
            catch (Exception E)
            {
                content.textContent = E.ToString();
            }

            ClearChildren(RenderedContent);
            RenderedContent.appendChild(content);

            _currentSelectedID = tab.Id;
            UpdateTitleStyles(title);
            TriggerAnimation();

            var pne = new PivotNavigateEvent(_currentSelectedID, tab.Id);

            Navigated?.Invoke(this, pne);

            return this;
        }

        private void UpdateTitleStyles(HTMLElement title)
        {
            foreach (var v in RenderedTitles.Values)
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
            SelectedNav = title;
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

                var ro = new ResizeObserver();
                ro.Observe(StylingContainer);
                ro.OnResize = () => TriggerAnimation();
            }

            return StylingContainer;
        }

        private void TriggerAnimation()
        {
            T0 = -1;
            window.requestAnimationFrame((t) => AnimateLine(t));
        }

        private HTMLElement SelectedNav;
        private HTMLElement HoveredNav;
        private double T0 = 0;
        private double CurrentWidth = 0;
        private double CurrentLeft = 0;
        private double TargetWidth;
        private double TargetLeft;
        private double Left0;
        private bool _firstRender = false;

        private void AnimateLine(double time)
        {
            if (T0 < 0)
            {
                var target = HoveredNav ?? SelectedNav;
                if (target is null) { return; }
                T0 = time;
                var r = (DOMRect)target.getBoundingClientRect();
                TargetWidth = r.width;
                TargetLeft = r.left;
                Left0 = ((DOMRect)RenderedTabs.getBoundingClientRect()).left;
            }

            var f = (time - T0) / 500; //500ms animation
            if (_firstRender)
            {
                f = 1;
                _firstRender = false;
            }

            if (f > 1)
            {
                f = 1;
            }

            CurrentWidth += (TargetWidth - CurrentWidth) * f;
            CurrentLeft += (TargetLeft - CurrentLeft) * f;
            Line.style.width = CurrentWidth + "px";
            Line.style.marginLeft = (CurrentLeft - Left0) + "px";
            
            if (Math.Abs(CurrentLeft - TargetLeft) > 1e-5 || Math.Abs(CurrentWidth - TargetWidth) > 1e-5)
            {
                window.requestAnimationFrame((t) => AnimateLine(t));
            }
        }

        internal sealed class Tab
        {
            public Tab(string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
            {
                Id = id;
                CanCacheContent = cached;
                ContentCreator = contentCreator;
                TitleCreator = titleCreator;
            }
            public string Id { get; private set; }
            private Func<IComponent> TitleCreator { get; }
            private Func<IComponent> ContentCreator { get; }

            private HTMLElement Content;
            private readonly bool CanCacheContent;

            public HTMLElement RenderContent()
            {
                if (CanCacheContent && Content is object)
                {
                    return Content;
                }
                else
                {
                    Content = ContentCreator().Render();
                    return Content;
                }
            }

            public HTMLElement RenderTitle()
            {
                return TitleCreator().Render();
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