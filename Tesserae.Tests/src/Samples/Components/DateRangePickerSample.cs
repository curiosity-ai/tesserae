using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.Calendar)]
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
                    SampleSubTitle("Pre-filled (today → next week)"),
                    Label("Selected range").SetContent(DateRangePicker(DateTime.Today, DateTime.Today.AddDays(7))),
                    SampleSubTitle("Reactive"),
                    DateRangePicker(DateTime.Today, DateTime.Today.AddDays(14))
                        .OnChange(r => Toast().Information($"{r.From:d} → {r.To:d}"))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
