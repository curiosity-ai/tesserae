using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 104, Icon = UIcons.Clip)]
    public class ContextCardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // "Attached context" section: the cards live in this row, and the sample owns the list.
        private readonly Stack _attached;
        private          int   _nextAttachment;

        // "Grouped" section.
        private readonly TextBlock _lastRemoved;

        // "In a chat composer" section.
        private readonly ContextCards _composerContext;
        private readonly TextBlock    _composerState;
        private          int          _attachedToComposer;

        private static readonly ContextSpec[] Attachments =
        {
            new ContextSpec("Kindersonnenschutzmittel-NEU.pdf", "PDF",         UIcons.FilePdf,   "#ef4444"),
            new ContextSpec("Q3-forecast.xlsx",                 "Spreadsheet", UIcons.FileExcel, "#16a34a"),
            new ContextSpec("architecture.md",                  "Markdown",    UIcons.FileCode,  "#6366f1"),
            new ContextSpec("tesserae.dev/components",          "Web page",    UIcons.Globe,     "#0ea5e9"),
            new ContextSpec("customers",                        "Dataset",     UIcons.Database,  "#f59e0b")
        };

        // The five items the grouped list is built from; the third column is what goes in the badge slot.
        private static readonly string[][] Items =
        {
            new[] { "Q3 revenue model",        "finance/q3-model.xlsx · 4 sheets",  "SharePoint", "#16a34a" },
            new[] { "Incident 482 postmortem", "docs/postmortem-482.md",            "Wiki",       "#3b82f6" },
            new[] { "Re: migration window",    "from ops@needle.dev · Nov 14",      "Inbox",      "#f59e0b" },
            new[] { "events.request_log",      "warehouse · 2.1M rows",             "Snowflake",  "#10b981" },
            new[] { "design-review/",          "14 files · 62 MB",                  "Drive",      "#94a3b8" }
        };

        private static readonly UIcons[] ItemIcons = { UIcons.Table, UIcons.FileInvoice, UIcons.Envelope, UIcons.Database, UIcons.Folder };

        // The documents the compact rows are built from.
        private static readonly string[][] Documents =
        {
            new[] { "Migration plan.docx",         "#3b82f6" },
            new[] { "topology-v3.png",             "#ec4899" },
            new[] { "cutover-checklist.csv",       "#16a34a" },
            new[] { "needle.dev/changelog",        "#0ea5e9" },
            new[] { "Migration window",            "#f59e0b" },
            new[] { "Runner.cs",                   "#a855f7" },
            new[] { "Quarterly report FY26.pdf",   "#ef4444" },
            new[] { "Supplier audit 2026.pdf",     "#ef4444" },
            new[] { "Onboarding checklist.docx",   "#3b82f6" },
            new[] { "Ada Lovelace",                "#64748b" }
        };

        private static readonly UIcons[] DocumentIcons =
        {
            UIcons.FileWord, UIcons.FileImage, UIcons.FileExcel, UIcons.Link, UIcons.CalendarClock,
            UIcons.FileCode, UIcons.FilePdf, UIcons.FilePdf, UIcons.FileWord, UIcons.UserPen
        };

        public ContextCardSample()
        {
            _attached        = HStack().Wrap().Gap(8.px()).WS();
            _lastRemoved     = TextBlock("Nothing detached yet.").Small().Foreground(Theme.Secondary.Foreground);
            _composerState   = TextBlock("").Small().Foreground(Theme.Secondary.Foreground);
            _composerContext = ContextCards().Compact().MaxVisible(3);

            AttachNext();

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ContextCardSample), UIcons.Clip, "Cards describing the context attached to a conversation, one by one or as a group")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Attached()))
                .FlatSection(VStack().WS().Children(Tiles()))
                .FlatSection(VStack().WS().Children(LabelOnly()))
                .FlatSection(VStack().WS().Children(Compact()))
                .FlatSection(VStack().WS().Children(Badges()))
                .FlatSection(VStack().WS().Children(Clickable()))
                .FlatSection(VStack().WS().Children(RightClick()))
                .FlatSection(VStack().WS().Children(Grouped()))
                .FlatSection(VStack().WS().Children(CompactRow()))
                .FlatSection(VStack().WS().Children(LongNames()))
                .FlatSection(VStack().WS().Children(Stretched()))
                .FlatSection(VStack().WS().Children(InAComposer()))
                .SeeAlso(typeof(ChatSample), typeof(OmniBoxSample), typeof(ToolCallSample), typeof(ResourceCardSample), typeof(CardSample));
        }

        // One feature per card: a subtitle, a line or two saying what to try, then the cards themselves.
        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        // ---------- Overview ----------

        private IComponent Overview()
        {
            return FeatureCard("Overview", "One card, and a group of them",
                "A ContextCard names one piece of context attached to a conversation — a file, a page, a dataset. It is an icon tile on a colored background, a label, and an optional second line, sized to sit in a wrapping row above a ChatArea composer. The tile takes a UIcons glyph, any component (an Icon with its own color, an emoji, a badge), or an image thumbnail that covers it. Passing a handler to OnRemove adds a round (x) button over the card's top-right corner that fades in while the card is hovered or focused — and stays visible on touch devices, where nothing hovers.",
                TextBlock("ContextCards (plural) holds a set of those cards and shows them as one thing: a summary pill (\"Added 5 items to context\") that expands into a bordered list of rows and collapses back, exactly like ToolCall and ToolsUsed do in a transcript, or — with Compact() — a wrapping row of pills with no header. A card added to a group has its remove button wired to the group, so clicking a card's ✕ detaches it, and a handler the caller registered on the card still runs. An empty group renders nothing and takes up no space, so it can sit permanently in a layout.").Small());
        }

        // ---------- Attached context ----------

        private IComponent Attached()
        {
            return FeatureCard("Attached context", "A wrapping row above a composer",
                "Hover a card to reveal its remove button. OnRemove does not remove the card by itself — the handler owns the list the cards live in, so it can drop the underlying context at the same time.",
                _attached.PT(8).PB(8),
                HStack().Gap(8.px()).Children(
                    Button("Attach context").SetIcon(UIcons.Clip).Primary().OnClick(() => AttachNext()),
                    Button("Remove all").SetIcon(UIcons.Trash).OnClick(() => _attached.Clear())));
        }

        // ---------- Tiles ----------

        private IComponent Tiles()
        {
            return FeatureCard("Tiles", "The icon at the head of the card",
                "The tile's background and glyph colors are set with IconBackground / IconForeground, IconTint derives both from one color, SetImage fills it with a thumbnail, and NoIconBackground drops the colored square entirely.",
                HStack().Wrap().Gap(8.px()).Children(
                    ContextCard("report-2026.pdf", UIcons.FilePdf).SetSubLabel("PDF").IconBackground("#ef4444"),
                    ContextCard("curiosity-logo.svg", UIcons.FileImage).SetSubLabel("Image")
                        .SetImage("./assets/img/curiosity-logo.svg"),
                    ContextCard("Design review", Icon(UIcons.Palette, color: "#a855f7")).SetSubLabel("Note").NoIconBackground(),
                    ContextCard("Sunny days", Icon(Emoji.SunWithFace)).SetSubLabel("Emoji").NoIconBackground(),
                    ContextCard("customers", UIcons.Database).SetSubLabel("42.109 rows").IconBackground("#f59e0b")));
        }

        // ---------- Label only ----------

        private IComponent LabelOnly()
        {
            return FeatureCard("Label only", "A card without a second line",
                "Without a second line the card collapses to a single centered row.",
                HStack().Wrap().Gap(8.px()).Children(
                    ContextCard("notes.txt", UIcons.File),
                    ContextCard("tesserae.dev", UIcons.Globe).IconBackground("#0ea5e9"),
                    ContextCard("A label long enough that it has to be ellipsized to fit the width the card was given", UIcons.FileCode)
                        .IconBackground("#6366f1").MaxWidth(260.px()).OnRemove(() => { })));
        }

        // ---------- Compact ----------

        private IComponent Compact()
        {
            return FeatureCard("Compact", "A one-line pill",
                "Compact() tightens the card into a one-line pill, with the second line beside the label — for a composer carrying many pieces of context at once, or for one file named inline. MonospaceSubLabel() gives a path or a size the monospace treatment ToolCall gives its command, and WithChevron() adds the hint that clicking the card opens it.",
                HStack().Wrap().Gap(6.px()).Children(
                    ContextCard("Runner.cs", UIcons.FileCode)
                        .SetSubLabel("src/Needle/Inference/ · 12 KB")
                        .MonospaceSubLabel()
                        .IconBackground("#a855f7")
                        .Compact()
                        .WithChevron()
                        .OnClick((c, _) => Toast().Information($"Opening {c.Label}"))),
                HStack().Wrap().Gap(6.px()).PT(8).Children(
                    ContextCard("report-2026.pdf", UIcons.FilePdf).SetSubLabel("PDF").IconBackground("#ef4444").Compact().OnRemove(() => { }),
                    ContextCard("Q3-forecast.xlsx", UIcons.FileExcel).SetSubLabel("Spreadsheet").IconBackground("#16a34a").Compact().OnRemove(() => { }),
                    ContextCard("architecture.md", UIcons.FileCode).IconBackground("#6366f1").Compact().OnRemove(() => { })));
        }

        // ---------- Badge ----------

        private IComponent Badges()
        {
            return FeatureCard("Badge", "A pill at the end of the card",
                "SetBadge puts a small pill at the end of the card. The card says nothing about what belongs there — what a piece of context is is already carried by the icon you pass — so it takes whatever the app wants: a source, a count, a status. The IComponent overload takes a component instead of plain text, and drops the pill chrome so that component's own styling shows.",
                HStack().Wrap().Gap(8.px()).Children(
                    ContextCard("Q3 revenue model", UIcons.Table).SetSubLabel("finance/q3-model.xlsx · 4 sheets").MonospaceSubLabel().SetBadge("SharePoint").IconTint("#16a34a").W(340.px()),
                    ContextCard("events.request_log", UIcons.Database).SetSubLabel("warehouse · 2.1M rows").MonospaceSubLabel().SetBadge("2.1M rows").IconTint("#10b981").W(340.px())),
                HStack().Wrap().Gap(8.px()).PT(8).Children(
                    ContextCard("Supplier audit 2026.pdf", UIcons.FilePdf).SetSubLabel("shared with you").SetBadge(Badge("New").Primary()).IconTint("#ef4444").W(340.px()),
                    ContextCard("design-review/", UIcons.Folder).SetSubLabel("indexing…").SetBadge(Spinner().Small()).IconTint("#94a3b8").W(340.px())));
        }

        // ---------- Clickable ----------

        private IComponent Clickable()
        {
            return FeatureCard("Clickable", "Opening the context a card stands for",
                "OnClick makes the whole card open the context it stands for (and makes it keyboard reachable, activated with Enter or Space). Clicking the remove button never reads as opening the card.",
                HStack().Wrap().Gap(8.px()).Children(
                    ContextCard("Kindersonnenschutzmittel-NEU.pdf", UIcons.FilePdf)
                        .SetSubLabel("PDF")
                        .IconBackground("#ef4444")
                        .OnClick((c, _) => Toast().Information($"Opening {c.Label}"))
                        .OnRemove(c => Toast().Information($"Removing {c.Label}"))));
        }

        // ---------- Right-click ----------

        private IComponent RightClick()
        {
            return FeatureCard("Right-click", "A menu of actions on the card",
                "OnContextMenu(() => items) attaches a ContextMenu that opens at the pointer, in place of the browser's own. The generator runs on every right-click, so the items can describe the card as it stands — the one below offers to show or hide its own ✕. A card carrying a menu also takes a tab stop, and answers the keyboard menu key (or Shift+F10) with the same menu anchored to itself. The Action and Action<ContextCard> overloads hand the right-click to a plain handler instead, and the (card, event) overload leaves the browser menu alone so a handler can decide for itself. Text on a card is not selectable, so a right-click never leaves half a file name highlighted; Selectable() opts back in for a label worth copying.",
                HStack().Wrap().Gap(8.px()).Children(
                    WithMenu(ContextCard("Q3 revenue model", UIcons.Table)
                        .SetSubLabel("finance/q3-model.xlsx · 4 sheets")
                        .MonospaceSubLabel()
                        .IconTint("#16a34a")
                        .W(340.px())
                        .OnRemove(c => Toast().Information($"Detached {c.Label}"))),
                    WithMenu(ContextCard("architecture.md", UIcons.FileCode).IconTint("#6366f1").Compact())),
                HStack().Wrap().Gap(8.px()).PT(8).Children(
                    ContextCard("tesserae.dev/components", UIcons.Globe)
                        .SetSubLabel("Web page")
                        .IconTint("#0ea5e9")
                        .Compact()
                        .OnContextMenu(c => Toast().Information($"Right-clicked {c.Label}")),
                    ContextCard("release-notes.txt", UIcons.File)
                        .SetSubLabel("Selectable(): the label can be copied")
                        .Compact()
                        .Selectable()));
        }

        // The menu is generated on every open, so it can describe the card as it stands right now.
        private static ContextCard WithMenu(ContextCard card)
        {
            return card.OnContextMenu(() => new[]
            {
                ContextMenuItem(card.Label).Header(),
                ContextMenuItem(MenuLine(UIcons.FolderOpen, "Open")).OnClick(() => Toast().Information($"Opening {card.Label}")),
                ContextMenuItem(MenuLine(UIcons.Copy, "Copy name")).OnClick(() => Toast().Success($"Copied \"{card.Label}\"")),
                ContextMenuItem().Divider(),
                ContextMenuItem(MenuLine(card.IsRemovable ? UIcons.EyeCrossed : UIcons.Eye, card.IsRemovable ? "Hide the ✕" : "Show the ✕"))
                    .OnClick(() => card.Removable(!card.IsRemovable)),
                ContextMenuItem(MenuLine(UIcons.Trash, "Detach", Theme.Danger.Background)).OnClick(() => Toast().Error($"Detached {card.Label}"))
            });
        }

        private static IComponent MenuLine(UIcons icon, string text, string color = null)
            => HStack().Children(Icon(icon, color: color), TextBlock(text).ML(8));

        // ---------- Grouped ----------

        private IComponent Grouped()
        {
            var expanded  = Group().Expanded();
            var collapsed = Group()
                .SetSummary("5 sources for this answer")
                .SetIcon(UIcons.Books)
                .OnToggle(g => Toast().Information(g.IsExpanded ? "Expanded" : "Collapsed"));

            return FeatureCard("Grouped", "A summary pill that expands into a list",
                "Clicking the pill toggles the list; the chevron follows. Rows are the same ContextCards, laid out as list rows — full width, one divider between each, the badge and the ✕ at the end. SetSummary replaces the auto-generated \"Added N items to context\" (which keeps itself up to date as cards come and go), SetIcon replaces the layers glyph, and OnToggle reports the state.",
                expanded,
                _lastRemoved.MT(4),
                TextBlock("Starting collapsed, with its own summary and icon:").Small().MT(12).MB(8),
                collapsed);
        }

        // ---------- Compact group ----------

        private IComponent CompactRow()
        {
            var row = ContextCards().Compact().MaxVisible(5);

            for (int i = 0; i < 9; i++)
            {
                row.Add(Document(i).OnRemove(c => { }));
            }

            var custom = ContextCards().Compact().MaxVisible(2).MoreText("Show {0} more documents", "Show fewer");

            for (int i = 0; i < 5; i++)
            {
                custom.Add(Document(i));
            }

            var handedOver = ContextCards().Compact().MaxVisible(3)
                .OnShowAll(() => Toast().Information("A host opens the full context here."));

            for (int i = 0; i < 8; i++)
            {
                handedOver.Add(Document(i));
            }

            return FeatureCard("Compact group", "A wrapping row of pills",
                "ContextCards().Compact() drops the header and renders the cards as pills. The first MaxVisible (5 by default) are shown and the rest collapse behind a dashed \"+N more\" pill that reveals them in place — click it again for \"Show less\". MoreText changes that wording; OnShowAll takes the pill over entirely, for a host that would rather open the full list somewhere else.",
                row,
                TextBlock("MaxVisible(2) with MoreText(\"Show {0} more documents\"):").Small().MT(12).MB(8),
                custom,
                TextBlock("OnShowAll: the pill hands over instead of expanding.").Small().MT(12).MB(8),
                handedOver);
        }

        // ---------- Long names ----------

        private IComponent LongNames()
        {
            var narrow = ContextCards().Compact().MaxVisible(6);

            for (int i = 6; i < 10; i++)
            {
                narrow.Add(Document(i).MaxLabelWidth(80.px()));
            }

            var whole = ContextCards().Compact().MaxVisible(6);

            for (int i = 6; i < 10; i++)
            {
                whole.Add(Document(i).KeepExtensionVisible(false).MaxLabelWidth(80.px()));
            }

            return FeatureCard("Long names", "The file extension survives the ellipsis",
                "MaxLabelWidth caps where a label is cut. A trailing file extension is held outside that width and the ellipsis is placed by measuring the text, so a card reads \"Quarterly repo….pdf\" rather than \"Quarterly repor…\" — the extension is usually the most useful part of a file name. KeepExtensionVisible(false) opts out and ellipsizes the whole thing.",
                HStack().Wrap().Gap(8.px()).Children(
                    ContextCard("Quarterly report FY26.pdf", UIcons.FilePdf).IconBackground("#ef4444").Compact().MaxLabelWidth(80.px()),
                    ContextCard("Quarterly report FY26.pdf", UIcons.FilePdf).IconBackground("#ef4444").Compact().MaxLabelWidth(80.px()).KeepExtensionVisible(false)),
                TextBlock("The same rule inside a group:").Small().MT(12).MB(8),
                narrow,
                TextBlock("And with KeepExtensionVisible(false):").Small().MT(12).MB(8),
                whole);
        }

        // ---------- Given a width ----------

        private IComponent Stretched()
        {
            var panel = VStack().W(300.px()).Children(
                ContextCard("Annual report", UIcons.FilePdf).SetSubLabel("PDF").IconTint("#ef4444").Compact().WithChevron().WS(),
                ContextCard("customers", UIcons.Database).SetSubLabel("Dataset").IconTint("#f59e0b").Compact().WithChevron().WS().MT(6),
                ContextCard("A cited page whose title runs past the panel", UIcons.Globe).SetSubLabel("Web page").IconTint("#0ea5e9").Compact().WithChevron().WS().MT(6),
                ContextCard("Q3-forecast.xlsx", UIcons.FileExcel).SetSubLabel("Spreadsheet").IconTint("#16a34a").SetBadge("3 refs").Compact().WithChevron().WS().MT(6));

            return FeatureCard("Given a width", "A column of sources in a side panel",
                "A card sizes itself to its content, but give it a width of its own — .WS() in a panel listing the sources behind a conversation — and the label takes the extra space, so the second line, the badge and the chevron line up at the cards' end and the label is what gets ellipsized.",
                panel);
        }

        // ---------- In a composer ----------

        private IComponent InAComposer()
        {
            AttachToComposer();
            AttachToComposer();

            var composer = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Ask about the attached documents",
                ChatHeader      = _composerContext,
                ChatFooter      = new OmniBox.FooterItems
                {
                    LeftSide = new[] { Button(UIcons.Clip).Tooltip("Attach a document").OnClick(() => AttachToComposer()) }
                }
            })
            .WS()
            .OnChat((s, m) => Toast().Success($"Sent, with {_composerContext.Count} document(s) attached"));

            return FeatureCard("In a chat composer", "Where a compact group usually lives",
                "OmniBox has a slot inside the box above the chat input for whatever the message is being written against: pass the group as Config.ChatHeader, or hand it over later with SetChatHeader. The slot takes up no space while it is empty, and neither does an empty group, so a chat with no context looks untouched. For individual cards below the input instead, see OmniBox.WithContextToAdd.",
                composer,
                _composerState.MT(4),
                HStack().WS().Wrap().Gap(8.px()).MT(8).Children(
                    Button("Attach a document").SetIcon(UIcons.Clip).OnClick(() => AttachToComposer()),
                    Button("Detach everything").SetIcon(UIcons.Broom).OnClick(() =>
                    {
                        _composerContext.Clear();
                        ReportComposerState();
                    })));
        }

        // ---------- Data helpers ----------

        private void AttachNext()
        {
            var spec = Attachments[_nextAttachment % Attachments.Length];
            _nextAttachment++;

            var card = ContextCard(spec.Label, spec.Icon).SetSubLabel(spec.Kind).IconBackground(spec.Color);

            card.OnRemove(c => _attached.Remove(c));

            _attached.Add(card);
        }

        // A group of the five items above, each a row with a mono second line and a badge.
        private ContextCards Group()
        {
            var group = ContextCards();

            for (int i = 0; i < Items.Length; i++)
            {
                var item = Items[i];

                var card = ContextCard(item[0], ItemIcons[i])
                    .SetSubLabel(item[1])
                    .MonospaceSubLabel()
                    .SetBadge(item[2])
                    .IconTint(item[3])
                    .OnRemove(c => _lastRemoved.Text = $"Detached {c.Label}.");

                group.Add(card);
            }

            return group;
        }

        private static ContextCard Document(int index)
        {
            var i   = index % Documents.Length;
            var doc = Documents[i];

            return ContextCard(doc[0], DocumentIcons[i]).IconTint(doc[1]);
        }

        private void AttachToComposer()
        {
            _composerContext.Add(Document(_attachedToComposer++).MaxLabelWidth(120.px()).OnRemove(c => ReportComposerState()));
            ReportComposerState();
        }

        private void ReportComposerState()
        {
            _composerState.Text = _composerContext.Count == 0
                ? "No context: both the group and the slot it sits in are collapsed."
                : $"This chat is scoped to {_composerContext.Count} document(s).";
        }

        public HTMLElement Render() => _content.Render();

        private class ContextSpec
        {
            public string Label { get; }
            public string Kind  { get; }
            public UIcons Icon  { get; }
            public string Color { get; }

            public ContextSpec(string label, string kind, UIcons icon, string color)
            {
                Label = label;
                Kind  = kind;
                Icon  = icon;
                Color = color;
            }
        }
    }
}
