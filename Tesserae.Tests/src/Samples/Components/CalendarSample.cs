using System;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 17, Icon = UIcons.Calendar)]
    public class CalendarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CalendarSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(CalendarSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A standalone calendar component for date selection.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Calendar().Do(c => c.OnChange((s, e) => Toast().Information($"Selected date: {s.SelectedDate.ToShortDateString()}")))
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
