using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 105, Icon = UIcons.Layers)]
    public class ContextCardsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        private readonly TextBlock    _lastRemoved;
        private readonly ContextCards _composerContext;
        private readonly TextBlock    _composerState;
        private          int          _attached;

        // The five items of the grouped list; the third column is what goes in the badge slot.
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

        public ContextCardsSample()
        {
            _lastRemoved     = TextBlock("Nothing detached yet.").Small().Foreground(Theme.Secondary.Foreground);
            _composerState   = TextBlock("").Small().Foreground(Theme.Secondary.Foreground);
            _composerContext = ContextCards().Compact().MaxVisible(3);

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ContextCardsSample), UIcons.Layers, "Many ContextCards behind one summary, or one compact row")
                .FlatSection(VStack().WS().Children(Overview()))
                .FlatSection(VStack().WS().Children(Grouped()))
                .FlatSection(VStack().WS().Children(CompactRow()))
                .FlatSection(VStack().WS().Children(LongNames()))
                .FlatSection(VStack().WS().Children(InAComposer()));
        }

        private static Card FeatureCard(string title, string subTitle, string description, params IComponent[] content)
        {
            var stack = VStack().WS().Children(SampleSubTitle(subTitle), TextBlock(description).MB(8));

            foreach (var c in content)
            {
                stack.Add(c);
            }

            return Card(stack).SetTitle(title);
        }

        private IComponent Overview()
        {
            return FeatureCard("Overview", "One group, two shapes",
                "ContextCards holds a set of ContextCards and shows them as one thing: a summary pill (\"Added 5 items to context\") that expands into a bordered list of rows and collapses back, exactly like ToolCall and ToolsUsed do in a transcript. Compact() switches the whole group to a wrapping row of pills with no header, showing the first MaxVisible and collapsing the rest behind a \"+N more\" pill.",
                TextBlock("A card added to a group has its remove button wired to the group, so hovering a card and clicking its ✕ detaches it — a handler the caller registered on the card still runs. An empty group renders nothing and takes up no space, so it can sit permanently in a layout.").Small());
        }

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

            return FeatureCard("Compact", "A wrapping row of pills",
                "Compact() drops the header and renders the cards as pills. The first MaxVisible (5 by default) are shown and the rest collapse behind a dashed \"+N more\" pill that reveals them in place — click it again for \"Show less\". MoreText changes that wording; OnShowAll takes the pill over entirely, for a host that would rather open the full list somewhere else.",
                row,
                TextBlock("MaxVisible(2) with MoreText(\"Show {0} more documents\"):").Small().MT(12).MB(8),
                custom,
                TextBlock("OnShowAll: the pill hands over instead of expanding.").Small().MT(12).MB(8),
                handedOver);
        }

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
                "MaxLabelWidth caps where a label is cut. A trailing file extension is held outside that width and the ellipsis is placed by measuring the text, so a pill reads \"Quarterly repo….pdf\" — the extension is usually the most useful part of a file name. KeepExtensionVisible(false) opts out and ellipsizes the whole thing.",
                narrow,
                TextBlock("The same names with KeepExtensionVisible(false):").Small().MT(12).MB(8),
                whole);
        }

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
            _composerContext.Add(Document(_attached++).MaxLabelWidth(120.px()).OnRemove(c => ReportComposerState()));
            ReportComposerState();
        }

        private void ReportComposerState()
        {
            _composerState.Text = _composerContext.Count == 0
                ? "No context: both the group and the slot it sits in are collapsed."
                : $"This chat is scoped to {_composerContext.Count} document(s).";
        }

        public HTMLElement Render() => _content.Render();
    }
}
