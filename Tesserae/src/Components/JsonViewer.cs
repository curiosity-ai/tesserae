using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A JSON document drawn as a collapsible, syntax-coloured tree: keys, strings, numbers, booleans and
    /// <c>null</c> each in their own colour, a collapsed object or array summarised by its size, and indent guides
    /// between a container's braces.
    /// <para>
    /// It is built for documents too big to draw at once. A container's children are only built the first time it
    /// is opened, and each container shows its first <see cref="MaxChildren(int)"/> entries, with a row that
    /// shows the next batch (<see cref="ShowMore(bool)"/>). Strings longer than <see cref="MaxStringLength(int)"/>
    /// are cut, and a click shows them whole.
    /// </para>
    /// <para>
    /// Every value goes into the page as text, never as markup, so it is safe to show JSON from anywhere. A
    /// document that does not parse is shown as the parser's error instead of a tree.
    /// </para>
    /// <para>
    /// It is a WAI-ARIA tree: Tab lands on it, Up and Down move between rows, Right opens a container or steps into
    /// it, Left closes it or steps out to its parent, Enter and Space open and close, Home and End go to the first
    /// and last row. Hovering a row (or focusing it) shows a button that copies its value; a key's tooltip is its
    /// path from the root (<c>$.items[3].name</c>).
    /// </para>
    /// </summary>
    [Transpose.Name("tss.JsonViewer")]
    public sealed class JsonViewer : ComponentBase<JsonViewer, HTMLElement>
    {
        private object _value;
        private string _error;
        private bool   _hasValue;
        private int    _expandDepth     = 1;
        private int    _maxChildren     = 100;
        private int    _maxStringLength = 300;
        private bool   _showMore        = true;
        private bool   _copyable        = true;
        private string _rootLabel;
        private bool   _dirty           = true;
        private bool   _rendered;

        /// <summary>
        /// Initializes a new viewer for the given JSON text, or an empty one to be given a document later with
        /// <see cref="Json(string)"/> or <see cref="Value(object)"/>.
        /// </summary>
        public JsonViewer(string json = null)
        {
            InnerElement = Div(Att("tss-jsonviewer", role: "tree"));
            InnerElement.addEventListener("keydown", e => OnKeyDown((KeyboardEvent)e));

            if (json is object) Json(json);
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render()
        {
            if (_dirty) Draw();

            _rendered = true;

            return InnerElement;
        }

        /// <summary>
        /// Shows the given JSON text. Text that does not parse is shown as the parser's error, so a caller can hand
        /// over what it received without checking it first.
        /// </summary>
        public JsonViewer Json(string json)
        {
            var result = Script.Write<object>("(function(t){ try { return { ok: true, v: JSON.parse(t) }; } catch (e) { return { ok: false, e: String((e && e.message) || e) }; } })({0})", json ?? "");

            if (Script.Write<bool>("{0}.ok", result))
            {
                _value = Script.Write<object>("{0}.v", result);
                _error = null;
            }
            else
            {
                _value = null;
                _error = Script.Write<string>("{0}.e", result);
            }

            _hasValue = true;

            return Invalidate();
        }

        /// <summary>
        /// Shows a value that is already parsed: a plain JavaScript object, array, string, number, boolean or
        /// <c>null</c>, as <c>JSON.parse</c> returns them.
        /// </summary>
        public JsonViewer Value(object value)
        {
            _value    = value;
            _error    = null;
            _hasValue = true;

            return Invalidate();
        }

        /// <summary>
        /// How many levels are open when the document is drawn: 0 shows only the root, collapsed; 1 (the default)
        /// opens the root; <see cref="int.MaxValue"/> opens everything (see <see cref="ExpandAll"/>).
        /// </summary>
        public JsonViewer ExpandToDepth(int depth)
        {
            _expandDepth = Math.Max(0, depth);

            return Invalidate();
        }

        /// <summary>
        /// Opens every container. Each still shows only its first <see cref="MaxChildren(int)"/> entries, which is
        /// what keeps this usable on a large document.
        /// </summary>
        public JsonViewer ExpandAll() => ExpandToDepth(int.MaxValue);

        /// <summary>Closes everything below the root.</summary>
        public JsonViewer CollapseAll() => ExpandToDepth(1);

        /// <summary>
        /// How many entries of one object or array are shown before a "show more" row (100 by default); 0 shows
        /// all of them.
        /// </summary>
        public JsonViewer MaxChildren(int count)
        {
            _maxChildren = Math.Max(0, count);

            return Invalidate();
        }

        /// <summary>
        /// Whether the row after the first <see cref="MaxChildren(int)"/> entries shows the next batch when clicked
        /// (the default), or only says how many entries were left out - for a viewer showing part of a document whose
        /// whole is available some other way, such as a download.
        /// </summary>
        public JsonViewer ShowMore(bool showMore = true)
        {
            _showMore = showMore;

            return Invalidate();
        }

        /// <summary>The longest string shown whole (300 characters by default); a longer one is cut, and shown whole when clicked. 0 never cuts.</summary>
        public JsonViewer MaxStringLength(int length)
        {
            _maxStringLength = Math.Max(0, length);

            return Invalidate();
        }

        /// <summary>Whether each row offers a button that copies its value (on by default).</summary>
        public JsonViewer Copyable(bool copyable = true)
        {
            _copyable = copyable;

            return Invalidate();
        }

        /// <summary>A name for the root row - a file name, a field name. Without one the root shows only its value.</summary>
        public JsonViewer RootLabel(string label)
        {
            _rootLabel = label;

            return Invalidate();
        }

        private JsonViewer Invalidate()
        {
            _dirty = true;

            if (_rendered) Draw();

            return this;
        }

        // ----------------------------------------------------------------------------------------
        // Drawing
        // ----------------------------------------------------------------------------------------

        private void Draw()
        {
            _dirty = false;

            ClearChildren(InnerElement);

            if (!_hasValue) return;

            if (_error is object)
            {
                InnerElement.appendChild(Div(Att("tss-jsonviewer-error", role: "alert", text: "Invalid JSON: " + _error)));

                return;
            }

            var root = BuildNode(_rootLabel, isIndex: false, _value, "$", level: 1, isRootLabel: true);

            InnerElement.appendChild(root.Element);

            root.Row.tabIndex = 0;
        }

        private Node BuildNode(string key, bool isIndex, object value, string path, int level, bool isRootLabel = false)
        {
            var node = new Node(value, path, level);
            var row  = Div(Att("tss-jsonviewer-row", role: "treeitem"));

            row.setAttribute("aria-level", level.ToString());
            row.tabIndex  = -1;
            node.Row      = row;
            node.Element  = Div(Att("tss-jsonviewer-node"), row);
            Script.Write("{0}.tssJsonNode = {1}", row, node);

            var isContainer = IsContainer(value) && Count(value) > 0;

            row.appendChild(isContainer ? Span(Att("tss-jsonviewer-toggle"), I(UIcons.AngleSmallRight)) : Span(Att("tss-jsonviewer-toggle")));

            if (key is object)
            {
                var keyClass = isRootLabel ? "tss-jsonviewer-label" : (isIndex ? "tss-jsonviewer-index" : "tss-jsonviewer-key");

                //A root label is a name the host gave the document, not one of its keys, so it is not quoted.
                row.appendChild(Span(Att(keyClass, text: isIndex || isRootLabel ? key : Quote(key), title: path)));
                row.appendChild(Span(Att("tss-jsonviewer-colon", text: ":")));
            }

            if (isContainer)
            {
                var isArray = IsArray(value);
                var count   = Count(value);

                row.setAttribute("aria-expanded", "false");
                row.appendChild(Span(Att("tss-jsonviewer-brace", text: isArray ? "[" : "{")));
                row.appendChild(Span(Att("tss-jsonviewer-summary", text: isArray ? "…]" : "…}")));
                row.appendChild(Span(Att("tss-jsonviewer-count", text: isArray ? (count == 1 ? "1 item" : count.ToString("n0") + " items") : (count == 1 ? "1 key" : count.ToString("n0") + " keys"))));

                node.Children = Div(Att("tss-jsonviewer-children", role: "group"));
                node.Element.appendChild(node.Children);
                node.Element.appendChild(Div(Att("tss-jsonviewer-close", text: isArray ? "]" : "}")));

                row.addEventListener("click", e =>
                {
                    if (IsFromInteractiveChild(e)) return;
                    SetExpanded(node, !node.Expanded);
                });

                if (level <= _expandDepth) SetExpanded(node, true);
            }
            else
            {
                row.appendChild(ScalarElement(value));
            }

            if (_copyable) row.appendChild(CopyButton(value));

            return node;
        }

        private void SetExpanded(Node node, bool expanded)
        {
            if (node.Children is null) return;

            node.Expanded = expanded;
            node.Row.setAttribute("aria-expanded", expanded ? "true" : "false");

            if (expanded)
            {
                node.Element.classList.add("tss-jsonviewer-expanded");

                if (node.Shown == 0) AppendChildren(node);
            }
            else
            {
                node.Element.classList.remove("tss-jsonviewer-expanded");
            }
        }

        /// <summary>The next batch of a container's entries, and a row for the rest when there are more.</summary>
        private void AppendChildren(Node node)
        {
            var value   = node.Value;
            var isArray = IsArray(value);
            var keys    = isArray ? null : Keys(value);
            var count   = isArray ? Length(value) : keys.Length;
            var end     = _maxChildren == 0 ? count : Math.Min(count, node.Shown + _maxChildren);

            for (var i = node.Shown; i < end; i++)
            {
                var name  = isArray ? i.ToString() : keys[i];
                var child = isArray ? Get(value, i) : Get(value, name);
                var path  = isArray ? node.Path + "[" + i + "]" : node.Path + PathSegment(name);

                node.Children.appendChild(BuildNode(name, isArray, child, path, node.Level + 1).Element);
            }

            node.Shown = end;

            if (end >= count) return;

            var remaining = count - end;
            var more      = Div(Att("tss-jsonviewer-more"));

            if (_showMore)
            {
                var batch  = Math.Min(remaining, _maxChildren);
                var button = Button(Att("tss-jsonviewer-more-button", type: "button", text: "Show " + batch.ToString("n0") + " more (" + remaining.ToString("n0") + " left)"));

                button.addEventListener("click", e =>
                {
                    e.stopPropagation();
                    more.remove();
                    AppendChildren(node);
                });

                more.appendChild(button);
            }
            else
            {
                more.textContent = "… " + remaining.ToString("n0") + " more";
            }

            node.Children.appendChild(more);
        }

        private HTMLElement ScalarElement(object value)
        {
            var kind = Kind(value);
            var text = kind == "string" ? Quote(Script.Write<string>("{0}", value)) : Script.Write<string>("JSON.stringify({0})", value);

            if (text is null) text = "null";

            var element = Span(Att("tss-jsonviewer-value tss-jsonviewer-" + kind));

            if (kind != "string" || _maxStringLength == 0 || text.Length <= _maxStringLength + 2)
            {
                element.textContent = text;

                return element;
            }

            var full = text;
            var cut  = text.Substring(0, _maxStringLength + 1) + "…\"";

            element.textContent = cut;
            element.classList.add("tss-jsonviewer-truncated");
            element.title = "Click to show all " + (full.Length - 2).ToString("n0") + " characters";
            element.addEventListener("click", e =>
            {
                e.stopPropagation();

                var isCut = element.classList.contains("tss-jsonviewer-truncated");

                element.textContent = isCut ? full : cut;
                element.classList.toggle("tss-jsonviewer-truncated", !isCut);
                element.title = isCut ? "Click to shorten" : "Click to show all " + (full.Length - 2).ToString("n0") + " characters";
            });

            return element;
        }

        private static HTMLElement CopyButton(object value)
        {
            var button = Button(Att("tss-jsonviewer-copy", type: "button", title: "Copy value", ariaLabel: "Copy value"), I(UIcons.Copy));

            button.tabIndex = -1;
            button.addEventListener("click", e =>
            {
                e.stopPropagation();

                var text = Kind(value) == "string"
                    ? Script.Write<string>("{0}", value)
                    : Script.Write<string>("JSON.stringify({0}, null, 2)", value);

                Clipboard.Copy(text, showMessage: true, customMessage: "Copied");
            });

            return button;
        }

        private static bool IsFromInteractiveChild(Event e)
        {
            var target = e.target?.As<HTMLElement>();

            return target is object && target.closest(".tss-jsonviewer-copy, .tss-jsonviewer-more-button, .tss-jsonviewer-truncated") is object;
        }

        // ----------------------------------------------------------------------------------------
        // Keyboard
        // ----------------------------------------------------------------------------------------

        private void OnKeyDown(KeyboardEvent e)
        {
            var row = e.target?.As<HTMLElement>().closest(".tss-jsonviewer-row")?.As<HTMLElement>();

            if (row is null) return;

            var node = Script.Write<Node>("{0}.tssJsonNode", row);

            switch (e.key)
            {
                case "ArrowDown": Move(row, +1); break;
                case "ArrowUp":   Move(row, -1); break;
                case "Home":      FocusAt(0); break;
                case "End":       FocusAt(-1); break;

                case "ArrowRight":
                    if (node.Children is null) return;
                    if (!node.Expanded) SetExpanded(node, true);
                    else Move(row, +1);
                    break;

                case "ArrowLeft":
                    if (node.Children is object && node.Expanded)
                    {
                        SetExpanded(node, false);
                    }
                    else
                    {
                        var parent = node.Element.parentElement?.closest(".tss-jsonviewer-node")?.querySelector(".tss-jsonviewer-row")?.As<HTMLElement>();
                        if (parent is object) Focus(row, parent);
                    }
                    break;

                case "Enter":
                case " ":
                    if (node.Children is null) return;
                    SetExpanded(node, !node.Expanded);
                    break;

                default: return;
            }

            e.preventDefault();
        }

        /// <summary>The rows on screen, in order: a row inside a closed container is not displayed, so it has no offsetParent.</summary>
        private HTMLElement[] VisibleRows() => Script.Write<HTMLElement[]>("Array.prototype.filter.call({0}.querySelectorAll('.tss-jsonviewer-row'), function(r) { return r.offsetParent !== null; })", InnerElement);

        private void Move(HTMLElement from, int delta)
        {
            var rows  = VisibleRows();
            var index = Array.IndexOf(rows, from) + delta;

            if (index >= 0 && index < rows.Length) Focus(from, rows[index]);
        }

        private void FocusAt(int index)
        {
            var rows = VisibleRows();

            if (rows.Length == 0) return;

            var current = InnerElement.querySelector(".tss-jsonviewer-row[tabindex='0']")?.As<HTMLElement>();

            Focus(current, index < 0 ? rows[rows.Length - 1] : rows[index]);
        }

        //Roving tab stop: the tree is one stop in the page's tab order, on the row that last had focus.
        private static void Focus(HTMLElement from, HTMLElement to)
        {
            if (from is object) from.tabIndex = -1;

            to.tabIndex = 0;
            to.focus();
        }

        // ----------------------------------------------------------------------------------------
        // JSON values, read as the plain JavaScript values JSON.parse returns
        // ----------------------------------------------------------------------------------------

        private static string Kind(object value) => Script.Write<string>("({0} === null ? 'null' : typeof {0})", value);

        private static bool IsArray(object value) => Script.Write<bool>("Array.isArray({0})", value);

        private static bool IsContainer(object value) => Script.Write<bool>("({0} !== null && typeof {0} === 'object')", value);

        private static int Count(object value) => IsArray(value) ? Length(value) : Keys(value).Length;

        private static string[] Keys(object value) => Script.Write<string[]>("Object.keys({0})", value);

        private static int Length(object value) => Script.Write<int>("{0}.length", value);

        private static object Get(object value, string key) => Script.Write<object>("{0}[{1}]", value, key);

        private static object Get(object value, int index) => Script.Write<object>("{0}[{1}]", value, index);

        private static string Quote(string text) => Script.Write<string>("JSON.stringify({0})", text);

        private static string PathSegment(string key) => Script.Write<bool>("/^[A-Za-z_$][A-Za-z0-9_$]*$/.test({0})", key) ? "." + key : "[" + Quote(key) + "]";

        /// <summary>One object, array or scalar in the tree, and the elements drawn for it.</summary>
        [Transpose.Name("tss.JsonViewerNode")]
        private sealed class Node
        {
            public Node(object value, string path, int level)
            {
                Value = value;
                Path  = path;
                Level = level;
            }

            public object      Value    { get; }
            public string      Path     { get; }
            public int         Level    { get; }
            public HTMLElement Element  { get; set; }
            public HTMLElement Row      { get; set; }
            public HTMLElement Children { get; set; }
            public bool        Expanded { get; set; }
            public int         Shown    { get; set; }
        }
    }
}
