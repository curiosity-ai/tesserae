using System;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>
    /// The four grounds the website draws its dash tiles on (the landing page's industry tiles and the Studio and
    /// Developers panels). Each is a background plus three dash tones: dim (the bars at rest), bright (lit bars)
    /// and accent (the bars a shape picks out).
    /// </summary>
    public enum DashMosaicScheme
    {
        /// <summary>The Signal ground: deep blue bars at rest, paper bars lit (Aerospace &amp; Defense).</summary>
        LightBlue,
        /// <summary>The ink ground: slate bars at rest, paper bars lit, the Signal as accent (Rail &amp; Mobility, the Studio panel).</summary>
        Dark,
        /// <summary>The deep blue ground: Signal bars at rest, paper bars lit (Industrial Manufacturing, the Developers panel).</summary>
        DarkBlue,
        /// <summary>The stone ground: pale bars at rest, ink bars lit, the deep blue as accent (Energy &amp; Infrastructure).</summary>
        Light
    }

    /// <summary>Which bars a <see cref="DashMosaic"/> lights, and how that moves.</summary>
    public enum DashMosaicPattern
    {
        /// <summary>Every bar lit, with a line of dim bars sweeping across on the diagonal.</summary>
        Wave,
        /// <summary>Rows lit in runs that slide, like a scanning display.</summary>
        Rows,
        /// <summary>A pixel line chart in the accent, scrolling left, on an empty ground.</summary>
        Chart,
        /// <summary>The same line chart over a grid of dim bars.</summary>
        ChartGrid,
        /// <summary>Everything under a slowly moving skyline lit, its left edge in the accent.</summary>
        Mountain,
        /// <summary>A slanted band of accent bars, swaying.</summary>
        Band,
        /// <summary>Dim bars everywhere, the accent collecting in a drifting patch.</summary>
        Dither
    }

    /// <summary>
    /// The website's animated dash tiles: a grid of short vertical bars in which a shape is made by which bars are
    /// lit. It fills its own box, with the website's spacing (11px across, 17px down, 3 by 9px bars; smaller under
    /// 260px wide) and at least 6px clear of every edge, the grid centred in what is left.
    /// <para>
    /// Drawn on a canvas at the size it is shown, animated at 20 frames a second only while on screen. With
    /// animation off, or reduced motion, it is drawn once and holds.
    /// </para>
    /// </summary>
    public sealed class DashMosaic : IComponent
    {
        private readonly HTMLElement       _container;
        private readonly HTMLCanvasElement _canvas;

        private DashMosaicScheme _scheme;
        private DashMosaicPattern _pattern;
        private bool                   _animated = true;
        private double                 _seed     = 7;
        private double                 _padding  = 6;
        private double                 _fps      = 20;

        private double[] _line;
        private double   _t0;
        private bool     _mounted, _seen = true, _looping;

        private ResizeObserver       _resize;
        private IntersectionObserver _visibility;

        /// <summary>Creates stripes in a scheme, with the pattern the website pairs with it unless one is given.</summary>
        public DashMosaic(DashMosaicScheme scheme = DashMosaicScheme.Dark, DashMosaicPattern? pattern = null)
        {
            _scheme = scheme;
            _pattern = pattern ?? DefaultPattern(scheme);

            _canvas = new HTMLCanvasElement();
            _canvas.setAttribute("aria-hidden", "true");
            _canvas.style.position = "absolute";
            _canvas.style.left     = "0";
            _canvas.style.top      = "0";
            _canvas.style.width    = "100%";
            _canvas.style.height   = "100%";
            _canvas.style.display  = "block";

            _container = Div(Att("tss-dash-mosaic"), _canvas);
            _container.setAttribute("aria-hidden", "true");
            _container.style.position = "relative";
            _container.style.overflow = "hidden";
            _container.style.width    = "100%";
            _container.style.height   = "200px";
            ApplyBackground();

            DomObserver.WhenMounted(_container, OnMounted);
        }

        /// <summary>The pattern the website uses with each scheme: a wave on the Signal, scanning rows on ink, the dither patch on deep blue, the line chart on stone.</summary>
        public static DashMosaicPattern DefaultPattern(DashMosaicScheme scheme)
        {
            switch (scheme)
            {
                case DashMosaicScheme.LightBlue: return DashMosaicPattern.Wave;
                case DashMosaicScheme.DarkBlue:  return DashMosaicPattern.Dither;
                case DashMosaicScheme.Light:     return DashMosaicPattern.Chart;
                default:                               return DashMosaicPattern.Rows;
            }
        }

        /// <summary>Sets the colour scheme.</summary>
        public DashMosaic Scheme(DashMosaicScheme scheme)
        {
            _scheme = scheme;
            ApplyBackground();
            Draw();
            return this;
        }

        /// <summary>Sets which bars are lit, and how that moves.</summary>
        public DashMosaic Pattern(DashMosaicPattern pattern)
        {
            _pattern = pattern;
            Draw();
            return this;
        }

        /// <summary>Turns the motion on or off. On by default; reduced motion turns it off whatever this says.</summary>
        public DashMosaic Animated(bool animated = true)
        {
            _animated = animated;
            Draw();
            EnsureLoop();
            return this;
        }

        /// <summary>Turns off the animation. Same as <c>Animated(false)</c>.</summary>
        public DashMosaic NoAnimation() => Animated(false);

        /// <summary>The seed of the random line the chart and mountain shapes follow; the same seed draws the same line.</summary>
        public DashMosaic Seed(int seed)
        {
            _seed = seed;
            _line = null;
            Draw();
            return this;
        }

        /// <summary>The least clear space kept between the bars and every edge, in pixels. 6 by default, as on the website.</summary>
        public DashMosaic EdgePadding(double padding)
        {
            _padding = Math.Max(0, padding);
            Draw();
            return this;
        }

        /// <summary>Frames per second while animating. 20 by default, as on the website.</summary>
        public DashMosaic FramesPerSecond(double fps)
        {
            _fps = Math.Max(1, Math.Min(60, fps));
            return this;
        }

        /// <inheritdoc />
        public HTMLElement Render() => _container;

        private bool IsAnimated => _animated && !BrandMotion.PrefersReducedMotion;

        private void ApplyBackground() => _container.style.background = Tones(_scheme)[0];

        // Background, dim, bright, accent.
        private static string[] Tones(DashMosaicScheme scheme)
        {
            switch (scheme)
            {
                case DashMosaicScheme.LightBlue: return new[] { BrandColors.Signal, BrandColors.Deep,   BrandColors.Paper, BrandColors.Paper };
                case DashMosaicScheme.DarkBlue:  return new[] { BrandColors.Deep,   BrandColors.Signal, BrandColors.Paper, BrandColors.Paper };
                case DashMosaicScheme.Light:     return new[] { BrandColors.Stone,  "#D6D9DA",              BrandColors.Ink,   BrandColors.Deep };
                default:                               return new[] { BrandColors.Ink,    BrandColors.Slate,  BrandColors.Paper, BrandColors.Signal };
            }
        }

        private void OnMounted()
        {
            _mounted = true;
            _t0      = BrandMotion.Now();

            _resize = new ResizeObserver((entries, obs) => Draw());
            _resize.observe(_container);

            _visibility = new IntersectionObserver((entries, obs) =>
            {
                _seen = entries[entries.Length - 1].isIntersecting;
                EnsureLoop();
            });
            _visibility.observe(_container);

            Draw();
            EnsureLoop();

            DomObserver.WhenRemoved(_container, () =>
            {
                _mounted = false;
                _resize?.disconnect();
                _visibility?.disconnect();
                _resize     = null;
                _visibility = null;
                DomObserver.WhenMounted(_container, OnMounted);
            });
        }

        private void EnsureLoop()
        {
            if (_looping || !_mounted || !_seen || !IsAnimated) return;

            _looping = true;
            double last = 0;
            FrameRequestCallback tick = null;
            tick = now =>
            {
                if (!_mounted || !_seen || !IsAnimated) { _looping = false; return; }
                if (now - last >= 1000 / _fps) { last = now; Draw(); }
                window.requestAnimationFrame(tick);
            };
            window.requestAnimationFrame(tick);
        }

        private void Draw()
        {
            if (!_mounted) return;
            var ctx = BrandMotion.Fit(_canvas, out var W, out var H);
            if (ctx is null) return;

            // Without motion the drawing stands at four seconds in, as the website's does.
            var t = IsAnimated ? (BrandMotion.Now() - _t0) / 1000 : 4;

            double dx = 11, dy = 17, bw = 3, bh = 9;
            if (W < 260) { dx = 8; dy = 13; bw = 2; bh = 7; }

            var cols = Math.Max(1, (int)Math.Floor((W - 2 * _padding) / dx));
            var rows = Math.Max(1, (int)Math.Floor((H - 2 * _padding) / dy));
            var ox   = (W - (cols - 1) * dx - bw) / 2;
            var oy   = (H - (rows - 1) * dy - bh) / 2;

            if (_line is null) _line = Walk(new SeededRandom(_seed), 90);

            var tones = Tones(_scheme);
            ctx.clearRect(0, 0, W, H);

            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    var u = cols > 1 ? (double)i / (cols - 1) : 0;
                    var v = rows > 1 ? (double)j / (rows - 1) : 0;
                    var k = Value(u, v, t, i, j, cols, rows);
                    if (k < 0) continue;

                    ctx.fillStyle = tones[k + 1];
                    ctx.fillRect(Math.Round(ox + i * dx), Math.Round(oy + j * dy), bw, bh);
                }
            }
        }

        // 0 dim, 1 bright, 2 accent, -1 not drawn: the website's mosaic patterns.
        private int Value(double u, double v, double t, int i, int j, int cols, int rows)
        {
            switch (_pattern)
            {
                case DashMosaicPattern.Wave:
                {
                    var d = (u + v) / 2 - ((t * 0.12) % 1.4 - 0.2);
                    return Math.Abs(d) < 0.05 ? 0 : 1;
                }
                case DashMosaicPattern.Rows:
                {
                    var s = Math.Sin(j * 12.9898) * 43758.5453; s -= Math.Floor(s);
                    var len = 0.2 + s * 0.5;
                    var at  = ((t * (0.04 + s * 0.06) + s) % 1.4) - 0.3;
                    return (u > at && u < at + len && (j % 3 != 1)) ? 1 : 0;
                }
                case DashMosaicPattern.Chart:
                case DashMosaicPattern.ChartGrid:
                {
                    var y   = Sample(_line, (u * 0.55 + t * 0.018) % 1);
                    var row = (int)Math.Round(y * (rows - 1));
                    if (j == row) return 2;
                    return _pattern == DashMosaicPattern.ChartGrid ? 0 : -1;
                }
                case DashMosaicPattern.Mountain:
                {
                    var y = Sample(_line, (u * 0.5 + t * 0.012) % 1) * 0.8 + 0.12;
                    if (v < y) return -1;
                    return u < 0.22 ? 2 : 1;
                }
                case DashMosaicPattern.Band:
                {
                    var c = 0.15 + 0.7 * (1 - v) + Math.Sin(t * 0.4) * 0.05;
                    return Math.Abs(u - c) < 0.16 ? 2 : -1;
                }
                default: // Dither
                {
                    var cx = 0.5 + Math.Sin(t * 0.25) * 0.25;
                    var cy = 0.5 + Math.Cos(t * 0.2) * 0.2;
                    var d  = Math.Sqrt((u - cx) * (u - cx) + (v - cy) * 1.2 * (v - cy) * 1.2);
                    var n  = Math.Sin(u * 91.7 + v * 47.3) * 43758.5; n -= Math.Floor(n);
                    return d < 0.28 + n * 0.08 ? 2 : 0;
                }
            }
        }

        // A smooth random line in 0..1, the one the chart and the mountain follow.
        private static double[] Walk(SeededRandom r, int n)
        {
            var p = new double[n];
            double y = 0.5, v = 0;
            for (int i = 0; i < n; i++)
            {
                v    = v * 0.7 + (r.Next() - 0.5) * 0.35;
                y    = Math.Max(0.12, Math.Min(0.88, y + v * 0.3));
                p[i] = y;
            }
            return p;
        }

        private static double Sample(double[] p, double u)
        {
            var x = u * (p.Length - 1);
            var i = (int)Math.Floor(x);
            var f = x - i;
            var a = p[i];
            var b = i + 1 < p.Length ? p[i + 1] : a;
            return a + (b - a) * f;
        }
    }
}
