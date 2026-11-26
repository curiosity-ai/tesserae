using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    SampleSubTitle("@ and filters"),
                    TextBlock("Type @ for places to search in and assistants to ask, or filetype: and ext: to narrow a search. A chip keeps the one door it belongs to.").Small().Secondary().PB(8),
                    WithChips(SearchAskBox(Placeholder).Layout(SearchAskBox.DoorLayout.Stacked), "Stacked: the door that goes gives its row to the other")))
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
            var said = TextBlock("Enter searches. Ctrl+Enter asks. Type @ for more.").Small().Secondary();

            var box = Configure(SearchAskBox(Placeholder), said);

            return VStack().WS().AlignItemsCenter().PT(48).PB(48).Children(
                TextBlock("What are you looking for?").Large().SemiBold().PB(20),
                box.MaxWidth(720.px()).WS(),
                said.PT(12));
        }

        private static IComponent WithChips(SearchAskBox box, string title)
        {
            var said = TextBlock(" ").Small().Secondary();

            return VStack().WS().PB(20).Children(
                TextBlock(title).Small().Secondary().PB(6),
                Configure(box, said).MaxWidth(720.px()).WS(),
                said.PT(6));
        }

        //Two places to search in, three assistants to ask - fetched, as a host would - and two filters.
        private static SearchAskBox Configure(SearchAskBox box, TextBlock said)
        {
            return box
               .Mentions(
                    new SearchAskBox.Mention("files",    "Files",     SearchAskBox.Door.Search, UIcons.Document, "Only files"),
                    new SearchAskBox.Mention("webpages", "Web pages", SearchAskBox.Door.Search, UIcons.Globe,    "Only web pages"))
               .Mentions(LoadAssistantsAsync)
               .Filters(
                    new SearchAskBox.Filter("filetype", "File type", new[] { "filetype", "kind" }, new[] { "PDF", "Word", "Excel", "PowerPoint", "Image", "Email" }, UIcons.Apps, "Filter by file type"),
                    new SearchAskBox.Filter("ext",      "Extension", new[] { "ext", "extension" }, new[] { "pdf", "docx", "xlsx", "pptx", "png", "msg" },           UIcons.FileCode, "Filter by file extension"))
               .OnSearch((s, text) => said.Text = "Search: " + Describe(text, s.Chips))
               .OnAsk((s, text)    => said.Text = "Ask " + (s.SelectedMentions.FirstOrDefault()?.Name ?? "the default assistant") + ": " + text);
        }

        private static async Task<IEnumerable<SearchAskBox.Mention>> LoadAssistantsAsync()
        {
            await Task.Delay(150);

            return new[]
            {
                new SearchAskBox.Mention("legal",    "Legal",           SearchAskBox.Door.Ask, UIcons.Scale,     "Contracts, clauses and obligations"),
                new SearchAskBox.Mention("quality",  "Quality",         SearchAskBox.Door.Ask, UIcons.Badge,     "Audits, findings and suppliers"),
                new SearchAskBox.Mention("research", "Research",        SearchAskBox.Door.Ask, UIcons.Flask,     "Reads widely and cites its sources")
            };
        }

        private static string Describe(string text, IReadOnlyList<SearchAskBox.Chip> chips)
        {
            var narrowed = chips.Count == 0 ? "" : " (" + string.Join(", ", chips.Select(c => c.Mention is object ? "@" + c.Mention.Name : c.Filter.Id + ":" + c.Value)) + ")";
            return (string.IsNullOrEmpty(text) ? "everything" : text) + narrowed;
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
