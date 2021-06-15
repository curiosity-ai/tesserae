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
            var stack1 = new ExtensionGrid((b) => Toast().Information("See more clicked!")).H(300);
            var stack2 = new ExtensionGrid().H(300);
            var stackSmall = new ExtensionGrid((b) => Toast().Information("See more clicked!"), isSmall: true).H(300);
            _content = SectionStack()
               .Title(SampleHeader(nameof(ExtensionGridSample)))
               .Section(stack1)
               .Section(stack2)
               .Section(stackSmall);
            SetChildren(stack1, 15);
            SetChildren(stack2, 15);
            SetChildrenSmall(stackSmall, 15);
        }

        private void SetChildren(ExtensionGrid stack, int count)
        {
            stack.Clear();
            for (int i = 0; i < count; i++)
            {
                stack.Add(Icon(LineAwesome.Random).XXLarge(), " This is the Title " + i, TextBlock("Space for content"), () => Toast().Information("Clicked"));
            }
        }

        private void SetChildrenSmall(ExtensionGrid stackSmall, int count)
        {
            stackSmall.Clear();
            for (int i = 0; i < count; i++)
            {
                stackSmall.Add(Icon(LineAwesome.Random).XXLarge(), " This is the Title " + i, () => Toast().Information("Clicked"));
            }
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}