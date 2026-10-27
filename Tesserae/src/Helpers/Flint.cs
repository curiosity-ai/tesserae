using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// The one place that knows how <see href="https://github.com/microsoft/flint-chart">Flint</see> is loaded and
    /// called, for the <see cref="FlintChart"/> component and the charts <see cref="Markdown"/> draws (both through
    /// <see cref="FlintChartDrawing"/>). Flint is not part of the bundle the page starts with: it
    /// ships as its own file, with the Vega-Lite renderer it draws through (Vega, Vega-Lite and vega-tooltip), and is
    /// fetched the first time a chart appears.
    /// </summary>
    /// <remarks>
    /// Vega-Lite is the only backend bundled. It is the one Flint's themes apply to (the ECharts and Chart.js
    /// assemblers ignore <c>theme_spec</c>) and the one its interactions run on, and Plotly, the other themed
    /// backend, weighs three times as much. <c>Build.Flint/</c> builds the file.
    /// </remarks>
    internal static class Flint
    {
        /// <summary>Where the compiler puts the unlinked <c>tss-flint.min.js</c> resource of <c>tps.json</c>.</summary>
        internal const string ScriptUrl = "assets/js/tss-flint.min.js";

        /// <summary>The id the bundle's <c>clickReport</c> interaction is mounted under, which tells its events from those of the spec's own interactions.</summary>
        internal const string ClickReportId = "tss-click";

        private static Task _loading;

        /// <summary>Whether the library is on the page, so a chart can be mounted without waiting.</summary>
        internal static bool IsLoaded { get; private set; }

        /// <summary>Loads the library once; a failed load is forgotten so the next call tries again.</summary>
        internal static async Task LoadAsync()
        {
            if (IsLoaded) return;

            if (_loading is null) _loading = Require.LoadScriptAsync(ScriptUrl);

            try
            {
                await _loading;
                IsLoaded = true;
            }
            catch
            {
                _loading = null;
                throw;
            }
        }

        /// <summary>
        /// Throws, with Flint's reason, when <paramref name="input"/> cannot make a chart: no data, a chart type Flint
        /// does not know, a channel the chart type does not have or lacks, a field that is not in the data. Flint
        /// would otherwise draw most of those, with an axis named <c>undefined</c>.
        /// </summary>
        internal static void Validate(object input)
            => Script.Write("globalThis.flint.validateChartInput({0}, 'vegalite');", input);

        /// <summary>
        /// Mounts the chart <paramref name="input"/> (a parsed <c>ChartAssemblyInput</c>) into <paramref name="host"/>,
        /// laid out for <paramref name="width"/> pixels. Throws when the spec cannot be compiled; the returned surface's
        /// <c>ready</c> promise rejects when it compiles but cannot be drawn.
        /// </summary>
        /// <param name="reportClicks">
        /// Mounts the bundle's <c>clickReport</c> interaction beside the spec's own: a click on a mark, a key entry or a
        /// category's axis label is reported through <see cref="OnInteraction"/> and changes nothing on the chart. A key
        /// entry or axis label one of the spec's interactions clicks is left to it, and reported by its events.
        /// </param>
        /// <param name="height">The height to lay the chart out for, or 0 to let the chart decide it.</param>
        internal static object Mount(HTMLElement host, object input, double width, double height = 0, bool reportClicks = false)
            => Script.Write<object>("globalThis.flint.mountChart({0}, {1}, { backend: 'vegalite', renderer: 'svg', availableSize: {3} > 0 ? { width: {2}, height: {3} } : { width: {2} }, interactions: {4} ? [globalThis.flint.clickReport({5}, {1})] : [] })", host, input, width, height, reportClicks, ClickReportId);

        /// <summary>Calls <paramref name="listener"/> with each gesture event the surface reports, for as long as it lives.</summary>
        internal static void OnInteraction(object surface, Action<object> listener)
            => Script.Write("{0}.onInteraction({1});", surface, listener);

        /// <summary>Completes once the chart is drawn, or throws with Flint's reason when it cannot be.</summary>
        internal static Task WhenReady(object surface)
        {
            var tcs = new TaskCompletionSource<bool>();
            Action          resolve = () => tcs.TrySetResult(true);
            Action<object>  reject  = error => tcs.TrySetException(new Exception(MessageOf(error)));
            Script.Write("{0}.ready.then({1}, {2});", surface, resolve, reject);
            return tcs.Task;
        }

        /// <summary>
        /// Lays a drawn chart out again for a new width, keeping what the reader did to it. Completes with
        /// <c>false</c> when the chart is built differently at that width, and has to be mounted again.
        /// </summary>
        internal static Task<bool> Relayout(object surface, double width, double height = 0)
        {
            var tcs = new TaskCompletionSource<bool>();
            Action<bool>   resolve = inPlace => tcs.TrySetResult(inPlace);
            Action<object> reject  = error => tcs.TrySetResult(false);
            Script.Write("{0}.relayout({2} > 0 ? { width: {1}, height: {2} } : { width: {1} }).then({3}, {4});", surface, width, height, resolve, reject);
            return tcs.Task;
        }

        /// <summary>Tears the chart down: its view, its listeners and the tooltip it put on the page.</summary>
        internal static void Destroy(object surface)
            => Script.Write("{0}.destroy();", surface);

        /// <summary>The message of something a script threw or a promise rejected with.</summary>
        internal static string MessageOf(object error)
            => Script.Write<string>("String({0} && {0}.message ? {0}.message : {0})", error);
    }
}
