using System;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>The ground a <see cref="GeneratingImage"/> is drawn on.</summary>
    public enum GeneratingImageSurface
    {
        /// <summary>Ink, with Signal pixels: the brand's dark canvas.</summary>
        Ink,
        /// <summary>Paper, with Signal pixels: the brand's light canvas.</summary>
        Paper
    }

    /// <summary>
    /// The placeholder for an image that is still being made, as a chat shows it: a field of square pixels in the
    /// Signal that swell and fade in rolling waves from the middle of the frame, with the pixels near the edges
    /// small and quiet. An optional badge in the bottom right corner reports progress.
    /// <para>
    /// It takes its shape from an aspect ratio (<c>new GeneratingImage(16, 9)</c>, or a single ratio) and, by default,
    /// is half as wide as the element it sits in: in a chat message that is allowed the full width of the chat area,
    /// it leaves room beside it, as a picture in a conversation does. <see cref="WidthFraction"/> changes the share,
    /// and an explicit <c>.W(...)</c> replaces it. The height follows from the width and the ratio, so the frame is
    /// the same shape the finished image will be and nothing moves when it arrives.
    /// </para>
    /// <para>
    /// Drawn on a canvas at the size it is shown, redrawn when that size changes, and animated only while on screen.
    /// Reduced motion gets a still frame. A reported progress also fills the field: the pixels start small and
    /// grow toward the full size as it nears 100.
    /// </para>
    /// </summary>
    public sealed class GeneratingImage : IComponent
    {
        private readonly HTMLElement       _container;
        private readonly HTMLCanvasElement _canvas;
        private readonly HTMLElement       _badge;

        private GeneratingImageSurface _surface  = GeneratingImageSurface.Ink;
        private double                 _ratio    = 1;
        private double                 _fraction = 0.5;
        private double                 _pitch    = 16;
        private bool                   _animated = true;
        private double                 _fps      = 30;

        private double? _target;          // reported progress, 0..100, or null for none
        private double  _shown = 0.65;    // fill of the field being drawn, eased toward what progress asks for
        private string  _label;

        private double _t0;
        private bool   _mounted, _seen = true, _looping;

        private ResizeObserver       _resize;
        private IntersectionObserver _visibility;

        /// <summary>Creates a square placeholder, half the width of its parent, with no progress badge.</summary>
        public GeneratingImage() : this(1) { }

        /// <summary>Creates a placeholder with a ratio of width to height, 16.0 / 9 for a wide frame, 0.75 for a tall one.</summary>
        public GeneratingImage(double aspectRatio)
        {
            _canvas = new HTMLCanvasElement();
            _canvas.setAttribute("aria-hidden", "true");
            _canvas.style.position = "absolute";
            _canvas.style.left     = "0";
            _canvas.style.top      = "0";
            _canvas.style.width    = "100%";
            _canvas.style.height   = "100%";
            _canvas.style.display  = "block";

            _badge = Div(Att("tss-generating-image-badge"));
            _badge.style.position      = "absolute";
            _badge.style.right         = "10px";
            _badge.style.bottom        = "10px";
            _badge.style.padding       = "3px 8px";
            _badge.style.setProperty("font-family", "var(--tss-font-family-mono, 'Geist Mono', ui-monospace, SFMono-Regular, Menlo, monospace)");
            _badge.style.fontSize      = "11px";
            _badge.style.lineHeight    = "16px";
            _badge.style.setProperty("letter-spacing", "0.04em");
            _badge.style.setProperty("font-variant-numeric", "tabular-nums");
            _badge.style.display       = "none";
            _badge.style.pointerEvents = "none";

            _container = Div(Att("tss-generating-image"), _canvas, _badge);
            _container.setAttribute("role", "progressbar");
            _container.setAttribute("aria-label", "Generating image");
            _container.setAttribute("aria-busy", "true");
            _container.style.position  = "relative";
            _container.style.overflow  = "hidden";
            _container.style.maxWidth  = "100%";
            _container.style.boxSizing = "border-box";

            AspectRatio(aspectRatio);
            WidthFraction(_fraction);
            ApplySurface();

            DomObserver.WhenMounted(_container, OnMounted);
        }

        /// <summary>Creates a placeholder for an image of the given pixel size (or any two numbers in its proportion): <c>new GeneratingImage(1536, 1024)</c>.</summary>
        public GeneratingImage(double width, double height) : this(height > 0 ? width / height : 1) { }

        /// <summary>Sets the shape of the frame as a ratio of width to height. A ratio that is not positive is a square.</summary>
        public GeneratingImage AspectRatio(double aspectRatio)
        {
            _ratio = aspectRatio > 0 && !double.IsNaN(aspectRatio) && !double.IsInfinity(aspectRatio) ? aspectRatio : 1;
            _container.style.setProperty("aspect-ratio", _ratio.ToString());
            Draw();
            return this;
        }

        /// <summary>Sets the shape of the frame from the size of the image it stands in for.</summary>
        public GeneratingImage AspectRatio(double width, double height) => AspectRatio(height > 0 ? width / height : 1);

        /// <summary>
        /// The share of its parent's width the frame takes, 0.5 by default. In a chat that is the share of the chat
        /// area when the message is allowed to be that wide. It never grows past the parent. An explicit
        /// <c>.W(...)</c> afterwards replaces it.
        /// </summary>
        public GeneratingImage WidthFraction(double fraction = 0.5)
        {
            _fraction = Math.Max(0.05, Math.Min(1, fraction));
            _container.style.width = (_fraction * 100).ToString() + "%";
            return this;
        }

        /// <summary>Reports progress, 0 to 100 (clamped), as a badge in the bottom right corner and as a fuller field.</summary>
        public GeneratingImage Progress(double percent)
        {
            _target = Math.Max(0, Math.Min(100, double.IsNaN(percent) ? 0 : percent));
            _container.setAttribute("aria-valuemin", "0");
            _container.setAttribute("aria-valuemax", "100");
            _container.setAttribute("aria-valuenow", Math.Round(_target.Value).ToString());
            UpdateBadge();
            EnsureLoop();
            Draw();
            return this;
        }

        /// <summary>Takes the badge away and goes back to the field's resting fill. This is how it starts.</summary>
        public GeneratingImage NoProgress()
        {
            _target = null;
            _container.removeAttribute("aria-valuemin");
            _container.removeAttribute("aria-valuemax");
            _container.removeAttribute("aria-valuenow");
            UpdateBadge();
            EnsureLoop();
            Draw();
            return this;
        }

        /// <summary>Sets what the badge says instead of the percentage, for a state that has no number ("Queued"). Null goes back to the percentage. The badge shows only while there is progress to report.</summary>
        public GeneratingImage Label(string label)
        {
            _label = label;
            UpdateBadge();
            return this;
        }

        /// <summary>Sets the ground: ink or paper.</summary>
        public GeneratingImage Surface(GeneratingImageSurface surface)
        {
            _surface = surface;
            ApplySurface();
            Draw();
            return this;
        }

        /// <summary>Draws on ink. This is how it starts.</summary>
        public GeneratingImage OnInk() => Surface(GeneratingImageSurface.Ink);

        /// <summary>Draws on paper.</summary>
        public GeneratingImage OnPaper() => Surface(GeneratingImageSurface.Paper);

        /// <summary>The distance between the centres of neighbouring pixels, in pixels at full size: 16 by default, 10 to 40.</summary>
        public GeneratingImage Pitch(double pitch)
        {
            _pitch = Math.Max(10, Math.Min(40, pitch));
            Draw();
            return this;
        }

        /// <summary>Turns the motion on or off. On by default; reduced motion turns it off whatever this says.</summary>
        public GeneratingImage Animated(bool animated = true)
        {
            _animated = animated;
            Draw();
            EnsureLoop();
            return this;
        }

        /// <summary>Turns off the animation. Same as <c>Animated(false)</c>.</summary>
        public GeneratingImage NoAnimation() => Animated(false);

        /// <summary>Frames per second while animating. 30 by default.</summary>
        public GeneratingImage FramesPerSecond(double fps)
        {
            _fps = Math.Max(1, Math.Min(60, fps));
            return this;
        }

        /// <inheritdoc />
        public HTMLElement Render() => _container;

        private bool IsAnimated => _animated && !BrandMotion.PrefersReducedMotion;

        private void ApplySurface()
        {
            var ink = _surface == GeneratingImageSurface.Ink;
            _container.style.background = ink ? BrandColors.Ink : BrandColors.Paper;
            _container.style.border     = "1px solid " + (ink ? BrandColors.Slate : BrandColors.Line);
            _badge.style.background     = ink ? "rgba(17, 20, 24, 0.82)" : "rgba(244, 244, 242, 0.88)";
            _badge.style.color          = ink ? BrandColors.Ink2 : BrandColors.Ink;
            _badge.style.border         = "1px solid " + (ink ? BrandColors.Slate : BrandColors.Line);
        }

        private void UpdateBadge()
        {
            if (_target is null)
            {
                _badge.style.display = "none";
                return;
            }

            _badge.textContent   = string.IsNullOrEmpty(_label) ? Math.Round(_target.Value) + "%" : _label;
            _badge.style.display = "block";
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

        // The fill the field is heading for: a resting 0.65 with no progress, 0.3 to 1 across 0 to 100.
        private double Goal => _target is null ? 0.65 : 0.3 + 0.7 * (_target.Value / 100);

        private void Draw()
        {
            if (!_mounted) return;
            var ctx = BrandMotion.Fit(_canvas, out var W, out var H);
            if (ctx is null) return;

            // Ease the fill so a jump in reported progress swells rather than snaps. Without motion it lands at once.
            _shown = IsAnimated ? _shown + (Goal - _shown) * 0.08 : Goal;
            if (Math.Abs(_shown - Goal) < 0.002) _shown = Goal;

            // Without motion the drawing stands at a fixed moment, as the website's do.
            var t = IsAnimated ? (BrandMotion.Now() - _t0) / 1000 : 2.4;

            // Smaller frames get a finer grid so there are still enough pixels to make a shape.
            var pitch = Math.Min(_pitch, Math.Max(10, W / 22));
            var cols  = Math.Max(3, (int)Math.Floor(W / pitch));
            var rows  = Math.Max(3, (int)Math.Floor(H / pitch));
            var dx    = W / cols;
            var dy    = H / rows;
            var cell  = Math.Min(dx, dy);

            var ink = _surface == GeneratingImageSurface.Ink;
            ctx.clearRect(0, 0, W, H);
            ctx.fillStyle = BrandColors.Signal;

            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    var u = (i + 0.5) / cols - 0.5;
                    var v = (j + 0.5) / rows - 0.5;

                    // Distance from the middle in frame units: 0 at the centre, 1 at the middle of an edge, so the swell
                    // is an ellipse that fits the frame whatever its ratio.
                    var d = Math.Sqrt(u * u + v * v) * 2;

                    // The swell: full in the middle, falling off toward the edges, reaching further as the fill grows.
                    var reach    = 0.45 + 0.75 * _shown;
                    var envelope = BrandMotion.Ease(1 - Math.Min(1, d / reach));

                    // Two waves rolling out from slightly different centres, so the field breathes rather than pulses.
                    var w1 = Math.Sin(d * 7.5 - t * 2.1 + Hash(i, j) * 0.6);
                    var w2 = Math.Sin((u * 5 + v * 4) * 2 + t * 1.3);
                    var wave = 0.5 + 0.5 * (0.65 * w1 + 0.35 * w2);

                    var size = cell * (0.1 + 0.62 * envelope * (0.35 + 0.65 * wave));
                    if (size < 0.9) continue;

                    var alpha = Math.Min(1, 0.12 + 0.88 * envelope * (0.4 + 0.6 * wave));
                    ctx.globalAlpha = ink ? alpha : Math.Min(1, alpha * 1.05);

                    var cx = (i + 0.5) * dx;
                    var cy = (j + 0.5) * dy;
                    ctx.fillRect(Math.Round(cx - size / 2), Math.Round(cy - size / 2), Math.Round(size), Math.Round(size));
                }
            }

            ctx.globalAlpha = 1;
        }

        // A stable value in 0..1 for a cell, so the waves are not perfectly regular.
        private static double Hash(int i, int j)
        {
            var s = Math.Sin(i * 12.9898 + j * 78.233) * 43758.5453;
            return s - Math.Floor(s);
        }
    }
}
