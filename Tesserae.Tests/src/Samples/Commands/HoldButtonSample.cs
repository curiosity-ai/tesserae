using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 90, Icon = UIcons.HandPaper, Description = "Press and hold to confirm an irreversible action")]
    public class HoldButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public HoldButtonSample()
        {
            var log = TextBlock("Nothing confirmed yet.").Secondary();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(HoldButtonSample), UIcons.HandPaper, "A button that has to be held to act")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("HoldButton guards a destructive or irreversible action without a confirmation dialog: the hold is the confirmation. A fill sweeps across the button while it is held; letting go early drains it back from wherever it reached, and holding to the end swaps the icon for a check and fires OnConfirmed."),
                        TextBlock("It works with the pointer (mouse, touch, pen) and the keyboard: hold Space or Enter while the button has focus.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Say what holding does in the label: \"Hold to delete\", not \"Delete\"."),
                        SampleDo("Keep the hold between about one and two seconds - long enough to be deliberate."),
                        SampleDont("Use it for everyday actions; it is friction on purpose."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Tones"),
                        HStack().Wrap().AlignItemsCenter().Children(
                            HoldButton("Hold to confirm").OnConfirmed(() => log.Text = "Confirmed: default"),
                            HoldButton("Hold to publish").Primary().SetIcon(UIcons.PaperPlane).ConfirmedText("Published").OnConfirmed(() => log.Text = "Confirmed: publish"),
                            HoldButton("Hold to delete", 1600).Danger().SetIcon(UIcons.Trash).ConfirmedText("Deleted").OnConfirmed(() => log.Text = "Confirmed: delete")),
                        log.PT(8),
                        SampleSubTitle("Stays confirmed until reset"),
                        TextBlock("ResetAfter(0) keeps the confirmed state (and the button disabled) until Reset() is called, for an action that cannot be repeated.").MB(8),
                        HoldButtonThatStays())).SetTitle("Usage")))
               .SeeAlso(typeof(ConfirmButtonSample), typeof(ButtonSample), typeof(DialogSample), typeof(MicroInteractionsSample));
        }

        private static IComponent HoldButtonThatStays()
        {
            var hold  = HoldButton("Hold to revoke all keys", 2000).Danger().SetIcon(UIcons.Key).ConfirmedText("Keys revoked").ResetAfter(0);
            var reset = Button("Reset").OnClick(() => hold.Reset());
            return HStack().AlignItemsCenter().Children(hold, reset);
        }

        public HTMLElement Render() => _content.Render();
    }
}
