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
        private readonly HTMLElement      _labels;
        private readonly HTMLSpanElement  _copiedSpan;
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

            // The label and "Copied" share one cell, so the button keeps the width of the longer one and the swap
            // is a CSS transition. An icon-only button has no labels at all.
            _copiedSpan = Span(Att(text: _copiedLabel));
            _labels     = _label.Length == 0 ? Span(Att("tss-copybutton-text")) : MicroInteractionExtensions.LabelStack(Span(Att(text: _label)), _copiedSpan);
            _live       = Span(Att("tss-holdbutton-live"));
            _live.setAttribute("aria-live", "polite");

            InnerElement = Button(Att("tss-btn tss-btn-default tss-copybutton tss-fontsize-small tss-fontweight-regular tss-default-component-margin"), icons, _labels, _live);
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
            _copiedSpan.textContent = _copiedLabel;
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

            InnerElement.classList.add("tss-copybutton-copied");
            if (_label.Length > 0) MicroInteractionExtensions.ShowLabel(_labels, 1);
            _live.textContent = string.IsNullOrEmpty(_copiedLabel) ? "Copied" : _copiedLabel;

            _copied?.Invoke(text);

            window.setTimeout(_ =>
            {
                if (generation != _generation) return;
                InnerElement.classList.remove("tss-copybutton-copied");
                if (_label.Length > 0) MicroInteractionExtensions.ShowLabel(_labels, 0);
                _live.textContent = "";
            }, _resetAfterMs);

            return this;
        }

        private void UpdateIconOnly()
        {
            var iconOnly = _label.Length == 0;
            InnerElement.UpdateClassIf(iconOnly, "tss-btn-only-icon");
            InnerElement.style.minWidth = iconOnly ? "unset" : "";

            // The labels are hidden from assistive technology (both are in the DOM at once), so the button is named
            // after its resting label; the confirmation is announced through the live region instead.
            InnerElement.setAttribute("aria-label", iconOnly ? "Copy" : _label);
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
