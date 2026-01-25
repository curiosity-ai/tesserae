using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.SettingsSliders)]
    public class SliderSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SliderSample()
        {
            var value = new SettableObservable<int>(50);
            var s1    = Slider(val: 50, min: 0, max: 100, step: 1).OnInput((s,  e) => value.Value = s.Value);
            var s2    = Slider(val: 20, min: 0, max: 100, step: 10).OnInput((s, e) => Toast().Information($"Value changed to {s.Value}"));

            _content = SectionStack()
               .Title(SampleHeader(nameof(SliderSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Sliders allow users to select a value from a continuous or discrete range of values by moving a thumb along a track."),
                    TextBlock("They are ideal for settings that don't require high precision but benefit from a visual representation of the available range, such as volume or brightness.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use sliders when users need to choose a value from a range where the relative position is more important than the exact value. Provide clear labels for the minimum and maximum values. If the user needs to select a precise number, consider using a NumberPicker alongside or instead of a slider. Use discrete steps (increments) if the available choices are limited to specific intervals.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Sliders"),
                    VStack().Children(
                        Label("Continuous Slider (step: 1)").SetContent(s1),
                        HStack().Children(TextBlock("Current Value: "), DeferSync(value, v => TextBlock(v.ToString()).SemiBold())),
                        Label("Discrete Slider (step: 10)").SetContent(s2)
                    ),
                    SampleSubTitle("States"),
                    VStack().Children(
                        Label("Disabled Slider").Disabled().SetContent(Slider(val: 30).Disabled()),
                        Label("Required Slider").Required().SetContent(Slider(val: 10))
                    ),
                    SampleSubTitle("Vertical Sliders"),
                    HStack().Children(
                        Slider(val: 50).Vertical().H(150),
                        Slider(val: 20).Vertical().H(150),
                        Slider(val: 80).Vertical().H(150)
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
