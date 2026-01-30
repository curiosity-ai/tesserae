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
            var saveButton = SaveButton().Pending().OnClick(async () => {
                // Demo logic inside the click
            });

            // For manual state control
            var manualButton = SaveButton().NothingToSave();

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
                            Button("Set Nothing to Save"  ).OnClick(() => manualButton.NothingToSave()),
                            Button("Set Pending"          ).OnClick(() => manualButton.Pending()),
                            Button("Set Verifying"        ).OnClick(() => manualButton.Verifying()),
                            Button("Set Saving"           ).OnClick(() => manualButton.Saving()),
                            Button("Set Saved"            ).OnClick(() => manualButton.Saved()),
                            Button("Set Error"            ).OnClick(() => manualButton.Error("Validation failed!"))
                        ).Gap(8.px())
                    ).Gap(16.px()),

                    SampleSubTitle("Live Demo"),
                    TextBlock("Click the button below to simulate a save operation."),
                    saveButton.OnClick(async () => {
                        saveButton.Verifying();
                        await Task.Delay(1000);
                        saveButton.Saving();
                        await Task.Delay(2000);
                        saveButton.Saved();
                        await Task.Delay(2000);
                        saveButton.NothingToSave();
                        await Task.Delay(2000);
                        saveButton.Pending();
                    })
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
