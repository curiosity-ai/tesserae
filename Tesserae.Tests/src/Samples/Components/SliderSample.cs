using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = LineAwesome.SlidersH)]
    public class SliderSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SliderSample()
        {
            var value = new SettableObservable<int>(0);
            var s1 = Slider(val: 0, min: 0, max: 100, step: 1).OnInput((s,  e) => value.Update(currVal => value.Value = s.Value));
            var s2 = Slider(val: 0, min: 0, max: 100, step: 10).OnInput((s, e) => value.Update(currVal => value.Value = s.Value));
            value.Observe(changedValue => s1.Value = changedValue);

            _content = SectionStack()
               .Title(SampleHeader(nameof(SliderSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TODO")
                    //                TextBlock("ChoiceGroup emphasize all options equally, and that may draw more attention to the options than necessary. Consider using other controls, unless the options deserve extra attention from the user. For example, if the default option is recommended for most users in most situations, use a Dropdown component instead."),
                    //                TextBlock("If there are only two mutually exclusive options, combine them into a single Checkbox or Toggle switch. For example, use a Checkbox for \"I agree\" instead of ChoiceGroup buttons for \"I agree\" and \"I don't agree.\"")
                ))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    HStack().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("TODO")),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("TODO")))))
               .Section(
                    Stack().Children(
                        SampleTitle("Usage"),
                        Label("Value").Medium().SetContent(Defer(value, currentValue => TextBlock(currentValue.ToString()).AsTask())),
                        Label("Default Slider (val: 0, min: 0, max: 100, step: 1)").Medium().SetContent(s1),
                        Label("Default Slider (val: 0, min: 0, max: 100, step: 10)").Medium().SetContent(s2)
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}