using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 35, Icon = UIcons.DashboardPanel, Description = "A KPI scorecard built from sparklines, deltas and target bars")]
    public class DashboardSample : IComponent, ISample
    {
        private enum Status { OnTrack, Watch, OffPace, Context }

        private sealed class KpiRow
        {
            public string  Name;
            public Status  Status;
            public string  Value;
            public string  Delta;
            public double  Change;          // signed, for the arrow
            public bool    HigherIsBetter;
            public double  RunRate;         // % of the run-rate target; ignored for Context
            public double  Start;           // where the 52-week series begins
            public double  Drift;           // its tendency, per week
            public double  Noise;
            public int     Seed;
        }

        private static readonly string ON_TRACK = Theme.Colors.Green700;
        private static readonly string WATCH    = Theme.Colors.Orange600;
        private static readonly string OFF_PACE = Theme.Colors.Red600;
        private static readonly string CONTEXT  = Theme.Colors.Neutral500;

        private readonly IComponent _content;

        public DashboardSample()
        {
            var revenue = new[]
            {
                new KpiRow { Name = "Net revenue per active customer",    Status = Status.Watch,   Value = "\u00a32,918", Delta = "\u00a331",   Change = -31, HigherIsBetter = false, RunRate =  84, Start = 50, Drift =  0.0, Noise = 6, Seed = 11 },
                new KpiRow { Name = "Refund rate, trailing four weeks",   Status = Status.OffPace, Value = "6.2%",     Delta = "0.4pp",  Change = -0.4, HigherIsBetter = false, RunRate =  93, Start = 40, Drift =  0.1, Noise = 9, Seed = 12 },
                new KpiRow { Name = "On-time delivery",                   Status = Status.Watch,   Value = "81.4%",    Delta = "2.2pp",  Change = -2.2, HigherIsBetter = true,  RunRate =  95, Start = 55, Drift =  0.0, Noise = 9, Seed = 13 },
                new KpiRow { Name = "Support tickets per 1,000 orders",   Status = Status.OnTrack, Value = "3.4",      Delta = "0.3",    Change =  0.3, HigherIsBetter = false, RunRate = 231, Start = 80, Drift = -1.2, Noise = 3, Seed = 14 },
                new KpiRow { Name = "Average basket size",                Status = Status.Context, Value = "5.1%",     Delta = "0.8pp",  Change =  0.8, HigherIsBetter = true,  RunRate =   0, Start = 55, Drift = -0.4, Noise = 6, Seed = 15 },
                new KpiRow { Name = "Marketing spend as a share of sales", Status = Status.OffPace, Value = "12.9%",    Delta = "1.7pp",  Change =  1.7, HigherIsBetter = false, RunRate =  64, Start = 25, Drift =  0.5, Noise = 7, Seed = 16 },
            };

            var growth = new[]
            {
                new KpiRow { Name = "New customers",                      Status = Status.OnTrack, Value = "1,284",    Delta = "96",     Change =  96, HigherIsBetter = true,  RunRate = 112, Start = 40, Drift =  0.6, Noise = 6, Seed = 21 },
                new KpiRow { Name = "Repeat purchase rate",               Status = Status.Watch,   Value = "38.7%",    Delta = "0.9pp",  Change = -0.9, HigherIsBetter = true, RunRate =  91, Start = 50, Drift =  0.0, Noise = 5, Seed = 22 },
                new KpiRow { Name = "Cost to acquire a customer",         Status = Status.OffPace, Value = "\u00a342.10", Delta = "\u00a32.30", Change = 2.3, HigherIsBetter = false, RunRate = 58, Start = 30, Drift =  0.4, Noise = 6, Seed = 23 },
                new KpiRow { Name = "Share of orders from the app",       Status = Status.Context, Value = "54.0%",    Delta = "0.0pp",  Change =  0,   HigherIsBetter = true, RunRate =   0, Start = 45, Drift =  0.2, Noise = 4, Seed = 24 },
            };

            _content = VStack().WS().Gap(20.px()).P(24).Children(
                Header(),
                Legend(),
                Section("Retail", Theme.Colors.Blue600, "Headline metrics \u2014 full set (58) in the deep dive", revenue),
                Section("Online", Theme.Colors.Orange600, "Headline metrics \u2014 full set (66) in the deep dive", growth)
            );
        }

        private static IComponent Header()
        {
            var tabs = HStack().Gap(8.px()).Wrap().Children(
                Chip("All regions").Pill().Filled().Primary(),
                Chip("North").Pill().Outline().Dot(Theme.Colors.Blue600),
                Chip("South").Pill().Outline().Dot(Theme.Colors.Orange600),
                Chip("Marketplace").Pill().Outline().Dot(Theme.Colors.Magenta600),
                Chip("Export").Pill().Outline().Dot(Theme.Colors.Green600));

            return VStack().Gap(6.px()).Children(
                TextBlock("WEEKLY BUSINESS REVIEW").XSmall().SemiBold().Foreground(Theme.Colors.Magenta600).Style(s => s.letterSpacing = "0.12em"),
                TextBlock("Northwind \u2014 Overview").XLarge().Bold(),
                tabs.PT(6));
        }

        private static IComponent Legend()
        {
            return Card(HStack().Gap(12.px()).Wrap().AlignItemsCenter().Children(
                HStack().Gap(8.px()).Wrap().Children(
                    Chip("On track vs run rate").Pill().Dot(ON_TRACK),
                    Chip("Watch").Pill().Dot(WATCH),
                    Chip("Off pace").Pill().Dot(OFF_PACE),
                    Chip("Context (no target)").Pill().Dot(CONTEXT)),
                TextBlock("Trend = last 52 weeks \u00b7 endpoint marks this week \u00b7 WoW = vs prior week \u00b7 colour follows the metric\u2019s own good direction").Small().Foreground(Theme.Secondary.Foreground)));
        }

        private static IComponent Section(string title, string color, string subtitle, KpiRow[] rows)
        {
            int on = 0, watch = 0, off = 0, ctx = 0;

            foreach (var row in rows)
            {
                if      (row.Status == Status.OnTrack) on++;
                else if (row.Status == Status.Watch)   watch++;
                else if (row.Status == Status.OffPace) off++;
                else                                   ctx++;
            }

            var swatch = Div(Att(""));
            swatch.style.width        = "10px";
            swatch.style.height       = "10px";
            swatch.style.borderRadius = "2px";
            swatch.style.background   = color;

            var table = DataTable()
               .Column("Metric", 3.fr())
               .Column("Trend (52wk)", 2.fr(), DataTableAlign.Center)
               .Column("This week", 1.fr(), DataTableAlign.End)
               .Column("WoW", 1.fr(), DataTableAlign.Center)
               .Column("vs run rate", 2.fr());

            foreach (var row in rows)
            {
                table.Row(StatusColor(row.Status), Cells(row));
            }

            return VStack().Gap(12.px()).Children(
                HStack().AlignItemsCenter().JustifyContent(ItemJustify.Between).Wrap().Gap(12.px()).Children(
                    VStack().Gap(2.px()).Children(
                        HStack().AlignItemsCenter().Gap(8.px()).Children(Raw(swatch), TextBlock(title).Medium().Bold()),
                        TextBlock(subtitle).Small().Foreground(Theme.Secondary.Foreground)),
                    HStack().AlignItemsCenter().Gap(8.px()).Wrap().Children(
                        Count(on,    "on track", ON_TRACK),
                        Count(watch, "watch",    WATCH),
                        Count(off,   "off pace", OFF_PACE),
                        Count(ctx,   "context",  CONTEXT),
                        Button("Deep dive \u2192").Primary())),
                table);
        }

        private static IComponent Count(int count, string label, string color)
        {
            return Chip((count < 10 ? "0" : "") + count + " " + label).Pill().Dot(color).Style(s => s.fontFamily = Theme.Fonts.Monospace);
        }

        private static IComponent[] Cells(KpiRow row)
        {
            var color    = StatusColor(row.Status);
            var series   = Series(row);
            var trend    = Sparkline(series, width: 200, height: 36, color: color).EndMarker().ReferenceLine().WS();
            var value    = TextBlock(row.Value).SmallPlus().SemiBold().Style(s => s.fontFamily = Theme.Fonts.Monospace);
            var delta    = row.Status == Status.Context
                ? TrendBadge.For(row.Change, row.Delta, row.HigherIsBetter).Neutral()
                : TrendBadge.For(row.Change, row.Delta, row.HigherIsBetter);
            var target   = row.Status == Status.Context
                ? TargetBar(0).NoValue("Context")
                : TargetBar(row.RunRate).Color(color).Label(StatusLabel(row.Status)).ValueText(Math.Round(row.RunRate) + "%");

            return new IComponent[] { TextBlock(row.Name).Small(), trend, value, delta, target };
        }

        // A seeded random walk, so the page renders the same on every run.
        private static double[] Series(KpiRow row)
        {
            var random = new SampleRandom(row.Seed);
            var data   = new double[52];
            var level  = row.Start;

            for (int i = 0; i < data.Length; i++)
            {
                level   += row.Drift + (random.NextDouble() - 0.5) * row.Noise;
                data[i]  = level;
            }

            return data;
        }

        private static string StatusColor(Status status)
        {
            switch (status)
            {
                case Status.OnTrack: return ON_TRACK;
                case Status.Watch:   return WATCH;
                case Status.OffPace: return OFF_PACE;
                default:             return CONTEXT;
            }
        }

        private static string StatusLabel(Status status)
        {
            switch (status)
            {
                case Status.OnTrack: return "On track";
                case Status.Watch:   return "Watch";
                case Status.OffPace: return "Off pace";
                default:             return "Context";
            }
        }

        public HTMLElement Render() => _content.Render();
    }
}
