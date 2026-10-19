using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Draws the <c>```mermaid</c> fences of rendered Markdown as diagrams, with
    /// <see href="https://pele.run">Pele</see>. Pele is not part of the bundle the page starts with: it ships as
    /// its own file and is fetched the first time a Markdown source actually contains a diagram.
    /// </summary>
    /// <remarks>
    /// The fence stays on the page as a plain code block until the library is there, and stays for good when the
    /// diagram cannot be drawn (a block still being streamed, a typo), with the reason as its tooltip.
    /// </remarks>
    internal static class MarkdownDiagrams
    {
        /// <summary>Where the compiler puts the unlinked <c>tss-pele.min.js</c> resource of <c>tps.json</c>.</summary>
        internal const string ScriptUrl = "assets/js/tss-pele.min.js";

        private static Task _loading;
        private static bool _loaded;

        /// <summary>Turns every <c>mermaid</c> code block under <paramref name="root"/> into a diagram.</summary>
        internal static void Render(HTMLElement root, MarkdownSanitization sanitization)
        {
            foreach (var found in root.querySelectorAll("pre > code.language-mermaid"))
            {
                var code = found.As<HTMLElement>();
                var pre  = code.parentElement;
                var text = code.textContent ?? string.Empty;

                if (_loaded)
                {
                    //Synchronous once loaded, so a source that is re-rendered as it streams in does not flash its code block.
                    Draw(pre, text, sanitization);
                }
                else
                {
                    DrawWhenLoaded(pre, text, sanitization).FireAndForget();
                }
            }
        }

        /// <summary>Stops the diagrams under <paramref name="root"/> watching their container. Call before it is emptied.</summary>
        internal static void Dispose(HTMLElement root)
        {
            foreach (var found in root.querySelectorAll(".tss-mermaid"))
            {
                Script.Write("if ({0}._pele) { {0}._pele.destroy(); {0}._pele = null; }", found);
            }
        }

        private static async Task DrawWhenLoaded(HTMLElement pre, string text, MarkdownSanitization sanitization)
        {
            try
            {
                if (_loading is null) _loading = Require.LoadScriptAsync(ScriptUrl);
                await _loading;
                _loaded = true;
            }
            catch
            {
                _loading = null; //retried by the next Markdown that has a diagram
                return;
            }

            //The Markdown may have been re-rendered while the library was on its way.
            if (pre.parentElement is null) return;

            Draw(pre, text, sanitization);
        }

        private static void Draw(HTMLElement pre, string text, MarkdownSanitization sanitization)
        {
            var parent = pre.parentElement;
            if (parent is null) return;

            var host = Div(Att("tss-mermaid"));
            parent.insertBefore(host, pre);

            //Links and images in a diagram are as much a call out to a third party as the ones in the prose, so the
            //strict mode takes them out of the diagram too. The SVG goes through DOMPurify on top of Pele's own escaping.
            var allowRemote = sanitization != MarkdownSanitization.NoLinksOrEmbeddedContent;

            var mounted = Script.Write<object>(
                "globalThis.pele.mount({0}, {1}, { links: {2}, images: {2}, sanitize: function (svg) { return DOMPurify.sanitize(svg, { USE_PROFILES: { svg: true, svgFilters: true }, RETURN_DOM_FRAGMENT: true }); } })",
                host, text, allowRemote);

            var error = Script.Write<string>("{0}.error ? String({0}.error.message) : null", mounted);

            if (error != null)
            {
                Script.Write("{0}.destroy();", mounted);
                parent.removeChild(host);
                pre.title = error;
                return;
            }

            Script.Write("{0}._pele = {1};", host, mounted);
            parent.removeChild(pre);
        }
    }
}
