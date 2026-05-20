using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 30, Icon = UIcons.ShieldCheck)]
    public class SanitizeHTMLSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SanitizeHTMLSample()
        {
            const string dirtyHtml =
@"<p>Trusted paragraph <strong>with bold</strong>.</p>
<p><a href=""https://github.com/curiosity-ai/tesserae"">A safe link</a></p>
<p>Dangerous: <script>alert('xss')</script> inline script.</p>
<img src=x onerror=""alert('xss')"" />
<iframe src=""javascript:alert('xss')""></iframe>
<p onclick=""alert('xss')"">Click handler attribute.</p>";

            var input  = TextArea(dirtyHtml).WS().H(180);
            var output = TextArea(SanitizeHTML.Sanitize(dirtyHtml)).WS().H(180).ReadOnly();
            var live   = MarkdownBlock(); // reuse as a sandboxed HTML renderer via inline HTML in markdown
            live.Text  = SanitizeHTML.Sanitize(dirtyHtml);

            input.OnInput((ta, _) =>
            {
                var sanitized = SanitizeHTML.Sanitize(ta.Text);
                output.Text   = sanitized;
                live.Text     = sanitized;
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SanitizeHTMLSample), UIcons.ShieldCheck, "A utility for sanitizing HTML with DOMPurify")
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                        TextBlock("SanitizeHTML is a thin wrapper around the bundled DOMPurify library. Pass it any HTML string and it returns a copy with scripts, dangerous attributes (onerror, onclick, javascript: URLs, ...) and other XSS vectors stripped out."),
                        TextBlock("DOMPurify is bundled with Tesserae and always loaded - there is no preload step, the helper is fully synchronous."))).SetTitle("Overview")))
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Use SanitizeHTML at every trust boundary: whenever you take HTML from an external source (user input, third-party API, AI-generated content, clipboard) and want to drop it into the DOM. Never trust the input upstream of this step. For Markdown sources, prefer MarkdownBlock - it already runs the output through DOMPurify for you."))).SetTitle("Best Practices")))
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Live demo"),
                        TextBlock("Edit the HTML on the left. The middle pane shows the sanitized string DOMPurify produces; the right pane shows what actually renders. Note that the script tag, onerror handler, javascript: URL and onclick attribute are all dropped."),
                        HStack().WS().Children(
                            VStack().Grow().Children(
                                SampleSubTitle("Input HTML"),
                                input
                            ),
                            VStack().Grow().PL(16).Children(
                                SampleSubTitle("Sanitized HTML"),
                                output
                            ),
                            VStack().Grow().PL(16).Children(
                                SampleSubTitle("Rendered"),
                                live
                            )
                        ),
                        SampleSubTitle("One-liner"),
                        TextBlock("Sanitize once, inject anywhere safe:"),
                        MarkdownBlock("```csharp\nvar safe = SanitizeHTML.Sanitize(untrustedHtml);\nsomeElement.innerHTML = safe;\n```")
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
