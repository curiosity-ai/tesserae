using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.DateTime, Order = 20, Icon = UIcons.CalendarLines)]
    public class DateRangePickerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DateRangePickerSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(DateRangePickerSample), UIcons.Calendar, "Pick a contiguous range of dates")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("DateRangePicker is a composite control that lets users choose a 'from' and 'to' date in one place. It's built from two DatePicker instances joined by a visual separator, with the min/max of each side kept in sync so the picker can never produce an invalid range."),
                    TextBlock("Use it for filter panels, report ranges, billing periods, scheduling and any other place where a start–end date pair is needed."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic"),
                    Label("Pick a range").SetContent(DateRangePicker()),
                    // SampleDate rather than DateTime.Today: the picker renders the dates as text.
                    SampleSubTitle("Pre-filled (a day → a week later)"),
                    Label("Selected range").SetContent(DateRangePicker(SampleDate.Today, SampleDate.Today.AddDays(7))),
                    SampleSubTitle("Reactive"),
                    DateRangePicker(SampleDate.Today, SampleDate.Today.AddDays(14))
                        .OnChange(r => Toast().Information($"{r.From:d} → {r.To:d}"))
                )).SetTitle("Usage")))
               .SeeAlso(typeof(DatePickerSample), typeof(DateTimePickerSample), typeof(TimeHistogramPickerSample), typeof(MonthPickerSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
