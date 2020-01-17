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
            content = Stack().Children(
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
                stack.Section(Stack().Children(
                TextBlock($"Section {i}").MediumPlus().SemiBold(),
                TextBlock("Wrap (Default)").SmallPlus(),
                TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").WidthPercents(50),
                TextBlock("No Wrap").SmallPlus(),
                TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").NoWrap().WidthPercents(50)
                ));
            }
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
