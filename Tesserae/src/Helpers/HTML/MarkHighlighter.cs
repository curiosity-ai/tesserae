using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transpose;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Marks every occurrence of a keyword inside a DOM subtree (same-origin iframes included) by
    /// wrapping the matching text in mark elements, and unmarks them again by unwrapping.
    /// Adapted from mark.js (https://github.com/julkue/mark.js, MIT).
    /// </summary>
    [Transpose.Name("tss.MarkHighlighter")]
    public static class MarkHighlighter
    {
        public static string Element   = "mark";
        public static string MarkData  = "marked";
        public static string ClassName = null;

        private const string FOCUSED_CLASS_NAME = "tss-highlight-focused";

        private static readonly string[] ExcludedTags = new[] { "script", "style", "title", "head", "html" };

        public static async Task MarkAsync(HTMLElement ctx, string keyword, Action<Node> eachCb, CancellationToken cancellationToken)
        {
            if (ctx is null) return;
            if (string.IsNullOrWhiteSpace(keyword)) return;

            var regex = RegExpCreator.Create(keyword);

            await DOMIterator.ForEachNodeAsync<Text>(ctx, textNode =>
                {
                    var node = textNode;

                    if (string.IsNullOrWhiteSpace(node.textContent)) return;
                    if (IsInsideExcludedElement(node)) return;

                    // The regex is global and shared across nodes, so each node starts from position 0
                    regex.lastIndex = 0;
                    var match = regex.exec(node.textContent);

                    while (match is object && match[0].Length > 0)
                    {
                        node = WrapGroups(node, match.index.As<uint>(), match[0].Length.As<uint>(), eachCb);

                        // node is now the text that follows the wrapped match, so the next search
                        // starts at the beginning of that remainder
                        regex.lastIndex = 0;
                        match           = regex.exec(node.textContent);
                    }
                },
                whatToShow: DOMIterator.SHOW_TEXT,
                nodeFilter: null,
                cancellationToken: cancellationToken);
        }

        public static Text WrapGroups(Text node, uint pos, uint len, Action<Node> eachCb)
        {
            node = WrapRangeInTextNode(node, pos, pos + len);
            eachCb?.Invoke(node.previousSibling);
            return node;
        }

        public static async Task UnmarkAsync(HTMLElement ctx)
        {
            if (ctx is null) return;

            var tagSelector = GetSelector();

            var nodeFilter = new NodeFilterObject()
            {
                acceptNode = (node) =>
                {
                    var matchesSel     = node.matches(tagSelector);
                    var matchesExclude = ExcludedTags.Any(s => node.matches(s));

                    if (!matchesSel || matchesExclude)
                    {
                        return DOMIterator.FILTER_REJECT;
                    }
                    else
                    {
                        return DOMIterator.FILTER_ACCEPT;
                    }
                }
            }.As<NodeFilter>();

            await DOMIterator.ForEachNodeAsync<HTMLElement>(ctx,
                eachCb: node =>
                {
                    UnwrapMatches(node);
                },
                whatToShow: DOMIterator.SHOW_ELEMENT,
                nodeFilter: nodeFilter,
                CancellationToken.None);
        }

        public static void FocusResult(HTMLElement ctx, HTMLElement elementToFocus, bool scrollIntoViewIfNeeded)
        {
            if (ctx is null || elementToFocus is null) return;

            // An iframe's document doesn't see the page stylesheet, so the theme color is resolved
            // here and applied inline; resolved per call so a theme switch is picked up
            var variableName   = Theme.Danger.Background.Substring("var(".Length, Theme.Danger.Background.Length - "var(".Length - ")".Length);
            var highlightColor = getComputedStyle(document.body).getPropertyValue(variableName);

            DOMIterator.QuerySelectorAllIframesRecursive(ctx, "." + FOCUSED_CLASS_NAME, n =>
            {
                n.classList.remove(FOCUSED_CLASS_NAME);
                n.removeAttribute("style");
            });

            elementToFocus.classList.add(FOCUSED_CLASS_NAME);
            elementToFocus.style.backgroundColor = highlightColor;

            if (scrollIntoViewIfNeeded)
            {
                elementToFocus.scrollIntoViewIfNeeded();
            }
        }

        private static Text WrapRangeInTextNode(Text node, uint start, uint end)
        {
            Text        startNode = node.splitText(start);
            Text        ret       = startNode.splitText(end - start);
            HTMLElement repl      = document.createElement(Element);
            repl.setAttribute("data-" + MarkData, "true");

            if (!string.IsNullOrEmpty(ClassName))
            {
                repl.setAttribute("class", ClassName);
            }
            repl.textContent = startNode.textContent;
            startNode.parentNode.replaceChild(repl, startNode);
            return ret;
        }

        private static bool IsInsideExcludedElement(Node node)
        {
            var parent = node.parentElement;

            if (parent is null) return false;
            return ExcludedTags.Any(s => parent.matches(s)) || parent.matches(GetSelector());
        }

        private static string GetSelector()
        {
            var sel = !string.IsNullOrWhiteSpace(Element) ? Element : "*";
            sel += $"[data-{MarkData}]";

            if (!string.IsNullOrWhiteSpace(ClassName))
            {
                sel += $".{ClassName}";
            }
            return sel;
        }

        private static void UnwrapMatches(Node node)
        {
            var parent = node.parentNode;

            while (node.firstChild != null)
            {
                parent.insertBefore(node.firstChild, node);
            }
            parent.removeChild(node);
            parent.normalize();
        }

        [ObjectLiteral]
        private class NodeFilterObject
        {
            public Func<HTMLElement, uint> acceptNode { get; set; }
        }
    }
}
