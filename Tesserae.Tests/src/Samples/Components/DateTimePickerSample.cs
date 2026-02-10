using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.CalendarClock)]
    public class DateTimePickerSample : IComponent, ISample
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
                    TextBlock("The DateTimePicker combines date and time selection into a single component, using the browser's native widget."),
                    TextBlock("It is ideal for scheduling events, setting deadlines, or any task where both the day and time are critical.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use the DateTimePicker when users need to specify a precise moment in time. Consider the user's timezone if the application handles users across different regions. Provide sensible defaults, such as the current time or a common starting point. Use min/max constraints to prevent invalid selections (e.g., booking an appointment in the past).")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic DateTimePicker"),
                    VStack().Children(
                        Label("Standard").SetContent(DateTimePicker()),
                        Label("Pre-selected (Now)").SetContent(DateTimePicker(DateTime.Now)),
                        Label("Disabled").Disabled().SetContent(DateTimePicker().Disabled())
                    ),
                    SampleSubTitle("Constraints"),
                    VStack().Children(
                        Label($"Range: {from:g} to {to:g}").SetContent(DateTimePicker().SetMin(from).SetMax(to)),
                        Label("10-second intervals").SetContent(DateTimePicker().SetStep(10))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Must be in the future").SetContent(DateTimePicker().Validation(Validation.NotInThePast)),
                        Label("Within next 48 hours").SetContent(DateTimePicker().Validation(dtp => dtp.DateTime <= DateTime.Now.AddHours(48) ? null : "Must be within 48 hours"))
                    ),
                    SampleSubTitle("Event Handling"),
                    DateTimePicker().OnChange((s, e) => Toast().Information($"Selected: {s.DateTime:g}"))
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
