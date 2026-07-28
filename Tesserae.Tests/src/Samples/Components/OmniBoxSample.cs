using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 21, Icon = UIcons.SearchBar)]
    public class OmniBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // Every box on the page, so the "Disabled" toggle in the overview can switch all of them at once.
        private readonly List<OmniBox> _allBoxes = new List<OmniBox>();

        public OmniBoxSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(OmniBoxSample), UIcons.Search, "One input for search and chat, feature by feature")
               .FlatSection(VStack().WS().Children(Overview()))
               .FlatSection(VStack().WS().Children(Modes()))
               .FlatSection(VStack().WS().Children(QuerySyntax()))
               .FlatSection(VStack().WS().Children(Suggestions()))
               .FlatSection(VStack().WS().Children(History()))
               .FlatSection(VStack().WS().Children(Snaps()))
               .FlatSection(VStack().WS().Children(FilterSnaps()))
               .FlatSection(VStack().WS().Children(Help()))
               .FlatSection(VStack().WS().Children(InlineChips()))
               .FlatSection(VStack().WS().Children(FooterItems()))
               .FlatSection(VStack().WS().Children(KeyboardShortcut()))
               .FlatSection(VStack().WS().Children(FileDrop()))
               .FlatSection(VStack().WS().Children(Models()))
               .FlatSection(VStack().WS().Children(Generating()))
               .FlatSection(VStack().WS().Children(ToolsAndAgents()))
               .FlatSection(VStack().WS().Children(ContextToAdd()))
               .SeeAlso(typeof(ChatSample), typeof(SearchBoxSample), typeof(ContextCardSample), typeof(ToolCallSample), typeof(CommandPaletteSample), typeof(KeyboardShortcutSample), typeof(FileSelectorAndDropAreaSample));
        }

        // ---------- Section helpers ----------

        // One feature per card: a subtitle, one or two lines saying what to try, then the box itself.
        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        // Registers a box with the page-wide "Disabled" toggle and returns it, so a section can write
        // Track(OmniBox(...)) inline.
        private OmniBox Track(OmniBox box)
        {
            _allBoxes.Add(box);
            return box;
        }

        // ---------- Overview ----------

        private IComponent Overview()
        {
            var toggle = Toggle("Disabled").OnChange((s, e) =>
            {
                foreach (var box in _allBoxes)
                {
                    box.Disabled(s.IsChecked);
                }
            });

            return FeatureCard("Overview", "What OmniBox is",
                "OmniBox is one input that can act as a search box, a chat composer, or both with a toggle between them. On top of the input it layers query parsing (AND / OR / NOT, parentheses, quotes), async suggestions, recent-search history, inline chips, '@' snaps and 'field:' filter snaps, a footer for custom actions, a model selector, a generating state, and a context row. Each section below turns on one of those features on its own, so it is clear which method produces what.",
                toggle.MB(8),
                TextBlock("The toggle above disables every box on this page — a disabled OmniBox keeps its content but stops taking input.").Small().Foreground(Theme.Secondary.Foreground));
        }

        // ---------- Modes ----------

        private IComponent Modes()
        {
            var search = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Search — one line, a search button, no footer"
            })
            .WS()
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            var chat = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Chat — a growing text area with a footer and a send button"
            })
            .WS()
            .OnChat((s, q) => Toast().Information(q.Text)));

            var both = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.SearchAndChat, initialMode: OmniBox.Mode.Chat)
            {
                PlaceholderSearch = "Search & Chat — switch with the toggle on the left of the footer",
                PlaceholderChat   = "Search & Chat — switch with the toggle on the left of the footer"
            })
            .WS()
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}"))
            .OnChat((s, q) => Toast().Information(q.Text)));

            var expanding = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "ExpandOnFocus — click me and the box grows",
                ExpandOnFocus   = true
            })
            .WS()
            .OnChat((s, q) => Toast().Information(q.Text)));

            return FeatureCard("Modes", "Search, Chat and both",
                "The mode is fixed at construction: new OmniBox.Config(OmniBox.Mode.Search | Chat | SearchAndChat). In SearchAndChat the footer gets a toggle between the two, and whatever is typed carries over when switching; initialMode picks the side it starts on. ExpandOnFocus makes a chat box grow while it has focus.",
                Label("Mode.Search").SetContent(search),
                Label("Mode.Chat").SetContent(chat).MT(6),
                Label("Mode.SearchAndChat (starting in chat)").SetContent(both).MT(6),
                Label("Mode.Chat with ExpandOnFocus").SetContent(expanding).MT(6));
        }

        // ---------- Query parsing ----------

        private IComponent QuerySyntax()
        {
            var parsed = TextBlock("").Small().BreakSpaces();

            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "potato AND ( tomato OR banana) AND NOT apple"
            })
            .WS()
            .SetSearchText("potato AND ( tomato OR banana) AND NOT \"granny smith\"")
            .OnSearch((s, q) =>
            {
                parsed.Text = q.Tokens == null || q.Tokens.Count == 0
                    ? "(no tokens)"
                    : string.Join("\n", q.Tokens
                        .Where(t => t.Type != OmniBox.SearchToken.TokenType.Whitespace)
                        .Select(t => $"{t.Type}: {t.Value}"));
            }));

            return FeatureCard("Query syntax", "Operators, grouping and quotes",
                "The search input highlights the boolean operators (AND, OR, NOT), parentheses and quoted phrases as they are typed, and hands OnSearch a SearchQuery whose Tokens are the parsed query. OmniBox.ParseQuery(string) does the same parsing without a component — useful to build the history entries below. Press Enter to see the tokens.",
                box,
                TextBlock("Parsed tokens").SemiBold().Small().MT(8),
                parsed);
        }

        // ---------- Suggestions ----------

        private IComponent Suggestions()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch  = "Start typing — suggestions are fetched async and grouped",
                SuggestionsFetcher = async (query) =>
                {
                    if (string.IsNullOrWhiteSpace(query)) return Array.Empty<OmniBox.OmniBoxSuggestionItem>();

                    await Task.Delay(150); // Simulate network

                    var items = new List<OmniBox.OmniBoxSuggestionItem>
                    {
                        new OmniBox.OmniBoxSuggestionItem(TextBlock("dataset / curiosity-prod"), Icon(UIcons.Table), Icon(UIcons.Check).Foreground(Theme.Primary.Foreground), null, "DATASETS"),
                        new OmniBox.OmniBoxSuggestionItem(TextBlock("dataset / tesserae-docs"), Icon(UIcons.Table), null, null, "DATASETS"),
                        new OmniBox.OmniBoxSuggestionItem(TextBlock("dataset / build-logs"),    Icon(UIcons.Table), Icon(UIcons.Check).Foreground(Theme.Primary.Foreground), null, "DATASETS"),
                        new OmniBox.OmniBoxSuggestionItem(TextBlock("a-model / Document.v3"),   Icon(UIcons.Document), null, null, "SCHEMAS"),
                        new OmniBox.OmniBoxSuggestionItem(TextBlock("a-model / Embedding.v1"),  Icon(UIcons.Document), null, null, "SCHEMAS")
                    };

                    return items.ToArray();
                }
            })
            .WS()
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            return FeatureCard("Suggestions", "Async suggestions with categories",
                "Config.SuggestionsFetcher is an async Func<string, Task<OmniBoxSuggestionItem[]>> called as the user types. Each item takes a content component, a leading icon, an optional right-hand component (a checkmark, a count), an onSelected callback, and a category that groups it under a header. Arrow Up/Down moves through the list, Enter picks the highlighted one.",
                box);
        }

        // ---------- History ----------

        private IComponent History()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Click the clock button on the left to see recent searches"
            })
            .WS()
            .WithHistory(async () =>
            {
                await Task.Delay(100); // Simulate network

                return new[]
                {
                    OmniBox.ParseQuery("apple"),
                    OmniBox.ParseQuery("orange"),
                    OmniBox.ParseQuery("tomato"),
                    OmniBox.ParseQuery("banana"),
                    OmniBox.ParseQuery("potato AND ( tomato OR banana) AND NOT apple")
                };
            })
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            return FeatureCard("History", "Recent searches",
                "WithHistory(Func<Task<SearchQuery[]>>) shows a history button on the left of the search input; clicking it opens the list the fetcher returns, and picking an entry puts it back in the box. Build the entries with OmniBox.ParseQuery so they keep their operator highlighting.",
                box);
        }

        // ---------- Snaps ----------

        private IComponent Snaps()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Type @ to pick a snap — @docs, @wiki, @code, @ai"
            })
            .WS()
            .RegisterSnaps(
                new OmniBox.SnapHandler("docs", "Docs",      new[] { "docs", "documentation" }, Icon(UIcons.Book),     "Search the documentation", exampleValue: "documentation pages"),
                new OmniBox.SnapHandler("wiki", "Wikipedia", new[] { "wiki", "wikipedia" },     Icon(UIcons.Globe),    "Search Wikipedia",         exampleValue: "encyclopedia articles"),
                new OmniBox.SnapHandler("code", "Code",      new[] { "code", "src", "source" }, Icon(UIcons.FileCode), "Search source code",       exampleValue: "src/, repo files"),
                new OmniBox.SnapHandler("ai",   "AI Assist", new[] { "ai", "ask" },             Icon(UIcons.MagicWand), "Switch to AI search (exclusive)", exclusive: true))
            .OnSearch((s, q) =>
            {
                var snapInfo = q.Snaps != null && q.Snaps.Length > 0
                    ? string.Join(", ", q.Snaps.Select(sn => sn.SnapId))
                    : "none";
                Toast().Information($"Searched for: {q.RawQuery} — snaps: {snapInfo}");
            }));

            return FeatureCard("Snaps", "'@' scopes that become chips",
                "RegisterSnaps declares scopes the user reaches by typing @: each SnapHandler has an id, a title, the trigger words that match it, an icon and a description. Committing one turns it into a removable chip in front of the query, and the selected snaps come back on SearchQuery.Snaps. A snap marked exclusive replaces any other (e.g. @ai here).",
                box);
        }

        // ---------- Filter snaps ----------

        private IComponent FilterSnaps()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Type 'ext:', 'lang:' or 'modified:' to autocomplete a filter value"
            })
            .WS()
            .RegisterFilterSnaps(
                new OmniBox.FilterSnapHandler(
                    "ext",
                    "File extension",
                    new[] { "ext", "filetype" },
                    new[] { "cs", "ts", "tsx", "js", "jsx", "json", "md", "css", "html", "py", "rb", "go", "rs", "java", "kt", "swift", "yml", "yaml", "xml" },
                    icon: Icon(UIcons.FileCode),
                    description: "Filter results by file extension",
                    exampleValue: "cs, ts, json…"),
                new OmniBox.FilterSnapHandler(
                    "lang",
                    "Language",
                    new[] { "lang", "language" },
                    async (input) =>
                    {
                        await Task.Delay(120); // Simulate network

                        var all = new[] { "csharp", "typescript", "javascript", "python", "ruby", "go", "rust", "java", "kotlin", "swift", "html", "css" };
                        if (string.IsNullOrEmpty(input)) return all;
                        return all.Where(v => v.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                    },
                    icon: Icon(UIcons.Globe),
                    description: "Filter results by language (async values)",
                    exampleValue: "csharp, typescript…"),
                OmniBox.FilterSnapHandler.TimeRange(
                    "modified",
                    "Modified date",
                    new[] { "modified", "date", "between" },
                    icon: Icon(UIcons.Calendar),
                    description: "Filter results by a date range (yyyy-MM-dd:yyyy-MM-dd)",
                    exampleValue: "2025-01-01"))
            .OnSearch((s, q) =>
            {
                var filterInfo = q.FilterSnaps != null && q.FilterSnaps.Length > 0
                    ? string.Join(", ", q.FilterSnaps.Select(DescribeFilterSnap))
                    : "none";
                Toast().Information($"Searched for: {q.RawQuery} — filters: {filterInfo}");
            }));

            return FeatureCard("Filter snaps", "'field:value' filters, including date ranges",
                "RegisterFilterSnaps declares 'field:' filters. Values come either from a fixed list, or from an async fetcher for a live source. FilterSnapHandler.TimeRange builds a date-range filter instead: typing 'modified:' opens a picker with shortcuts (last week, last month, last 90 days, last year), and the range can also be typed as yyyy-MM-dd:yyyy-MM-dd. Committed filters become chips and arrive on SearchQuery.FilterSnaps, where TryGetDateRange unpacks a range one.",
                box);
        }

        // ---------- Help ----------

        private IComponent Help()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Click the ? button on the left"
            })
            .WS()
            .RegisterSnaps(
                new OmniBox.SnapHandler("docs", "Docs", new[] { "docs", "documentation" }, Icon(UIcons.Book), "Search the documentation", exampleValue: "documentation pages"))
            .RegisterFilterSnaps(
                new OmniBox.FilterSnapHandler(
                    "ext",
                    "File extension",
                    new[] { "ext", "filetype" },
                    new[] { "cs", "ts", "md", "json" },
                    icon: Icon(UIcons.FileCode),
                    description: "Filter results by file extension",
                    exampleValue: "cs, ts, json…"))
            .WithHelp(showSyntax: true)
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            return FeatureCard("Help", "A panel listing what the box understands",
                "WithHelp() adds a ? button that opens a panel listing the registered filters (with their example values) and snaps, so the syntax is discoverable without documentation. WithHelp(showSyntax: true) also documents the boolean operators, grouping and exact-phrase quoting, with an example next to each.",
                box);
        }

        // ---------- Inline chips + right text ----------

        private IComponent InlineChips()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Chips sit inside the input, before the text"
            })
            .WS()
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            box.InlineFilterChips.Add(new OmniBox.InlineFilterChip("Tag: Red", "var(--tss-danger-background-color)", "var(--tss-danger-foreground-color)"));
            box.InlineFilterChips.Add(new OmniBox.InlineFilterChip("Author: Jules", onClick: (_) => Toast().Success("hi!")));
            box.InlineFilterChips.Add(new OmniBox.InlineFilterChip(Button("IComponent")));
            box.SetSearchRightText("124 results");

            var addChip = Button("Add a chip").SetIcon(UIcons.Plus).OnClick(() =>
                box.InlineFilterChips.Add(new OmniBox.InlineFilterChip($"Filter {box.InlineFilterChips.Count + 1}")));

            var clearChips = Button("Clear chips").SetIcon(UIcons.Trash).OnClick(() => box.InlineFilterChips.Clear());

            return FeatureCard("Inline chips", "Filters pinned inside the input, and a result count",
                "InlineFilterChips is an observable list rendered at the head of the search input — use it for filters the app owns (a selected tag, an author, a facet from elsewhere in the UI) rather than ones the user typed. A chip takes a text with optional background/foreground colors, an onClick, or an arbitrary IComponent. SetSearchRightText puts a label at the far end of the input, e.g. a result count.",
                box,
                HStack().Gap(8.px()).MT(8).Children(addChip, clearChips));
        }

        // ---------- Footer items ----------

        private IComponent FooterItems()
        {
            var searchBox = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Search with footer actions",
                SearchFooter = new OmniBox.FooterItems
                {
                    LeftSide  = new IComponent[] { Button(UIcons.Rocket).Tooltip("Left side").OnClick(() => Toast().Success("Lift off 🚀")) },
                    RightSide = new IComponent[] { Button("Feeling lucky").SetIcon(UIcons.Dice).OnClick(() => Toast().Information("🎲")) }
                }
            })
            .WS()
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            var chatBox = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Chat with footer actions",
                ChatFooter = new OmniBox.FooterItems
                {
                    LeftSide = new IComponent[]
                    {
                        Dropdown().ML(16).Searchable().Items(
                            DropdownItem("Consult Documents", icon: UIcons.Book).Selected(),
                            DropdownItem("Find a flight", icon: UIcons.AirplaneJourney),
                            DropdownItem("Book a hotel", icon: UIcons.Hotel))
                    },
                    RightSide = new IComponent[] { Button(UIcons.Microphone).Tooltip("Dictate").OnClick(() => Toast().Information("🎤")) }
                }
            })
            .WS()
            .OnChat((s, q) => Toast().Information(q.Text)));

            return FeatureCard("Footer items", "Your own actions around the input",
                "Config.SearchFooter and Config.ChatFooter each take LeftSide / RightSide arrays of components, placed beside the built-in buttons of that mode — an attachment button, a mode dropdown, a dictate button. In SearchAndChat mode the items of the side that isn't active are hidden along with the rest of that mode's chrome.",
                Label("Search footer").SetContent(searchBox),
                Label("Chat footer").SetContent(chatBox).MT(6));
        }

        // ---------- Keyboard shortcut ----------

        private IComponent KeyboardShortcut()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Search)
            {
                PlaceholderSearch = "Press Ctrl+K anywhere on this page to focus me"
            })
            .WS()
            .SetKeyboardShortcut("Ctrl", "K")
            .OnSearch((s, q) => Toast().Information($"Searched for: {q.RawQuery}")));

            return FeatureCard("Keyboard shortcut", "A global key that focuses the box",
                "SetKeyboardShortcut(\"Ctrl\", \"K\") registers a document-level shortcut that focuses the input, and shows the keys as a hint at the end of the search input. The hint is there to be discovered, so it steps out of the way while the input has focus and comes back on blur — and it is hidden in chat mode. Focus() does the same thing programmatically.",
                box,
                Button("Focus() it").SetIcon(UIcons.Cursor).MT(8).OnClick(() => box.Focus()));
        }

        // ---------- File drop ----------

        private IComponent FileDrop()
        {
            var attachBtn = Button(UIcons.PaperclipVertical).Tooltip("Add attachment");

            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Drop files on me, or click the paperclip",
                ChatFooter      = new OmniBox.FooterItems { RightSide = new IComponent[] { attachBtn } }
            })
            .WS()
            .OnChat((s, q) => Toast().Information(q.Text)));

            var dropArea = FileDropArea(box).OnFilesDropped((s, files) =>
            {
                Toast().Information($"Dropped: {string.Join(", ", files.Select(f => f.name))}");
            }).SetAccepts("*");

            attachBtn.OnClick((s, e) => dropArea.OpenFileSelection());

            return FeatureCard("Files", "Drag & drop, and a file picker",
                "OmniBox has no file handling of its own — wrap it in a FileDropArea to accept drops over the box, and call OpenFileSelection() from a footer button for the click path. Pair it with the context row further down to show what was attached.",
                dropArea.WS());
        }

        // ---------- Models ----------

        private IComponent Models()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Pick a model in the footer, on the right"
            })
            .WS()
            .SetModels(
                new OmniBox.ModelOption("Opus 4.7"),
                new OmniBox.ModelOption("Opus 4.7", "1M"),
                new OmniBox.ModelOption("Sonnet 4.6"),
                new OmniBox.ModelOption("Haiku 4.5"))
            .SetThinkingEffort(OmniBox.ThinkingEffort.High)
            .OnModelChanged((s, model, effort) => Toast().Information($"Selected {model.Name} with {effort} thinking effort"))
            .OnChat((s, q) => Toast().Information(q.Text)));

            var locked = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "This chat has a locked model"
            })
            .WS()
            .LockModel(new OmniBox.ModelOption("Sonnet 4.6"))
            .SetThinkingEffort(OmniBox.ThinkingEffort.Medium)
            .OnChat((s, q) => Toast().Information(q.Text)));

            return FeatureCard("Models", "A model selector with thinking effort",
                "SetModels(params ModelOption[]) adds a selector to the chat footer; a ModelOption can carry a variant label (e.g. a 1M context window) shown next to its name. SetThinkingEffort picks the initial effort, OnModelChanged reports both back. LockModel pins one choice: the button shows it with a lock and the popover stops opening — for a chat where the model is decided elsewhere.",
                Label("Selectable model + effort").SetContent(box),
                Label("Locked model").SetContent(locked).MT(6));
        }

        // ---------- Generating ----------

        private IComponent Generating()
        {
            var box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Send something — the footer shows a spinner and the send button becomes a stop button",
                GeneratingText  = "Thinking"
            })
            .WS()
            .OnChat((s, q) =>
            {
                s.IsGenerating = true;

                // Rewrite the label while it runs, the way a real reply reports its phases.
                window.setTimeout((_) => { if (s.IsGenerating) s.GeneratingText = "Reading documents"; }, 1500);
                window.setTimeout((_) => { if (s.IsGenerating) s.GeneratingText = "Writing the answer"; }, 3000);

                window.setTimeout((_) =>
                {
                    if (s.IsGenerating) // Make sure it wasn't cancelled
                    {
                        s.IsGenerating   = false;
                        s.GeneratingText = "Thinking";
                        Toast().Information(q.Text);
                    }
                }, 5000);
            })
            .OnStop(s =>
            {
                s.IsGenerating   = false;
                s.GeneratingText = "Thinking";
                Toast().Warning("Stopped");
            }));

            return FeatureCard("Generating", "The state while a reply streams",
                "Setting IsGenerating = true swaps the send button for a stop button and shows a spinner in the footer with the elapsed time appended to the label; OnStop fires when the user presses stop, and it is the handler's job to set IsGenerating back to false. GeneratingText (on the config, or written live) is that label — send a message below to watch it change.",
                box);
        }

        // ---------- Tools & agents ----------

        private IComponent ToolsAndAgents()
        {
            var selectorForChat = ToolAgentSelector()
                .Agents(
                    ToolAgentSelectorItem("deep-researcher", "Deep Researcher", "Multi-step web research with citations", UIcons.Search),
                    ToolAgentSelectorItem("code-assistant", "Code Assistant", "Plans, writes and debugs code end to end", UIcons.FileCode).Selected(),
                    ToolAgentSelectorItem("data-analyst", "Data Analyst", "Explores data and builds charts", UIcons.ChartHistogram))
                .Tools(
                    ToolAgentSelectorItem("web-search", "Web Search", "Search the live web for fresh results", UIcons.Globe),
                    ToolAgentSelectorItem("code-interpreter", "Code Interpreter", "Run Python in a sandbox", UIcons.Terminal).Selected(),
                    ToolAgentSelectorItem("image-generation", "Image Generation", "Create images from a prompt", UIcons.Picture),
                    ToolAgentSelectorItem("file-search", "File Search", "Search files in this workspace", UIcons.FolderOpen),
                    ToolAgentSelectorItem("calculator", "Calculator", "Evaluate math expressions", UIcons.Calculator))
                .OnChange(s => Toast().Information($"{s.SelectedCount} tool(s)/agent(s) enabled"));

            var chatBox = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Ask me anything — type @ to mention a tool or agent",
                ChatFooter = new OmniBox.FooterItems { LeftSide = new IComponent[] { selectorForChat } }
            })
            .WS()
            .OnChat((s, q) => Toast().Information(q.Text))
            .EnableChatMentions(new OmniBox.ChatMention
            {
                OnShow         = (x, y) => selectorForChat.ShowInlineAt(x, y),
                OnQueryChanged = text    => selectorForChat.Filter(text),
                OnMove         = dir     => selectorForChat.MoveHighlight(dir),
                OnCommit       = ()      => selectorForChat.ActivateHighlighted(),
                OnHide         = ()      => selectorForChat.Hide(),
                IsOpen         = ()      => selectorForChat.IsVisible
            }));

            return FeatureCard("Tools & agents", "A selector in the chat footer, and '@' mentions",
                "ToolAgentSelector is a trigger button plus a searchable popup for enabling agents and tools, grouped into \"Agents\" and \"Tools\" with an icon, a title and an optional description per row; the trigger shows a count badge, and .Compact() drops the descriptions for a denser list. Put it in Config.ChatFooter like any other footer item. EnableChatMentions then turns typing @ into the same picker anchored at the caret: keep typing to filter, Arrow Up/Down to move, Enter/Tab to pick, Escape to close. ChatMention is a set of plain callbacks, so any anchored popup can be wired to it.",
                chatBox);
        }

        // ---------- Context to add ----------

        private IComponent ContextToAdd()
        {
            var contextSpecs = new[]
            {
                new[] { "Kindersonnenschutzmittel-NEU.pdf", "PDF",         "#ef4444" },
                new[] { "Q3-forecast.xlsx",                 "Spreadsheet", "#16a34a" },
                new[] { "architecture.md",                  "Markdown",    "#6366f1" },
                new[] { "tesserae.dev/components",          "Web page",    "#0ea5e9" },
                new[] { "customers",                        "Dataset",     "#f59e0b" }
            };

            var contextIcons = new[] { UIcons.FilePdf, UIcons.FileExcel, UIcons.FileCode, UIcons.Globe, UIcons.Database };

            var nextContext = 1; // the first one is attached up front, below

            OmniBox box = null;

            ContextCard CardFor(int index)
            {
                var i    = index % contextSpecs.Length;
                var spec = contextSpecs[i];

                return ContextCard(spec[0], contextIcons[i]).SetSubLabel(spec[1]).IconBackground(spec[2]);
            }

            var addContextBtn = Button(UIcons.Clip).Tooltip("Add context").OnClick(() => box.AddContext(CardFor(nextContext++)));

            box = Track(OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Ask me anything about the attached context",
                ChatFooter      = new OmniBox.FooterItems { LeftSide = new IComponent[] { addContextBtn } }
            })
            .WS()
            .WithContextToAdd(CardFor(0))
            .OnChat((s, q) =>
            {
                var sent = s.ContextToAdd.Select(c => c.Label).ToArray();

                // The box keeps the context until it is told otherwise, so the handler is where it gets
                // sent along with the message and cleared.
                s.ClearContext();

                Toast().Information(sent.Length > 0
                    ? $"{q.Text} (with {sent.Length} context: {string.Join(", ", sent)})"
                    : q.Text);
            }));

            return FeatureCard("Context", "The context the next message carries",
                "In chat mode, WithContextToAdd renders ContextCards inside the box — a wrapping row just below the input and above the footer. AddContext appends one, RemoveContext / ClearContext take them out, ContextToAdd reads the current list, and each card's remove button is already wired to the row, so hovering a card and clicking its (x) detaches it. Click the clip button to attach more, then send: the handler reports what went with the message and clears the row.",
                box);
        }

        private static string DescribeFilterSnap(OmniBox.FilterSnap f)
        {
            if (f.TryGetDateRange(out var from, out var to))
            {
                return $"{f.FilterId}: {from:yyyy-MM-dd} → {to:yyyy-MM-dd}";
            }
            return f.FilterId + "=" + f.Value;
        }

        public HTMLElement Render() => _content.Render();
    }
}
