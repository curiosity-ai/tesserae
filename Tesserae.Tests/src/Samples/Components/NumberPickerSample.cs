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
                    TextBlock("The NumberPicker allows users to pick a number from a native browser widget."), Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/number", "Please see here for further information.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Basic NumberPicker").Medium(),
                    Stack().Width(40.percent()).Children(
                        Label("Standard").SetContent(NumberPicker()),
                        Label("With default initial value of 2").SetContent(NumberPicker(2)),
                        Label("With step increment of 10").SetContent(NumberPicker().SetStep(10)),
                        Label($"With max of 10").SetContent(NumberPicker().SetMax(10)),
                        Label($"With min of 5").SetContent(NumberPicker(8).SetMin(5)),
                        Label("Disabled").Disabled().SetContent(NumberPicker().Disabled()),
                        Label("Required").Required().SetContent(NumberPicker()), NumberPicker().Required(),
                        Label("With error message").SetContent(NumberPicker().Error("Error message").IsInvalid()),
                        Label("With validation").SetContent(NumberPicker().Validation(dateTimePicker => dateTimePicker.Value % 2 == 0 ? null : "Please choose an even value")))));
        }

        public HTMLElement Render() => _content.Render();
    }
}