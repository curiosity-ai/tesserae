using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class StackSample : IComponent
    {
        private IComponent content;

        public StackSample()
        {
            var stack = Stack();
            var countSlider = Slider(5, 0, 10, 1);
            content = Stack().Children(
                TextBlock("Stack").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components."),
                TextBlock("Usage").MediumPlus(),
                Stack().Children(
                    Stack().Horizontal().Children(
                        Stack().Children(
                            Label("Number of items:").Content(countSlider.OnInputed((s, e) => SetChildren(stack, e.Value))),
                            Stack().Horizontal().Children(
                                ChoiceGroup("Orientation:").Horizontal().Options(Option("Vertical").Selected(), Option("Horizontal"), Option("Vertical Reverse"), Option("Horizontal Reverse")).OnChanged(
                                (s, e) =>
                                {
                                    if (e.SelectedOption.Text == "Horizontal")
                                        stack.Horizontal();
                                    else if (e.SelectedOption.Text == "Vertical")
                                        stack.Vertical();
                                    else if (e.SelectedOption.Text == "Horizontal Reverse")
                                        stack.HorizontalReverse();
                                    else if (e.SelectedOption.Text == "Vertical Reverse")
                                        stack.VerticalReverse();
                                })
                            )
                        )
                    ),
                    stack.HeightAuto()
                )
            );
            SetChildren(stack, 5);
        }

        private void SetChildren(Stack stack, int count)
        {
            stack.Clear();
            for (int i = 0; i < count; i++)
            {
                stack.Add(Button(i.ToString()));
            }
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
