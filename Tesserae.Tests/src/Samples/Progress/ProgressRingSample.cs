using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Progress", Order = 10, Icon = UIcons.Circle)]
    public class ProgressRingSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ProgressRingSample()
        {
            var ring75 = ProgressRing(64, 6).Progress(75, 100).Label("75%");
            var ring50 = ProgressRing(64, 6).Progress(50, 100).Label("50%");
            var ring25 = ProgressRing(64, 6).Progress(25, 100).Label("25%");

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ProgressRingSample), UIcons.Circle, "A circular progress indicator")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ProgressRing displays progress in a circular donut style. Use it alongside a metric value, in a dashboard card header, or to track quota/usage."),
                    TextBlock("It supports determinate values (0–100), an indeterminate spinning state, and an optional text label in the centre."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use ProgressRing when horizontal space is limited and a compact indicator is preferred. For linear progress (e.g. file upload bars), use ProgressIndicator instead. Always include a textual label or aria-label so screen readers can convey the value."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Determinate Rings"),
                    HStack().Gap(24.px()).AlignItems(ItemAlign.End).Children(
                        VStack().AlignItems(ItemAlign.Center).Children(ring25, TextBlock("25%").Small().MT(8)),
                        VStack().AlignItems(ItemAlign.Center).Children(ring50, TextBlock("50%").Small().MT(8)),
                        VStack().AlignItems(ItemAlign.Center).Children(ring75, TextBlock("75%").Small().MT(8)),
                        VStack().AlignItems(ItemAlign.Center).Children(ProgressRing(64, 6).Progress(100, 100).Label("100%"), TextBlock("100%").Small().MT(8))
                    ),
                    SampleSubTitle("Sizes"),
                    HStack().Gap(24.px()).AlignItems(ItemAlign.End).Children(
                        VStack().AlignItems(ItemAlign.Center).Children(ProgressRing(32, 3).Progress(60, 100),   TextBlock("Small").XSmall().MT(8)),
                        VStack().AlignItems(ItemAlign.Center).Children(ProgressRing(48, 4).Progress(60, 100),   TextBlock("Medium").XSmall().MT(8)),
                        VStack().AlignItems(ItemAlign.Center).Children(ProgressRing(64, 6).Progress(60, 100),   TextBlock("Large").XSmall().MT(8)),
                        VStack().AlignItems(ItemAlign.Center).Children(ProgressRing(96, 8).Progress(60, 100).Label("60%"), TextBlock("XLarge").XSmall().MT(8))
                    ),
                    SampleSubTitle("Indeterminate (Loading)"),
                    HStack().Gap(24.px()).AlignItems(ItemAlign.Center).Children(
                        ProgressRing(48, 4).Indeterminate(),
                        ProgressRing(64, 6).Indeterminate(),
                        ProgressRing(96, 8).Indeterminate().Label("…")
                    ),
                    SampleSubTitle("Animated Fill"),
                    Button("Start 5s countdown").Primary().Var(out var animBtn).OnClickSpinWhile(async () =>
                    {
                        animBtn.Disabled();
                        var liveRing = ProgressRing(64, 6).Label("0%");
                        var container = HStack().AlignItems(ItemAlign.Center).Gap(12.px()).Children(liveRing, TextBlock("Processing…").Small());
                        Toast().Information(container).Duration(System.TimeSpan.FromSeconds(7));
                        for (int i = 1; i <= 5; i++)
                        {
                            await Task.Delay(1000);
                            liveRing.Progress(i, 5).Label($"{i * 20}%");
                        }
                        animBtn.Disabled(false);
                    })
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
