using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.Calendar)]
    public class DatePickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DatePickerSample()
        {
            var from = DateTime.Now.AddDays(-7);
            var to = DateTime.Now.AddDays(7);

            _content = SectionStack()
               .Title(SampleHeader(nameof(DatePickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The DatePicker allows users to pick a datetime from a native browser widget."), Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/datetime-local", "Please see here for further information.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Basic DatePicker").Medium(),
                    Stack().Width(40.percent()).Children(
                        Label("Standard").SetContent(DatePicker()),
                        Label("With default day of two days in the future").SetContent(DatePicker(DateTime.Now.AddDays(2))),
                        Label("With step increment of 10").SetContent(DatePicker().SetStep(10)),
                        Label($"With max of {to.ToString("D")}").SetContent(DatePicker().SetMax(to)),
                        Label($"With min of {from.ToString("D")}").SetContent(DatePicker().SetMin(from)),
                        Label("Disabled").Disabled().SetContent(DatePicker().Disabled()),
                        Label("Required").Required().SetContent(DatePicker()), DatePicker().Required(),
                        Label("With error message").SetContent(DatePicker().Error("Error message").IsInvalid()),
                        Label("With validation").SetContent(DatePicker().Validation(dateTimePicker => dateTimePicker.Date <= DateTime.Now.AddMonths(2) ? null : "Please choose a date less than 2 months in the future")),
                        Label("With validation on type - not in the future").SetContent(DatePicker().Validation(Validation.NotInTheFuture)),
                        Label("With validation on type - not in the past").SetContent(DatePicker().Validation(Validation.NotInThePast)),
                        Label($"With validation on type - between {from.ToString("D")} and {to.ToString("D")}").SetContent(DatePicker().Validation(dateTimePicker => Validation.BetweenRange(dateTimePicker, from, to))))));
        }

        public HTMLElement Render() => _content.Render();
    }
}