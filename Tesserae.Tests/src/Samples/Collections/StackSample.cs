using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 0, Icon = LineAwesome.RulerVertical)]
    public class StackSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public StackSample()
        {

            var mainButton = Button("Some Text").TextLeft().MinWidth(200.px()).Ellipsis().IconOnHover();
            mainButton.Tooltip("Tooltip for the main Button").SetIcon(LineAwesome.ChevronLeft, Theme.Primary.Background);

            var otherButton = Button().Tooltip("Tooltip for the other Button").SetIcon(LineAwesome.ThumbsDown, color: Theme.Danger.Background).Fade();
            var hoverStack = HStack().MaxWidth(500.px()).Children(mainButton, otherButton);

            var stack = Stack();
            var countSlider = Slider(5, 0, 10, 1);

            _content = SectionStack()
               .Title(SampleHeader(nameof(StackSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Stack().Children(
                        HStack().Children(
                            Stack().Children(
                                Label("Number of items:").SetContent(countSlider.OnInput((s, e) => SetChildren(stack, s.Value))),
                                HStack().Children(
                                    ChoiceGroup("Orientation:").Horizontal().Choices(Choice("Vertical").Selected(), Choice("Horizontal"), Choice("Vertical Reverse"), Choice("Horizontal Reverse")).OnChange(
                                        (s, e) =>
                                        {
                                            if (s.SelectedOption.Text == "Horizontal")
                                                stack.Horizontal();
                                            else if (s.SelectedOption.Text == "Vertical")
                                                stack.Vertical();
                                            else if (s.SelectedOption.Text == "Horizontal Reverse")
                                                stack.HorizontalReverse();
                                            else if (s.SelectedOption.Text == "Vertical Reverse")
                                                stack.VerticalReverse();
                                        })
                                )
                            )
                        ), stack.HeightAuto())))
               .Section(Stack().Children(SampleTitle("Advanced"),
                    Label("Stack with hover events").SetContent(hoverStack
                       .OnMouseOver((s, e) => otherButton.Show())
                       .OnMouseOut((s,  e) => otherButton.Fade())
                       .Children(mainButton.WS(), otherButton))));
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
            return _content.Render();
        }
    }
}