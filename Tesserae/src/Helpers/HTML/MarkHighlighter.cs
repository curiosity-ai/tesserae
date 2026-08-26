using System;
using System.Collections.Generic;
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
    /// Passes on the same root are serialized: starting a new mark or unmark cancels and awaits the
    /// previous one, so callers can fire on every keystroke without racing themselves.
    /// Adapted from mark.js (https://github.com/julkue/mark.js, MIT).
    /// </summary>
    [Transpose.Name("tss.MarkHighlighter")]
    public static class MarkHighlighter
    {
        /// <summary>Default wrapper tag, used when <see cref="MarkOptions.Element"/> is unset.</summary>
        public static string Element = "mark";

        /// <summary>Default data-* attribute name, used when <see cref="MarkOptions.MarkData"/> is unset.</summary>
        public static string MarkData = "marked";

        /// <summary>Default extra class on each wrapper, used when <see cref="MarkOptions.ClassName"/> is unset.</summary>
        public static string ClassName = null;

        private const string FOCUSED_CLASS_NAME    = "tss-highlight-focused";
        private const string EXCLUDED_TAGS_SELECTOR = "script,style,title,head,html";

        // One in-flight pass per root: ctx -> Pass. A WeakMap, so a discarded root never pins its entry.
        private static readonly WeakMap _passes = new WeakMap();

        private sealed class Pass
        {
            public CancellationTokenSource CTS;
            public Task                    Task;
        }

        public static Task MarkAsync(HTMLElement ctx, string keyword, Action<Node> eachCb, CancellationToken cancellationToken)
        {
            return MarkAsync(ctx, keyword, null, eachCb, cancellationToken);
        }

        public static async Task MarkAsync(HTMLElement ctx, string keyword, MarkOptions options, Action<Node> eachCb, CancellationToken cancellationToken)
        {
            if (ctx is null) return;
            if (string.IsNullOrWhiteSpace(keyword)) return;

            await RunExclusiveAsync(ctx, token => MarkCoreAsync(ctx, keyword, options, eachCb, token), cancellationToken);
        }

        public static async Task UnmarkAsync(HTMLElement ctx, MarkOptions options = null, CancellationToken cancellationToken = default)
        {
            if (ctx is null) return;

            await RunExclusiveAsync(ctx, token => UnmarkCoreAsync(ctx, options, token), cancellationToken);
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
                elementToFocus.scrollIntoView(new ScrollIntoViewOptions { block = ScrollLogicalPosition.nearest, inline = ScrollLogicalPosition.nearest });
            }
        }

        public static Text WrapGroups(Text node, uint pos, uint len, Action<Node> eachCb) => WrapGroups(node, pos, len, eachCb, null);

        public static Text WrapGroups(Text node, uint pos, uint len, Action<Node> eachCb, MarkOptions options)
        {
            node = WrapRangeInTextNode(node, pos, pos + len, options);
            eachCb?.Invoke(node.previousSibling);
            return node;
        }

        /// <summary>
        /// Runs one pass at a time per root: a new pass cancels the in-flight one and waits for it
        /// to finish, so a mark can never interleave with the unmark it replaces
        /// </summary>
        private static async Task RunExclusiveAsync(HTMLElement ctx, Func<CancellationToken, Task> pass, CancellationToken cancellationToken)
        {
            var previous = _passes.Get(ctx).As<Pass>();

            if (previous is object)
            {
                previous.CTS.Cancel();

                if (previous.Task is object)
                {
                    try
                    {
                        await previous.Task;
                    }
                    catch (Exception)
                    {
                        // the previous pass reported its failure to its own caller; here it only
                        // matters that it is no longer running
                    }
                }
            }

            var cts     = CancellationTokenSource.CreateLinkedTokenSource(new[] { cancellationToken });
            var current = new Pass { CTS = cts };
            _passes.Set(ctx, current);

            try
            {
                var task     = pass(cts.Token);
                current.Task = task;
                await task;
            }
            finally
            {
                if (_passes.Get(ctx).As<Pass>() == current) _passes.Delete(ctx);
                cts.Dispose();
            }
        }

        private static async Task MarkCoreAsync(HTMLElement ctx, string keyword, MarkOptions options, Action<Node> eachCb, CancellationToken cancellationToken)
        {
            var regex = RegExpCreator.Create(keyword, options);
            if (regex is null) return;

            var excludeSelector = GetExcludeSelector(options);

            if (options?.AcrossElements ?? false)
            {
                await MarkAcrossElementsAsync(ctx, regex, options, excludeSelector, eachCb, cancellationToken);
                return;
            }

            await DOMIterator.ForEachNodeAsync<Text>(ctx, textNode =>
                {
                    var node = textNode;

                    if (string.IsNullOrWhiteSpace(node.textContent)) return;
                    if (IsInsideExcludedElement(node, excludeSelector)) return;

                    // The regex is global and shared across nodes, so each node starts from position 0
                    regex.lastIndex = 0;
                    var match = regex.exec(node.textContent);

                    while (match is object && match[0].Length > 0)
                    {
                        node = WrapGroups(node, match.index.As<uint>(), match[0].Length.As<uint>(), eachCb, options);

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

        private sealed class MappedTextNode
        {
            public int  Start;
            public int  End;
            public Text Node;
        }

        private static async Task MarkAcrossElementsAsync(HTMLElement ctx, es5.RegExp regex, MarkOptions options, string excludeSelector, Action<Node> eachCb, CancellationToken cancellationToken)
        {
            // Text nodes are collected first and grouped per document, so a match can span inline
            // elements but never leak from the page into an iframe (whose nodes arrive later and
            // possibly out of order)
            var groups       = new List<List<Text>>();
            List<Text> group = null;

            await DOMIterator.ForEachNodeAsync<Text>(ctx, textNode =>
                {
                    if (IsInsideExcludedElement(textNode, excludeSelector)) return;

                    if (group is null || group[0].ownerDocument != textNode.ownerDocument)
                    {
                        group = groups.FirstOrDefault(g => g[0].ownerDocument == textNode.ownerDocument);

                        if (group is null)
                        {
                            group = new List<Text>();
                            groups.Add(group);
                        }
                    }
                    group.Add(textNode);
                },
                whatToShow: DOMIterator.SHOW_TEXT,
                nodeFilter: null,
                cancellationToken: cancellationToken);

            foreach (var textNodes in groups)
            {
                if (cancellationToken.IsCancellationRequested) return;
                WrapMatchesAcrossElements(textNodes, regex, options, eachCb);
            }
        }

        private static void WrapMatchesAcrossElements(List<Text> textNodes, es5.RegExp regex, MarkOptions options, Action<Node> eachCb)
        {
            var value = "";
            var nodes = new List<MappedTextNode>();

            foreach (var node in textNodes)
            {
                var start = value.Length;
                value += node.textContent;
                nodes.Add(new MappedTextNode { Start = start, End = value.Length, Node = node });
            }

            regex.lastIndex = 0;
            var match = regex.exec(value);

            while (match is object && match[0].Length > 0)
            {
                var start = match.index.As<int>();
                var end   = start + match[0].Length;

                // Wrapping consumes the matched text from value and shifts the mapped offsets, so
                // the next exec continues on the shrunken string from where the last wrap ended
                regex.lastIndex = WrapRangeAcrossNodes(nodes, ref value, start, end, options, eachCb);
                match           = regex.exec(value);
            }
        }

        private static int WrapRangeAcrossNodes(List<MappedTextNode> nodes, ref string value, int start, int end, MarkOptions options, Action<Node> eachCb)
        {
            var lastIndex = 0;

            for (var i = 0; i < nodes.Count; i++)
            {
                var current = nodes[i];
                var next    = (i + 1 < nodes.Count) ? nodes[i + 1] : null;

                if (next is object && next.Start <= start) continue;

                var s = start - current.Start;
                var e = ((end > current.End) ? current.End : end) - current.Start;

                var startStr = value.Substring(0, current.Start);
                var endStr   = value.Substring(e + current.Start);

                current.Node = WrapRangeInTextNode(current.Node, s.As<uint>(), e.As<uint>(), options);
                value        = startStr + endStr;

                for (var j = i; j < nodes.Count; j++)
                {
                    if (j != i && nodes[j].Start > 0) nodes[j].Start -= e;
                    nodes[j].End -= e;
                }
                end -= e;

                lastIndex = current.Start;
                eachCb?.Invoke(current.Node.previousSibling);

                if (end > current.Start)
                {
                    start = current.Start;
                }
                else
                {
                    break;
                }
            }
            return lastIndex;
        }

        private static async Task UnmarkCoreAsync(HTMLElement ctx, MarkOptions options, CancellationToken cancellationToken)
        {
            var tagSelector = GetSelector(options);

            var nodeFilter = new NodeFilterObject()
            {
                acceptNode = (node) => (node.matches(tagSelector) && !node.matches(EXCLUDED_TAGS_SELECTOR))
                    ? DOMIterator.FILTER_ACCEPT
                    : DOMIterator.FILTER_REJECT
            }.As<NodeFilter>();

            // Collect first so the iterator never walks a mutating tree, then unwrap and normalize
            // each affected parent once instead of once per mark
            var marks = new List<HTMLElement>();

            await DOMIterator.ForEachNodeAsync<HTMLElement>(ctx,
                eachCb: node => marks.Add(node),
                whatToShow: DOMIterator.SHOW_ELEMENT,
                nodeFilter: nodeFilter,
                cancellationToken);

            var parents = new List<Node>();

            foreach (var mark in marks)
            {
                var parent = mark.parentNode;
                UnwrapMatches(mark);
                if (!parents.Contains(parent)) parents.Add(parent);
            }

            foreach (var parent in parents)
            {
                parent.normalize();
            }
        }

        private static Text WrapRangeInTextNode(Text node, uint start, uint end, MarkOptions options)
        {
            Text        startNode = node.splitText(start);
            Text        ret       = startNode.splitText(end - start);
            HTMLElement repl      = document.createElement(options?.Element ?? Element);
            repl.setAttribute("data-" + (options?.MarkData ?? MarkData), "true");

            var className = options?.ClassName ?? ClassName;

            if (!string.IsNullOrEmpty(className))
            {
                repl.setAttribute("class", className);
            }
            repl.textContent = startNode.textContent;
            startNode.parentNode.replaceChild(repl, startNode);
            return ret;
        }

        private static bool IsInsideExcludedElement(Node node, string excludeSelector)
        {
            var parent = node.parentElement;

            if (parent is null) return false;
            return parent.matches(excludeSelector);
        }

        private static string GetExcludeSelector(MarkOptions options)
        {
            // One selector list, so exclusion is a single matches() call per text node
            return EXCLUDED_TAGS_SELECTOR + "," + GetSelector(options);
        }

        private static string GetSelector(MarkOptions options)
        {
            var element   = options?.Element ?? Element;
            var markData  = options?.MarkData ?? MarkData;
            var className = options?.ClassName ?? ClassName;

            var sel = !string.IsNullOrWhiteSpace(element) ? element : "*";
            sel += $"[data-{markData}]";

            if (!string.IsNullOrWhiteSpace(className))
            {
                sel += $".{className}";
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
        }

        [ObjectLiteral]
        private class NodeFilterObject
        {
            public Func<HTMLElement, uint> acceptNode { get; set; }
        }
    }
}
