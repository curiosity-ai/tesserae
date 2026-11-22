using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Binds a plain <see cref="TextArea"/> or <see cref="TextBox"/> to a <see cref="Y.Text"/>: what the user types is
    /// written into the shared text, and what other peers write appears in the field with the caret kept where it was.
    /// A minimal binding for plain text - an editor with its own model binds its own way (Tesserae.Monaco's
    /// <c>BindYjs</c>, for one).
    /// </summary>
    /// <remarks>
    /// <para>The field reports the result of an edit, not the edit, so each input is diffed against the previous value:
    /// the common prefix and suffix are kept and the middle is replaced. That is exact for typing, pasting and
    /// deleting; for an ambiguous edit (typing a second <c>a</c> after an <c>a</c>) it picks one of the equivalent
    /// positions, which converges to the same text.</para>
    /// <para>A remote change is applied by replacing the field's value and moving the selection through the change's
    /// delta, so text inserted before the caret pushes it along and text inserted after it leaves it alone.</para>
    /// </remarks>
    public sealed class YjsInputBinding : IDisposable
    {
        private readonly Y.Text      _text;
        private readonly HTMLElement _element;

        private readonly Action<Event>                    _onInput;
        private readonly Action<Y.TextEvent, Y.Transaction> _onTextChanged;

        private string _value;
        private bool   _disposed;

        /// <summary>Binds <paramref name="textArea"/> to <paramref name="text"/>; the field takes the shared text's content.</summary>
        public YjsInputBinding(TextArea textArea, Y.Text text) : this((HTMLElement)textArea.InnerElement, text) { }

        /// <summary>Binds <paramref name="textBox"/> to <paramref name="text"/>; the field takes the shared text's content.</summary>
        public YjsInputBinding(TextBox textBox, Y.Text text) : this((HTMLElement)textBox.InnerElement, text) { }

        private YjsInputBinding(HTMLElement element, Y.Text text)
        {
            _element = element;
            _text    = text ?? throw new ArgumentNullException(nameof(text));
            _value   = _text.asString();

            Field.value = _value;

            _onInput       = _ => OnInput();
            _onTextChanged = (e, transaction) => OnTextChanged(e, transaction);

            _element.addEventListener("input", _onInput);
            _text.observe(_onTextChanged);
        }

        /// <summary>The shared text.</summary>
        public Y.Text Text => _text;

        // An <input> and a <textarea> have the same value and selection members, so the binding reads both through
        // one type. The cast emits nothing.
        private HTMLTextAreaElement Field => (HTMLTextAreaElement)(object)_element;

        private void OnInput()
        {
            var next = Field.value;
            var prev = _value;

            if (next == prev) return;

            var prefix = 0;
            var max    = Math.Min(next.Length, prev.Length);

            while (prefix < max && next[prefix] == prev[prefix]) prefix++;

            var suffix = 0;

            while (suffix < max - prefix && next[next.Length - 1 - suffix] == prev[prev.Length - 1 - suffix]) suffix++;

            var deleted  = prev.Length - prefix - suffix;
            var inserted = next.Substring(prefix, next.Length - prefix - suffix);

            _value = next;

            _text.doc.transact(_ =>
            {
                if (deleted > 0) _text.@delete(prefix, deleted);
                if (inserted.Length > 0) _text.insert(prefix, inserted, null);
            }, this);
        }

        private void OnTextChanged(Y.TextEvent e, Y.Transaction transaction)
        {
            if (transaction.origin == (object)this) return;

            var field    = Field;
            var focused  = document.activeElement == _element;
            var start    = (int)field.selectionStart;
            var end      = (int)field.selectionEnd;
            var index    = 0;
            var delta    = e.delta;

            for (var i = 0; i < delta.Length; i++)
            {
                var op = delta[i];

                if (op.retain.HasValue)
                {
                    index += op.retain.Value;
                }
                else if (op.insert is string s)
                {
                    if (start > index) start += s.Length;
                    if (end   > index) end   += s.Length;
                    index += s.Length;
                }
                else if (op.@delete.HasValue)
                {
                    var count = op.@delete.Value;
                    if (start > index) start -= Math.Min(count, start - index);
                    if (end   > index) end   -= Math.Min(count, end - index);
                }
            }

            _value      = _text.asString();
            field.value = _value;

            if (focused) field.setSelectionRange((uint)start, (uint)end);
        }

        /// <summary>Stops listening to the field and to the shared text. Both keep their content.</summary>
        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _element.removeEventListener("input", _onInput);
            _text.unobserve(_onTextChanged);
        }
    }

    /// <summary>Binds Tesserae's text fields to a shared <see cref="Y.Text"/>.</summary>
    public static class YjsInputBindingExtensions
    {
        /// <summary>Binds the field to <paramref name="text"/> (see <see cref="YjsInputBinding"/>). Dispose the binding to unbind.</summary>
        public static YjsInputBinding BindYjs(this TextArea textArea, Y.Text text) => new YjsInputBinding(textArea, text);

        /// <summary>Binds the field to <paramref name="text"/> (see <see cref="YjsInputBinding"/>). Dispose the binding to unbind.</summary>
        public static YjsInputBinding BindYjs(this TextBox textBox, Y.Text text) => new YjsInputBinding(textBox, text);
    }
}
