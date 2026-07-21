using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Static helpers for converting Markdown text to sanitized HTML using the
    /// globally-loaded <c>marked</c> and <c>DOMPurify</c> libraries.
    /// The dependencies are bundled with Tesserae and always loaded - no preload step is required.
    /// </summary>
    [Transpose.Name("tss.Markdown")]
    public static class Markdown
    {
        private static object _shared;

        private static object GetShared()
        {
            if (_shared == null)
            {
                _shared = Script.Write<object>("new globalThis.marked.Marked({async:false, breaks:false, silent:false, pedantic:false, gfm:true})");
            }
            return _shared;
        }

        /// <summary>
        /// Parses the given markdown <paramref name="text"/> and runs the resulting HTML through DOMPurify.
        /// </summary>
        public static string ConvertMarkdownSanitized(string text)
        {
            var marked = GetShared();
            var parsedAsMarkdown = Script.Write<string>("{0}.parse({1})", marked, text);
            var cleaned = RemoveExcessiveNewLines(parsedAsMarkdown);
            var sanitized = Script.Write<string>("DOMPurify.sanitize({0})", cleaned);
            return sanitized;
        }

        private static string RemoveExcessiveNewLines(string markedOutput)
        {
            return Script.Write<string>("{0}.replace(/>\\r?\\n</g, \"><\")", markedOutput);
        }

        /// <summary>
        /// Parses and sanitizes the markdown, then wraps the resulting HTML in a span with the
        /// <c>tss-markdown</c> class so it picks up the default Tesserae markdown styling.
        /// </summary>
        public static HTMLElement RenderMarkdownSanitized(string text)
        {
            var convertedText = ConvertMarkdownSanitized(text);
            var el            = Raw(convertedText, forceParseAsHTML: true);
            el.classList.add("tss-markdown");
            el.style.whiteSpace = "break-spaces";
            return el;
        }
    }
}
