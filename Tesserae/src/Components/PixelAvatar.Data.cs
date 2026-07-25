using Transpose;

namespace Tesserae
{
    /// <summary>
    /// The available coat designs for a <see cref="PixelAvatar"/>. All designs share the same
    /// artwork and differ only in their <see cref="PixelAvatarPalette"/>.
    /// </summary>
    [Enum(Emit.StringName)]
    [Transpose.Name("tss.PixelAvatarDesign")]
    public enum PixelAvatarDesign
    {
        [Name("Black")]         Black,
        [Name("Orange")]        Orange,
        [Name("White")]         White,
        [Name("Beige")]         Beige,
        [Name("Siamese")]       Siamese,
        [Name("SpottedGrey")]   SpottedGrey,
        [Name("SpottedOrange")] SpottedOrange,
        [Name("Tuxedo")]        Tuxedo
    }

    /// <summary>
    /// The animations a <see cref="PixelAvatar"/> can play. The four <c>*Idle</c> animations loop
    /// forever; the others play once and then hand over to a follow-up animation (for example
    /// <see cref="Sit"/> settles into <see cref="SitIdle"/>).
    /// </summary>
    [Enum(Emit.StringName)]
    [Transpose.Name("tss.PixelAvatarAnimation")]
    public enum PixelAvatarAnimation
    {
        [Name("Move")]       Move,
        [Name("Idle")]       Idle,
        [Name("Interact")]   Interact,
        [Name("JumpUp")]     JumpUp,
        [Name("JumpDown")]   JumpDown,
        [Name("Startle")]    Startle,
        [Name("Stretch")]    Stretch,
        [Name("Sit")]        Sit,
        [Name("SitIdle")]    SitIdle,
        [Name("Crouch")]     Crouch,
        [Name("CrouchIdle")] CrouchIdle,
        [Name("Sleep")]      Sleep,
        [Name("SleepIdle")]  SleepIdle
    }

    /// <summary>
    /// The direction a <see cref="PixelAvatar"/> faces. The artwork is drawn facing
    /// <see cref="Right"/>; <see cref="Left"/> mirrors it horizontally.
    /// </summary>
    [Enum(Emit.StringName)]
    [Transpose.Name("tss.PixelAvatarFacing")]
    public enum PixelAvatarFacing
    {
        [Name("Right")] Right,
        [Name("Left")]  Left
    }

    /// <summary>
    /// Where a <see cref="PixelAvatar"/> is placed relative to the component it is attached to.
    /// The <c>Top*</c> and <c>Bottom*</c> anchors perch the avatar just outside the target's edge,
    /// so a cat attached with <see cref="TopLeft"/> appears to be sitting on top of it.
    /// </summary>
    [Enum(Emit.StringName)]
    [Transpose.Name("tss.PixelAvatarAnchor")]
    public enum PixelAvatarAnchor
    {
        [Name("tss-pixelavatar-anchor-topleft")]      TopLeft,
        [Name("tss-pixelavatar-anchor-topcenter")]    TopCenter,
        [Name("tss-pixelavatar-anchor-topright")]     TopRight,
        [Name("tss-pixelavatar-anchor-bottomleft")]   BottomLeft,
        [Name("tss-pixelavatar-anchor-bottomcenter")] BottomCenter,
        [Name("tss-pixelavatar-anchor-bottomright")]  BottomRight,
        [Name("tss-pixelavatar-anchor-leftcenter")]   LeftCenter,
        [Name("tss-pixelavatar-anchor-rightcenter")]  RightCenter
    }

    /// <summary>
    /// A single animation frame: a grid of palette indices, where 0 means transparent and any
    /// other value indexes into a <see cref="PixelAvatarPalette"/>.
    /// </summary>
    [Transpose.Name("tss.PixelSprite")]
    public sealed class PixelSprite
    {
        /// <summary>
        /// Initializes a new instance of this class from a row-major grid of palette indices.
        /// </summary>
        public PixelSprite(int width, int height, byte[] pixels)
        {
            Width  = width;
            Height = height;
            Pixels = pixels;
        }

        /// <summary>Gets the width of the frame, in sprite pixels.</summary>
        public int Width { get; }

        /// <summary>Gets the height of the frame, in sprite pixels.</summary>
        public int Height { get; }

        /// <summary>
        /// Gets the palette index of every pixel, row by row (so the pixel at <c>(x, y)</c> lives
        /// at <c>y * Width + x</c>). A value of 0 means the pixel is transparent.
        /// </summary>
        public byte[] Pixels { get; }

        /// <summary>
        /// Returns the palette index at the given coordinates, or 0 when out of bounds.
        /// </summary>
        public byte At(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return 0;
            return Pixels[y * Width + x];
        }
    }

    /// <summary>
    /// A sequence of <see cref="PixelSprite"/> frames plus the timing and chaining rules used to
    /// play them.
    /// </summary>
    [Transpose.Name("tss.PixelSpriteAnimation")]
    public sealed class PixelSpriteAnimation
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public PixelSpriteAnimation(PixelAvatarAnimation animation, PixelSprite[] frames, int frameDurationMs, bool loops, PixelAvatarAnimation next)
        {
            Animation       = animation;
            Frames          = frames;
            FrameDurationMs = frameDurationMs;
            Loops           = loops;
            Next            = next;
        }

        /// <summary>Gets the animation these frames belong to.</summary>
        public PixelAvatarAnimation Animation { get; }

        /// <summary>Gets the frames, in playback order.</summary>
        public PixelSprite[] Frames { get; }

        /// <summary>Gets how long each frame is shown, in milliseconds.</summary>
        public int FrameDurationMs { get; }

        /// <summary>Gets whether playback restarts from the first frame after the last one.</summary>
        public bool Loops { get; }

        /// <summary>
        /// Gets the animation that takes over once this one finishes. Only meaningful when
        /// <see cref="Loops"/> is false.
        /// </summary>
        public PixelAvatarAnimation Next { get; }

        /// <summary>Gets how long a full cycle of this animation takes, in milliseconds.</summary>
        public int DurationMs => Frames.Length * FrameDurationMs;
    }

    /// <summary>
    /// The colors a <see cref="PixelAvatar"/> paints its sprite with. Palette index 0 is always
    /// transparent and is not stored, so <see cref="Colors"/>[0] is the color for index 1.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarPalette")]
    public sealed class PixelAvatarPalette
    {
        /// <summary>
        /// Initializes a new instance of this class from CSS colors for palette indices 1..N.
        /// </summary>
        public PixelAvatarPalette(string name, string[] colors)
        {
            Name   = name;
            Colors = colors;
        }

        /// <summary>Gets the name of the palette.</summary>
        public string Name { get; }

        /// <summary>Gets the CSS color of palette indices 1..N, in order.</summary>
        public string[] Colors { get; }

        /// <summary>
        /// Returns the CSS color for a palette index, or an empty string for the transparent
        /// index 0 and for indices this palette does not define.
        /// </summary>
        public string ColorAt(byte index)
        {
            if (index == 0 || index > Colors.Length) return string.Empty;
            return Colors[index - 1];
        }
    }
}
