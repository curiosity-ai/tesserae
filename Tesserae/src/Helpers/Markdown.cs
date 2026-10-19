using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Static helpers for converting Markdown text to sanitized HTML using the
    /// globally-loaded <c>marked</c> and <c>DOMPurify</c> libraries.
    /// A <c>```mermaid</c> block is drawn as a diagram by the Mermaid library, loaded lazily the first time one appears
    /// (never under <see cref="MarkdownSanitization.NoLinksOrEmbeddedContent"/>, which forbids SVG).
    /// The dependencies are bundled with Tesserae and always loaded - no preload step is required.
    /// </summary>
    [Transpose.Name("tss.Markdown")]
    public static class Markdown
    {
        private static object _shared;
        private static object _noLinksOrEmbeddedContentConfig;

        private static object GetShared()
        {
            if (_shared == null)
            {
                _shared = Script.Write<object>("new globalThis.marked.Marked({async:false, breaks:false, silent:false, pedantic:false, gfm:true})");
            }
            return _shared;
        }

        /// <summary>
        /// The DOMPurify configuration behind <see cref="MarkdownSanitization.NoLinksOrEmbeddedContent"/>:
        /// every element that navigates or fetches a URL, plus the attributes that carry one (a style
        /// attribute included - <c>background-image: url(...)</c> loads a remote image as surely as an
        /// <c>img</c> does). DOMPurify keeps a forbidden element's children by default, so an anchor
        /// leaves its label behind as plain text while an image, having no children, disappears with
        /// its tag.
        /// </summary>
        private static object GetNoLinksOrEmbeddedContentConfig()
        {
            if (_noLinksOrEmbeddedContentConfig == null)
            {
                _noLinksOrEmbeddedContentConfig = Script.Write<object>(
                    "{ FORBID_TAGS: ['a','area','map','img','image','picture','source','track','svg','use','video','audio','iframe','frame','frameset','embed','object','portal','link','base','style','form','input','button','textarea'],"
                  + "  FORBID_ATTR: ['href','xlink:href','src','srcset','sizes','poster','background','data','action','formaction','ping','usemap','longdesc','style'] }");
            }
            return _noLinksOrEmbeddedContentConfig;
        }

        /// <summary>
        /// Parses the given markdown <paramref name="text"/> and runs the resulting HTML through DOMPurify.
        /// </summary>
        /// <param name="text">The Markdown source.</param>
        /// <param name="sanitization">How strictly to sanitize the parsed HTML. See <see cref="MarkdownSanitization"/>.</param>
        public static string ConvertMarkdownSanitized(string text, MarkdownSanitization sanitization = MarkdownSanitization.Default)
        {
            var marked = GetShared();
            var parsedAsMarkdown = Script.Write<string>("{0}.parse({1})", marked, text);
            var cleaned = RemoveExcessiveNewLines(parsedAsMarkdown);

            if (sanitization == MarkdownSanitization.NoLinksOrEmbeddedContent)
            {
                return Script.Write<string>("DOMPurify.sanitize({0}, {1})", cleaned, GetNoLinksOrEmbeddedContentConfig());
            }

            var sanitized = Script.Write<string>("DOMPurify.sanitize({0})", cleaned);
            return sanitized;
        }

        /// <summary>The URL <see cref="RenderMermaidDiagrams"/> loads the Mermaid library from, on first use.</summary>
        public const string MermaidScriptUrl = "assets/js/tss-mermaid.min.js";

        private static int _mermaidCounter;

        /// <summary>
        /// Replaces every <c>```mermaid</c> code block under <paramref name="root"/> with the diagram it describes.
        /// The Mermaid library is not part of the always-loaded bundle: it is fetched the first time a page has
        /// a diagram to draw. A block that fails to parse stays as its source, with the error beneath it.
        /// Meant for HTML that came out of <see cref="ConvertMarkdownSanitized"/>; <see cref="MarkdownBlock"/>
        /// and <see cref="RenderMarkdownSanitized"/> call it themselves.
        /// </summary>
        public static void RenderMermaidDiagrams(HTMLElement root)
        {
            var blocks = root.querySelectorAll("pre > code.language-mermaid");
            if (blocks.length == 0) return;

            foreach (var found in blocks)
            {
                RenderMermaidDiagramAsync(found.As<HTMLElement>()).FireAndForget();
            }
        }

        private static async Task RenderMermaidDiagramAsync(HTMLElement code)
        {
            var pre    = code.parentElement;
            var source = code.textContent ?? string.Empty;

            try
            {
                await Require.LoadScriptAsync(MermaidScriptUrl);

                //The text was set again while the library loaded, and this block went with it.
                if (pre.parentElement == null) return;

                var dark = document.body.classList.contains("tss-dark-mode");
                var id   = "tss-mermaid-" + (++_mermaidCounter);

                //Mermaid is an untyped global with no C# surface; strict security level makes it sanitize its own output.
                var svg = await Script.Write<Task<string>>(
                    "(async function(){ var m = globalThis.mermaid; m.initialize({ startOnLoad:false, securityLevel:'strict', theme:{0} ? 'dark' : 'default' });"
                  + " return (await m.render({1}, {2})).svg; })()", dark, id, source);

                if (pre.parentElement == null) return;

                var host = Div(Att("tss-mermaid"));
                host.innerHTML = svg;
                pre.parentElement.replaceChild(host, pre);
            }
            catch (Exception e)
            {
                //A failed render leaves mermaid's own error element on the body.
                Script.Write("var leftover = document.getElementById('d' + {0}); if (leftover) leftover.remove();", "tss-mermaid-" + _mermaidCounter);
                if (pre.parentElement == null) return;
                pre.classList.add("tss-mermaid-error");
                pre.title = e.Message;
            }
        }

        private static string RemoveExcessiveNewLines(string markedOutput)
        {
            return Script.Write<string>("{0}.replace(/>\\r?\\n</g, \"><\")", markedOutput);
        }

        /// <summary>
        /// Parses and sanitizes the markdown, then wraps the resulting HTML in a span with the
        /// <c>tss-markdown</c> class so it picks up the default Tesserae markdown styling.
        /// </summary>
        public static HTMLElement RenderMarkdownSanitized(string text, MarkdownSanitization sanitization = MarkdownSanitization.Default)
        {
            var convertedText = ConvertMarkdownSanitized(text, sanitization);
            var el            = Raw(convertedText, forceParseAsHTML: true);
            el.classList.add("tss-markdown");
            el.style.whiteSpace = "break-spaces";
            if (sanitization == MarkdownSanitization.Default) RenderMermaidDiagrams(el);
            return el;
        }
    }
}
