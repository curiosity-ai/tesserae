using Transpose;

namespace Tesserae
{
    // PixelAvatarDesign lives in the generated PixelAvatar.Palettes.cs, next to the palettes it
    // names, so that adding a design stays a single edit.

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
    /// The three shading levels the artwork is drawn with. Every palette index belongs to exactly
    /// one of them, which is why a whole coat can be described by just three colors — see
    /// <see cref="PixelAvatarSprites.ShadeOf"/> and <see cref="PixelAvatarPalette.FromShades"/>.
    /// </summary>
    [Enum(Emit.StringName)]
    [Transpose.Name("tss.PixelAvatarShade")]
    public enum PixelAvatarShade
    {
        [Name("Highlight")] Highlight,
        [Name("Base")]      Base,
        [Name("Shadow")]    Shadow
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

        /// <summary>
        /// Returns a copy of this palette with one index recolored. Palettes are immutable, so this
        /// is how an editor builds up a custom coat.
        /// </summary>
        public PixelAvatarPalette WithColor(byte index, string color)
        {
            if (index == 0 || index > Colors.Length) return this;

            var colors = new string[Colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Colors[i];
            }

            colors[index - 1] = color;
            return new PixelAvatarPalette(Name, colors);
        }

        /// <summary>
        /// Returns a copy of this palette under a different name.
        /// </summary>
        public PixelAvatarPalette WithName(string name) => new PixelAvatarPalette(name, Colors);

        /// <summary>
        /// Returns a copy of this palette with every color shifted in HSL space. The deltas are
        /// relative, so all-zero returns the same colors: hue wraps around in degrees, while
        /// saturation and lightness are added as percentage points and clamped to 0..100. Shifting
        /// the whole palette together keeps the shading relationships that make the sprite read as
        /// one coat, which recoloring each index by hand does not.
        /// </summary>
        public PixelAvatarPalette Adjust(int hueDelta, int saturationDelta, int lightnessDelta)
        {
            if (hueDelta == 0 && saturationDelta == 0 && lightnessDelta == 0) return this;

            var colors = new string[Colors.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                var color = Color.FromString(Colors[i]);

                colors[i] = Color.FromHsl(
                    color.GetHue() + hueDelta,
                    color.GetSaturation() + saturationDelta / 100f,
                    color.GetBrightness() + lightnessDelta / 100f).ToHex();
            }

            return new PixelAvatarPalette(Name, colors);
        }

        /// <summary>
        /// Returns the palette as a comma-separated list of CSS colors, which is the format
        /// <see cref="Parse"/> reads back.
        /// </summary>
        public override string ToString() => string.Join(", ", Colors);

        /// <summary>
        /// Returns C# source that reconstructs this palette, for pasting into an application.
        /// </summary>
        public string ToCode()
        {
            var quoted = new string[Colors.Length];
            for (var i = 0; i < quoted.Length; i++)
            {
                quoted[i] = $"\"{Colors[i]}\"";
            }

            // Transpose does not unescape {{ / }} inside interpolated strings, so the braces of the
            // array initializer are concatenated in rather than escaped.
            return "new PixelAvatarPalette(\"" + Name + "\", new[] { " + string.Join(", ", quoted) + " })";
        }

        /// <summary>
        /// Builds a full palette from just the artwork's three shading levels, the way the
        /// single-hue built-in designs are built. Every palette index is filled in according to
        /// <see cref="PixelAvatarSprites.ShadeOf"/>.
        /// </summary>
        public static PixelAvatarPalette FromShades(string highlight, string baseColor, string shadow, string name = "Custom")
        {
            var colors = new string[PixelAvatarSprites.PaletteSize];

            for (byte index = 1; index <= PixelAvatarSprites.PaletteSize; index++)
            {
                var shade = PixelAvatarSprites.ShadeOf(index);
                colors[index - 1] = shade == PixelAvatarShade.Highlight ? highlight
                                  : shade == PixelAvatarShade.Base      ? baseColor
                                                                        : shadow;
            }

            return new PixelAvatarPalette(name, colors);
        }

        /// <summary>
        /// Reads a palette from a list of CSS colors separated by commas, semicolons or whitespace.
        /// Two lengths are accepted: <see cref="PixelAvatarSprites.PaletteSize"/> colors map
        /// straight onto palette indices 1..N, and exactly three are read as
        /// highlight/base/shadow and expanded through <see cref="FromShades"/>. Returns null for
        /// anything else, so callers can report a bad paste rather than render a broken cat.
        /// </summary>
        public static PixelAvatarPalette Parse(string colors, string name = "Custom")
        {
            if (string.IsNullOrWhiteSpace(colors)) return null;

            var parts = colors.Split(new[] { ',', ';', ' ', '\t', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            if (parts.Length == 3)
            {
                return FromShades(parts[0], parts[1], parts[2], name);
            }

            if (parts.Length == PixelAvatarSprites.PaletteSize)
            {
                return new PixelAvatarPalette(name, parts);
            }

            return null;
        }
    }
}
