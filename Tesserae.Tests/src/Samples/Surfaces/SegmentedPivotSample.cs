using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 23, Icon = UIcons.MenuDots)]
    public class SegmentedPivotSample : IComponent, ISample
    {
        private readonly IComponent content;

        public SegmentedPivotSample()
        {
            content = SectionStack().Secondary()
               .SampleTitle(typeof(SegmentedPivotSample), UIcons.Apps, "A segmented navigation pivot")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A SegmentedPivot is a tabbed interface styled as a segmented control. It's best used for toggling between closely related views or filters where space is limited."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Usage"),
                    SegmentedPivot()
                        .SegmentedPivot("tab1", SegmentTitle("Overview"),  () => CenteredWithBackground(Message("Overview Content")))
                        .SegmentedPivot("tab2", SegmentTitle("Logs"),      () => CenteredWithBackground(Message("Logs Content")))
                        .SegmentedPivot("tab3", SegmentTitle("Analytics"), () => CenteredWithBackground(Message("Analytics Content")))
                        .SegmentedPivot("tab4", SegmentTitle("Firewall"),  () => CenteredWithBackground(Message("Firewall Content")))
               )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Filling content"),
                    TextBlock("Tab content sized to fill (with .S(), .Grow(), or its own height) expands to the whole content area, so panel-filling UIs such as a search area do not collapse."),
                    Stack().H(320).WS().Children(
                        SegmentedPivot()
                            .SegmentedPivot("fill1", SegmentTitle("Search"),  () => FillingArea("Results pane fills the panel"))
                            .SegmentedPivot("fill2", SegmentTitle("Details"), () => FillingArea("This one fills too"))
                        .S())
               )).SetTitle("Filling")));
        }

        // A panel-filling tab body: content sized to fill (here via CenteredWithBackground,
        // which is height:100%) expands to the whole pivot content area instead of
        // collapsing, mirroring a search area whose results pane should fill the panel.
        private static IComponent FillingArea(string label) =>
            CenteredWithBackground(Message(label));

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
