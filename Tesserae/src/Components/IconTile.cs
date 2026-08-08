using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The rounded, tinted square that leads a row: a glyph, a few letters ("PPTX", "CSV") or a small
    /// component of the host's own, drawn over a pale wash of one color.
    /// <para>
    /// It is the tile <see cref="OmniResult{T}"/> puts in front of every search result, <see cref="Banner"/>
    /// in front of its message and <see cref="Metric"/> beside its value - one shape, one way of tinting it,
    /// wherever something needs to be marked with what it is.
    /// </para>
    /// <para>
    /// The one color the host passes is the color the glyph keeps; the tile behind it is computed from that
    /// color - a light wash of it under a light theme, a deep one under a dark theme - so a host only ever
    /// picks the color that means something ("red is an error"), never the four that draw it.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.IconTile")]
    public sealed class IconTile : ComponentBase<IconTile, HTMLElement>
    {
        /// <summary>
        /// Initializes a new instance of this class, empty and untinted. Fill it with one of the
        /// <c>SetIcon</c> overloads.
        /// </summary>
        public IconTile()
        {
            InnerElement = Div(Att("tss-icontile"));

            Tint(null);
        }

        /// <summary>
        /// Initializes a new instance of this class showing the given icon, in the given color.
        /// </summary>
        public IconTile(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular) : this()
        {
            SetIcon(icon, color, weight);
        }

        /// <summary>
        /// Initializes a new instance of this class showing the given short text, in the given color.
        /// </summary>
        public IconTile(string text, string color = null, TextSize? size = null) : this()
        {
            SetIcon(text, color, size);
        }

        /// <summary>
        /// Initializes a new instance of this class showing the given component, optionally tinted.
        /// </summary>
        public IconTile(IComponent iconOrImage, string color = null) : this()
        {
            SetIcon(iconOrImage, color);
        }

        /// <summary>
        /// Puts the given icon on the tile, in the given color, over a paler wash of that same color. Pass
        /// the full-strength color the glyph should be - the background is computed from it (and cached), a
        /// light tint of it under a light theme and a deep one under a dark theme. A null color leaves the
        /// tile in the neutral, untinted colors.
        /// </summary>
        public IconTile SetIcon(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            ClearChildren(InnerElement);

            InnerElement.appendChild(I(icon, weight, "tss-icontile-glyph"));

            return Tint(color);
        }

        /// <summary>
        /// Puts the given short text on the tile in place of an icon - a file type, "PPTX" or "CSV", where
        /// no glyph says it as plainly - in the given color, over a paler wash of that same color. It is drawn
        /// at the size the tile is sized for unless <paramref name="size"/> asks for another one - text longer
        /// than the three or four letters a type name usually is wants <see cref="TextSize.Tiny"/>.
        /// </summary>
        public IconTile SetIcon(string text, string color = null, TextSize? size = null)
        {
            ClearChildren(InnerElement);

            var className = size.HasValue ? $"tss-icontile-text {size.Value}" : "tss-icontile-text";

            InnerElement.appendChild(Span(Att(className, text: text ?? string.Empty)));

            return Tint(color);
        }

        /// <summary>
        /// Puts the given component on the tile - an <see cref="Image"/> thumbnail, an <see cref="Avatar"/>,
        /// an emoji - optionally tinting the tile with the given color.
        /// </summary>
        public IconTile SetIcon(IComponent iconOrImage, string color = null)
        {
            ClearChildren(InnerElement);

            if (iconOrImage != null) InnerElement.appendChild(iconOrImage.Render());

            return Tint(color);
        }

        /// <summary>
        /// Sets how big the tile is drawn - 34px square by default, the size a result row wants. The glyph
        /// (or the letters) inside it scale with it unless <see cref="GlyphSize(UnitSize)"/> says otherwise.
        /// </summary>
        public IconTile Size(UnitSize size)
        {
            InnerElement.style.setProperty("--tss-icontile-size", size is object ? size.ToString() : "34px");

            return this;
        }

        /// <summary>
        /// Sets how big the glyph - or the text - inside the tile is drawn. By default it follows the tile's
        /// own size (about 45% of it), which is what keeps a bigger tile from holding a tiny icon.
        /// </summary>
        public IconTile GlyphSize(UnitSize size)
        {
            InnerElement.style.setProperty("--tss-icontile-glyph-size", size is object ? size.ToString() : "");

            return this;
        }

        /// <summary>
        /// Sets how round the tile's corners are - 8px by default. Pass <c>50.percent()</c> for a circle.
        /// </summary>
        public IconTile Rounded(UnitSize radius)
        {
            InnerElement.style.setProperty("--tss-icontile-radius", radius is object ? radius.ToString() : "8px");

            return this;
        }

        /// <summary>
        /// Draws the tile as a circle.
        /// </summary>
        public IconTile Circular() => Rounded(50.percent());

        /// <summary>
        /// Re-tints the tile from the given color without touching what is on it. A null or empty color puts
        /// it back in the neutral, untinted colors.
        /// </summary>
        public IconTile Tint(string color)
        {
            InnerElement.classList.remove("tss-icontile-plain");

            if (string.IsNullOrEmpty(color))
            {
                InnerElement.classList.add("tss-icontile-plain");
                return this;
            }

            var tint = IconTints.For(color);

            InnerElement.style.setProperty("--tss-icontile-background",      tint.Background);
            InnerElement.style.setProperty("--tss-icontile-foreground",      tint.Foreground);
            InnerElement.style.setProperty("--tss-icontile-background-dark", tint.BackgroundDark);
            InnerElement.style.setProperty("--tss-icontile-foreground-dark", tint.ForegroundDark);

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }

    /// <summary>
    /// The colors an <see cref="IconTile"/> is drawn with, derived from the one color the host passed: the
    /// glyph in that color and the tile in a wash of it, in a light and a dark variant.
    /// </summary>
    internal sealed class IconTint
    {
        internal IconTint(string background, string foreground, string backgroundDark, string foregroundDark)
        {
            Background     = background;
            Foreground     = foreground;
            BackgroundDark = backgroundDark;
            ForegroundDark = foregroundDark;
        }

        internal string Background     { get; }
        internal string Foreground     { get; }
        internal string BackgroundDark { get; }
        internal string ForegroundDark { get; }
    }

    /// <summary>
    /// Computes - and remembers - the tile colors derived from a given icon color. A list of results
    /// usually draws the same handful of colors over and over (one per file type), and every one of them
    /// costs a parse and two HSL round-trips, so the results are cached by the color they came from.
    /// </summary>
    internal static class IconTints
    {
        private static readonly Dictionary<string, IconTint> _cache = new Dictionary<string, IconTint>();

        internal static IconTint For(string color)
        {
            if (_cache.TryGetValue(color, out var cached)) return cached;

            var tint = Compute(color);

            _cache[color] = tint;

            return tint;
        }

        private static IconTint Compute(string color)
        {
            try
            {
                var parsed     = Color.FromString(color);
                var hue        = parsed.GetHue();
                var saturation = parsed.GetSaturation();
                var lightness  = parsed.GetBrightness();

                // Light theme: a pale wash of the color under the glyph, which keeps the color it was given.
                var background = Color.FromHsl(hue, Math.Min(saturation, 0.85f), 0.925f).ToHex();

                // Dark theme: the wash goes deep instead of pale, and the glyph is lifted until it reads
                // against it. A grey (unsaturated) color stays grey through both.
                var backgroundDark = Color.FromHsl(hue, Math.Min(saturation, 0.5f),  0.19f).ToHex();
                var foregroundDark = Color.FromHsl(hue, Math.Min(saturation, 0.85f), Math.Max(lightness, 0.68f)).ToHex();

                return new IconTint(background, color, backgroundDark, foregroundDark);
            }
            catch (Exception)
            {
                // Not a color this can take apart (a gradient, a color function, an unknown keyword): mix it
                // down for the wash instead of computing one, and let the glyph keep it as it was given.
                return new IconTint(
                    $"color-mix(in srgb, {color} 14%, transparent)",
                    color,
                    $"color-mix(in srgb, {color} 24%, transparent)",
                    color);
            }
        }
    }
}
