using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.CalendarCheck)]
    public class UptimeSample : IComponent, ISample
    {
        private readonly IComponent _content;

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
                        )).SetTitle("Service Uptime History"));
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
            var r = Math.Random();
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
                Div(_("tss-uptime-tooltip-content"),
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
