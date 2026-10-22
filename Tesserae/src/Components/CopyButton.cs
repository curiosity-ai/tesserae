using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A button that copies text to the clipboard and says so in place: the copy icon turns into a check, the label
    /// (if it has one) morphs to "Copied", and both turn back after a moment. The confirmation is where the user is
    /// looking, so no toast is needed, and it is announced to screen readers.
    /// </summary>
    [Transpose.Name("tss.CopyButton")]
    public sealed class CopyButton : ComponentBase<CopyButton, HTMLButtonElement>
    {
        private readonly Func<string>     _getText;
        private readonly HTMLSpanElement  _textSpan;
        private readonly HTMLSpanElement  _live;

        private event Action<string> _copied;

        private string _label;
        private string _copiedLabel = "Copied";
        private int    _resetAfterMs = 1600;
        private int    _generation;

        /// <summary>
        /// Creates a copy button for a fixed text.
        /// </summary>
        /// <param name="textToCopy">The text the button copies.</param>
        /// <param name="label">The label beside the icon; null or empty draws an icon-only button, named "Copy".</param>
        public CopyButton(string textToCopy, string label = null) : this(() => textToCopy, label) { }

        /// <summary>
        /// Creates a copy button that reads the text to copy at the moment it is clicked.
        /// </summary>
        /// <param name="getTextToCopy">Returns the text the button copies.</param>
        /// <param name="label">The label beside the icon; null or empty draws an icon-only button, named "Copy".</param>
        public CopyButton(Func<string> getTextToCopy, string label = null)
        {
            _getText = getTextToCopy ?? (() => "");
            _label   = label ?? "";

            var icons = Span(Att("tss-copybutton-icons"),
                I(UIcons.Copy,  cssClass: "tss-copybutton-copy"),
                I(UIcons.Check, cssClass: "tss-copybutton-check"));

            _textSpan = Span(Att("tss-copybutton-text", text: _label));
            _live     = Span(Att("tss-holdbutton-live"));
            _live.setAttribute("aria-live", "polite");

            InnerElement = Button(Att("tss-btn tss-btn-default tss-copybutton tss-fontsize-small tss-fontweight-regular tss-default-component-margin"), icons, _textSpan, _live);
            InnerElement.type = "button";
            UpdateIconOnly();

            InnerElement.addEventListener("click", _ => Copy());
        }

        /// <summary>Registers an action to run after a copy, with the text that was copied.</summary>
        public CopyButton OnCopied(Action<string> onCopied)
        {
            _copied += onCopied;
            return this;
        }

        /// <summary>The label shown while the copy is confirmed. Defaults to "Copied".</summary>
        public CopyButton CopiedText(string text)
        {
            _copiedLabel = text ?? "";
            return this;
        }

        /// <summary>How long the confirmation is shown, in milliseconds.</summary>
        public CopyButton ResetAfter(int milliseconds)
        {
            _resetAfterMs = Math.Max(300, milliseconds);
            return this;
        }

        /// <summary>Uses the primary tone.</summary>
        public CopyButton Primary()
        {
            InnerElement.classList.remove("tss-btn-default");
            InnerElement.classList.add("tss-btn-primary");
            return this;
        }

        /// <summary>Draws the button without a background or border, for use inside a code block or a field.</summary>
        public CopyButton NoBackground()
        {
            InnerElement.classList.add("tss-btn-nobg", "tss-copybutton-ghost");
            return this;
        }

        /// <summary>Copies now, as a click would, and plays the confirmation.</summary>
        public CopyButton Copy()
        {
            var text = _getText() ?? "";
            Clipboard.Copy(text, showMessage: false);

            var generation = ++_generation;
            var label      = _label.Length == 0 ? "" : _copiedLabel;

            MicroInteractionExtensions.MorphWidth(InnerElement, () =>
            {
                InnerElement.classList.add("tss-copybutton-copied");
                _textSpan.textContent = label;
            });

            if (label.Length > 0) MicroInteractionExtensions.Play(_textSpan, "tss-mi-text-in");
            _live.textContent = string.IsNullOrEmpty(_copiedLabel) ? "Copied" : _copiedLabel;

            _copied?.Invoke(text);

            window.setTimeout(_ =>
            {
                if (generation != _generation) return;
                MicroInteractionExtensions.MorphWidth(InnerElement, () =>
                {
                    InnerElement.classList.remove("tss-copybutton-copied");
                    _textSpan.textContent = _label;
                });
                _live.textContent = "";
            }, _resetAfterMs);

            return this;
        }

        private void UpdateIconOnly()
        {
            var iconOnly = _label.Length == 0;
            InnerElement.UpdateClassIf(iconOnly, "tss-btn-only-icon");
            InnerElement.style.minWidth = iconOnly ? "unset" : "";

            if (iconOnly) InnerElement.setAttribute("aria-label", "Copy");
            else InnerElement.removeAttribute("aria-label");
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
