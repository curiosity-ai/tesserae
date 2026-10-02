using System;
using System.Collections.Generic;
using Transpose.Core;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>
    /// Tells the stylesheet which scroller is moving. CSS has no selector for "is scrolling", and the
    /// hairline scrollbar is ink only while it is in use, so this puts <see cref="ActiveClass"/> on the
    /// element that raised a <c>scroll</c> event and takes it off again <see cref="IdleMs"/> after the
    /// last one.
    ///
    /// <para>
    /// One capture-phase listener on <c>window</c> sees every scroll event on the page (they do not
    /// bubble, but they are captured), so it reaches a scroller written in an application with nothing
    /// for that application to declare. A scroll of the document marks <c>html</c> and <c>body</c>,
    /// the two elements the engines read the page scrollbar from.
    /// </para>
    /// </summary>
    internal static class ScrollActivity
    {
        /// <summary>The class on a scroller while it moves. The stylesheet's scrollbar rules key on it.</summary>
        internal const string ActiveClass = "tss-cur-scrolling";

        /// <summary>How long after the last scroll event the scroller still counts as moving.</summary>
        private const double IdleMs = 600;

        /// <summary>How often idle scrollers are looked for while any is marked.</summary>
        private const double SweepMs = 150;

        private sealed class Scroller
        {
            internal HTMLElement Element;
            internal double      LastMs;
        }

        // Rarely more than one or two entries, so a list and a reference scan beat any lookup structure.
        private static readonly List<Scroller> _scrollers = new List<Scroller>();
        private static          Action<Event>  _onScroll;
        private static          double         _sweepTimer = -1;

        internal static void Start()
        {
            if (_onScroll != null) return;

            _onScroll = OnScroll;
            window.addEventListener("scroll", _onScroll, true);
        }

        internal static void Stop()
        {
            if (_onScroll == null) return;

            window.removeEventListener("scroll", _onScroll, true);
            _onScroll = null;

            if (_sweepTimer >= 0) { window.clearTimeout(_sweepTimer); _sweepTimer = -1; }

            foreach (var scroller in _scrollers) scroller.Element.classList.remove(ActiveClass);
            _scrollers.Clear();
        }

        private static void OnScroll(Event e)
        {
            if (e.target == document)
            {
                Mark(document.documentElement);
                Mark(document.body);
            }
            else
            {
                Mark(e.target.As<HTMLElement>());
            }
        }

        private static void Mark(HTMLElement element)
        {
            if (element is null) return;

            var now = Transpose.Core.es5.Date.now();

            foreach (var scroller in _scrollers)
            {
                if (scroller.Element == element)
                {
                    scroller.LastMs = now;
                    return;
                }
            }

            element.classList.add(ActiveClass);
            _scrollers.Add(new Scroller { Element = element, LastMs = now });

            if (_sweepTimer < 0) _sweepTimer = window.setTimeout(_ => Sweep(), SweepMs);
        }

        private static void Sweep()
        {
            _sweepTimer = -1;

            var now = Transpose.Core.es5.Date.now();

            for (var i = _scrollers.Count - 1; i >= 0; i--)
            {
                if (now - _scrollers[i].LastMs < IdleMs) continue;

                _scrollers[i].Element.classList.remove(ActiveClass);
                _scrollers.RemoveAt(i);
            }

            if (_scrollers.Count > 0) _sweepTimer = window.setTimeout(_ => Sweep(), SweepMs);
        }
    }
}
