using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A thin bar that measures a value against a target: the bar is filled up to the value, a tick marks
    /// where the target sits on the same scale, and a line under it names the state on the left and the
    /// figure on the right ("Watch ... 84%").
    /// </summary>
    /// <remarks>
    /// The scale runs from zero to <see cref="Max"/> (150 by default), so a value that overshoots its
    /// target - 231% of a run rate - still shows how far past it went, and the tick stays put at the
    /// target (100 by default). A bar with no target to speak of is drawn as an empty track by
    /// <see cref="NoValue"/>.
    /// </remarks>
    [Transpose.Name("tss.TargetBar")]
    public sealed class TargetBar : ComponentBase<TargetBar, HTMLElement>
    {
        private readonly HTMLElement _fill;
        private readonly HTMLElement _marker;
        private readonly HTMLElement _track;
        private readonly HTMLElement _label;
        private readonly HTMLElement _valueText;

        private double _value;
        private double _target = 100;
        private double _max    = 150;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="value">What was reached, on the same scale as <paramref name="target"/>.</param>
        /// <param name="target">Where the tick goes. 100 by default, for a value expressed as a percentage of its target.</param>
        /// <param name="max">The value at which the bar is full.</param>
        public TargetBar(double value, double target = 100, double max = 150)
        {
            _value  = value;
            _target = target;
            _max    = max;

            _fill   = Div(Att("tss-targetbar-fill"));
            _marker = Div(Att("tss-targetbar-marker"));
            _track  = Div(Att("tss-targetbar-track"), _fill, _marker);

            _label     = Span(Att("tss-targetbar-label"));
            _valueText = Span(Att("tss-targetbar-value"));

            InnerElement = Div(Att("tss-targetbar"), _track, Div(Att("tss-targetbar-caption"), _label, _valueText));

            Layout();
        }

        /// <summary>
        /// Sets what was reached.
        /// </summary>
        public TargetBar Value(double value)
        {
            _value = value;
            InnerElement.classList.remove("tss-targetbar-empty");

            return Layout();
        }

        /// <summary>
        /// Sets where the tick sits on the scale.
        /// </summary>
        public TargetBar Target(double target)
        {
            _target = target;

            return Layout();
        }

        /// <summary>
        /// Sets the value at which the bar is full.
        /// </summary>
        public TargetBar Max(double max)
        {
            _max = max;

            return Layout();
        }

        /// <summary>
        /// Sets the text at the left of the line under the bar - the state the value puts the metric in.
        /// </summary>
        public TargetBar Label(string text)
        {
            _label.innerText = text ?? string.Empty;

            return this;
        }

        /// <summary>
        /// Sets the text at the right of the line under the bar - the figure itself.
        /// </summary>
        public TargetBar ValueText(string text)
        {
            _valueText.innerText = text ?? string.Empty;

            return this;
        }

        /// <summary>
        /// Draws an empty track with no fill and no tick, for a figure that has no target to be measured against.
        /// </summary>
        public TargetBar NoValue(string label = null, string valueText = "—")
        {
            InnerElement.classList.add("tss-targetbar-empty");

            if (label != null) Label(label);
            if (valueText != null) ValueText(valueText);

            return this;
        }

        /// <summary>
        /// Colors the fill with any CSS color, or a theme variable.
        /// </summary>
        public TargetBar Color(string color)
        {
            InnerElement.style.setProperty("--tss-targetbar-color", color);

            return this;
        }

        /// <summary>Colors the fill with the theme's success color.</summary>
        public TargetBar Success() => Color("var(--tss-success-background-color)");

        /// <summary>Colors the fill with the theme's warning color.</summary>
        public TargetBar Warning() => Color("var(--tss-warning-background-color)");

        /// <summary>Colors the fill with the theme's danger color.</summary>
        public TargetBar Danger() => Color("var(--tss-danger-background-color)");

        /// <summary>Colors the fill with a muted neutral.</summary>
        public TargetBar Neutral() => Color("var(--tss-secondary-foreground-color)");

        /// <summary>
        /// Sets how thick the bar is. 4px by default.
        /// </summary>
        public TargetBar Thickness(UnitSize size)
        {
            InnerElement.style.setProperty("--tss-targetbar-thickness", size.ToString());

            return this;
        }

        private TargetBar Layout()
        {
            var max = _max <= 0 ? 1 : _max;

            _fill.style.width = (100 * Math.Max(0, Math.Min(_value, max)) / max) + "%";
            _marker.style.left = (100 * Math.Max(0, Math.Min(_target, max)) / max) + "%";

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
