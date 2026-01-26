using System;
using H5.Core;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 0, Icon = UIcons.RulerVertical)]
    public class StackSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private const string sampleSortableStackLocalStorageKey = "sampleSortableStackOrder";

        public StackSample()
        {
            var mainButton = Button("Hover me").TextLeft().MinWidth(200.px());
            var otherButton = Button().SetIcon(UIcons.ThumbsDown, color: Theme.Danger.Background).Fade();
            var hoverStack = HStack().MaxWidth(500.px()).Children(mainButton, otherButton);

            var sortableStack = new SortableStack(Stack.Orientation.Horizontal).WS().AlignItemsCenter().PB(8).MaxWidth(500.px());
            sortableStack.Add("1", Button().SetIcon(UIcons._1));
            sortableStack.Add("2", Button().SetIcon(UIcons._2));
            sortableStack.Add("3", Button().SetIcon(UIcons._3));
            sortableStack.Add("4", Button().SetIcon(UIcons._4));
            sortableStack.Add("5", Button().SetIcon(UIcons._5));

            var stack = Stack();
            var countSlider = Slider(5, 0, 10, 1);

            _content = SectionStack()
               .Title(SampleHeader(nameof(StackSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Stacks are container components that simplify the use of Flexbox for layout. They allow you to arrange children components either horizontally (HStack) or vertically (VStack)."),
                    TextBlock("Tesserae's Stack also includes advanced features like 'SortableStack' for drag-and-drop reordering.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Stacks as the primary way to organize your UI elements. Use HStack for side-by-side components and VStack for top-to-bottom arrangements. Leverage the 'Gap' property to ensure consistent spacing between children. Use SortableStack when users need to customize the order of items, such as in a dashboard or task list. Avoid deeply nested stacks if a Grid layout would be more appropriate for the complexity.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Live Layout Playground"),
                    VStack().Children(
                        HStack().Children(
                            VStack().Children(
                                Label("Number of items:").SetContent(countSlider.OnInput((s, e) => SetChildren(stack, s.Value))),
                                ChoiceGroup("Orientation:").Horizontal().Choices(Choice("Vertical").Selected(), Choice("Horizontal"), Choice("Vertical Reverse"), Choice("Horizontal Reverse")).OnChange((s, e) => {
                                    if (s.SelectedOption.Text == "Horizontal") stack.Horizontal();
                                    else if (s.SelectedOption.Text == "Vertical") stack.Vertical();
                                    else if (s.SelectedOption.Text == "Horizontal Reverse") stack.HorizontalReverse();
                                    else if (s.SelectedOption.Text == "Vertical Reverse") stack.VerticalReverse();
                                })
                            )
                        ).MB(16),
                        Card(stack.HeightAuto())
                    ),
                    SampleSubTitle("Sortable Stack"),
                    TextBlock("Drag and drop these buttons to reorder them. The state is saved to local storage."),
                    sortableStack.MB(32),
                    SampleSubTitle("Interactive Events"),
                    TextBlock("Stacks can respond to mouse events, allowing for complex hover behaviors."),
                    hoverStack.OnMouseOver((s, e) => otherButton.Show()).OnMouseOut((s, e) => otherButton.Fade()),
                    SampleSubTitle("Rounded Stacks"),
                    TextBlock("Rounding is visible when the stack has a background or border."),
                    HStack().Children(
                        VStack().Children(TextBlock("Small")).P(16).Rounded(BorderRadius.Small).Background(Theme.Colors.Blue200).W(100).AlignItemsCenter(),
                        VStack().Children(TextBlock("Medium")).P(16).Rounded(BorderRadius.Medium).Background(Theme.Colors.Blue200).W(100).AlignItemsCenter(),
                        VStack().Children(TextBlock("Full")).P(16).Rounded(BorderRadius.Full).Background(Theme.Colors.Blue200).W(100).AlignItemsCenter()
                    )
                ));
            SetChildren(stack, 5);
        }

        private void SetChildren(Stack stack, int count) { stack.Clear(); for (int i = 0; i < count; i++) stack.Add(Button($"Item {i}")); }
        public HTMLElement Render() => _content.Render();
    }
}
