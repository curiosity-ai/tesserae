using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class PivotSample : IComponent
    {
        private IComponent content;

        public PivotSample()
        {
            content = SectionStack()
                .Title(TextBlock("Pivot").XLarge().Bold())
                .Section(Stack().Children(
                                       SampleTitle("Overview"),
                                       TextBlock("TODO"),
                                       TextBlock("Examples of experiences that use Panels").MediumPlus()))
                .Section(Stack().Children(
                                       SampleTitle("Best Practices"),
                                       Stack().Horizontal().Children(
                                           Stack().Width(40, Unit.Percents).Children(
                                               SampleSubTitle("Do"),
                                               SampleDo("TODO")
                                           ),
                                           Stack().Width(40, Unit.Percents).Children(
                                               SampleSubTitle("Don't"),
                                               SampleDont("TODO")
                                           )
                                       )))
                .Section(Stack().Children(
                                       SampleTitle("Usage"),
                                           Pivot().Pivot("tab1", () => Button().SetText("Cached").NoBorder().NoBackground().MediumPlus().Regular(),
                                                                 () => TextBlock(DateTimeOffset.UtcNow.ToString()).MediumPlus(), cached: true)
                                                  .Pivot("tab2", () => Button().SetText("Not Cached").SetIcon("las la-sync").NoBorder().NoBackground().MediumPlus().Regular(),
                                                                 () => TextBlock(DateTimeOffset.UtcNow.ToString()).MediumPlus(), cached: false)
                                       ));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
