using System;
using System.Collections.Generic;
using System.Text;
using Transpose;
using static Transpose.Core.dom;
using Range = Transpose.Core.dom.Range;

namespace Tesserae
{
    /// <summary>
    /// The Markdown side of a <see cref="SquireEditor"/>: renders Markdown into the editor block by block, and reads
    /// it back so that a block nobody edited comes back exactly as it was written.
    /// </summary>
    /// <remarks>
    /// <para>Markdown has many spellings for one document (<c>__b__</c> and <c>**b**</c>, one blank line or three), so
    /// HTML cannot be turned back into the text it was rendered from. Every top-level block rendered from Markdown
    /// therefore carries a <c>data-tss-md</c> id that names a record of the source it came from and of what
    /// <see cref="SquireMarkdown"/> writes for it right after rendering. Reading the document back, a block whose
    /// serialization still equals that snapshot has not been edited, and its source is written instead, together
    /// with the original text between it and the next block. So an edit in one paragraph changes only that
    /// paragraph's Markdown, which is what keeps a collaborative session (<see cref="SquireYjsBinding"/>) from
    /// rewriting text another peer wrote somewhere else.</para>
    /// <para>The id is an attribute rather than a property so it survives Squire's undo (which restores HTML) and a
    /// block split (which clones it); both halves of a split carry it, and the snapshot check settles which one is
    /// still the original. <see cref="SquireEditor.HTML"/> strips it.</para>
    /// </remarks>
    internal sealed class SquireMarkdownDocument
    {
        internal const string IdAttribute = "data-tss-md";

        private static object _marked;

        private readonly Dictionary<int, BlockRecord> _records = new Dictionary<int, BlockRecord>();

        private int    _nextId = 1;
        private int    _firstId = -1;
        private string _leading = "";

        internal sealed class BlockRecord
        {
            public int    Id;
            public string Source;
            public string Rendered;
            public string GapAfter;
            public int    NextId = -1;
        }

        /// <summary>A top-level block of Markdown, as marked's lexer splits it.</summary>
        internal sealed class ParsedBlock
        {
            public string Source;
            public string Gap;
            public int    Start;
            public string Html;
        }

        internal sealed class Parsed
        {
            public string            Leading = "";
            public List<ParsedBlock> Blocks  = new List<ParsedBlock>();
        }

        /// <summary>A top-level element of the editor and what it contributes to the Markdown.</summary>
        internal sealed class Entry
        {
            public Element     Element;
            public string      Text;
            public string      Serialized;
            public bool        Preserved;
            public BlockRecord Record;
            public int         Start;
        }

        internal sealed class Snapshot
        {
            public string      Markdown;
            public List<Entry> Entries;
        }

        #region Parsing

        private static object Marked => _marked ?? (_marked = Script.Write<object>("new globalThis.marked.Marked({async:false, gfm:true, breaks:false, silent:true})"));

        internal static Parsed Parse(string markdown)
        {
            var result = new Parsed();
            markdown   = (markdown ?? "").Replace("\r\n", "\n").Replace('\r', '\n');

            var marked = Marked;
            var tokens = Script.Write<object[]>("{0}.lexer({1})", marked, markdown);
            var links  = Script.Write<object>("{0}.links", tokens);
            var offset = 0;

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                var type  = Script.Write<string>("{0}.type", token);
                var raw   = Script.Write<string>("{0}.raw", token) ?? "";
                var start = offset;
                offset   += raw.Length;

                string html = null;

                if (type == "hr")
                {
                    html = "<div class=\"" + SquireMarkdown.HrClass + "\"><br></div>";
                }
                else if (type != "space" && type != "def")
                {
                    html = Script.Write<string>("{0}.parser(Object.assign([{1}], { links: {2} }))", marked, token, links);
                }

                if (string.IsNullOrWhiteSpace(html))
                {
                    // Nothing to show (blank lines, link reference definitions): it stays text between blocks.
                    if (result.Blocks.Count == 0) result.Leading += raw;
                    else result.Blocks[result.Blocks.Count - 1].Gap += raw;
                    continue;
                }

                var source = raw.TrimEnd('\n');

                result.Blocks.Add(new ParsedBlock
                {
                    Source = source,
                    Gap    = raw.Substring(source.Length),
                    Start  = start,
                    Html   = html
                });
            }

            return result;
        }

        /// <summary>The HTML of <paramref name="markdown"/>, block by block as the editor renders it (a rule as <c>tss-squire-hr</c>).</summary>
        internal static string RenderHtml(string markdown)
        {
            var html = new StringBuilder();
            foreach (var block in Parse(markdown).Blocks) html.Append(block.Html);
            return html.ToString();
        }

        /// <summary>One block's HTML as exactly one sanitized element, wrapped in a <c>DIV</c> when it is not.</summary>
        private static HTMLElement CreateElement(ParsedBlock block)
        {
            var fragment = Script.Write<DocumentFragment>("DOMPurify.sanitize({0}, { RETURN_DOM_FRAGMENT: true, FORBID_TAGS: ['style', 'script'] })", block.Html);
            Tidy(fragment);

            var elements = new List<Node>();
            var children = fragment.childNodes;
            for (var i = 0; i < children.length; i++) elements.Add((Node)children[(uint)i]);

            if (elements.Count == 1 && SquireMarkdown.IsBlock(elements[0])) return (HTMLElement)elements[0];

            var div = (HTMLElement)document.createElement("div");
            foreach (var e in elements) div.appendChild(e);
            if (div.childNodes.length == 0) div.appendChild(document.createElement("br"));
            return div;
        }

        /// <summary>What Squire's own clean-up does to inserted HTML that matters here: no whitespace text between blocks, and a <c>&lt;br&gt;</c> in an empty block for the cursor.</summary>
        private static void Tidy(Node node)
        {
            var children = node.childNodes;

            for (var i = (int)children.length - 1; i >= 0; i--)
            {
                var child = (Node)children[(uint)i];

                if (child.nodeType == 3)
                {
                    if (node.nodeName != "PRE" && node.nodeName != "CODE" && string.IsNullOrWhiteSpace(child.textContent) && (node.nodeType == 11 || HasBlockChildren(node)))
                    {
                        node.removeChild(child);
                    }
                }
                else if (child.nodeType == 1 && child.nodeName != "PRE")
                {
                    Tidy(child);
                }
            }

            // A trailing newline inside a block's text shows as nothing; marked leaves one after a code block.
            var last = node.nodeName == "PRE" ? (Node)node.lastChild : null;

            if (last != null && last.nodeName == "CODE" && last.lastChild != null && ((Node)last.lastChild).nodeType == 3)
            {
                var text = (Text)last.lastChild;
                if (text.data.EndsWith("\n")) text.data = text.data.Substring(0, text.data.Length - 1);
            }

            if (node.nodeType == 1 && SquireMarkdown.IsBlock(node) && node.childNodes.length == 0 && node.nodeName != "HR" && node.nodeName != "UL" && node.nodeName != "OL" && node.nodeName != "TABLE")
            {
                node.appendChild(document.createElement("br"));
            }
        }

        private static bool HasBlockChildren(Node node)
        {
            var children = node.childNodes;
            for (var i = 0; i < children.length; i++) if (SquireMarkdown.IsBlock((Node)children[(uint)i])) return true;
            return false;
        }

        #endregion

        #region Rendering

        /// <summary>Replaces the whole content of <paramref name="squire"/> with <paramref name="markdown"/> (through <c>setHTML</c>, so undo starts over).</summary>
        public void Render(Squire squire, string markdown)
        {
            var parsed = Parse(markdown);
            var ids    = new List<int>();
            var html   = new StringBuilder();

            _records.Clear();

            foreach (var block in parsed.Blocks)
            {
                var el = CreateElement(block);
                var id = _nextId++;
                el.setAttribute(IdAttribute, id.ToString());
                ids.Add(id);
                html.Append(el.outerHTML);
            }

            squire.setHTML(html.ToString());

            var root = squire.getRoot();
            var byId = new Dictionary<int, Element>();
            ForEachTopLevel(root, el =>
            {
                var id = IdOf(el);
                if (id > 0 && !byId.ContainsKey(id)) byId[id] = el;
            });

            for (var i = 0; i < parsed.Blocks.Count; i++)
            {
                var block = parsed.Blocks[i];
                byId.TryGetValue(ids[i], out var el);

                _records[ids[i]] = new BlockRecord
                {
                    Id       = ids[i],
                    Source   = block.Source,
                    Rendered = el != null ? SquireMarkdown.SerializeBlock(el, false, out _) : null,
                    GapAfter = block.Gap,
                    NextId   = i + 1 < ids.Count ? ids[i + 1] : -1
                };
            }

            _leading = parsed.Leading;
            _firstId = ids.Count > 0 ? ids[0] : -1;
        }

        /// <summary>Forgets every record, so the editor is read back as it is (after the content was set as HTML).</summary>
        public void Clear()
        {
            _records.Clear();
            _leading = "";
            _firstId = -1;
        }

        /// <summary>
        /// Makes the editor hold <paramref name="markdown"/> by replacing only the blocks that differ from what it holds
        /// now (<paramref name="current"/>). Blocks before and after the change keep their elements, so a caret in them
        /// stays where it is; a caret in a replaced block is carried through <paramref name="transformOffset"/>, which
        /// moves a Markdown offset in the old text to the new one.
        /// </summary>
        public void Update(Squire squire, string markdown, Snapshot current, Func<int, int> transformOffset)
        {
            var root   = squire.getRoot();
            var parsed = Parse(markdown);
            var old    = current.Entries;
            var news   = parsed.Blocks;

            var prefix = 0;
            while (prefix < old.Count && prefix < news.Count && old[prefix].Text == news[prefix].Source) prefix++;

            var suffix = 0;
            while (suffix < old.Count - prefix && suffix < news.Count - prefix && old[old.Count - 1 - suffix].Text == news[news.Count - 1 - suffix].Source) suffix++;

            // The selection, as a Markdown offset in the new text, if it is in a block about to be replaced.
            var range        = squire.getSelection();
            var hadSelection = range != null && root.contains(range.startContainer);
            int? newStart    = null;
            int? newEnd      = null;

            var allTop    = TopLevelElements(root);
            var firstGone = prefix > 0 ? allTop.IndexOf(old[prefix - 1].Element) + 1 : 0;
            var lastGone  = suffix > 0 ? allTop.IndexOf(old[old.Count - suffix].Element) - 1 : allTop.Count - 1;

            // Nothing of the old content changed: insert without taking out the empty blocks someone may be typing in.
            if (prefix + suffix >= old.Count) lastGone = firstGone - 1;

            if (hadSelection)
            {
                var startBlock = TopLevelOf(root, range.startContainer);
                var endBlock   = TopLevelOf(root, range.endContainer);
                var startIndex = allTop.IndexOf(startBlock);
                var endIndex   = allTop.IndexOf(endBlock);

                if ((startIndex >= firstGone && startIndex <= lastGone) || (endIndex >= firstGone && endIndex <= lastGone))
                {
                    var s = OffsetOf(current, range.startContainer, (int)range.startOffset);
                    var e = OffsetOf(current, range.endContainer,   (int)range.endOffset);
                    newStart = transformOffset(s);
                    newEnd   = transformOffset(e);
                }
            }

            var inserted = new List<HTMLElement>();
            var ids      = new int[news.Count];

            squire.modifyDocument(() =>
            {
                Node anchor = lastGone + 1 < allTop.Count ? allTop[lastGone + 1] : null;
                if (lastGone < firstGone) anchor = firstGone < allTop.Count ? allTop[firstGone] : null;

                for (var i = firstGone; i <= lastGone && i < allTop.Count; i++)
                {
                    if (allTop[i].parentNode == root) root.removeChild(allTop[i]);
                }

                for (var k = prefix; k < news.Count - suffix; k++)
                {
                    var el = CreateElement(news[k]);
                    var id = _nextId++;
                    el.setAttribute(IdAttribute, id.ToString());
                    ids[k] = id;
                    root.insertBefore(el, anchor);
                    inserted.Add(el);
                }

                if (root.childNodes.length == 0) root.appendChild(squire.createDefaultBlock());

                // Every block that stays is re-labelled with the block it now stands for, so its gaps are the new ones.
                for (var k = 0; k < prefix; k++)                       ids[k] = Relabel(old[k].Element);
                for (var k = news.Count - suffix; k < news.Count; k++) ids[k] = Relabel(old[old.Count - (news.Count - k)].Element);
            });

            _records.Clear();

            var elementsById = new Dictionary<int, Element>();
            ForEachTopLevel(root, el =>
            {
                var id = IdOf(el);
                if (id > 0 && !elementsById.ContainsKey(id)) elementsById[id] = el;
            });

            for (var k = 0; k < news.Count; k++)
            {
                elementsById.TryGetValue(ids[k], out var el);
                _records[ids[k]] = new BlockRecord
                {
                    Id       = ids[k],
                    Source   = news[k].Source,
                    Rendered = el != null ? SquireMarkdown.SerializeBlock(el, false, out _) : null,
                    GapAfter = news[k].Gap,
                    NextId   = k + 1 < news.Count ? ids[k + 1] : -1
                };
            }

            _leading = parsed.Leading;
            _firstId = news.Count > 0 ? ids[0] : -1;

            if (newStart.HasValue)
            {
                var after = Read(root, false);
                var start = PositionAt(after, newStart.Value);
                var end   = PositionAt(after, newEnd.Value);

                if (start != null && end != null)
                {
                    var r = document.createRange();
                    r.setStart(start.Item1, (uint)start.Item2);
                    if (newEnd.Value >= newStart.Value) r.setEnd(end.Item1, (uint)end.Item2);
                    else r.collapse(true);
                    squire.setSelection(r);
                }
            }

            int Relabel(Element el)
            {
                var id = _nextId++;
                el.setAttribute(IdAttribute, id.ToString());
                return id;
            }
        }

        #endregion

        #region Reading back

        /// <summary>The Markdown of the editor, with the top-level blocks and where each one starts.</summary>
        public Snapshot Read(HTMLElement root, bool preserve = true)
        {
            var entries = new List<Entry>();

            ForEachTopLevel(root, el =>
            {
                var serialized = SquireMarkdown.SerializeBlock(el, false, out _);
                var id         = IdOf(el);
                BlockRecord record = null;

                if (preserve && id > 0) _records.TryGetValue(id, out record);

                var preserved = record != null && record.Rendered != null && record.Rendered == serialized;
                var text      = preserved ? record.Source : serialized;

                if (text.Length == 0) return;

                entries.Add(new Entry { Element = el, Text = text, Serialized = serialized, Preserved = preserved, Record = record });
            });

            var sb = new StringBuilder();

            if (entries.Count > 0 && entries[0].Preserved && entries[0].Record.Id == _firstId) sb.Append(_leading);

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                entry.Start = sb.Length;
                sb.Append(entry.Text);

                if (i + 1 < entries.Count)
                {
                    var next = entries[i + 1];
                    sb.Append(entry.Preserved && next.Preserved && entry.Record.NextId == next.Record.Id ? entry.Record.GapAfter : "\n\n");
                }
                else
                {
                    sb.Append(entry.Preserved && entry.Record.NextId == -1 ? entry.Record.GapAfter : "\n");
                }
            }

            return new Snapshot { Markdown = sb.ToString(), Entries = entries };
        }

        /// <summary>The Markdown offset a DOM position in the editor stands at.</summary>
        public static int OffsetOf(Snapshot snapshot, Node container, int offset)
        {
            var entries = snapshot.Entries;
            if (entries.Count == 0) return 0;

            Entry entry = null;
            foreach (var e in entries)
            {
                if (e.Element == container || e.Element.contains(container))
                {
                    entry = e;
                    break;
                }
            }

            if (entry == null)
            {
                // An empty block: it stands where the next block with content starts.
                foreach (var e in entries)
                {
                    if ((e.Element.compareDocumentPosition(container) & 2) != 0) return e.Start; // container precedes e
                }
                var last = entries[entries.Count - 1];
                return last.Start + last.Text.Length;
            }

            var serialized = SquireMarkdown.SerializeBlock(entry.Element, true, out var points);
            var inBlock    = FindPoint(points, container, offset);
            if (entry.Preserved) inBlock = SquireMarkdown.AlignOffset(serialized, inBlock, entry.Text);

            return entry.Start + Math.Min(inBlock, entry.Text.Length);
        }

        /// <summary>The DOM position that stands at a Markdown offset, or null when the editor is empty.</summary>
        public static Tuple<Node, int> PositionAt(Snapshot snapshot, int markdownOffset)
        {
            var entries = snapshot.Entries;
            if (entries.Count == 0) return null;

            var entry = entries[entries.Count - 1];
            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (markdownOffset <= e.Start + e.Text.Length)
                {
                    entry = markdownOffset < e.Start && i > 0 ? entries[i - 1] : e;
                    break;
                }
            }

            var inBlock = Math.Max(0, Math.Min(markdownOffset - entry.Start, entry.Text.Length));

            var serialized = SquireMarkdown.SerializeBlock(entry.Element, true, out var points);
            if (entry.Preserved) inBlock = SquireMarkdown.AlignOffset(entry.Text, inBlock, serialized);

            MarkdownPoint best = null;
            foreach (var p in points)
            {
                if (p.MarkdownOffset > inBlock) break;
                if (best == null || p.MarkdownOffset > best.MarkdownOffset || p.Node.nodeType == 3) best = p;
            }

            if (best == null) return Tuple.Create((Node)entry.Element, 0);

            return ToRangePosition(best.Node, best.NodeOffset);
        }

        private static Tuple<Node, int> ToRangePosition(Node node, int offset)
        {
            if (node.nodeType == 3) return Tuple.Create(node, Math.Min(offset, node.textContent.Length));

            if (node.nodeName == "BR" || node.nodeName == "IMG")
            {
                var parent = node.parentNode;
                var index  = IndexOf(parent, node);
                return Tuple.Create((Node)parent, index);
            }

            return Tuple.Create(node, Math.Min(offset, (int)node.childNodes.length));
        }

        private static int IndexOf(Node parent, Node child)
        {
            var children = parent.childNodes;
            for (var i = 0; i < children.length; i++) if ((Node)children[(uint)i] == child) return i;
            return 0;
        }

        private static int FindPoint(List<MarkdownPoint> points, Node container, int offset)
        {
            if (container.nodeType != 3)
            {
                // An element position: find the first or last text (or leaf) beneath the child it points at.
                var children = container.childNodes;

                if (offset < children.length)
                {
                    var target = FirstLeaf((Node)children[(uint)offset]);
                    var found  = FirstPointIn(points, target);
                    if (found >= 0) return found;
                }

                if (offset > 0 && offset - 1 < children.length)
                {
                    var target = LastLeaf((Node)children[(uint)(offset - 1)]);
                    var found  = LastPointIn(points, target);
                    if (found >= 0) return found;
                }

                foreach (var p in points) if (p.Node == container) return p.MarkdownOffset;
                return 0;
            }

            var best = -1;
            var bestNodeOffset = -1;

            foreach (var p in points)
            {
                if (p.Node != container || p.NodeOffset > offset) continue;
                if (p.NodeOffset > bestNodeOffset || (p.NodeOffset == bestNodeOffset && p.MarkdownOffset > best))
                {
                    best           = p.MarkdownOffset;
                    bestNodeOffset = p.NodeOffset;
                }
            }

            if (best >= 0) return best;

            // A text node whose characters were all dropped (whitespace): use the nearest point before it.
            var last = 0;
            foreach (var p in points)
            {
                if ((p.Node.compareDocumentPosition(container) & 4) != 0) last = p.MarkdownOffset; // container follows p.Node
            }
            return last;
        }

        private static Node FirstLeaf(Node node)
        {
            while (node.firstChild != null) node = (Node)node.firstChild;
            return node;
        }

        private static Node LastLeaf(Node node)
        {
            while (node.lastChild != null) node = (Node)node.lastChild;
            return node;
        }

        private static int FirstPointIn(List<MarkdownPoint> points, Node node)
        {
            foreach (var p in points) if (p.Node == node || node.contains(p.Node)) return p.MarkdownOffset;
            return -1;
        }

        private static int LastPointIn(List<MarkdownPoint> points, Node node)
        {
            var found = -1;
            foreach (var p in points) if (p.Node == node || node.contains(p.Node)) found = Math.Max(found, p.MarkdownOffset);
            return found;
        }

        #endregion

        #region DOM helpers

        private static int IdOf(Element el) => int.TryParse(el.getAttribute(IdAttribute) ?? "", out var id) ? id : 0;

        private static void ForEachTopLevel(HTMLElement root, Action<Element> action)
        {
            foreach (var el in TopLevelElements(root)) action(el);
        }

        private static List<Element> TopLevelElements(HTMLElement root)
        {
            var list     = new List<Element>();
            var children = root.childNodes;

            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];
                if (child.nodeType != 1) continue;
                if (((Element)child).classList.contains("squire-image-resize-container")) continue;
                list.Add((Element)child);
            }

            return list;
        }

        private static Element TopLevelOf(HTMLElement root, Node node)
        {
            while (node != null && node.parentNode != root) node = node.parentNode;
            return (Element)node;
        }

        /// <summary>Removes the block ids from HTML read out of the editor.</summary>
        internal static string StripIds(string html) => string.IsNullOrEmpty(html) ? html : Script.Write<string>("{0}.replace(/ data-tss-md=\"\\d+\"/g, '')", html);

        #endregion
    }
}
