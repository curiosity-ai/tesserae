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

        /// <summary>Never adjusted. For a glyph composited over arbitrary other icons, where moving it would misregister it against all of them.</summary>
        Fixed,

        /// <summary>
        /// A left/right pair. Only the vertical offset is shared; the horizontal one stays per glyph,
        /// because mirrored glyphs legitimately need mirrored horizontal nudges.
        /// </summary>
        SharedVertical,

        /// <summary>An up/down pair. Only the horizontal offset is shared, for the same reason.</summary>
        SharedHorizontal,
    }

    /// <summary>
    /// Icons whose name is a base icon plus a state suffix are the same drawing with something struck
    /// through it, and a UI swaps one for the other in place — muting a microphone, disabling a camera.
    /// The variant therefore adopts the base icon's offset, so the part they share never jumps.
    /// <para>
    /// This is the general form of the curated heart/heart-slash style entries: 464 such pairs exist in
    /// the set, and 93 of them disagreed with their base before the rule was applied.
    /// </para>
    /// </summary>
    internal static class StateVariants
    {
        public static readonly string[] Suffixes = { "slash", "crossed", "off", "mute", "muted", "disabled" };

        /// <summary>The base icon this name is a state variant of, or null if it is not one.</summary>
        public static string BaseIconOf(string iconName)
        {
            foreach (var suffix in Suffixes)
            {
                if (iconName.Length > suffix.Length + 1 && iconName.EndsWith("-" + suffix, System.StringComparison.Ordinal))
                {
                    return iconName.Substring(0, iconName.Length - suffix.Length - 1);
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Shapes an icon can be drawn inside. Icons that share one of these words <em>and</em> share an ink
    /// box are drawn on the same frame — a square with a letter in it, a circle with an arrow in it — and
    /// the frame has to stay registered across the whole family or a row of them looks ragged.
    /// <para>
    /// The name is doing essential work here: <c>circle</c> and <c>square</c> have identical ink boxes,
    /// because both fill the em box edge to edge, as do thousands of unrelated icons. Requiring the word as
    /// well is what separates a frame family from icons that merely fill their box.
    /// </para>
    /// </summary>
    internal static class FrameShapes
    {
        public static readonly string[] Words = { "square", "circle", "rectangle", "hexagon", "octagon", "diamond", "triangle" };

        public static bool Mentions(string iconName, string word) =>
            iconName == word ||
            iconName.StartsWith(word + "-", System.StringComparison.Ordinal) ||
            iconName.EndsWith("-" + word, System.StringComparison.Ordinal) ||
            iconName.Contains("-" + word + "-", System.StringComparison.Ordinal);
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
            // Swapped in place by components through --uicon-var-square, --uicon-var-checkbox and
            // --uicon-var-square-a. Their names share no word, so no rule finds them: they need naming.
            new AlignmentGroup("square frame", AlignmentKind.Aligned, "square", "checkbox", "square-a", "square-minus"),

            // A toggle switch flipping in place. Again, no shared base icon for a rule to key on.
            new AlignmentGroup("toggle", AlignmentKind.Aligned, "toggle-off", "toggle-on"),

            // A base icon plus variants whose names a rule cannot derive.
            new AlignmentGroup("lock", AlignmentKind.AnchorFirst, "lock", "unlock", "lock-open-alt"),

            // Composited over other icons through --uicon-var-slash, so it has to stay where it is drawn:
            // moving it would misregister it against every icon it is struck through.
            new AlignmentGroup("slash overlay", AlignmentKind.Fixed, "slash"),

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
