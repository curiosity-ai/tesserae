using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using Range = Transpose.Core.dom.Range;

namespace Tesserae
{
    /// <summary>
    /// Binds a <see cref="SquireEditor"/> to a shared <see cref="Y.Text"/> that holds <b>Markdown</b>, for real-time
    /// collaborative editing. Because the shared text is Markdown, the other end can be anything that edits Markdown:
    /// another <see cref="SquireEditor"/>, or a code editor bound to the same text (Tesserae.Monaco's <c>BindYjs</c>).
    /// </summary>
    /// <remarks>
    /// <para><b>Local edits.</b> After every edit the editor is read back as Markdown
    /// (<see cref="SquireEditor.Markdown"/>) and the difference to the shared text is written as one transaction: the
    /// common prefix and suffix are kept and the middle is replaced. Blocks nobody edited are read back exactly as they
    /// were written, so an edit in one paragraph only ever changes that paragraph's Markdown.</para>
    /// <para><b>Remote edits.</b> The new Markdown is split into blocks, and only the blocks that differ from what the
    /// editor shows are rendered again; the elements before and after them stay, and with them a caret in them. A caret
    /// inside a re-rendered block is carried through the change as a Markdown offset.</para>
    /// <para><b>Presence.</b> With an <see cref="Awareness"/>, the editor's selection is published as the awareness
    /// field <c>selection</c> = <c>{ anchor, head }</c> of relative positions into the shared text - the shape y-monaco
    /// and Tesserae.Monaco use - so a code editor on the same Markdown shows this user's caret where it is in the text,
    /// and this editor shows theirs. Remote carets and selections are drawn over the editor in the collaborator's
    /// colour (<c>user.color</c>, see <see cref="YjsUserColors"/>) with their name next to the caret while it moves.</para>
    /// <para>Binding turns on the editor's <see cref="SquireEditor.MarkdownMode"/>: the toolbar offers only what the
    /// shared Markdown can hold, and pasted HTML is converted to Markdown on the way in.</para>
    /// <para><b>Undo.</b> With <c>undo</c> on, the editor's undo and redo (the toolbar, Ctrl+Z, Ctrl+Y) go through a
    /// <see cref="Y.UndoManager"/> that only reverts this user's own edits, never a collaborator's.</para>
    /// </remarks>
    public sealed class SquireYjsBinding : IDisposable
    {
        /// <summary>The awareness field the local selection is published in.</summary>
        public const string AwarenessSelectionField = "selection";

        private const int NameVisibleMilliseconds = 2000;

        private readonly SquireEditor _editor;
        private readonly Y.Text       _text;
        private readonly Y.Doc        _doc;
        private readonly Awareness    _awareness;
        private readonly bool         _withUndo;

        private readonly Action<Event>                      _onInput;
        private readonly Action<Event>                      _onSelection;
        private readonly Action<Y.TextEvent, Y.Transaction> _onText;
        private readonly Action<AwarenessChanges, object>   _onAwareness;
        private readonly Action<Event>                      _onResize;

        private readonly Dictionary<int, string> _lastCaret  = new Dictionary<int, string>();
        private readonly Dictionary<int, double> _caretMoved = new Dictionary<int, double>();

        private Squire          _squire;
        private Y.UndoManager   _undo;
        private string          _published;
        private bool            _applying;
        private bool            _disposed;
        private double          _nameTimer;

        /// <summary>
        /// Binds <paramref name="editor"/> to <paramref name="text"/>. The editor takes the shared text's content once
        /// Squire has loaded (<see cref="Attached"/>).
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="text">The shared Markdown. It must be in a document, and Yjs must be loaded (<see cref="Yjs.LoadAsync"/>).</param>
        /// <param name="awareness">Presence for remote cursors and the local selection, or null for none.</param>
        /// <param name="undo">Whether undo and redo only revert this user's own edits.</param>
        public SquireYjsBinding(SquireEditor editor, Y.Text text, Awareness awareness = null, bool undo = true)
        {
            _editor    = editor ?? throw new ArgumentNullException(nameof(editor));
            _text      = text   ?? throw new ArgumentNullException(nameof(text));
            _doc       = text.doc ?? throw new ArgumentException("The text has to be part of a Y.Doc.", nameof(text));
            _awareness = awareness;
            _withUndo  = undo;

            // The shared text is Markdown: formatting it cannot hold would be lost on the other side, so do not offer it.
            _editor.MarkdownMode();

            _onInput     = _ => OnLocalInput();
            _onSelection = _ => PublishSelection();
            _onText      = (e, transaction) => OnTextChanged(e, transaction);
            _onAwareness = (changes, origin) => RenderCursors();
            _onResize    = _ => RenderCursors();

            Attached = AttachAsync();
        }

        /// <summary>Completes once the editor shows the shared text and edits flow both ways.</summary>
        public Task Attached { get; }

        /// <summary>The shared text.</summary>
        public Y.Text Text => _text;

        /// <summary>The editor.</summary>
        public SquireEditor Editor => _editor;

        /// <summary>The undo manager behind the editor's undo and redo, or null when <c>undo</c> was off.</summary>
        public Y.UndoManager UndoManager => _undo;

        private async Task AttachAsync()
        {
            await _editor.Ready;

            if (_disposed) return;

            _squire = _editor.Squire;

            _applying = true;
            try
            {
                _editor.MarkdownDocument.Render(_squire, _text.asString());
            }
            finally
            {
                _applying = false;
            }

            _squire.onInput(_onInput);
            _squire.onSelectionChange(_onSelection);
            _text.observe(_onText);
            window.addEventListener("resize", _onResize);

            if (_awareness != null) _awareness.onChange(_onAwareness);

            if (_withUndo)
            {
                _undo = new Y.UndoManager(_text);
                _undo.addTrackedOrigin(this);
                _undo.removeTrackedOrigin(null);

                Action undo = () => { _undo?.undo(); };
                Action redo = () => { _undo?.redo(); };

                // An own property shadows the prototype's: every way into undo (toolbar, keys, beforeinput) lands here.
                Script.Write("{0}.undo = function() { {1}(); return this.focus(); }; {0}.redo = function() { {2}(); return this.focus(); };", _squire, undo, redo);
            }

            RenderCursors();
        }

        #region Local edits

        private void OnLocalInput()
        {
            if (_applying || _disposed) return;

            var next = _editor.MarkdownDocument.Read(_editor.ContentElement).Markdown;
            var prev = _text.asString();

            if (next != prev)
            {
                var prefix = 0;
                var max    = Math.Min(next.Length, prev.Length);

                while (prefix < max && next[prefix] == prev[prefix]) prefix++;

                var suffix = 0;

                while (suffix < max - prefix && next[next.Length - 1 - suffix] == prev[prev.Length - 1 - suffix]) suffix++;

                var deleted  = prev.Length - prefix - suffix;
                var inserted = next.Substring(prefix, next.Length - prefix - suffix);

                _doc.transact(_ =>
                {
                    if (deleted > 0) _text.@delete(prefix, deleted);
                    if (inserted.Length > 0) _text.insert(prefix, inserted, null);
                }, this);
            }

            PublishSelection();
            RenderCursors();
        }

        private void PublishSelection()
        {
            if (_awareness == null || _disposed || _applying || _squire == null || !_editor.HasFocus) return;

            var range = _squire.getSelection();
            if (range == null || !_editor.ContentElement.contains(range.startContainer)) return;

            var snapshot = _editor.MarkdownDocument.Read(_editor.ContentElement);
            var start    = SquireMarkdownDocument.OffsetOf(snapshot, range.startContainer, (int)range.startOffset);
            var end      = range.collapsed ? start : SquireMarkdownDocument.OffsetOf(snapshot, range.endContainer, (int)range.endOffset);
            var length   = _text.length;

            var state = new SquireYjsSelection
            {
                anchor = Y.createRelativePositionFromTypeIndex(_text, Math.Min(start, length), 0),
                head   = Y.createRelativePositionFromTypeIndex(_text, Math.Min(end,   length), 0)
            };

            var key = Transpose.Core.es5.JSON.stringify(state);
            if (key == _published) return;

            _published = key;
            _awareness.setLocalStateField(AwarenessSelectionField, state);
        }

        #endregion

        #region Remote edits

        private void OnTextChanged(Y.TextEvent e, Y.Transaction transaction)
        {
            if (transaction.origin == (object)this || _disposed || _squire == null) return;

            var delta = e.delta;

            Func<int, int> transform = offset =>
            {
                var index = 0;

                for (var i = 0; i < delta.Length; i++)
                {
                    var op = delta[i];

                    if (op.retain.HasValue)
                    {
                        index += op.retain.Value;
                    }
                    else if (op.insert is string s)
                    {
                        if (offset > index) offset += s.Length;
                        index += s.Length;
                    }
                    else if (op.@delete.HasValue)
                    {
                        var count = op.@delete.Value;
                        if (offset > index) offset -= Math.Min(count, offset - index);
                    }
                }

                return offset;
            };

            _applying = true;

            try
            {
                var current = _editor.MarkdownDocument.Read(_editor.ContentElement);
                _editor.MarkdownDocument.Update(_squire, _text.asString(), current, transform);
            }
            finally
            {
                _applying = false;
            }

            RenderCursors();
        }

        #endregion

        #region Remote cursors

        private void RenderCursors()
        {
            var layer = _editor.CursorLayer;

            if (_disposed || _squire == null)
            {
                layer.innerHTML = "";
                return;
            }

            layer.innerHTML = "";

            if (_awareness == null) return;

            Transpose.Core.es5.Map<int, AwarenessState> states = _awareness.getStates();
            SquireMarkdownDocument.Snapshot snapshot = null;
            var layerRect = layer.getBoundingClientRect().As<DOMRect>();
            var now       = DateTime.UtcNow.Ticks / 10000.0;
            var nextHide  = double.MaxValue;

            states.forEach((state, clientId, map) =>
            {
                if (clientId == _doc.clientID || state == null) return;

                var field = state.field(AwarenessSelectionField);
                if (field == null) return;

                var selection = (SquireYjsSelection)field;
                if (selection.anchor == null || selection.head == null) return;

                var anchor = Y.createAbsolutePositionFromRelativePosition(Y.createRelativePositionFromJSON(selection.anchor), _doc);
                var head   = Y.createAbsolutePositionFromRelativePosition(Y.createRelativePositionFromJSON(selection.head),   _doc);

                if (anchor == null || head == null || anchor.type != (Y.AbstractType)_text || head.type != (Y.AbstractType)_text) return;

                if (snapshot == null) snapshot = _editor.MarkdownDocument.Read(_editor.ContentElement);

                var a = SquireMarkdownDocument.PositionAt(snapshot, anchor.index);
                var h = SquireMarkdownDocument.PositionAt(snapshot, head.index);
                if (a == null || h == null) return;

                var user  = state.user;
                var name  = user != null ? user.name : null;
                var color = user != null && !string.IsNullOrEmpty(user.color) ? user.color : YjsUserColors.ColorFor(clientId.ToString());
                var light = user != null && !string.IsNullOrEmpty(user.colorLight) ? user.colorLight : color + "33";

                if (anchor.index != head.index)
                {
                    var range    = document.createRange();
                    var forwards = anchor.index < head.index;
                    var from     = forwards ? a : h;
                    var to       = forwards ? h : a;

                    range.setStart(from.Item1, (uint)from.Item2);
                    range.setEnd(to.Item1, (uint)to.Item2);

                    var rects = range.getClientRects().As<DOMRectList>();
                    for (var i = 0; i < rects.length; i++)
                    {
                        var r = rects.item((uint)i);
                        if (r.width < 1) continue;
                        var box = (HTMLElement)document.createElement("div");
                        box.className        = "tss-squire-remote-selection";
                        box.style.left       = (r.left - layerRect.left) + "px";
                        box.style.top        = (r.top - layerRect.top) + "px";
                        box.style.width      = r.width + "px";
                        box.style.height     = r.height + "px";
                        box.style.background = light;
                        layer.appendChild(box);
                    }
                }

                var caretRect = CaretRect(h);
                if (caretRect == null) return;

                var caret = (HTMLElement)document.createElement("div");
                caret.className         = "tss-squire-remote-caret";
                caret.style.left        = (caretRect.left - layerRect.left) + "px";
                caret.style.top         = (caretRect.top - layerRect.top) + "px";
                caret.style.height      = caretRect.height + "px";
                caret.style.borderColor = color;

                var key = anchor.index + ":" + head.index;
                if (!_lastCaret.TryGetValue(clientId, out var last) || last != key)
                {
                    _lastCaret[clientId]  = key;
                    _caretMoved[clientId] = now;
                }

                if (!string.IsNullOrWhiteSpace(name) && _caretMoved.TryGetValue(clientId, out var moved) && now - moved < NameVisibleMilliseconds)
                {
                    var label = (HTMLElement)document.createElement("div");
                    label.className        = "tss-squire-remote-name";
                    label.textContent      = name;
                    label.style.background = color;
                    caret.appendChild(label);
                    nextHide = Math.Min(nextHide, moved + NameVisibleMilliseconds - now);
                }

                layer.appendChild(caret);
            });

            if (_nameTimer != 0) window.clearTimeout(_nameTimer);
            _nameTimer = 0;

            if (nextHide < double.MaxValue)
            {
                _nameTimer = window.setTimeout(_ =>
                {
                    _nameTimer = 0;
                    RenderCursors();
                }, Math.Max(50, nextHide + 20));
            }
        }

        private static DOMRect CaretRect(Tuple<Node, int> position)
        {
            var range = document.createRange();
            range.setStart(position.Item1, (uint)position.Item2);
            range.collapse(true);

            var rects = range.getClientRects().As<DOMRectList>();
            if (rects.length > 0) return rects.item(0);

            // A collapsed range in an empty block (or next to a <br>) has no box: use the element's.
            var node = position.Item1;
            if (node.nodeType == 1 && position.Item2 < node.childNodes.length)
            {
                var child = (Node)node.childNodes[(uint)position.Item2];
                if (child.nodeType == 1) return ((Element)child).getBoundingClientRect().As<DOMRect>();
            }

            var element = node.nodeType == 1 ? (Element)node : (Element)node.parentNode;
            return element?.getBoundingClientRect().As<DOMRect>();
        }

        #endregion

        /// <summary>Stops synchronising. The editor and the shared text keep their content.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_nameTimer != 0) window.clearTimeout(_nameTimer);

            _text.unobserve(_onText);
            window.removeEventListener("resize", _onResize);

            if (_squire != null)
            {
                _squire.offInput(_onInput);
                _squire.offSelectionChange(_onSelection);
                Script.Write("delete {0}.undo; delete {0}.redo;", _squire);
            }

            if (_awareness != null)
            {
                _awareness.offChange(_onAwareness);
                _awareness.setLocalStateField(AwarenessSelectionField, null);
            }

            _undo?.destroy();
            _undo = null;

            _editor.CursorLayer.innerHTML = "";
        }
    }

    /// <summary>The awareness <c>selection</c> field: relative positions into the shared text, as y-monaco publishes it.</summary>
    [ObjectLiteral]
    public class SquireYjsSelection
    {
        public object anchor;
        public object head;
    }

    /// <summary>Binds a <see cref="SquireEditor"/> to a shared Markdown <see cref="Y.Text"/>.</summary>
    public static class SquireYjsBindingExtensions
    {
        /// <summary>Binds the editor to <paramref name="text"/> (see <see cref="SquireYjsBinding"/>). Dispose the binding to unbind.</summary>
        public static SquireYjsBinding BindYjs(this SquireEditor editor, Y.Text text, Awareness awareness = null, bool undo = true) => new SquireYjsBinding(editor, text, awareness, undo);
    }
}
