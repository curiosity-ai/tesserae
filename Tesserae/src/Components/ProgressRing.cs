using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A circular progress indicator (ring/donut style).
    /// </summary>
    [H5.Name("tss.ProgressRing")]
    public sealed class ProgressRing : ComponentBase<ProgressRing, HTMLElement>
    {
        private readonly HTMLElement _svg;
        private readonly HTMLElement _track;
        private readonly HTMLElement _fill;
        private readonly HTMLElement _label;
        private readonly HTMLElement _container;

        private double _value;
        private double _max;
        private bool   _indeterminate;
        private int    _size;
        private int    _thickness;

        private const int DefaultSize      = 48;
        private const int DefaultThickness = 4;

        /// <summary>
        /// Initializes a new ProgressRing.
        /// </summary>
        /// <param name="size">Diameter in pixels.</param>
        /// <param name="thickness">Ring thickness in pixels.</param>
        public ProgressRing(int size = DefaultSize, int thickness = DefaultThickness)
        {
            _size      = size;
            _thickness = thickness;
            _max       = 100;
            _value     = 0;

            var radius      = (size / 2.0) - thickness;
            var cx          = size / 2.0;
            var cy          = size / 2.0;
            var circumference = 2 * Math.PI * radius;

            _track = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            _track.setAttribute("class",         "tss-progressring-track");
            _track.setAttribute("cx",            cx.ToString());
            _track.setAttribute("cy",            cy.ToString());
            _track.setAttribute("r",             radius.ToString());
            _track.setAttribute("fill",          "none");
            _track.setAttribute("stroke-width",  thickness.ToString());

            _fill = document.createElementNS("http://www.w3.org/2000/svg", "circle");
            _fill.setAttribute("class",            "tss-progressring-fill");
            _fill.setAttribute("cx",               cx.ToString());
            _fill.setAttribute("cy",               cy.ToString());
            _fill.setAttribute("r",                radius.ToString());
            _fill.setAttribute("fill",             "none");
            _fill.setAttribute("stroke-width",     thickness.ToString());
            _fill.setAttribute("stroke-dasharray", circumference.ToString());
            _fill.setAttribute("stroke-dashoffset", circumference.ToString());
            _fill.setAttribute("transform",        $"rotate(-90 {cx} {cy})");

            _svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            _svg.setAttribute("width",  size.ToString());
            _svg.setAttribute("height", size.ToString());
            _svg.setAttribute("viewBox", $"0 0 {size} {size}");
            _svg.appendChild(_track);
            _svg.appendChild(_fill);

            _label = Div(_("tss-progressring-label"));

            _container = Div(_("tss-progressring", role: "progressbar"));
            _container.setAttribute("aria-valuenow", "0");
            _container.setAttribute("aria-valuemin", "0");
            _container.setAttribute("aria-valuemax", _max.ToString());
            _container.style.width  = size + "px";
            _container.style.height = size + "px";
            _container.appendChild(_svg);
            _container.appendChild(_label);

            InnerElement = _container;

            UpdateFill(circumference);
        }

        /// <summary>Gets or sets the current progress value.</summary>
        public double Value
        {
            get => _value;
            set
            {
                _indeterminate = false;
                _value         = Math.Max(0, Math.Min(value, _max));
                _container.setAttribute("aria-valuenow", _value.ToString());
                _container.classList.remove("tss-progressring-indeterminate");

                var radius       = (_size / 2.0) - _thickness;
                var circumference = 2 * Math.PI * radius;
                UpdateFill(circumference);
            }
        }

        /// <summary>Gets or sets the maximum value (default 100).</summary>
        public double Max
        {
            get => _max;
            set
            {
                _max = Math.Max(1, value);
                _container.setAttribute("aria-valuemax", _max.ToString());
                Value = _value;
            }
        }

        /// <summary>Sets progress as percentage 0–100.</summary>
        public ProgressRing Progress(double current, double total)
        {
            Max   = total;
            Value = current;
            return this;
        }

        /// <summary>Shows an indeterminate spinning ring.</summary>
        public ProgressRing Indeterminate(bool value = true)
        {
            _indeterminate = value;
            _container.UpdateClassIf(_indeterminate, "tss-progressring-indeterminate");
            _container.removeAttribute("aria-valuenow");
            return this;
        }

        /// <summary>Sets the label text shown in the centre of the ring.</summary>
        public ProgressRing Label(string text)
        {
            _label.innerText = text ?? string.Empty;
            _container.setAttribute("aria-label", text ?? string.Empty);
            return this;
        }

        /// <summary>Hides the centre label.</summary>
        public ProgressRing NoLabel()
        {
            _label.innerText = string.Empty;
            return this;
        }

        public override HTMLElement Render() => InnerElement;

        private void UpdateFill(double circumference)
        {
            var pct    = _max > 0 ? _value / _max : 0;
            var offset = circumference * (1 - pct);
            _fill.setAttribute("stroke-dashoffset", offset.ToString());
        }
    }
}
