using System;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Themes.Curiosity
{
    /// <summary>The ground a <see cref="FlowField"/> is drawn on.</summary>
    public enum FlowFieldSurface
    {
        /// <summary>Ink, with ash dashes: the website's hero.</summary>
        Ink,
        /// <summary>Paper, with slate dashes: the website's page bands.</summary>
        Paper,
        /// <summary>No ground of its own: slate dashes over whatever is behind it.</summary>
        Transparent
    }

    /// <summary>Which side of a <see cref="FlowField"/> fades out.</summary>
    public enum FlowFieldFade
    {
        /// <summary>Every dash at full strength.</summary>
        None,
        /// <summary>Fades toward the left edge (the field sits right of copy it fades into).</summary>
        Left,
        /// <summary>Fades toward the right edge.</summary>
        Right,
        /// <summary>Fades toward the top.</summary>
        Up,
        /// <summary>Fades toward the bottom.</summary>
        Down
    }

    /// <summary>How the field's pixel is drawn.</summary>
    public enum FlowFieldMark
    {
        /// <summary>In the Signal: the field spends its layout's one Signal.</summary>
        Signal,
        /// <summary>In the ground's own tone (ink on paper, paper on ink), for a layout whose Signal is elsewhere.</summary>
        Tone,
        /// <summary>In the ground's tone, and only while the pointer is over the field.</summary>
        Hover
    }

    /// <summary>
    /// The brand's field (BRAND.md s6), live: a grid of short dashes whose angle bends around one point, the signal
    /// pixel. The website's hero draws it on ink; the pixel drifts slowly round its place and, while the pointer is
    /// over the field, eases toward it at the brand's lerp of 0.06 a frame and back home when it leaves. While it
    /// follows, the mouse cursor is hidden, so the square is the only cursor there is.
    /// <para>
    /// It is redrawn on a canvas at the size it is shown, whenever that size changes. The animation loop runs only
    /// while the pixel is travelling or drifting and the field is on screen, so a field at rest costs nothing.
    /// Reduced motion gets the drawing with no drift and no follow, and keeps its cursor.
    /// </para>
    /// </summary>
    public sealed class FlowField : IComponent
    {
        private const double Lerp = 0.06, DashLength = 18, Dot = 12;

        private readonly HTMLElement       _container;
        private readonly HTMLCanvasElement _canvas;

        private FlowFieldSurface _surface     = FlowFieldSurface.Ink;
        private FlowFieldFade    _fade        = FlowFieldFade.None;
        private FlowFieldMark    _mark        = FlowFieldMark.Signal;
        private double                _pitch       = 32;
        private double                _homeX       = 0.5, _homeY = 0.5;
        private bool                  _animated    = true;
        private bool                  _interactive = true;
        private bool                  _drift       = true;

        private double _dotX = 0.5, _dotY = 0.5, _targetX = 0.5, _targetY = 0.5;
        private bool   _inside, _seen = true, _looping, _mounted;
        private double _driftStart;

        private ResizeObserver       _resize;
        private IntersectionObserver _visibility;

        /// <summary>Creates a field on ink, drifting, following the pointer, its pixel in the Signal at the centre.</summary>
        public FlowField()
        {
            _canvas = new HTMLCanvasElement();
            _canvas.setAttribute("aria-hidden", "true");
            _canvas.style.position = "absolute";
            _canvas.style.left     = "0";
            _canvas.style.top      = "0";
            _canvas.style.width    = "100%";
            _canvas.style.height   = "100%";
            _canvas.style.display  = "block";

            _container = Div(Att("tss-flow-field"), _canvas);
            _container.setAttribute("aria-hidden", "true");
            _container.style.position = "relative";
            _container.style.overflow = "hidden";
            _container.style.width    = "100%";
            _container.style.height   = "320px";
            ApplySurface();

            _container.addEventListener("pointermove",  e => OnPointerMove(e));
            _container.addEventListener("pointerleave", e => OnPointerLeave());

            DomObserver.WhenMounted(_container, OnMounted);
        }

        /// <summary>The ground: <see cref="FlowFieldSurface.Ink"/> by default.</summary>
        public FlowField Surface(FlowFieldSurface surface)
        {
            _surface = surface;
            ApplySurface();
            Draw();
            return this;
        }

        /// <summary>Draws on ink with ash dashes (the default).</summary>
        public FlowField OnInk() => Surface(FlowFieldSurface.Ink);

        /// <summary>Draws on paper with slate dashes.</summary>
        public FlowField OnPaper() => Surface(FlowFieldSurface.Paper);

        /// <summary>Which side fades out. None by default.</summary>
        public FlowField Fade(FlowFieldFade fade)
        {
            _fade = fade;
            Draw();
            return this;
        }

        /// <summary>How the pixel is drawn. The Signal by default.</summary>
        public FlowField Mark(FlowFieldMark mark)
        {
            _mark = mark;
            Draw();
            return this;
        }

        /// <summary>The cell pitch in screen pixels (the brand allows 30 to 45; the website's hero uses 32).</summary>
        public FlowField Pitch(double pitch)
        {
            _pitch = Math.Max(8, pitch);
            Draw();
            return this;
        }

        /// <summary>
        /// Where the pixel rests, from 0 to 1 on each axis. Keep it near the middle (0.4 to 0.6) or a narrow crop hides it.
        /// </summary>
        public FlowField DotAt(double x, double y)
        {
            _homeX = _dotX = _targetX = Clamp01(x);
            _homeY = _dotY = _targetY = Clamp01(y);
            Draw();
            return this;
        }

        /// <summary>
        /// Turns the motion on or off: the drift, and the easing toward the pointer. With animations off the field
        /// is drawn once and, if still interactive, the pixel jumps straight to the pointer. On by default; reduced
        /// motion turns it off whatever this says.
        /// </summary>
        public FlowField Animated(bool animated = true)
        {
            _animated = animated;
            if (!IsAnimated) { _dotX = _targetX; _dotY = _targetY; Draw(); }
            EnsureLoop();
            return this;
        }

        /// <summary>Turns off every animation. Same as <c>Animated(false)</c>.</summary>
        public FlowField NoAnimation() => Animated(false);

        /// <summary>
        /// Whether the pixel follows the pointer (and hides the cursor while it does). On by default. Touch never
        /// drags it: a finger has no cursor to stand in for, and dragging one scrolls.
        /// </summary>
        public FlowField Interactive(bool interactive = true)
        {
            _interactive = interactive;
            if (!interactive) OnPointerLeave();
            _container.style.cursor = "";
            return this;
        }

        /// <summary>Stops the pixel following the pointer. Same as <c>Interactive(false)</c>.</summary>
        public FlowField NoInteraction() => Interactive(false);

        /// <summary>Whether the pixel circles its place slowly while the field is on screen (the website's hero does). On by default.</summary>
        public FlowField Drift(bool drift = true)
        {
            _drift = drift;
            if (!drift) { _targetX = _homeX; _targetY = _homeY; }
            EnsureLoop();
            return this;
        }

        /// <inheritdoc />
        public HTMLElement Render() => _container;

        private bool IsAnimated => _animated && !BrandMotion.PrefersReducedMotion;
        private bool IsDrifting => IsAnimated && _drift && _seen && _mounted;
        private bool IsFollowing => _interactive && !BrandMotion.PrefersReducedMotion;

        private static double Clamp01(double v) => Math.Max(0, Math.Min(1, v));

        private void ApplySurface()
        {
            _container.style.background = _surface == FlowFieldSurface.Ink ? BrandColors.Ink
                                        : _surface == FlowFieldSurface.Paper ? BrandColors.Paper
                                        : "transparent";
        }

        private void OnMounted()
        {
            _mounted    = true;
            _driftStart = BrandMotion.Now();

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

        private void OnPointerMove(Event e)
        {
            if (!IsFollowing) return;
            if (e["pointerType"].As<string>() == "touch") return;

            var box   = (DOMRect)_container.getBoundingClientRect();
            var mouse = e.As<MouseEvent>();
            if (box.width < 1 || box.height < 1) return;

            _inside  = true;
            _targetX = (mouse.clientX - box.left) / box.width;
            _targetY = (mouse.clientY - box.top) / box.height;
            _container.style.cursor = "none";

            if (IsAnimated) EnsureLoop();
            else { _dotX = _targetX; _dotY = _targetY; Draw(); }
        }

        private void OnPointerLeave()
        {
            _inside  = false;
            _targetX = _homeX;
            _targetY = _homeY;
            _container.style.cursor = "";

            if (IsAnimated) EnsureLoop();
            else { _dotX = _targetX; _dotY = _targetY; Draw(); }
        }

        // One animation loop, and only while the pixel is travelling or drifting on screen.
        private void EnsureLoop()
        {
            if (_looping || !_mounted) return;
            if (!IsDrifting && !NeedsTravel()) return;

            _looping = true;
            FrameRequestCallback step = null;
            step = now =>
            {
                if (!_mounted) { _looping = false; return; }

                if (IsDrifting && !_inside)
                {
                    var t = (now - _driftStart) / 1000;
                    _targetX = _homeX + Math.Cos(t * 0.35) * 0.16;
                    _targetY = _homeY + Math.Sin(t * 0.35) * 0.2;
                }
                else if (!IsDrifting && !_inside)
                {
                    _targetX = _homeX;
                    _targetY = _homeY;
                }

                if (!IsAnimated)
                {
                    _dotX = _targetX; _dotY = _targetY;
                    Draw();
                    _looping = false;
                    return;
                }

                var moving = NeedsTravel();
                _dotX += (_targetX - _dotX) * Lerp;
                _dotY += (_targetY - _dotY) * Lerp;
                Draw();

                if (IsDrifting || moving) window.requestAnimationFrame(step);
                else _looping = false;
            };
            window.requestAnimationFrame(step);
        }

        private bool NeedsTravel() => Math.Abs(_targetX - _dotX) >= 0.0005 || Math.Abs(_targetY - _dotY) >= 0.0005;

        // The brand's loop (BRAND.md s6), as the website draws it.
        private void Draw()
        {
            if (!_mounted) return;
            var ctx = BrandMotion.Fit(_canvas, out var W, out var H);
            if (ctx is null) return;

            var onInk = _surface == FlowFieldSurface.Ink;
            var dash  = onInk ? BrandColors.Ash : BrandColors.Slate;
            var pixel = _mark == FlowFieldMark.Signal ? BrandColors.Signal : (onInk ? BrandColors.Paper : BrandColors.Ink);

            var cols  = Math.Max(4, (int)Math.Round(W / _pitch));
            var rows  = Math.Max(4, (int)Math.Round(H / _pitch));
            var cw    = W / cols;
            var ch    = H / rows;
            var len   = Math.Min(DashLength, cw * 0.55);
            var ax    = _dotX * W;
            var ay    = _dotY * H;
            var sigma = 0.3 * Math.Max(W, H);
            var keep  = Math.Max(len * 0.6, Dot * 0.9);

            ctx.clearRect(0, 0, W, H);
            ctx.lineWidth   = 1.5;
            ctx.lineCap     = "square";
            ctx.strokeStyle = dash;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var x  = c * cw + cw / 2;
                    var y  = r * ch + ch / 2;
                    var dx = x - ax;
                    var dy = y - ay;
                    var d  = Math.Sqrt(dx * dx + dy * dy);
                    if (d < keep) continue;   // keep the dashes off the pixel

                    var w    = Math.Exp(-(d * d) / (2 * sigma * sigma));
                    var circ = Math.Atan2(dy, dx) + Math.PI / 2;
                    var a    = Math.Atan2(w * Math.Sin(circ), (1 - w) + w * Math.Cos(circ));

                    ctx.globalAlpha = FadeAlpha(c, r, cols, rows);

                    var hx = Math.Cos(a) * len / 2;
                    var hy = Math.Sin(a) * len / 2;
                    ctx.beginPath();
                    ctx.moveTo(x - hx, y - hy);
                    ctx.lineTo(x + hx, y + hy);
                    ctx.stroke();
                }
            }

            ctx.globalAlpha = 1;

            // A field with no pixel at rest shows one only while it is being pointed at, or travelling home.
            if (_mark == FlowFieldMark.Hover && !_inside && !NeedsTravel()) return;

            ctx.fillStyle = pixel;
            ctx.fillRect(Math.Round(ax) - Dot / 2, Math.Round(ay) - Dot / 2, Dot, Dot);
        }

        private double FadeAlpha(int c, int r, int cols, int rows)
        {
            switch (_fade)
            {
                case FlowFieldFade.Right: return 0.15 + 0.85 * (1 - (double)c / (cols - 1));
                case FlowFieldFade.Left:  return 0.15 + 0.85 * ((double)c / (cols - 1));
                case FlowFieldFade.Down:  return 0.2 + 0.8 * (1 - (double)r / (rows - 1));
                case FlowFieldFade.Up:    return 0.2 + 0.8 * ((double)r / (rows - 1));
                default:                       return 1;
            }
        }
    }
}
