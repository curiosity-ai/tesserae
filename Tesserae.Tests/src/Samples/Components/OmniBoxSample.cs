using System;
using System.Collections.Generic;
using System.Linq;
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
            var attBtnForSearch = Button(UIcons.PaperclipVertical).Tooltip("Add attachment");
            var searchModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch =    "Type something like: potato AND ( tomato OR banana) AND NOT apple",
                SearchFooter = new OmniBox.FooterItems
                {
                    RightSide = new[]
                    {
                        attBtnForSearch
                    } 
                }
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

            var fileDropAreaOnSearch = FileDropArea(searchModeSample).OnFilesDropped((s, files) =>
            {
                Toast().Information($"Dropped files on search box: {string.Join(", ", files.Select(f => f.name))}");
            }).SetAccepts("*");

            searchModeSample.InlineFilterChips.Add(new OmniBox.InlineFilterChip("Tag: Red", "var(--tss-danger-background-color)", "var(--tss-danger-foreground-color)"));
            searchModeSample.InlineFilterChips.Add(new OmniBox.InlineFilterChip("Author: Jules"));
            searchModeSample.SetSearchRightText("124 results");

            attBtnForSearch.OnClick((s, e) => fileDropAreaOnSearch.OpenFileSelection());

            var attBtnForChat = Button(UIcons.PaperclipVertical).Tooltip("Add attachment");
            
            var chatModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat  =    "Ask me anything",
                ChatFooter = new OmniBox.FooterItems
                {
                    RightSide = new[]
                    {
                        attBtnForChat
                    } 
                }

            })
            .WS()
            .OnChat((s, q) =>
            {
                Toast().Information(q.Text);
            });

            var fileDropAreaOnChat = FileDropArea(chatModeSample).OnFilesDropped((s, files) =>
            {
                Toast().Information($"Dropped files on chat box: {string.Join(", ", files.Select(f => f.name))}");
            }).SetAccepts("*");

            attBtnForChat.OnClick((s, e) => fileDropAreaOnChat.OpenFileSelection());

            var searchAndChatModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.SearchAndChat)
            {
                PlaceholderChat   = "Ask me anything",
                PlaceholderSearch = "Search for anything",
                ChatFooter = new OmniBox.FooterItems { LeftSide = new IComponent[]
                {
                    Dropdown().ML(16).Searchable().Items(DropdownItem("Consult Documents", icon: UIcons.Book).Selected(),
                                                  DropdownItem("Find a flight", icon: UIcons.AirplaneJourney),
                                                  DropdownItem("Book a hotel", icon: UIcons.Hotel))
                }
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


            var toggle = Toggle("Disabled").OnChange((s, e) =>
            {
                var disabled = s.IsChecked;
                if (disabled)
                {
                    searchModeSample.Disabled();
                    chatModeSample.Disabled();
                    searchAndChatModeSample.Disabled();
                }
                else
                {
                    searchModeSample.Disabled(false);
                    chatModeSample.Disabled(false);
                    searchAndChatModeSample.Disabled(false);
                }
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(OmniBoxSample), UIcons.Search, "An omnibox search component")
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                    toggle.MB(16),
                    TextBlock("Omnibox provides a powerful input field for switching between a chat and a search interaction. For search, it also provides support for parsing and visual rendering of logical operators like AND, OR, NOT, parenthesis, and quotes."))).SetTitle("Overview")))
               .FlatSection(VStack().WS().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Modes"),
                        Label("Search (with FileDropArea)").SetContent(fileDropAreaOnSearch.WS()),
                        Label("Chat").SetContent(fileDropAreaOnChat.WS()).MT(6),
                        Label("Search & Chat").SetContent(searchAndChatModeSample.WS()).MT(6)
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
