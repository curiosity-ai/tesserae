using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.Bell)]
    public class SavingToastSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SavingToastSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(SavingToastSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The SavingToast component helps viewing the state of a saving operation (Saving, Saved, Error) with appropriate icons and colors.")
               ))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    HStack().Children(
                        Button("Trigger Saving").OnClick(() => SavingToast().Saving("Saving data...")),
                        Button("Trigger Saved" ).OnClick(() => SavingToast().Saved("Data saved successfully!")),
                        Button("Trigger Error" ).OnClick(() => SavingToast().Error("Could not save data.")),
                        Button("Many Toasts").OnClickSpinWhile(() => ShowMany())
                    ).Gap(8.px()),

                    SampleSubTitle("Live Demo"),
                    Button("Simulate Save Process").OnClick(async () => {
                        var savingToast = SavingToast();
                        savingToast.Saving("Starting save...");
                        await Task.Delay(2000);
                        savingToast.Saved("All done!");
                    })
                ));
        }

        private async Task ShowMany()
        {
            var toast1 = SavingToast();
            var toast2 = SavingToast();
            var toast3 = SavingToast();
            toast1.Saving("Starting save...");
            toast2.Saving("Starting save...");
            toast3.Saving("Starting save...");
            await Task.Delay(2000);
            toast1.Saved("All done!");
            await Task.Delay(2000);
            toast2.Saved("All done!");
            await Task.Delay(2000);
            toast3.Saved("All done!");
        }

        public HTMLElement Render() => _content.Render();
    }
}
