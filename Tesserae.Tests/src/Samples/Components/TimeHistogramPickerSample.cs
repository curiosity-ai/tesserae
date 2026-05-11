using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.ChartHistogram)]
    public class TimeHistogramPickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TimeHistogramPickerSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TimeHistogramPickerSample), UIcons.ChartHistogram, "A histogram control for selecting a time range")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("The TimeHistogramPicker turns a DateTime array into adaptive buckets and lets users narrow the selected range from either side."),
                        TextBlock("It sorts a private copy of the input values, so callers can pass unsorted data without changing their source array."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Use the picker when users need to filter a time-based dataset while keeping the shape and density of the data visible. The component adapts from second-level ranges through multi-year spans and is designed for large arrays."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Three Values"),
                        PickerDemo(GetSmallValues(), 12),
                        SampleSubTitle("Dense Minute and Hour Data"),
                        PickerDemo(GetDenseValues(), 64),
                        SampleSubTitle("Fine-grained Seconds"),
                        PickerDemo(GetFineGrainedValues(), 360, date => date.ToString("HH:mm:ss"), showBucketTooltipOnHover: true),
                        SampleSubTitle("Custom Time Rendering"),
                        PickerDemo(GetDenseValues(), 48, date => date.ToString("MMM d, HH:mm")),
                        SampleSubTitle("Sparse Multi-year Data"),
                        PickerDemo(GetMultiYearValues(), 48),
                        SampleSubTitle("Gapped Clusters with Uneven Groups"),
                        PickerDemo(GetGappedClusterValues(), 80),
                        SampleSubTitle("Large Dataset (100,000 Values)"),
                        PickerDemo(GetLargeValues(), 80)
                    )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Daily Buckets from Backend"),
                        BucketPickerDemo(GetDailyBackendBuckets()),
                        SampleSubTitle("Long-range Aggregated Buckets from Backend"),
                        BucketPickerDemo(GetLongRangeBackendBuckets())
                    )).SetTitle("Backend Precomputed Buckets")));
        }

        public HTMLElement Render() => _content.Render();

        private static IComponent PickerDemo(DateTime[] values, int maxBuckets, Func<DateTime, string> renderTime = null, bool showBucketTooltipOnHover = true)
        {
            var selection = new SettableObservable<string>();
            var picker = TimeHistogramPicker(values, maxBuckets)
               .WithCustomTimeRender(renderTime)
               .ShowBucketTooltipOnHover(showBucketTooltipOnHover)
               .OnRangeChanged((from, to, count) => selection.Value = FormatSelection(from, to, count, renderTime));

            selection.Value = FormatSelection(picker.SelectedFrom, picker.SelectedTo, picker.SelectedCount, renderTime);

            return VStack().WS().Children(
                picker,
                HStack().AlignItemsCenter().Children(
                    TextBlock("Selected: ").SemiBold(),
                    DeferSync(selection, value => TextBlock(value))
                )
            ).MB(24);
        }

        private static IComponent BucketPickerDemo(TimeHistogramBucket[] buckets)
        {
            var selection = new SettableObservable<string>();
            var picker = TimeHistogramPicker(buckets)
               .OnRangeChanged((from, to, count) => selection.Value = FormatSelection(from, to, count));

            selection.Value = FormatSelection(picker.SelectedFrom, picker.SelectedTo, picker.SelectedCount);

            return VStack().WS().Children(
                picker,
                HStack().AlignItemsCenter().Children(
                    TextBlock("Selected: ").SemiBold(),
                    DeferSync(selection, value => TextBlock(value))
                )
            ).MB(24);
        }

        private static string FormatSelection(DateTime from, DateTime to, int count, Func<DateTime, string> renderTime = null)
        {
            if (count == 0)
            {
                return "No values";
            }

            renderTime = renderTime ?? DefaultRenderTime;
            return $"{renderTime(from)} - {renderTime(to)} ({count:n0} values)";
        }

        private static string DefaultRenderTime(DateTime time) => time.ToString("g");

        private static DateTime[] GetSmallValues()
        {
            var now = DateTime.Now;
            return new[]
            {
                now.AddMinutes(30),
                now.AddMinutes(-15),
                now.AddHours(2)
            };
        }

        private static DateTime[] GetDenseValues()
        {
            var start = DateTime.Today.AddHours(8);
            return Enumerable.Range(0, 720)
               .SelectMany(i => Enumerable.Range(0, 1 + (i % 9)).Select(j => start.AddMinutes(i).AddSeconds(j * 7)))
               .ToArray();
        }

        private static DateTime[] GetFineGrainedValues()
        {
            var start = DateTime.Today.AddHours(14);
            return Enumerable.Range(0, 360)
               .SelectMany(second =>
               {
                   var count = second % 45 < 9 ? 8 : second % 30 < 4 ? 5 : second % 11 == 0 ? 3 : 1;
                   return Enumerable.Range(0, count).Select(index => start.AddSeconds(second).AddMilliseconds(index * 90));
               })
               .ToArray();
        }

        private static DateTime[] GetMultiYearValues()
        {
            var start = DateTime.Today.AddYears(-6);
            return Enumerable.Range(0, 180)
               .Select(i => start.AddDays((i * 17) % 2190).AddHours((i * 11) % 24))
               .ToArray();
        }

        private static DateTime[] GetGappedClusterValues()
        {
            var start = DateTime.Today.AddYears(-4);

            var tinyBurst = Enumerable.Range(0, 18)
               .Select(i => start.AddSeconds(i * 11));

            var releaseCluster = Enumerable.Range(0, 240)
               .Select(i => start.AddMonths(9).AddMinutes(i * 5).AddSeconds(i % 13));

            var sparseReview = Enumerable.Range(0, 9)
               .Select(i => start.AddMonths(18).AddDays(i * 3).AddHours(i % 5));

            var denseMigration = Enumerable.Range(0, 1200)
               .Select(i => start.AddYears(3).AddMinutes(i % 180).AddSeconds(i / 180));

            var longTail = Enumerable.Range(0, 45)
               .Select(i => start.AddYears(4).AddDays(i * 2).AddHours(i % 7));

            return denseMigration
               .Concat(tinyBurst)
               .Concat(longTail)
               .Concat(releaseCluster)
               .Concat(sparseReview)
               .ToArray();
        }

        private static TimeHistogramBucket[] GetDailyBackendBuckets()
        {
            var start = DateTime.Today.AddDays(-18);
            var counts = new[] { 12, 0, 3, 45, 0, 0, 18, 95, 30, 0, 4, 8, 0, 150, 70, 0, 22, 6, -5 };

            return counts
               .Select((count, i) => new TimeHistogramBucket(start.AddDays(i), start.AddDays(i + 1), count))
               .Reverse()
               .ToArray();
        }

        private static TimeHistogramBucket[] GetLongRangeBackendBuckets()
        {
            var start = DateTime.Today.AddYears(-8);
            var counts = new[] { 20, 0, 45, 140, 12, 0, 0, 260, 410, 90, 0, 35, 75, 0, 510, 180 };

            return counts.Select((count, i) =>
            {
                var bucketStart = start.AddMonths(i * 6);
                return new TimeHistogramBucket(bucketStart, bucketStart.AddMonths(6), count);
            }).ToArray();
        }

        private static DateTime[] GetLargeValues()
        {
            var start = DateTime.Today.AddYears(-1);
            var minutesInYear = 365 * 24 * 60;
            return Enumerable.Range(0, 100000)
               .Select(i => start.AddMinutes((i * 37) % minutesInYear).AddSeconds(i % 60))
               .ToArray();
        }
    }
}
