namespace Tesserae.Tests
{
    /// <summary>
    /// The gallery's categories, and the order they appear in the sidebar.
    /// A sample names its category through <see cref="SampleDetailsAttribute.Group"/>; the
    /// sidebar renders one separator per category, in the order <see cref="InDisplayOrder"/>
    /// lists them, so a category is added by adding a constant here and listing it there.
    /// The order runs from what you reach for first (containers, text, buttons, inputs) to the
    /// specialised surfaces, and finally to the helpers that render nothing on their own.
    /// </summary>
    internal static class SampleGroup
    {
        public const string Layout     = "Layout";
        public const string Text       = "Text & Content";
        public const string Commands   = "Buttons & Commands";
        public const string Inputs     = "Inputs";
        public const string DateTime   = "Date & Time";
        public const string Forms      = "Forms & Validation";
        public const string Navigation = "Navigation";
        public const string Lists      = "Lists & Data";
        public const string Search     = "Search";
        public const string Charts     = "Charts & Visualization";
        public const string Feedback   = "Feedback & Status";
        public const string Overlays   = "Overlays & Dialogs";
        public const string AI         = "AI & Chat";
        public const string Media      = "Media & Graphics";
        public const string Theming    = "Theming & Icons";
        public const string Utilities  = "Utilities & Behaviors";

        /// <summary>Every category, in the order the sidebar shows them.</summary>
        public static readonly string[] InDisplayOrder = new[]
        {
            Layout,
            Text,
            Commands,
            Inputs,
            DateTime,
            Forms,
            Navigation,
            Lists,
            Search,
            Charts,
            Feedback,
            Overlays,
            AI,
            Media,
            Theming,
            Utilities,
        };

        /// <summary>
        /// Where a category sits in the sidebar. A category that isn't listed sorts after every
        /// listed one, so a sample with a typo'd (or missing) group still shows up, at the end.
        /// </summary>
        public static int DisplayIndex(string group)
        {
            for (var i = 0; i < InDisplayOrder.Length; i++)
            {
                if (InDisplayOrder[i] == group) return i;
            }
            return InDisplayOrder.Length;
        }
    }
}
