namespace Tesserae.Tests
{
    /// <summary>
    /// The gallery's categories, and the order they appear in the sidebar.
    /// A sample names its category through <see cref="SampleDetailsAttribute.Group"/>; the
    /// sidebar renders one separator per category, in the order <see cref="InDisplayOrder"/>
    /// lists them, so a category is added by adding a constant here and listing it there.
    /// The order runs from what you reach for first (containers, text, buttons, inputs) to the
    /// specialised surfaces, and finally to the helpers that render nothing on their own.
    /// <para>
    /// Each entry also carries what the home page needs to introduce the category — a one-line
    /// blurb, an icon and a tint — so the sidebar and the home page cannot disagree about what a
    /// category is called or what belongs in it.
    /// </para>
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

        /// <summary>One category: its name, what it holds, and how the home page draws it.</summary>
        internal sealed class Category
        {
            public Category(string name, string blurb, UIcons icon, string tint)
            {
                Name  = name;
                Blurb = blurb;
                Icon  = icon;
                Tint  = tint;
            }

            /// <summary>The category name, as a sample's <c>Group</c> and as the sidebar separator.</summary>
            public string Name  { get; }

            /// <summary>One line saying what belongs in the category.</summary>
            public string Blurb { get; }

            /// <summary>The glyph the home page marks the category's section with.</summary>
            public UIcons Icon  { get; }

            /// <summary>The colour the category's cards are tinted with, as a CSS colour.</summary>
            public string Tint  { get; }
        }

        /// <summary>Every category, in the order the sidebar shows them.</summary>
        public static readonly Category[] InDisplayOrder = new[]
        {
            new Category(Layout,     "The containers a page is built out of.",                     UIcons.BorderAll,      "#6366f1"),
            new Category(Text,       "Text, labels and rich content blocks.",                      UIcons.Text,           "#0ea5e9"),
            new Category(Commands,   "What the user clicks to make something happen.",             UIcons.Cursor,         "#8b5cf6"),
            new Category(Inputs,     "Form controls that capture a value.",                        UIcons.InputText,      "#10b981"),
            new Category(DateTime,   "Calendar, clock and schedule pickers.",                      UIcons.CalendarClock,  "#14b8a6"),
            new Category(Forms,      "Binding a form to data, validating it and saving it.",       UIcons.ListCheck,      "#22c55e"),
            new Category(Navigation, "Moving between pages, sections and tabs.",                   UIcons.Sidebar,        "#3b82f6"),
            new Category(Lists,      "Rendering a collection of items.",                           UIcons.TableRows,      "#0891b2"),
            new Category(Search,     "Search inputs and the surfaces that show their results.",    UIcons.Search,         "#06b6d4"),
            new Category(Charts,     "Numbers and relationships, drawn.",                          UIcons.ChartMixed,     "#f59e0b"),
            new Category(Feedback,   "Progress, notifications, tooltips and empty states.",        UIcons.Bell,           "#ef4444"),
            new Category(Overlays,   "Surfaces that float above the page.",                        UIcons.WindowRestore,  "#a855f7"),
            new Category(AI,         "Conversation, tool calls and the context around them.",      UIcons.CommentAlt,     "#ec4899"),
            new Category(Media,      "Images, avatars and embedded content.",                      UIcons.Images,         "#f97316"),
            new Category(Theming,    "Colours, gradients, icons and emoji.",                       UIcons.Swatchbook,     "#eab308"),
            new Category(Utilities,  "Helpers that render little or nothing on their own.",        UIcons.Link,           "#64748b"),
        };

        /// <summary>
        /// Where a category sits in the sidebar. A category that isn't listed sorts after every
        /// listed one, so a sample with a typo'd (or missing) group still shows up, at the end.
        /// </summary>
        public static int DisplayIndex(string group)
        {
            for (var i = 0; i < InDisplayOrder.Length; i++)
            {
                if (InDisplayOrder[i].Name == group) return i;
            }
            return InDisplayOrder.Length;
        }

        /// <summary>
        /// The metadata for a category, or null when the group isn't one of the listed categories —
        /// which is what a sample with a typo'd (or missing) group ends up in.
        /// </summary>
        public static Category Describe(string group)
        {
            for (var i = 0; i < InDisplayOrder.Length; i++)
            {
                if (InDisplayOrder[i].Name == group) return InDisplayOrder[i];
            }
            return null;
        }
    }
}
