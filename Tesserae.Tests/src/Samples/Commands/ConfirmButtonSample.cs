using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 100, Icon = UIcons.CheckDouble, Description = "Click twice to confirm, in place of a dialog")]
    public class ConfirmButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ConfirmButtonSample()
        {
            var items = VStack().WS().Children(Row("design-tokens.json"), Row("release-notes.md"), Row("old-logo.svg"));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ConfirmButtonSample), UIcons.CheckDouble, "A two-step button that confirms in place")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ConfirmButton asks for a second click instead of opening a dialog. The first click arms it: the label morphs to the confirmation prompt, the button turns to the danger tone and a bar drains along its foot. A second click before the bar runs out confirms it and fires OnConfirmed; letting it run out, pressing Escape or tabbing away disarms it."),
                        TextBlock("The label change animates the button's width, so the row around it eases instead of jumping.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Use it for small destructive actions that can be redone: removing a row, discarding a draft."),
                        SampleDont("Use it where a mistake is expensive - a hold (HoldButton) or a dialog asks for more intent."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Basic"),
                        HStack().Wrap().AlignItemsCenter().Children(
                            ConfirmButton("Remove item", "Click again to confirm", "Removed").SetIcon(UIcons.Trash),
                            ConfirmButton("Discard draft", "Sure?", "Discarded").Timeout(5000),
                            ConfirmButton("Sign out everywhere", "Confirm sign out")),
                        SampleSubTitle("In a list"),
                        items)).SetTitle("Usage")))
               .SeeAlso(typeof(HoldButtonSample), typeof(ButtonSample), typeof(DialogSample), typeof(MicroInteractionsSample));
        }

        private static IComponent Row(string name)
        {
            var row = HStack().WS().AlignItemsCenter().JustifyContent(ItemJustify.Between).PL(12).PR(4).H(44);
            row.Children(
                TextBlock(name),
                ConfirmButton("Remove", "Remove?").SetIcon(UIcons.Trash).OnConfirmed(() => row.DismissWithSwipe())); 
            return row;
        }

        public HTMLElement Render() => _content.Render();
    }
}
