using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.TimePast)]
    public class TimelineSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TimelineSample()
        {
            _content = SectionStack().WidthStretch()
               .SampleTitle(nameof(TimelineSample), UIcons.Clock, "A component to display a timeline")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Timeline displays a series of events in chronological order, using a vertical line to connect them."),
                    TextBlock("It is ideal for activity feeds, version histories, or any process where the sequence of steps is important."))).SetTitle("Overview")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Timelines to show the progression of time or a sequence of related events. Clearly distinguish between past, current, and future events if applicable. Use the 'SameSide' property if you want a more linear, left-aligned layout, or the default staggered layout for a more balanced visual look. Ensure that each event has a clear timestamp and a concise description."))).SetTitle("Best Practices")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Default Staggered Timeline"),
                    Timeline().Children(GetSomeItems(6)).Height(300.px()).MB(32),
                    SampleSubTitle("Same Side Alignment"),
                    Timeline().SameSide().Children(GetSomeItems(6)).Height(300.px()).MB(32),
                    SampleSubTitle("Constrained Width"),
                    Timeline().TimelineWidth(400.px()).Children(GetSomeItems(6)).Height(300.px())
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();

        private IComponent[] GetSomeItems(int count)
        {
            return Enumerable.Range(1, count).Select(n =>
                VStack().Children(
                    TextBlock($"Event {n}").SemiBold(),
                    TextBlock($"{DateTime.Today.AddHours(-n):t} - Description of the event happens here.").Small()
                )).ToArray();
        }
    }
}
