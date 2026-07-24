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
                // A marked instance configured with a KaTeX math extension so that inline ($...$)
                // and block ($$...$$) math render as formulas via the bundled katex library.
                // The tokenizer/renderer logic is ported from marked-katex-extension (MIT):
                // https://github.com/UziTech/marked-katex-extension
                // \${1,2} is written as \$\$? so the template carries no {n}-style token.
                _shared = Script.Write<object>(@"(function () {
    var m = new globalThis.marked.Marked({ async: false, breaks: false, silent: false, pedantic: false, gfm: true });
    var inlineRule = /^(\$\$?)(?!\$)((?:\\.|[^\\\n])*?(?:\\.|[^\\\n$]))\1(?=[\s?!\.,:？！。，：]|$)/;
    var blockRule = /^(\$\$?)\n((?:\\[^]|[^\\])+?)\n\1(?:\n|$)/;
    function render(token) {
        if (!globalThis.katex) { return token.raw; }
        return globalThis.katex.renderToString(token.text, { throwOnError: false, displayMode: token.displayMode });
    }
    var inlineKatex = {
        name: 'inlineKatex',
        level: 'inline',
        start: function (src) {
            var index;
            var indexSrc = src;
            while (indexSrc) {
                index = indexSrc.indexOf('$');
                if (index === -1) { return; }
                if (index === 0 || indexSrc.charAt(index - 1) === ' ') {
                    if (indexSrc.substring(index).match(inlineRule)) { return index; }
                }
                indexSrc = indexSrc.substring(index + 1).replace(/^\$+/, '');
            }
        },
        tokenizer: function (src, tokens) {
            var match = src.match(inlineRule);
            if (match) { return { type: 'inlineKatex', raw: match[0], text: match[2].trim(), displayMode: match[1].length === 2 }; }
        },
        renderer: render
    };
    var blockKatex = {
        name: 'blockKatex',
        level: 'block',
        tokenizer: function (src, tokens) {
            var match = src.match(blockRule);
            if (match) { return { type: 'blockKatex', raw: match[0], text: match[2].trim(), displayMode: match[1].length === 2 }; }
        },
        renderer: function (token) { return render(token) + '\n'; }
    };
    m.use({ extensions: [inlineKatex, blockKatex] });
    return m;
})()");
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
