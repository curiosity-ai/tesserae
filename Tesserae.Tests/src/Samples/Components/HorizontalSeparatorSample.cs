using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.HorizontalRule)]
    public class HorizontalSeparatorSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public HorizontalSeparatorSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(HorizontalSeparatorSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A HorizontalSeparator visually divides content into groups. It can optionally contain text or other components to label the group it introduces."),
                    TextBlock("The content can be aligned to the left, center, or right of the line.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use separators to provide structure to long forms or pages. Keep labels short and concise. Use them sparingly; too many separators can clutter the UI. Ensure the labels accurately describe the section that follows.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Text Alignment"),
                    HorizontalSeparator("Center Aligned (Default)"),
                    HorizontalSeparator("Left Aligned").Left(),
                    HorizontalSeparator("Right Aligned").Right(),
                    SampleSubTitle("Themed and Custom Content"),
                    VStack().Children(
                        HorizontalSeparator("Primary Color").Primary(),
                        HorizontalSeparator(HStack().Children(
                            Icon(UIcons.Info).PaddingRight(8.px()),
                            TextBlock("Information Section").SemiBold()
                        )).Primary().Left()
                    ),
                    SampleSubTitle("Empty Separator"),
                    TextBlock("A simple line without any label:"),
                    HorizontalSeparator("")
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
