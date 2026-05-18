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
                    LeftSide = new[]
                    {
                        Button(UIcons.Rocket).OnClick(() => Toast().Success("Lift off 🚀"))
                    },
                    RightSide = new[]
                    {
                        attBtnForSearch
                    } 
                },
                SuggestionsFetcher = async (query) =>
                {
                    if (string.IsNullOrWhiteSpace(query)) return Array.Empty<OmniBox.OmniBoxSuggestionItem>();

                    await Task.Delay(150); // Simulate network

                    var items = new List<OmniBox.OmniBoxSuggestionItem>();
                    var q = query.ToLower();

                    if ("dataset / curiosity-prod".Contains(q) || "dataset / tesserae-docs".Contains(q) || "dataset / build-logs".Contains(q))
                    {
                        items.Add(new OmniBox.OmniBoxSuggestionItem(TextBlock("dataset / curiosity-prod"), Icon(UIcons.Table), Icon(UIcons.Check).Foreground(Theme.Primary.Foreground), null, "DATASETS"));
                        items.Add(new OmniBox.OmniBoxSuggestionItem(TextBlock("dataset / tesserae-docs"), Icon(UIcons.Table), null, null, "DATASETS"));
                        items.Add(new OmniBox.OmniBoxSuggestionItem(TextBlock("dataset / build-logs"), Icon(UIcons.Table), Icon(UIcons.Check).Foreground(Theme.Primary.Foreground), null, "DATASETS"));
                    }

                    items.Add(new OmniBox.OmniBoxSuggestionItem(TextBlock("a-model / Document.v3"), Icon(UIcons.Document), null, null, "SCHEMAS"));
                    items.Add(new OmniBox.OmniBoxSuggestionItem(TextBlock("a-model / Embedding.v1"), Icon(UIcons.Document), null, null, "SCHEMAS"));

                    return items.ToArray();
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
                var snapInfo = q.Snaps != null && q.Snaps.Length > 0
                    ? $" — snaps: {string.Join(", ", q.Snaps.Select(sn => sn.SnapId))}"
                    : "";
                Toast().Information($"Searched for: {q.RawQuery} (Parsed into {q.Tokens?.Count ?? 0} tokens){snapInfo}");
            })
            .RegisterSnaps(
                new OmniBox.SnapHandler("docs", "Docs", new[] { "docs", "documentation" }, Icon(UIcons.Book), "Search the documentation"),
                new OmniBox.SnapHandler("wiki", "Wikipedia", new[] { "wiki", "wikipedia" }, Icon(UIcons.Globe), "Search Wikipedia"),
                new OmniBox.SnapHandler("code", "Code", new[] { "code", "src", "source" }, Icon(UIcons.FileCode), "Search source code"),
                new OmniBox.SnapHandler("ai", "AI Assist", new[] { "ai", "ask" }, Icon(UIcons.MagicWand), "Switch to AI search (exclusive)", exclusive: true))
            .SetKeyboardShortcut("Ctrl", "K")
            .SetSearchText("potato AND ( tomato OR banana) AND NOT apple");

            var fileDropAreaOnSearch = FileDropArea(searchModeSample).OnFilesDropped((s, files) =>
            {
                Toast().Information($"Dropped files on search box: {string.Join(", ", files.Select(f => f.name))}");
            }).SetAccepts("*");

            searchModeSample.InlineFilterChips.Add(new OmniBox.InlineFilterChip("Tag: Red", "var(--tss-danger-background-color)", "var(--tss-danger-foreground-color)"));
            searchModeSample.InlineFilterChips.Add(new OmniBox.InlineFilterChip("Author: Jules", onClick:(_)=> Toast().Success("hi!")));
            searchModeSample.InlineFilterChips.Add(new OmniBox.InlineFilterChip(Button("IComponent")));
            searchModeSample.SetSearchRightText("124 results");

            attBtnForSearch.OnClick((s, e) => fileDropAreaOnSearch.OpenFileSelection());

            var attBtnForChat = Button(UIcons.PaperclipVertical).Tooltip("Add attachment");

            var chatModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Ask me anything",
                ChatFooter = new OmniBox.FooterItems
                {
                    LeftSide = new[]
                    {
                        Button(UIcons.Rocket).OnClick(() => Toast().Success("Lift off 🚀"))
                    },
                    RightSide = new[]
                    {
                        attBtnForChat
                    }
                }

            })
            .WS()
            .SetModels(
                new OmniBox.ModelOption("Opus 4.7"),
                new OmniBox.ModelOption("Opus 4.7", "1M"),
                new OmniBox.ModelOption("Sonnet 4.6"),
                new OmniBox.ModelOption("Haiku 4.5"))
            .SetThinkingEffort(OmniBox.ThinkingEffort.High)
            .OnModelChanged((s, model, effort) =>
            {
                Toast().Information($"Selected {model.Name} with {effort} thinking effort");
            })
            .OnChat((s, q) =>
            {
                s.IsGenerating = true;
                window.setTimeout((_) =>
                {
                    if (s.IsGenerating) // Make sure it wasn't cancelled
                    {
                        s.IsGenerating = false;
                        Toast().Information(q.Text);
                    }
                }, 5000);
            })
            .OnStop(s =>
            {
                s.IsGenerating = false;
            });

            var lockedModel = new OmniBox.ModelOption("Sonnet 4.6");
            var lockedChatModeSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "This chat has a locked model"
            })
            .WS()
            .LockModel(lockedModel)
            .SetThinkingEffort(OmniBox.ThinkingEffort.Medium);

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
                s.IsGenerating = true;
                window.setTimeout((_) => 
                { 
                    if (s.IsGenerating) // Make sure it wasn't cancelled
                    {
                        s.IsGenerating = false;
                        Toast().Information(q.Text);
                    }
                }, 5000);
            })
            .OnStop(s =>
            {
                s.IsGenerating = false;
            })
            .WithHistory(async () => {
                return new OmniBox.SearchQuery[0];
            })
            .SetKeyboardShortcut("Ctrl", "Shift", "K");


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
                        Label("Chat with locked model").SetContent(lockedChatModeSample.WS()).MT(6),
                        Label("Search & Chat").SetContent(searchAndChatModeSample.WS()).MT(6)
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
