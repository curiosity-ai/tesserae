using Tesserae;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 102, Icon = UIcons.AppsSort)]
    public class SegmentedControlSample : IComponent, ISample
    {
        private IComponent _content;

        public SegmentedControlSample()
        {
            var segmentedControl = SegmentedControl()
                .AddOption("list", "List", UIcons.List)
                .AddOption("grid", "Grid", UIcons.Apps)
                .AddOption("table", "Table", UIcons.Table);

            var result = TextBlock("Selected: list").MarginTop(16.px());

            segmentedControl.OnChange((s, value) =>
            {
                result.Text = $"Selected: {value}";
            });

            _content = SectionStack()
                .Title(SampleHeader(nameof(SegmentedControlSample)))
                .Section(
                    Stack()
                        .Children(
                            TextBlock("SegmentedControl").MediumPlus().SemiBold(),
                            segmentedControl,
                            result
                        )
                );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}