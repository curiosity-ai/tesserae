using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 0, Icon = LineAwesome.Tag)]
    public class TippySample : IComponent, ISample
    {
        private readonly IComponent content;

        public TippySample()
        {
            var stack = SectionStack();
            var countSlider = Slider(5, 0, 10, 1);

            content = SectionStack()
               .Title(SampleHeader(nameof(TippySample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Use .Tooltip(...) to add tooltips to your components, or Tippy.ShowFor(...) to show them on demand.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    VStack().Children(
                        Button("Hover me").W(200).Tooltip("This is a simple text tooltip"),
                        Button("Animated Tooltip").W(200).Tooltip("This is a simple text tooltip with animations", TooltipAnimation.ShiftAway),
                        Button("Interactive Tooltip").W(200).Tooltip(Button("Click me").OnClick(() => Toast().Success("You clicked!")), interactive: true),
                        Button("Nested Tooltips").W(200).Tooltip(Button("Click me").OnClick((b1, _) => Tippy.ShowFor(b1, Button("Click me").OnClick(() => Toast().Success("You clicked!")), out var _)), interactive: true)
                    )));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}