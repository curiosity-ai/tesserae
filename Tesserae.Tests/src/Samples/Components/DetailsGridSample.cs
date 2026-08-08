using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 106, Icon = UIcons.Table)]
    public class DetailsGridSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DetailsGridSample()
        {
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(DetailsGridSample), UIcons.Table, "A bordered table of label/value rows: the metadata block of a preview")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Components()))
                .FlatSection(VStack().WS().Children(Shapes()))
                .FlatSection(VStack().WS().Children(Stacked()))
                .FlatSection(VStack().WS().Children(HeaderBlock()))
                .SeeAlso(typeof(OmniResultSample), typeof(DetailsListSample), typeof(CardSample));
        }

        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        private IComponent Overview()
        {
            return FeatureCard("Overview", "Label, value, one row each",
                "DetailsGrid is the \"Owner / Size / Modified\" block a preview shows about the thing it is previewing. Row(label, value) adds a row; the labels read in the secondary color and share one column, so the values line up however long the labels are. A row whose value is null or empty still shows, drawn as an em dash, so the same set of fields reads the same whether every one of them is known - pass skipIfEmpty to leave it out instead.",
                DetailsGrid()
                    .Row("Owner",    "Pius Neuhaus")
                    .Row("Size",     "2.4 MB")
                    .Row("Modified", "Apr 12, 2024")
                    .Row("Pages",    "24")
                    .Row("Retention", (string)null)
                    .MaxWidth(480.px()));
        }

        private IComponent Components()
        {
            return FeatureCard("Values that aren't text", "Anything renderable in the value column",
                "Row(label, IComponent) puts a component of the host's own in the value slot - a Link, a Badge, an Avatar, a row of them. A null component leaves the row out entirely, so a caller can build one conditionally without branching around the call.",
                DetailsGrid()
                    .Row("Owner",  HStack().AlignItemsCenter().Children(Avatar(initials: "PN").Size(AvatarSize.Small).MR(8), TextBlock("Pius Neuhaus")))
                    .Row("Status", Badge("Approved").Pill().Success())
                    .Row("Source", InlineLabel("curiosity-ai/tesserae").SetIcon(UIcons.Globe).SetHref("https://github.com/curiosity-ai/tesserae", openInNewTab: true))
                    .Row("Labels", HStack().Children(Badge("brakes").Pill().MR(4), Badge("calibration").Pill()))
                    .MaxWidth(480.px()));
        }

        private IComponent Shapes()
        {
            return FeatureCard("Shapes", "Compact, borderless, wider labels, more columns",
                "Compact() tightens the rows for a grid inside something small. NoBorder() drops the frame and the rules, for a grid that already sits inside something bordered. LabelWidth sets how much room the labels get (120px by default), and Columns(n) lays the rows out n-up rather than one under the other.",
                HStack().WS().Children(
                    VStack().Grow().MR(16).Children(
                        TextBlock("Compact").Small().SemiBold().MB(4),
                        DetailsGrid().Compact()
                            .Row("Owner",    "Marie Lang")
                            .Row("Size",     "480 KB")
                            .Row("Modified", "Apr 11, 2024")),
                    VStack().Grow().Children(
                        TextBlock("NoBorder, wider labels").Small().SemiBold().MB(4),
                        DetailsGrid().NoBorder().LabelWidth(160.px())
                            .Row("Last calibration", "Mar 28, 2024")
                            .Row("Next due",         "Sep 28, 2024")
                            .Row("Responsible",      "Quality team"))),
                TextBlock("Two columns — DetailsGrid(2), or .Columns(2) on one already built:").Small().SemiBold().MT(16).MB(4),
                DetailsGrid(2)
                    .Row("Owner",    "Pius Neuhaus")
                    .Row("Status",   "Approved")
                    .Row("Size",     "2.4 MB")
                    .Row("Pages",    "24")
                    .Row("Modified", "Apr 12, 2024")
                    .Row("Source",   "Box"));
        }

        private IComponent Stacked()
        {
            return FeatureCard("Stacked", "The label as a header over its value",
                "Stacked() turns each row on its side: the label becomes a small, semibold, uppercase header in the secondary color with its value under it, and the frame and the rules go. It is the mode for a sheet of dates and references whose labels (\"FIRST MSN AFFECTED\") are longer than the values under them — the row layout would spend most of its width on the labels. Pair it with Columns(n) to read two or three across.",
                HStack().WS().Wrap().Gap(32.px()).Children(
                    VStack().Grow().Children(
                        TextBlock("One column").Small().SemiBold().MB(8),
                        DetailsGrid().Stacked()
                            .Row("ATA · Sub-ATA",  "73 · 73-21")
                            .Row("Issue number",   "Iss. 3")
                            .Row("Mod available",  "14 Oct 2025")
                            .MaxWidth(240.px())),
                    VStack().Grow().Children(
                        TextBlock("Two columns").Small().SemiBold().MB(8),
                        DetailsGrid(2).Stacked()
                            .Row("ATA · Sub-ATA",       "73 · 73-21")
                            .Row("First MSN affected",  "MSN 0142")
                            .Row("Issue number",        "Iss. 3")
                            .Row("Impacted docs",       "12")),
                    VStack().Grow().Children(
                        TextBlock("Three columns, compact").Small().SemiBold().MB(8),
                        DetailsGrid(3).Stacked().Compact()
                            .Row("ACD",  "16 Oct 2025")
                            .Row("CID",  "25 Oct 2025")
                            .Row("TAC",  "10 Dec 2025")
                            .Row("EIS",  "01 Mar 2026")
                            .Row("SB embodiment",  "15 Jan 2026")
                            .Row("MPD embodiment", "20 Feb 2026"))));
        }

        private IComponent HeaderBlock()
        {
            var grid = DetailsGrid(2).Stacked()
                .Row("ATA · Sub-ATA",      "73 · 73-21")
                .Row("First MSN affected", "MSN 0142")
                .Row("Issue number",       "Iss. 3")
                .Row("Impacted docs",      "12")
                .Row("Mod available",      "14 Oct 2025")
                .Row("PAWO end date",      "23 Oct 2025")
                .Row("ACD",                "16 Oct 2025")
                .Row("CID",                "25 Oct 2025")
                .Row("EIS",                "01 Mar 2026")
                .Row("TAC",                "10 Dec 2025")
                .Row("SB embodiment",      "15 Jan 2026")
                .Row("MPD embodiment",     "20 Feb 2026");

            var freeze = HStack().WS().AlignItemsCenter().Gap(12.px()).PT(16).Children(
                IconTile(UIcons.Snowflake, Theme.Colors.Purple600),
                VStack().Grow().Children(
                    TextBlock("Design frozen date").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground),
                    TextBlock("30 Oct 2025").Medium().SemiBold()),
                Badge("10d to freeze").Pill().Warning());

            return Card(VStack().WS().Children(
                    TextBlock("A record's header block: a stacked, two-column grid of everything the record is filed under, closed by a row the grid isn't part of — an IconTile, one highlighted field and a badge.").MB(16),
                    grid,
                    HorizontalSeparator("").PT(16),
                    freeze).MaxWidth(560.px()))
                .SetTitle("Example: a record header", UIcons.ClipboardList, Theme.Colors.Purple600);
        }

        public HTMLElement Render() => _content.Render();
    }
}
