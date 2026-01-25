using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.F)]
    public class TextBlockSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TextBlockSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(TextBlockSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TextBlock is the fundamental component for displaying text in Tesserae. It provides a consistent way to apply typography styles, sizes, and weights across your application."),
                    TextBlock("It supports various built-in sizes, from tiny to mega, and different weights and colors.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use the predefined text sizes to maintain visual hierarchy. Use semi-bold or bold weights for headers and important information. Leverage the built-in color options (primary, success, danger, etc.) to convey meaning consistently. For long blocks of text, ensure the width is constrained for better readability. Use 'NoWrap' and text-overflow properties when dealing with limited space, such as in list items.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Text Sizes"),
                    VStack().Children(
                        TextBlock("Mega Text").Mega(),
                        TextBlock("XXLarge Text").XXLarge(),
                        TextBlock("XLarge Text").XLarge(),
                        TextBlock("Large Text").Large(),
                        TextBlock("MediumPlus Text").MediumPlus(),
                        TextBlock("Medium Text (Default)").Medium(),
                        TextBlock("SmallPlus Text").SmallPlus(),
                        TextBlock("Small Text").Small(),
                        TextBlock("XSmall Text").XSmall(),
                        TextBlock("Tiny Text").Tiny()
                    ),
                    SampleSubTitle("Weights and Colors"),
                    VStack().Children(
                        TextBlock("Bold Primary Text").Bold().Primary(),
                        TextBlock("Semi-Bold Success Text").SemiBold().Success(),
                        TextBlock("Regular Danger Text").Regular().Danger()
                    ),
                    SampleSubTitle("Wrapping and Overflow"),
                    VStack().Children(
                        TextBlock("Default wrapping:").SemiBold(),
                        TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.").Width(300.px()),
                        TextBlock("No wrapping (ellipsis):").SemiBold().MT(16),
                        TextBlock("This is a very long text that will be truncated with an ellipsis because it has NoWrap set and a constrained width.").NoWrap().Width(300.px())
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
