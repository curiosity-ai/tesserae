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
                        .SegmentedPivot("tab1", () => TextBlock("Overview"),  () => CenteredWithBackground(Message("Overview Content")))
                        .SegmentedPivot("tab2", () => TextBlock("Logs"),      () => CenteredWithBackground(Message("Logs Content")))
                        .SegmentedPivot("tab3", () => TextBlock("Analytics"), () => CenteredWithBackground(Message("Analytics Content")))
                        .SegmentedPivot("tab4", () => TextBlock("Firewall"),  () => CenteredWithBackground(Message("Firewall Content")))
               ));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
