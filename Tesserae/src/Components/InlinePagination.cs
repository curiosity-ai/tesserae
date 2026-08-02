using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The compact "‹ 3 of 7 ›" control: a chevron either side of where you are in a set, inside one
    /// rounded pill. It is for stepping through things one at a time - the result open in a preview, the
    /// photo in a lightbox, the record in an editor - beside other commands in a toolbar or a header,
    /// where <see cref="Pagination"/>'s row of numbered page buttons would be far too much.
    /// <para>
    /// Each chevron is enabled by having a handler: <see cref="OnPrevious(Action{InlinePagination})"/> and
    /// <see cref="OnNext(Action{InlinePagination})"/>. Leaving one out greys its chevron, which is how the
    /// first and the last of a set say so - the position and count only write the label, so a set that
    /// loads more as it goes stays in charge of when there is a next.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.InlinePagination")]
    public sealed class InlinePagination : ComponentBase<InlinePagination, HTMLElement>
    {
        private readonly HTMLButtonElement _previous;
        private readonly HTMLButtonElement _next;
        private readonly HTMLElement       _label;

        private int    _position;
        private int    _count;
        private string _text;

        private Func<int, int, string> _format = (position, count) => $"{position} of {count}";

        private Action<InlinePagination> _onPrevious;
        private Action<InlinePagination> _onNext;

        /// <summary>
        /// Initializes a new instance of this class showing the given position in the given count, both
        /// 1-based. A count of zero leaves the label out and the control is the two chevrons alone.
        /// </summary>
        public InlinePagination(int position = 0, int count = 0)
        {
            _previous = Chevron(UIcons.AngleLeft,  "Previous", () => _onPrevious?.Invoke(this));
            _next     = Chevron(UIcons.AngleRight, "Next",     () => _onNext?.Invoke(this));

            _label = Span(Att("tss-inlinepagination-label"));

            InnerElement = Div(Att("tss-inlinepagination"), _previous, _label, _next);

            SetPosition(position, count);
            UpdateChevrons();
        }

        /// <summary>Gets or sets where in the set the control says you are, 1-based.</summary>
        public int Position
        {
            get => _position;
            set => SetPosition(value, _count);
        }

        /// <summary>Gets or sets how many there are in the set. Zero leaves the label out.</summary>
        public int Count
        {
            get => _count;
            set => SetPosition(_position, value);
        }

        /// <summary>Returns a value indicating whether the previous chevron is enabled.</summary>
        public bool CanGoPrevious => _onPrevious is object;

        /// <summary>Returns a value indicating whether the next chevron is enabled.</summary>
        public bool CanGoNext => _onNext is object;

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Sets where in the set the control says you are, and how many there are - both 1-based. A count
        /// of zero (or a position past it) leaves the label out, so a set whose size isn't known yet shows
        /// the chevrons alone rather than a label that would have to lie.
        /// </summary>
        public InlinePagination SetPosition(int position, int count)
        {
            _position = position < 0 ? 0 : position;
            _count    = count    < 0 ? 0 : count;
            _text     = null;

            return RenderLabel();
        }

        /// <summary>
        /// Puts the given text between the chevrons in place of the position and count - "Page 3",
        /// "March", the name of the thing you are on. Pass null to go back to the counted label.
        /// </summary>
        public InlinePagination SetLabel(string text)
        {
            _text = text;

            return RenderLabel();
        }

        /// <summary>
        /// Changes how the position and the count are written - for another language, or for "3 / 7".
        /// </summary>
        public InlinePagination SetFormat(Func<int, int, string> format)
        {
            _format = format ?? ((position, count) => $"{position} of {count}");

            return RenderLabel();
        }

        /// <summary>
        /// Registers what the previous chevron does, and enables it. Pass null to grey it out - which is
        /// how the first of a set says it is the first.
        /// </summary>
        public InlinePagination OnPrevious(Action<InlinePagination> onPrevious)
        {
            _onPrevious = onPrevious;

            return UpdateChevrons();
        }

        /// <summary>
        /// Registers what the next chevron does, and enables it. Pass null to grey it out.
        /// </summary>
        public InlinePagination OnNext(Action<InlinePagination> onNext)
        {
            _onNext = onNext;

            return UpdateChevrons();
        }

        /// <summary>
        /// Sets what each chevron is called for a screen reader and in its tooltip - "Previous result" and
        /// "Next result" rather than the bare "Previous" and "Next" they carry by default.
        /// </summary>
        public InlinePagination SetTooltips(string previous, string next)
        {
            Describe(_previous, previous);
            Describe(_next,     next);

            return this;
        }

        private InlinePagination RenderLabel()
        {
            var text = _text ?? (_count > 0 && _position > 0 ? _format(_position, _count) : null);

            var isEmpty = string.IsNullOrEmpty(text);

            _label.textContent   = isEmpty ? string.Empty : text;
            _label.style.display = isEmpty ? "none" : "";

            return this;
        }

        private InlinePagination UpdateChevrons()
        {
            _previous.disabled = _onPrevious is null;
            _next.disabled     = _onNext is null;

            return this;
        }

        private static HTMLButtonElement Chevron(UIcons icon, string label, Action onClick)
        {
            var button = UI.Button(Att("tss-inlinepagination-button", type: "button"), I(icon, UIconsWeight.Regular));

            Describe(button, label);

            button.addEventListener("click", e =>
            {
                StopEvent(e);

                onClick();
            });

            return button;
        }

        private static void Describe(HTMLElement button, string label)
        {
            if (string.IsNullOrEmpty(label)) return;

            button.setAttribute("aria-label", label);
            button.setAttribute("title",      label);
        }
    }
}
