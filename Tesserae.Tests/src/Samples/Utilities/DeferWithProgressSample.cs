using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 10, Icon = UIcons.Spinner)]
    public class DeferWithProgressSample : IComponent, ISample
    {
        private readonly IComponent content;

        public DeferWithProgressSample()
        {
            var container = VStack();

            content = SectionStack()
               .Title(SampleHeader(nameof(DeferWithProgressSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("DeferWithProgress extends Defer by providing a way to report progress during the async operation. This is useful for long-running tasks where you want to show a progress bar or status updates."),
                    SampleTitle("Usage"),
                    TextBlock("Click the button below to start a simulated long-running task with progress reporting."),
                    Button("Start Task").Primary().OnClick(() =>
                    {
                        container.Clear();
                        container.Add(
                            DeferWithProgress(async (reportProgress) =>
                            {
                                for (int i = 0; i <= 100; i+=10)
                                {
                                    reportProgress(i / 100f, $"Processing step {i/10} of 10...");
                                    await Task.Delay(500);
                                }
                                return TextBlock("Task Completed Successfully!").Success().SemiBold().P(10);
                            }) // Using default loadMessageGenerator
                        );
                    }),
                    container
                ));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
