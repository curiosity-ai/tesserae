using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 211, Icon = UIcons.OfficePaperclip)]
    public class ContextBarSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private readonly TextBlock  _lastAction;

        public ContextBarSample()
        {
            _lastAction = TextBlock("Nothing removed yet.").Small();

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ContextBarSample), UIcons.OfficePaperclip, "Small bubbles naming the context something is scoped to")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("ContextBar shows what a chat, a search or a generated answer is scoped to: one bubble per document or record, each with an icon, an ellipsized name and an optional remove button. It is the indicator that survives closing whatever panel the context was picked in."),
                        TextBlock("A name is ellipsized at 50px, but a trailing file extension is held outside that width — a bubble reads \"Quarterly repo….pdf\", never \"Quarterly repor…\". Bubbles past MaxVisible (3 by default) are not rendered at all: they collapse into a \"+N more\" button that calls OnShowAll.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A bubble per context item"),
                        TextBlock("Clicking a bubble opens what it names. Without an OnClick handler a bubble is not interactive."),
                        Bar(Bubble("Q3 revenue breakdown.xlsx", UIcons.FileExcel),
                            Bubble("Ada Lovelace",              UIcons.UserPen)),

                        SampleSubTitle("Overflow behind \"+N more\""),
                        TextBlock("Five bubbles, three visible. The button reports how many are collapsed and hands over to the host, which is where the full context belongs — a search restricted to it, a list, a panel."),
                        Bar(Bubble("Q3 revenue breakdown.xlsx", UIcons.FileExcel),
                            Bubble("Supplier audit 2026.pdf",   UIcons.FilePdf),
                            Bubble("Onboarding checklist.docx", UIcons.FileWord),
                            Bubble("Kickoff deck.pptx",         UIcons.Presentation),
                            Bubble("Ada Lovelace",              UIcons.UserPen)),
                        _lastAction,

                        SampleSubTitle("Every bubble visible"),
                        TextBlock("MaxVisible controls where the row stops; a large value renders everything and never shows the button."),
                        Bar(Bubble("Q3 revenue breakdown.xlsx", UIcons.FileExcel),
                            Bubble("Supplier audit 2026.pdf",   UIcons.FilePdf),
                            Bubble("Onboarding checklist.docx", UIcons.FileWord),
                            Bubble("Kickoff deck.pptx",         UIcons.Presentation),
                            Bubble("Ada Lovelace",              UIcons.UserPen)).MaxVisible(10),

                        SampleSubTitle("Wider names, custom overflow text"),
                        TextBlock("MaxNameWidth widens (or narrows) where a name is cut, and MoreText replaces the button's wording."),
                        Bar(Bubble("Q3 revenue breakdown.xlsx", UIcons.FileExcel).MaxNameWidth(160.px()),
                            Bubble("Supplier audit 2026.pdf",   UIcons.FilePdf).MaxNameWidth(160.px()),
                            Bubble("Onboarding checklist.docx", UIcons.FileWord).MaxNameWidth(160.px()),
                            Bubble("Kickoff deck.pptx",         UIcons.Presentation).MaxNameWidth(160.px()))
                           .MaxVisible(2)
                           .MoreText("Show {0} more documents")
                    )).SetTitle("Usage")));
        }

        private ContextBar.Item Bubble(string name, UIcons icon)
        {
            return ContextBarItem(name, icon).OnClick(i => Toast().Information($"Opening {i.Name}"));
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

        public HTMLElement Render() => _content.Render();
    }
}
