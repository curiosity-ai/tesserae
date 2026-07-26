using System;
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

        /// <summary>Gets the left edge of the frame's non-transparent pixels.</summary>
        public int InkLeft { get { MeasureInk(); return _inkLeft; } }

        /// <summary>Gets the top edge of the frame's non-transparent pixels.</summary>
        public int InkTop { get { MeasureInk(); return _inkTop; } }

        /// <summary>Gets the width of the frame's non-transparent pixels, or 0 if there are none.</summary>
        public int InkWidth { get { MeasureInk(); return _inkWidth; } }

        /// <summary>Gets the height of the frame's non-transparent pixels, or 0 if there are none.</summary>
        public int InkHeight { get { MeasureInk(); return _inkHeight; } }

        // Frames share one 10x8 box so they stay aligned while animating, which means an individual
        // pose sits wherever it sits inside it - SitIdle, for one, is a 6x6 cat a whole pixel left
        // of the box's center. Anything that has to center a single frame needs these, not the box.
        private void MeasureInk()
        {
            if (_inkMeasured) return;
            _inkMeasured = true;

            int minX = Width, minY = Height, maxX = -1, maxY = -1;

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    if (Pixels[y * Width + x] == 0) continue;

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < 0) return;

            _inkLeft   = minX;
            _inkTop    = minY;
            _inkWidth  = maxX - minX + 1;
            _inkHeight = maxY - minY + 1;
        }

        private bool _inkMeasured;
        private int  _inkLeft;
        private int  _inkTop;
        private int  _inkWidth;
        private int  _inkHeight;
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
    /// The colors a <see cref="PixelAvatar"/> paints its sprite with, plus the background an
    /// avatar-shaped host such as <see cref="PixelAvatarBadge"/> should sit it on. Palette index 0
    /// is always transparent and is not stored, so <see cref="Colors"/>[0] is the color for
    /// index 1. Instances are immutable; the <c>With*</c> methods return modified copies.
    /// </summary>
    [Transpose.Name("tss.PixelAvatarPalette")]
    public sealed class PixelAvatarPalette
    {
        /// <summary>
        /// Initializes a new instance of this class from colors for palette indices 1..N.
        /// </summary>
        /// <param name="name">The name of the palette.</param>
        /// <param name="colors">Exactly <see cref="PixelAvatarSprites.PaletteSize"/> colors.</param>
        /// <param name="background">
        /// The avatar background color, or null to derive one from <see cref="DominantColor"/>.
        /// </param>
        public PixelAvatarPalette(string name, Color[] colors, Color background = null)
        {
            if (colors == null) throw new ArgumentNullException(nameof(colors));

            if (colors.Length != PixelAvatarSprites.PaletteSize)
            {
                throw new ArgumentException($"A palette needs exactly {PixelAvatarSprites.PaletteSize} colors, one per palette index, but {colors.Length} were given.", nameof(colors));
            }

            Name       = name;
            Colors     = colors;
            Background = background ?? DominantColor();
        }

        /// <summary>Gets the name of the palette.</summary>
        public string Name { get; }

        /// <summary>Gets the colors of palette indices 1..N, in order.</summary>
        public Color[] Colors { get; }

        /// <summary>
        /// Gets the background color an avatar-shaped host paints behind the sprite. Only its hue
        /// is used by <see cref="BackgroundGradient"/>, which matches how <see cref="Avatar"/>
        /// colors itself.
        /// </summary>
        public Color Background { get; }

        /// <summary>
        /// Returns the color for a palette index, or null for the transparent index 0 and for
        /// indices this palette does not define.
        /// </summary>
        public Color ColorAt(byte index)
        {
            if (index == 0 || index > Colors.Length) return null;
            return Colors[index - 1];
        }

        /// <summary>
        /// Returns the CSS color for a palette index, or an empty string for the transparent
        /// index 0 and for indices this palette does not define.
        /// </summary>
        public string CssAt(byte index)
        {
            var color = ColorAt(index);
            return color == null ? string.Empty : color.ToHex();
        }

        /// <summary>
        /// Returns the CSS background for this palette, built from <see cref="Background"/> by the
        /// same <see cref="Avatar.GradientForHue"/> the regular avatar uses, so a pixel-art badge
        /// and an initials avatar look like they came out of the same set.
        /// </summary>
        public string BackgroundGradient() => Avatar.GradientForColor(Background);

        /// <summary>
        /// Returns the color that covers the most of the sprite, weighing each index by
        /// <see cref="PixelAvatarSprites.PixelCounts"/> and adding up indices that share a color.
        /// Used to pick a background when one is not given.
        /// </summary>
        public Color DominantColor()
        {
            var dominant = Colors.Length == 0 ? null : Colors[0];
            var best     = -1;

            for (var i = 0; i < Colors.Length; i++)
            {
                var total = 0;

                for (var j = 0; j < Colors.Length; j++)
                {
                    if (Colors[j].ToHex() == Colors[i].ToHex()) total += PixelAvatarSprites.PixelCounts[j + 1];
                }

                if (total > best)
                {
                    best     = total;
                    dominant = Colors[i];
                }
            }

            return dominant;
        }

        /// <summary>
        /// Returns a copy of this palette with one index recolored.
        /// </summary>
        public PixelAvatarPalette WithColor(byte index, Color color)
        {
            if (index == 0 || index > Colors.Length) return this;

            var colors = new Color[Colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Colors[i];
            }

            colors[index - 1] = color;
            return new PixelAvatarPalette(Name, colors, Background);
        }

        /// <summary>
        /// Returns a copy of this palette with a different avatar background color. This is how a
        /// custom palette picks the background its badge sits on; pass null to go back to one
        /// derived from the coat.
        /// </summary>
        public PixelAvatarPalette WithBackground(Color background) => new PixelAvatarPalette(Name, Colors, background);

        /// <summary>
        /// Returns a copy of this palette under a different name.
        /// </summary>
        public PixelAvatarPalette WithName(string name) => new PixelAvatarPalette(name, Colors, Background);

        /// <summary>
        /// Returns the palette as a comma-separated list of CSS colors.
        /// </summary>
        public override string ToString()
        {
            var hex = new string[Colors.Length];
            for (var i = 0; i < hex.Length; i++)
            {
                hex[i] = Colors[i].ToHex();
            }

            return string.Join(", ", hex);
        }

        /// <summary>
        /// Returns C# source that reconstructs this palette, for pasting into an application.
        /// </summary>
        public string ToCode()
        {
            var quoted = new string[Colors.Length];
            for (var i = 0; i < quoted.Length; i++)
            {
                quoted[i] = $"Color.FromString(\"{Colors[i].ToHex()}\")";
            }

            // Transpose does not unescape {{ / }} inside interpolated strings, so the braces are
            // concatenated in rather than escaped.
            return "PixelAvatarPalette.FromColors(\"" + Name + "\", Color.FromString(\"" + Background.ToHex() + "\"), "
                 + string.Join(", ", quoted) + ")";
        }

        /// <summary>
        /// Builds a palette from every color of the sprite.
        /// </summary>
        /// <param name="name">The name of the palette.</param>
        /// <param name="background">The avatar background color, or null to derive one from the coat.</param>
        /// <param name="colors">Exactly <see cref="PixelAvatarSprites.PaletteSize"/> colors, for palette indices 1..N.</param>
        /// <exception cref="ArgumentException">Thrown when the wrong number of colors is given.</exception>
        public static PixelAvatarPalette FromColors(string name, Color background, params Color[] colors)
        {
            return new PixelAvatarPalette(name, colors, background);
        }

        /// <summary>
        /// Builds a full palette from just the artwork's three shading levels, the way the
        /// single-hue built-in designs are built. Every palette index is filled in according to
        /// <see cref="PixelAvatarSprites.ShadeOf"/>.
        /// </summary>
        /// <param name="name">The name of the palette.</param>
        /// <param name="background">The avatar background color, or null to derive one from the coat.</param>
        public static PixelAvatarPalette FromShades(string name, Color background, Color highlight, Color baseColor, Color shadow)
        {
            var colors = new Color[PixelAvatarSprites.PaletteSize];

            for (byte index = 1; index <= PixelAvatarSprites.PaletteSize; index++)
            {
                var shade = PixelAvatarSprites.ShadeOf(index);
                colors[index - 1] = shade == PixelAvatarShade.Highlight ? highlight
                                  : shade == PixelAvatarShade.Base      ? baseColor
                                                                        : shadow;
            }

            return new PixelAvatarPalette(name, colors, background);
        }
    }
}
