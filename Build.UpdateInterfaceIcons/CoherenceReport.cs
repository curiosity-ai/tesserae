using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>
    /// Prints what the pass did and holds it to the properties that matter. Three of them fail the run:
    /// no icon may end up further off centre than rounding explains, icons meant to sit on top of each
    /// other may not drift apart, and a set of icons pinned to one offset must actually all have it.
    /// The rest is reported for a human to judge - the font metrics that define the reference box, how
    /// many icons were left alone, and how far the lookalike guarantee reaches.
    /// </summary>
    internal static class CoherenceReport
    {
        public static bool Print(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var ok = true;

            PrintFontMetrics(fonts);
            PrintDistribution(fonts, settings);
            ok &= PrintResiduals(fonts, settings);
            ok &= PrintAlignmentGroups(fonts, settings);
            ok &= PrintPinnedGroups(fonts, settings);
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

        /// <summary>How far the icon's optical centre still sits from the centre of its box, in em.</summary>
        private static double OffCentreAfter(GlyphAdjustment g) => Math.Max(Math.Abs(g.OpticalX - g.X), Math.Abs(g.OpticalY - g.Y));

        private static double OffCentreBefore(GlyphAdjustment g) => Math.Max(Math.Abs(g.OpticalX), Math.Abs(g.OpticalY));

        /// <summary>
        /// Does this actually centre the icons? Reports how far off centre they sit before and after, and
        /// holds the run to the rule that no icon may end up further off centre than it started. Rounding
        /// can cost half a step; anything beyond that means a shared offset dragged an icon off centre,
        /// which is what the pinning rules exist to avoid. Exempt are the icons that deliberately give up
        /// their own centering to stay registered with another one - a state variant taking its base icon's
        /// offset, a frame family left as drawn - where overlapping correctly outranks being centred.
        /// </summary>
        private static bool PrintResiduals(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var usable = fonts.SelectMany(f => f.Glyphs).Where(g => g.Measurement.IsUsable).ToList();

            Console.WriteLine();
            Console.WriteLine("How far the icons sit from the centre of their box, before and after (em)");
            Console.WriteLine($"  {"set",-24} {"n",6} {"mean",16} {"worst",16}");

            void Row(string name, List<GlyphAdjustment> glyphs)
            {
                if (glyphs.Count == 0) return;

                Console.WriteLine($"  {name,-24} {glyphs.Count,6} " +
                                  $"{glyphs.Average(OffCentreBefore),7:0.0000} ->{glyphs.Average(OffCentreAfter),7:0.0000} " +
                                  $"{glyphs.Max(OffCentreBefore),7:0.0000} ->{glyphs.Max(OffCentreAfter),7:0.0000}");
            }

            Row("all glyphs", usable);
            Row("given a rule", usable.Where(g => g.IsAdjusted).ToList());
            Row("left alone as drawn", usable.Where(g => g.IsRejected).ToList());

            var tolerance = settings.Step / 2 + 1e-9;
            var worsened  = usable
               .Where(g => !g.PinnedToPartner && !g.LeftAloneForItsFamily)
               .Where(g => OffCentreAfter(g) > OffCentreBefore(g) + tolerance)
               .OrderByDescending(g => OffCentreAfter(g) - OffCentreBefore(g))
               .ToList();

            var exempt = usable.Count(g => (g.PinnedToPartner || g.LeftAloneForItsFamily)
                                        && OffCentreAfter(g) > OffCentreBefore(g) + tolerance);

            Console.WriteLine($"  {worsened.Count} glyphs ended up further off centre than rounding explains" +
                              $" (plus {exempt} that give up their own centering to stay registered with another icon)");

            foreach (var glyph in worsened.Take(10))
            {
                Console.WriteLine($"  FAILED: {glyph.Glyph.CssClass,-40} {OffCentreBefore(glyph):0.0000} -> {OffCentreAfter(glyph):0.0000}" +
                                  $"   pinned to {glyph.PinnedGroupSize - 1} other icons");
            }

            return worsened.Count == 0;
        }

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
                if (group.Kind == AlignmentKind.Fixed)
                {
                    Console.WriteLine($"  {group.Name,-28} {"-",9} {"-",9}  never adjusted, so nothing can drift");
                    continue;
                }

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
        private static bool PrintPinnedGroups(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var ok     = true;
            var worst  = 0.0;
            string worstCluster = null;

            foreach (var font in fonts)
            {
                // A glyph whose offset a later rule replaced - a state variant taking its base icon's, a
                // frame family left as drawn - is no longer bound by the group it was pinned into, and
                // should not be: following the icon you are a state of outranks matching a lookalike.
                foreach (var cluster in font.Glyphs
                   .Where(g => g.Measurement.IsUsable && g.IsPinned && !g.PinnedToPartner && !g.LeftAloneForItsFamily)
                   .GroupBy(g => g.PinnedGroup))
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

            var glyphs     = fonts.SelectMany(f => f.Glyphs).ToList();
            var shared     = glyphs.Count(g => g.IsPinned);
            var disagreed  = glyphs.Count(g => g.FrameGroupSize > 1 && !g.IsPinned);

            Console.WriteLine();
            Console.WriteLine($"Icons drawn on the same frame: {shared} glyphs are pinned to the other icons sharing their ink box");
            Console.WriteLine($"  {disagreed} glyphs share an ink box with something but disagree by more than " +
                              $"{settings.MaxSharedFrameSpread:0.0000}em on where their centre is, so they stay un-pinned " +
                              "(a shared ink box alone does not make two icons the same drawing)");
            Console.WriteLine($"  largest offset spread inside a pinned group {worst.ToString("0.0000", CultureInfo.InvariantCulture)}em " +
                              "(must be zero: a pinned group is one offset by construction)");

            if (worst > 1e-9)
            {
                Console.WriteLine($"  FAILED: {worstCluster} does not share one offset");
                ok = false;
            }

            PrintLookalikeDivergence(fonts, settings);
            PrintCompositionRules(fonts);
            return ok;
        }

        /// <summary>
        /// The icons that gave up their own centering to stay registered with another icon: state variants
        /// taking their base icon's offset, and frame families where the members could not agree.
        /// </summary>
        private static void PrintCompositionRules(List<FontAdjustments> fonts)
        {
            var glyphs = fonts.SelectMany(f => f.Glyphs).ToList();

            Console.WriteLine();
            Console.WriteLine("Icons that compose with another icon");
            Console.WriteLine($"  {fonts.Sum(f => f.StateVariantsAnchored)} state variants (-{string.Join(", -", StateVariants.Suffixes)})" +
                              " took the offset of the icon they are a state of");
            Console.WriteLine($"  {fonts.Sum(f => f.FrameFamiliesSuppressed)} offsets dropped because a frame family " +
                              $"({string.Join(", ", FrameShapes.Words)}) could not agree on one");

            var byFrame = glyphs.Where(g => g.LeftAloneForItsFamily).ToList();

            foreach (var example in byFrame.Where(g => g.Glyph.CssClass.StartsWith("fi-rr-", StringComparison.Ordinal))
                                           .OrderBy(g => g.Glyph.IconName, StringComparer.Ordinal).Take(8))
            {
                Console.WriteLine($"    {example.Glyph.CssClass,-40} wanted {example.TargetX,7:0.000} / {example.TargetY,7:0.000}, left as drawn");
            }
        }

        /// <summary>
        /// Measures, rather than assumes, how well icons that look alike end up with the same offset.
        /// <para>
        /// Two things stop this being a guarantee. Pinning groups icons around a leader, so two icons that
        /// agree with each other can still land in different groups — being within a tolerance is not
        /// transitive, and no rounding grid escapes that. And the dead zone and the cap are cliffs: two
        /// values a hair apart either side of one get told to do different things.
        /// </para>
        /// <para>
        /// So this compares every pair of icons that shares an ink box and agrees on its centre to within
        /// half a step. That is a loose proxy for "the same drawing" — it also catches icons that merely
        /// fill their box and happen to want the same nudge, which is why the pairs are printed rather than
        /// counted alone. The guarantees live elsewhere: curated groups and pinned groups are exact.
        /// </para>
        /// </summary>
        private static void PrintLookalikeDivergence(List<FontAdjustments> fonts, CenteringSettings settings)
        {
            var tolerance = settings.Step / 2 + 1e-9;
            var pairs     = 0;
            var diverged  = new List<(double Divergence, string Pair)>();

            foreach (var font in fonts)
            {
                foreach (var frame in font.Glyphs
                   .Where(g => g.Measurement.IsUsable && g.FrameGroupSize > 1)
                   .GroupBy(g => g.FrameGroup))
                {
                    var members = frame.ToList();

                    for (int i = 0; i < members.Count; i++)
                    {
                        for (int j = i + 1; j < members.Count; j++)
                        {
                            var a = members[i];
                            var b = members[j];

                            if (Math.Abs(a.OpticalX - b.OpticalX) > tolerance || Math.Abs(a.OpticalY - b.OpticalY) > tolerance) continue;

                            pairs++;
                            var divergence = Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

                            if (divergence > settings.Step + 1e-9) diverged.Add((divergence, $"{a.Glyph.CssClass} vs {b.Glyph.IconName}"));
                        }
                    }
                }
            }

            Console.WriteLine($"  {pairs} pairs of icons share an ink box and agree on their centre to within half a step;" +
                              $" {diverged.Count} of them were given offsets more than one rounding step apart," +
                              " because the dead zone and the cap are cliffs and two near-identical values can fall either side");

            foreach (var (divergence, pair) in diverged.OrderByDescending(d => d.Divergence).Take(8))
            {
                Console.WriteLine($"    {divergence.ToString("0.0000", CultureInfo.InvariantCulture)}em  {pair}");
            }
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
