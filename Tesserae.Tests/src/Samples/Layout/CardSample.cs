using System;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Layout, Order = 60, Icon = UIcons.AddressCard)]
    public class CardSample : IComponent, ISample
    {
        private IComponent _content;

        public CardSample()
        {
            _content = SectionStack().Secondary()
                .SampleTitle(typeof(CardSample), UIcons.AddressCard, "A card component with optional headers and footers")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Cards are surfaces that display content and actions on a single topic."),
                    TextBlock("They should be easy to scan for relevant and actionable information. Elements, like text and images, should be placed on them in a way that clearly indicates hierarchy."),
                    TextBlock("Cards can contain different types of components. They can be used to show a list of items, a single item, or a mix of both.")
                )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    Stack().Horizontal().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("Use cards to group related information."),
                            SampleDo("Keep the information on a card concise."),
                            SampleDo("Use clear, concise, and easy to understand language.")
                        ),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Don't use cards to display unrelated information."),
                            SampleDont("Don't overload a card with too much information."),
                            SampleDont("Don't use cards to display a list of items.")
                        )
                    )
                )).SetTitle("Best Practices")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Basic Card").SemiBold().PT(8),
                    Card(TextBlock("This is a basic card.")),
                    TextBlock("Card with Header").SemiBold().PT(16),
                    Card(TextBlock("This is a card with a header.")).SetTitle(HStack().WS().AlignItemsCenter().Children(TextBlock("Header").SemiBold(), Tag("Sample Card").Primary().ML(8))),
                    TextBlock("Card with Header and Footer").SemiBold().PT(16),
                    Card(TextBlock("This is a card with a header and a footer.")).SetTitle("Header").SetFooter(Button("Action").Primary()),
                    TextBlock("Compact Card").SemiBold().PT(16),
                    Card(TextBlock("This is a compact card.")).SetTitle("Header").Compact(),
                    TextBlock("Hover Card").SemiBold().PT(16),
                    Card(TextBlock("This card has hover effect.")).HoverColor(),
                    TextBlock("Custom Background").SemiBold().PT(16),
                    Card(TextBlock("This card has a custom background.")).BackgroundColor(Theme.Primary.Background)
                )).SetTitle("Usage")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("SetTitle(text) takes an optional icon drawn just before the title — what the card is about, said in a glyph as well as in words. Pass a color for the icon to give it one of its own; without one it takes the title's."),
                    TextBlock("Titled cards with icons").SemiBold().PT(16),
                    Card(TextBlock("The icon takes the title's own color.")).SetTitle("Documents", UIcons.Folder),
                    Card(TextBlock("A color of its own says something the words don't.")).SetTitle("Rejected items", UIcons.CircleXmark, Theme.Colors.Red600).MT(8),
                    Card(TextBlock("Any UIcons glyph works, in any weight.")).SetTitle("In my scope", UIcons.Inbox, Theme.Colors.Purple600, UIconsWeight.Solid).MT(8)
                )).SetTitle("Icons in the title", UIcons.AddressCard, Theme.Colors.Blue600)))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("AI() marks a card as a model's output: a faint purple-to-blue tint over whatever background it had, an accent border, and a soft shadow in the same hue. The tint is deliberately light - a card is a large surface, and the point is to say where the content came from, not to colour the page."),
                    TextBlock("It works on a plain card and on one with a header and a footer; the strips take one step more tint than the content, the same way the default card's do.").MT(8),
                    TextBlock("Plain").SemiBold().PT(16),
                    Card(TextBlock("Summarised from 12 documents.")).AI(),
                    TextBlock("With a header and a footer").SemiBold().PT(16),
                    Card(TextBlock("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs.")).AI()
                        .SetTitle(HStack().WS().AlignItemsCenter().Gap(8.px()).Children(AIIcon(), TextBlock("What went wrong on line 3").SemiBold().AI(), AIBadge().ML(4)))
                        .SetFooter(HStack().WS().AlignItemsCenter().Gap(8.px()).Children(Button("Ask a follow-up").AI().NoMargin(), Button("Show sources").AISubtle(withSparklesIcon: false).SetIcon(UIcons.Books).NoMargin())),
                    TextBlock("With hover").SemiBold().PT(16),
                    Card(TextBlock("A pressable AI card lifts to the stronger tint.")).AI().HoverColor()
                )).SetTitle("AI output", UIcons.Sparkles, Theme.Colors.Purple600)))
                .SeeAlso(typeof(SectionStackSample), typeof(ResourceCardSample), typeof(AccordionSample), typeof(MasonrySample), typeof(CardPivotSample), typeof(AIVariantsSample));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
