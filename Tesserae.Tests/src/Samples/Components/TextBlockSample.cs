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
                    TextBlock("Text is a component for displaying text. You can use Text to standardize text across your web app.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("TextBox Ramp Example").Medium(),
                    HStack().Children(TextBlock("Variant").Width(200.px()).SemiBold(), TextBlock("Example").SemiBold()),
                    HStack().Children(TextBlock("tiny").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").Tiny()),
                    HStack().Children(TextBlock("xSmall").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").XSmall()),
                    HStack().Children(TextBlock("small").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").Small()),
                    HStack().Children(TextBlock("smallPlus").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").SmallPlus()),
                    HStack().Children(TextBlock("medium").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").Medium()),
                    HStack().Children(TextBlock("mediumPlus").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").MediumPlus()),
                    HStack().Children(TextBlock("large").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").Large()),
                    HStack().Children(TextBlock("xLarge").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").XLarge()),
                    HStack().Children(TextBlock("xxLarge").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").XXLarge()),
                    HStack().Children(TextBlock("mega").Width(200.px()), TextBlock("The quick brown fox jumped over the lazy dog.").Mega()),
                    TextBlock("TextBox Wrap Example").Medium(),
                    TextBlock("Wrap (Default)").SmallPlus(),
                    TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").Width(50.percent()),
                    TextBlock("No Wrap").SmallPlus(),
                    TextBlock("This is a very long text that can wrap but from here on it will never wrap:", afterText: "this will not wrap").SmallPlus().WS(),
                    TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").NoWrap().Width(50.percent())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}