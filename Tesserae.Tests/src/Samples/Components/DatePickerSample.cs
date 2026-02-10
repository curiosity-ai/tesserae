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
            var to   = DateTime.Now.AddDays(7);

            _content = SectionStack()
               .Title(SampleHeader(nameof(DatePickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("DatePickers allow users to select a specific date using a browser-native date selection widget. They ensure that the input is always a valid date format."),
                    TextBlock("This component is suitable for forms requiring birthdays, appointment dates, or any date-driven data entry.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use the DatePicker when users need to enter a specific date. If you need to include time as well, use the DateTimePicker instead. Always provide min and max constraints if the acceptable date range is limited. Use clear validation messages to guide users if they select an invalid date (e.g., a date in the past when only future dates are allowed).")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic DatePicker"),
                    VStack().Children(
                        Label("Standard").SetContent(DatePicker()),
                        Label("Pre-selected Date (Next Week)").SetContent(DatePicker(DateTime.Now.AddDays(7))),
                        Label("Disabled").Disabled().SetContent(DatePicker().Disabled())
                    ),
                    SampleSubTitle("Range and Constraints"),
                    VStack().Children(
                        Label($"Limited Range (Between {from:d} and {to:d})").SetContent(DatePicker().SetMin(from).SetMax(to)),
                        Label("Step increment of 5 days").SetContent(DatePicker().SetStep(5))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Not in the future").SetContent(DatePicker().Validation(Validation.NotInTheFuture)),
                        Label("Not in the past").SetContent(DatePicker().Validation(Validation.NotInThePast)),
                        Label("Custom validation (within 2 months)").SetContent(DatePicker().Validation(dp => dp.Date <= DateTime.Now.AddMonths(2) ? null : "Please choose a date less than 2 months in the future"))
                    ),
                    SampleSubTitle("Event Handling"),
                    DatePicker().OnChange((s, e) => Toast().Information($"Selected date: {s.Date:d}"))
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
