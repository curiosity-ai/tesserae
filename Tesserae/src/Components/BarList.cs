using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A ranked list of horizontal bars: one row per item, with the label on the left, a thin rounded bar drawn
    /// against a grey track in the middle and the formatted value on the right - the "top paths by data transfer"
    /// card of an analytics dashboard. Bars are scaled to the largest value (or to <see cref="Max"/>), a label that
    /// does not fit is cut with an ellipsis and shows its full text in a tooltip, and the value columns line up
    /// across rows.
    /// </summary>
    [Transpose.Name("tss.BarList")]
    public sealed class BarList : ComponentBase<BarList, HTMLElement>
    {
        private sealed class Item
        {
            public string Label;
            public double Value;
            public string Color;
        }

        private readonly List<Item> _items = new List<Item>();

        private Func<double, string> _formatter = FormatCompact;
        private double               _max       = double.NaN;
        private string               _color     = "var(--tss-primary-background-color)";
        private string               _thickness = "6px";

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public BarList()
        {
            InnerElement = Div(Att("tss-barlist"));
            InnerElement.setAttribute("role", "list");
        }

        /// <summary>Adds a row. A null color uses the list's color (<see cref="Color"/>).</summary>
        public BarList Add(string label, double value, string color = null)
        {
            _items.Add(new Item { Label = label, Value = value, Color = color });
            Redraw();

            return this;
        }

        /// <summary>Replaces the rows with the given labels and values, in order. Extra labels or values are ignored.</summary>
        public BarList Items(string[] labels, double[] values)
        {
            _items.Clear();

            var count = Math.Min(labels?.Length ?? 0, values?.Length ?? 0);

            for (var i = 0; i < count; i++)
            {
                _items.Add(new Item { Label = labels[i], Value = values[i] });
            }

            Redraw();

            return this;
        }

        /// <summary>Removes every row.</summary>
        public BarList Clear()
        {
            _items.Clear();
            Redraw();

            return this;
        }

        /// <summary>
        /// Pins the value a full track stands for. By default it is the largest value in the list, so the top row
        /// always fills its track; pin it to compare against a quota, or to keep the scale when rows change.
        /// </summary>
        public BarList Max(double max)
        {
            _max = max;
            Redraw();

            return this;
        }

        /// <summary>Sets the color of the bars that have none of their own. Any CSS color or theme variable.</summary>
        public BarList Color(string color)
        {
            _color = string.IsNullOrEmpty(color) ? "var(--tss-primary-background-color)" : color;
            Redraw();

            return this;
        }

        /// <summary>Sets the thickness (height) of the bars. Defaults to <c>6px</c>.</summary>
        public BarList Thickness(UnitSize size)
        {
            _thickness = size.ToString();
            InnerElement.style.setProperty("--tss-barlist-thickness", _thickness);

            return this;
        }

        /// <summary>
        /// Sets how a value is written in the right-hand column and the row's tooltip. The default abbreviates
        /// large numbers (<c>69.28k</c>); pass your own for bytes, currency or durations.
        /// </summary>
        public BarList FormatValues(Func<double, string> formatter)
        {
            _formatter = formatter ?? FormatCompact;
            Redraw();

            return this;
        }

        /// <summary>
        /// Sets the width of the label column, so lists stacked in one card line their bars up. Defaults to
        /// half of the list.
        /// </summary>
        public BarList LabelWidth(UnitSize size)
        {
            InnerElement.style.setProperty("--tss-barlist-label-width", size.ToString());

            return this;
        }

        private void Redraw()
        {
            ClearChildren(InnerElement);

            var max = _max;

            if (double.IsNaN(max))
            {
                max = 0;

                foreach (var item in _items)
                {
                    if (!double.IsNaN(item.Value) && item.Value > max) max = item.Value;
                }
            }

            foreach (var item in _items)
            {
                var value = double.IsNaN(item.Value) ? 0 : Math.Max(0, item.Value);
                var share = max > 0 ? Math.Min(1, value / max) : 0;
                var text  = _formatter(item.Value);

                var label = Div(Att("tss-barlist-label"));
                label.textContent = item.Label ?? string.Empty;

                // The shared overflow tooltip finds a cut-short text by this class, and says the whole of it.
                label.classList.add("tss-textblock");

                var fill = Div(Att("tss-barlist-fill"));
                fill.style.width      = (share * 100).ToString("0.##") + "%";
                fill.style.background = item.Color ?? _color;

                var track = Div(Att("tss-barlist-track"), fill);

                var number = Div(Att("tss-barlist-value"));
                number.textContent = text;

                var row = Div(Att("tss-barlist-row"), label, track, number);
                row.setAttribute("role",       "listitem");
                row.setAttribute("aria-label", (item.Label ?? string.Empty) + ": " + text);

                InnerElement.appendChild(row);
            }
        }

        private static string FormatCompact(double value)
        {
            var abs = Math.Abs(value);

            if (abs >= 1e12) return (value / 1e12).ToString("0.##") + "T";
            if (abs >= 1e9)  return (value / 1e9).ToString("0.##")  + "G";
            if (abs >= 1e6)  return (value / 1e6).ToString("0.##")  + "M";
            if (abs >= 1e4)  return (value / 1e3).ToString("0.##")  + "k";

            return value.ToString("0.##");
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
