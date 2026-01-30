using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 99, Icon = UIcons.Bell)]
    public class SavingToastSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SavingToastSample()
        {
            var savingToast = SavingToast("Initial saving message...");

            _content = SectionStack()
               .Title(SampleHeader(nameof(SavingToastSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The SavingToast component helps visualising the state of a saving operation (Saving, Saved, Error) with appropriate icons and colors."),
                    savingToast
               ))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    HStack().Children(
                        Button("Trigger Saving").OnClick(() => savingToast.Saving("Saving data...")),
                        Button("Trigger Saved").OnClick(() => savingToast.Saved("Data saved successfully!")),
                        Button("Trigger Error").OnClick(() => savingToast.Error("Could not save data."))
                    ).Style(s => s.gap = "8px"),

                    SampleSubTitle("Live Demo"),
                    Button("Simulate Save Process").OnClick(async () => {
                        savingToast.Saving("Starting save...");
                        await Task.Delay(2000);
                        savingToast.Saved("All done!");
                    })
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
