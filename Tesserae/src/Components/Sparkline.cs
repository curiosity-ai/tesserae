using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A tiny inline line chart used to show a trend without occupying chart-sized real-estate.
    /// </summary>
    [Transpose.Name("tss.Sparkline")]
    public class Sparkline : IComponent
    {
        private readonly HTMLElement _innerElement;
        private readonly double[]    _data;
        private readonly double      _width;
        private readonly double      _height;
        private readonly string      _color;

        private double  _strokeWidth     = 1;
        private bool    _fill            = true;
        private bool    _endMarker;
        private bool    _hasReference;
        private bool    _referenceIsMean;
        private double  _referenceValue;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Sparkline(double[] data, double width = 100, double height = 30, string color = "")
        {
            if (string.IsNullOrEmpty(color))
            {
                color = "var(--tss-primary-background-color)";
            }

            _data   = data;
            _width  = width;
            _height = height;
            _color  = color;

            _innerElement = Div(
                Att("tss-sparkline")
            );

            // The chart is drawn into a viewBox of exactly these dimensions, so that is the size the
            // component asks for. This used to say 100% / 100% and rely on the stack item wrapping it
            // to bound the percentage; with the component as the stack item itself, that percentage
            // resolves against the whole stack and the chart stretches across it. Callers that want it
            // to fill the row still say so — .WS() / .Width() / .Height() are written afterwards and win.
            _innerElement.style.width     = width + "px";
            _innerElement.style.height    = height + "px";
            _innerElement.style.minHeight = height + "px";

            Draw();
        }

        /// <summary>
        /// Sets the width of the line, in pixels. The line keeps that width when the chart is stretched
        /// to fill its row, rather than being scaled with the drawing.
        /// </summary>
        public Sparkline StrokeWidth(double width)
        {
            _strokeWidth = width;
            Draw();

            return this;
        }

        /// <summary>
        /// Turns the tinted area under the line on or off. On by default.
        /// </summary>
        public Sparkline Fill(bool value = true)
        {
            _fill = value;
            Draw();

            return this;
        }

        /// <summary>
        /// Marks the last point of the series with a dot in the line's colour - the "this week" end of a
        /// trend that runs up to now.
        /// </summary>
        public Sparkline EndMarker(bool value = true)
        {
            _endMarker = value;
            Draw();

            return this;
        }

        /// <summary>
        /// Draws a dotted horizontal line across the chart at the given value - a target, a threshold or a
        /// baseline the trend should be read against. A value outside the range of the series is clamped
        /// to the nearest edge of the chart.
        /// </summary>
        public Sparkline ReferenceLine(double value)
        {
            _hasReference    = true;
            _referenceIsMean = false;
            _referenceValue  = value;
            Draw();

            return this;
        }

        /// <summary>
        /// Draws a dotted horizontal line across the chart at the mean of the series.
        /// </summary>
        public Sparkline ReferenceLine()
        {
            _hasReference    = true;
            _referenceIsMean = true;
            Draw();

            return this;
        }

        private void Draw()
        {
            while (_innerElement.firstChild != null)
            {
                _innerElement.removeChild(_innerElement.firstChild);
            }

            var data = _data;

            if (data == null || data.Length == 0)
            {
                return;
            }
            if (data.Length == 1)
            {
                data = new double[] { data[0], data[0] };
            }

            var color  = _color;
            var width  = _width;
            var height = _height;

            double min = data.Min();
            double max = data.Max();
            double range = max - min;
            if (range == 0) range = 1;

            var svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            svg.setAttribute("viewBox", $"0 0 {width} {height}");
            svg.setAttribute("preserveAspectRatio", "none");
            svg.setAttribute("width", "100%");
            svg.setAttribute("height", "100%");

            // Create gradient
            var defs = document.createElementNS("http://www.w3.org/2000/svg", "defs");
            var gradientId = "gradient-" + Guid.NewGuid().ToString();
            var linearGradient = document.createElementNS("http://www.w3.org/2000/svg", "linearGradient");
            linearGradient.setAttribute("id", gradientId);
            linearGradient.setAttribute("x1", "0%");
            linearGradient.setAttribute("y1", "0%");
            linearGradient.setAttribute("x2", "0%");
            linearGradient.setAttribute("y2", "100%");

            var stop1 = document.createElementNS("http://www.w3.org/2000/svg", "stop");
            stop1.setAttribute("offset", "0%");
            stop1.setAttribute("stop-color", color);
            stop1.setAttribute("stop-opacity", "0.5");

            var stop2 = document.createElementNS("http://www.w3.org/2000/svg", "stop");
            stop2.setAttribute("offset", "100%");
            stop2.setAttribute("stop-color", color);
            stop2.setAttribute("stop-opacity", "0");

            linearGradient.appendChild(stop1);
            linearGradient.appendChild(stop2);
            defs.appendChild(linearGradient);
            svg.appendChild(defs);

            var points = "";
            for (int i = 0; i < data.Length; i++)
            {
                double x = (i / (double)(data.Length - 1)) * width;
                double y = height - (((data[i] - min) / range) * height);
                points += $"{x},{y} ";
            }

            // Add area under the line with gradient
            if (_fill)
            {
                var polygon = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
                polygon.setAttribute("fill", $"url(#{gradientId})");
                polygon.setAttribute("points", $"{width},{height} 0,{height} " + points);
                svg.appendChild(polygon);
            }

            if (_hasReference)
            {
                double reference = _referenceValue;

                if (_referenceIsMean)
                {
                    reference = data.Average();
                }

                double referenceY = height - ((Math.Max(min, Math.Min(max, reference)) - min) / range) * height;

                var line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute("x1", "0");
                line.setAttribute("x2", width.ToString());
                line.setAttribute("y1", referenceY.ToString());
                line.setAttribute("y2", referenceY.ToString());
                line.setAttribute("class", "tss-sparkline-reference");
                svg.appendChild(line);
            }

            var polyline = document.createElementNS("http://www.w3.org/2000/svg", "polyline");
            polyline.setAttribute("fill", "none");
            polyline.setAttribute("stroke", color);
            polyline.setAttribute("stroke-width", _strokeWidth.ToString());
            polyline.setAttribute("stroke-linejoin", "round");
            polyline.setAttribute("class", "tss-sparkline-line");
            polyline.setAttribute("points", points);

            svg.appendChild(polyline);
            _innerElement.appendChild(svg);

            if (_endMarker)
            {
                // An HTML dot over the chart rather than an SVG circle: the drawing is stretched to whatever
                // box it ends up in (preserveAspectRatio="none"), which would turn a circle into an ellipse.
                double lastY = height - (((data[data.Length - 1] - min) / range) * height);

                var dot = Div(Att("tss-sparkline-end"));
                dot.style.top        = (100 * lastY / height) + "%";
                dot.style.background = color;
                _innerElement.appendChild(dot);
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return _innerElement;
        }
    }
}
