using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Lays two components over each other and lets the user drag a divider across to compare them: "before" on the
    /// left of the divider, "after" on the right. For two renderings of the same thing (an image before and after an
    /// edit, two versions of a chart, light and dark). The divider is a slider to assistive technology and moves with
    /// the arrow keys (Shift for bigger steps), Home and End.
    /// <para>
    /// The "after" component sets the size; "before" is stretched over it, so the two should be the same shape.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.CompareSlider")]
    public sealed class CompareSlider : ComponentBase<CompareSlider, HTMLElement>
    {
        private readonly HTMLElement _handle;
        private readonly HTMLElement _beforeLabel;
        private readonly HTMLElement _afterLabel;

        private event Action<double> _positionChanged;

        private double _position;
        private bool   _dragging;

        /// <summary>
        /// Creates a before/after comparison.
        /// </summary>
        /// <param name="before">Shown to the left of the divider.</param>
        /// <param name="after">Shown to the right of the divider; it sets the component's size.</param>
        /// <param name="position">Where the divider starts, in percent of the width.</param>
        public CompareSlider(IComponent before, IComponent after, double position = 50)
        {
            _beforeLabel = Span(Att("tss-compare-label tss-compare-label-before"));
            _afterLabel  = Span(Att("tss-compare-label tss-compare-label-after"));

            _handle = Div(Att("tss-compare-handle"), Div(Att("tss-compare-knob"), I(UIcons.AngleLeft), I(UIcons.AngleRight)));
            _handle.tabIndex = 0;
            _handle.setAttribute("role", "slider");
            _handle.setAttribute("aria-label", "Comparison position");
            _handle.setAttribute("aria-valuemin", "0");
            _handle.setAttribute("aria-valuemax", "100");

            InnerElement = Div(Att("tss-compare"),
                Div(Att("tss-compare-after"),  after.Render()),
                Div(Att("tss-compare-before"), before.Render()),
                _beforeLabel, _afterLabel, _handle);

            InnerElement.addEventListener("pointerdown", e =>
            {
                if (e.As<MouseEvent>().button != 0) return;
                _dragging = true;
                InnerElement.classList.add("tss-compare-dragging");
                Script.Write("try { {0}.setPointerCapture({1}.pointerId); } catch (x) { }", InnerElement, e);
                MoveTo(e.As<MouseEvent>().clientX);
                _handle.focus();
                e.preventDefault();
            });

            InnerElement.addEventListener("pointermove", e =>
            {
                if (_dragging) MoveTo(e.As<MouseEvent>().clientX);
            });

            Action<Event> endDrag = _ =>
            {
                _dragging = false;
                InnerElement.classList.remove("tss-compare-dragging");
            };

            InnerElement.addEventListener("pointerup",     endDrag);
            InnerElement.addEventListener("pointercancel", endDrag);

            _handle.addEventListener("keydown", e =>
            {
                var ke   = e.As<KeyboardEvent>();
                var step = ke.shiftKey ? 10 : 2;

                switch (ke.key)
                {
                    case "ArrowLeft":
                    case "ArrowDown": Position = _position - step; break;
                    case "ArrowRight":
                    case "ArrowUp":   Position = _position + step; break;
                    case "Home":      Position = 0;   break;
                    case "End":       Position = 100; break;
                    default: return;
                }

                e.preventDefault();
            });

            SetPosition(position, raise: false);
        }

        /// <summary>Gets or sets where the divider is, in percent of the width (0 shows only "after", 100 only "before").</summary>
        public double Position
        {
            get => _position;
            set => SetPosition(value, raise: true);
        }

        /// <summary>Moves the divider, in percent of the width.</summary>
        public CompareSlider SetPosition(double percent) => SetPosition(percent, raise: true);

        /// <summary>Registers an action to run when the divider moves, with its new position in percent.</summary>
        public CompareSlider OnPositionChanged(Action<double> onChanged)
        {
            _positionChanged += onChanged;
            return this;
        }

        /// <summary>Labels the two sides, drawn in their top corners; they fade as the divider passes over them.</summary>
        public CompareSlider Labels(string before, string after)
        {
            _beforeLabel.textContent = before ?? "";
            _afterLabel.textContent  = after ?? "";
            return this;
        }

        private void MoveTo(double clientX)
        {
            var rect = InnerElement.getBoundingClientRect().As<DOMRect>();
            if (rect.width <= 0) return;
            Position = (clientX - rect.left) / rect.width * 100;
        }

        private CompareSlider SetPosition(double percent, bool raise)
        {
            percent = Math.Max(0, Math.Min(100, percent));
            if (percent == _position && InnerElement.style.getPropertyValue("--tss-compare-pos") != "") return this;

            _position = percent;
            InnerElement.style.setProperty("--tss-compare-pos", percent.ToString("0.##") + "%");
            _handle.setAttribute("aria-valuenow", percent.ToString("0"));
            _handle.setAttribute("aria-valuetext", percent.ToString("0") + "% before");
            _beforeLabel.UpdateClassIf(percent < 15, "tss-compare-label-hidden");
            _afterLabel.UpdateClassIf(percent > 85, "tss-compare-label-hidden");

            if (raise) _positionChanged?.Invoke(percent);
            return this;
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
