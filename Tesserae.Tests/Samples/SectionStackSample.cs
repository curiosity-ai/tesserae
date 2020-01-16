using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class SectionStackSample : IComponent
    {
        private IComponent content;

        public SectionStackSample()
        {
            var stack = SectionStack();
            var countSlider = Slider(5, 0, 10, 1);
            content = Stack().Background("#faf9f8").Children(
                TextBlock("Section Stack").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("A Session Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components."),
                TextBlock("Usage").MediumPlus(),
                Stack().Children(
                    Stack().Horizontal().Children(
                        Stack().Children(
                            Label("Number of items:").Content(countSlider.OnInputed((s, e) => SetChildren(stack, e.Value)))
                            )
                        )
                    ),
                    stack.HeightAuto()
            );
            SetChildren(stack, 5);
        }

        private void SetChildren(SectionStack stack, int count)
        {
            stack.Clear();
            for (int i = 0; i < count; i++)
            {
                stack.Section(Button(i.ToString()));
            }
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
