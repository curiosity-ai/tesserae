using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 1, Icon = UIcons.TextSize)]
    public class TextAreaSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TextAreaSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(TextAreaSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TextAreas allow users to enter and edit multi-line text. They are commonly used for comments, descriptions, or any input that requires multiple lines of text.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use a TextArea when the expected input might be long or span multiple lines. Always pair it with a clear label. Consider using the AutoResize functionality to provide a better user experience without taking up too much initial screen real estate.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic TextAreas"),
                    VStack().Children(
                        Label("Standard").SetContent(TextArea()),
                        Label("Placeholder").SetContent(TextArea().SetPlaceholder("Enter your description...")),
                        Label("Disabled").Disabled().SetContent(TextArea("Disabled content").Disabled()),
                        Label("Read-only").SetContent(TextArea("Read-only content").ReadOnly())
                    ),
                    SampleSubTitle("Auto Resize"),
                    VStack().Children(
                        Label("AutoResize (allowShrink = true)").SetContent(TextArea("Type multiple lines here...").AutoResize(allowShrink: true)),
                        Label("AutoResize (allowShrink = false)").SetContent(TextArea("Type multiple lines here...").AutoResize(allowShrink: false)),
                        Label("AutoResize (minHeight = 100)").SetContent(TextArea("Type here...").AutoResize(minHeight: 100)),
                        Label("AutoResize (maxHeight = 150)").SetContent(TextArea("Type many lines to see scrolling...").AutoResize(maxHeight: 150))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Required").Required().SetContent(TextArea()),
                        Label("Custom Error").SetContent(TextArea().Error("Something went wrong").IsInvalid())
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
