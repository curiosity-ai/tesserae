using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// The one place that knows how <see href="https://wavesurfer.xyz">wavesurfer.js</see> is loaded and called, for <see cref="AudioPlayer"/>.
    /// It is not part of the bundle the page starts with: it ships as its own file (<c>tss-wavesurfer.min.js</c>, unlinked) and is fetched
    /// the first time a waveform is drawn.
    /// </summary>
    internal static class WaveSurfer
    {
        /// <summary>Where the compiler puts the unlinked resource of <c>tps.json</c>.</summary>
        internal const string ScriptUrl = "assets/js/tss-wavesurfer.min.js";

        private static Task _loading;

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

        /// <summary>Draws the waveform of <paramref name="media"/> into <paramref name="host"/>; the media element stays the one that plays.</summary>
        internal static object Create(HTMLElement host, HTMLMediaElement media, string wave, string progress, string cursor, int height)
            => Script.Write<object>(
                "globalThis.WaveSurfer.create({ container: {0}, media: {1}, waveColor: {2}, progressColor: {3}, cursorColor: {4}, height: {5}, barWidth: 3, barRadius: 3, barGap: 3, cursorWidth: 1 })",
                host, media, wave, progress, cursor, height);

        internal static void SetColors(object instance, string wave, string progress, string cursor)
            => Script.Write("{0}.setOptions({ waveColor: {1}, progressColor: {2}, cursorColor: {3} })", instance, wave, progress, cursor);

        internal static void OnError(object instance, Action<string> handler)
            => Script.Write("{0}.on('error', function (e) { {1}(String((e && e.message) || e)); })", instance, handler);

        internal static void Destroy(object instance)
            => Script.Write("{0}.destroy()", instance);

        /// <summary>
        /// A CSS colour (a token like <c>var(--tss-primary-background-color)</c> too, which a canvas cannot read) as <c>rgba(...)</c> with the
        /// given opacity, resolved where <paramref name="where"/> is, so a theme's colour is the one used. The waveform is drawn on a canvas, which
        /// takes literal colours only.
        /// </summary>
        internal static string Resolve(HTMLElement where, string css, double alpha)
            => Script.Write<string>(@"(function (host, c, a) {
                    var probe = document.createElement('span');
                    probe.style.color = c;
                    host.appendChild(probe);
                    var computed = getComputedStyle(probe).color;
                    host.removeChild(probe);
                    var cv = document.createElement('canvas'); cv.width = cv.height = 1;
                    var g = cv.getContext('2d', { willReadFrequently: true });
                    g.fillStyle = computed; g.fillRect(0, 0, 1, 1);
                    var d = g.getImageData(0, 0, 1, 1).data;
                    return 'rgba(' + d[0] + ',' + d[1] + ',' + d[2] + ',' + a + ')';
                })({0}, {1}, {2})", where, css, alpha);
    }
}
