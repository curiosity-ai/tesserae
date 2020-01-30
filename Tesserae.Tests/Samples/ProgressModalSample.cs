using System;
using System.Threading.Tasks;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ProgressModalSample : IComponent
    {
        private IComponent _content;

        public ProgressModalSample()
        {
            var modal = ProgressModal().Title("Lorem Ipsum");

            float progress = 0;

            void ProgressFrame(object a)
            {
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
                progress = 0;
                modal.Message("Preparing to process...").ProgressSpin().Show();
                await Task.Delay(1500);
                window.setTimeout(ProgressFrame, 16);
            }

            _content = SectionStack()
                .Title(TextBlock("Progress Modal").XLarge().Bold())
                .Section(Stack().Children(
                    TextBlock("Overview").MediumPlus(),
                    TextBlock(
                        "TODO"))
                )
                .Section(Stack().Children(
                    TextBlock("Best Practices").MediumPlus(),
                    Stack().Horizontal().Children(
                        Stack().Children(
                            TextBlock("Do").Medium(),
                            TextBlock("TODO")
                        ),
                        Stack().Children(
                            TextBlock("Don't").Medium(),
                            TextBlock("TODO")
                        ))
                ))
                .Section(
                    Stack().Width(400, Unit.Pixels).Children(
                        TextBlock("Usage").MediumPlus(),
                        Button("Open Modal").OnClick((s, e) => PlayModal().FireAndForget())
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
