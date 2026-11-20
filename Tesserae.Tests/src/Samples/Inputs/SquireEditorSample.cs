using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 25, Icon = UIcons.PenNib, Description = "Rich text editor with Markdown and collaboration")]
    public class SquireEditorSample : IComponent, ISample
    {
        internal const string SeedMarkdown =
            "# Release notes\n\n" +
            "The editor reads and writes **Markdown**: headings, *emphasis*, ~~strikethrough~~, `inline code` and [links](https://github.com/fastmail/Squire).\n\n" +
            "- Lists, with\n  - nested items\n- and more items\n\n" +
            "1. Numbered\n2. Lists\n\n" +
            "> A quote stays a quote.\n\n" +
            "```csharp\nvar editor = SquireEditor();\n```\n";

        private readonly IComponent _content;

        public SquireEditorSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SquireEditorSample), UIcons.PenNib, "A rich text editor that speaks Markdown")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("`SquireEditor()` is a WYSIWYG editor built on Squire, with a formatting toolbar. Squire is loaded on demand the first time an editor is created; content set before then is applied once it has loaded."),
                        TextBlock("Content goes in and out as HTML (`HTML`) or Markdown (`Markdown`). Reading Markdown back returns every block nobody edited exactly as it was written, so a document round-trips without being reformatted."),
                        TextBlock("`editor.BindYjs(text, awareness)` binds the editor to a shared `Y.Text` holding Markdown: two editors, or an editor and a code editor on the same Markdown, edit one document at the same time and see each other's carets.")
                    )).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Use `Markdown` when the content is stored or shared as text, `HTML` when it is sent as is (an email)."),
                        SampleDo("Set the content with `SetMarkdown(...)` (or the `Markdown` property) when it is stored as Markdown: that turns on `MarkdownMode()`, so the toolbar only offers what Markdown can say and pasted HTML is converted on the way in. Binding to Yjs turns it on too."),
                        SampleDo("Pass a `SquireEditorText` to `Texts(...)` to translate the toolbar."),
                        SampleDo("Bind to a `Y.Text` for collaboration: the shared text is Markdown, so any Markdown editor can be on the other end."),
                        SampleDont("Leave the full toolbar on for Markdown content: fonts, sizes, colours, underline and alignment have no Markdown spelling and are dropped.")
                    )).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(new MarkdownOut().Content).SetTitle("Rich text and its Markdown"),
                    Card(Defer(async () =>
                    {
                        await Yjs.LoadAsync();
                        return new TwoEditors().Content;
                    }, TextBlock("Loading Yjs…"))).SetTitle("Two editors, one document")));
        }

        public HTMLElement Render() => _content.Render();

        /// <summary>An editor with its Markdown written out beside it as it changes.</summary>
        private sealed class MarkdownOut
        {
            public IComponent Content { get; }

            public MarkdownOut()
            {
                var editor = SquireEditor().H(360);
                var output = TextArea().H(360).Style(s => s.fontFamily = "var(--tss-monospace-font-family)").Class("sample-squire-markdown");

                editor.Markdown = SeedMarkdown;
                output.Text     = SeedMarkdown;
                output.IsReadOnly = true;

                editor.OnInput(e => output.Text = e.Markdown);

                // Setting Markdown turned the mode on; the toggle starts where the editor is.
                var markdownMode = Toggle("Markdown mode").Class("sample-squire-markdown-mode");
                markdownMode.IsChecked = editor.IsMarkdownMode;

                Content = VStack().WS().Gap(8.px()).Children(
                    HStack().WS().Gap(16.px()).Children(editor.W(10).Grow(), output.W(10).Grow()),
                    HStack().Gap(8.px()).AlignItemsCenter().Children(
                        markdownMode.OnChange((t, _) =>
                        {
                            editor.MarkdownMode(t.IsChecked);
                            output.Text = editor.Markdown;
                        }),
                        Button("Reset").SetIcon(UIcons.Refresh).OnClick(() =>
                        {
                            editor.Markdown = SeedMarkdown;
                            output.Text     = editor.Markdown;
                            markdownMode.IsChecked = editor.IsMarkdownMode;
                        }),
                        Button("Show HTML").SetIcon(UIcons.CodeSimple).OnClick(() => Modal("HTML").Content(TextArea(editor.HTML).H(300).WS()).Show())));
            }
        }

        /// <summary>Two editors on two documents, kept in step through an in-memory network with latency.</summary>
        private sealed class TwoEditors
        {
            private readonly Peer      _ada;
            private readonly Peer      _grace;
            private readonly TextBlock _convergence;

            private int  _latency = 150;
            private bool _offline;

            public IComponent Content { get; }

            public TwoEditors()
            {
                _ada   = new Peer("Ada",   this);
                _grace = new Peer("Grace", this);

                _ada.Connect(_grace);
                _grace.Connect(_ada);

                _ada.Doc.transact(_ => _ada.Text.insert(0, SeedMarkdown, null), "seed");

                _convergence = TextBlock().Small().Class("sample-squire-convergence");

                var latencyLabel = TextBlock($"Latency: {_latency} ms").Small();

                var latency = Slider(val: _latency, min: 0, max: 1500, step: 50).W(200).OnInput((s, e) =>
                {
                    _latency          = s.Value;
                    latencyLabel.Text = $"Latency: {_latency} ms";
                });

                var offline = Toggle("Offline").OnChange((t, e) => SetOfflineAsync(t.IsChecked).FireAndForget());

                _ada.Text.observe((e, tr) => UpdateConvergence());
                _grace.Text.observe((e, tr) => UpdateConvergence());

                Content = VStack().WS().Children(
                    TextBlock("Each editor has its own Y.Doc; updates travel through an in-memory network after the latency. Type in either one, format text with the toolbar, or go offline, edit both and come back.").Small().PB(8),
                    HStack().WS().AlignItemsCenter().Wrap().Gap(16.px()).Children(latencyLabel, latency, offline),
                    HStack().WS().Wrap().Gap(16.px()).Children(_ada.Content.Grow().W(10), _grace.Content.Grow().W(10)),
                    _convergence.PT(8))
                   .WhenRemoved(() =>
                    {
                        _ada.Dispose();
                        _grace.Dispose();
                    });

                ConnectAsync().FireAndForget();
                UpdateConvergence();
            }

            internal int Latency => _latency;

            internal bool Offline => _offline;

            private async Task ConnectAsync()
            {
                await _ada.Provider.ConnectAsync();
                await _grace.Provider.ConnectAsync();
            }

            private async Task SetOfflineAsync(bool offline)
            {
                _offline = offline;

                if (offline)
                {
                    _ada.Provider.Disconnect();
                    _grace.Provider.Disconnect();
                }
                else
                {
                    await ConnectAsync();
                }

                UpdateConvergence();
            }

            private void UpdateConvergence()
            {
                var same = _ada.Text.asString() == _grace.Text.asString();

                _convergence.Text = same
                    ? "Both documents hold the same Markdown."
                    : _offline ? "The documents differ: they are offline." : "The documents differ: updates are in flight.";

                _convergence.Foreground(same ? Theme.Success.Background : Theme.Secondary.Foreground);
            }
        }

        private sealed class Peer : IDisposable
        {
            private readonly TwoEditors       _owner;
            private readonly SquireYjsBinding _binding;

            public Y.Doc           Doc       { get; }
            public Y.Text          Text      { get; }
            public Awareness       Awareness { get; }
            public YjsSyncProvider Provider  { get; private set; }
            public SquireEditor    Editor    { get; }
            public IComponent      Content   { get; }

            public Peer(string name, TwoEditors owner)
            {
                _owner    = owner;
                Doc       = new Y.Doc();
                Text      = Doc.getText("markdown");
                Awareness = new Awareness(Doc);
                Awareness.setLocalStateField("user", YjsUserColors.UserFor(name));

                Editor   = SquireEditor().H(360).Class("sample-squire-peer");
                _binding = Editor.BindYjs(Text, Awareness);

                Content = VStack().Gap(8.px()).Children(
                    HStack().AlignItemsCenter().Gap(8.px()).Children(
                        Raw(Swatch(YjsUserColors.ColorFor(name))),
                        TextBlock(name + "'s editor").SemiBold()),
                    Editor);
            }

            public void Connect(Peer other)
            {
                Provider = YjsSyncProvider.WithUint8ArraySender(Doc, Awareness, message =>
                {
                    window.setTimeout(_ =>
                    {
                        if (!_owner.Offline) other.Provider.Receive(message);
                    }, _owner.Latency);

                    return Task.FromResult(0);
                });
            }

            private static HTMLElement Swatch(string color) => Div(Att(styles: s =>
            {
                s.width        = "10px";
                s.height       = "10px";
                s.borderRadius = "50%";
                s.background   = color;
            }));

            public void Dispose()
            {
                _binding.Dispose();
                Provider.Dispose();
                Awareness.destroy();
                Doc.destroy();
                Editor.Destroy();
            }
        }
    }
}
