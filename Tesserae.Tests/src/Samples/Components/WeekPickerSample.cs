using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 204, Icon = UIcons.CalendarWeek)]
    public class WeekPickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public WeekPickerSample()
        {
            var minWeek = (year: DateTime.Today.Year, week: 1);
            var maxWeek = (year: DateTime.Today.Year, week: 52);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(WeekPickerSample), UIcons.CalendarWeek, "A control to select an ISO week number")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("WeekPicker lets users choose a specific ISO week within a year using the browser's native week-input widget. It surfaces the selection as a typed (year, weekNumber) tuple."),
                    TextBlock("It is well-suited for scheduling, sprint planning, payroll cycles, or any reporting context that aligns to week boundaries rather than individual days or months."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use WeekPicker when your domain explicitly talks about calendar weeks (e.g. 'Week 23 of 2025'). Avoid using it as a substitute for a date range picker — if the user ultimately needs a start and end date, use DatePicker pairs instead. Apply Min/Max constraints to prevent selection of weeks outside the valid planning horizon."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic WeekPicker"),
                    VStack().Children(
                        Label("Week 1 of This Year").SetContent(
                            new WeekPicker((DateTime.Today.Year, 1))
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Week.year}-W{s.Week.weekNumber}"))
                        ),
                        Label("No Default").SetContent(
                            new WeekPicker(null)
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Week.year}-W{s.Week.weekNumber}"))
                        ),
                        Label("Disabled").Disabled().SetContent(
                            new WeekPicker((DateTime.Today.Year, 1)).Disabled()
                        )
                    ),
                    SampleSubTitle("With Min / Max Constraints"),
                    VStack().Children(
                        Label($"Range: {minWeek.year}-W{minWeek.week} to {maxWeek.year}-W{maxWeek.week}").SetContent(
                            new WeekPicker((DateTime.Today.Year, 1))
                               .SetMin(minWeek)
                               .SetMax(maxWeek)
                               .OnChange((s, e) => Toast().Information($"Selected: {s.Week.year}-W{s.Week.weekNumber}"))
                        )
                    ),
                    SampleSubTitle("Event Handling"),
                    new WeekPicker((DateTime.Today.Year, 1))
                       .OnChange((s, e) => Toast().Information($"Week changed to {s.Week.year}-W{s.Week.weekNumber}"))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
