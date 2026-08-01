using System.Collections.Generic;

namespace Build.UIconsOpticalCentering
{
    /// <summary>How the members of an alignment group must relate to each other.</summary>
    internal enum AlignmentKind
    {
        /// <summary>Every member gets the same offset, so the icons stay interchangeable in place.</summary>
        Aligned,

        /// <summary>Every member adopts the first member's offset: the base icon is the reference the variants sit on.</summary>
        AnchorFirst,

        /// <summary>
        /// A left/right pair. Only the vertical offset is shared; the horizontal one stays per glyph,
        /// because mirrored glyphs legitimately need mirrored horizontal nudges.
        /// </summary>
        SharedVertical,

        /// <summary>An up/down pair. Only the horizontal offset is shared, for the same reason.</summary>
        SharedHorizontal,
    }

    /// <summary>A set of icons that must keep lining up with each other after the adjustment.</summary>
    internal sealed class AlignmentGroup
    {
        public AlignmentGroup(string name, AlignmentKind kind, params string[] icons)
        {
            Name  = name;
            Kind  = kind;
            Icons = icons;
        }

        public string        Name  { get; }
        public AlignmentKind Kind  { get; }
        public string[]      Icons { get; }
    }

    /// <summary>
    /// Icons the toolkit swaps in place — a checkbox drawn on top of a square, a slashed variant drawn
    /// on top of its base icon, an arrow flipped to point the other way. Optical centering is computed
    /// per glyph, so without pinning these together a fractionally different ink distribution could
    /// nudge one of a pair and not the other, and the swap would visibly jump.
    /// <para>
    /// Frame clustering already keeps icons that share a visual frame together; these groups cover the
    /// cases where the frames legitimately differ (a slash sticking out, a mirrored arrow) but the icons
    /// still have to overlap.
    /// </para>
    /// </summary>
    internal static class AlignmentGroups
    {
        public static readonly IReadOnlyList<AlignmentGroup> All = new[]
        {
            // --uicon-var-square and --uicon-var-checkbox are swapped in place by components, so they must overlap exactly.
            new AlignmentGroup("square frame", AlignmentKind.Aligned, "square", "checkbox", "square-minus"),

            // A base icon plus the variants that draw on top of it.
            new AlignmentGroup("lock", AlignmentKind.AnchorFirst, "lock", "unlock", "lock-open-alt"),
            new AlignmentGroup("heart", AlignmentKind.AnchorFirst, "heart", "heart-slash"),
            new AlignmentGroup("bookmark", AlignmentKind.AnchorFirst, "bookmark", "bookmark-slash"),
            new AlignmentGroup("thumbtack", AlignmentKind.AnchorFirst, "thumbtack", "thumbtack-slash"),
            new AlignmentGroup("eye", AlignmentKind.AnchorFirst, "eye", "eye-crossed"),

            // Mirrored pairs: only the axis they share is pinned.
            new AlignmentGroup("angle left/right", AlignmentKind.SharedVertical, "angle-left", "angle-right"),
            new AlignmentGroup("angle small left/right", AlignmentKind.SharedVertical, "angle-small-left", "angle-small-right"),
            new AlignmentGroup("sidebar", AlignmentKind.SharedVertical, "sidebar", "sidebar-flip"),
            new AlignmentGroup("angle up/down", AlignmentKind.SharedHorizontal, "angle-up", "angle-down"),
            new AlignmentGroup("upload/download", AlignmentKind.SharedHorizontal, "upload", "download"),
            new AlignmentGroup("cloud upload/download", AlignmentKind.SharedHorizontal, "cloud-upload", "cloud-download"),
            new AlignmentGroup("cloud upload/download alt", AlignmentKind.SharedHorizontal, "cloud-upload-alt", "cloud-download-alt"),
            new AlignmentGroup("thumbs up/down", AlignmentKind.SharedHorizontal, "thumbs-up", "thumbs-down"),
        };
    }
}
