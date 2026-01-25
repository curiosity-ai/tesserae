using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.Checkbox)]
    public class CheckBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CheckBoxSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(CheckBoxSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("CheckBoxes allow users to select one or more items from a set. They can also be used to turn an option on or off."),
                    TextBlock("Unlike a Toggle, which is typically used for immediate actions, a CheckBox is often used when a user needs to confirm their selection by clicking a submit button.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use CheckBoxes when users can select any number of options from a list. Clearly label each CheckBox so the user knows what they are selecting. If you have only two mutually exclusive options, consider using a ChoiceGroup (Radio buttons) or a Toggle. Don't use CheckBoxes as an on/off control for immediate actions; use a Toggle instead.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic CheckBoxes"),
                    VStack().Children(
                        CheckBox("Unchecked checkbox"),
                        CheckBox("Checked checkbox").Checked(),
                        CheckBox("Disabled checkbox").Disabled(),
                        CheckBox("Disabled checked checkbox").Checked().Disabled()),
                    SampleSubTitle("Validation and States"),
                    VStack().Children(
                        Label("Required choice").Required().SetContent(CheckBox("I agree to the terms")),
                        CheckBox("Checkbox with tooltip").Tooltip("More info here"),
                        CheckBox("Triggers event").OnChange((s, e) => Toast().Information($"Checked: {s.IsChecked}")))
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
