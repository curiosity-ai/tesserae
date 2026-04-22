using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Threading;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Progress", Order = 20, Icon = UIcons.WindowMaximize)]
    public class ProgressModalSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ProgressModalSample()
        {
            ProgressModal modal;

            CancellationTokenSource cts;

            float progress = 0;

            void ProgressFrame(object a)
            {
                if (cts.IsCancellationRequested)
                {
                    modal.ProgressSpin().Message("Cancelling...");
                    Task.Delay(2000).ContinueWith(_ => modal.Hide()).FireAndForget();
                    return;
                }
                progress++;

                if (progress < 100)
                {
                    modal.Message($"Processing {progress}%").Progress(progress);
                    window.setTimeout(ProgressFrame, 16);
                }
                else
                {
                    modal.Message("Finishing...").ProgressIndeterminated();
                    Task.Delay(5000).ContinueWith(_ => modal.Hide()).FireAndForget();
                }
            }

            async Task PlayModal()
            {
                modal = ProgressModal().Title("Lorem Ipsum");
                cts   = new CancellationTokenSource();

                modal.WithCancel((b) =>
                {
                    b.Disabled();
                    cts.Cancel();
                });
                progress = 0;
                modal.Message("Preparing to process...").ProgressSpin().Show();
                await Task.Delay(1500);
                window.setTimeout(ProgressFrame, 16);
            }

            _content = SectionStack()
               .Title(SampleHeader(nameof(ProgressModalSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ProgressModal is a specialized modal overlay that combines a title, a message, and a progress indicator. It is used for long-running operations where it is important to block other user interactions until the task is complete, while keeping the user informed of the progress.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use ProgressModal only for operations that truly require the user's focus and shouldn't be interrupted. Ensure that the message provides clear context for what is being processed. Always provide a way to cancel the operation if possible. For background tasks that don't need to block the entire UI, consider using an in-place ProgressIndicator or Spinner instead.")))
               .Section(
                    Stack().Width(400.px()).Children(
                        SampleTitle("Usage"),
                        Button("Open Modal").OnClick((s, e) => PlayModal().FireAndForget())
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}