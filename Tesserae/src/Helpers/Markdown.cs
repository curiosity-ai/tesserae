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
    /// <para>
    /// A <c>```mermaid</c> fence is drawn as a diagram by <see href="https://pele.run">Pele</see> in the elements
    /// <see cref="RenderMarkdownSanitized"/> and <see cref="MarkdownBlock"/> produce. Pele is loaded on demand, the
    /// first time a diagram appears, and the fence stays as a code block until then and when it cannot be drawn.
    /// <see cref="ConvertMarkdownSanitized"/> only returns HTML, so its fence is left as the code block marked
    /// <c>language-mermaid</c>.
    /// </para>
    /// <para>
    /// A <c>```flint</c> fence is drawn as a chart by <see href="https://github.com/microsoft/flint-chart">Flint</see>
    /// when it is asked for: <see cref="MarkdownBlock.Flint(bool)"/>, or the overload of
    /// <see cref="RenderMarkdownSanitized(string, MarkdownSanitization, IFlintTheme, bool)"/> that takes a theme.
    /// </para>
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
            => ConvertMarkdownSanitized(text, null, sanitization);

        /// <summary>
        /// Parses the given markdown <paramref name="text"/> and runs the resulting HTML through DOMPurify, keeping
        /// only the embedded content <paramref name="sources"/> allows: an image, a <c>srcset</c>, a poster, a CSS
        /// <c>url(...)</c> from anywhere else loses the attribute that would have fetched it. Links stay.
        /// </summary>
        /// <param name="text">The Markdown source.</param>
        /// <param name="sources">Where the Markdown may load content from. <c>null</c> is <see cref="MarkdownContentSources.Any"/>.</param>
        /// <param name="sanitization">How strictly to sanitize the parsed HTML. See <see cref="MarkdownSanitization"/>.</param>
        public static string ConvertMarkdownSanitized(string text, MarkdownContentSources sources, MarkdownSanitization sanitization = MarkdownSanitization.Default)
        {
            var marked = GetShared();
            var parsedAsMarkdown = Script.Write<string>("{0}.parse({1})", marked, text);
            var cleaned = RemoveExcessiveNewLines(parsedAsMarkdown);

            var config  = sanitization == MarkdownSanitization.NoLinksOrEmbeddedContent ? GetNoLinksOrEmbeddedContentConfig() : null;

            //The filter is part of the DOMPurify pass itself, so it runs before any element exists that could fetch.
            if (MarkdownContentSources.Restricts(sources)) return sources.Sanitize(cleaned, config);

            if (config != null)
            {
                return Script.Write<string>("DOMPurify.sanitize({0}, {1})", cleaned, config);
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
            => Render(text, sanitization, null, charts: null);

        /// <summary>
        /// Parses and sanitizes the markdown like <see cref="RenderMarkdownSanitized(string, MarkdownSanitization)"/>,
        /// loading embedded content only from <paramref name="sources"/>: in the prose, and in the pictures of a
        /// <c>```mermaid</c> diagram, whenever it is drawn.
        /// </summary>
        /// <param name="text">The Markdown source.</param>
        /// <param name="sources">Where the Markdown may load content from. <c>null</c> is <see cref="MarkdownContentSources.Any"/>.</param>
        /// <param name="sanitization">How strictly to sanitize the parsed HTML.</param>
        public static HTMLElement RenderMarkdownSanitized(string text, MarkdownContentSources sources, MarkdownSanitization sanitization = MarkdownSanitization.Default)
            => Render(text, sanitization, sources, charts: null);

        /// <summary>
        /// Parses and sanitizes the markdown like <see cref="RenderMarkdownSanitized(string, MarkdownSanitization)"/>, and
        /// also draws every <c>```flint</c> fence as a chart, in <paramref name="flintTheme"/> (see
        /// <see cref="MarkdownBlock.Flint(IFlintTheme, bool)"/>).
        /// </summary>
        /// <param name="text">The Markdown source.</param>
        /// <param name="sanitization">How strictly to sanitize the parsed HTML.</param>
        /// <param name="flintTheme">The theme for a chart whose fence names none. <c>null</c> is <see cref="FlintThemes.Tesserae"/>.</param>
        /// <param name="overrideFenceTheme">Whether <paramref name="flintTheme"/> also replaces a <c>theme_spec</c> the fence states.</param>
        public static HTMLElement RenderMarkdownSanitized(string text, MarkdownSanitization sanitization, IFlintTheme flintTheme, bool overrideFenceTheme = false)
            => Render(text, sanitization, null, new MarkdownChartOptions(flintTheme, overrideFenceTheme));

        /// <summary>
        /// Parses and sanitizes the markdown and draws its <c>```flint</c> fences like
        /// <see cref="RenderMarkdownSanitized(string, MarkdownSanitization, IFlintTheme, bool)"/>, loading embedded
        /// content only from <paramref name="sources"/>: in the prose, in a diagram's pictures, and in a chart, which
        /// stays a code block when its spec would load from somewhere else (a <c>data.url</c>, a map's outlines).
        /// </summary>
        /// <param name="text">The Markdown source.</param>
        /// <param name="sources">Where the Markdown may load content from. <c>null</c> is <see cref="MarkdownContentSources.Any"/>.</param>
        /// <param name="sanitization">How strictly to sanitize the parsed HTML.</param>
        /// <param name="flintTheme">The theme for a chart whose fence names none. <c>null</c> is <see cref="FlintThemes.Tesserae"/>.</param>
        /// <param name="overrideFenceTheme">Whether <paramref name="flintTheme"/> also replaces a <c>theme_spec</c> the fence states.</param>
        public static HTMLElement RenderMarkdownSanitized(string text, MarkdownContentSources sources, MarkdownSanitization sanitization, IFlintTheme flintTheme, bool overrideFenceTheme = false)
            => Render(text, sanitization, sources, new MarkdownChartOptions(flintTheme, overrideFenceTheme));

        private static HTMLElement Render(string text, MarkdownSanitization sanitization, MarkdownContentSources sources, MarkdownChartOptions charts)
        {
            var convertedText = ConvertMarkdownSanitized(text, sources, sanitization);
            var el            = Raw(convertedText, forceParseAsHTML: true);
            el.classList.add("tss-markdown");
            el.style.whiteSpace = "break-spaces";
            MarkdownDiagrams.Render(el, sanitization, sources);
            MarkdownCharts.Render(el, sanitization, sources, charts);
            return el;
        }
    }
}
