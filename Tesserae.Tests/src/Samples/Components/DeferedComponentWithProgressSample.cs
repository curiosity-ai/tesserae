using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 99, Icon = UIcons.Refresh)]
    public class DeferedComponentWithProgressSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DeferedComponentWithProgressSample()
        {
            var trigger = new SettableObservable<int>(0);

            _content = SectionStack()
               .Title(SampleHeader(nameof(DeferedComponentWithProgressSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("DeferedComponentWithProgress allows deferred loading with progress reporting, triggered by observables."),
                    Button("Refresh").OnClick(() => trigger.Value++),
                    DeferWithProgress(trigger, async (val, progress) =>
                    {
                        progress(0, "Starting...");
                        await Task.Delay(500);
                        progress(0.3f, "Step 1 complete");
                        await Task.Delay(500);
                        progress(0.6f, "Step 2 complete");
                        await Task.Delay(500);
                        progress(0.9f, "Finalizing...");
                        await Task.Delay(500);
                        return TextBlock($"Loaded content for trigger value: {val}").Success();
                    })
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
