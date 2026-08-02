using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Build.UIconsOpticalCentering
{
    /// <summary>One emitted rule: an offset and every icon class that needs it.</summary>
    internal sealed class AdjustmentRule
    {
        public double       X        { get; set; }
        public double       Y        { get; set; }
        public List<string> Selectors { get; } = new List<string>();
    }

    /// <summary>
    /// Writes <c>tss.uicons.adjustments.css</c>: the icons that need a nudge, grouped by the nudge.
    /// <para>
    /// The offsets are applied with <c>position: relative</c> on the icon's <c>::before</c>, in em, so
    /// they scale with the icon and take no part in layout. A transform would need the pseudo-element to
    /// stop being inline, and <c>margin-top</c> does nothing on an inline box.
    /// </para>
    /// </summary>
    internal static class AdjustmentCss
    {
        private const int MaxLineLength = 110;

        public static List<AdjustmentRule> BuildRules(IEnumerable<FontAdjustments> fonts, CenteringSettings settings)
        {
            var rules = new Dictionary<(long, long), AdjustmentRule>();

            foreach (var glyph in fonts.SelectMany(f => f.Glyphs).Where(g => g.IsAdjusted))
            {
                var key = (Steps(glyph.X, settings), Steps(glyph.Y, settings));

                if (!rules.TryGetValue(key, out var rule))
                {
                    rule       = new AdjustmentRule { X = glyph.X, Y = glyph.Y };
                    rules[key] = rule;
                }

                rule.Selectors.Add(glyph.Glyph.CssClass);
            }

            foreach (var rule in rules.Values)
            {
                rule.Selectors.Sort(StringComparer.Ordinal);
            }

            return rules.Values.OrderBy(r => r.Y).ThenBy(r => r.X).ToList();
        }

        private static long Steps(double value, CenteringSettings settings) => (long)Math.Round(value / settings.Step);

        public static string Render(List<AdjustmentRule> rules, List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var totalGlyphs   = fonts.Sum(f => f.Glyphs.Count);
            var adjusted      = rules.Sum(r => r.Selectors.Count);
            var sb            = new StringBuilder();

            sb.AppendLine("/*!");
            sb.AppendLine(" *  tss.uicons.adjustments.css");
            sb.AppendLine(" *");
            sb.AppendLine(" *  GENERATED FILE - do not edit by hand.");
            sb.AppendLine(" *  Regenerate with: dotnet run --project Build.UIconsOpticalCentering");
            sb.AppendLine(" *");
            sb.AppendLine(" *  Optical centering for the bundled UIcons fonts. Every glyph is rendered on its own and");
            sb.AppendLine(" *  measured against the box the browser lays it out in (its advance width, and the font's");
            sb.AppendLine(" *  ascent/descent around the baseline). Icons whose visual centre does not land on the centre");
            sb.AppendLine(" *  of that box get a nudge here, expressed in em so it scales with the icon.");
            sb.AppendLine(" *");
            sb.AppendLine(" *  \"Visual centre\" is the centre of the trimmed ink extent - the icon's frame, with hairlines");
            sb.AppendLine(" *  and antialiasing discounted - pulled towards the centre of ink mass, so an icon that carries");
            sb.AppendLine(" *  its weight to one side is balanced rather than merely boxed. The pull is capped.");
            sb.AppendLine(" *");
            sb.AppendLine(" *  An offset applied at paint time cannot be a fraction of a pixel: the browser rounds it to a");
            sb.AppendLine(" *  whole one. So each offset is also declared through round(), which fixes the value from the font");
            sb.AppendLine(" *  size instead of leaving it to paint-time snapping - otherwise the same icon shifts by a pixel");
            sb.AppendLine(" *  in one container and not at all in another, depending on where it lands on the pixel grid.");
            sb.AppendLine(" *  The consequence is that an offset only takes effect once it reaches half a pixel, so these");
            sb.AppendLine(" *  corrections act on larger icons and correctly do nothing on the smallest ones.");
            sb.AppendLine(" *");
            sb.AppendLine(" *  Icons that have to stay on top of each other keep one shared offset. Exactly, for the icons");
            sb.AppendLine(" *  the toolkit swaps in place (square and checkbox, the -slash variants on their base icon) and");
            sb.AppendLine(" *  for any set of icons drawn on the same frame that agrees on where its centre is. Elsewhere it");
            sb.AppendLine(" *  is a tendency, not a promise: rounding to a grid can always separate two values a hair apart.");
            sb.AppendLine(" *  What is guaranteed is the other direction - no icon is moved further off centre than rounding");
            sb.AppendLine(" *  explains, the exception being the ones deliberately pinned to an icon they get swapped with.");
            sb.AppendLine(" *");
            sb.AppendLine($" *  {adjusted} of {totalGlyphs} glyphs need an adjustment, in {rules.Count} groups.");
            sb.AppendLine(" *");
            sb.AppendLine($" *  mass weight {settings.MassWeight.ToString(CultureInfo.InvariantCulture)}," +
                          $" optical pull cap {Em(settings.MaxOpticalPull)}," +
                          $" step {Em(settings.Step)}," +
                          $" dead zone {Em(settings.DeadZone)}," +
                          $" cap {Em(settings.MaxAdjustment)},");
            sb.AppendLine($" *  frame tolerance {Em(settings.FrameTolerance)}," +
                          $" shared frame spread {Em(settings.MaxSharedFrameSpread)}," +
                          $" trim {settings.Trim.ToString(CultureInfo.InvariantCulture)}," +
                          $" rasterized at {settings.RasterEm}px.");
            sb.AppendLine(" *");
            sb.AppendLine(" *  Per font, the median offset is treated as that font's norm and subtracted out, so this file");
            sb.AppendLine(" *  only corrects outliers instead of shifting every icon in every existing layout:");

            foreach (var font in fonts.OrderBy(f => f.Font.FontFamily, StringComparer.Ordinal))
            {
                sb.AppendLine($" *    {font.Font.FontFamily,-26} baseline {Em(font.BaselineX),8} / {Em(font.BaselineY),8}" +
                              $"  {font.Glyphs.Count(g => g.IsAdjusted),5} of {font.Glyphs.Count,5} adjusted");
            }

            sb.AppendLine(" *");
            sb.AppendLine(" *  Note: components that draw an icon through content: var(--uicon-var-*) instead of an");
            sb.AppendLine(" *  fi-* class are not covered by these selectors. Icons swapped that way are pinned to each");
            sb.AppendLine(" *  other during generation, so they move together or not at all.");
            sb.AppendLine(" */");
            sb.AppendLine();

            foreach (var rule in rules)
            {
                sb.AppendLine($"/* left {Signed(rule.X)}, top {Signed(rule.Y)} - " +
                              $"{rule.Selectors.Count} {(rule.Selectors.Count == 1 ? "icon" : "icons")} */");
                AppendSelectors(sb, rule.Selectors);
                sb.Append(" { position: relative;");

                // Each offset is declared twice: the em value, then the same value rounded to a whole
                // pixel. A browser without round() keeps the em and lets paint-time snapping deal with it;
                // one with round() takes the second declaration and gets the same pixel every time.
                if (rule.X != 0) sb.Append($" left: {Em(rule.X)}; left: round({Em(rule.X)}, 1px);");
                if (rule.Y != 0) sb.Append($" top: {Em(rule.Y)}; top: round({Em(rule.Y)}, 1px);");
                sb.AppendLine(" }");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static void AppendSelectors(StringBuilder sb, List<string> selectors)
        {
            var line = new StringBuilder();

            for (int i = 0; i < selectors.Count; i++)
            {
                var selector = $".{selectors[i]}::before";
                var suffix   = i < selectors.Count - 1 ? "," : "";

                if (line.Length > 0 && line.Length + selector.Length + suffix.Length > MaxLineLength)
                {
                    sb.AppendLine(line.ToString());
                    line.Clear();
                }

                line.Append(selector).Append(suffix);
            }

            sb.Append(line);
        }

        private static string Em(double value) => value == 0 ? "0" : value.ToString("0.####", CultureInfo.InvariantCulture) + "em";

        private static string Signed(double value) => value == 0 ? "0" : (value > 0 ? "+" : "") + value.ToString("0.###", CultureInfo.InvariantCulture) + "em";
    }
}
