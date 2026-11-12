using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Layout, Order = 90, Icon = UIcons.HorizontalRule, Description = "A rule, with optional text in the middle")]
    public class HorizontalSeparatorSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public HorizontalSeparatorSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(HorizontalSeparatorSample), UIcons.Minus, "A visual separator for components")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A HorizontalSeparator visually divides content into groups. It can optionally contain text or other components to label the group it introduces."),
                    TextBlock("The content can be aligned to the left, center, or right of the line."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use separators to provide structure to long forms or pages. Keep labels short and concise. Use them sparingly; too many separators can clutter the UI. Ensure the labels accurately describe the section that follows."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Text Alignment"),
                    HorizontalSeparator("Center Aligned (Default)"),
                    HorizontalSeparator("Left Aligned").Left(),
                    HorizontalSeparator("Right Aligned").Right(),
                    SampleSubTitle("Themed and Custom Content"),
                    VStack().Children(
                        HorizontalSeparator("Primary Color").Primary(),
                        HorizontalSeparator(HStack().Children(
                            Icon(UIcons.Info).PaddingRight(8.px()),
                            TextBlock("Information Section").SemiBold()
                        )).Primary().Left()
                    ),
                    SampleSubTitle("Empty Separator"),
                    TextBlock("A simple line without any label:"),
                    HorizontalSeparator("")
                )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("VerticalSeparator is the same rule turned on its side, for the items of a row: it stretches to the row's height and can carry a short label or any component, such as the \"or\" between two halves of a form."),
                    SampleSubTitle("In a row"),
                    HStack().Gap(8.px()).AlignItemsCenter().Children(TextBlock("Home"), VerticalSeparator(), TextBlock("Docs"), VerticalSeparator(), TextBlock("Blog")),
                    SampleSubTitle("With a label"),
                    HStack().H(120).Children(
                        VStack().Grow().Children(TextBlock("Sign in with a password").SemiBold(), TextBox().SetPlaceholder("Email"), TextBox().Password()),
                        VerticalSeparator("or"),
                        VStack().Grow().Children(TextBlock("Sign in with a link").SemiBold(), Button("Email me a link").Primary())),
                    SampleSubTitle("Label position and tone"),
                    HStack().H(100).Gap(32.px()).Children(
                        VerticalSeparator("top").Top(), VerticalSeparator("center"), VerticalSeparator("bottom").Bottom(),
                        VerticalSeparator("tone").Primary(), VerticalSeparator(Icon(UIcons.Info)))
                )).SetTitle("Vertical")))
               .SeeAlso(typeof(StackSample), typeof(SectionStackSample), typeof(SidebarSeparatorSample), typeof(CardSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
