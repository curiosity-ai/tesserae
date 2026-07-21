using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 205, Icon = UIcons.Clock)]
    public class TimePickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TimePickerSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TimePickerSample), UIcons.Clock, "A control to select a time of day")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("TimePicker lets users select a time of day using the browser's native time-input widget. It exposes the selected value as a DateTimeOffset, making it straightforward to combine with a date or to compute durations."),
                    TextBlock("It is ideal for scheduling meetings, setting reminders, configuring cron-like triggers, or any feature where the user must specify a wall-clock time."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use TimePicker whenever the domain requires a specific time rather than a duration. When presenting times across time zones, always display the relevant zone label next to the control. For coarse scheduling (morning / afternoon), prefer a ChoiceGroup or Dropdown to keep the UI lightweight. Consider pairing TimePicker with a DatePicker when a full date-time is needed rather than using a separate DateTimePicker, as two focused controls can be clearer in form contexts."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic TimePicker"),
                    VStack().Children(
                        Label("No default (empty)").SetContent(
                            new TimePicker()
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Time:t}"))
                        ),
                        Label("Pre-filled with current time").SetContent(
                            new TimePicker(DateTimeOffset.Now)
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Time:t}"))
                        ),
                        Label("Disabled").Disabled().SetContent(
                            new TimePicker(DateTimeOffset.Now).Disabled()
                        )
                    ),
                    SampleSubTitle("Event Handling"),
                    new TimePicker(DateTimeOffset.Now)
                       .OnChange((s, e) => Toast().Information($"Time changed to {s.Time:T}"))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
