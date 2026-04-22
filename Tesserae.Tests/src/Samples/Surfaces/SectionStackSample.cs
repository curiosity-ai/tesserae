using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 20, Icon = UIcons.BorderAll)]
    public class SectionStackSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SectionStackSample()
        {
            var stack = SectionStack();

            _content = Stack().Children(SectionStack().Title(SampleHeader(nameof(SectionStackSample)))
                   .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("SectionStack is a high-level layout component designed for creating long-form pages or detailed views. It organizes content into distinct vertical sections, typically with a header and footer, providing a consistent structure for complex information architectures.")))
                   .Section(Stack().Children(
                        SampleTitle("Best Practices"),
                        TextBlock("Use SectionStack for the main content area of your pages. Organize related components into distinct sections to improve readability and scanability. Utilize the 'Title' and 'Commands' features of the SectionStack to provide context and actions at the top of the page.")))
                   .Section(Stack().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Dynamic Section Generation"),
                        Label("Number of sections:").SetContent(Slider(5, 0, 10, 1).OnInput((s, e) => SetChildren(stack, s.Value))))),
                stack);
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
                    TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").Width(50.percent()),
                    TextBlock("No Wrap").SmallPlus(),
                    TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").NoWrap().Width(50.percent())
                ));
            }
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}