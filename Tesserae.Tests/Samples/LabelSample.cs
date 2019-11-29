using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class LabelSample : IComponent
    {
        private IComponent content;

        public LabelSample()
        {
            content = Stack().Children(
                TextBlock("Label").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("Labels give a name or title to a component or group of components. Labels should be in close proximity to the component or group they are paired with. Some components, such as TextField, Dropdown, or Toggle, already have Labels incorporated, but other components may optionally add a Label if it helps inform the user of the component’s purpose."),
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().WidthPercents(40).Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Use sentence casing, e.g. “First name”."),
                        TextBlock("Be short and concise."),
                        TextBlock("When adding a Label to components, use the text as a noun or short noun phrase.")
                    ),
                    Stack().WidthPercents(40).Children(
                        TextBlock("Don't").Medium(),
                        TextBlock("Use Labels as instructional text, e.g. “Click to get started”."),
                        TextBlock("Don’t use full sentences or complex punctuation (colons, semicolons, etc.).")
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                TextBlock("Label").Medium(),
                Label("I'm Label"),
                Label("I'm a disabled Label").Disabled(),
                Label("I'm a required Label").Required(),
                Label("A Label for An Input").Content(TextBox())
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
