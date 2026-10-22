using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Media, Order = 60, Icon = UIcons.ArrowComparison, Description = "Drag a divider to compare before and after")]
    public class CompareSliderSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CompareSliderSample()
        {
            var position = TextBlock("50%").Secondary();

            var landscape = CompareSlider(Landscape(colour: false), Landscape(colour: true))
               .Labels("Before", "After")
               .OnPositionChanged(p => position.Text = p.ToString("0") + "%")
               .MaxWidth(640.px());

            var themes = CompareSlider(Mock(dark: false), Mock(dark: true), 35).Labels("Light", "Dark").MaxWidth(480.px());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(CompareSliderSample), UIcons.ArrowComparison, "Before and after, under one divider")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("CompareSlider lays two components over each other and lets the user drag a divider across them: \"before\" shows to the left of it, \"after\" to the right. It suits two renderings of the same thing - an image before and after an edit, two versions of a chart, light and dark."),
                        TextBlock("Click anywhere to jump the divider, or drag it. To a screen reader the divider is a slider; it moves with the arrow keys (Shift for bigger steps), Home and End. The \"after\" component sets the size, so the two should be the same shape.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("An edit"),
                        landscape,
                        HStack().AlignItemsCenter().Gap(8.px()).PT(8).Children(TextBlock("Divider at"), position, Button("Reset").OnClick(() => landscape.SetPosition(50))),
                        SampleSubTitle("Any component: light and dark"),
                        themes)).SetTitle("Usage")))
               .SeeAlso(typeof(CarouselSample), typeof(SliderSample), typeof(CodeDiffSample), typeof(MicroInteractionsSample));
        }

        private static IComponent Landscape(bool colour)
        {
            var sky   = colour ? "#7cc4f5" : "#c9c9c9";
            var sun   = colour ? "#ffcf4a" : "#efefef";
            var far   = colour ? "#5a7fa8" : "#8c8c8c";
            var near  = colour ? "#2f6b4f" : "#5c5c5c";
            var lake  = colour ? "#3d8fd1" : "#a3a3a3";

            var div = Div(Att());
            div.innerHTML = "<svg viewBox=\"0 0 640 300\" preserveAspectRatio=\"xMidYMid slice\" style=\"display:block;width:100%;height:auto\" aria-hidden=\"true\">"
                          + $"<rect width=\"640\" height=\"300\" fill=\"{sky}\"/>"
                          + $"<circle cx=\"470\" cy=\"90\" r=\"38\" fill=\"{sun}\"/>"
                          + $"<path d=\"M0 210 L120 110 L210 180 L320 80 L450 190 L540 130 L640 200 L640 300 L0 300Z\" fill=\"{far}\"/>"
                          + $"<path d=\"M0 250 L90 200 L190 240 L300 190 L420 245 L520 210 L640 250 L640 300 L0 300Z\" fill=\"{near}\"/>"
                          + $"<rect y=\"268\" width=\"640\" height=\"32\" fill=\"{lake}\"/>"
                          + "</svg>";
            return Raw(div);
        }

        private static IComponent Mock(bool dark)
        {
            var bg   = dark ? "#16181d" : "#ffffff";
            var fg   = dark ? "#e8eaed" : "#1b1d22";
            var line = dark ? "#2b2f37" : "#e6e8ec";

            return VStack().Gap(10.px()).Padding(20.px()).PT(44).Style(s => { s.background = bg; s.color = fg; }).Children(
                TextBlock("Workspace").SemiBold().Foreground(fg),
                TextBlock("Appearance settings").Small().Foreground(dark ? "#9aa0aa" : "#6b7280"),
                Raw(Div(Att(styles: s => { s.height = "56px"; s.borderRadius = "8px"; s.background = line; }))),
                HStack().Gap(10.px()).Children(
                    Raw(Div(Att(styles: s => { s.height = "32px"; s.flexGrow = "1"; s.borderRadius = "6px"; s.background = line; }))),
                    Raw(Div(Att(styles: s => { s.height = "32px"; s.width = "96px"; s.borderRadius = "6px"; s.background = "#248efa"; })))));
        }

        public HTMLElement Render() => _content.Render();
    }
}
