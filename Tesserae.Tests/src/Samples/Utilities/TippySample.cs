using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 0, Icon = UIcons.CommentInfo)]
    public class TippySample : IComponent, ISample
    {
        private readonly IComponent content;

        public TippySample()
        {
            var stack       = SectionStack();
            var countSlider = Slider(5, 0, 10, 1);

            var size = new SettableObservable<int>();
            var deferedWithChangingSize = DeferSync(size, sz => Button($"Height = {sz:n0}px").H(sz)).WS();

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
                        Button("Defers on Tooltips").W(200).Tooltip(deferedWithChangingSize),
                        Button("Nested Tooltips").W(200).Tooltip(Button("Click me").OnClick((b1, _) => Tippy.ShowFor(b1, Button("Click me").OnClick(() => Toast().Success("You clicked!")), out var _)), interactive: true)
                    )));

            content.WhenMounted(() =>
            {
                var rng = new Random();
                var repeat = window.setInterval(_ =>
                {
                    size.Value = rng.Next(0, 10) * 50 + 50;
                }, 1000);
                content.WhenRemoved(() => window.clearInterval(repeat));
            });
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}