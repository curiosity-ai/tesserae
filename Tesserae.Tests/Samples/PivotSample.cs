using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class PivotSample : IComponent
    {
        private IComponent content;

        public PivotSample()
        {
            content = Stack().Children(TextBlock("Pivot").XLarge(),
                                       TextBlock("Overview").MediumPlus(),
                                       TextBlock("TODO"),
                                       TextBlock("Examples of experiences that use Panels").MediumPlus(),
                                       TextBlock("Best Practices").MediumPlus(),
                                       Stack().Horizontal().Children(
                                           Stack().WidthPercents(40).Children(
                                               TextBlock("Do: TODO").Medium()
                                           ),
                                           Stack().WidthPercents(40).Children(
                                               TextBlock("Don't: TODO").Medium()
                                           )
                                       ),
                                       TextBlock("Usage").MediumPlus(),
                                           Pivot().Pivot("tab1", () => Button().Text("Cached").NoBorder().MediumPlus().Regular(),
                                                              () => TextBlock(DateTimeOffset.UtcNow.ToString()).MediumPlus(), cached: true)
                                                 .Pivot("tab2", () => Button().Text("Not Cached").Icon("fal fa-sync").NoBorder().MediumPlus().Regular(),
                                                              () => TextBlock(DateTimeOffset.UtcNow.ToString()).MediumPlus(), cached: false)
                                       );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
