using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A card with two faces that turns over in 3D to show the other one: a summary and its details, a question and
    /// its answer, a setting and its explanation. The card is as tall as its taller face, so nothing around it moves
    /// when it flips. The face turned away is hidden from screen readers and the tab order.
    /// </summary>
    [Transpose.Name("tss.FlipCard")]
    public sealed class FlipCard : ComponentBase<FlipCard, HTMLElement>
    {
        private readonly HTMLElement _front;
        private readonly HTMLElement _back;

        private event Action<bool> _flipped;

        private bool _isFlipped;
        private bool _flipOnClick;

        /// <summary>
        /// Creates a flip card.
        /// </summary>
        /// <param name="front">The face shown first.</param>
        /// <param name="back">The face shown once flipped.</param>
        public FlipCard(IComponent front, IComponent back)
        {
            _front = Div(Att("tss-flipcard-face tss-flipcard-front"), front.Render());
            _back  = Div(Att("tss-flipcard-face tss-flipcard-back"),  back.Render());

            InnerElement = Div(Att("tss-flipcard"), Div(Att("tss-flipcard-inner"), _front, _back));

            InnerElement.addEventListener("click", e =>
            {
                if (!_flipOnClick || IsFromControl(e)) return;
                Flip();
            });

            InnerElement.addEventListener("keydown", e =>
            {
                if (!_flipOnClick || !ReferenceEquals(e.target, InnerElement)) return;
                var key = e.As<KeyboardEvent>().key;
                if (key != "Enter" && key != " ") return;
                e.preventDefault();
                Flip();
            });

            ApplyFaces();
        }

        /// <summary>Gets or sets whether the back face is showing.</summary>
        public bool IsFlipped
        {
            get => _isFlipped;
            set => Flip(value);
        }

        /// <summary>Turns the card over.</summary>
        public FlipCard Flip() => Flip(!_isFlipped);

        /// <summary>Shows the back face (<c>true</c>) or the front face (<c>false</c>).</summary>
        public FlipCard Flip(bool showBack)
        {
            if (_isFlipped == showBack) return this;
            _isFlipped = showBack;
            ApplyFaces();
            _flipped?.Invoke(showBack);
            return this;
        }

        /// <summary>Registers an action to run when the card turns, with whether the back face is now showing.</summary>
        public FlipCard OnFlip(Action<bool> onFlip)
        {
            _flipped += onFlip;
            return this;
        }

        /// <summary>
        /// Turns the card on a click anywhere on it (except on a button, link or field inside it), and on Enter or
        /// Space while it has focus. The card becomes focusable and is announced as a button.
        /// </summary>
        public FlipCard FlipOnClick(bool value = true)
        {
            _flipOnClick = value;
            InnerElement.UpdateClassIf(value, "tss-flipcard-clickable");

            if (value)
            {
                InnerElement.tabIndex = 0;
                InnerElement.setAttribute("role", "button");
                InnerElement.setAttribute("aria-pressed", _isFlipped ? "true" : "false");
            }
            else
            {
                InnerElement.removeAttribute("tabindex");
                InnerElement.removeAttribute("role");
                InnerElement.removeAttribute("aria-pressed");
            }

            return this;
        }

        /// <summary>Turns the card over its horizontal axis (top over bottom) instead of its vertical one.</summary>
        public FlipCard Vertical(bool value = true)
        {
            InnerElement.UpdateClassIf(value, "tss-flipcard-vertical");
            return this;
        }

        private void ApplyFaces()
        {
            InnerElement.UpdateClassIf(_isFlipped, "tss-flipcard-flipped");
            SetHidden(_front, _isFlipped);
            SetHidden(_back, !_isFlipped);
            if (_flipOnClick) InnerElement.setAttribute("aria-pressed", _isFlipped ? "true" : "false");
        }

        private static void SetHidden(HTMLElement face, bool hidden)
        {
            if (hidden)
            {
                face.setAttribute("aria-hidden", "true");
                face.setAttribute("inert", "");
            }
            else
            {
                face.removeAttribute("aria-hidden");
                face.removeAttribute("inert");
            }
        }

        private bool IsFromControl(Event e)
        {
            var target = e.target.As<HTMLElement>();
            if (target is null) return false;
            var control = target.closest("button, a, input, select, textarea, label, [role=button], [contenteditable]").As<HTMLElement>();
            return control is object && !ReferenceEquals(control, InnerElement) && InnerElement.contains(control);
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
