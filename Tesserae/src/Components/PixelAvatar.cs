using System;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An animated pixel-art avatar. The artwork is stored as a grid of palette indices (see
    /// <see cref="PixelAvatarSprites"/>) and every pixel is rendered as an absolutely positioned
    /// square div, so the avatar scales crisply to any size and can be recolored by swapping its
    /// <see cref="PixelAvatarPalette"/>.
    ///
    /// Use <see cref="AttachTo"/> to perch the avatar on another component, for example on top of
    /// a button.
    /// </summary>
    [Transpose.Name("tss.PixelAvatar")]
    public sealed class PixelAvatar : ComponentBase<PixelAvatar, HTMLElement>
    {
        /// <summary>The default size, in CSS pixels, of a single sprite pixel.</summary>
        public const int DefaultPixelSize = 4;

        // Pixels reference their color through a CSS custom property rather than carrying the
        // literal color, so switching design only rewrites eleven variables on the root instead of
        // repainting the whole grid - and consumers can override a single index from CSS.
        private static readonly string[] ColorVariables = BuildColorVariables();

        private readonly HTMLElement   _canvas;
        private readonly HTMLElement[] _cells;
        private readonly string[]      _painted;
        private readonly int           _width;
        private readonly int           _height;

        private Action               _pixelSizeChanged;
        private PixelAvatarPalette   _palette;
        private PixelAvatarDesign    _design;
        private PixelSpriteAnimation _animation;
        private PixelAvatarFacing    _facing;
        private int                  _pixelSize;
        private int                  _frame;
        private double               _speed;
        private double               _timer;
        private bool                 _paused;
        private bool                 _isMounted;

        private event Action<PixelAvatar, PixelAvatarAnimation> AnimationFinished;
        private event Action<PixelAvatar, PixelAvatarAnimation> AnimationStarted;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public PixelAvatar(PixelAvatarDesign design = PixelAvatarDesign.Black, PixelAvatarAnimation animation = PixelAvatarAnimation.Idle)
        {
            _width     = PixelAvatarSprites.FrameWidth;
            _height    = PixelAvatarSprites.FrameHeight;
            _cells     = new HTMLElement[_width * _height];
            _painted   = new string[_cells.Length];
            _pixelSize = DefaultPixelSize;
            _speed     = 1;
            _facing    = PixelAvatarFacing.Right;
            _animation = PixelAvatarSprites.Get(animation);

            _canvas      = Div(Att("tss-pixelavatar-canvas"));
            InnerElement = Div(Att("tss-pixelavatar", role: "img"), _canvas);

            for (var i = 0; i < _cells.Length; i++)
            {
                var cell = Div(Att("tss-pixelavatar-pixel"));
                _cells[i]   = cell;
                _painted[i] = string.Empty;
                _canvas.appendChild(cell);
            }

            ApplyPixelSize();
            SetDesign(design);
            RenderFrame();
            UpdateAriaLabel();

            TrackMounting();
        }

        /// <summary>Gets the design currently used by the component.</summary>
        public PixelAvatarDesign DesignValue => _design;

        /// <summary>Gets the palette currently used by the component.</summary>
        public PixelAvatarPalette Palette => _palette;

        /// <summary>Gets the animation currently playing.</summary>
        public PixelAvatarAnimation CurrentAnimation => _animation.Animation;

        /// <summary>Gets the index of the frame currently shown.</summary>
        public int CurrentFrame => _frame;

        /// <summary>Gets the rendered width of the avatar, in CSS pixels.</summary>
        public int RenderedWidth => _width * _pixelSize;

        /// <summary>Gets the rendered height of the avatar, in CSS pixels.</summary>
        public int RenderedHeight => _height * _pixelSize;

        /// <summary>Gets or sets the size, in CSS pixels, of a single sprite pixel.</summary>
        public int PixelSizeValue
        {
            get => _pixelSize;
            set => PixelSize(value);
        }

        /// <summary>Gets or sets the direction the avatar faces.</summary>
        public PixelAvatarFacing FacingValue
        {
            get => _facing;
            set => Facing(value);
        }

        /// <summary>Gets or sets whether playback is paused.</summary>
        public bool IsPaused
        {
            get => _paused;
            set
            {
                _paused = value;
                SyncTimer();
            }
        }

        /// <summary>
        /// Sets the design (and therefore the palette) of the component.
        /// </summary>
        public PixelAvatar SetDesign(PixelAvatarDesign design)
        {
            _design = design;
            return SetPalette(PixelAvatarPalettes.Get(design));
        }

        /// <summary>
        /// Sets a custom palette, whose colors map to palette indices 1..N of the sprite data.
        /// </summary>
        public PixelAvatar SetPalette(PixelAvatarPalette palette)
        {
            if (palette == null) return this;

            _palette = palette;

            for (byte index = 1; index < ColorVariables.Length; index++)
            {
                InnerElement.style.setProperty(VariableName(index), _palette.ColorAt(index));
            }

            return this;
        }

        /// <summary>
        /// Imports a custom palette from a list of CSS colors separated by commas, semicolons or
        /// whitespace — either all <see cref="PixelAvatarSprites.PaletteSize"/> of them, or just
        /// three read as highlight/base/shadow. Unparseable input leaves the current palette alone;
        /// use <see cref="PixelAvatarPalette.Parse"/> directly to detect that case.
        /// </summary>
        public PixelAvatar SetPalette(string colors, string name = "Custom")
        {
            return SetPalette(PixelAvatarPalette.Parse(colors, name));
        }

        /// <summary>
        /// Imports a custom palette built from the artwork's three shading levels, the way the
        /// single-hue built-in designs are built.
        /// </summary>
        public PixelAvatar SetShades(string highlight, string baseColor, string shadow, string name = "Custom")
        {
            return SetPalette(PixelAvatarPalette.FromShades(highlight, baseColor, shadow, name));
        }

        /// <summary>
        /// Recolors a single palette index. This only rewrites that index's CSS variable, so it is
        /// cheap enough to drive from a color picker's input event.
        /// </summary>
        public PixelAvatar SetColor(byte index, string color)
        {
            if (index == 0 || index > PixelAvatarSprites.PaletteSize) return this;

            _palette = _palette.WithColor(index, color);
            InnerElement.style.setProperty(VariableName(index), color);
            return this;
        }

        /// <summary>
        /// Sets the size, in CSS pixels, of a single sprite pixel. The rendered avatar ends up
        /// <c>PixelAvatarSprites.FrameWidth * pixelSize</c> wide.
        /// </summary>
        public PixelAvatar PixelSize(int pixelSize)
        {
            if (pixelSize < 1) pixelSize = 1;
            if (pixelSize == _pixelSize) return this;

            _pixelSize = pixelSize;
            ApplyPixelSize();
            _pixelSizeChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Sets the direction the avatar faces, mirroring the artwork when facing left.
        /// </summary>
        public PixelAvatar Facing(PixelAvatarFacing facing)
        {
            _facing = facing;
            _canvas.UpdateClassIf(facing == PixelAvatarFacing.Left, "tss-pixelavatar-mirrored");
            return this;
        }

        /// <summary>
        /// Sets whether the sprite is drawn with a hairline halo in the theme's contrasting color.
        /// It is on by default because several palettes are pure white and several are near-black,
        /// so without it those designs disappear against one theme or the other. Turn it off when
        /// the avatar sits on a background you control and you want the artwork's colors untouched.
        /// </summary>
        public PixelAvatar Outline(bool value = true)
        {
            InnerElement.UpdateClassIf(!value, "tss-pixelavatar-flat");
            return this;
        }

        /// <summary>
        /// Overrides the color of the halo drawn by <see cref="Outline"/>, which defaults to a
        /// translucent black in light mode and a translucent white in dark mode.
        /// </summary>
        public PixelAvatar OutlineColor(string color)
        {
            InnerElement.style.setProperty("--tss-pxav-outline", color);
            return this;
        }

        /// <summary>
        /// Multiplies the playback speed of every animation. Values above 1 play faster.
        /// </summary>
        public PixelAvatar Speed(double speed)
        {
            _speed = speed <= 0 ? 1 : speed;
            SyncTimer();
            return this;
        }

        /// <summary>
        /// Plays an animation from its first frame. Animations that do not loop hand over to a
        /// follow-up animation when they finish (see <see cref="PixelSpriteAnimation.Next"/>).
        /// </summary>
        public PixelAvatar Play(PixelAvatarAnimation animation)
        {
            _animation = PixelAvatarSprites.Get(animation);
            _frame     = 0;
            _paused    = false;

            RenderFrame();
            UpdateAriaLabel();
            SyncTimer();

            AnimationStarted?.Invoke(this, animation);
            return this;
        }

        /// <summary>
        /// Pauses playback on the current frame.
        /// </summary>
        public PixelAvatar Pause()
        {
            IsPaused = true;
            return this;
        }

        /// <summary>
        /// Resumes playback from the current frame.
        /// </summary>
        public PixelAvatar Resume()
        {
            IsPaused = false;
            return this;
        }

        /// <summary>
        /// Shows a specific frame of the current animation without changing playback state.
        /// </summary>
        public PixelAvatar GoToFrame(int frame)
        {
            var count = _animation.Frames.Length;
            _frame = count == 0 ? 0 : ((frame % count) + count) % count;
            RenderFrame();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when a non-looping animation reaches its last frame, just
        /// before its follow-up animation takes over. Calling <see cref="Play"/> from the callback
        /// suppresses that hand-over.
        /// </summary>
        public PixelAvatar OnAnimationFinished(Action<PixelAvatar, PixelAvatarAnimation> onAnimationFinished, bool clearPrevious = true)
        {
            if (clearPrevious) AnimationFinished = null;
            AnimationFinished += onAnimationFinished;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked whenever a new animation starts playing.
        /// </summary>
        public PixelAvatar OnAnimationStarted(Action<PixelAvatar, PixelAvatarAnimation> onAnimationStarted, bool clearPrevious = true)
        {
            if (clearPrevious) AnimationStarted = null;
            AnimationStarted += onAnimationStarted;
            return this;
        }

        /// <summary>
        /// Wraps <paramref name="target"/> so that this avatar is anchored to one of its edges. The
        /// returned component renders the target as usual, with the avatar perched next to it.
        /// </summary>
        public PixelAvatarAttachment AttachTo(IComponent target, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            return new PixelAvatarAttachment(target, this, anchor);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        // Lets a PixelAvatarAttachment keep the room it reserves in sync with the avatar's size.
        // Only one attachment can own an avatar, so a single callback is enough.
        internal void TrackPixelSize(Action onPixelSizeChanged)
        {
            _pixelSizeChanged = onPixelSizeChanged;
            onPixelSizeChanged();
        }

        private void ApplyPixelSize()
        {
            var size = $"{_pixelSize}px";

            InnerElement.style.width  = $"{_width * _pixelSize}px";
            InnerElement.style.height = $"{_height * _pixelSize}px";

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var style = _cells[y * _width + x].style;
                    style.left   = $"{x * _pixelSize}px";
                    style.top    = $"{y * _pixelSize}px";
                    style.width  = size;
                    style.height = size;
                }
            }
        }

        private void RenderFrame()
        {
            if (_animation.Frames.Length == 0) return;

            var sprite = _animation.Frames[_frame];

            for (var i = 0; i < _cells.Length; i++)
            {
                var index = sprite.Pixels[i];
                var color = index == 0 ? string.Empty : ColorVariables[index];

                // Most cells keep their color between frames, so skipping the unchanged ones keeps
                // a running avatar down to a handful of style writes per frame.
                if (_painted[i] == color) continue;

                _painted[i] = color;
                _cells[i].style.backgroundColor = color;
            }
        }

        private void Tick()
        {
            var frames = _animation.Frames.Length;
            if (frames == 0) return;

            if (_frame + 1 < frames)
            {
                _frame++;
                RenderFrame();
                return;
            }

            if (_animation.Loops)
            {
                _frame = 0;
                RenderFrame();
                return;
            }

            var finished = _animation;
            AnimationFinished?.Invoke(this, finished.Animation);

            // A handler is allowed to pick the next animation itself, in which case we leave it be.
            if (_animation != finished) return;

            if (finished.Next == finished.Animation)
            {
                _frame = 0;
                RenderFrame();
                return;
            }

            Play(finished.Next);
        }

        private void SyncTimer()
        {
            StopTimer();

            if (_paused || !_isMounted || _animation.Frames.Length < 2) return;

            var interval = (int)System.Math.Round(_animation.FrameDurationMs / _speed);
            if (interval < 16) interval = 16;

            _timer = window.setInterval(_ => Tick(), interval);
        }

        private void StopTimer()
        {
            if (_timer == 0) return;

            window.clearInterval(_timer);
            _timer = 0;
        }

        // DomObserver's callbacks are one-shot, so each removal re-arms the mount tracking. This
        // keeps the interval alive only while the avatar is actually on screen.
        private void TrackMounting()
        {
            DomObserver.WhenMounted(InnerElement, () =>
            {
                _isMounted = true;
                SyncTimer();

                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    _isMounted = false;
                    StopTimer();
                    TrackMounting();
                });
            });
        }

        private void UpdateAriaLabel()
        {
            AriaLabel = $"{_palette.Name} pixel avatar, {_animation.Animation}";
        }

        private static string VariableName(byte index) => $"--tss-pxav-{index}";

        private static string[] BuildColorVariables()
        {
            var variables = new string[PixelAvatarSprites.PaletteSize + 1];
            variables[0] = string.Empty;

            for (byte index = 1; index < variables.Length; index++)
            {
                variables[index] = $"var({VariableName(index)})";
            }

            return variables;
        }
    }

    /// <summary>
    /// A component that renders another component with a <see cref="PixelAvatar"/> anchored to one
    /// of its edges. Created through <see cref="PixelAvatar.AttachTo"/> or
    /// <see cref="PixelAvatarExtensions.WithPixelAvatar"/>.
    ///
    /// By default the wrapper reserves room for the avatar on the anchored side, so the avatar
    /// stays inside the wrapper's box and cannot be clipped by a scrolling ancestor. Call
    /// <see cref="Overlap"/> to hang it outside the box instead, leaving the target's own footprint
    /// untouched.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarAttachment")]
    public sealed class PixelAvatarAttachment : IComponent, ISpecialCaseStyling
    {
        // Room left between the avatar and the target when anchored to a side rather than an edge.
        private const int SideGap = 4;

        private readonly HTMLElement       _host;
        private          PixelAvatarAnchor _anchor;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public PixelAvatarAttachment(IComponent target, PixelAvatar avatar, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (avatar == null) throw new ArgumentNullException(nameof(avatar));

            Target  = target;
            Avatar  = avatar;
            _anchor = anchor;

            _host = Div(Att($"tss-pixelavatar-host {anchor}"), target.Render(), avatar.Render());

            avatar.TrackPixelSize(UpdateReservedSpace);
        }

        /// <summary>Gets the avatar anchored to the target.</summary>
        public PixelAvatar Avatar { get; }

        /// <summary>Gets the component the avatar is anchored to.</summary>
        public IComponent Target { get; }

        /// <summary>Gets the element sizing helpers should style, which is the wrapper itself so the
        /// avatar stays anchored to the target's edges.</summary>
        public HTMLElement StylingContainer => _host;

        /// <summary>Gets whether styling should propagate to the stack item parent.</summary>
        public bool PropagateToStackItemParent => true;

        /// <summary>
        /// Moves the avatar to a different edge of the target.
        /// </summary>
        public PixelAvatarAttachment Anchor(PixelAvatarAnchor anchor)
        {
            _host.classList.remove($"{_anchor}");
            _anchor = anchor;
            _host.classList.add($"{anchor}");
            UpdateReservedSpace();
            return this;
        }

        /// <summary>
        /// Lets the avatar hang outside the wrapper's box instead of reserving room for it, so the
        /// target keeps exactly the footprint it would have on its own. Beware that an avatar in
        /// overlap mode is clipped by any ancestor that scrolls or hides its overflow.
        /// </summary>
        public PixelAvatarAttachment Overlap(bool value = true)
        {
            _host.UpdateClassIf(value, "tss-pixelavatar-overlap");
            return this;
        }

        /// <summary>
        /// Nudges the avatar away from its anchor by a number of CSS pixels, with positive values
        /// moving it right and down.
        /// </summary>
        public PixelAvatarAttachment Offset(int x, int y)
        {
            _host.style.setProperty("--tss-pxav-dx", $"{x}px");
            _host.style.setProperty("--tss-pxav-dy", $"{y}px");
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _host;

        private void UpdateReservedSpace()
        {
            var isSideAnchor = _anchor == PixelAvatarAnchor.LeftCenter || _anchor == PixelAvatarAnchor.RightCenter;
            var reserved     = isSideAnchor ? Avatar.RenderedWidth + SideGap : Avatar.RenderedHeight;

            _host.style.setProperty("--tss-pxav-reserve", $"{reserved}px");
        }
    }

    /// <summary>
    /// Fluent helpers for attaching a <see cref="PixelAvatar"/> to any component.
    /// </summary>
    public static class PixelAvatarExtensions
    {
        /// <summary>
        /// Wraps the component so that <paramref name="avatar"/> is perched on one of its edges.
        /// </summary>
        public static PixelAvatarAttachment WithPixelAvatar(this IComponent component, PixelAvatar avatar, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            return new PixelAvatarAttachment(component, avatar, anchor);
        }

        /// <summary>
        /// Wraps the component so that a new avatar with the given design is perched on one of its
        /// edges. The avatar is available through <see cref="PixelAvatarAttachment.Avatar"/>.
        /// </summary>
        public static PixelAvatarAttachment WithPixelAvatar(this IComponent component, PixelAvatarDesign design, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            return new PixelAvatarAttachment(component, new PixelAvatar(design), anchor);
        }
    }
}
