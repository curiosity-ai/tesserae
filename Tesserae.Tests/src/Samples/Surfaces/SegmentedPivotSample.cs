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
                    SampleSubTitle("Filling the content area"),
                    TextBlock("The content pane lays the active tab out like a Stack with a single child, so content sized to fill expands to the whole area. Switch tabs to see what each helper claims — the coloured box is the tab content."),
                    Stack().H(320).WS().Children(
                        SegmentedPivot()
                            .SegmentedPivot("f-s",    SegmentTitle(".S()"),        () => Box(".S() — fills width and height").S())
                            .SegmentedPivot("f-hs",   SegmentTitle(".HS()"),       () => Box(".HS() — fills height (.AlignStart keeps width natural)").HS().AlignStart())
                            .SegmentedPivot("f-ws",   SegmentTitle(".WS()"),       () => Box(".WS() — fills width, natural height").WS())
                            .SegmentedPivot("f-grow", SegmentTitle(".Grow()"),     () => Box(".Grow() — grows to fill the height").Grow())
                            .SegmentedPivot("f-own",  SegmentTitle("height:100%"), () => CenteredWithBackground(Message("Own height:100% (e.g. CenteredWithBackground) — fills")))
                        .S())
               )).SetTitle("Filling")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Restricting the content size"),
                    TextBlock("The same helpers cap or pin the content. Fixed and intrinsic sizes keep their value; content taller than the panel scrolls within the pane."),
                    Stack().H(320).WS().Children(
                        SegmentedPivot()
                            .SegmentedPivot("r-h",   SegmentTitle(".Height"),    () => Box(".WS().Height(120) — fixed height").WS().Height(120.px()))
                            .SegmentedPivot("r-mxh", SegmentTitle(".MaxHeight"), () => Box(".HS().MaxHeight(160) — capped height").HS().MaxHeight(160.px()))
                            .SegmentedPivot("r-mnh", SegmentTitle(".MinHeight"), () => Box(".WS().MinHeight(240) — at least this tall").WS().MinHeight(240.px()))
                            .SegmentedPivot("r-w",   SegmentTitle(".Width"),     () => Box(".Width(260) — fixed width").Width(260.px()))
                            .SegmentedPivot("r-mxw", SegmentTitle(".MaxWidth"),  () => Box(".WS().MaxWidth(320) — capped width").WS().MaxWidth(320.px()))
                            .SegmentedPivot("r-tall", SegmentTitle("Overflow"),  () => TallBox("Intrinsic content taller than the panel scrolls"))
                        .S())
               )).SetTitle("Restricting")));
        }

        // A visible, centred box with no size of its own; callers apply the sizing helper
        // being demonstrated so its effect on the content area is obvious.
        private static Stack Box(string label) =>
            VStack()
               .Background("var(--tss-secondary-background-color)")
               .AlignItemsCenter()
               .JustifyContent(ItemJustify.Center)
               .P(16)
               .Children(TextBlock(label).SemiBold());

        // Intrinsic content taller than the 320px panel, to show the pane scrolling.
        private static Stack TallBox(string label) =>
            VStack()
               .Background("var(--tss-secondary-background-color)")
               .AlignItemsCenter()
               .WS()
               .P(16)
               .Children(
                    TextBlock(label).SemiBold(),
                    TextBlock("Row 1"), TextBlock("Row 2"), TextBlock("Row 3"), TextBlock("Row 4"),
                    TextBlock("Row 5"), TextBlock("Row 6"), TextBlock("Row 7"), TextBlock("Row 8"),
                    TextBlock("Row 9"), TextBlock("Row 10"), TextBlock("Row 11"), TextBlock("Row 12"),
                    TextBlock("Row 13"), TextBlock("Row 14"), TextBlock("Row 15"), TextBlock("Row 16"),
                    TextBlock("Row 17"), TextBlock("Row 18"), TextBlock("Row 19"), TextBlock("Row 20"));

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
