using System;
using System.Text;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A one-time-code field drawn as a row of segments, one per character: each segment fills with a small pop as
    /// it is typed, the next one carries the caret, and <see cref="OnCompleted"/> fires when the last is filled.
    /// <para>
    /// Underneath it is a single <c>&lt;input&gt;</c> laid over the segments, so pasting a whole code, the
    /// browser's and the phone's one-time-code autofill, Backspace and screen readers all work as they do in a plain
    /// field. <see cref="Invalid"/> shakes the row and outlines it for a code that was refused.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.CodeInput")]
    public sealed class CodeInput : ComponentBase<CodeInput, HTMLInputElement>, IBindableComponent<string>
    {
        private readonly HTMLElement                _container;
        private readonly HTMLElement[]              _cells;
        private readonly int                        _length;
        private readonly SettableObservable<string> _observable;

        private event Action<string> _completed;

        private bool _numeric = true;
        private bool _masked;

        /// <summary>
        /// Creates a segmented code field.
        /// </summary>
        /// <param name="length">How many characters the code has.</param>
        public CodeInput(int length = 6)
        {
            _length     = Math.Max(1, Math.Min(12, length));
            _observable = new SettableObservable<string>("");
            _cells      = new HTMLElement[_length];

            var row = Div(Att("tss-codeinput-cells"));
            row.setAttribute("aria-hidden", "true");

            for (var i = 0; i < _length; i++)
            {
                _cells[i] = Div(Att("tss-codeinput-cell"), Span(Att("tss-codeinput-char")));
                row.appendChild(_cells[i]);
            }

            InnerElement = UI.TextBox(Att("tss-codeinput-input"));
            InnerElement.maxLength = _length;
            InnerElement.setAttribute("autocomplete", "one-time-code");
            InnerElement.setAttribute("inputmode", "numeric");
            InnerElement.setAttribute("aria-label", $"{_length}-character code");
            InnerElement.spellcheck = false;

            _container = Div(Att("tss-codeinput tss-default-component-margin"), row, InnerElement);
            _container.style.setProperty("--tss-codeinput-length", _length.ToString());

            InnerElement.addEventListener("input", _ => OnTyped());
            InnerElement.addEventListener("focus", _ => { _container.classList.add("tss-codeinput-focused"); KeepCaretAtEnd(); Paint(); });
            InnerElement.addEventListener("blur",  _ => { _container.classList.remove("tss-codeinput-focused"); Paint(); });
            InnerElement.addEventListener("click", _ => KeepCaretAtEnd());
            InnerElement.addEventListener("keyup", _ => KeepCaretAtEnd());

            Paint();
        }

        /// <summary>Gets or sets the code.</summary>
        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = Clean(value ?? "");
                OnTyped(fromUser: false);
            }
        }

        /// <summary>Whether every segment is filled.</summary>
        public bool IsComplete => InnerElement.value.Length == _length;

        /// <summary>Registers an action to run when the last segment is filled, with the code.</summary>
        public CodeInput OnCompleted(Action<string> onCompleted)
        {
            _completed += onCompleted;
            return this;
        }

        /// <summary>Accepts digits only (the default) or any letter and digit.</summary>
        public CodeInput Numeric(bool value = true)
        {
            _numeric = value;
            InnerElement.setAttribute("inputmode", value ? "numeric" : "text");
            InnerElement.style.textTransform = value ? "" : "uppercase";
            return this;
        }

        /// <summary>Draws a dot in each filled segment instead of the character, for a secret code.</summary>
        public CodeInput Masked(bool value = true)
        {
            _masked = value;
            Paint();
            return this;
        }

        /// <summary>Clears the code.</summary>
        public CodeInput Clear()
        {
            Text = "";
            return this;
        }

        /// <summary>
        /// Marks the code as refused (or not): the row turns to the danger colour and, when set, shakes once. Typing
        /// again clears it.
        /// </summary>
        public CodeInput Invalid(bool value = true)
        {
            _container.UpdateClassIf(value, "tss-codeinput-invalid");
            InnerElement.setAttribute("aria-invalid", value ? "true" : "false");
            if (value) MicroInteractionExtensions.Play(_container, "tss-mi-shake");
            return this;
        }

        /// <summary>Enables or disables the field.</summary>
        public CodeInput Disabled(bool value = true)
        {
            InnerElement.disabled = value;
            _container.UpdateClassIf(value, "tss-disabled");
            return this;
        }

        /// <summary>Moves focus into the field.</summary>
        public CodeInput Focus()
        {
            DomObserver.WhenMounted(InnerElement, () => InnerElement.focus());
            return this;
        }

        /// <inheritdoc />
        public IObservable<string> AsObservable() => _observable;

        /// <inheritdoc />
        public void SetBoundValue(string value)
        {
            InnerElement.value = Clean(value ?? "");
            _observable.Value  = InnerElement.value;
            Paint();
        }

        private void OnTyped(bool fromUser = true)
        {
            var before  = _observable.Value ?? "";
            var cleaned = Clean(InnerElement.value);
            if (cleaned != InnerElement.value) InnerElement.value = cleaned;

            if (fromUser && cleaned != before) _container.classList.remove("tss-codeinput-invalid");

            _observable.Value = cleaned;
            Paint(popFrom: before.Length);

            if (cleaned.Length == _length && before.Length != _length) _completed?.Invoke(cleaned);
        }

        private string Clean(string value)
        {
            var sb = new StringBuilder();

            foreach (var c in value)
            {
                if (sb.Length >= _length) break;
                if (_numeric ? char.IsDigit(c) : char.IsLetterOrDigit(c)) sb.Append(_numeric ? c : char.ToUpper(c));
            }

            return sb.ToString();
        }

        private void KeepCaretAtEnd()
        {
            var end = InnerElement.value.Length;
            try { InnerElement.setSelectionRange((uint)end, (uint)end); } catch { }
        }

        private void Paint(int popFrom = int.MaxValue)
        {
            var value   = InnerElement.value ?? "";
            var focused = _container.classList.contains("tss-codeinput-focused");
            var active  = Math.Min(value.Length, _length - 1);

            for (var i = 0; i < _length; i++)
            {
                var cell = _cells[i];
                var ch   = cell.firstElementChild.As<HTMLElement>();
                var has  = i < value.Length;

                ch.textContent = has ? (_masked ? "•" : value[i].ToString()) : "";
                cell.UpdateClassIf(has, "tss-codeinput-filled");
                cell.UpdateClassIf(focused && i == active, "tss-codeinput-active");
                cell.UpdateClassIf(focused && i == value.Length, "tss-codeinput-caret");

                if (has && i >= popFrom) MicroInteractionExtensions.Play(ch, "tss-codeinput-pop");
            }
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => _container;
    }
}
