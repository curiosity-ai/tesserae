using System;
using Transpose;

namespace Tesserae
{
    /// <summary>
    /// Vega-Lite <see href="https://vega.github.io/vega-lite/docs/bind.html#legend-binding">legend binding</see> for a
    /// <see cref="FlintChart"/> (<see cref="FlintChart.LegendBinding(FlintLegendBinding)"/>): clicking an entry of the
    /// chart's key chooses that series and fades the rest, shift-click adds or removes one, and a double click on the
    /// chart clears the choice. Every property is optional.
    /// </summary>
    /// <remarks>
    /// Only a key drawn with symbols can be bound (a colour, shape, dash or size key for categories); a gradient key for
    /// a measure, or a chart that labels its series directly instead of drawing a key, has nothing to bind, and the
    /// chart is drawn without the binding.
    /// </remarks>
    [Transpose.Name("tss.FlintLegendBinding")]
    public sealed class FlintLegendBinding
    {
        /// <summary>
        /// The key to bind when the chart draws more than one: <see cref="FlintChannel.Color"/>, <see cref="FlintChannel.Shape"/>,
        /// <see cref="FlintChannel.StrokeDash"/>, <see cref="FlintChannel.Size"/> or <see cref="FlintChannel.Opacity"/>. By default
        /// the first key drawn with symbols, colour first.
        /// </summary>
        public FlintChannel? Channel { get; set; }

        /// <summary>
        /// The <see href="https://vega.github.io/vega/docs/event-streams/">Vega event stream</see> on a key entry that
        /// chooses it, such as <c>"dblclick"</c> or <c>"mouseover"</c>; <c>"click"</c> by default.
        /// </summary>
        public string Events { get; set; }

        /// <summary>Also chooses a series by clicking one of its marks.</summary>
        public bool ClickMarks { get; set; }

        /// <summary>The opacity of the marks of the series not chosen, from 0 to 1 (0.2 by default).</summary>
        public double MutedOpacity { get; set; } = 0.2;

        /// <summary>The binding as the spec's <c>bind.legend</c>.</summary>
        internal object ToSpec()
        {
            if (MutedOpacity < 0 || MutedOpacity > 1) throw new ArgumentOutOfRangeException(nameof(MutedOpacity), "The opacity of the marks not chosen must be from 0 to 1.");

            var spec = Script.Write<object>("{ opacity: {0} }", MutedOpacity);

            if (Channel.HasValue)
            {
                switch (Channel.Value)
                {
                    case FlintChannel.Color:
                    case FlintChannel.Shape:
                    case FlintChannel.StrokeDash:
                    case FlintChannel.Size:
                    case FlintChannel.Opacity:
                        Script.Write("{0}.channel = {1};", spec, FlintNames.Of(Channel.Value));
                        break;
                    default:
                        throw new ArgumentException($"A key is drawn for color, shape, strokeDash, size or opacity, not {FlintNames.Of(Channel.Value)}.", nameof(Channel));
                }
            }

            if (!string.IsNullOrWhiteSpace(Events)) Script.Write("{0}.events = {1};", spec, Events);
            if (ClickMarks) Script.Write("{0}.marks = true;", spec);

            return spec;
        }
    }

    /// <summary>
    /// Vega-Lite <see href="https://vega.github.io/vega-lite/docs/bind.html#scale-binding">scale binding</see> for a
    /// <see cref="FlintChart"/> (<see cref="FlintChart.ScaleBinding(FlintScaleBinding)"/>): an interval selection bound to
    /// the chart's x and y scales, so dragging pans the chart and the mouse wheel zooms it; a double click puts it back.
    /// Small multiples share their scales, so every panel pans and zooms together. Every property is optional.
    /// </summary>
    /// <remarks>
    /// Only a continuous axis (numbers, dates) can be bound: a bar chart's categories stay put while its values pan and
    /// zoom, and a chart with neither (a pie) is drawn without the binding. A bound axis gets ticks for the range in
    /// view rather than the ones Flint planned for the whole range, in the format Flint chose.
    /// </remarks>
    [Transpose.Name("tss.FlintScaleBinding")]
    public sealed class FlintScaleBinding
    {
        /// <summary>Binds the x scale (on by default).</summary>
        public bool X { get; set; } = true;

        /// <summary>Binds the y scale (on by default).</summary>
        public bool Y { get; set; } = true;

        /// <summary>Whether the chart zooms (on by default).</summary>
        public bool Zoom { get; set; } = true;

        /// <summary>Whether the chart pans (on by default).</summary>
        public bool Pan { get; set; } = true;

        /// <summary>
        /// The <see href="https://vega.github.io/vega/docs/event-streams/">Vega event stream</see> that zooms, in place of
        /// Vega-Lite's <c>"wheel!"</c>, which takes every wheel turn over the chart from the page; e.g.
        /// <c>"wheel![event.shiftKey]"</c> zooms only with Shift held and leaves the page to scroll otherwise.
        /// </summary>
        public string ZoomEvents { get; set; }

        /// <summary>
        /// The <see href="https://vega.github.io/vega/docs/event-streams/">Vega event stream</see> that pans, in place of
        /// Vega-Lite's drag (<c>"[pointerdown, window:pointerup] > window:pointermove!"</c>).
        /// </summary>
        public string PanEvents { get; set; }

        /// <summary>The binding as the spec's <c>bind.scales</c>.</summary>
        internal object ToSpec()
        {
            if (!X && !Y) throw new ArgumentException("A scale binding binds x, y or both.");

            var encodings = X && Y ? new[] { "x", "y" } : new[] { X ? "x" : "y" };
            var spec      = Script.Write<object>("{ encodings: {0}.slice() }", encodings);

            if (!Zoom) Script.Write("{0}.zoom = false;", spec);
            else if (!string.IsNullOrWhiteSpace(ZoomEvents)) Script.Write("{0}.zoom = {1};", spec, ZoomEvents);

            if (!Pan) Script.Write("{0}.translate = false;", spec);
            else if (!string.IsNullOrWhiteSpace(PanEvents)) Script.Write("{0}.translate = {1};", spec, PanEvents);

            return spec;
        }
    }
}
