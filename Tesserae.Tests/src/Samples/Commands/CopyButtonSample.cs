using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 110, Icon = UIcons.CopyAlt, Description = "Copies text and confirms right where you clicked")]
    public class CopyButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CopyButtonSample()
        {
            var link = TextBox("https://tesserae.curiosity.ai/s/7f3a9c").ReadOnly().Grow();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(CopyButtonSample), UIcons.CopyAlt, "Copy to the clipboard, with the confirmation in place")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("CopyButton copies a text and says so where the user is looking: the copy icon turns into a check, a label (if the button has one) morphs to \"Copied\", and both turn back after a moment. The confirmation is announced to screen readers, so no toast is needed."),
                        TextBlock("Give it a fixed string, or a function that is read at the moment of the click for text that changes.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("With a label"),
                        HStack().Wrap().AlignItemsCenter().Children(
                            CopyButton("npm i tesserae", "Copy command"),
                            CopyButton("sk_live_51H...", "Copy key").Primary().CopiedText("Key copied")),
                        SampleSubTitle("Icon only, beside a field"),
                        HStack().WS().AlignItemsCenter().Children(link, CopyButton(() => link.Text).Tooltip("Copy link")),
                        SampleSubTitle("Inside a code block"),
                        HStack().WS().AlignItemsCenter().JustifyContent(ItemJustify.Between).PL(12).PR(4).Style(s => { s.background = "var(--tss-secondary-background-color)"; s.borderRadius = "var(--tss-border-radius-md)"; }).Children(
                            TextBlock("dotnet add package Tesserae").Style(s => s.fontFamily = "ui-monospace, Consolas, monospace"),
                            CopyButton("dotnet add package Tesserae").NoBackground().Tooltip("Copy")))).SetTitle("Usage")))
               .SeeAlso(typeof(ButtonSample), typeof(TextBoxSample), typeof(MarkdownBlockSample), typeof(MicroInteractionsSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
