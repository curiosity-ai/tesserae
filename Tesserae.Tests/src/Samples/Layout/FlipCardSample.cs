using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Layout, Order = 100, Icon = UIcons.CardsBlank, Description = "A card with two faces that turns over in 3D")]
    public class FlipCardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public FlipCardSample()
        {
            var plan = FlipCard(
                VStack().Gap(4.px()).Children(
                    TextBlock("Team plan").SemiBold().Medium(),
                    TextBlock("$24 per seat, per month").Secondary(),
                    TextBlock("Click the card to see what is included.").Small().Secondary().PT(16)),
                VStack().Gap(4.px()).Children(
                    TextBlock("Included").SemiBold(),
                    TextBlock("Unlimited projects"),
                    TextBlock("Single sign-on"),
                    TextBlock("Audit log, 90 days"))).FlipOnClick().W(260);

            var status = TextBlock("Front").Secondary();

            var question = FlipCard(
                VStack().Children(TextBlock("Why measure an accordion's height instead of animating max-height?").SemiBold()),
                VStack().Children(TextBlock("A hard-coded max-height eases at the wrong speed and clips the moment the content grows. Measuring gives the real height to transition to.")))
               .Vertical().W(320).OnFlip(back => status.Text = back ? "Back" : "Front");

            var controlled = FlipCard(
                VStack().Children(TextBlock("Front face").SemiBold(), TextBlock("A button inside a face does not flip the card:").Small().PT(8), Button("Just a button").OnClick(() => Toast().Information("The button ran"))),
                VStack().Children(TextBlock("Back face").SemiBold(), TextBlock("Flipped from code.").Small().PT(8))).FlipOnClick().W(260);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(FlipCardSample), UIcons.CardsBlank, "Two faces, one card")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("FlipCard holds two components and turns over in 3D to show the other one: a summary and its details, a question and its answer. The card is as tall as its taller face, so nothing around it moves when it flips, and the face turned away is hidden from screen readers and the tab order."),
                        TextBlock("FlipOnClick() turns it on a click (but not on a button, link or field inside it) and on Enter or Space; Flip() and IsFlipped turn it from code; Vertical() turns it top over bottom. Under reduced motion the faces crossfade.").PT(8))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Flip on click"),
                        plan,
                        SampleSubTitle("Turned from code, vertically"),
                        HStack().AlignItemsCenter().Gap(16.px()).Children(question, VStack().Children(Button("Flip").OnClick(() => question.Flip()), status.PT(8))),
                        SampleSubTitle("Controls inside a face"),
                        controlled)).SetTitle("Usage")))
               .SeeAlso(typeof(CardSample), typeof(AccordionSample), typeof(MicroInteractionsSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
