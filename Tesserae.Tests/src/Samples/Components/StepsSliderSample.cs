using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 202, Icon = UIcons.SettingsSliders)]
    public class StepsSliderSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public StepsSliderSample()
        {
            var sizeValue    = new SettableObservable<string>("M");
            var percentValue = new SettableObservable<int>(50);

            var sizeSlider = new StepsSlider<string>("XS", "S", "M", "L", "XL")
               .OnChange(v =>
                {
                    sizeValue.Value = v;
                    Toast().Information($"Size changed to: {v}");
                });

            var percentSlider = new StepsSlider<int>(0, 25, 50, 75, 100)
               .OnChange(v => percentValue.Value = v);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(StepsSliderSample), UIcons.SettingsSliders, "A slider that snaps to discrete named steps")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("StepsSlider is a generic slider component that constrains movement to a fixed set of named values. Unlike a continuous slider, it only ever lands on one of the provided steps, making it suitable for cases where only a handful of distinct choices are valid."),
                    TextBlock("Common uses include selecting T-shirt sizes, quality levels, priority tiers, or any domain-specific ordered category where a free numeric value would be meaningless."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use StepsSlider when the number of valid values is small (typically 3–7) and each value has a clear, user-facing name. Prefer a Dropdown or ChoiceGroup when there are many options or the labels are long. Always show the currently selected value adjacent to the slider so users understand what they have chosen."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("String Steps (T-shirt Sizes)"),
                    VStack().Children(
                        Label("Size").SetContent(sizeSlider),
                        HStack().Children(TextBlock("Selected: "), DeferSync(sizeValue, v => TextBlock(v).SemiBold()))
                    ),
                    SampleSubTitle("Integer Steps (Percentage)"),
                    VStack().Children(
                        Label("Progress").SetContent(percentSlider),
                        HStack().Children(TextBlock("Selected: "), DeferSync(percentValue, v => TextBlock($"{v}%").SemiBold()))
                    ),
                    SampleSubTitle("Disabled"),
                    VStack().Children(
                        Label("Disabled Slider").Disabled().SetContent(new StepsSlider<string>("Low", "Medium", "High").Disabled())
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
