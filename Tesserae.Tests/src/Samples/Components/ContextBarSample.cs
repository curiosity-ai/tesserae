using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 211, Icon = UIcons.OfficePaperclip)]
    public class ContextBarSample : IComponent, ISample
    {
        private static readonly (string name, UIcons icon)[] _documents =
        {
            ("Q3 revenue breakdown.xlsx", UIcons.FileExcel),
            ("Supplier audit 2026.pdf",   UIcons.FilePdf),
            ("Onboarding checklist.docx", UIcons.FileWord),
            ("Kickoff deck.pptx",         UIcons.Presentation),
            ("Ada Lovelace",              UIcons.UserPen)
        };

        private readonly IComponent _content;
        private readonly TextBlock  _lastAction;

        private readonly ContextBar _composerBar = ContextBar();
        private readonly TextBlock  _composerState;
        private          int        _attached;

        public ContextBarSample()
        {
            _lastAction    = TextBlock("Nothing removed yet.").Small();
            _composerState = TextBlock("").Small();

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ContextBarSample), UIcons.OfficePaperclip, "Small bubbles naming the context something is scoped to")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ContextBar shows what a chat, a search or a generated answer is scoped to: one bubble per document or record, each with an icon, an ellipsized name and an optional remove button. It is the indicator that survives closing whatever panel the context was picked in."),
                        TextBlock("A name is ellipsized at 80px, but a trailing file extension is held outside that width — a bubble reads \"Quarterly repo….pdf\", never \"Quarterly repor…\". Bubbles past MaxVisible (3 by default) are not rendered at all: they collapse into a \"+N more\" button that calls OnShowAll.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A bubble per context item"),
                        TextBlock("Clicking a bubble opens what it names. Without an OnClick handler a bubble is not interactive. The ✕ stays quiet until its bubble is hovered, so a row reads as names rather than as a row of delete buttons."),
                        Bar(Bubble(0), Bubble(4)),

                        SampleSubTitle("Overflow behind \"+N more\""),
                        TextBlock("Five bubbles, three visible. The button reports how many are collapsed and hands over to the host, which is where the full context belongs — a search restricted to it, a list, a panel."),
                        Bar(Bubble(0), Bubble(1), Bubble(2), Bubble(3), Bubble(4)),
                        _lastAction,

                        SampleSubTitle("Every bubble visible"),
                        TextBlock("MaxVisible controls where the row stops; a large value renders everything and never shows the button."),
                        Bar(Bubble(0), Bubble(1), Bubble(2), Bubble(3), Bubble(4)).MaxVisible(10),

                        SampleSubTitle("Wider names, custom overflow text"),
                        TextBlock("MaxNameWidth widens (or narrows) where a name is cut, and MoreText replaces the button's wording."),
                        Bar(Bubble(0).MaxNameWidth(160.px()),
                            Bubble(1).MaxNameWidth(160.px()),
                            Bubble(2).MaxNameWidth(160.px()),
                            Bubble(3).MaxNameWidth(160.px()))
                           .MaxVisible(2)
                           .MoreText("Show {0} more documents")
                    )).SetTitle("Usage")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Where the bar usually belongs: inside the chat box itself, above the text being typed, so the message being written says what it will be answered from. OmniBox has a slot for exactly this — pass the bar as Config.ChatHeader, or hand it over later with SetChatHeader. The slot takes up no space while it is empty, so a chat with no context looks untouched."),
                        BuildComposer().PT(8),
                        _composerState.PT(4),
                        HStack().WS().Wrap().PT(8).Children(
                            Button("Attach a document").SetIcon(UIcons.OfficePaperclip).OnClick(() => AttachToComposer()),
                            Button("Detach everything").SetIcon(UIcons.Broom).OnClick(() =>
                            {
                                _composerBar.Clear();
                                ReportComposerState();
                            }))
                    )).SetTitle("In a chat composer")));
        }

        private ContextBar.Item Bubble(int document)
        {
            var doc = _documents[document % _documents.Length];

            return ContextBarItem(doc.name, doc.icon)
               .Tooltip(doc.name)
               .OnClick(i => Toast().Information($"Opening {i.Name}"));
        }

        // The bar is what knows which bubbles it holds, so removal is wired once the bar exists.
        private ContextBar Bar(params ContextBar.Item[] items)
        {
            var bar = ContextBar(items).OnShowAll(() => Toast().Information("A host opens the full context here."));

            foreach (var item in items)
            {
                item.OnRemove(i =>
                {
                    bar.Remove(i);
                    _lastAction.Text = $"Removed {i.Name}.";
                });
            }

            return bar;
        }

        // A chat box whose context lives inside it, above the text being typed.
        private IComponent BuildComposer()
        {
            _composerBar.OnShowAll(() => Toast().Information("A host opens the full context here."));

            AttachToComposer();
            AttachToComposer();

            return OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
            {
                PlaceholderChat = "Ask about the attached documents",
                ChatHeader      = _composerBar,
                ChatFooter      = new OmniBox.FooterItems
                {
                    LeftSide = new[] { Button(UIcons.OfficePaperclip).Tooltip("Attach a document").OnClick(() => AttachToComposer()) }
                }
            }).WS().OnChat((s, m) => Toast().Success($"Sent, with {_composerBar.Count} document(s) attached"));
        }

        private void AttachToComposer()
        {
            var bubble = Bubble(_attached++);

            bubble.OnRemove(i =>
            {
                _composerBar.Remove(i);
                ReportComposerState();
            });

            _composerBar.Add(bubble);
            ReportComposerState();
        }

        private void ReportComposerState()
        {
            _composerState.Text = _composerBar.IsEmpty
                ? "No context: the slot above the input is empty, and collapsed."
                : $"This chat is scoped to {_composerBar.Count} document(s).";
        }

        public HTMLElement Render() => _content.Render();
    }
}
