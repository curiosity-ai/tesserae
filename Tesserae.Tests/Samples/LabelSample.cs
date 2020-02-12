using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class LabelSample : IComponent
    {
        private IComponent _content;

        public LabelSample()
        {
            _content = SectionStack()
            .Title(
            TextBlock("Label").XLarge().Bold())
            .Section(Stack().Children(
                SampleTitle("Overview"),
                TextBlock("Labels give a name or title to a component or group of components. Labels should be in close proximity to the component or group they are paired with. Some components, such as TextField, Dropdown, or Toggle, already have Labels incorporated, but other components may optionally add a Label if it helps inform the user of the component’s purpose.")))
            .Section(Stack().Children(
                SampleTitle("Best Practices"),
                Stack().Horizontal().Children(
                Stack().Width(40, Unit.Percent).Children(
                    SampleSubTitle("Do"),
                    SampleDo("Use sentence casing, e.g. “First name”."),
                    SampleDo("Be short and concise."),
                    SampleDo("When adding a Label to components, use the text as a noun or short noun phrase.")
                    ),
            Stack().Width(40, Unit.Percent).Children(
                SampleSubTitle("Don't"),
                SampleDo("Use Labels as instructional text, e.g. “Click to get started”."),
                SampleDo("Don’t use full sentences or complex punctuation (colons, semicolons, etc.).")))))
                .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Label").Medium(),
                    Label("I'm Label"),
                    Label("I'm a disabled Label").Disabled(),
                    Label("I'm a required Label").Required(),
                    Label("A Label for An Input").SetContent(TextBox())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
