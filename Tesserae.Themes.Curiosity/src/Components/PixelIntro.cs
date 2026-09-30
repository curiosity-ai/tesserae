using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>The two first-load intros the website has.</summary>
    public enum PixelIntroStyle
    {
        /// <summary>
        /// The website's default (the Blend style): the brand's square field on ink. Paper squares grow in on a
        /// diagonal sweep, turn toward the Signal the way the field bends round it, the Signal square lands last,
        /// then they shrink away in the same order and the ink lifts off the page. Under two seconds.
        /// </summary>
        SquareField,

        /// <summary>
        /// The brand style's intro: the screen is laid with ink and slate tiles from the corner, then they are
        /// taken away in the same order, uncovering the page; the escaped Signal square, top right, leaves last.
        /// About a second and a half.
        /// </summary>
        Tiles
    }

    /// <summary>
    /// The website's first-load animation, drawn over the whole window and removed from the document when it ends.
    /// It is decoration: never interactive, hidden from assistive technology, and skipped entirely when the
    /// reader asked for reduced motion.
    /// </summary>
    public static class PixelIntro
    {
        private const string PlayedKey = "curiosity:intro-played";

        /// <summary>
        /// Plays the intro the first time it is called in a tab, as the website does: a reload plays it again,
        /// navigating within the app does not. Call it once, as the app starts. Completes when the intro has
        /// finished (at once when it does not play).
        /// </summary>
        public static Task PlayOnce(PixelIntroStyle style = PixelIntroStyle.SquareField)
        {
            try
            {
                if (sessionStorage.getItem(PlayedKey) is object && !IsReload()) return Task.CompletedTask;
                sessionStorage.setItem(PlayedKey, "1");
            }
            catch
            {
                // No session storage (a sandboxed frame, storage blocked): the intro is not worth an error.
                return Task.CompletedTask;
            }

            return Play(style);
        }

        /// <summary>
        /// Plays the intro now, whether or not it already played (for a preview). Still skipped under reduced
        /// motion. Completes when it has finished.
        /// </summary>
        public static Task Play(PixelIntroStyle style = PixelIntroStyle.SquareField)
        {
            if (BrandMotion.PrefersReducedMotion || document.body is null) return Task.CompletedTask;

            var done = new TaskCompletionSource<bool>();

            if (style == PixelIntroStyle.Tiles) PlayTiles(() => done.TrySetResult(true));
            else                                    PlaySquareField(() => done.TrySetResult(true));

            return done.Task;
        }

        private static bool IsReload()
        {
            return Script.Write<bool>("(function(){ try { var n = performance.getEntriesByType('navigation')[0]; if (n && n.type) return n.type === 'reload'; return !!(performance.navigation && performance.navigation.type === 1); } catch (e) { return false; } })()");
        }

        private static HTMLCanvasElement Overlay(out CanvasRenderingContext2D ctx, out double width, out double height)
        {
            var canvas = new HTMLCanvasElement();
            canvas.setAttribute("aria-hidden", "true");
            canvas.className                      = "tss-pixel-intro";
            canvas.style.position                 = "fixed";
            canvas.style.left                     = "0";
            canvas.style.top                      = "0";
            canvas.style.width                    = "100vw";
            canvas.style.height                   = "100vh";
            canvas.style.zIndex                   = "2147483000";
            canvas.style.pointerEvents            = "none";
            document.body.appendChild(canvas);

            ctx = BrandMotion.Fit(canvas, out width, out height);
            return canvas;
        }

        /* The square field (BRAND.md s6) as the intro. On ink, separated paper squares grow in on a diagonal sweep,
           then turn toward the signal the way the field bends round it, and the Signal square lands last. Then the
           squares shrink away in the same order and the ink lifts off the page. */
        private static void PlaySquareField(Action onDone)
        {
            var canvas = Overlay(out var ctx, out var W, out var H);
            if (ctx is null) { canvas.remove(); onDone(); return; }

            var pitch = W < 640 ? 36.0 : 50.0;
            var cols  = (int)Math.Ceiling(W / pitch);
            var rows  = (int)Math.Ceiling(H / pitch);
            var side  = pitch * 0.4;
            var sigma = 0.3 * Math.Max(W, H);
            var sc    = (int)Math.Round(cols * 0.62);
            var sr    = (int)Math.Round(rows * 0.45);
            var ax    = sc * pitch + pitch / 2;
            var ay    = sr * pitch + pitch / 2;

            var count = rows * cols;
            var xs = new double[count]; var ys = new double[count]; var ws = new double[count];
            var ds = new double[count]; var al = new double[count]; var sg = new bool[count];

            for (int r = 0, n = 0; r < rows; r++)
            {
                for (int k = 0; k < cols; k++, n++)
                {
                    var x = k * pitch + pitch / 2; var y = r * pitch + pitch / 2;
                    var dx = x - ax; var dy = y - ay;
                    xs[n] = x; ys[n] = y;
                    ws[n] = Math.Exp(-(dx * dx + dy * dy) / (2 * sigma * sigma));
                    ds[n] = (double)(k + r) / (cols + rows);
                    al[n] = 0.22 + 0.78 * ((double)k / Math.Max(1, cols - 1));
                    sg[n] = k == sc && r == sr;
                }
            }

            const double IN = 560, TURN = 380, HOLD = 220, OUT = 520;
            const double GO = IN + TURN + HOLD, END = GO + OUT;
            var t0 = BrandMotion.Now();

            FrameRequestCallback frame = null;
            frame = now =>
            {
                var t = now - t0;
                ctx.clearRect(0, 0, W, H);

                // The ink lifts off during the last part of the way out.
                ctx.globalAlpha = 1 - BrandMotion.Ease((t - GO - OUT * 0.35) / (OUT * 0.65));
                ctx.fillStyle   = BrandColors.Ink;
                ctx.fillRect(0, 0, W, H);

                var turn = BrandMotion.Ease((t - IN * 0.6) / TURN);

                for (int i = 0; i < count; i++)
                {
                    var grow   = BrandMotion.Ease((t - ds[i] * IN * 0.7) / (IN * 0.3));
                    var shrink = BrandMotion.Ease((t - GO - ds[i] * OUT * 0.55) / (OUT * 0.3));

                    if (sg[i])
                    {
                        grow   = BrandMotion.Ease((t - IN * 0.85) / 160);
                        shrink = BrandMotion.Ease((t - END + 200) / 200);
                    }

                    var s = side * grow * (1 - shrink);
                    if (s < 0.5) continue;

                    ctx.globalAlpha = sg[i] ? 1 : al[i];
                    ctx.fillStyle   = sg[i] ? BrandColors.Signal : BrandColors.Paper;
                    ctx.save();
                    ctx.translate(xs[i], ys[i]);
                    if (!sg[i]) ctx.rotate(Math.PI / 4 * ws[i] * turn);
                    ctx.fillRect(-s / 2, -s / 2, s, s);
                    ctx.restore();
                }

                ctx.globalAlpha = 1;

                if (t < END)
                {
                    window.requestAnimationFrame(frame);
                }
                else
                {
                    canvas.remove();
                    onDone();
                }
            };

            window.requestAnimationFrame(frame);
        }

        /* The tile sweep: each tile is shown for an interval, on at its place in the sweep in and off at its place in
           the sweep out, in the same order, so the page is uncovered from the corner the sweep started from. */
        private static void PlayTiles(Action onDone)
        {
            var canvas = Overlay(out var ctx, out var W, out var H);
            if (ctx is null) { canvas.remove(); onDone(); return; }

            var tones = new[] { BrandColors.Ink, BrandColors.Ink, BrandColors.Slate };
            var rnd   = new SeededRandom(20260923);   // the same arrangement every time
            var cell  = W < 640 ? 48.0 : 80.0;
            var cols  = (int)Math.Ceiling(W / cell);
            var rows  = (int)Math.Ceiling(H / cell);
            var count = rows * cols;
            var fill  = new string[count];
            var delay = new double[count];

            for (int j = 0, n = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++, n++)
                {
                    fill[n]  = tones[(int)(rnd.Next() * tones.Length)];
                    delay[n] = i + j + rnd.Next() * 2.5;
                }
            }

            // The escaped square: top right, in Signal, the last to leave.
            fill[cols - 1]  = BrandColors.Signal;
            delay[cols - 1] = cols + rows + 2.5;

            var span = cols + rows + 2.5;
            const double IN = 620, HOLD = 160, OUT = 620;
            var t0 = BrandMotion.Now();

            FrameRequestCallback frame = null;
            frame = now =>
            {
                var t = now - t0;
                ctx.clearRect(0, 0, W, H);

                for (int n = 0; n < count; n++)
                {
                    var at = delay[n] / span;
                    if (t < at * IN || t > IN + HOLD + at * OUT) continue;
                    ctx.fillStyle = fill[n];
                    ctx.fillRect((n % cols) * cell, (n / cols) * cell, cell, cell);
                }

                if (t < IN + HOLD + OUT + 120)
                {
                    window.requestAnimationFrame(frame);
                }
                else
                {
                    canvas.remove();
                    onDone();
                }
            };

            window.requestAnimationFrame(frame);
        }
    }
}
