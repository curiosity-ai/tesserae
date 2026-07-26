using System;
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
    /// unreadable. The background is the palette's own <see cref="PixelAvatarPalette.Background"/>,
    /// run through the same gradient formula a regular <see cref="Avatar"/> uses.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarBadge")]
    public sealed class PixelAvatarBadge : ComponentBase<PixelAvatarBadge, HTMLElement>
    {
        // The cat is sized so the diagonal of its ink box fits the circle, with a hair to spare -
        // the corners of the pose are drawn, so fitting the width alone would clip an ear against
        // the rim. Anything the badge measures comes from the pose's ink rather than from the 10x8
        // frame, which SitIdle only partly fills.
        private const double DiagonalFill = 0.98;

        private readonly PixelAvatar _avatar;
        private          AvatarSize  _size;
        private          bool        _customBackground;

        /// <summary>
        /// Initializes a new instance of this class for one of the built-in designs.
        /// </summary>
        public PixelAvatarBadge(byte key, PixelAvatarDesign design = PixelAvatarDesign.Black, AvatarSize size = AvatarSize.Medium)
            : this(new PixelAvatar(key, design, PixelAvatarAnimation.SitIdle), size)
        {
        }

        /// <summary>
        /// Initializes a new instance of this class wrapping an existing avatar. The avatar is put
        /// into <see cref="PixelAvatarAnimation.SitIdle"/> and paused.
        /// </summary>
        public PixelAvatarBadge(PixelAvatar avatar, AvatarSize size = AvatarSize.Medium)
        {
            _avatar = avatar ?? throw new ArgumentNullException(nameof(avatar));
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

            LayOutSprite();

            if (!_customBackground) ApplyBackground();

            return this;
        }

        /// <summary>
        /// Sets the design of the cat in the badge. The background follows the new palette unless
        /// <see cref="Background"/> has pinned one.
        /// </summary>
        public PixelAvatarBadge SetDesign(PixelAvatarDesign design)
        {
            _avatar.SetDesign(design);
            if (!_customBackground) ApplyBackground();
            return this;
        }

        /// <summary>
        /// Sets the palette of the cat in the badge. The background follows the new palette unless
        /// <see cref="Background"/> has pinned one.
        /// </summary>
        public PixelAvatarBadge SetPalette(PixelAvatarPalette palette)
        {
            _avatar.SetPalette(palette);
            if (!_customBackground) ApplyBackground();
            return this;
        }

        /// <summary>
        /// Pins the CSS background of the badge instead of taking the palette's. Pass null to go
        /// back to the palette's.
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

        // Scales the pose to the circle and offsets the avatar so the pose's ink lands dead center,
        // rather than centering the frame box the pose only partly fills.
        private void LayOutSprite()
        {
            var diameter = PixelsFor(_size);
            var sprite   = PixelAvatarSprites.Get(PixelAvatarAnimation.SitIdle).Frames[0];
            var diagonal = System.Math.Sqrt(sprite.InkWidth * sprite.InkWidth + sprite.InkHeight * sprite.InkHeight);

            var pixelSize = diagonal <= 0 ? 1 : (int)System.Math.Floor(diameter * DiagonalFill / diagonal);
            if (pixelSize < 1) pixelSize = 1;

            _avatar.PixelSize(pixelSize);

            var element = _avatar.Render();
            element.style.left = $"{diameter / 2.0 - (sprite.InkLeft + sprite.InkWidth / 2.0) * pixelSize}px";
            element.style.top  = $"{diameter / 2.0 - (sprite.InkTop + sprite.InkHeight / 2.0) * pixelSize}px";
        }

        // The badge's gradient sits at a fixed mid lightness, so the halo has to contrast with the
        // coat rather than with the page theme: a dark cat gets a light one and a light cat a dark
        // one. Otherwise a black cat on a mid background would carry a black halo and lose its
        // silhouette.
        private void ApplyBackground()
        {
            var palette = _avatar.Palette;

            InnerElement.style.background = palette.BackgroundGradient();

            var coat = palette.DominantColor();
            _avatar.OutlineColor(coat != null && coat.GetBrightness() < 0.5f ? "rgba(255, 255, 255, 0.55)" : "rgba(0, 0, 0, 0.5)");
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
