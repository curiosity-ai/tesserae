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
            return el;
        }
    }
}
