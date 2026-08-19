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
    [Transpose.Name("tss.pav")]
    public sealed class PixelAvatar : ComponentBase<PixelAvatar, HTMLElement>
    {
        /// <summary>The default size, in CSS pixels, of a single sprite pixel.</summary>
        public const int DefaultPixelSize = 4;

        /// <summary>The default length, in milliseconds, of a <see cref="Turn"/>.</summary>
        public const int DefaultTurnDurationMs = 320;

        /// <summary>
        /// How long an auto-idling avatar rests before falling asleep, in milliseconds. Jittered on
        /// use, so the cat does not nod off on a stopwatch.
        /// </summary>
        public const int DefaultSleepAfterMs = 16000;

        /// <summary>
        /// How long after a click a second one still counts as a double click, in milliseconds.
        /// See <see cref="ReactToClicks"/>.
        /// </summary>
        public const int DoubleClickWindowMs = 400;

        // How far the viewer sits from the sprite, as a multiple of its rendered width. Low enough
        // that the near edge visibly swings toward you mid-turn, high enough that the sprite does
        // not distort at rest.
        private const int PerspectiveFactor = 4;

        // Pixels reference their color through a CSS custom property rather than carrying the
        // literal color, so switching design only rewrites eleven variables on the root instead of
        // repainting the whole grid - and consumers can override a single index from CSS.
        private static readonly string[] ColorVariables = BuildColorVariables();

        // What AutoIdle drifts between. Sit and Crouch are the entry animations; their own
        // hand-overs land on SitIdle and CrouchIdle, which rest in turn.
        private static readonly PixelAvatarAnimation[] RestingPoses =
        {
            PixelAvatarAnimation.Idle,
            PixelAvatarAnimation.Sit,
            PixelAvatarAnimation.Crouch
        };

        // Waking is a little performance rather than a snap back to resting: the cat stretches,
        // then startles at whoever woke it, and only then goes back to drifting.
        private static readonly PixelAvatarAnimation[] WakeSequence =
        {
            PixelAvatarAnimation.Stretch,
            PixelAvatarAnimation.Startle
        };

        // The two click reactions. They are sequences of one rather than a plain Play so that the
        // avatar hands back to whatever it was doing once the reaction is over - an auto-idling cat
        // goes on drifting instead of being left standing in whatever the reaction chained into.
        private static readonly PixelAvatarAnimation[] PokeSequence    = { PixelAvatarAnimation.Interact };
        private static readonly PixelAvatarAnimation[] StartleSequence = { PixelAvatarAnimation.Startle };

        // The accent is not a palette index - it is an extra half-size pixel laid over each ear
        // tip, so a design can carry a spot of color the shared artwork has no cell for.
        private const double AccentScale = 0.5;

        private readonly HTMLElement   _canvas;
        private readonly HTMLElement   _accentLeft;
        private readonly HTMLElement   _accentRight;
        private readonly HTMLElement[] _cells;
        private readonly string[]      _painted;
        private readonly int           _width;
        private readonly int           _height;

        private Action                       _pixelSizeChanged;
        private Action<PixelAvatarAnimation> _animationStarted;
        private Action<PixelAvatarAnimation> _animationFinished;
        private PixelAvatarPalette           _palette;
        private PixelAvatarDesign            _design;
        private PixelSpriteAnimation         _animation;
        private PixelAvatarFacing            _facing;
        private int                          _pixelSize;
        private int                          _frame;
        private double                       _speed;
        private double                       _timer;
        private bool                         _paused;
        private bool                         _isMounted;
        private bool                         _autoIdle;
        private int                          _restMinMs;
        private int                          _restMaxMs;
        private int                          _sleepAfterMs = DefaultSleepAfterMs;
        private int                          _restedMs;
        private int                          _sleepBudgetMs;
        private int                          _lastHoldMs;
        private PixelAvatarAnimation[]       _sequence;
        private int                          _sequenceIndex;
        private bool                         _reactsToClicks = true;
        private bool                         _hasClickHandler;
        private double                       _clickTimer;

        private event Action<PixelAvatar, PixelAvatarAnimation> AnimationFinished;
        private event Action<PixelAvatar, PixelAvatarAnimation> AnimationStarted;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="key">
        /// The key the sprite artwork is scrambled with. The library ships the sheet obfuscated
        /// and does not carry the key, so the application supplies it here; the first avatar
        /// constructed is what decodes the artwork for all of them.
        /// </param>
        public PixelAvatar(byte key, PixelAvatarDesign design = PixelAvatarDesign.Black, PixelAvatarAnimation animation = PixelAvatarAnimation.Idle)
        {
            PixelAvatarSprites.Load(key);

            _width     = PixelAvatarSprites.FrameWidth;
            _height    = PixelAvatarSprites.FrameHeight;
            _cells     = new HTMLElement[_width * _height];
            _painted   = new string[_cells.Length];
            _pixelSize = DefaultPixelSize;
            _speed     = 1;
            _facing        = PixelAvatarFacing.Right;
            _autoIdle      = animation == PixelAvatarAnimation.AutoIdle;
            _sleepBudgetMs = PixelAvatarRandom.Jittered(_sleepAfterMs);
            _animation = PixelAvatarSprites.Get(animation);

            _canvas      = Div(Att("tss-pixelavatar-canvas"));
            _accentLeft  = Div(Att("tss-pixelavatar-accent"));
            _accentRight = Div(Att("tss-pixelavatar-accent"));
            InnerElement = Div(Att("tss-pixelavatar", role: "img"), _canvas);

            for (var i = 0; i < _cells.Length; i++)
            {
                var cell = Div(Att("tss-pixelavatar-pixel"));
                _cells[i]   = cell;
                _painted[i] = string.Empty;
                _canvas.appendChild(cell);
            }

            // After the pixels, so the accent paints over the ear tip rather than under it.
            _canvas.appendChild(_accentLeft);
            _canvas.appendChild(_accentRight);

            ApplyPixelSize();
            SetDesign(design);
            RenderFrame();
            UpdateAriaLabel();

            AttachClick();

            // Registered after AttachClick so an application's own handler runs first. It only ever
            // does something when there is no such handler - see ReactToClicks.
            InnerElement.addEventListener("click", _ => ReactToClick());

            TrackMounting();
        }

        /// <summary>Gets the design currently used by the component.</summary>
        public PixelAvatarDesign DesignValue => _design;

        /// <summary>Gets the palette currently used by the component.</summary>
        public PixelAvatarPalette Palette => _palette;

        /// <summary>
        /// Gets the animation currently playing. While auto-idling this is whichever resting pose
        /// is showing, not <see cref="PixelAvatarAnimation.AutoIdle"/>.
        /// </summary>
        public PixelAvatarAnimation CurrentAnimation => _animation.Animation;

        /// <summary>
        /// Gets whether the avatar is drifting between resting poses on its own, which
        /// <see cref="PixelAvatarAnimation.AutoIdle"/> turns on and any other
        /// <see cref="Play"/> turns off.
        /// </summary>
        public bool IsAutoIdling => _autoIdle;

        /// <summary>
        /// Gets whether the avatar is asleep, which for an auto-idling one happens on its own after
        /// <see cref="SleepAfter"/> of resting.
        /// </summary>
        public bool IsAsleep => _animation.Animation == PixelAvatarAnimation.Sleep
                             || _animation.Animation == PixelAvatarAnimation.SleepIdle;

        /// <summary>
        /// Overrides how long the resting poses hold their first frame before playing their cycle,
        /// in milliseconds. The actual hold is picked uniformly from the range every time. Pass
        /// zero for both to go back to each animation's own timing.
        /// </summary>
        public PixelAvatar RestDelay(int minMs, int maxMs)
        {
            _restMinMs = minMs < 0 ? 0 : minMs;
            _restMaxMs = maxMs < _restMinMs ? _restMinMs : maxMs;
            SyncTimer();
            return this;
        }

        /// <summary>
        /// Sets how long an auto-idling avatar rests before falling asleep, in milliseconds. The
        /// value is jittered on use. Pass zero to keep it awake indefinitely. Only
        /// <see cref="PixelAvatarAnimation.AutoIdle"/> sleeps on its own; playing an animation
        /// directly never does.
        /// </summary>
        public PixelAvatar SleepAfter(int milliseconds)
        {
            _sleepAfterMs  = milliseconds < 0 ? 0 : milliseconds;
            _sleepBudgetMs = PixelAvatarRandom.Jittered(_sleepAfterMs);
            _restedMs      = 0;
            return this;
        }

        /// <summary>
        /// Restarts the countdown to <see cref="SleepAfter"/> and, if the avatar is currently
        /// asleep, wakes it with a stretch and a startle before handing it back to whatever it was
        /// doing - which for an auto-idling avatar means drifting between resting poses again.
        /// </summary>
        public PixelAvatar Wake()
        {
            _restedMs      = 0;
            _sleepBudgetMs = PixelAvatarRandom.Jittered(_sleepAfterMs);

            if (!IsAsleep) return this;

            return PlaySequence(WakeSequence);
        }

        /// <summary>
        /// Sets whether the avatar reacts to being clicked on its own: a click plays
        /// <see cref="PixelAvatarAnimation.Interact"/>, a second one inside
        /// <see cref="DoubleClickWindowMs"/> plays <see cref="PixelAvatarAnimation.Startle"/>
        /// instead, and either one on a sleeping cat wakes it with <see cref="Wake"/>. The reaction
        /// hands the avatar back to what it was doing, so an auto-idling cat goes on drifting
        /// between resting poses afterwards.
        ///
        /// It is on by default and turns itself off as soon as the avatar has a click of its own to
        /// do: registering an <see cref="OnClick"/> handler or wrapping the cat in a button with
        /// <see cref="AsButton"/> hands the click to the application. Call this to get the reaction
        /// back on top of that, or to turn it off entirely. A paused avatar never reacts, which is
        /// what keeps a <see cref="PixelAvatarBadge"/> still.
        /// </summary>
        public PixelAvatar ReactToClicks(bool value = true)
        {
            _reactsToClicks = value;

            if (!value) ClearClickTimer();
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the avatar is clicked. Doing so turns off the built-in
        /// click reaction described in <see cref="ReactToClicks"/>, since the click now belongs to
        /// the application.
        /// </summary>
        public override PixelAvatar OnClick(ComponentEventHandler<PixelAvatar, MouseEvent> onClick, bool clearPrevious = true)
        {
            if (onClick != null) _hasClickHandler = true;
            else if (clearPrevious) _hasClickHandler = false;

            return base.OnClick(onClick, clearPrevious);
        }

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
                InnerElement.style.setProperty(VariableName(index), _palette.CssAt(index));
            }

            RenderAccent();
            return this;
        }

        /// <summary>
        /// Sets a custom palette built from the artwork's three shading levels, the way the
        /// single-hue built-in designs are built.
        /// </summary>
        public PixelAvatar SetShades(Color highlight, Color baseColor, Color shadow, Color background = null, string name = "Custom")
        {
            return SetPalette(PixelAvatarPalette.FromShades(name, background, highlight, baseColor, shadow));
        }

        /// <summary>
        /// Recolors a single palette index. This only rewrites that index's CSS variable, so it is
        /// cheap enough to drive from a color picker's input event.
        /// </summary>
        public PixelAvatar SetColor(byte index, Color color)
        {
            if (index == 0 || index > PixelAvatarSprites.PaletteSize || color == null) return this;

            _palette = _palette.WithColor(index, color);
            InnerElement.style.setProperty(VariableName(index), color.ToHex());
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
            RenderAccent();
            _pixelSizeChanged?.Invoke();
            return this;
        }

        /// <summary>
        /// Sets the direction the avatar faces, mirroring the artwork when facing left. The change
        /// is instant; use <see cref="Turn"/> to animate it.
        /// </summary>
        public PixelAvatar Facing(PixelAvatarFacing facing)
        {
            _canvas.classList.remove("tss-pixelavatar-turning");
            _facing = facing;
            _canvas.UpdateClassIf(facing == PixelAvatarFacing.Left, "tss-pixelavatar-mirrored");
            return this;
        }

        /// <summary>
        /// Changes the direction the avatar faces by pivoting it about its vertical axis, so it
        /// reads as the sprite turning around rather than its pixels swapping sides. Facing the
        /// direction it already faces does nothing.
        /// </summary>
        public PixelAvatar Turn(PixelAvatarFacing facing, int durationMs = DefaultTurnDurationMs)
        {
            if (facing == _facing) return this;

            _facing = facing;

            InnerElement.style.setProperty("--tss-pxav-turn", $"{durationMs}ms");
            _canvas.classList.add("tss-pixelavatar-turning");
            _canvas.UpdateClassIf(facing == PixelAvatarFacing.Left, "tss-pixelavatar-mirrored");

            return this;
        }

        /// <summary>
        /// Pivots the avatar to face the other way. See <see cref="Turn"/>.
        /// </summary>
        public PixelAvatar TurnAround(int durationMs = DefaultTurnDurationMs)
        {
            return Turn(_facing == PixelAvatarFacing.Right ? PixelAvatarFacing.Left : PixelAvatarFacing.Right, durationMs);
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
            // Only an explicit Play decides whether the avatar is auto-idling; the hand-overs
            // below go through PlayCore so a drift between resting poses does not cancel it.
            _autoIdle = animation == PixelAvatarAnimation.AutoIdle;
            _sequence = null;

            if (_autoIdle)
            {
                _restedMs      = 0;
                _sleepBudgetMs = PixelAvatarRandom.Jittered(_sleepAfterMs);
            }

            return PlayCore(animation);
        }

        // Plays a scripted run of animations in place of the usual hand-over chain. Tick hands the
        // avatar back to whatever it was doing when the last step finishes, which is what lets a
        // wake-up or a click reaction happen without cancelling auto-idling.
        private PixelAvatar PlaySequence(PixelAvatarAnimation[] sequence)
        {
            _sequence      = sequence;
            _sequenceIndex = 1;
            return PlayCore(sequence[0]);
        }

        // A cat that ignores you is furniture. One click and it looks up; click again while it is
        // still looking and you have startled it.
        private void ReactToClick()
        {
            if (!_reactsToClicks || _hasClickHandler || _paused) return;

            var wasAsleep = IsAsleep;

            // Pushes the sleep countdown back either way, and when the cat was actually out this
            // plays the wake-up performance - which already ends in a startle, so there is no
            // reaction to add on top of it.
            Wake();

            if (wasAsleep)
            {
                ClearClickTimer();
                return;
            }

            if (_clickTimer != 0)
            {
                ClearClickTimer();
                PlaySequence(StartleSequence);
                return;
            }

            // The reaction plays on the first click rather than waiting out the double-click window,
            // so the cat answers immediately; a second click within the window simply overrides it.
            _clickTimer = window.setTimeout(_ => _clickTimer = 0, DoubleClickWindowMs);
            PlaySequence(PokeSequence);
        }

        private void ClearClickTimer()
        {
            if (_clickTimer == 0) return;

            window.clearTimeout(_clickTimer);
            _clickTimer = 0;
        }

        private PixelAvatar PlayCore(PixelAvatarAnimation animation)
        {
            _animation = PixelAvatarSprites.Get(animation);
            _frame     = 0;
            _paused    = false;

            RenderFrame();
            UpdateAriaLabel();
            SyncTimer();

            // The internal hook first, and separately from the public event: OnAnimationStarted
            // clears previous handlers by default, so an app wiring up its own would otherwise
            // silently unsubscribe whatever behaviour is driving this avatar.
            // Both get the resolved pose rather than what was asked for, so a Play(AutoIdle) reports
            // the resting animation that actually started.
            _animationStarted?.Invoke(_animation.Animation);
            AnimationStarted?.Invoke(this, _animation.Animation);
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
        ///
        /// A <see cref="Modal"/> target is not wrapped - see
        /// <see cref="PixelAvatarAttachment.IsAdopted"/>.
        /// </summary>
        public PixelAvatarAttachment AttachTo(IComponent target, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            return new PixelAvatarAttachment(target, this, anchor);
        }

        /// <summary>
        /// Wraps the avatar in a <see cref="Button"/> that has no background, border, padding or
        /// minimum size, so it hugs the avatar exactly instead of the button's usual chrome. The
        /// avatar becomes the button's content via <see cref="Button.ReplaceContent"/> - the cat
        /// itself is the clickable surface - and the button's size tracks <see cref="RenderedWidth"/>
        /// / <see cref="RenderedHeight"/>, so it stays a perfect fit if <see cref="PixelSize"/> later
        /// changes.
        ///
        /// This claims the same pixel-size tracking slot <see cref="AttachTo"/> uses, so an avatar
        /// already attached to another component should not also be turned into a button.
        ///
        /// The button owns the click from here on, so the built-in click reaction is turned off -
        /// see <see cref="ReactToClicks"/>, which turns it back on if you want the cat to answer as
        /// well as act.
        /// </summary>
        public Button AsButton()
        {
            ReactToClicks(false);

            var button = UI.Button()
               .ReplaceContent(this)
               .NoBackground()
               .NoBorder()
               .NoPadding()
               .NoMinSize()
               .NoMargin();

            TrackPixelSize(() => button.Width(RenderedWidth.px()).Height(RenderedHeight.px()));

            return button;
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

        // Lets a PixelAvatarCompanion follow the animation without competing with the public
        // OnAnimationStarted event, which apps are free to take over.
        internal void TrackAnimation(Action<PixelAvatarAnimation> onAnimationStarted, Action<PixelAvatarAnimation> onAnimationFinished = null)
        {
            _animationStarted  = onAnimationStarted;
            _animationFinished = onAnimationFinished;
        }

        private void ApplyPixelSize()
        {
            var size = $"{_pixelSize}px";

            InnerElement.style.width  = $"{_width * _pixelSize}px";
            InnerElement.style.height = $"{_height * _pixelSize}px";
            InnerElement.style.setProperty("--tss-pxav-perspective", $"{_width * _pixelSize * PerspectiveFactor}px");

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

            RenderAccent();
        }

        // The ear tips move from frame to frame, so the two accent squares are repositioned with
        // every repaint rather than placed once.
        private void RenderAccent()
        {
            var accent = _palette == null ? null : _palette.Accent;

            if (accent == null || _animation.Frames.Length == 0)
            {
                _accentLeft.style.display  = "none";
                _accentRight.style.display = "none";
                return;
            }

            var sprite = _animation.Frames[_frame];

            if (!sprite.HasEars)
            {
                _accentLeft.style.display  = "none";
                _accentRight.style.display = "none";
                return;
            }

            var size = _pixelSize * AccentScale;
            var hex  = accent.ToHex();

            PlaceAccent(_accentLeft,  sprite.EarLeftX,  sprite.EarY, size, hex);
            PlaceAccent(_accentRight, sprite.EarRightX, sprite.EarY, size, hex);
        }

        private void PlaceAccent(HTMLElement element, int x, int y, double size, string color)
        {
            element.style.display         = "block";
            element.style.left            = $"{x * _pixelSize}px";
            element.style.top             = $"{y * _pixelSize}px";
            element.style.width           = $"{size}px";
            element.style.height          = $"{size}px";
            element.style.backgroundColor = color;
        }

        private void Tick()
        {
            var frames = _animation.Frames.Length;
            if (frames == 0) return;

            if (_frame + 1 < frames)
            {
                _frame++;
                RenderFrame();
                ScheduleFrame();
                return;
            }

            if (_animation.Loops)
            {
                // A resting animation has just finished one cycle. Auto-idling is the only thing
                // that gets to move the cat somewhere else at this point.
                if (_autoIdle && _animation.Rests)
                {
                    _restedMs += _lastHoldMs + _animation.DurationMs;

                    if (_sleepAfterMs > 0 && _restedMs >= _sleepBudgetMs)
                    {
                        PlayCore(PixelAvatarAnimation.Sleep);
                        return;
                    }

                    if (DriftToRestingPose()) return;
                }

                _frame = 0;
                RenderFrame();
                ScheduleFrame();
                return;
            }

            var finished = _animation;

            // Internal first, for the same reason as _animationStarted: OnAnimationFinished clears
            // previous handlers by default. Either handler may call Play, and the guard below then
            // suppresses the built-in hand-over.
            _animationFinished?.Invoke(finished.Animation);
            AnimationFinished?.Invoke(this, finished.Animation);

            // A handler is allowed to pick the next animation itself, in which case we leave it be.
            if (_animation != finished) return;

            // A scripted run (waking up) chains its own steps in place of the animation's usual
            // follow-up, then hands back to whatever the avatar was doing before it started.
            if (_sequence != null)
            {
                if (_sequenceIndex < _sequence.Length)
                {
                    var step = _sequence[_sequenceIndex];
                    _sequenceIndex++;
                    PlayCore(step);
                    return;
                }

                _sequence = null;
                PlayCore(_autoIdle ? PixelAvatarAnimation.AutoIdle : finished.Next);
                return;
            }

            if (finished.Next == finished.Animation)
            {
                _frame = 0;
                RenderFrame();
                ScheduleFrame();
                return;
            }

            PlayCore(finished.Next);
        }

        // Picks the next resting pose. Returns false when the pick is the pose already showing, so
        // the caller simply replays it rather than restarting the animation from scratch.
        private bool DriftToRestingPose()
        {
            var pick = RestingPoses[PixelAvatarRandom.Next(RestingPoses.Length)];

            if (pick == PixelAvatarAnimation.Idle   && _animation.Animation == PixelAvatarAnimation.Idle)       return false;
            if (pick == PixelAvatarAnimation.Sit    && _animation.Animation == PixelAvatarAnimation.SitIdle)    return false;
            if (pick == PixelAvatarAnimation.Crouch && _animation.Animation == PixelAvatarAnimation.CrouchIdle) return false;

            PlayCore(pick);
            return true;
        }

        private void SyncTimer()
        {
            StopTimer();
            ScheduleFrame();
        }

        // Frames are chained with timeouts rather than driven by one interval, because how long a
        // frame is held is not constant: a resting animation holds its first frame for a random
        // spell so the cat looks still rather than fidgety.
        private void ScheduleFrame()
        {
            StopTimer();

            if (_paused || !_isMounted || _animation.Frames.Length < 2) return;

            var hold = _animation.FrameDurationMs;

            if (_animation.Rests && _frame == 0)
            {
                hold = _restMaxMs > 0
                    ? PixelAvatarRandom.Between(_restMinMs, _restMaxMs)
                    : PixelAvatarRandom.Between(_animation.RestMinMs, _animation.RestMaxMs);

                _lastHoldMs = hold;
            }

            var delay = (int)System.Math.Round(hold / _speed);
            if (delay < 16) delay = 16;

            _timer = window.setTimeout(_ => Tick(), delay);
        }

        private void StopTimer()
        {
            if (_timer == 0) return;

            window.clearTimeout(_timer);
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
    ///
    /// A <see cref="Modal"/> is the one target that is not wrapped: see <see cref="IsAdopted"/>.
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

            // A modal is not laid out by whoever builds it - it centers itself inside its own
            // full-screen container and is put on screen by Show() - so a wrapper around it would
            // simply be dropped. It lends its own box instead, and the avatar hangs off the outside
            // of it, perched on the dialog rather than sitting in a band inside it. That is overlap
            // mode, so a modal turns it on for itself; Overlap(false) moves the cat inside.
            var modal = target as Modal;

            IsAdopted = modal != null;

            if (IsAdopted)
            {
                _host = target.Render();
                _host.classList.add("tss-pixelavatar-adopted");
                _host.classList.add($"{anchor}");
                _host.appendChild(avatar.Render());
                Overlap();

                // So ShowEmbedded can pull the cat back inside: embedded in someone else's layout,
                // there is no room above the modal to hang off.
                modal._pixelAvatar = this;
            }
            else
            {
                _host = Div(Att($"tss-pixelavatar-host {anchor}"), target.Render(), avatar.Render());
            }

            avatar.TrackPixelSize(UpdateReservedSpace);

            // An avatar perched on a top edge with room to walk along gets a life of its own. An
            // OmniBox additionally gives the companion something to react to - typing, and a caret
            // to pad over to - which a modal does not, so that cat only roams.
            var omniBox = Target as OmniBox;

            if ((omniBox != null || IsAdopted) && IsTopAnchor(anchor))
            {
                Companion = new PixelAvatarCompanion(omniBox, avatar, _host, anchor);
            }
        }

        /// <summary>Gets the avatar anchored to the target.</summary>
        public PixelAvatar Avatar { get; }

        /// <summary>Gets the component the avatar is anchored to.</summary>
        public IComponent Target { get; }

        /// <summary>
        /// Gets whether the target lends its own box to the avatar instead of being wrapped, which
        /// is what a <see cref="Modal"/> target does. An adopted target is still the component the
        /// application goes on using - <see cref="Modal.Show"/> and the rest all still apply - and
        /// this attachment's <see cref="Render"/> returns the target's own element. Adopted targets
        /// start in <see cref="Overlap"/> mode so the avatar sits outside the dialog.
        /// </summary>
        public bool IsAdopted { get; }

        /// <summary>
        /// Gets the behaviour driving the avatar, or null when there is none. Set when the anchor is
        /// one of the <c>Top*</c> ones and the target is either an <see cref="OmniBox"/> or a
        /// <see cref="Modal"/>.
        /// </summary>
        public PixelAvatarCompanion Companion { get; }

        /// <summary>Gets the element sizing helpers should style, which is the wrapper itself so the
        /// avatar stays anchored to the target's edges.</summary>
        public HTMLElement StylingContainer => _host;

        /// <summary>Gets whether a sizing helper applied to this component should tag it so a wrapper-building container hoists the style onto the wrapper.</summary>
        public bool PropagateStylesToWrapper => true;

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
        ///
        /// An adopted <see cref="Modal"/> starts in overlap mode, so the cat perches on the outside
        /// of the dialog; <c>Overlap(false)</c> moves it into a reserved band inside instead.
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

        private static bool IsTopAnchor(PixelAvatarAnchor anchor)
        {
            return anchor == PixelAvatarAnchor.TopLeft
                || anchor == PixelAvatarAnchor.TopCenter
                || anchor == PixelAvatarAnchor.TopRight;
        }

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
        /// edges. The avatar is available through <see cref="PixelAvatarAttachment.Avatar"/>. See
        /// <see cref="PixelAvatar(byte, PixelAvatarDesign, PixelAvatarAnimation)"/> for the key.
        /// </summary>
        public static PixelAvatarAttachment WithPixelAvatar(this IComponent component, byte key, PixelAvatarDesign design, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            return new PixelAvatarAttachment(component, new PixelAvatar(key, design), anchor);
        }

        /// <summary>
        /// Perches an avatar on one of the modal's own edges. A modal lends its box to the avatar
        /// rather than being wrapped, so the modal itself is returned and the caller goes on using
        /// it as usual. Reach the attachment - and its companion - through
        /// <see cref="PixelAvatar.AttachTo"/> when you need to tune it.
        /// </summary>
        public static Modal WithPixelAvatar(this Modal modal, PixelAvatar avatar, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            avatar.AttachTo(modal, anchor);
            return modal;
        }

        /// <summary>
        /// Perches a new avatar with the given design on one of the modal's own edges. See
        /// <see cref="PixelAvatar(byte, PixelAvatarDesign, PixelAvatarAnimation)"/> for the key.
        /// </summary>
        public static Modal WithPixelAvatar(this Modal modal, byte key, PixelAvatarDesign design, PixelAvatarAnchor anchor = PixelAvatarAnchor.TopLeft)
        {
            return modal.WithPixelAvatar(new PixelAvatar(key, design), anchor);
        }
    }
}
