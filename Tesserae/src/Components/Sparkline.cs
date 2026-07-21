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

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Sparkline(double[] data, double width = 100, double height = 30, string color = "")
        {
            if (string.IsNullOrEmpty(color))
            {
                color = "var(--tss-primary-background-color)";
            }

            _innerElement = Div(
                Att("tss-sparkline")
            );

            _innerElement.style.width = "100%";
            _innerElement.style.height = "100%";
            _innerElement.style.minHeight = height + "px";

            if (data == null || data.Length == 0)
            {
                return;
            }
            if (data.Length == 1)
            {
                data = new double[] { data[0], data[0] };
            }

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

            var polyline = document.createElementNS("http://www.w3.org/2000/svg", "polyline");
            polyline.setAttribute("fill", "none");
            polyline.setAttribute("stroke", color);
            polyline.setAttribute("stroke-width", "1"); // Changed to 1
            polyline.setAttribute("points", points);

            // Add area under the line with gradient
            var polygon = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
            polygon.setAttribute("fill", $"url(#{gradientId})");
            polygon.setAttribute("points", $"{width},{height} 0,{height} " + points);

            svg.appendChild(polygon);
            svg.appendChild(polyline);
            _innerElement.appendChild(svg);
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
