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
        /// at the size the tile is sized for, shrunk to fit when the word is wider than the tile: the text is
        /// measured, so what fits is decided by the letters themselves rather than by counting them, and a
        /// three-letter type keeps the full size whatever the tile's size is. Pass <paramref name="size"/> to
        /// pin a <see cref="TextSize"/> instead, which opts the text out of the fitting.
        /// </summary>
        public IconTile SetIcon(string text, string color = null, TextSize? size = null)
        {
            ClearChildren(InnerElement);

            var className = size.HasValue ? $"tss-icontile-text {size.Value}" : "tss-icontile-text";

            InnerElement.appendChild(Span(Att(className, text: text ?? string.Empty)));

            //A pinned TextSize is the host saying how big the letters are, so it opts out of the fitting.
            if (size.HasValue)
            {
                InnerElement.style.removeProperty("--tss-icontile-text-fit");
            }
            else
            {
                IconTileTextFit.Apply(InnerElement, text);
            }

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
    /// How much an <see cref="IconTile"/> has to scale its letters down to keep them inside the tile. The
    /// tile clips what does not fit (<c>overflow: hidden</c>), and three bold capitals are all the 35% of
    /// the tile's side the stylesheet draws them at has room for - so "PPTX" or "PARQUET" would be cut off.
    /// <para>
    /// The answer is a scale rather than a size, which is what lets one measurement serve every tile: both
    /// the room and the letters scale with the tile, so the tile's own size cancels out of the comparison
    /// and the same word fits identically at 22px and at 34px. It is published as
    /// <c>--tss-icontile-text-fit</c>, and <c>tss.icontile.css</c> hands it to a <c>scale()</c>, which
    /// composes with whatever font size the cascade chose for that tile instead of overriding it.
    /// </para>
    /// <para>
    /// The width is measured, so it is the letters that decide and not how many of them there are ("MAIL"
    /// is a fifth narrower than "DRAW"). It is measured on a canvas rather than by laying a hidden span out
    /// and reading its box: same shaper, same numbers to within a hundredth of a percent, but nothing is
    /// added to the document and no layout is forced in the middle of building a list of results.
    /// </para>
    /// <para>
    /// The words a tile is given are file types, and there are only so many of those - so the ones listed in
    /// <c>_known</c> were measured once, on the font stack Tesserae draws with, and ship as numbers. A tile
    /// holding one of them measures nothing at all; anything else is measured on first sight and remembered.
    /// </para>
    /// </summary>
    internal static class IconTileTextFit
    {
        //Kept in step with tss.icontile.css: the size the text is drawn at, how much of the tile's side the
        //letters may take before they are touching its rounded corners, and the weight and tracking they are
        //drawn with - a canvas knows nothing of the class the tile's span carries, so it is told.
        private const double STANDARD_FRACTION = 0.35;
        private const double ROOM_FRACTION     = 0.86;
        private const string FONT_WEIGHT       = "700";
        private const double LETTER_SPACING_EM = 0.02;

        //The font size the word is measured at - big enough that a pixel of rounding doesn't matter.
        private const double MEASURED_AT = 100.0;

        //A tile spells out a file type, so the same handful of words come back over and over and remembering
        //them is what keeps a long list to one measurement each. A host passing text that is different every
        //time would grow this without bound instead, so it starts over rather than growing past a list of
        //type names.
        private const int MAX_REMEMBERED = 512;

        //The word the shipped table is checked against before any of it is believed. A host drawing in a font
        //of its own would make every number in it slightly wrong - out by a percent or two between two
        //grotesques, by a fifth against something condensed - so one measurement of a word whose answer we
        //claim to know decides whether the rest of the table is about this font at all.
        private const string PROBE               = "PDF";
        private const double PROBE_TOLERANCE     = 0.01;

        //Measured with Build's canvas on "Segoe UI", SegoeUI, "Helvetica Neue", Helvetica, Arial, sans-serif
        //at weight 700 with 0.02em tracking - the width of the word per pixel of font size. Every file type
        //mosaik spells out, the short extensions its unknown-type tile falls back to, and the ones the
        //sample gallery draws. Regenerate rather than hand-edit: the numbers are what a browser said.
        private static readonly Dictionary<string, double> _known = new Dictionary<string, double>()
        {
            ["PPT"]  = 2.005, ["DOC"]  = 2.282, ["PDF"]  = 2.060, ["XLS"]  = 2.005,
            ["MAIL"] = 2.524, ["ZIP"]  = 1.616, ["TXT"]  = 1.949, ["MD"]   = 1.595,
            ["HTML"] = 2.857, ["IMG"]  = 1.949, ["MP4"]  = 2.116, ["MP3"]  = 2.116,
            ["DRAW"] = 3.135, ["CAD"]  = 2.227, ["EPUB"] = 2.858, ["ICS"]  = 1.727,
            ["DB"]   = 1.484, ["CODE"] = 2.969, ["FILE"] = 2.247, ["DOCX"] = 2.969,
            ["XLSX"] = 2.692, ["PPTX"] = 2.692, ["CSV"]  = 2.116, ["TSV"]  = 2.005,
            ["JSON"] = 2.803, ["YAML"] = 2.821, ["YML"]  = 2.171, ["XML"]  = 2.171,
            ["RTF"]  = 2.004, ["ODT"]  = 2.171, ["ODS"]  = 2.227, ["ODP"]  = 2.227,
            ["LOG"]  = 2.227, ["INI"]  = 1.338, ["CFG"]  = 2.171, ["SQL"]  = 2.116,
            ["PNG"]  = 2.227, ["JPG"]  = 2.061, ["JPEG"] = 2.748, ["GIF"]  = 1.727,
            ["WEBP"] = 3.080, ["HEIC"] = 2.469, ["TIFF"] = 2.190, ["BMP"]  = 2.282,
            ["SVG"]  = 2.172, ["ICO"]  = 1.838, ["PSD"]  = 2.116, ["AI"]   = 1.040,
            ["WAV"]  = 2.264, ["FLAC"] = 2.746, ["AAC"]  = 2.227, ["OGG"]  = 2.393,
            ["AVI"]  = 1.653, ["MOV"]  = 2.338, ["MKV"]  = 2.282, ["WEBM"] = 3.246,
            ["M4A"]  = 2.171, ["RAR"]  = 2.227, ["GZ"]   = 1.429, ["TAR"]  = 2.041,
            ["7Z"]   = 1.207, ["ISO"]  = 1.783, ["EXE"]  = 2.061, ["DMG"]  = 2.393,
            ["APK"]  = 2.171, ["WASM"] = 3.191, ["EML"]  = 2.171, ["MSG"]  = 2.338,
            ["KEY"]  = 2.116, ["NUM"]  = 2.337, ["JSONL"] = 3.434,
        };

        private static readonly Dictionary<string, double> _measured = new Dictionary<string, double>();

        private static CanvasRenderingContext2D _measurer;

        /// <summary>
        /// Publishes onto the tile the scale the given text has to be drawn at to stay inside it, or takes
        /// the property away when the word fits as it is.
        /// </summary>
        internal static void Apply(HTMLElement tile, string text)
        {
            var scale = ScaleFor(text);

            if (scale >= 1)
            {
                tile.style.removeProperty("--tss-icontile-text-fit");
                return;
            }

            tile.style.setProperty("--tss-icontile-text-fit", scale.ToString("0.###"));
        }

        private static double ScaleFor(string text)
        {
            if (string.IsNullOrEmpty(text)) return 1;

            var widthPerPixel = WidthPerPixelOfFontSize(text);

            if (widthPerPixel <= 0) return 1;

            //What the word takes at the standard size against what the tile has for it, both in tile sides.
            return ROOM_FRACTION / (STANDARD_FRACTION * widthPerPixel);
        }

        //How wide the word is per pixel of font size, which is the one thing about it that doesn't depend on
        //the tile it lands on.
        private static double WidthPerPixelOfFontSize(string text)
        {
            //First, because creating it is what checks the shipped table against the font in use.
            var measurer = Measurer();

            if (_known.TryGetValue(text, out var known))       return known;
            if (_measured.TryGetValue(text, out var remembered)) return remembered;

            if (measurer is null) return 0;

            var width = Measure(measurer, text);

            if (_measured.Count >= MAX_REMEMBERED) _measured.Clear();

            _measured[text] = width;

            return width;
        }

        //The tile uppercases its text through the stylesheet, and letter-spaces it - a canvas measures the
        //string it is given, so it is given the drawn one and the tracking is added back.
        private static double Measure(CanvasRenderingContext2D measurer, string text)
        {
            var advance = measurer.measureText(text.ToUpper()).width + LETTER_SPACING_EM * MEASURED_AT * text.Length;

            return advance / MEASURED_AT;
        }

        private static CanvasRenderingContext2D Measurer()
        {
            if (_measurer is object) return _measurer;

            //Before the document has a body there is no font to resolve the stack against. The shipped table
            //still answers for the words it knows - unchecked, which is the better of the two wrongs - and
            //the next tile built tries again.
            if (document.body is null) return null;

            var context = Canvas(Att()).getContext("2d").As<CanvasRenderingContext2D>();

            if (context is null) return null;

            context.font = FONT_WEIGHT + " " + MEASURED_AT + "px " + window.getComputedStyle(document.body).fontFamily;

            _measurer = context;

            DropTheTableIfItIsAboutAnotherFont(context);

            return _measurer;
        }

        private static void DropTheTableIfItIsAboutAnotherFont(CanvasRenderingContext2D measurer)
        {
            if (!_known.TryGetValue(PROBE, out var claimed)) return;

            var actual = Measure(measurer, PROBE);

            if (Math.Abs(actual - claimed) / claimed > PROBE_TOLERANCE) _known.Clear();
        }
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
