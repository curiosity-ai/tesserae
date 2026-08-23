using System.Collections.Generic;
using System.Linq;
using Transpose.Core;
using Tesserae.Tests.Samples;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    /// <summary>
    /// The gallery's home page — what you get before a sample is picked, and what the "home" route
    /// returns to. It draws the same list the sidebar does, in the same order
    /// (<see cref="SampleGroup.InDisplayOrder"/>), but as a grid of <see cref="ContextCard"/>s under
    /// a header per category, so the whole toolkit can be read at a glance instead of scrolled
    /// through one item at a time.
    /// <para>
    /// A card carries the sample's icon, its name and the one-line
    /// <see cref="SampleDetailsAttribute.Description"/> declared on the sample, and navigates to it
    /// on click — through <see cref="Router"/>, so the sidebar selection follows along exactly as if
    /// the sidebar entry had been clicked.
    /// </para>
    /// </summary>
    internal sealed class LandingPage : IComponent
    {
        // The sub-label is one ellipsized line, so a card needs a width it can say something in. The
        // cards sit in an auto-filled grid rather than a wrapping row, so a row is filled edge to edge
        // whatever the window is: as many equal columns as fit at this width, each taking a share of
        // what is left over.
        private static readonly UnitSize CardColumns = new UnitSize("repeat(auto-fill, minmax(310px, 1fr))");

        // One colour per category, in the order SampleGroup.InDisplayOrder lists them, so a card's
        // tile still says which group it belongs to once its header has scrolled away. A category
        // past the end of this list falls back to the theme's primary colour.
        private static readonly string[] GroupColors =
        {
            Theme.Colors.Blue600,    // Layout
            Theme.Colors.Teal600,    // Text & Content
            Theme.Colors.Purple600,  // Buttons & Commands
            Theme.Colors.Green600,   // Inputs
            Theme.Colors.Orange600,  // Date & Time
            Theme.Colors.Lime600,    // Forms & Validation
            Theme.Colors.Magenta600, // Navigation
            Theme.Colors.Blue500,    // Lists & Data
            Theme.Colors.Teal500,    // Search
            Theme.Colors.Purple500,  // Charts & Visualization
            Theme.Colors.Yellow600,  // Feedback & Status
            Theme.Colors.Red600,     // Overlays & Dialogs
            Theme.Colors.Magenta500, // AI & Chat
            Theme.Colors.Orange500,  // Media & Graphics
            Theme.Colors.Green500,   // Theming & Icons
            Theme.Colors.Neutral600, // Utilities & Behaviors
        };

        private readonly IComponent _content;

        public LandingPage(IEnumerable<Sample> samples)
        {
            // Same grouping and ordering as the sidebar in App.cs, so the two read as one list.
            var groups = samples
               .GroupBy(s => s.Group)
               .OrderBy(g => SampleGroup.DisplayIndex(g.Key))
               .ThenBy(g => g.Key)
               .ToArray();

            var page = VStack().WS().Gap(32.px()).Padding(32.px());

            page.Add(Header(groups.Sum(g => g.Count()), groups.Length));

            foreach (var group in groups)
            {
                page.Add(Section(group.Key, group.OrderBy(s => s.Order).ThenBy(s => s.Name.ToLower())));
            }

            _content = page;
        }

        public HTMLElement Render() => _content.Render();

        private static IComponent Header(int componentCount, int groupCount) =>
            VStack().WS().Gap(6.px()).Children(
                TextBlock("Tesserae").XLarge().Bold(),
                TextBlock($"{componentCount} components in {groupCount} categories. Pick one to see it running, alongside the code that draws it.")
                   .Medium().Foreground(Theme.Colors.Neutral600),
                HStack().Gap(8.px()).PT(12).Children(
                    Button("Documentation").SetIcon(UIcons.Books).Primary()
                       .OnClick(() => window.open("https://docs.curiosity.ai/tesserae/", "_blank")),
                    Button("Source code").SetIcon(UIcons.ArrowUpRightFromSquare)
                       .OnClick(() => window.open("https://github.com/curiosity-ai/tesserae", "_blank"))));

        private static IComponent Section(string group, IEnumerable<Sample> samples)
        {
            var color = GroupColors[SampleGroup.DisplayIndex(group) % GroupColors.Length];
            var cards = Grid(CardColumns).WS().Gap(12.px());

            foreach (var sample in samples)
            {
                cards.Add(SampleCard(sample, color));
            }

            return VStack().WS().Gap(12.px()).Children(GroupHeader(group, color), cards);
        }

        // The category name, with a rule running out to the end of the row so the groups stay legible
        // once the page is a few hundred cards long.
        private static IComponent GroupHeader(string group, string color) =>
            HStack().WS().AlignItems(ItemAlign.Center).Gap(12.px()).Children(
                TextBlock(group).Large().SemiBold(),
                Raw(Div(Att(styles: s =>
                {
                    s.height     = "1px";
                    s.background = $"color-mix(in srgb, {color} 45%, transparent)";
                }))).Grow());

        private static ContextCard SampleCard(Sample sample, string color) =>
            ContextCard(sample.Name, sample.Icon)
               .SetSubLabel(sample.Description)
               .IconTint(color)
               .WithChevron()
               // A ContextCard sizes itself to its content, and a grid item only stretches to its
               // track when its own width is auto - so the card is told to fill the column.
               .WS()
               // The sub-label is cut to one line, so the full text stays reachable on hover.
               .Tooltip(string.IsNullOrEmpty(sample.Description) ? sample.Name : sample.Description)
               .OnClick((_, __) => Router.Navigate($"#/view/{sample.Name}"));
    }
}
