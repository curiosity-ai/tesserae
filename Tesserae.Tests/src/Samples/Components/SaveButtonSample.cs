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
            var saveButton = SaveButton().Pending().OnClickSpinWhile(async () => {
                // Demo logic inside the click
            });

            // For manual state control
            var manualButton = SaveButton().NothingToSave();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SaveButtonSample), UIcons.Disk, "A button specialized for save operations")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("The SaveButton component is a wrapper around a Button that manages common saving states: Pending, Verifying, Saving, Saved, and Error.")
               )).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
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
                    saveButton.OnClickSpinWhile(async () => {
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
                )).SetTitle("Manual State Control")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("This SaveButton has a hover text configured. Hover over it when it is in Pending state."),
                    SaveButton().Configure(save: "Disabled", saveHover: "Enable Now!", saveIcon: UIcons.ToggleOff   , saveHoverIcon: UIcons.ToggleOn).Pending()
               )).SetTitle("Hover State")))
               .FlatSection(Stack().Children(
                   Card(VStack().WS().Children(
                   TextBlock("This SaveButton text can be updated dynamically."),
                   DynamicTextUpdateSample()
              )).SetTitle("Dynamic Text Update")))
               .FlatSection(Stack().Children(
                   Card(VStack().WS().Children(
                   TextBlock("This button verifies using a custom async task."),
                   GetVerifyingWhileButton()
               )).SetTitle("Verifying While")));
        }

        private IComponent GetVerifyingWhileButton()
        {
            var btn = SaveButton().Pending();
            btn.OnClickSpinWhile(async () => {
                await btn.VerifyingWhile(async () => {
                    await Task.Delay(2000);
                    return SaveButton.State.PendingSave;
                });
            });
            return btn;
        }

        private IComponent DynamicTextUpdateSample()
        {
            var btn = SaveButton().Pending();
            return HStack().Children(
                btn,
                Stack().Children(
                    Button("Update Save Text").OnClick(() => btn.Configure(save: "New Save Text")),
                    Button("Update Hover Text").OnClick(() => btn.Configure(saveHover: "New Hover Text"))
                ).Gap(8.px())
            ).Gap(16.px());
        }

        public HTMLElement Render() => _content.Render();
    }
}
