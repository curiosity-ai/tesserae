using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Build.UIconsOpticalCentering
{
    /// <summary>
    /// Prints what the pass did and checks the properties that matter: that the reference box the icons
    /// were centred against is the one the browser actually uses, that most icons were left alone, and
    /// that icons meant to sit on top of each other still do.
    /// </summary>
    internal static class CoherenceReport
    {
        public static bool Print(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var ok = true;

            PrintFontMetrics(fonts);
            PrintDistribution(fonts, settings);
            ok &= PrintAlignmentGroups(fonts, settings);
            ok &= PrintFrameClusters(fonts, settings);
            PrintLargestAdjustments(fonts);

            return ok;
        }

        private static void PrintFontMetrics(List<FontAdjustments> fonts)
        {
            Console.WriteLine();
            Console.WriteLine("Font metrics (px at the raster size; dom values drive the reference box, canvas values cross-check them)");
            Console.WriteLine($"  {"font",-26} {"em",5} {"ascent",7} {"descent",8} {"canvas asc",11} {"canvas desc",12} {"box centre",11}");

            foreach (var font in fonts)
            {
                var m         = font.Measurement;
                var mismatch  = Math.Abs(m.Ascent - m.CanvasAscent) > 1 || Math.Abs(m.Descent - m.CanvasDescent) > 1;

                Console.WriteLine($"  {m.FontFamily,-26} {m.Em,5:0} {m.Ascent,7:0.00} {m.Descent,8:0.00} {m.CanvasAscent,11:0.00} " +
                                  $"{m.CanvasDescent,12:0.00} {m.BoxCenterY,11:0.00}{(mismatch ? "   <- dom and canvas disagree" : "")}");
            }
        }

        private static void PrintDistribution(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var all      = fonts.SelectMany(f => f.Glyphs).ToList();
            var usable   = all.Where(g => g.Measurement.IsUsable).ToList();
            var adjusted = usable.Where(g => g.IsAdjusted).ToList();

            Console.WriteLine();
            Console.WriteLine($"Adjustments: {adjusted.Count} of {usable.Count} glyphs ({100.0 * adjusted.Count / Math.Max(1, usable.Count):0.0}%), " +
                              $"{all.Count - usable.Count} unmeasurable");

            var buckets     = new[] { 0.010, 0.015, 0.020, 0.025, 0.030, settings.MaxAdjustment };
            double previous = 0;

            foreach (var bucket in buckets)
            {
                var count = adjusted.Count(g => Magnitude(g) > previous && Magnitude(g) <= bucket + 1e-9);
                Console.WriteLine($"  |offset| <= {bucket,6:0.000}em  {count,6}  {Bar(count, adjusted.Count)}");
                previous = bucket;
            }

            var horizontal = adjusted.Count(g => g.X != 0);
            var vertical   = adjusted.Count(g => g.Y != 0);
            Console.WriteLine($"  horizontal {horizontal}, vertical {vertical}, both {adjusted.Count(g => g.X != 0 && g.Y != 0)}");

            var rejected = usable.Where(g => g.IsRejected).ToList();
            Console.WriteLine();
            Console.WriteLine($"Left alone as drawn: {rejected.Count} glyphs are further than {settings.MaxAdjustment:0.000}em off centre, " +
                              "which reads as intentional asymmetry rather than a centering mistake");

            foreach (var glyph in rejected.Where(g => g.Glyph.CssClass.StartsWith("fi-rr-", StringComparison.Ordinal))
                                          .OrderByDescending(g => Math.Max(Math.Abs(g.TargetX), Math.Abs(g.TargetY)))
                                          .Take(12))
            {
                Console.WriteLine($"  {glyph.Glyph.CssClass,-40} would need {glyph.TargetX,7:0.000} / {glyph.TargetY,7:0.000}");
            }
        }

        private static double Magnitude(GlyphAdjustment g) => Math.Max(Math.Abs(g.X), Math.Abs(g.Y));

        private static string Bar(int count, int total)
        {
            var width = total == 0 ? 0 : (int)Math.Round(40.0 * count / total);
            return new string('#', width);
        }

        /// <summary>
        /// The check the whole design hinges on: after adjustment, do icons that are drawn on top of each
        /// other still line up? Measured on the raw ink boxes, so it is the actual pixels being compared.
        /// </summary>
        private static bool PrintAlignmentGroups(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            Console.WriteLine();
            Console.WriteLine("Icons that must keep overlapping. 'before' and 'after' are the worst mismatch between any two");
            Console.WriteLine("members on any ink box edge, across all fonts, in em. A group whose members are not the same");
            Console.WriteLine("shape starts out non-zero; what matters is that the adjustment does not make it worse.");
            Console.WriteLine($"  {"group",-28} {"before",9} {"after",9}  offsets (regular rounded)");

            var ok = true;

            foreach (var group in AlignmentGroups.All)
            {
                double worstBefore = 0, worstAfter = 0;
                var    offsets     = new List<string>();

                foreach (var font in fonts)
                {
                    var members = group.Icons
                       .Select(name => font.Glyphs.FirstOrDefault(g => g.Glyph.IconName == name && g.Measurement.IsUsable))
                       .Where(g => g != null)
                       .ToList();

                    if (members.Count < 2) continue;

                    worstBefore = Math.Max(worstBefore, WorstEdgeMismatch(font, group, members, adjusted: false));
                    worstAfter  = Math.Max(worstAfter, WorstEdgeMismatch(font, group, members, adjusted: true));

                    if (font.Font.ClassPrefix == "fi-rr-")
                    {
                        offsets.AddRange(members.Select(m => $"{m.Glyph.IconName} {OpticalCentering.Format(m.X)}/{OpticalCentering.Format(m.Y)}"));
                    }
                }

                // A mirrored pair is only expected to agree on the axis it shares, so its mismatch is
                // reported but not held against the run.
                var enforced = group.Kind == AlignmentKind.Aligned || group.Kind == AlignmentKind.AnchorFirst;
                var failed   = enforced && worstAfter > worstBefore + settings.Step + 1e-9;
                ok          &= !failed;

                Console.WriteLine($"  {group.Name,-28} {worstBefore,9:0.0000} {worstAfter,9:0.0000}  {string.Join(", ", offsets)}" +
                                  (failed ? "   <- FAILED: drifted apart" : ""));
            }

            return ok;
        }

        /// <summary>Largest disagreement between any two members on any ink box edge, in em.</summary>
        private static double WorstEdgeMismatch(FontAdjustments font, AlignmentGroup group, List<GlyphAdjustment> members, bool adjusted)
        {
            var em    = font.Measurement.Em;
            var worst = 0.0;

            // Mirrored pairs are compared on the shared axis only.
            var compareX = group.Kind != AlignmentKind.SharedVertical;
            var compareY = group.Kind != AlignmentKind.SharedHorizontal;

            foreach (var a in members)
            {
                foreach (var b in members)
                {
                    if (ReferenceEquals(a, b)) continue;

                    var dx = adjusted ? a.X - b.X : 0;
                    var dy = adjusted ? a.Y - b.Y : 0;

                    if (compareX)
                    {
                        worst = Math.Max(worst, Math.Abs((a.Measurement.InkLeft - b.Measurement.InkLeft) / em + dx));
                        worst = Math.Max(worst, Math.Abs((a.Measurement.InkRight - b.Measurement.InkRight) / em + dx));
                    }

                    if (compareY)
                    {
                        worst = Math.Max(worst, Math.Abs((a.Measurement.InkTop - b.Measurement.InkTop) / em + dy));
                        worst = Math.Max(worst, Math.Abs((a.Measurement.InkBottom - b.Measurement.InkBottom) / em + dy));
                    }
                }
            }

            return worst;
        }

        /// <summary>
        /// Every glyph in a frame cluster is given the same offset before rounding, so after rounding they
        /// must still agree. The only glyphs allowed to differ are the ones a curated alignment group
        /// deliberately pulled out of their cluster.
        /// </summary>
        private static bool PrintFrameClusters(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var pinned = new HashSet<string>(AlignmentGroups.All.SelectMany(g => g.Icons), StringComparer.Ordinal);
            var ok     = true;
            var worst  = 0.0;
            string worstCluster = null;

            foreach (var font in fonts)
            {
                foreach (var cluster in font.Glyphs
                   .Where(g => g.Measurement.IsUsable && g.SharesFrame && !pinned.Contains(g.Glyph.IconName))
                   .GroupBy(g => g.FrameCluster))
                {
                    var spread = Math.Max(
                        cluster.Max(g => g.X) - cluster.Min(g => g.X),
                        cluster.Max(g => g.Y) - cluster.Min(g => g.Y));

                    if (spread > worst)
                    {
                        worst        = spread;
                        worstCluster = $"{font.Font.FontFamily} cluster {cluster.Key} ({cluster.Count()} icons: " +
                                       $"{string.Join(", ", cluster.Take(4).Select(g => g.Glyph.IconName))})";
                    }
                }
            }

            var glyphs    = fonts.SelectMany(f => f.Glyphs).ToList();
            var shared    = glyphs.Count(g => g.SharesFrame);
            var oversized = glyphs.Count(g => g.FrameClusterSize > settings.MaxSharedFrameGroup);

            Console.WriteLine();
            Console.WriteLine($"Icons drawn on the same frame: {shared} glyphs are pinned to the other icons sharing their ink box");
            Console.WriteLine($"  {oversized} glyphs are in groups bigger than {settings.MaxSharedFrameGroup} and stay un-pinned " +
                              "(an ink box shared by hundreds of icons says nothing about shape)");
            Console.WriteLine($"  largest offset spread inside a pinned group {worst.ToString("0.0000", CultureInfo.InvariantCulture)}em " +
                              "(must be zero: same frame means same offset)");

            if (worst > 1e-9)
            {
                Console.WriteLine($"  FAILED: {worstCluster} does not share one offset");
                ok = false;
            }

            return ok;
        }

        private static void PrintLargestAdjustments(List<FontAdjustments> fonts)
        {
            var font = fonts.FirstOrDefault(f => f.Font.ClassPrefix == "fi-rr-") ?? fonts.First();

            Console.WriteLine();
            Console.WriteLine($"Largest adjustments in {font.Font.FontFamily}");

            foreach (var glyph in font.Glyphs.Where(g => g.IsAdjusted).OrderByDescending(Magnitude).Take(15))
            {
                Console.WriteLine($"  {glyph.Glyph.CssClass,-40} left {OpticalCentering.Format(glyph.X),9} top {OpticalCentering.Format(glyph.Y),9}" +
                                  $"   frame {glyph.FrameX,7:0.000}/{glyph.FrameY,7:0.000}  optical {glyph.OpticalX,7:0.000}/{glyph.OpticalY,7:0.000}");
            }
        }
    }
}
