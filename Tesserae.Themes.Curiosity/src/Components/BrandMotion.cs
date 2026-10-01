using System;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>Small shared pieces for the Curiosity components' drawing and motion.</summary>
    internal static class BrandMotion
    {
        /// <summary>True when the reader asked the system for less motion. Read every time, so a change applies.</summary>
        public static bool PrefersReducedMotion
        {
            get
            {
                try
                {
                    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>The device pixel ratio, capped at 2: sharper than that costs fill rate and shows nothing.</summary>
        public static double PixelRatio => Math.Min(Math.Max(window.devicePixelRatio, 1), 2);

        /// <summary>The page clock in milliseconds.</summary>
        public static double Now() => performance.now();

        /// <summary>Hermite smoothstep of p, clamped to 0..1.</summary>
        public static double Ease(double p)
        {
            p = Math.Max(0, Math.Min(1, p));
            return p * p * (3 - 2 * p);
        }

        /// <summary>
        /// The mount wiring of a canvas component that animates only while it is on screen. Each time the element is
        /// mounted it calls <paramref name="mounted"/> (which draws and starts the loop), redraws on every resize and
        /// reports visibility changes to <paramref name="seen"/>; when it is removed the observers are dropped,
        /// <paramref name="removed"/> runs and it waits for the next mount.
        /// </summary>
        public static void WhileMounted(HTMLElement element, Action mounted, Action draw, Action<bool> seen, Action removed)
        {
            void OnMounted()
            {
                mounted();

                var resize = new ResizeObserver((entries, obs) => draw());
                resize.observe(element);

                var visibility = new IntersectionObserver((entries, obs) => seen(entries[entries.Length - 1].isIntersecting));
                visibility.observe(element);

                DomObserver.WhenRemoved(element, () =>
                {
                    removed();
                    resize.disconnect();
                    visibility.disconnect();
                    DomObserver.WhenMounted(element, OnMounted);
                });
            }

            DomObserver.WhenMounted(element, OnMounted);
        }

        /// <summary>
        /// Sizes a canvas's backing store to its box at the device pixel ratio and returns its 2D context with the
        /// ratio applied, so drawing is in CSS pixels. Returns null while the box is too small to draw into.
        /// </summary>
        public static CanvasRenderingContext2D Fit(HTMLCanvasElement canvas, out double width, out double height)
        {
            var box = (DOMRect)canvas.getBoundingClientRect();
            width  = box.width;
            height = box.height;
            if (width < 2 || height < 2) return null;

            var dpr = PixelRatio;
            var w   = (uint)Math.Round(width  * dpr);
            var h   = (uint)Math.Round(height * dpr);
            if (canvas.width != w)  canvas.width  = w;
            if (canvas.height != h) canvas.height = h;

            var ctx = canvas.getContext("2d").As<CanvasRenderingContext2D>();
            if (ctx is null) return null;
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            return ctx;
        }
    }

    /// <summary>
    /// The website's seeded generator (a 32-bit linear congruential one), so a drawing made from a seed is the same
    /// arrangement every time rather than a new accident. Kept in doubles: every intermediate value stays under
    /// 2^53, so it is exact in JavaScript's numbers without relying on integer overflow.
    /// </summary>
    internal sealed class SeededRandom
    {
        private double _state;

        public SeededRandom(double seed)
        {
            _state = seed % 4294967296;
            if (_state <= 0) _state = 1;
        }

        /// <summary>The next value in 0..1.</summary>
        public double Next()
        {
            _state = (_state * 1664525 + 1013904223) % 4294967296;
            return _state / 4294967296;
        }
    }
}
