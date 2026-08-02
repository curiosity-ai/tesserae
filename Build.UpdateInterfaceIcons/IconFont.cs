using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>A single glyph of an icon font: the CSS class that selects it and the codepoint it maps to.</summary>
    internal sealed class IconGlyph
    {
        public IconGlyph(string cssClass, string iconName, int codePoint)
        {
            CssClass  = cssClass;
            IconName  = iconName;
            CodePoint = codePoint;
        }

        /// <summary>The full class name, e.g. <c>fi-rr-square</c>.</summary>
        public string CssClass { get; }

        /// <summary>The class name without the weight prefix, e.g. <c>square</c>.</summary>
        public string IconName { get; }

        /// <summary>The codepoint the <c>content</c> declaration points at.</summary>
        public int CodePoint { get; }

        public override string ToString() => $"{CssClass} (U+{CodePoint:X4})";
    }

    /// <summary>One of the bundled uicons webfonts, with every glyph its stylesheet declares.</summary>
    internal sealed class IconFont
    {
        public IconFont(string fontFamily, string classPrefix, IReadOnlyList<IconGlyph> glyphs)
        {
            FontFamily  = fontFamily;
            ClassPrefix = classPrefix;
            Glyphs      = glyphs;
        }

        /// <summary>The <c>font-family</c> name, which is also the css/woff2 file name, e.g. <c>uicons-regular-rounded</c>.</summary>
        public string FontFamily { get; }

        /// <summary>The shared class prefix of every glyph in this font, e.g. <c>fi-rr-</c>.</summary>
        public string ClassPrefix { get; }

        public IReadOnlyList<IconGlyph> Glyphs { get; }

        public override string ToString() => $"{FontFamily} ({Glyphs.Count} glyphs)";
    }

    /// <summary>
    /// Reads the bundled <c>uicons-*.css</c> stylesheets, which are the source of truth for which
    /// glyph each icon class renders (the codepoints change with every UIcons release).
    /// </summary>
    internal static class IconFontReader
    {
        /// <summary>The weight prefixes used by UIcons, matching <c>UIconsWeight</c> plus the brands font.</summary>
        private static readonly string[] Prefixes = { "rr", "rs", "br", "bs", "sr", "ss", "tr", "ts", "brands" };

        public static List<IconFont> ReadAll(string cssDirectory)
        {
            var fonts = new List<IconFont>();

            foreach (var file in Directory.GetFiles(cssDirectory, "uicons-*.css").OrderBy(f => f, StringComparer.Ordinal))
            {
                var font = Read(file);

                if (font.Glyphs.Count == 0)
                {
                    Console.WriteLine($"  warning: no glyphs found in {Path.GetFileName(file)}, skipping");
                    continue;
                }

                fonts.Add(font);
            }

            return fonts;
        }

        private static IconFont Read(string cssFile)
        {
            var fontFamily = Path.GetFileNameWithoutExtension(cssFile);
            var lines      = File.ReadAllLines(cssFile);
            var glyphs     = new List<IconGlyph>();
            var seen       = new HashSet<string>(StringComparer.Ordinal);
            string prefix  = null;

            for (int i = 0; i < lines.Length - 1; i++)
            {
                var selectorLine = lines[i].Trim();

                if (!selectorLine.StartsWith(".fi-", StringComparison.Ordinal) || !selectorLine.EndsWith(":before {", StringComparison.Ordinal)) continue;

                var codePoint = TryReadContentCodePoint(lines, i + 1);
                if (codePoint <= 0) continue;

                foreach (var selector in selectorLine.Substring(0, selectorLine.Length - " {".Length).Split(','))
                {
                    var cssClass = selector.Trim();

                    if (!cssClass.StartsWith(".fi-", StringComparison.Ordinal) || !cssClass.EndsWith(":before", StringComparison.Ordinal)) continue;

                    cssClass = cssClass.Substring(1, cssClass.Length - 1 - ":before".Length);

                    var classPrefix = Prefixes
                       .Select(p => $"fi-{p}-")
                       .FirstOrDefault(p => cssClass.StartsWith(p, StringComparison.Ordinal));

                    if (classPrefix is null) continue;

                    prefix = prefix ?? classPrefix;

                    // The stylesheets contain a handful of duplicated selectors; the first one wins, as it does in the browser.
                    if (!seen.Add(cssClass)) continue;

                    glyphs.Add(new IconGlyph(cssClass, cssClass.Substring(classPrefix.Length), codePoint));
                }
            }

            return new IconFont(fontFamily, prefix ?? "", glyphs);
        }

        /// <summary>Reads the <c>content: "\eXXX";</c> declaration that follows an icon selector.</summary>
        private static int TryReadContentCodePoint(string[] lines, int from)
        {
            for (int i = from; i < Math.Min(from + 3, lines.Length); i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("}", StringComparison.Ordinal)) return 0;
                if (!line.StartsWith("content:", StringComparison.Ordinal)) continue;

                var quoted = line.Split('"');
                if (quoted.Length < 2) return 0;

                var escaped = quoted[1].TrimStart('\\');

                return int.TryParse(escaped, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codePoint) ? codePoint : 0;
            }

            return 0;
        }
    }
}
