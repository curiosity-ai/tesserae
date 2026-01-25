using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.Edit)]
    public class EditableLabelSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public EditableLabelSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(EditableLabelSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("EditableLabels and EditableAreas allow users to view content as standard text and switch to an editing mode (input or textarea) upon interaction."),
                    TextBlock("They are useful for 'in-place' editing where you want to keep the UI clean but allow users to quickly modify specific fields without navigating to a separate form.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use EditableLabels for short, single-line content like titles or names. Use EditableAreas for longer, multi-line content like descriptions. Always provide an OnSave() callback to persist the changes. Ensure the interaction to trigger editing is clear—typically by showing an edit icon on hover or using a distinct visual style. Consider using validation to ensure the entered data meets your requirements.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Editable Labels"),
                    VStack().Children(
                        EditableLabel("Click to edit this text"),
                        EditableLabel("Large and Bold Title").Large().Bold(),
                        EditableLabel("Pre-configured font size").MediumPlus()
                    ),
                    SampleSubTitle("Editable Area"),
                    TextBlock("For multi-line text input:"),
                    EditableArea("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Click here to edit the entire block of text.").Width(400.px()),
                    SampleSubTitle("Events and Validation"),
                    VStack().Children(
                        EditableLabel("Change me and check the toast")
                           .OnSave((s, text) => { Toast().Success($"Saved: {text}"); return true; }),
                        Label("Required Field").Required().SetContent(EditableLabel("Can't be empty"))
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
