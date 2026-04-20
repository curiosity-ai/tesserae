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
            content = SectionStack()
               .Title(SampleHeader(nameof(SegmentedPivotSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A SegmentedPivot is a tabbed interface styled as a segmented control. It's best used for toggling between closely related views or filters where space is limited.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Usage"),
                    SegmentedPivot()
                        .SegmentedPivot("tab1", () => TextBlock("Overview"), () => Card(TextBlock("Overview Content").P(32)))
                        .SegmentedPivot("tab2", () => TextBlock("Logs"), () => Card(TextBlock("Logs Content").P(32)))
                        .SegmentedPivot("tab3", () => TextBlock("Analytics"), () => Card(TextBlock("Analytics Content").P(32)))
                        .SegmentedPivot("tab4", () => TextBlock("Firewall"), () => Card(TextBlock("Firewall Content").P(32)))
               ));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
