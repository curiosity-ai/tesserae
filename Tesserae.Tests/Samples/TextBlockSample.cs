using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class TextBlockSample : IComponent
    {
        private IComponent content;

        public TextBlockSample()
        {
            content = Stack().Children(
                TextBlock("TextBlock").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("Text is a component for displaying text. You can use Text to standardize text across your web app."),
                TextBlock("Usage").MediumPlus(),
                TextBlock("TextBox Ramp Example").Medium(),
                Stack().Horizontal().Children(TextBlock("Variant").WidthPixels(200).SemiBold(), TextBlock("Example").SemiBold()),
                Stack().Horizontal().Children(TextBlock("tiny").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").Tiny()),
                Stack().Horizontal().Children(TextBlock("xSmall").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").XSmall()),
                Stack().Horizontal().Children(TextBlock("small").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").Small()),
                Stack().Horizontal().Children(TextBlock("smallPlus").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").SmallPlus()),
                Stack().Horizontal().Children(TextBlock("medium").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").Medium()),
                Stack().Horizontal().Children(TextBlock("mediumPlus").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").MediumPlus()),
                Stack().Horizontal().Children(TextBlock("large").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").Large()),
                Stack().Horizontal().Children(TextBlock("xLarge").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").XLarge()),
                Stack().Horizontal().Children(TextBlock("xxLarge").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").XXLarge()),
                Stack().Horizontal().Children(TextBlock("mega").WidthPixels(200), TextBlock("The quick brown fox jumped over the lazy dog.").Mega()),
                TextBlock("TextBox Wrap Example").Medium(),
                TextBlock("Wrap (Default)").SmallPlus(),
                TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").WidthPercents(50),
                TextBlock("No Wrap").SmallPlus(),
                TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").NoWrap().WidthPercents(50)
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
