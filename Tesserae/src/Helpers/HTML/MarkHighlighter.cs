using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transpose;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;
using Range = Transpose.Core.dom.Range;

namespace Tesserae
{
    /// <summary>
    /// Marks every occurrence of a keyword inside a DOM subtree (same-origin iframes included) and
    /// unmarks it again. Where the browser supports the CSS Custom Highlight API, matches in the
    /// page itself are painted through the highlight registry (see tss.markhighlighter.css) with no
    /// DOM mutation at all; text inside iframes, browsers without the API, and callers that opt out
    /// get the classic wrap-in-mark-element treatment. Passes on the same root are serialized:
    /// starting a new mark or unmark cancels and awaits the previous one, so callers can fire on
    /// every keystroke without racing themselves.
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

        private const string FOCUSED_CLASS_NAME     = "tss-highlight-focused";
        private const string EXCLUDED_TAGS_SELECTOR = "script,style,title,head,html";

        // Registry names styled by tss.markhighlighter.css - one Highlight per name, shared by
        // every root on the page; per-root bookkeeping below says which ranges belong to whom
        private const string HIGHLIGHT_NAME         = "tss-marked";
        private const string HIGHLIGHT_FOCUSED_NAME = "tss-marked-focused";

        // One in-flight pass per root: ctx -> Pass. WeakMaps, so a discarded root never pins its entries.
        private static readonly WeakMap _passes              = new WeakMap();
        private static readonly WeakMap _registryRangesByRoot = new WeakMap(); // ctx -> List<Range>
        private static readonly WeakMap _focusedRangeByRoot   = new WeakMap(); // ctx -> Range

        private static bool? _highlightApiSupported;

        /// <summary>Whether this browser has the CSS Custom Highlight API (CSS.highlights and the Highlight constructor).</summary>
        public static bool IsHighlightApiSupported
        {
            get
            {
                // The Highlight constructor and the registry ship together, but a partial or
                // flag-gated implementation could carry one without the other - require both
                if (_highlightApiSupported is null)
                {
                    _highlightApiSupported = Script.Write<bool>("(typeof Highlight !== 'undefined' && typeof CSS !== 'undefined' && !!CSS.highlights)");
                }
                return _highlightApiSupported.Value;
            }
        }

        private sealed class Pass
        {
            public CancellationTokenSource CTS;
            public Task                    Task;
        }

        public static Task MarkAsync(HTMLElement ctx, string keyword, Action<Node> eachCb, CancellationToken cancellationToken)
        {
            // The element-only contract from before the highlight backend existed: every wrapper
            // element is reported individually
            return MarkAsync(ctx, keyword, new MarkOptions { UseHighlightApi = false }, match =>
            {
                if (eachCb is null || match.Elements is null) return;

                foreach (var element in match.Elements)
                {
                    eachCb(element);
                }
            }, cancellationToken);
        }

        public static async Task MarkAsync(HTMLElement ctx, string keyword, MarkOptions options, Action<MarkedMatch> eachCb, CancellationToken cancellationToken)
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

        public static void FocusResult(HTMLElement ctx, MarkedMatch match, bool scrollIntoViewIfNeeded)
        {
            if (ctx is null || match is null) return;

            ClearFocus(ctx);

            if (match.Range is object)
            {
                var focused = GetOrCreateHighlight(HIGHLIGHT_FOCUSED_NAME);
                Script.Write("{0}.priority = 1", focused); // wins over the base highlight where they overlap
                Script.Write("{0}.add({1})", focused, match.Range);
                _focusedRangeByRoot.Set(ctx, match.Range);

                if (scrollIntoViewIfNeeded) ScrollRangeIntoView(match.Range);
            }
            else if (match.Elements is object && match.Elements.Length > 0)
            {
                var highlightColor = ResolveFocusColor();

                foreach (var element in match.Elements)
                {
                    ApplyElementFocus(element, highlightColor);
                }

                if (scrollIntoViewIfNeeded)
                {
                    match.Elements[0].scrollIntoView(new ScrollIntoViewOptions { block = ScrollLogicalPosition.nearest, inline = ScrollLogicalPosition.nearest });
                }
            }
        }

        public static void FocusResult(HTMLElement ctx, HTMLElement elementToFocus, bool scrollIntoViewIfNeeded)
        {
            if (ctx is null || elementToFocus is null) return;

            ClearFocus(ctx);
            ApplyElementFocus(elementToFocus, ResolveFocusColor());

            if (scrollIntoViewIfNeeded)
            {
                elementToFocus.scrollIntoView(new ScrollIntoViewOptions { block = ScrollLogicalPosition.nearest, inline = ScrollLogicalPosition.nearest });
            }
        }

        public static Text WrapGroups(Text node, uint pos, uint len, Action<Node> eachCb)
        {
            node = WrapRangeInTextNode(node, pos, pos + len, null);
            eachCb?.Invoke(node.previousSibling);
            return node;
        }

        // ---- focus internals -------------------------------------------------------------------

        private static string ResolveFocusColor()
        {
            // An iframe's document doesn't see the page stylesheet, so the theme color is resolved
            // here and applied inline; resolved per call so a theme switch is picked up
            var variableName = Theme.Danger.Background.Substring("var(".Length, Theme.Danger.Background.Length - "var(".Length - ")".Length);
            return getComputedStyle(document.body).getPropertyValue(variableName);
        }

        private static void ApplyElementFocus(HTMLElement element, string highlightColor)
        {
            element.classList.add(FOCUSED_CLASS_NAME);
            element.style.backgroundColor = highlightColor;
        }

        private static void ClearFocus(HTMLElement ctx)
        {
            DOMIterator.QuerySelectorAllIframesRecursive(ctx, "." + FOCUSED_CLASS_NAME, n =>
            {
                n.classList.remove(FOCUSED_CLASS_NAME);
                n.removeAttribute("style");
            });

            if (!IsHighlightApiSupported) return;

            var previous = _focusedRangeByRoot.Get(ctx).As<Range>();

            if (previous is object)
            {
                Script.Write("{0}.delete({1})", GetOrCreateHighlight(HIGHLIGHT_FOCUSED_NAME), previous);
                _focusedRangeByRoot.Delete(ctx);
            }
        }

        /// <summary>
        /// Brings a highlight-registry match into view: unlike an element, a range has no
        /// scrollIntoView, so each scrollable ancestor is nudged until the range's rect is visible
        /// </summary>
        private static void ScrollRangeIntoView(Range range)
        {
            var node = range.startContainer.parentElement;

            while (node is object)
            {
                var rect       = range.getBoundingClientRect().As<ClientRect>();
                var isViewport = ReferenceEquals(node, document.documentElement) || ReferenceEquals(node, document.body);

                if (isViewport)
                {
                    if (rect.top < 0 || rect.bottom > window.innerHeight)
                    {
                        var scroller = (document.scrollingElement ?? document.documentElement).As<HTMLElement>();
                        scroller.scrollTop += rect.top - (window.innerHeight - rect.height) / 2;
                    }
                    return;
                }

                var overflowY  = getComputedStyle(node).overflowY;
                var scrollable = (overflowY == "auto" || overflowY == "scroll" || overflowY == "overlay") && node.scrollHeight > node.clientHeight;

                if (scrollable)
                {
                    var nodeRect = node.getBoundingClientRect().As<ClientRect>();

                    if (rect.top < nodeRect.top || rect.bottom > nodeRect.bottom)
                    {
                        node.scrollTop += rect.top - nodeRect.top - (nodeRect.height - rect.height) / 2;
                    }
                }
                node = node.parentElement;
            }
        }

        // ---- highlight registry internals ------------------------------------------------------

        private static bool RegistryEnabled(MarkOptions options)
        {
            if (options?.UseHighlightApi == false) return false;
            if (!IsHighlightApiSupported) return false;

            // A custom wrapper element, attribute or class asks for real DOM elements
            if (options?.UseHighlightApi is null && (options?.Element ?? options?.MarkData ?? options?.ClassName) is object) return false;
            return true;
        }

        // The Highlight API has no binding yet, so the registry is reached through narrow
        // interop; the Highlight objects stay behind these helpers as opaque handles
        private static object GetOrCreateHighlight(string name)
        {
            return Script.Write<object>("CSS.highlights.get({0}) || (CSS.highlights.set({0}, new Highlight()), CSS.highlights.get({0}))", name);
        }

        private static void AddRegistryRange(object highlight, HTMLElement ctx, Range range)
        {
            Script.Write("{0}.add({1})", highlight, range);

            var ranges = _registryRangesByRoot.Get(ctx).As<List<Range>>();

            if (ranges is null)
            {
                ranges = new List<Range>();
                _registryRangesByRoot.Set(ctx, ranges);
            }
            ranges.Add(range);
        }

        private static void UnmarkRegistry(HTMLElement ctx)
        {
            if (!IsHighlightApiSupported) return;

            var ranges = _registryRangesByRoot.Get(ctx).As<List<Range>>();
            if (ranges is null) return;

            var highlight = GetOrCreateHighlight(HIGHLIGHT_NAME);

            foreach (var range in ranges)
            {
                Script.Write("{0}.delete({1})", highlight, range);
            }
            _registryRangesByRoot.Delete(ctx);
        }

        // ---- pass serialization ----------------------------------------------------------------

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

        // ---- marking ---------------------------------------------------------------------------

        private static async Task MarkCoreAsync(HTMLElement ctx, string keyword, MarkOptions options, Action<MarkedMatch> eachCb, CancellationToken cancellationToken)
        {
            var regex = RegExpCreator.Create(keyword, options);
            if (regex is null) return;

            var excludeSelector = GetExcludeSelector(options);
            var registryEnabled = RegistryEnabled(options);
            var highlight       = registryEnabled ? GetOrCreateHighlight(HIGHLIGHT_NAME) : null;

            if (options?.AcrossElements ?? false)
            {
                await MarkAcrossElementsAsync(ctx, regex, options, excludeSelector, registryEnabled, highlight, eachCb, cancellationToken);
                return;
            }

            await DOMIterator.ForEachNodeAsync<Text>(ctx, textNode =>
                {
                    if (string.IsNullOrWhiteSpace(textNode.textContent)) return;
                    if (IsInsideExcludedElement(textNode, excludeSelector)) return;

                    // A registration is per document, and an iframe's document has no
                    // ::highlight() rules - so iframe text is always wrapped in elements
                    if (registryEnabled && ReferenceEquals(textNode.ownerDocument, document))
                    {
                        HighlightMatchesInNode(textNode, regex, ctx, highlight, eachCb);
                    }
                    else
                    {
                        WrapMatchesInNode(textNode, regex, options, eachCb);
                    }
                },
                whatToShow: DOMIterator.SHOW_TEXT,
                nodeFilter: null,
                cancellationToken: cancellationToken);
        }

        private static void HighlightMatchesInNode(Text textNode, es5.RegExp regex, HTMLElement ctx, object highlight, Action<MarkedMatch> eachCb)
        {
            // No mutation happens, so the node's text stays put and exec continues from lastIndex
            regex.lastIndex = 0;
            var match = regex.exec(textNode.textContent);

            while (match is object && match[0].Length > 0)
            {
                var range = document.createRange();
                range.setStart(textNode, match.index.As<uint>());
                range.setEnd(textNode, (match.index + match[0].Length).As<uint>());

                AddRegistryRange(highlight, ctx, range);
                eachCb?.Invoke(new MarkedMatch { Range = range, Text = match[0] });

                match = regex.exec(textNode.textContent);
            }
        }

        private static void WrapMatchesInNode(Text textNode, es5.RegExp regex, MarkOptions options, Action<MarkedMatch> eachCb)
        {
            var node = textNode;

            // The regex is global and shared across nodes, so each node starts from position 0
            regex.lastIndex = 0;
            var match = regex.exec(node.textContent);

            while (match is object && match[0].Length > 0)
            {
                var matchText = match[0];
                node = WrapRangeInTextNode(node, match.index.As<uint>(), (match.index + matchText.Length).As<uint>(), options);
                eachCb?.Invoke(new MarkedMatch { Elements = new[] { node.previousSibling.As<HTMLElement>() }, Text = matchText });

                // node is now the text that follows the wrapped match, so the next search starts
                // at the beginning of that remainder
                regex.lastIndex = 0;
                match           = regex.exec(node.textContent);
            }
        }

        // ---- across-element marking --------------------------------------------------------------

        private sealed class MappedTextNode
        {
            public int  Start;
            public int  End;
            public Text Node;
        }

        private static async Task MarkAcrossElementsAsync(HTMLElement ctx, es5.RegExp regex, MarkOptions options, string excludeSelector, bool registryEnabled, object highlight, Action<MarkedMatch> eachCb, CancellationToken cancellationToken)
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

                if (registryEnabled && ReferenceEquals(textNodes[0].ownerDocument, document))
                {
                    HighlightMatchesAcrossNodes(textNodes, regex, ctx, highlight, eachCb);
                }
                else
                {
                    WrapMatchesAcrossElements(textNodes, regex, options, eachCb);
                }
            }
        }

        private static List<MappedTextNode> MapTextNodes(List<Text> textNodes, out string value)
        {
            value     = "";
            var nodes = new List<MappedTextNode>();

            foreach (var node in textNodes)
            {
                var start = value.Length;
                value += node.textContent;
                nodes.Add(new MappedTextNode { Start = start, End = value.Length, Node = node });
            }
            return nodes;
        }

        private static void HighlightMatchesAcrossNodes(List<Text> textNodes, es5.RegExp regex, HTMLElement ctx, object highlight, Action<MarkedMatch> eachCb)
        {
            var nodes = MapTextNodes(textNodes, out var value);

            // One live range covers the whole spanning match, and nothing mutates, so matching
            // simply continues from lastIndex on the unchanged concatenation
            regex.lastIndex = 0;
            var match = regex.exec(value);

            while (match is object && match[0].Length > 0)
            {
                var start = match.index.As<int>();
                var range = CreateRangeOverNodes(nodes, start, start + match[0].Length);

                AddRegistryRange(highlight, ctx, range);
                eachCb?.Invoke(new MarkedMatch { Range = range, Text = match[0] });

                match = regex.exec(value);
            }
        }

        private static Range CreateRangeOverNodes(List<MappedTextNode> nodes, int start, int end)
        {
            MappedTextNode startNode = null;
            MappedTextNode endNode   = null;

            foreach (var node in nodes)
            {
                if (startNode is null && node.End > start) startNode = node;

                if (node.End >= end)
                {
                    endNode = node;
                    break;
                }
            }

            var range = document.createRange();
            range.setStart(startNode.Node, (start - startNode.Start).As<uint>());
            range.setEnd(endNode.Node, (end - endNode.Start).As<uint>());
            return range;
        }

        private static void WrapMatchesAcrossElements(List<Text> textNodes, es5.RegExp regex, MarkOptions options, Action<MarkedMatch> eachCb)
        {
            var nodes = MapTextNodes(textNodes, out var value);

            regex.lastIndex = 0;
            var match = regex.exec(value);

            while (match is object && match[0].Length > 0)
            {
                var matchText = match[0];
                var start     = match.index.As<int>();
                var wrappers  = new List<HTMLElement>();

                // Wrapping consumes the matched text from value and shifts the mapped offsets, so
                // the next exec continues on the shrunken string from where the last wrap ended
                regex.lastIndex = WrapRangeAcrossNodes(nodes, ref value, start, start + matchText.Length, options, wrappers);
                eachCb?.Invoke(new MarkedMatch { Elements = wrappers.ToArray(), Text = matchText });

                match = regex.exec(value);
            }
        }

        private static int WrapRangeAcrossNodes(List<MappedTextNode> nodes, ref string value, int start, int end, MarkOptions options, List<HTMLElement> wrappers)
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
                wrappers.Add(current.Node.previousSibling.As<HTMLElement>());

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

        // ---- unmarking ---------------------------------------------------------------------------

        private static async Task UnmarkCoreAsync(HTMLElement ctx, MarkOptions options, CancellationToken cancellationToken)
        {
            ClearFocus(ctx);
            UnmarkRegistry(ctx);

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

        // ---- shared helpers ----------------------------------------------------------------------

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
