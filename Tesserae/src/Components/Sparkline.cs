using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Sparkline")]
    public class Sparkline : IComponent
    {
        private readonly HTMLElement _innerElement;

        public Sparkline(double[] data, double width = 100, double height = 30, string color = "")
        {
            if (string.IsNullOrEmpty(color))
            {
                color = "var(--tss-primary-background-color)";
            }

            _innerElement = Div(
                _("tss-sparkline")
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
            polyline.setAttribute("stroke-width", "2");
            polyline.setAttribute("points", points);

            // Optional: Add area under the line
            var polygon = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
            polygon.setAttribute("fill", color);
            polygon.setAttribute("opacity", "0.2");
            polygon.setAttribute("points", $"{width},{height} 0,{height} " + points);

            svg.appendChild(polygon);
            svg.appendChild(polyline);
            _innerElement.appendChild(svg);
        }

        public HTMLElement Render()
        {
            return _innerElement;
        }
    }
}
