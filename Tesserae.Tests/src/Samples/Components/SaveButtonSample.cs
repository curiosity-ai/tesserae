using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.Disk)]
    public class SaveButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SaveButtonSample()
        {
            var saveButton = SaveButton().OnClick(async () => {
                // Demo logic inside the click
            });

            // For manual state control
            var manualButton = SaveButton();

            _content = SectionStack()
               .Title(SampleHeader(nameof(SaveButtonSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The SaveButton component is a wrapper around a Button that manages common saving states: Pending, Verifying, Saving, Saved, and Error.")
               ))
               .Section(Stack().Children(
                    SampleTitle("Manual State Control"),
                    HStack().Children(
                        manualButton,
                        Stack().Children(
                            Button("Set Pending").OnClick(() => manualButton.SetState(SaveButton.SaveState.SavePending)),
                            Button("Set Verifying").OnClick(() => manualButton.SetState(SaveButton.SaveState.Verifying)),
                            Button("Set Saving").OnClick(() => manualButton.SetState(SaveButton.SaveState.Saving)),
                            Button("Set Saved").OnClick(() => manualButton.SetState(SaveButton.SaveState.Saved)),
                            Button("Set Error").OnClick(() => manualButton.SetState(SaveButton.SaveState.Error, "Validation failed!"))
                        ).Style(s => s.gap = "8px")
                    ).Style(s => s.gap = "16px"),

                    SampleSubTitle("Live Demo"),
                    TextBlock("Click the button below to simulate a save operation."),
                    saveButton.OnClick(async () => {
                        saveButton.SetState(SaveButton.SaveState.Verifying);
                        await Task.Delay(1000);
                        saveButton.SetState(SaveButton.SaveState.Saving);
                        await Task.Delay(2000);
                        saveButton.SetState(SaveButton.SaveState.Saved);
                        await Task.Delay(2000);
                        saveButton.SetState(SaveButton.SaveState.SavePending);
                    })
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
