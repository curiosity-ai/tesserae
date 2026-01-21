using System;
using static Tesserae.UI;
using static H5.Core.dom;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// A VisibilitySensor component that triggers an action when it becomes visible in the viewport.
    /// </summary>
    [H5.Name("tss.VisibilitySensor")]
    public class VisibilitySensor : IComponent
    {
        private readonly HTMLElement              InnerElement;
        private readonly double                   _debounceTimeout = 50;
        private          double                   _debounce;
        private readonly Action<VisibilitySensor> _onVisible;
        private          int                      _maxCalls;
        private          IntersectionObserver     _observer;

        /// <summary>
        /// Initializes a new instance of the VisibilitySensor class.
        /// </summary>
        /// <param name="onVisible">The action to perform when the sensor becomes visible.</param>
        /// <param name="singleCall">Whether to trigger the action only once.</param>
        /// <param name="message">An optional component to display as the content of the sensor.</param>
        public VisibilitySensor(Action<VisibilitySensor> onVisible, bool singleCall = true, IComponent message = null)
        {
            InnerElement = DIV();

            if (message is object)
            {
                InnerElement.appendChild(message.Render());
            }

            _onVisible = onVisible;

            _maxCalls = singleCall ? 1 : int.MaxValue;

            DomObserver.WhenMounted(InnerElement, HookCheck);
        }

        /// <summary>
        /// Renders the visibility sensor.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => InnerElement;

        /// <summary>
        /// Resets the sensor so it can trigger the action again.
        /// </summary>
        public void Reset()
        {
            DomObserver.WhenMounted(InnerElement, HookCheck);

            if (_maxCalls < 1) //will only reach 0 if it was single call
            {
                _maxCalls = 1;
            }
        }

        private void HookCheck()
        {
            IntersectionObserverCallback observerListener = (entries, obs) =>
            {
                if (entries.Any(e => e.isIntersecting))
                {
                    OnScroll(null);
                }
            };

            _observer = new IntersectionObserver(observerListener);
            _observer.observe(InnerElement);

            window.addEventListener("focus", OnScroll, true);
            DomObserver.WhenRemoved(InnerElement, UnHookCheck);
            //Trigger one time on first render, to force check if visible
            OnScroll(null);
        }

        private void UnHookCheck()
        {
            window.removeEventListener("focus", OnScroll);
            _observer?.disconnect();
            _observer = null;
        }

        private void OnScroll(Event ev)
        {
            window.clearTimeout(_debounce);
            _debounce = window.setTimeout(CheckVisibility, _debounceTimeout);
        }

        private void CheckVisibility(object t)
        {
            var viewport_top    = window.scrollY;
            var viewport_bottom = window.scrollY + window.innerHeight;
            var rect            = (DOMRect)InnerElement.getBoundingClientRect();

            if (rect.top > viewport_top && rect.bottom < viewport_bottom)
            {
                if (_maxCalls > 0)
                {
                    _maxCalls--;
                    _onVisible(this);
                }

                if (_maxCalls == 0)
                {
                    UnHookCheck();
                }
            }
        }
    }
}