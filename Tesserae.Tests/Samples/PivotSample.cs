using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

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
                                           Stack().Width(40, Unit.Percents).Children(
                                               TextBlock("Do: TODO").Medium()
                                           ),
                                           Stack().Width(40, Unit.Percents).Children(
                                               TextBlock("Don't: TODO").Medium()
                                           )
                                       ),
                                       TextBlock("Usage").MediumPlus(),
                                           Pivot().Pivot("tab1", () => Button().Text("Cached").NoBorder().NoBackground().MediumPlus().Regular(),
                                                                 () => TextBlock(DateTimeOffset.UtcNow.ToString()).MediumPlus(), cached: true)
                                                  .Pivot("tab2", () => Button().Text("Not Cached").Icon("fal fa-sync").NoBorder().NoBackground().MediumPlus().Regular(),
                                                                 () => TextBlock(DateTimeOffset.UtcNow.ToString()).MediumPlus(), cached: false)
                                       );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
