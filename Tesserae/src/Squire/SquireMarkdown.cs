using System;
using System.Collections.Generic;
using System.Text;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Turns the HTML a <see cref="SquireEditor"/> holds into Markdown (CommonMark plus GitHub's strikethrough and
    /// tables). Headings, paragraphs, hard line breaks, bold, italic, strikethrough, inline code, code blocks, links,
    /// images, block quotes, nested lists, tables and horizontal rules have a Markdown spelling and get it; underline,
    /// subscript and superscript are written as their HTML tags, which Markdown passes through. Fonts, sizes, colours
    /// and alignment have no Markdown form and are dropped.
    /// </summary>
    public static class SquireMarkdown
    {
        // Set for the length of a strict conversion: underline, subscript and superscript are dropped instead of written as HTML.
        private static bool _strict;

        /// <summary>
        /// The Markdown of the blocks inside <paramref name="root"/> (an editor's root, or any element). With
        /// <paramref name="strict"/>, formatting Markdown has no syntax for (underline, subscript, superscript) is dropped
        /// rather than written as inline HTML.
        /// </summary>
        public static string ToMarkdown(Element root, bool strict = false)
        {
            if (root == null) return "";

            var wasStrict = _strict;
            _strict = strict;

            try
            {
                var writer = new MarkdownWriter(false);
                SerializeChildrenAsBlocks(writer, root, "\n\n");
                return writer.ToString().TrimEnd('\n') + "\n";
            }
            finally
            {
                _strict = wasStrict;
            }
        }

        /// <summary>The Markdown of an HTML string, as <see cref="ToMarkdown(Element, bool)"/> writes it for an editor holding it.</summary>
        public static string HtmlToMarkdown(string html, bool strict = false)
        {
            var div = (HTMLElement)document.createElement("div");
            div.innerHTML = SanitizeHTML.Sanitize(html ?? "");
            return ToMarkdown(div, strict);
        }

        /// <summary>The Markdown of one top-level block, and where each of its characters came from when <paramref name="track"/> is set.</summary>
        internal static string SerializeBlock(Element block, bool track, out List<MarkdownPoint> points)
        {
            var writer = new MarkdownWriter(track);
            writer.MarkStart(block);
            SerializeBlockOrGroup(writer, new List<Node> { block });
            points = writer.Points;
            return writer.ToString();
        }

        #region Classification

        private const int ELEMENT_NODE = 1;
        private const int TEXT_NODE    = 3;

        internal const string HrClass = "tss-squire-hr";

        private static readonly HashSet<string> BlockTags = new HashSet<string>(new[]
        {
            "DIV", "P", "H1", "H2", "H3", "H4", "H5", "H6", "BLOCKQUOTE", "UL", "OL", "LI", "PRE", "HR", "TABLE", "THEAD", "TBODY", "TFOOT", "TR",
            "TD", "TH", "SECTION", "ARTICLE", "HEADER", "FOOTER", "FIGURE", "FIGCAPTION", "ADDRESS", "ASIDE", "DL", "DT", "DD", "DETAILS", "SUMMARY", "MAIN", "NAV"
        });

        private static readonly HashSet<string> SkippedTags = new HashSet<string>(new[] { "STYLE", "SCRIPT", "TEMPLATE", "HEAD", "META", "TITLE", "LINK" });

        internal static bool IsBlock(Node node) => node.nodeType == ELEMENT_NODE && BlockTags.Contains(node.nodeName);

        private static bool IsSkipped(Node node) => node.nodeType == ELEMENT_NODE && (SkippedTags.Contains(node.nodeName) || ((Element)node).classList.contains("squire-image-resize-container"));

        #endregion

        #region Blocks

        /// <summary>Serializes the children of <paramref name="parent"/> as a sequence of blocks: runs of inline nodes become paragraphs.</summary>
        private static void SerializeChildrenAsBlocks(MarkdownWriter w, Node parent, string separator)
        {
            var any   = false;
            var group = new List<Node>();

            void Flush()
            {
                if (group.Count == 0) return;
                var before = w.Length;
                if (any) w.Pending = separator;
                SerializeBlockOrGroup(w, group);
                if (w.Length > before) any = true;
                else w.Pending = null;
                group.Clear();
            }

            var children = parent.childNodes;

            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];
                if (IsSkipped(child)) continue;

                if (IsBlock(child))
                {
                    Flush();
                    group.Add(child);
                    Flush();
                }
                else
                {
                    group.Add(child);
                }
            }

            Flush();
        }

        /// <summary>One block element (a group of one), or a run of inline nodes written as a paragraph.</summary>
        private static void SerializeBlockOrGroup(MarkdownWriter w, List<Node> group)
        {
            if (group.Count == 1 && IsBlock(group[0]))
            {
                SerializeBlockElement(w, (Element)group[0]);
            }
            else
            {
                WriteInline(w, group, InlineContext.Paragraph);
            }
        }

        private static void SerializeBlockElement(MarkdownWriter w, Element el)
        {
            switch (el.nodeName)
            {
                case "H1": case "H2": case "H3": case "H4": case "H5": case "H6":
                {
                    var level = el.nodeName[1] - '0';
                    var mark  = w.Length;
                    w.Write(new string('#', level) + " ");
                    var start = w.Length;
                    WriteInline(w, ChildrenOf(el), InlineContext.Heading);
                    if (w.Length == start) w.Truncate(mark);
                    return;
                }
                case "BLOCKQUOTE":
                {
                    var mark = w.Length;
                    w.Write("> ");
                    w.PushPrefix("> ");
                    var start = w.Length;
                    SerializeChildrenAsBlocks(w, el, "\n\n");
                    w.PopPrefix();
                    if (w.Length == start) w.Truncate(mark);
                    return;
                }
                case "UL": case "OL":
                    SerializeList(w, el);
                    return;
                case "LI":
                    // A list item outside a list: write it as a bullet.
                    SerializeListItem(w, el, "- ");
                    return;
                case "PRE":
                    SerializeCodeBlock(w, el);
                    return;
                case "HR":
                    w.Write("---");
                    return;
                case "TABLE":
                    SerializeTable(w, el);
                    return;
                case "DIV":
                    if (el.classList.contains(HrClass) && string.IsNullOrWhiteSpace(el.textContent))
                    {
                        w.Write("---");
                        return;
                    }
                    break;
            }

            if (HasBlockChild(el))
            {
                SerializeChildrenAsBlocks(w, el, "\n\n");
            }
            else
            {
                WriteInline(w, ChildrenOf(el), InlineContext.Paragraph);
            }
        }

        private static bool HasBlockChild(Node el)
        {
            var children = el.childNodes;
            for (var i = 0; i < children.length; i++)
            {
                if (IsBlock((Node)children[(uint)i])) return true;
            }
            return false;
        }

        private static List<Node> ChildrenOf(Node el)
        {
            var list     = new List<Node>();
            var children = el.childNodes;
            for (var i = 0; i < children.length; i++) list.Add((Node)children[(uint)i]);
            return list;
        }

        private static void SerializeList(MarkdownWriter w, Element list)
        {
            var ordered    = list.nodeName == "OL";
            var startAttr  = list.getAttribute("start");
            var number     = ordered && int.TryParse(startAttr ?? "", out var parsed) ? parsed : 1;
            var lastIndent = "  ";
            var first      = true;
            var children   = list.childNodes;

            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];
                if (child.nodeType == TEXT_NODE && string.IsNullOrWhiteSpace(child.textContent)) continue;
                if (IsSkipped(child)) continue;

                if (child.nodeName == "UL" || child.nodeName == "OL")
                {
                    // Squire nests a list directly in its parent list, after the item it belongs to.
                    var before = w.Length;
                    if (!first) w.Pending = "\n";
                    w.Write(lastIndent);
                    w.PushPrefix(lastIndent);
                    var start = w.Length;
                    SerializeList(w, (Element)child);
                    w.PopPrefix();
                    if (w.Length == start) { w.Truncate(before); w.Pending = null; }
                    else first = false;
                    continue;
                }

                var marker = ordered ? number + ". " : "- ";
                number++;

                if (!first) w.Pending = "\n";
                first = false;

                if (child.nodeName == "LI")
                {
                    SerializeListItem(w, (Element)child, marker);
                }
                else
                {
                    w.Write(marker);
                    w.PushPrefix(new string(' ', marker.Length));
                    SerializeBlockOrGroup(w, new List<Node> { child });
                    w.PopPrefix();
                }

                lastIndent = new string(' ', marker.Length);
            }
        }

        private static void SerializeListItem(MarkdownWriter w, Element li, string marker)
        {
            w.Write(marker);
            w.PushPrefix(new string(' ', marker.Length));

            var group    = new List<Node>();
            var any      = false;
            var children = li.childNodes;

            void Flush(Node next)
            {
                if (group.Count == 0) return;
                var before = w.Length;
                if (any) w.Pending = next != null && (next.nodeName == "UL" || next.nodeName == "OL") || group[0].nodeName == "UL" || group[0].nodeName == "OL" ? "\n" : "\n\n";
                SerializeBlockOrGroup(w, group);
                if (w.Length > before) any = true;
                else w.Pending = null;
                group.Clear();
            }

            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];
                if (IsSkipped(child)) continue;

                if (IsBlock(child))
                {
                    Flush(child);
                    group.Add(child);
                    Flush(child);
                }
                else
                {
                    group.Add(child);
                }
            }

            Flush(null);
            w.PopPrefix();
        }

        private static void SerializeCodeBlock(MarkdownWriter w, Element pre)
        {
            var language = "";
            var code     = (Element)pre.querySelector("code");

            if (code != null)
            {
                foreach (var cls in (code.getAttribute("class") ?? "").Split(' '))
                {
                    if (cls.StartsWith("language-")) language = cls.Substring(9);
                }
            }

            // Collect the characters with where they came from; a <br> is a line break.
            var chars = new List<MarkdownChar>();
            CollectPreText(pre, chars);

            while (chars.Count > 0 && chars[chars.Count - 1].Char == '\n') chars.RemoveAt(chars.Count - 1);

            var longest = 0;
            var run     = 0;
            foreach (var c in chars)
            {
                run     = c.Char == '`' ? run + 1 : 0;
                longest = Math.Max(longest, run);
            }

            var fence = new string('`', Math.Max(3, longest + 1));

            w.Write(fence + language + "\n");
            foreach (var c in chars)
            {
                if (c.Char == '\n') w.Write("\n", c.Node, c.Offset);
                else w.WriteChar(c.Char, c.Node, c.Offset);
            }
            w.Write("\n" + fence);
        }

        private static void CollectPreText(Node node, List<MarkdownChar> chars)
        {
            var children = node.childNodes;

            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];

                if (child.nodeType == TEXT_NODE)
                {
                    var data = child.textContent;
                    for (var k = 0; k < data.Length; k++)
                    {
                        var ch = data[k];
                        if (ch == '​' || ch == '\r') continue;
                        chars.Add(new MarkdownChar(ch == ' ' ? ' ' : ch, child, k));
                    }
                }
                else if (child.nodeName == "BR")
                {
                    chars.Add(new MarkdownChar('\n', node, i));
                }
                else if (child.nodeType == ELEMENT_NODE && !IsSkipped(child))
                {
                    if (IsBlock(child) && chars.Count > 0 && chars[chars.Count - 1].Char != '\n') chars.Add(new MarkdownChar('\n', node, i));
                    CollectPreText(child, chars);
                }
            }
        }

        private static void SerializeTable(MarkdownWriter w, Element table)
        {
            var rows = table.querySelectorAll("tr");
            if (rows.length == 0) return;

            var width = 0;
            for (var r = 0; r < rows.length; r++) width = Math.Max(width, CellsOf((Element)rows[(uint)r]).Count);
            if (width == 0) return;

            for (var r = 0; r < rows.length; r++)
            {
                if (r > 0) w.Write("\n");

                var cells = CellsOf((Element)rows[(uint)r]);
                w.Write("|");

                for (var c = 0; c < width; c++)
                {
                    w.Write(" ");
                    if (c < cells.Count) WriteInline(w, ChildrenOf(cells[c]), InlineContext.TableCell);
                    w.Write(" |");
                }

                if (r == 0)
                {
                    w.Write("\n|");
                    for (var c = 0; c < width; c++) w.Write(" --- |");
                }
            }
        }

        private static List<Element> CellsOf(Element row)
        {
            var cells    = new List<Element>();
            var children = row.childNodes;
            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];
                if (child.nodeName == "TD" || child.nodeName == "TH") cells.Add((Element)child);
            }
            return cells;
        }

        #endregion

        #region Inline

        private enum InlineContext
        {
            Paragraph,
            Heading,
            TableCell
        }

        // Outermost first: the order marks are opened in, and the reverse of the order they are closed in.
        private enum MarkKind
        {
            Link,
            Bold,
            Italic,
            Strike,
            Underline,
            Sub,
            Sup,
            Code
        }

        private sealed class Mark
        {
            public MarkKind Kind;
            public string   Href;
            public string   Title;

            public bool SameAs(Mark other) => other != null && other.Kind == Kind && other.Href == Href && other.Title == Title;
        }

        private sealed class InlineItem
        {
            public char       Char;     // '\0' for an image or a line break
            public bool       IsBreak;
            public Element    Image;
            public Node       Node;
            public int        Offset;
            public List<Mark> Marks;
        }

        private static void WriteInline(MarkdownWriter w, List<Node> nodes, InlineContext context)
        {
            var items = new List<InlineItem>();
            var marks = new List<Mark>();

            foreach (var node in nodes) CollectInline(node, marks, items);

            // Whitespace collapses as HTML collapses it, and a block starts and ends without it.
            var collapsed = new List<InlineItem>();
            foreach (var item in items)
            {
                var isSpace = item.Char == ' ';
                if (isSpace && !HasMark(item.Marks, MarkKind.Code))
                {
                    if (collapsed.Count == 0) continue;
                    var last = collapsed[collapsed.Count - 1];
                    if (last.Char == ' ' || last.IsBreak) continue;
                }
                collapsed.Add(item);
            }

            // The <br> Squire keeps at the end of a block is there for the cursor, and spaces before a break do nothing.
            while (collapsed.Count > 0 && (collapsed[collapsed.Count - 1].IsBreak || collapsed[collapsed.Count - 1].Char == ' ')) collapsed.RemoveAt(collapsed.Count - 1);

            new InlineEmitter(w, collapsed, context).Emit();
        }

        private static bool HasMark(List<Mark> marks, MarkKind kind)
        {
            foreach (var m in marks) if (m.Kind == kind) return true;
            return false;
        }

        private static void CollectInline(Node node, List<Mark> marks, List<InlineItem> items)
        {
            if (node.nodeType == TEXT_NODE)
            {
                var data = node.textContent;
                for (var k = 0; k < data.Length; k++)
                {
                    var ch = data[k];
                    if (ch == '​' || ch == '﻿') continue;
                    if (ch == '\t' || ch == '\n' || ch == '\r' || ch == ' ') ch = ' ';
                    items.Add(new InlineItem { Char = ch, Node = node, Offset = k, Marks = marks });
                }
                return;
            }

            if (node.nodeType != ELEMENT_NODE || IsSkipped(node)) return;

            var el = (Element)node;

            switch (el.nodeName)
            {
                case "BR":
                    items.Add(new InlineItem { IsBreak = true, Node = el, Marks = marks });
                    return;
                case "IMG":
                    items.Add(new InlineItem { Image = el, Node = el, Marks = marks });
                    return;
            }

            var added = MarkOf(el);
            var inner = marks;

            if (added.Count > 0)
            {
                inner = new List<Mark>(marks);
                foreach (var m in added)
                {
                    if (!HasMark(inner, m.Kind)) inner.Add(m);
                }
            }

            var children = el.childNodes;
            for (var i = 0; i < children.length; i++)
            {
                var child = (Node)children[(uint)i];

                // A block inside inline content (a paragraph inside a table cell, say) is a line break.
                if (IsBlock(child) && items.Count > 0) items.Add(new InlineItem { IsBreak = true, Node = el, Offset = i, Marks = inner });
                CollectInline(child, inner, items);
            }
        }

        private static List<Mark> MarkOf(Element el)
        {
            var list = new List<Mark>();

            switch (el.nodeName)
            {
                case "B": case "STRONG":               list.Add(new Mark { Kind = MarkKind.Bold });      break;
                case "I": case "EM": case "CITE":      list.Add(new Mark { Kind = MarkKind.Italic });    break;
                case "U": case "INS":                  if (!_strict) list.Add(new Mark { Kind = MarkKind.Underline }); break;
                case "S": case "STRIKE": case "DEL":   list.Add(new Mark { Kind = MarkKind.Strike });    break;
                case "CODE": case "KBD": case "SAMP": case "TT": list.Add(new Mark { Kind = MarkKind.Code }); break;
                case "SUB":                            if (!_strict) list.Add(new Mark { Kind = MarkKind.Sub }); break;
                case "SUP":                            if (!_strict) list.Add(new Mark { Kind = MarkKind.Sup }); break;
                case "A":
                {
                    var href = el.getAttribute("href");
                    if (!string.IsNullOrEmpty(href)) list.Add(new Mark { Kind = MarkKind.Link, Href = href, Title = el.getAttribute("title") });
                    break;
                }
            }

            var style = ((HTMLElement)el).style;
            if (style != null)
            {
                var weight = style.fontWeight;
                if (weight == "bold" || weight == "bolder" || weight == "600" || weight == "700" || weight == "800" || weight == "900") list.Add(new Mark { Kind = MarkKind.Bold });
                if (style.fontStyle == "italic") list.Add(new Mark { Kind = MarkKind.Italic });
                var decoration = style.textDecoration ?? "";
                if (decoration.Contains("line-through")) list.Add(new Mark { Kind = MarkKind.Strike });
                if (decoration.Contains("underline") && el.nodeName != "A" && !_strict) list.Add(new Mark { Kind = MarkKind.Underline });
            }

            return list;
        }

        /// <summary>Writes a run of inline items, opening and closing delimiters where the formatting changes.</summary>
        private sealed class InlineEmitter
        {
            private readonly MarkdownWriter   _w;
            private readonly List<InlineItem> _items;
            private readonly InlineContext    _context;
            private readonly List<Mark>       _open    = new List<Mark>();
            private readonly List<InlineItem> _pending = new List<InlineItem>();
            private readonly string           _codeFence;

            private bool _escapeDelimiterAfterDigits;
            private char _lastCodeChar;

            public InlineEmitter(MarkdownWriter w, List<InlineItem> items, InlineContext context)
            {
                _w       = w;
                _items   = items;
                _context = context;

                var longest = 0;
                var run     = 0;
                foreach (var item in items)
                {
                    if (item.Char == '`' && HasMark(item.Marks, MarkKind.Code)) run++;
                    else run = 0;
                    longest = Math.Max(longest, run);
                }
                _codeFence = new string('`', longest + 1);
            }

            public void Emit()
            {
                for (var i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];

                    if (item.Char == ' ' && !HasMark(item.Marks, MarkKind.Code))
                    {
                        // Deferred: a space next to a delimiter goes outside it, or the delimiter would not count.
                        _pending.Add(item);
                        continue;
                    }

                    Transition(item.Marks, i);
                    WriteItem(item, i);
                }

                CloseTo(0);

                // Trailing spaces were trimmed; a caret on them belongs at the end.
                foreach (var p in _pending) _w.RecordPoint(p.Node, p.Offset);
                _pending.Clear();
            }

            private void Transition(List<Mark> target, int index)
            {
                var ordered = new List<Mark>(target);
                ordered.Sort((a, b) => ((int)a.Kind).CompareTo((int)b.Kind));

                var keep = 0;
                while (keep < _open.Count && keep < ordered.Count && _open[keep].SameAs(ordered[keep])) keep++;

                CloseTo(keep);
                FlushPending();

                for (var k = keep; k < ordered.Count; k++)
                {
                    Open(ordered[k], index);
                    _open.Add(ordered[k]);
                }
            }

            private void FlushPending()
            {
                foreach (var p in _pending) _w.WriteChar(' ', p.Node, p.Offset);
                _pending.Clear();
            }

            private void CloseTo(int keep)
            {
                for (var k = _open.Count - 1; k >= keep; k--)
                {
                    Close(_open[k]);
                    _open.RemoveAt(k);
                }
            }

            private void Open(Mark mark, int index)
            {
                switch (mark.Kind)
                {
                    case MarkKind.Link:      _w.Write("[");       break;
                    case MarkKind.Bold:      _w.Write("**");      break;
                    case MarkKind.Italic:    _w.Write("*");       break;
                    case MarkKind.Strike:    _w.Write("~~");      break;
                    case MarkKind.Underline: _w.Write("<u>");     break;
                    case MarkKind.Sub:       _w.Write("<sub>");   break;
                    case MarkKind.Sup:       _w.Write("<sup>");   break;
                    case MarkKind.Code:
                        _w.Write(_codeFence);
                        if (index < _items.Count && _items[index].Char == '`') _w.Write(" ");
                        _lastCodeChar = '\0';
                        break;
                }
            }

            private void Close(Mark mark)
            {
                switch (mark.Kind)
                {
                    case MarkKind.Link:
                        _w.Write("](" + LinkDestination(mark.Href) + (string.IsNullOrEmpty(mark.Title) ? "" : " \"" + mark.Title.Replace("\"", "\\\"") + "\"") + ")");
                        break;
                    case MarkKind.Bold:      _w.Write("**");     break;
                    case MarkKind.Italic:    _w.Write("*");      break;
                    case MarkKind.Strike:    _w.Write("~~");     break;
                    case MarkKind.Underline: _w.Write("</u>");   break;
                    case MarkKind.Sub:       _w.Write("</sub>"); break;
                    case MarkKind.Sup:       _w.Write("</sup>"); break;
                    case MarkKind.Code:
                        if (_lastCodeChar == '`') _w.Write(" ");
                        _w.Write(_codeFence);
                        break;
                }
            }

            private void WriteItem(InlineItem item, int index)
            {
                if (item.IsBreak)
                {
                    if (_context == InlineContext.Paragraph) _w.Write("\\\n", item.Node, item.Offset);
                    else _w.Write(_context == InlineContext.TableCell ? "<br>" : " ", item.Node, item.Offset);
                    return;
                }

                if (item.Image != null)
                {
                    var alt   = item.Image.getAttribute("alt") ?? "";
                    var src   = item.Image.getAttribute("src") ?? "";
                    var title = item.Image.getAttribute("title");
                    _w.RecordPoint(item.Node, 0);
                    _w.Write("![" + EscapeText(alt) + "](" + LinkDestination(src) + (string.IsNullOrEmpty(title) ? "" : " \"" + title.Replace("\"", "\\\"") + "\"") + ")");
                    return;
                }

                var c = item.Char;

                if (HasMark(item.Marks, MarkKind.Code))
                {
                    _w.WriteChar(c, item.Node, item.Offset);
                    _lastCodeChar = c;
                    return;
                }

                var prev = index > 0 ? _items[index - 1].Char : '\0';
                var next = index + 1 < _items.Count ? _items[index + 1].Char : '\0';

                if (_w.AtLineStart)
                {
                    _escapeDelimiterAfterDigits = false;

                    if (c == '#' || c == '>' || ((c == '-' || c == '+' || c == '=') && (next == ' ' || next == '\0' || next == c)))
                    {
                        _w.Write("\\");
                    }
                    else if (char.IsDigit(c))
                    {
                        var k = index;
                        while (k < _items.Count && char.IsDigit(_items[k].Char)) k++;
                        _escapeDelimiterAfterDigits = k < _items.Count && (_items[k].Char == '.' || _items[k].Char == ')') && (k + 1 >= _items.Count || _items[k + 1].Char == ' ');
                    }
                }
                else if (_escapeDelimiterAfterDigits && (c == '.' || c == ')'))
                {
                    _escapeDelimiterAfterDigits = false;
                    _w.Write("\\");
                }
                else if (!char.IsDigit(c))
                {
                    _escapeDelimiterAfterDigits = false;
                }

                if (NeedsEscape(c, prev, next, _context)) _w.Write("\\");

                _w.WriteChar(c, item.Node, item.Offset);
            }
        }

        private static bool NeedsEscape(char c, char prev, char next, InlineContext context)
        {
            switch (c)
            {
                case '\\': case '*': case '`': case '[': case ']': case '~':
                    return true;
                case '_':
                    return !(char.IsLetterOrDigit(prev) && char.IsLetterOrDigit(next));
                case '<':
                    return char.IsLetter(next) || next == '/' || next == '!' || next == '?';
                case '&':
                    return char.IsLetter(next) || next == '#';
                case '|':
                    return context == InlineContext.TableCell;
                default:
                    return false;
            }
        }

        private static string EscapeText(string text)
        {
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (c == '\\' || c == '[' || c == ']') sb.Append('\\');
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static string LinkDestination(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            if (url.IndexOf(' ') >= 0 || url.IndexOf('(') >= 0 || url.IndexOf(')') >= 0) return "<" + url.Replace("<", "%3C").Replace(">", "%3E") + ">";
            return url;
        }

        #endregion

        /// <summary>
        /// Maps an offset in one Markdown spelling of a block to the matching offset in another spelling of the same
        /// content (<c>__b__</c> against <c>**b**</c>): the characters that are not Markdown syntax are the same in both, so
        /// the offset after the n-th of them in one is the offset after the n-th of them in the other.
        /// </summary>
        internal static int AlignOffset(string from, int offset, string to)
        {
            if (from == to) return Math.Max(0, Math.Min(offset, to.Length));

            offset = Math.Max(0, Math.Min(offset, from.Length));

            var count = 0;
            for (var i = 0; i < offset; i++) if (IsContentChar(from[i])) count++;

            if (count == 0)
            {
                var k = 0;
                while (k < to.Length && !IsContentChar(to[k])) k++;
                return k;
            }

            for (var i = 0; i < to.Length; i++)
            {
                if (IsContentChar(to[i]) && --count == 0) return i + 1;
            }

            return to.Length;
        }

        private static bool IsContentChar(char c) => c != '\\' && c != '*' && c != '_' && c != '`' && c != '~' && c != '#' && c != '>' && c != '[' && c != ']' && c != '(' && c != ')' && c != '|' && c != '\n' && c != ' ' && c != '-' && c != '!' && c != '<';
    }

    /// <summary>One character on its way into the Markdown, with the text node and offset it came from.</summary>
    internal struct MarkdownChar
    {
        public readonly char Char;
        public readonly Node Node;
        public readonly int  Offset;

        public MarkdownChar(char c, Node node, int offset)
        {
            Char   = c;
            Node   = node;
            Offset = offset;
        }
    }

    /// <summary>Where a DOM position (a text node and an offset in it, or an element and a child index) landed in the Markdown.</summary>
    internal sealed class MarkdownPoint
    {
        public int  MarkdownOffset;
        public Node Node;
        public int  NodeOffset;
    }

    /// <summary>The Markdown being written: line prefixes for quotes and list items, deferred block separators, and the position map.</summary>
    internal sealed class MarkdownWriter
    {
        private readonly StringBuilder _sb       = new StringBuilder();
        private readonly List<string>  _prefixes = new List<string>();
        private readonly bool          _track;

        public readonly List<MarkdownPoint> Points = new List<MarkdownPoint>();

        /// <summary>A separator written before the next thing that is written, and dropped if nothing is.</summary>
        public string Pending;

        private int _lineStart;

        public MarkdownWriter(bool track)
        {
            _track = track;
        }

        public int Length => _sb.Length;

        public bool AtLineStart => _sb.Length == _lineStart;

        public void PushPrefix(string prefix) => _prefixes.Add(prefix);

        public void PopPrefix() => _prefixes.RemoveAt(_prefixes.Count - 1);

        public void Truncate(int length)
        {
            _sb.Length = length;
            if (_lineStart > length) _lineStart = length;
            while (Points.Count > 0 && Points[Points.Count - 1].MarkdownOffset > length) Points.RemoveAt(Points.Count - 1);
        }

        public void MarkStart(Node node) => RecordPoint(node, 0);

        public void RecordPoint(Node node, int offset)
        {
            if (!_track || node == null) return;
            FlushPending();
            Points.Add(new MarkdownPoint { MarkdownOffset = _sb.Length, Node = node, NodeOffset = offset });
        }

        public void Write(string text, Node node = null, int offset = 0)
        {
            FlushPending();
            if (node != null) RecordPoint(node, offset);
            WriteRaw(text);
        }

        public void WriteChar(char c, Node node, int offset)
        {
            FlushPending();
            if (_track) Points.Add(new MarkdownPoint { MarkdownOffset = _sb.Length, Node = node, NodeOffset = offset });
            _sb.Append(c);
            if (_track) Points.Add(new MarkdownPoint { MarkdownOffset = _sb.Length, Node = node, NodeOffset = offset + 1 });
        }

        private void FlushPending()
        {
            if (Pending == null) return;
            var pending = Pending;
            Pending = null;
            WriteRaw(pending);
        }

        private void WriteRaw(string text)
        {
            foreach (var c in text)
            {
                _sb.Append(c);

                if (c == '\n')
                {
                    foreach (var p in _prefixes) _sb.Append(p);
                    _lineStart = _sb.Length;
                }
            }
        }

        public override string ToString() => _sb.ToString();
    }
}
