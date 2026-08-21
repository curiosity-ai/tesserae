using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 70, Icon = UIcons.Columns3)]
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
                        .SegmentedPivot("tab4", SegmentTitle("Firewall"),  () => CenteredWithBackground(Message("Firewall Content"))),
                    SampleSubTitle("Tab Overflow"),
                    SplitView().Resizable().WS().H(400).LeftIsSmaller(320.px()).Left(
                        SegmentedPivot().S()
                            .SegmentedPivot("s1", SegmentTitle("Overview"),   () => CenteredWithBackground(Message("Overview Content")))
                            .SegmentedPivot("s2", SegmentTitle("Logs"),       () => CenteredWithBackground(Message("Logs Content")))
                            .SegmentedPivot("s3", SegmentTitle("Analytics"),  () => CenteredWithBackground(Message("Analytics Content")))
                            .SegmentedPivot("s4", SegmentTitle("Firewall"),   () => CenteredWithBackground(Message("Firewall Content")))
                            .SegmentedPivot("s5", SegmentTitle("Alerts"),     () => CenteredWithBackground(Message("Alerts Content")))
                            .SegmentedPivot("s6", SegmentTitle("Reports"),    () => CenteredWithBackground(Message("Reports Content")))
                            .SegmentedPivot("s7", SegmentTitle("Settings"),   () => CenteredWithBackground(Message("Settings Content"))))
                    .Right(TextBlock("👈 resize this area to squeeze the segmented control — use the chevrons or the mouse wheel to scroll the tab strip, and click the ⋯ button for an All Tabs menu").WS().BreakSpaces())
               )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("AI() marks the pivot as an AI one - tabs over what a model produced, or over the ways of asking it. The segmented track takes the purple-to-blue tint, the tabs the accent, and the selected tab is filled with the gradient, the same way an AI IconToggle fills its selected pill. The chevrons and the overflow menu belong to the pill, so they take the accent too."),
                    SampleSubTitle("An answer, its sources and its reasoning"),
                    SegmentedPivot().AI()
                        .SegmentedPivot("ai1", SegmentTitle("Answer"),    () => CenteredWithBackground(TextBlock("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs.").AISurface()))
                        .SegmentedPivot("ai2", SegmentTitle("Sources"),   () => CenteredWithBackground(HStack().Wrap().Gap(6.px()).Children(InlineLabel("12 documents").AI(), InlineLabel("BRK-SEN-447").SetIcon(UIcons.Folder))))
                        .SegmentedPivot("ai3", SegmentTitle("Reasoning"), () => CenteredWithBackground(TextBlock("Every failure follows a mount torque below 12 Nm.").AISurface()))
               )).SetTitle("The AI variant", UIcons.Sparkles, Theme.Colors.Purple600)))
               .SeeAlso(typeof(PivotSample), typeof(CardPivotSample), typeof(PivotSelectorSample), typeof(ChoiceGroupSample), typeof(ToggleSample), typeof(AIVariantsSample));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
