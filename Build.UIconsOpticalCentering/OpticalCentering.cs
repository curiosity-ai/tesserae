using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Build.UIconsOpticalCentering
{
    /// <summary>Tuning for the centering pass. Every distance is a fraction of the font size (i.e. em).</summary>
    internal sealed class CenteringSettings
    {
        /// <summary>Font size the glyphs are rasterized at. Bigger is more precise and slower.</summary>
        public int RasterEm { get; set; } = 80;

        /// <summary>Largest raster canvas edge, in pixels. Controls how many glyphs share a readback.</summary>
        public int MaxCanvas { get; set; } = 2048;

        /// <summary>Alpha at which a pixel counts as ink for the raw bounding box.</summary>
        public int InkThreshold { get; set; } = 24;

        /// <summary>Fraction of the ink mass trimmed off each side when looking for the visual frame.</summary>
        public double Trim { get; set; } = 0.02;

        /// <summary>How much the centre of ink mass is allowed to weigh in against the visual frame.</summary>
        public double MassWeight { get; set; } = 0.25;

        /// <summary>Cap on how far optical weight may pull an icon away from its frame centre.</summary>
        public double MaxOpticalPull { get; set; } = 0.010;

        /// <summary>
        /// Largest offset that is still treated as a centering mistake. Anything beyond this is left
        /// alone: an icon that is a long way off centre is usually meant to be (a half circle, an empty
        /// crate drawn at the bottom of its box), and half-correcting it is worse than not touching it.
        /// </summary>
        public double MaxAdjustment { get; set; } = 0.040;

        /// <summary>Offsets are rounded to a multiple of this, which is what makes the icons group up.</summary>
        public double Step { get; set; } = 0.005;

        /// <summary>Offsets smaller than this are dropped: invisible in practice, and pure noise in the output.</summary>
        public double DeadZone { get; set; } = 0.020;

        /// <summary>
        /// How closely two ink boxes must agree for the icons to count as the same shape and share one
        /// offset. Deliberately tight: the point is to catch icons drawn on the same outline, like a
        /// square and a checkbox, not merely icons that happen to fill a similar area.
        /// </summary>
        public double FrameTolerance { get; set; } = 0.004;

        /// <summary>
        /// How closely two same-frame icons must already agree on where their optical centre is before they
        /// are pinned to one shared offset. Sharing an ink box does not by itself make two icons the same
        /// drawing - thousands are simply drawn edge to edge - so agreement is what identifies a lookalike.
        /// Icons that agree lose nothing by being pinned and gain immunity to landing either side of a
        /// rounding step; icons that disagree are left to their own offsets rather than dragged off centre.
        /// Half a rounding step, so pinning can never cost more than the rounding already does.
        /// </summary>
        public double MaxSharedFrameSpread { get; set; } = 0.0025;
    }

    /// <summary>The offset computed for one glyph, in em, along with the numbers it came from.</summary>
    internal sealed class GlyphAdjustment
    {
        public IconGlyph        Glyph       { get; set; }
        public GlyphMeasurement Measurement { get; set; }

        /// <summary>Offset that would centre the icon's visual frame in its layout box.</summary>
        public double FrameX { get; set; }

        public double FrameY { get; set; }

        /// <summary>Offset after optical weight is taken into account, before grouping and rounding.</summary>
        public double OpticalX { get; set; }

        public double OpticalY { get; set; }

        /// <summary>Offset after frame clusters and alignment groups have had their say, before rounding.</summary>
        public double TargetX { get; set; }

        public double TargetY { get; set; }

        /// <summary>Set when the offset was too large to be a centering mistake, so the icon is left as drawn.</summary>
        public bool RejectedX { get; set; }

        public bool RejectedY { get; set; }

        /// <summary>The offset that ends up in the stylesheet.</summary>
        public double X { get; set; }

        public double Y { get; set; }

        /// <summary>Index of the set of icons with the same ink box as this glyph.</summary>
        public int FrameGroup { get; set; } = -1;

        /// <summary>How many icons have the same ink box as this glyph, itself included.</summary>
        public int FrameGroupSize { get; set; }

        /// <summary>Index of the set of icons this glyph shares its offset with, or -1 when it is on its own.</summary>
        public int PinnedGroup { get; set; } = -1;

        /// <summary>How many icons share this glyph's offset, itself included.</summary>
        public int PinnedGroupSize { get; set; }

        /// <summary>Set when this glyph is pinned to other icons that are the same drawing.</summary>
        public bool IsPinned { get; set; }

        public bool IsAdjusted => X != 0 || Y != 0;

        public bool IsRejected => RejectedX || RejectedY;
    }

    /// <summary>The adjustments computed for one font.</summary>
    internal sealed class FontAdjustments
    {
        public IconFont             Font        { get; set; }
        public FontMeasurement      Measurement { get; set; }
        public List<GlyphAdjustment> Glyphs     { get; } = new List<GlyphAdjustment>();

        /// <summary>The font's own typical offset, which is subtracted out instead of shifting every icon.</summary>
        public double BaselineX { get; set; }

        public double BaselineY { get; set; }

        /// <summary>How many distinct ink boxes the font's glyphs fall into.</summary>
        public int FrameGroups { get; set; }

        /// <summary>How many sets of icons ended up sharing one offset.</summary>
        public int PinnedGroups { get; set; }
    }

    /// <summary>
    /// Turns raw glyph rasterizations into the per-icon nudges that centre them in their layout box.
    /// <para>
    /// The reference the icons are centred against is the box the browser lays each glyph out in:
    /// <c>[0, advance]</c> horizontally and <c>[-ascent, +descent]</c> around the baseline vertically.
    /// Its centre sits at <c>(advance / 2, (descent - ascent) / 2)</c> regardless of line-height, because
    /// extra leading is split evenly above and below the glyph.
    /// </para>
    /// <para>
    /// Where the icon <em>looks</em> centred is not the middle of its bounding box. Two things are
    /// combined: the trimmed ink extent, which is the icon's visual frame with hairlines and antialiasing
    /// discounted, and the centre of ink mass, which is where the icon's weight actually sits. The frame
    /// dominates and the mass term is capped, because letting weight take over would slide a checkbox off
    /// the square it is drawn inside.
    /// </para>
    /// </summary>
    internal static class OpticalCentering
    {
        public static FontAdjustments Compute(IconFont font, FontMeasurement measurement, CenteringSettings settings, List<string> warnings)
        {
            var byName = measurement.Glyphs.ToDictionary(g => g.IconName, StringComparer.Ordinal);
            var result = new FontAdjustments { Font = font, Measurement = measurement };
            var em     = measurement.Em;

            foreach (var glyph in font.Glyphs)
            {
                if (!byName.TryGetValue(glyph.IconName, out var m))
                {
                    warnings.Add($"{font.FontFamily}: {glyph.CssClass} was never measured");
                    continue;
                }

                var adjustment = new GlyphAdjustment { Glyph = glyph, Measurement = m };

                if (m.IsUsable)
                {
                    var boxCenterX = m.Advance / 2;
                    var boxCenterY = measurement.BoxCenterY;

                    var frameCenterX = (m.TrimLeft + m.TrimRight) / 2;
                    var frameCenterY = (m.TrimTop + m.TrimBottom) / 2;

                    adjustment.FrameX = (boxCenterX - frameCenterX) / em;
                    adjustment.FrameY = (boxCenterY - frameCenterY) / em;

                    // The optical term is the pull from the frame centre towards the centre of mass, capped.
                    var pullX = Clamp(settings.MassWeight * (frameCenterX - m.CentroidX) / em, settings.MaxOpticalPull);
                    var pullY = Clamp(settings.MassWeight * (frameCenterY - m.CentroidY) / em, settings.MaxOpticalPull);

                    adjustment.OpticalX = adjustment.FrameX + pullX;
                    adjustment.OpticalY = adjustment.FrameY + pullY;
                }

                result.Glyphs.Add(adjustment);
            }

            var usable = result.Glyphs.Where(g => g.Measurement.IsUsable).ToList();

            if (usable.Count == 0)
            {
                warnings.Add($"{font.FontFamily}: no usable glyph measurements");
                return result;
            }

            // A whole font sitting slightly off centre is a property of the font, not of individual icons.
            // Shifting all of them would move every icon in every existing layout, so the font's own median
            // is treated as the norm and only the outliers are corrected.
            result.BaselineX = Median(usable.Select(g => g.OpticalX));
            result.BaselineY = Median(usable.Select(g => g.OpticalY));

            foreach (var glyph in usable)
            {
                glyph.FrameX   -= result.BaselineX;
                glyph.FrameY   -= result.BaselineY;
                glyph.OpticalX -= result.BaselineX;
                glyph.OpticalY -= result.BaselineY;
            }

            var final = usable.ToDictionary(g => g, g => (X: g.OpticalX, Y: g.OpticalY));

            (result.FrameGroups, result.PinnedGroups) = PinIconsThatAreTheSameDrawing(usable, final, em, settings);
            ApplyAlignmentGroups(font, usable, final, warnings);

            foreach (var glyph in usable)
            {
                (glyph.TargetX, glyph.TargetY) = final[glyph];
                glyph.RejectedX                = Math.Abs(glyph.TargetX) > settings.MaxAdjustment;
                glyph.RejectedY                = Math.Abs(glyph.TargetY) > settings.MaxAdjustment;
            }

            RejectAlignmentGroupsTogether(font, usable);

            foreach (var glyph in usable)
            {
                glyph.X = glyph.RejectedX ? 0 : Quantize(glyph.TargetX, settings);
                glyph.Y = glyph.RejectedY ? 0 : Quantize(glyph.TargetY, settings);
            }

            return result;
        }

        /// <summary>
        /// Finds the icons that are the same drawing and gives each such set one shared offset, so a
        /// square and a checkbox cannot end up either side of a rounding step. Two icons count as the same
        /// drawing when their ink boxes match <em>and</em> they already agree on where their optical centre
        /// is; a shared ink box alone means nothing, since thousands of icons are simply drawn edge to edge.
        /// <para>
        /// Both passes cluster around a leader rather than merging neighbours, so a long chain of slightly
        /// different values cannot collapse into one oversized group. Because a member is always within the
        /// tolerance of its leader, sharing the group mean moves nobody by more than that tolerance - which
        /// is set to half a rounding step, so pinning never costs more than the rounding already does.
        /// </para>
        /// </summary>
        private static (int Frames, int Pinned) PinIconsThatAreTheSameDrawing(
            List<GlyphAdjustment>                            glyphs,
            Dictionary<GlyphAdjustment, (double X, double Y)> final,
            double                                           em,
            CenteringSettings                                settings)
        {
            double[] InkBox(GlyphAdjustment g) => new[]
            {
                g.Measurement.InkLeft / em, g.Measurement.InkTop / em,
                g.Measurement.InkRight / em, g.Measurement.InkBottom / em,
            };

            var frames = Cluster(glyphs, InkBox, settings.FrameTolerance);
            var pinned = 0;

            for (int f = 0; f < frames.Count; f++)
            {
                var frame = frames[f];

                foreach (var member in frame)
                {
                    member.FrameGroup     = f;
                    member.FrameGroupSize = frame.Count;
                }

                // Same ink box, now split by where the icons actually look centred.
                foreach (var group in Cluster(frame, g => new[] { final[g].X, final[g].Y }, settings.MaxSharedFrameSpread))
                {
                    if (group.Count < 2) continue;

                    var x = group.Average(m => final[m].X);
                    var y = group.Average(m => final[m].Y);

                    foreach (var member in group)
                    {
                        member.PinnedGroup     = pinned;
                        member.PinnedGroupSize = group.Count;
                        member.IsPinned        = true;
                        final[member]          = (x, y);
                    }

                    pinned++;
                }
            }

            return (frames.Count, pinned);
        }

        /// <summary>
        /// Leader clustering: each item joins the first existing group whose leader is within tolerance on
        /// every axis, or starts one. Bounds every group's radius at the tolerance, unlike merging
        /// neighbours, which lets a chain of near-matches grow without limit.
        /// </summary>
        private static List<List<GlyphAdjustment>> Cluster(
            IReadOnlyList<GlyphAdjustment>           glyphs,
            Func<GlyphAdjustment, double[]> signature,
            double                                   tolerance)
        {
            var ordered = glyphs
               .Select(g => (Glyph: g, Signature: signature(g)))
               .OrderBy(g => g.Signature[0])
               .ThenBy(g => g.Signature.Length > 1 ? g.Signature[1] : 0)
               .ToList();

            var leaders  = new List<double[]>();
            var clusters = new List<List<GlyphAdjustment>>();

            foreach (var (glyph, values) in ordered)
            {
                int match = -1;

                // Leaders are visited newest first: the input is sorted on the first axis, so a match is
                // almost always the group that was just created, which keeps this linear in practice.
                for (int i = leaders.Count - 1; i >= 0; i--)
                {
                    if (leaders[i][0] < values[0] - tolerance) break;

                    if (Enumerable.Range(0, values.Length).All(k => Math.Abs(leaders[i][k] - values[k]) <= tolerance))
                    {
                        match = i;
                        break;
                    }
                }

                if (match < 0)
                {
                    leaders.Add(values);
                    clusters.Add(new List<GlyphAdjustment>());
                    match = leaders.Count - 1;
                }

                clusters[match].Add(glyph);
            }

            return clusters;
        }

        /// <summary>
        /// If one icon of a pinned group is too far off centre to be corrected, none of them are corrected:
        /// nudging only half of a pair is exactly the jump these groups exist to prevent.
        /// </summary>
        private static void RejectAlignmentGroupsTogether(IconFont font, List<GlyphAdjustment> glyphs)
        {
            var byName = glyphs.ToDictionary(g => g.Glyph.IconName, StringComparer.Ordinal);

            foreach (var group in AlignmentGroups.All)
            {
                var members = group.Icons.Where(byName.ContainsKey).Select(n => byName[n]).ToList();

                if (members.Count < 2) continue;

                // A mirrored pair only shares one axis, so only that axis is rejected together.
                if (group.Kind != AlignmentKind.SharedVertical && members.Any(m => m.RejectedX))
                {
                    foreach (var member in members) member.RejectedX = true;
                }

                if (group.Kind != AlignmentKind.SharedHorizontal && members.Any(m => m.RejectedY))
                {
                    foreach (var member in members) member.RejectedY = true;
                }
            }
        }

        /// <summary>Pins the icons the toolkit swaps in place to each other.</summary>
        private static void ApplyAlignmentGroups(
            IconFont                                         font,
            List<GlyphAdjustment>                            glyphs,
            Dictionary<GlyphAdjustment, (double X, double Y)> final,
            List<string>                                     warnings)
        {
            var byName = glyphs.ToDictionary(g => g.Glyph.IconName, StringComparer.Ordinal);

            foreach (var group in AlignmentGroups.All)
            {
                var members = group.Icons.Where(byName.ContainsKey).Select(n => byName[n]).ToList();

                // A font that has none of a group's icons simply does not cover that group (the brands font
                // covers almost nothing); only a partially present group is worth a warning.
                if (members.Count > 0)
                {
                    foreach (var missing in group.Icons.Where(n => !byName.ContainsKey(n)))
                    {
                        warnings.Add($"{font.FontFamily}: alignment group '{group.Name}' has no glyph named {missing}");
                    }
                }

                if (members.Count < 2) continue;

                switch (group.Kind)
                {
                    case AlignmentKind.Aligned:
                        Assign(members, members.Average(m => final[m].X), members.Average(m => final[m].Y));
                        break;

                    case AlignmentKind.AnchorFirst:
                        Assign(members, final[members[0]].X, final[members[0]].Y);
                        break;

                    case AlignmentKind.SharedVertical:
                        var sharedY = members.Average(m => final[m].Y);
                        foreach (var member in members) final[member] = (final[member].X, sharedY);
                        break;

                    case AlignmentKind.SharedHorizontal:
                        var sharedX = members.Average(m => final[m].X);
                        foreach (var member in members) final[member] = (sharedX, final[member].Y);
                        break;
                }
            }

            void Assign(List<GlyphAdjustment> members, double x, double y)
            {
                foreach (var member in members) final[member] = (x, y);
            }
        }

        private static double Clamp(double value, double limit) => Math.Max(-limit, Math.Min(limit, value));

        private static double Quantize(double value, CenteringSettings settings)
        {
            if (Math.Abs(value) < settings.DeadZone) return 0;

            var steps = Math.Round(value / settings.Step, MidpointRounding.AwayFromZero);
            return steps * settings.Step;
        }

        private static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(v => v).ToArray();
            if (sorted.Length == 0) return 0;
            return sorted.Length % 2 == 1 ? sorted[sorted.Length / 2] : (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;
        }

        /// <summary>Formats an offset the way it is written to css.</summary>
        public static string Format(double em) => em.ToString("0.###", CultureInfo.InvariantCulture) + "em";
    }
}
