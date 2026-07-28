using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 104, Icon = UIcons.Clip)]
    public class ContextCardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        private readonly Stack _attached;
        private          int   _nextAttachment;

        private static readonly ContextSpec[] Attachments =
        {
            new ContextSpec("Kindersonnenschutzmittel-NEU.pdf", "PDF",         UIcons.FilePdf,   "#ef4444"),
            new ContextSpec("Q3-forecast.xlsx",                 "Spreadsheet", UIcons.FileExcel, "#16a34a"),
            new ContextSpec("architecture.md",                  "Markdown",    UIcons.FileCode,  "#6366f1"),
            new ContextSpec("tesserae.dev/components",          "Web page",    UIcons.Globe,     "#0ea5e9"),
            new ContextSpec("customers",                        "Dataset",     UIcons.Database,  "#f59e0b")
        };

        public ContextCardSample()
        {
            _attached = HStack().Wrap().Gap(8.px()).WS();

            AttachNext();

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(ContextCardSample), UIcons.Clip, "Cards describing the context attached to a conversation")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A ContextCard names one piece of context attached to a conversation — a file, a page, a dataset. It is an icon tile on a colored background, a label, and an optional second line, sized to sit in a wrapping row above a ChatArea composer."),
                        TextBlock("The tile takes a UIcons glyph, any component (an Icon with its own color, an emoji, a badge), or an image thumbnail that covers it. Passing a handler to OnRemove adds a round (x) button over the card's top-right corner that fades in while the card is hovered or focused — and stays visible on touch devices, where nothing hovers.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Attached context above a composer"),
                        TextBlock("Hover a card to reveal its remove button. OnRemove does not remove the card by itself — the handler owns the list the cards live in, so it can drop the underlying context at the same time."),
                        _attached.PT(8).PB(8),
                        HStack().Gap(8.px()).Children(
                            Button("Attach context").SetIcon(UIcons.Clip).Primary().OnClick(() => AttachNext()),
                            Button("Remove all").SetIcon(UIcons.Trash).OnClick(() => _attached.Clear())),

                        SampleSubTitle("Tiles"),
                        TextBlock("The tile's background and glyph colors are set with IconBackground / IconForeground, SetImage fills it with a thumbnail, and NoIconBackground drops the colored square entirely."),
                        HStack().Wrap().Gap(8.px()).PT(8).Children(
                            ContextCard("report-2026.pdf", UIcons.FilePdf).SetSubLabel("PDF").IconBackground("#ef4444"),
                            ContextCard("curiosity-logo.svg", UIcons.FileImage).SetSubLabel("Image")
                                .SetImage("./assets/img/curiosity-logo.svg"),
                            ContextCard("Design review", Icon(UIcons.Palette, color: "#a855f7")).SetSubLabel("Note").NoIconBackground(),
                            ContextCard("Sunny days", Icon(Emoji.SunWithFace)).SetSubLabel("Emoji").NoIconBackground(),
                            ContextCard("customers", UIcons.Database).SetSubLabel("42.109 rows").IconBackground("#f59e0b")),

                        SampleSubTitle("Label only"),
                        TextBlock("Without a second line the card collapses to a single centered row."),
                        HStack().Wrap().Gap(8.px()).PT(8).Children(
                            ContextCard("notes.txt", UIcons.File),
                            ContextCard("tesserae.dev", UIcons.Globe).IconBackground("#0ea5e9"),
                            ContextCard("A label long enough that it has to be ellipsized to fit the width the card was given", UIcons.FileCode)
                                .IconBackground("#6366f1").MaxWidth(260.px()).OnRemove(() => { })),

                        SampleSubTitle("Compact"),
                        TextBlock("Compact() tightens the card into one row, with the second line beside the label — for a composer carrying many pieces of context at once."),
                        HStack().Wrap().Gap(6.px()).PT(8).Children(
                            ContextCard("report-2026.pdf", UIcons.FilePdf).SetSubLabel("PDF").IconBackground("#ef4444").Compact().OnRemove(() => { }),
                            ContextCard("Q3-forecast.xlsx", UIcons.FileExcel).SetSubLabel("Spreadsheet").IconBackground("#16a34a").Compact().OnRemove(() => { }),
                            ContextCard("architecture.md", UIcons.FileCode).IconBackground("#6366f1").Compact().OnRemove(() => { })),

                        SampleSubTitle("Clickable"),
                        TextBlock("OnClick makes the whole card open the context it stands for (and makes it keyboard reachable, activated with Enter or Space). Clicking the remove button never reads as opening the card."),
                        HStack().Wrap().Gap(8.px()).PT(8).Children(
                            ContextCard("Kindersonnenschutzmittel-NEU.pdf", UIcons.FilePdf)
                                .SetSubLabel("PDF")
                                .IconBackground("#ef4444")
                                .OnClick((c, _) => Toast().Information($"Opening {c.Label}"))
                                .OnRemove(c => Toast().Information($"Removing {c.Label}")))
                    )).SetTitle("Usage")));
        }

        private void AttachNext()
        {
            var spec = Attachments[_nextAttachment % Attachments.Length];
            _nextAttachment++;

            var card = ContextCard(spec.Label, spec.Icon).SetSubLabel(spec.Kind).IconBackground(spec.Color);

            card.OnRemove(c => _attached.Remove(c));

            _attached.Add(card);
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
