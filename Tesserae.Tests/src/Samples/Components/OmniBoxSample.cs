using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 21, Icon = UIcons.SearchBar)]
    public class OmniBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public OmniBoxSample()
        {
            var searchModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch =    "Type something like: potato AND ( tomato OR banana) AND NOT apple",
            })
            .WS()
            .WithHistory(async () => {
                return new[] 
                {
                    OmniBox.ParseQuery("apple"),
                    OmniBox.ParseQuery("orange"),
                    OmniBox.ParseQuery("tomato"),
                    OmniBox.ParseQuery("banana"),
                    OmniBox.ParseQuery("potato AND ( tomato OR banana) AND NOT apple"),
                };
            })
            .OnSearch((s, q) =>
            {
                Toast().Information($"Searched for: {q.RawQuery} (Parsed into {q.Tokens?.Count ?? 0} tokens)");
            })
            .SetSearchText("potato AND ( tomato OR banana) AND NOT apple");

            var chatModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat  =    "Ask me anything",
            })
            .WS()
            .OnChat((s, q) =>
            {
                Toast().Information(q.Text);
            });


            var searchAndChatModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.SearchAndChat)
            {
                PlaceholderChat   = "Ask me anything",
                PlaceholderSearch = "Search for anything",
                ChatFooterLeftSide = new IComponent[]
                {
                    Dropdown().Searchable().Items(DropdownItem("Consult Documents", icon: UIcons.Book).Selected(),
                                                  DropdownItem("Find a flight", icon: UIcons.AirplaneJourney),
                                                  DropdownItem("Book a hotel", icon: UIcons.Hotel))
                } 
            })
            .WS()
            .OnSearch((s, q) =>
            {
                Toast().Information($"Searched for: {q.RawQuery} (Parsed into {q.Tokens?.Count ?? 0} tokens)");
            })
            .OnChat((s, q) =>
            {
                Toast().Information(q.Text);
            })
            .WithHistory(async () => {
                return new OmniBox.SearchQuery[0];
            });


            _content = SectionStack()
               .Title(SampleHeader(nameof(OmniBoxSample)))
               .Section(VStack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Omnibox provides a powerful input field for switching between a chat and a search interaction. For search, it also provides support for parsing and visual rendering of logical operators like AND, OR, NOT, parenthesis, and quotes.")))
               .Section(VStack().WS().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Modes"),
                        Label("Search").SetContent(searchModeSample),
                        Label("Chat").SetContent(chatModeSample.MT(6)),
                        Label("Search & Chat").SetContent(searchAndChatModeSample.MT(6)),
                    SampleSubTitle("Customization"),
                        Label("Disabled").Disabled().SetContent(OmniBox(new OmniBox.Config(OmniBox.Mode.Search) { PlaceholderSearch = "Search disabled" }).Disabled())
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
