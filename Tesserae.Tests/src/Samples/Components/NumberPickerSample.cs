using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.InputNumeric)]
    public class NumberPickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public NumberPickerSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(NumberPickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The NumberPicker provides an input field specifically for numeric values, leveraging the browser's native number input widget."),
                    TextBlock("It supports constraints like minimum and maximum values, as well as step increments for easier value adjustment.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use the NumberPicker whenever a precise numeric input is required. Set appropriate 'min', 'max', and 'step' values to guide the user. If the range of numbers is small and discrete, consider using a Slider or ChoiceGroup instead. Use validation to ensure the entered number meets specific criteria (e.g., must be even or positive).")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic NumberPickers"),
                    VStack().Children(
                        Label("Standard").SetContent(NumberPicker()),
                        Label("Initial value (42)").SetContent(NumberPicker(42)),
                        Label("Disabled").Disabled().SetContent(NumberPicker().Disabled())
                    ),
                    SampleSubTitle("Constraints"),
                    VStack().Children(
                        Label("Between 0 and 100").SetContent(NumberPicker().SetMin(0).SetMax(100)),
                        Label("Step increment of 5").SetContent(NumberPicker().SetStep(5))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Must be an even number").SetContent(NumberPicker().Validation(np => np.Value % 2 == 0 ? null : "Please choose an even value")),
                        Label("Required field").SetContent(NumberPicker().Required())
                    ),
                    SampleSubTitle("Event Handling"),
                    NumberPicker().OnChange((s, e) => Toast().Information($"Value changed to: {s.Value}"))
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
