using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 45, Icon = UIcons.CalendarDays, Description = "Daily activity as a month or a year of squares")]
    public class ActivityCalendarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // Fixed seed: a year of fake activity has to render the same on every run, so a diff of the gallery only
        // shows what a change actually did.
        private readonly SampleRandom _rng = new SampleRandom(36_524);

        public ActivityCalendarSample()
        {
            var today    = SampleDate.Today;
            var spend    = FakeDays(today, 365, burstiness: 7, quietShare: 0.45, scale: 0.000004);
            var requests = FakeDays(today, 365, burstiness: 4, quietShare: 0.25, scale: 3, integral: true);

            // The year view, as a dashboard shows it: a title, the summary row, and a picker of what is drawn. Picking
            // another metric hands the same calendar new data, which re-colours the squares in place.
            var year = ActivityCalendar()
               .LastDays(365)
               .Today(today)
               .Data(spend)
               .Scale(ActivityCalendarScale.Log10)
               .FormatValue(v => "$" + FormatSmall(v))
               .Summary()
               .WS();

            var metric = Dropdown().Items(
                DropdownItem("Spend").Selected().OnSelected(_ => year.Data(spend).FormatValue(v => "$" + FormatSmall(v))),
                DropdownItem("Requests").OnSelected(_ => year.Data(requests).FormatValue(v => v.ToString("n0"))));

            year.Title("Activity", "One square per day. Hover a day for its value; the legend's squares show the range each colour covers.")
                .Commands(metric);

            // One skewed data set under every scale: a few heavy days next to many light ones is where they differ.
            var skewed = FakeDays(today, 182, burstiness: 9, quietShare: 0.3, scale: 1, integral: true);
            var scaled = ActivityCalendar().LastDays(182).Today(today).Data(skewed).Scale(ActivityCalendarScale.Log2).WS();

            var scales = ChoiceGroup().Horizontal().Choices(
                Choice("Linear"),
                Choice("Sqrt"),
                Choice("Log2").Selected(),
                Choice("Log10"),
                Choice("Quantile")
            ).OnChange((s, e) => scaled.Scale(ParseScale(s.SelectedOption.Text)));

            // The month view: weeks as rows, a larger square with the day printed in it.
            var month = ActivityCalendar()
               .Month(today.Year, today.Month)
               .Today(today)
               .Vertical()
               .FirstDayOfWeek(DayOfWeek.Monday)
               .ShowDayNumbers()
               .CellSize(40)
               .Data(requests)
               .Scale(ActivityCalendarScale.Quantile)
               .FormatValue(v => v.ToString("n0") + " requests");

            // Clicking a day: the handler gets the date and its value, and Selected() marks it.
            var clickable = ActivityCalendar().LastDays(120).Today(today).Data(requests).WS();
            clickable.OnDayClick(day =>
            {
                clickable.Selected(day.Date);
                Toast().Information(day.HasValue ? $"{day.Value:n0} requests on {day.Date:MMM d, yyyy}" : $"Nothing recorded on {day.Date:MMM d, yyyy}");
            });

            var calendarYear = ActivityCalendar()
               .Year(today.Year)
               .Today(today)
               .FirstDayOfWeek(DayOfWeek.Monday)
               .Data(requests)
               .Color(Theme.Colors.Green600)
               .Summary()
               .Tooltip(day => VStack().Children(
                    TextBlock(day.Date.ToString("dddd, MMM d")).SemiBold(),
                    TextBlock(day.HasValue ? $"{day.Value:n0} requests" : "No requests").Small().Secondary()))
               .WS();

            var palette = ActivityCalendar()
               .LastDays(120)
               .Today(today)
               .Data(skewed)
               .Thresholds(1, 10, 50, 200)
               .Palette(Theme.Colors.Yellow300, Theme.Colors.Orange400, Theme.Colors.Red500, Theme.Colors.Red800)
               .WS();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ActivityCalendarSample), UIcons.CalendarDays, "Daily activity as a month or a year of squares")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ActivityCalendar draws one square per day, coloured by how much happened on it, with month and weekday labels and a \"Less … More\" legend. It covers anything from a single month to a year and more, laid out as columns of weeks (the year view) or as rows of weeks (a wall-calendar month)."),
                        TextBlock("Every day has a tooltip, and the calendar draws it itself: one element is shared by every calendar on the page and moved to the day under the pointer, so a year of squares costs no popover objects. The legend's squares show the range of values each colour stands for."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Pick the scale for the data. Linear works when the days are of a similar size; when a few days dwarf the rest, Log10 (orders of magnitude) or Log2 (doublings) keep the small days visible, and Quantile gives each colour the same number of days. Pin a common Max(...) when two calendars sit side by side, so the same value gets the same colour in both."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("The last 365 days, with a summary and a metric picker"),
                        year,
                        SampleSubTitle("Scales: the same half-year of skewed data"),
                        scales,
                        scaled,
                        SampleSubTitle("A month, as a wall calendar"),
                        month,
                        SampleSubTitle("Click a day"),
                        clickable,
                        SampleSubTitle("A calendar year, with the days still to come, a colour and a component tooltip"),
                        calendarYear,
                        SampleSubTitle("Explicit thresholds and a palette per level"),
                        palette
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(UptimeSample), typeof(ChartsSample), typeof(SparklineSample), typeof(MetricSample));
        }

        private List<(DateTime date, double value)> FakeDays(
            DateTime today,
            int      days,
            double   burstiness,
            double   quietShare,
            double   scale,
            bool     integral = false)
        {
            var list = new List<(DateTime, double)>();

            for (int i = days - 1; i >= 0; i--)
            {
                var date    = today.AddDays(-i);
                var weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                var quiet   = _rng.NextDouble() < (weekend ? quietShare * 1.8 : quietShare);

                if (quiet) continue;

                var value = Math.Exp(_rng.NextDouble() * burstiness) * scale;

                list.Add((date, integral ? Math.Max(1, Math.Round(value)) : Math.Round(value * 1e6) / 1e6));
            }

            return list;
        }

        private static ActivityCalendarScale ParseScale(string name)
        {
            switch (name)
            {
                case "Sqrt":     return ActivityCalendarScale.Sqrt;
                case "Log2":     return ActivityCalendarScale.Log2;
                case "Log10":    return ActivityCalendarScale.Log10;
                case "Quantile": return ActivityCalendarScale.Quantile;
                default:         return ActivityCalendarScale.Linear;
            }
        }

        private static string FormatSmall(double v) => v >= 1 ? v.ToString("n2") : Transpose.Script.Write<string>("String(Number({0}.toPrecision(3)))", v);

        public HTMLElement Render() => _content.Render();
    }
}
