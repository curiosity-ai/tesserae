using System;
using Tesserae.Components;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class DateTimePickerSample : IComponent
    {
        private readonly IComponent _content;

        public DateTimePickerSample()
        {
            var from = DateTime.Now.AddDays(-7);
            var to   = DateTime.Now.AddDays(7);
            _content = SectionStack()
            .Title(SampleHeader(nameof(DateTimePickerSample)))
            .Section(Stack().Children(
                SampleTitle("Overview"),
                TextBlock("The DateTimePicker allows users to pick a datetime from a native browser widget."), Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/datetime-local", "Please see here for further information.")))
            .Section(Stack().Children(
                SampleTitle("Usage"),
                TextBlock("Basic DateTimePicker").Medium(),
                Stack().Width(40.percent()).Children(
                    Label("Standard").SetContent(DateTimePicker()),
                    Label("With default day of two days in the future").SetContent(DateTimePicker(DateTime.Now.AddDays(2))),
                    Label("With step increment of 10").SetContent(DateTimePicker().SetStep(10)),
                    Label($"With max of {to.ToShortDateString()}").SetContent(DateTimePicker().SetMax(to)),
                    Label($"With min of {from.ToShortDateString()}").SetContent(DateTimePicker().SetMin(from)),
                    Label("Disabled").Disabled().SetContent(DateTimePicker().Disabled()),
                    Label("Required").Required().SetContent(DateTimePicker()), DateTimePicker().Required(),
                    Label("With error message").SetContent(DateTimePicker().Error("Error message").IsInvalid())),
                    Label("With validation").SetContent(DateTimePicker().Validation(dateTimePicker => dateTimePicker.DateTime <= DateTime.Now.AddMonths(2) ? null : "Please choose a date less than 2 months in the future")),
                    Label("With validation on type - not in the future").SetContent(DateTimePicker().Validation(Validation.NotInTheFuture)),
                    Label("With validation on type - not in the past").SetContent(DateTimePicker().Validation(Validation.NotInThePast)),
                    Label($"With validation on type - between {from.ToShortDateString()} and {to.ToShortDateString()}").SetContent(DateTimePicker().Validation(dateTimePicker => Validation.BetweenRange(dateTimePicker, from, to)))));
        }

        public HTMLElement Render() => _content.Render();
    }
}
