using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.TimePast)]
    public class TimelineSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TimelineSample()
        {
            var obsList = new ObservableList<IComponent>();

            var vs = VisibilitySensor((v) =>
            {
                obsList.Remove(v);
                obsList.AddRange(GetSomeItems(20));
                v.Reset();
                obsList.Add(v);
            });

            obsList.AddRange(GetSomeItems(10));
            obsList.Add(vs);

            _content = SectionStack().WidthStretch()
               .Title(SampleHeader(nameof(TimelineSample)))
               .Section(
                    Stack()
                       .Children(
                            SampleTitle("Overview"),
                            TextBlock("Timeline provides a base component for rendering vertical timelines. " +
                                    "It is agnostic of the tile component used, and selection "               +
                                    "management. These concerns can be layered separately.")
                               .PB(16)))
               .Section(
                    Stack()
                       .Children(
                            SampleTitle("Usage"),
                            TextBlock("Timeline").Medium().PB(16),
                            Timeline().Children(GetSomeItems(10)).PB(16).Height(500.px()).PaddingBottom(32.px()),
                            TextBlock("Timeline with Max Width").Medium().PB(16),
                            Timeline().TimelineWidth(600.px()).Children(GetSomeItems(10)).PB(16).Height(500.px()).PaddingBottom(32.px()),
                            TextBlock("Timeline Same Side").Medium().PB(16),
                            Timeline().SameSide().Children(GetSomeItems(10)).PB(16).Height(500.px()).PaddingBottom(32.px()),
                            TextBlock("Timeline Same Side with Max Width").Medium().PB(16),
                            Timeline().TimelineWidth(600.px()).SameSide().Children(GetSomeItems(10)).PB(16).Height(500.px()).PaddingBottom(32.px())
                        ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private IComponent[] GetSomeItems(int count)
        {
            return Enumerable
               .Range(1, count)
               .Select(number => TextBlock($"Lorem Ipsum {number}").NonSelectable())
               .ToArray();
        }
    }
}