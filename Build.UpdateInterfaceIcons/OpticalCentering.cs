using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Build.UpdateInterfaceIcons
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

        /// <summary>
        /// How far a correction is allowed to push a glyph's ink out of the box it is laid out in. Zero means
        /// never: an icon drawn to the edge of the em square then keeps whatever centering fits in the room it
        /// has, and most of them have none, so most vertical corrections go. Raising it trades the guarantee
        /// back for centering - at 0.005em an icon may lose a third of a pixel off a 20px render - and the
        /// report says exactly how many offsets the current value costs.
        /// </summary>
        public double Overhang { get; set; } = 0.0;

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

    /// <summary>
    /// One glyph's drawn extent and advance, in font units, read from the outline rather than from a
    /// rasterization. What the centering is measured against is the rendered glyph; what a correction has
    /// to keep inside the layout box is the outline, exactly, which is why this comes from the font.
    /// </summary>
    internal sealed class GlyphOutline
    {
        public int UnitsPerEm { get; set; }

        /// <summary>Advance width, i.e. the right edge of the box the glyph is laid out in.</summary>
        public int Advance { get; set; }

        /// <summary>The ink, in font units, x from the pen and y up from the baseline.</summary>
        public TransformedGlyf.Box Ink { get; set; }
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

        /// <summary>Set when this glyph's offset was taken from another icon it has to stay registered with.</summary>
        public bool PinnedToPartner { get; set; }

        /// <summary>Set when the offset was dropped so the frame family this icon belongs to stays registered.</summary>
        public bool LeftAloneForItsFamily { get; set; }

        /// <summary>Set when the offset was cut back to keep the glyph's ink inside the box it is laid out in.</summary>
        public bool CappedToStayInside { get; set; }

        /// <summary>How much correction that cap gave up, in em, on each axis.</summary>
        public double GivenUpX { get; set; }

        public double GivenUpY { get; set; }

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

        /// <summary>How many state variants took their base icon's offset.</summary>
        public int StateVariantsAnchored { get; set; }

        /// <summary>How many offsets were dropped to keep a frame family registered.</summary>
        public int FrameFamiliesSuppressed { get; set; }

        /// <summary>How many offsets were cut back to keep the glyph's ink inside its layout box.</summary>
        public int OffsetsCapped { get; set; }

        /// <summary>How many of those were cut back to nothing, losing the correction on that axis entirely.</summary>
        public int OffsetsCappedToNothing { get; set; }
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
        public static FontAdjustments Compute(
            IconFont                             font,
            FontMeasurement                      measurement,
            IReadOnlyDictionary<int, GlyphOutline> outlines,
            CenteringSettings                    settings,
            List<string>                         warnings)
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
            result.StateVariantsAnchored = AnchorStateVariantsToTheirBase(usable, final);
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

            result.FrameFamiliesSuppressed = LeaveDisagreeingFrameFamiliesAlone(usable, em, settings);

            // Last, because it caps whatever the rules above settled on: every earlier step can only make an
            // offset larger or share it around, and this is the one that has to hold for the value that ships.
            KeepInkInsideItsLayoutBox(font, result, usable, outlines, measurement, settings, warnings);

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

        /// <summary>
        /// Gives every <c>X-slash</c> / <c>X-crossed</c> / <c>X-off</c> / <c>X-mute</c> icon the offset of the
        /// <c>X</c> it is a state of, so the drawing the two share stays put when a UI swaps one for the other.
        /// </summary>
        private static int AnchorStateVariantsToTheirBase(
            List<GlyphAdjustment>                            glyphs,
            Dictionary<GlyphAdjustment, (double X, double Y)> final)
        {
            var byName  = glyphs.ToDictionary(g => g.Glyph.IconName, StringComparer.Ordinal);
            var anchored = 0;

            foreach (var glyph in glyphs)
            {
                var baseName = StateVariants.BaseIconOf(glyph.Glyph.IconName);

                if (baseName is null || !byName.TryGetValue(baseName, out var baseGlyph)) continue;

                final[glyph]           = final[baseGlyph];
                glyph.PinnedToPartner  = true;
                anchored++;
            }

            return anchored;
        }

        /// <summary>
        /// Icons drawn on the same frame — same ink box, and the frame's name in common — have to keep that
        /// frame registered with each other. Where they cannot agree on one offset, none of them is moved:
        /// leaving a family as drawn is always safe, whereas nudging part of it is what makes a row of
        /// square-framed icons look ragged.
        /// </summary>
        private static int LeaveDisagreeingFrameFamiliesAlone(List<GlyphAdjustment> glyphs, double em, CenteringSettings settings)
        {
            var suppressed = 0;

            foreach (var word in FrameShapes.Words)
            {
                var family = glyphs.Where(g => FrameShapes.Mentions(g.Glyph.IconName, word)).ToList();

                if (family.Count < 2) continue;

                foreach (var sharingAFrame in family.GroupBy(g => (
                             Math.Round(g.Measurement.InkLeft / em, 3), Math.Round(g.Measurement.InkRight / em, 3),
                             Math.Round(g.Measurement.InkTop / em, 3), Math.Round(g.Measurement.InkBottom / em, 3))))
                {
                    var members = sharingAFrame.ToList();

                    if (members.Count < 2) continue;

                    var disagrees = members.Max(m => m.X) - members.Min(m => m.X) > 1e-9
                                 || members.Max(m => m.Y) - members.Min(m => m.Y) > 1e-9;

                    if (!disagrees) continue;

                    foreach (var member in members.Where(m => m.IsAdjusted))
                    {
                        member.X                   = 0;
                        member.Y                   = 0;
                        member.LeftAloneForItsFamily = true;
                        suppressed++;
                    }
                }
            }

            return suppressed;
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

                // Fixed is the one kind that is meaningful for a single icon.
                if (group.Kind == AlignmentKind.Fixed)
                {
                    foreach (var member in members)
                    {
                        final[member]         = (0, 0);
                        member.PinnedToPartner = true;
                    }

                    continue;
                }

                if (members.Count < 2) continue;

                foreach (var member in members) member.PinnedToPartner = true;

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

        /// <summary>
        /// Caps every offset so a correction can never push a glyph's ink out of the box the browser lays it
        /// out in - <c>[0, advance]</c> across, and the ascent and descent around the baseline down, which in
        /// these fonts is exactly the em square. Ink outside that box is what a container of
        /// <c>height:1em;overflow:hidden</c> crops, and the centering used to walk icons straight out of it:
        /// an icon drawn to the edge of the em square has no room above it, so any upward nudge cost ink.
        /// <para>
        /// A glyph already drawn outside its box keeps what it has - that is the vendor's drawing, not a
        /// mistake to correct - and is only stopped from going further out. So the room on each side is
        /// whatever slack the ink has, or zero, never negative, which means <em>no shift</em> is always
        /// inside the cap and the cap can always be satisfied.
        /// </para>
        /// <para>
        /// Capping is per glyph but has to apply per <em>shared offset</em>, or it is the thing that pulls a
        /// checkbox off its square: whatever the rules above put on one offset is capped by the least room any
        /// of its members has. And the cap lands on the same rounding grid as everything else, snapped towards
        /// zero, so an offset that survives is still one a lookalike can share.
        /// </para>
        /// </summary>
        private static void KeepInkInsideItsLayoutBox(
            IconFont                               font,
            FontAdjustments                        result,
            List<GlyphAdjustment>                  glyphs,
            IReadOnlyDictionary<int, GlyphOutline> outlines,
            FontMeasurement                        measurement,
            CenteringSettings                      settings,
            List<string>                           warnings)
        {
            if (outlines is null || outlines.Count == 0)
            {
                warnings.Add($"{font.FontFamily}: no outlines to cap the offsets against, so nothing was capped");
                return;
            }

            var index = new Dictionary<GlyphAdjustment, int>(glyphs.Count);
            for (int i = 0; i < glyphs.Count; i++) index[glyphs[i]] = i;

            var acrossSet = new SharedOffsets(glyphs.Count);
            var downSet   = new SharedOffsets(glyphs.Count);
            var byName    = glyphs.ToDictionary(g => g.Glyph.IconName, StringComparer.Ordinal);

            // The three rules that put glyphs on one offset, mirrored - including which of them a glyph is
            // no longer bound by once a later rule replaced its offset, the same exclusions the pinned group
            // check makes.
            foreach (var cluster in glyphs
               .Where(g => g.IsPinned && !g.PinnedToPartner && !g.LeftAloneForItsFamily)
               .GroupBy(g => g.PinnedGroup))
            {
                Join(cluster.ToList(), across: true, down: true);
            }

            foreach (var glyph in glyphs)
            {
                var baseName = StateVariants.BaseIconOf(glyph.Glyph.IconName);

                if (baseName != null && byName.TryGetValue(baseName, out var baseGlyph))
                {
                    Join(new List<GlyphAdjustment> { baseGlyph, glyph }, across: true, down: true);
                }
            }

            foreach (var group in AlignmentGroups.All)
            {
                Join(group.Icons.Where(byName.ContainsKey).Select(n => byName[n]).ToList(),
                     across: group.Kind != AlignmentKind.SharedVertical,
                     down:   group.Kind != AlignmentKind.SharedHorizontal);
            }

            void Join(List<GlyphAdjustment> members, bool across, bool down)
            {
                for (int i = 1; i < members.Count; i++)
                {
                    if (across) acrossSet.Join(index[members[0]], index[members[i]]);
                    if (down)   downSet.Join(index[members[0]], index[members[i]]);
                }
            }

            // The layout box, in font units. Read from what inline layout actually uses rather than assumed
            // to be the em square, which is only what these fonts happen to declare.
            var ascent  = measurement.Ascent / measurement.Em;
            var descent = measurement.Descent / measurement.Em;

            var acrossLow = Fill(glyphs.Count, double.NegativeInfinity);
            var acrossHigh = Fill(glyphs.Count, double.PositiveInfinity);
            var downLow   = Fill(glyphs.Count, double.NegativeInfinity);
            var downHigh  = Fill(glyphs.Count, double.PositiveInfinity);
            var unknown   = 0;

            for (int i = 0; i < glyphs.Count; i++)
            {
                if (!outlines.TryGetValue(glyphs[i].Glyph.CodePoint, out var outline))
                {
                    unknown++;
                    continue;
                }

                var upem = (double)outline.UnitsPerEm;
                var ink  = outline.Ink;

                // css x grows right and y grows down; font units grow right and up.
                var spare = settings.Overhang * upem;

                var room = (
                    Left:  -Math.Max(0, ink.XMin + spare) / upem,
                    Right:  Math.Max(0, outline.Advance - ink.XMax + spare) / upem,
                    Up:    -Math.Max(0, ascent * upem - ink.YMax + spare) / upem,
                    Down:   Math.Max(0, ink.YMin + descent * upem + spare) / upem);

                var across = acrossSet.Root(i);
                var down   = downSet.Root(i);

                acrossLow[across]  = Math.Max(acrossLow[across], room.Left);
                acrossHigh[across] = Math.Min(acrossHigh[across], room.Right);
                downLow[down]      = Math.Max(downLow[down], room.Up);
                downHigh[down]     = Math.Min(downHigh[down], room.Down);
            }

            if (unknown > 0)
            {
                warnings.Add($"{font.FontFamily}: {unknown} glyphs have no outline in the font, so their offsets were left uncapped");
            }

            for (int i = 0; i < glyphs.Count; i++)
            {
                var glyph  = glyphs[i];
                var across = acrossSet.Root(i);
                var down   = downSet.Root(i);
                var x      = Snap(Bound(glyph.X, acrossLow[across], acrossHigh[across]), settings);
                var y      = Snap(Bound(glyph.Y, downLow[down], downHigh[down]), settings);

                if (x == glyph.X && y == glyph.Y) continue;

                glyph.GivenUpX           = glyph.X - x;
                glyph.GivenUpY           = glyph.Y - y;
                glyph.CappedToStayInside = true;
                glyph.X                  = x;
                glyph.Y                  = y;
                result.OffsetsCapped++;

                if (x == 0 && y == 0) result.OffsetsCappedToNothing++;
            }
        }

        private static double[] Fill(int length, double value)
        {
            var values = new double[length];
            Array.Fill(values, value);
            return values;
        }

        /// <summary>Clamps without throwing on an inverted range, which the room can never produce but a bug could.</summary>
        private static double Bound(double value, double low, double high) => Math.Min(Math.Max(value, low), high);

        /// <summary>
        /// Rounds towards zero onto the offset grid, so a capped offset is still a value another icon can be
        /// pinned to, and is still small enough to survive the conversion to whole font units at bake time.
        /// </summary>
        private static double Snap(double value, CenteringSettings settings)
        {
            var steps = Math.Truncate(Math.Abs(value) / settings.Step + 1e-9);
            return Math.Sign(value) * steps * settings.Step;
        }

        /// <summary>
        /// Which glyphs have to be capped as one. A union-find rather than a group id, because the three
        /// rules that share an offset overlap: a state variant of a pinned icon ties its cluster to the
        /// cluster of the icon it is a state of.
        /// </summary>
        private sealed class SharedOffsets
        {
            private readonly int[] _parent;

            public SharedOffsets(int count)
            {
                _parent = new int[count];
                for (int i = 0; i < count; i++) _parent[i] = i;
            }

            public int Root(int i)
            {
                while (_parent[i] != i) i = _parent[i] = _parent[_parent[i]];
                return i;
            }

            public void Join(int a, int b)
            {
                a = Root(a);
                b = Root(b);
                if (a != b) _parent[b] = a;
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
