using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 50, Icon = UIcons.HeartRate)]
    public class UptimeSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // Fixed seed: the fake 90 days of history has to render the same on every run, so a diff of
        // the gallery only shows what a change actually did.
        private readonly SampleRandom _rng = new SampleRandom(90_210);

        public UptimeSample()
        {
            var barsItems = new List<(UptimeStatus, IComponent)>();
            for (int i = 0; i < 90; i++)
            {
                var status = GetRandomStatus();
                barsItems.Add((status, GetTooltip(status, i)));
            }

            _content = SectionStack().Secondary()
                .Title(TextBlock("Uptime").XLarge().Bold())
                .FlatSection(
                    Card(Stack().Children(
                    TextBlock("Displays system status over time using colored segments and month grids.").Medium()
                    )).SetTitle("Overview"))
                .FlatSection(Card(UptimeBars().Items(barsItems)).SetTitle("Last 90 days uptime"))
                .FlatSection(Card(UptimeBars().Compact().Items(barsItems)).SetTitle("Service Uptime (compact view)"))
                .FlatSection(Card(Grid(1.fr(), 1.fr(), 1.fr()).Gap(16.px()).Children(
                            UptimeCalendar("July 2024", "99.8%").Items(GetCalendarItems(90)),
                            UptimeCalendar("August 2024", "98.1%").Items(GetCalendarItems(60)),
                            UptimeCalendar("September 2024", "100%").Items(GetCalendarItems(30))
                        )).SetTitle("Service Uptime History"))
                .SeeAlso(typeof(ContributionBarSample), typeof(SparklineSample), typeof(ChartsSample), typeof(TimelineSample));
        }

        private IEnumerable<(UptimeStatus, IComponent)> GetCalendarItems(int startDaysAgo)
        {
            var calItems = new List<(UptimeStatus, IComponent)>();
            for (int i = 0; i < 30; i++)
            {
                var status = GetRandomStatus();
                calItems.Add((status, GetTooltip(status, startDaysAgo - i)));
            }
            // pad the rest of the calendar month to show empty spaces
            for (int i = 0; i < 5; i++)
            {
                calItems.Add((UptimeStatus.Future, null));
            }
            return calItems;
        }

        private UptimeStatus GetRandomStatus()
        {
            var r = _rng.NextDouble();
            if (r > 0.95) return UptimeStatus.Major;
            if (r > 0.90) return UptimeStatus.Minor;
            if (r > 0.85) return UptimeStatus.Maintenance;
            return UptimeStatus.Operational;
        }

        private IComponent GetTooltip(UptimeStatus status, int daysAgo)
        {
            var date = DateTime.Today.AddDays(-daysAgo).ToShortDateString();
            // Wrap in a div and add a CSS class to ensure the tooltip styling matches the dark theme properly
            return Raw(
                Div(Att("tss-uptime-tooltip-content"),
                    Stack().Children(
                        TextBlock(date).SemiBold(),
                        TextBlock(status.ToString()).Small()
                    ).Render()
                )
            );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
