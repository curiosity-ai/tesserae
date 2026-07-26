using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A <see cref="PixelAvatar"/> dressed as a round profile picture, sized with the same
    /// <see cref="AvatarSize"/> presets as <see cref="Avatar"/> so the two can sit side by side in
    /// a <see cref="ChatMessage"/> or a list.
    ///
    /// The cat sits still — <see cref="PixelAvatarAnimation.SitIdle"/>, held on its first frame —
    /// because a badge is an identity, not an animation, and a transcript full of moving cats is
    /// unreadable. The background is derived from the coat, so it always belongs to the cat in
    /// front of it.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarBadge")]
    public sealed class PixelAvatarBadge : ComponentBase<PixelAvatarBadge, HTMLElement>
    {
        // How much of the badge's width the sprite spans. Leaves a margin so the cat does not
        // touch the rim of the circle at any size.
        private const double SpriteFill = 0.68;

        private readonly PixelAvatar _avatar;
        private          AvatarSize  _size;
        private          bool        _customBackground;

        /// <summary>
        /// Initializes a new instance of this class for one of the built-in designs.
        /// </summary>
        public PixelAvatarBadge(PixelAvatarDesign design = PixelAvatarDesign.Black, AvatarSize size = AvatarSize.Medium)
            : this(new PixelAvatar(design, PixelAvatarAnimation.SitIdle), size)
        {
        }

        /// <summary>
        /// Initializes a new instance of this class wrapping an existing avatar. The avatar is put
        /// into <see cref="PixelAvatarAnimation.SitIdle"/> and paused.
        /// </summary>
        public PixelAvatarBadge(PixelAvatar avatar, AvatarSize size = AvatarSize.Medium)
        {
            _avatar = avatar ?? new PixelAvatar();
            _avatar.Play(PixelAvatarAnimation.SitIdle).GoToFrame(0).Pause();

            InnerElement = Div(Att("tss-pixelavatar-badge", role: "img"), _avatar.Render());

            Size(size);
        }

        /// <summary>Gets the avatar shown in the badge.</summary>
        public PixelAvatar Avatar => _avatar;

        /// <summary>Gets or sets the size preset of the badge.</summary>
        public AvatarSize SizeValue
        {
            get => _size;
            set => Size(value);
        }

        /// <summary>
        /// Sets the size of the badge, matching the <see cref="Avatar"/> presets.
        /// </summary>
        public PixelAvatarBadge Size(AvatarSize size)
        {
            _size = size;

            InnerElement.classList.remove("tss-avatar-xs", "tss-avatar-sm", "tss-avatar-md", "tss-avatar-lg", "tss-avatar-xl");
            InnerElement.classList.add(ClassFor(size));

            _avatar.PixelSize((int)System.Math.Round(PixelsFor(size) * SpriteFill / PixelAvatarSprites.FrameWidth));

            if (!_customBackground) ApplyBackground();

            return this;
        }

        /// <summary>
        /// Sets the design of the cat in the badge, and re-derives the background from it unless
        /// <see cref="Background"/> has pinned one.
        /// </summary>
        public PixelAvatarBadge SetDesign(PixelAvatarDesign design)
        {
            _avatar.SetDesign(design);
            if (!_customBackground) ApplyBackground();
            return this;
        }

        /// <summary>
        /// Sets the palette of the cat in the badge, and re-derives the background from it unless
        /// <see cref="Background"/> has pinned one.
        /// </summary>
        public PixelAvatarBadge SetPalette(PixelAvatarPalette palette)
        {
            _avatar.SetPalette(palette);
            if (!_customBackground) ApplyBackground();
            return this;
        }

        /// <summary>
        /// Pins the CSS background of the badge instead of deriving it from the coat. Pass null to
        /// go back to the derived one.
        /// </summary>
        public PixelAvatarBadge Background(string background)
        {
            _customBackground = !string.IsNullOrEmpty(background);

            if (_customBackground)
            {
                InnerElement.style.background = background;
            }
            else
            {
                ApplyBackground();
            }

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Builds the badge background for a palette: the hue of the color that covers most of the
        /// sprite, with the lightness pushed the other way so the largest area of the cat always
        /// contrasts against it, and washed-out coats nudged up to something colorful.
        /// </summary>
        public static string BackgroundFor(PixelAvatarPalette palette)
        {
            var coat       = Color.FromString(palette.DominantColor());
            var hue        = coat.GetHue();
            var saturation = coat.GetSaturation();

            saturation = saturation < 0.35f ? 0.35f : saturation > 0.6f ? 0.6f : saturation;

            var lightness = coat.GetBrightness() > 0.55f ? 0.30f : 0.84f;
            var second    = lightness > 0.5f ? lightness - 0.12f : lightness + 0.12f;

            return $"linear-gradient(135deg, {Color.FromHsl(hue, saturation, lightness).ToHex()}, {Color.FromHsl(hue + 35, saturation, second).ToHex()})";
        }

        private void ApplyBackground()
        {
            InnerElement.style.background = BackgroundFor(_avatar.Palette);
        }

        private static string ClassFor(AvatarSize size)
        {
            switch (size)
            {
                case AvatarSize.XSmall: return "tss-avatar-xs";
                case AvatarSize.Small:  return "tss-avatar-sm";
                case AvatarSize.Large:  return "tss-avatar-lg";
                case AvatarSize.XLarge: return "tss-avatar-xl";
                default:                return "tss-avatar-md";
            }
        }

        // Kept in step with the widths in tss.avatar.css.
        private static int PixelsFor(AvatarSize size)
        {
            switch (size)
            {
                case AvatarSize.XSmall: return 24;
                case AvatarSize.Small:  return 32;
                case AvatarSize.Large:  return 56;
                case AvatarSize.XLarge: return 72;
                default:                return 40;
            }
        }
    }
}
