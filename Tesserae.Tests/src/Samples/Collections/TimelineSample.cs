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
            _content = SectionStack().Secondary().WidthStretch()
               .SampleTitle(typeof(TimelineSample), UIcons.Clock, "A component to display a timeline")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Timeline displays a series of events in chronological order, using a vertical line to connect them."),
                    TextBlock("It is ideal for activity feeds, version histories, or any process where the sequence of steps is important."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Timelines to show the progression of time or a sequence of related events. Clearly distinguish between past, current, and future events if applicable. Use the 'SameSide' property if you want a more linear, left-aligned layout, or the default staggered layout for a more balanced visual look. Ensure that each event has a clear timestamp and a concise description."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Default Staggered Timeline"),
                    BuildTimeline(Timeline()).Height(400.px()).MB(32),
                    SampleSubTitle("Same Side Alignment"),
                    BuildTimeline(Timeline().SameSide()).Height(400.px()).MB(32),
                    SampleSubTitle("Constrained Width"),
                    BuildTimeline(Timeline().TimelineWidth(400.px())).Height(400.px())
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();

        private Timeline BuildTimeline(Timeline timeline)
        {
            timeline.Add(VStack().Children(
                HStack().WS().JustifyContent(ItemJustify.Between).Children(
                    TextBlock("Build #4182 succeeded").SemiBold(),
                    TextBlock("2m ago").Small().Foreground(Theme.Secondary.Foreground)
                ),
                TextBlock("All 47 tests passing on master.").Small().Foreground(Theme.Secondary.Foreground).PT(8)
            ), Theme.Success.Background);

            timeline.Add(VStack().Children(
                HStack().WS().JustifyContent(ItemJustify.Between).Children(
                    TextBlock("Anna pushed to master").SemiBold(),
                    TextBlock("14m ago").Small().Foreground(Theme.Secondary.Foreground)
                ),
                TextBlock("Merge #312: Add ProgressRing component (3 files changed).").Small().Foreground(Theme.Secondary.Foreground).PT(8)
            ), Theme.Primary.Background);

            timeline.Add(VStack().Children(
                HStack().WS().JustifyContent(ItemJustify.Between).Children(
                    TextBlock("Build #4181 failed").SemiBold(),
                    TextBlock("1h ago").Small().Foreground(Theme.Secondary.Foreground)
                ),
                TextBlock("2 of 47 tests failed in Tesserae.Tests/Buttons.").Small().Foreground(Theme.Secondary.Foreground).PT(8)
            ), Theme.Danger.Background);

            timeline.Add(VStack().Children(
                HStack().WS().JustifyContent(ItemJustify.Between).Children(
                    TextBlock("Index rebuilt").SemiBold(),
                    TextBlock("yesterday").Small().Foreground(Theme.Secondary.Foreground)
                ),
                TextBlock("4,182 documents reindexed from curiosity-prod.").Small().Foreground(Theme.Secondary.Foreground).PT(8)
            ), Theme.Default.DarkBorder);

            return timeline;
        }
    }
}
