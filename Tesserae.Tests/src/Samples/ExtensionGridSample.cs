using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class ExtensionGridSample : IComponent
    {
        private readonly IComponent _content;

        public ExtensionGridSample()
        {
            var stack = new ExtensionGrid((b) => Toast().Information("See more clicked!")).H(300);
            var stackSmall = new ExtensionGridSmall((b) => Toast().Information("See more clicked!")).H(300);
            _content = SectionStack()
               .Title(SampleHeader(nameof(ExtensionGridSample)))
               .Section(stack)
               .Section(stackSmall);
            SetChildren(stack, stackSmall, 15);
        }

        private void SetChildren(ExtensionGrid stack, ExtensionGridSmall stackSmall, int count)
        {
            stack.Clear();
            for (int i = 0; i < count; i++)
            {
                stack.Add(Icon(LineAwesome.Random).XXLarge(), " This is the Title " + i, TextBlock("Space for content"), () => Toast().Information("Clicked"));
                stackSmall.Add(Icon(LineAwesome.Random).XXLarge(), " This is the Title " + i, () => Toast().Information("Clicked"));
            }
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}