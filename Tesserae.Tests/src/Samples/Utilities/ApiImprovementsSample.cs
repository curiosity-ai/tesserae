using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 100, Icon = UIcons.MagicWand)]
    public class ApiImprovementsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ApiImprovementsSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ApiImprovementsSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("This sample demonstrates recent improvements to the Tesserae API, focusing on more consistent fluent methods and broader availability of common styling and event extensions.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Generic OnClick"),
                    TextBlock("Previously, only certain components had an OnClick method. Now, any component can have a click handler."),
                    HStack().Children(
                        Icon(UIcons.Heart, color: "red").Large().OnClick(() => alert("Icon clicked!")),
                        TextBlock("Click this text").SemiBold().OnClick(() => alert("Text clicked!"))
                    ),

                    SampleSubTitle("New Style Extensions"),
                    TextBlock("New extensions for common CSS properties like Opacity, Cursor, Display, ZIndex, and NativeTitle (native tooltip)."),
                    HStack().Children(
                        Button("Opacity 0.5").Opacity(0.5),
                        Button("Pointer Cursor").Cursor("pointer"),
                        Button("Native Title").NativeTitle("This is a native HTML title tooltip")
                    ),

                    SampleSubTitle("Scrolling Extensions"),
                    TextBlock("Easily make any component scrollable."),
                    VStack().Children(
                        VStack().Children(
                            TextBlock("Scrollable Area").SemiBold(),
                            TextBlock("Line 1"), TextBlock("Line 2"), TextBlock("Line 3"),
                            TextBlock("Line 4"), TextBlock("Line 5"), TextBlock("Line 6")
                        ).Height(100).Scrollable().Padding(8).Background("#eee")
                    ),

                    SampleSubTitle("Improved Background and Foreground"),
                    TextBlock("Background and Foreground extensions are now available for all components."),
                    HStack().Children(
                        TextBlock("Text with Background").Background("yellow").Padding(4),
                        Icon(UIcons.Star, color: "gold").Background("navy").Padding(4)
                    ),

                    SampleSubTitle("Pixel Overloads"),
                    TextBlock("Overloads taking integers now exist for most layout methods, avoiding the need for .px()."),
                    HStack().Children(
                        Button("Width 200").Width(200),
                        Button("Margin 20").Margin(20)
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
