using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.AddressCard)]
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
                )).SetTitle("Usage")));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
