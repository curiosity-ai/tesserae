using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Search, Order = 45, Icon = UIcons.DoorOpen, Description = "One field with a Search and an Ask door")]
    public class SearchAskBoxSample : IComponent, ISample
    {
        private const string Placeholder = "Search or ask about your workspace";

        private readonly IComponent _content;

        public SearchAskBoxSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SearchAskBoxSample), UIcons.DoorOpen, "Type once, then choose: search for it or ask about it")
               .FlatSection(VStack().WS().Children(
                    SampleSubTitle("On a home page"),
                    HomePage()))
               .FlatSection(VStack().WS().Children(
                    SampleSubTitle("Where the doors sit"),
                    Example("Side by side (default)", SearchAskBox(Placeholder), "supplier audit 2025"),
                    Example("Stacked", SearchAskBox(Placeholder).Layout(SearchAskBox.DoorLayout.Stacked), "Which contracts renew before March?"),
                    Example("Outside the frame", SearchAskBox(Placeholder).Layout(SearchAskBox.DoorLayout.Outside), "")))
               .FlatSection(VStack().WS().Children(
                    SampleSubTitle("Keys on the doors (optional, .ShowKeys())"),
                    Example("Side by side", SearchAskBox(Placeholder).ShowKeys(), "supplier audit 2025"),
                    Example("Stacked", SearchAskBox(Placeholder).Layout(SearchAskBox.DoorLayout.Stacked).ShowKeys(), "Which contracts renew before March?"),
                    Example("Outside the frame", SearchAskBox(Placeholder).Layout(SearchAskBox.DoorLayout.Outside).ShowKeys(), "open tickets for Acme")))
               .FlatSection(VStack().WS().Children(
                    SampleSubTitle("A door filled at rest"),
                    Example("Neither (default): both grey, blue under the pointer", SearchAskBox(Placeholder), "open tickets for Acme"),
                    Example("Ask", SearchAskBox(Placeholder).Primary(SearchAskBox.PrimaryDoor.Ask), "open tickets for Acme"),
                    Example("Search", SearchAskBox(Placeholder).Primary(SearchAskBox.PrimaryDoor.Search), "open tickets for Acme")))
               .FlatSection(VStack().WS().Children(
                    SampleSubTitle("Nothing to answer with"),
                    Example("Ask disabled, with the reason as its tooltip", SearchAskBox(Placeholder).AskEnabled(false, "No AI model is configured"), "quality manual")))
               .FlatSection(VStack().WS().Children(
                    SampleSubTitle("Long questions"),
                    Example("Grows to six lines, then scrolls. Shift+Enter starts a new line.", SearchAskBox(Placeholder),
                        "Compare the audit findings for our three largest suppliers this year with last year's, and tell me which of them got worse and in what area.")))
               .SeeAlso(typeof(OmniBoxSample), typeof(SearchBoxSample), typeof(TextAreaSample));
        }

        public HTMLElement Render() => _content.Render();

        private static IComponent HomePage()
        {
            var said = TextBlock("Enter searches. Ctrl+Enter asks.").Small().Secondary();

            var box = SearchAskBox(Placeholder)
               .OnSearch((_, text) => said.Text = "Search: " + text)
               .OnAsk((_, text)    => said.Text = "Ask: " + text);

            return VStack().WS().AlignItemsCenter().PT(48).PB(48).Children(
                TextBlock("What are you looking for?").Large().SemiBold().PB(20),
                box.MaxWidth(720.px()).WS(),
                said.PT(12));
        }

        private static IComponent Example(string title, SearchAskBox box, string text)
        {
            var said = TextBlock(" ").Small().Secondary();

            box.OnSearch((_, t) => said.Text = "Search: " + t)
               .OnAsk((_, t)    => said.Text = "Ask: " + t);

            if (!string.IsNullOrEmpty(text)) box.Text = text;

            return VStack().WS().PB(20).Children(
                TextBlock(title).Small().Secondary().PB(6),
                box.MaxWidth(720.px()).WS(),
                said.PT(6));
        }
    }
}
