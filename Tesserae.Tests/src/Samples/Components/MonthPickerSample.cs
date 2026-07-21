using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 203, Icon = UIcons.CalendarLines)]
    public class MonthPickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MonthPickerSample()
        {
            var minMonth = (year: DateTime.Today.Year - 1, month: 1);
            var maxMonth = (year: DateTime.Today.Year + 1, month: 12);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MonthPickerSample), UIcons.CalendarLines, "A control to select a year and month")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("MonthPicker lets users select a specific year and month using the browser's native month-input widget. It is ideal when day-level precision is unnecessary, such as selecting billing periods, report months, or subscription renewal dates."),
                    TextBlock("The component surfaces the selected value as a typed (year, month) tuple, removing the need for manual string parsing."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use MonthPicker when the unit of time is a calendar month rather than a specific day. Always pre-populate with a sensible default (e.g. the current month) so users can confirm quickly without additional input. Apply Min/Max constraints when only a limited historical or future range makes sense for your use-case."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic MonthPicker"),
                    VStack().Children(
                        Label("Current Month").SetContent(
                            new MonthPicker((DateTime.Today.Year, DateTime.Today.Month))
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Month.year}-{s.Month.month:D2}"))
                        ),
                        Label("No Default").SetContent(
                            new MonthPicker(null)
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Month.year}-{s.Month.month:D2}"))
                        ),
                        Label("Disabled").Disabled().SetContent(
                            new MonthPicker((DateTime.Today.Year, DateTime.Today.Month)).Disabled()
                        )
                    ),
                    SampleSubTitle("With Min / Max Constraints"),
                    VStack().Children(
                        Label($"Range: {minMonth.year}-{minMonth.Item2:D2} to {maxMonth.year}-{maxMonth.Item2:D2}").SetContent(
                            new MonthPicker((DateTime.Today.Year, DateTime.Today.Month))
                               .SetMin(minMonth)
                               .SetMax(maxMonth)
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Month.year}-{s.Month.month:D2}"))
                        )
                    ),
                    SampleSubTitle("Event Handling"),
                    new MonthPicker((DateTime.Today.Year, DateTime.Today.Month))
                       .OnChange((s, e) => Toast().Information($"Month changed to {s.Month.year}-{s.Month.month:D2}"))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
