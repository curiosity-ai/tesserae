using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.Paragraph)]
    public class MarkdownBlockSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MarkdownBlockSample()
        {
            const string staticSample =
@"# MarkdownBlock

`MarkdownBlock` renders **Markdown** as sanitized HTML.

- It is backed by the [marked](https://marked.js.org/) parser
- The output is run through [DOMPurify](https://github.com/cure53/DOMPurify) before being inserted
- A blockquote:

> Markdown source goes in, safe HTML comes out.

```csharp
MarkdownBlock(""# Hello"");
```
";

            const string startingMarkdown =
@"## Try editing this!

Type some markdown below to see it rendered live.

- **Bold**, *italic*, and ~~strike~~
- Lists, [links](https://github.com/curiosity-ai/tesserae), and code

| Feature  | Supported |
| -------- | --------- |
| GFM      | yes       |
| Sanitize | yes       |
";

            var live   = MarkdownBlock(startingMarkdown);
            var editor = TextArea(startingMarkdown).WS().H(220).OnInput((ta, _) => live.Text = ta.Text);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MarkdownBlockSample), UIcons.Paragraph, "A component that renders Markdown as sanitized HTML")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("MarkdownBlock turns a Markdown source string into safely-rendered HTML. The bundled marked parser produces the HTML and DOMPurify strips anything unsafe before it ever reaches the DOM, so it is safe to feed user-authored content."),
                        TextBlock("The component exposes a single Text property: assigning to it re-renders the output, which makes it easy to drive from a TextArea, an observable, or streamed assistant output."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Prefer MarkdownBlock over treating raw HTML strings as text - the sanitization step protects against script injection from third-party content. Keep MarkdownBlock inside a width-constrained container so wide tables and code blocks scroll instead of breaking the layout. For purely static labels, plain TextBlock is cheaper - reach for MarkdownBlock when the input genuinely contains Markdown."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Static Markdown"),
                        MarkdownBlock(staticSample),
                        SampleSubTitle("Live editing"),
                        TextBlock("Edit the Markdown on the left, the rendered output updates on the right."),
                        HStack().WS().Children(
                            editor.Grow(),
                            VStack().Grow().PL(16).Children(live)
                        ),
                        SampleSubTitle("Sanitization"),
                        TextBlock("MarkdownBlock will strip dangerous HTML even when it is embedded inside Markdown:"),
                        MarkdownBlock("This `<script>alert('xss')</script>` will not run, and this <img src=x onerror=alert(1)> attribute is stripped too.")
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
