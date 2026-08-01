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
                    .Row("Source", Link("https://github.com/curiosity-ai/tesserae", "curiosity-ai/tesserae"))
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
                TextBlock("Two columns:").Small().SemiBold().MT(16).MB(4),
                DetailsGrid().Columns(2)
                    .Row("Owner",    "Pius Neuhaus")
                    .Row("Status",   "Approved")
                    .Row("Size",     "2.4 MB")
                    .Row("Pages",    "24")
                    .Row("Modified", "Apr 12, 2024")
                    .Row("Source",   "Box"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
