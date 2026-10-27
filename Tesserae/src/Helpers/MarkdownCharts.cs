using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// How the <c>```flint</c> fences of rendered Markdown are drawn: in which theme, and whether that theme replaces
    /// one the fence states. <see cref="MarkdownBlock.Flint(IFlintTheme, bool)"/> and
    /// <see cref="Markdown.RenderMarkdownSanitized(string, MarkdownSanitization, IFlintTheme, bool)"/> make one.
    /// </summary>
    internal sealed class MarkdownChartOptions
    {
        internal MarkdownChartOptions(IFlintTheme theme, bool overrideFenceTheme)
        {
            Theme              = theme ?? FlintThemes.Tesserae;
            OverrideFenceTheme = overrideFenceTheme;
        }

        /// <summary>The theme a chart is drawn in when its fence names none.</summary>
        internal IFlintTheme Theme { get; }

        /// <summary>Whether <see cref="Theme"/> replaces a <c>theme_spec</c> the fence states, so every chart looks alike.</summary>
        internal bool OverrideFenceTheme { get; }
    }

    /// <summary>
    /// Draws the <c>```flint</c> (or <c>```flint-chart</c>, the name Flint's own documentation uses) fences of rendered
    /// Markdown as charts, with <see href="https://github.com/microsoft/flint-chart">Flint</see> (see <see cref="Flint"/>
    /// for how it is loaded). The fence body is a Flint chart spec: <c>data</c>, <c>semantic_types</c> and
    /// <c>chart_spec</c>, with optional <c>theme_spec</c> and <c>interaction_spec</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each fence becomes one <c>div.tss-flint</c> holding the code block and, once it is drawn, the chart's frame
    /// (<c>.tss-flint-chart</c>). The code block shows until the chart is drawn, and stays for good when it cannot be
    /// (a block still being streamed, a field that is not in the data), with the reason as its tooltip. A chart is laid
    /// out for the width it has, again when that width changes, and drawn again when a page theme change changes the
    /// theme it is drawn in.
    /// </para>
    /// <para>
    /// The element carries a <see cref="DeltaComponent.KeyAttribute"/> made from the fence, so a streamed reply that
    /// <see cref="DeltaComponent"/> patches chunk by chunk keeps the chart it already drew instead of patching it back
    /// into a code block: drawing is asynchronous, and the Markdown the patch comes from is never on the page.
    /// </para>
    /// </remarks>
    internal static class MarkdownCharts
    {
        private const string Selector = "pre > code.language-flint, pre > code.language-flint-chart";

        //Below this, a width reading is a transient layout pass rather than room to lay a chart out into.
        private const double MinWidth = 120;

        //A resize re-lays the chart out once it has settled, and only by more than scrollbar jitter.
        private const int    RelayoutDelayMs     = 150;
        private const double RelayoutTolerancePx = 4;

        //How long a drawn chart that left the page waits for a host of the same spec to take it over (see Park).
        private const int ParkedForMs = 10000;

        private static readonly Dictionary<string, FlintChartInMarkdown> _parked = new Dictionary<string, FlintChartInMarkdown>();

        static MarkdownCharts()
        {
            Theme.OnThemeChanged += RedrawWhereTheThemeChanged;
        }

        /// <summary>Turns every <c>flint</c> code block under <paramref name="root"/> into a chart.</summary>
        internal static void Render(HTMLElement root, MarkdownSanitization sanitization, MarkdownChartOptions options)
        {
            if (options is null) return;

            foreach (var found in root.querySelectorAll(Selector))
            {
                var code = found.As<HTMLElement>();
                var pre  = code.parentElement;
                var text = code.textContent ?? string.Empty;
                var host = Div(Att("tss-flint"));

                host.setAttribute(DeltaComponent.KeyAttribute, KeyOf(text, sanitization, options));
                pre.parentElement.insertBefore(host, pre);
                host.appendChild(pre);

                var chart = new FlintChartInMarkdown(host, pre, text, host.getAttribute(DeltaComponent.KeyAttribute), sanitization, options);
                Script.Write("{0}._tssFlint = {1};", host, chart);

                //A chart is laid out for the room it has and coloured from the tokens around it, so it waits for the page.
                DomObserver.WhenMounted(host, () => chart.DrawAsync().FireAndForget());
            }
        }

        /// <summary>
        /// Lets go of the charts under <paramref name="root"/>. Call before it is emptied. A drawn chart is kept for a
        /// moment rather than destroyed, so the same fence rendered again takes it over instead of drawing it anew.
        /// </summary>
        internal static void Dispose(HTMLElement root)
        {
            foreach (var found in root.querySelectorAll(".tss-flint"))
            {
                ChartOf(found.As<HTMLElement>())?.Park();
            }
        }

        private static FlintChartInMarkdown ChartOf(HTMLElement host)
            => Script.Write<FlintChartInMarkdown>("{0}._tssFlint || null", host);

        //Which charts are on the page is read off the DOM, so there is no registry to keep in step with it.
        private static void RedrawWhereTheThemeChanged()
        {
            foreach (var found in document.querySelectorAll(".tss-flint"))
            {
                ChartOf(found.As<HTMLElement>())?.RedrawIfThemeChanged();
            }
        }

        //Same spec, same options: the same chart. The theme is told apart by instance, which is why FlintThemes
        //hands out one instance per preset.
        private static string KeyOf(string source, MarkdownSanitization sanitization, MarkdownChartOptions options)
        {
            //Trimmed: a streamed fence reads with a trailing line break until its closing ``` arrives.
            source = source.Trim();

            var hash = Script.Write<string>("(function (s) { var h = 5381; for (var i = 0; i < s.length; i++) { h = ((h << 5) + h + s.charCodeAt(i)) | 0; } return (h >>> 0).toString(36); })({0})", source);
            return "flint:" + hash + ":" + source.Length + ":" + ThemeIds.Of(options.Theme) + ":" + (options.OverrideFenceTheme ? "o" : "f") + ":" + (int)sanitization;
        }

        //A stamp on the theme object rather than a list of them, so a page that makes themes as it goes holds none.
        private static class ThemeIds
        {
            private static int _last;

            public static int Of(IFlintTheme theme)
            {
                var id = Script.Write<int>("{0}.__tssFlintThemeId || 0", theme);
                if (id != 0) return id;

                id = ++_last;
                Script.Write("{0}.__tssFlintThemeId = {1};", theme, id);
                return id;
            }
        }

        private sealed class FlintChartInMarkdown
        {
            private readonly HTMLElement          _host;
            private readonly HTMLElement          _pre;
            private readonly string               _source;
            private readonly string               _key;
            private readonly MarkdownSanitization _sanitization;
            private readonly MarkdownChartOptions _options;
            private          HTMLElement          _frame;
            private          object               _surface;
            private          string               _themeJson;
            private          double               _laidOutWidth;
            private          ResizeObserver       _resizeObserver;
            private          int                  _relayoutTimer;
            private          int                  _version;
            private          bool                 _drawing;
            private          bool                 _hostSurface;
            private          int                  _parkedTimer;
            private          bool                 _watchingRemoval;
            private          bool                 _awaitingRemount;
            private          FlintChartInMarkdown _adoptedBy;
            private          bool                 _disposed;

            public FlintChartInMarkdown(HTMLElement host, HTMLElement pre, string source, string key, MarkdownSanitization sanitization, MarkdownChartOptions options)
            {
                _host         = host;
                _pre          = pre;
                _source       = source;
                _key          = key;
                _sanitization = sanitization;
                _options      = options;
            }

            public async Task DrawAsync()
            {
                //Mounted again after a move (a DeltaComponent swap, a re-parent): what was drawn is still there.
                if (_disposed || _drawing || _surface != null) return;

                if (!_watchingRemoval)
                {
                    _watchingRemoval = true;
                    DomObserver.WhenRemoved(_host, Park);
                }

                if (TryAdopt()) return;

                _drawing = true;

                try
                {
                    if (!Flint.IsLoaded)
                    {
                        try
                        {
                            await Flint.LoadAsync();
                        }
                        catch (Exception e)
                        {
                            //Retried by the next Markdown that has a chart; this one stays the code block it was.
                            Fail("Flint could not be loaded: " + e.Message);
                            return;
                        }

                        //The Markdown may have been re-rendered while the library was on its way. Nothing is drawn
                        //yet, so a host that took this chart on draws for itself.
                        if (_disposed) return;

                        if (!_host.IsMounted())
                        {
                            Destroy();
                            ReleaseAdopter();
                            return;
                        }
                    }

                    await MountAsync();
                }
                finally
                {
                    _drawing = false;
                }
            }

            public void RedrawIfThemeChanged()
            {
                if (_disposed || _surface is null || !_host.IsMounted()) return;

                if (ThemeJson(_frame.firstElementChild.As<HTMLElement>()) != _themeJson) MountAsync().FireAndForget();
            }

            /// <summary>
            /// The chart's element has left the page: re-rendered Markdown, or a <see cref="DeltaComponent"/> that
            /// swapped it for a fence it could not draw yet - a streamed closing <c>```</c> arrives a backtick at a
            /// time. The drawn chart waits, and the next host of the same spec to mount adopts it (<see cref="TryAdopt"/>),
            /// selection and all; one nobody takes is destroyed.
            /// </summary>
            public void Park()
            {
                if (_disposed) return;

                //Nothing drawn and nothing on its way: there is nothing to keep.
                if (_surface is null && !_drawing)
                {
                    Destroy();
                    return;
                }

                if (!IsParked)
                {
                    if (_parked.TryGetValue(_key, out var waiting)) waiting.Destroy();

                    _parked[_key] = this;
                    StopWatchingWidth();

                    //A redraw on its way is dropped; a first draw is let finish, for whoever adopts it.
                    if (_surface != null) _version++;

                    _parkedTimer = (int)window.setTimeout(_ =>
                    {
                        if (IsParked) Destroy();
                    }, ParkedForMs);
                }

                //Parked while still on the page (Markdown about to be re-rendered): its removal parks it again, and
                //only then is there anything to wait for.
                if (_host.IsMounted() || _awaitingRemount) return;

                //Put back where it was (a list that re-adds its rows): it is its own host again.
                _awaitingRemount = true;
                DomObserver.WhenMounted(_host, () =>
                {
                    _awaitingRemount = false;
                    if (!IsParked) return;

                    Unpark();
                    WatchWidth();
                    DomObserver.WhenRemoved(_host, Park);
                    RedrawIfThemeChanged();
                });
            }

            private bool IsParked => _parked.TryGetValue(_key, out var parked) && parked == this;

            private void Destroy()
            {
                Unpark();
                _disposed = true;
                _version++;
                StopWatchingWidth();
                DestroySurface();
            }

            private void Unpark()
            {
                window.clearTimeout(_parkedTimer);
                if (IsParked) _parked.Remove(_key);
            }

            //Takes over the chart a host of the same spec parked: the drawn frame moves here with the surface in it,
            //or, when it is still being drawn, it is handed over once it is.
            private bool TryAdopt()
            {
                if (!_parked.TryGetValue(_key, out var parked) || parked == this || parked._host.IsMounted()) return false;

                if (parked._surface is null)
                {
                    if (!parked._drawing) return false;

                    parked.Unpark();
                    parked._adoptedBy = this;
                    _drawing          = true;
                    return true;
                }

                parked.Unpark();

                var frame   = parked._frame;
                var surface = parked._surface;
                parked._frame    = null;
                parked._surface  = null;
                parked._disposed = true;

                TakeOver(frame, surface, parked._themeJson, parked._laidOutWidth, parked._hostSurface);
                return true;
            }

            private void TakeOver(HTMLElement frame, object surface, string themeJson, double laidOutWidth, bool hostSurface)
            {
                _drawing      = false;
                _frame        = frame;
                _surface      = surface;
                _themeJson    = themeJson;
                _laidOutWidth = laidOutWidth;
                _hostSurface  = hostSurface;

                _host.appendChild(frame);
                ShowDrawn();

                //Observing reports the size once, so a host of another width relays the chart out from here.
                WatchWidth();
                RedrawIfThemeChanged();
            }

            //A chart adopted while it was being drawn that could not be drawn: the adopter tries for itself.
            private void ReleaseAdopter()
            {
                var adopter = _adoptedBy;
                _adoptedBy = null;
                if (adopter is null || adopter._disposed) return;

                adopter._drawing = false;
                adopter.DrawAsync().FireAndForget();
            }

            private async Task MountAsync()
            {
                var version = ++_version;

                //Parsed before anything is built: a fence still being streamed fails here, once per chunk.
                object input;
                try
                {
                    input = Parse();
                }
                catch (Exception e)
                {
                    Fail(e.Message);
                    ReleaseAdopter();
                    return;
                }

                //Drawn out of sight beside the chart it replaces, so a redraw never flashes or shifts the page.
                var frame = Div(Att("tss-flint-chart tss-flint-pending"));
                var plot  = Div(Att("tss-flint-plot"));
                frame.appendChild(plot);
                _host.appendChild(frame);

                try
                {
                    ApplyTheme(input, plot);
                    Flint.Validate(input);
                }
                catch (Exception e)
                {
                    frame.remove();
                    Fail(e.Message);
                    ReleaseAdopter();
                    return;
                }

                var width = AvailableWidth(frame);

                object surface;
                try
                {
                    surface = Flint.Mount(plot, input, width);
                }
                catch (Exception e)
                {
                    frame.remove();
                    if (version == _version) Fail(e.Message);
                    ReleaseAdopter();
                    return;
                }

                try
                {
                    await Flint.WhenReady(surface);
                }
                catch (Exception e)
                {
                    Flint.Destroy(surface);
                    frame.remove();
                    if (version == _version) Fail(e.Message);
                    ReleaseAdopter();
                    return;
                }

                if (version != _version)
                {
                    //Superseded (re-themed or torn down) while it was being drawn.
                    Flint.Destroy(surface);
                    frame.remove();
                    ReleaseAdopter();
                    return;
                }

                if (_adoptedBy != null)
                {
                    //Its host left the page while it was drawn, and another host of the same spec took it on.
                    var adopter = _adoptedBy;
                    _adoptedBy = null;
                    _disposed  = true;

                    frame.classList.remove("tss-flint-pending");
                    frame.UpdateClassIf(_hostSurface, "tss-flint-host-surface");
                    adopter.TakeOver(frame, surface, _themeJson, width, _hostSurface);
                    return;
                }

                DestroySurface();
                _frame?.remove();

                _frame        = frame;
                _surface      = surface;
                _laidOutWidth = width;

                frame.classList.remove("tss-flint-pending");
                frame.UpdateClassIf(_hostSurface, "tss-flint-host-surface");
                ShowDrawn();
                WatchWidth();
            }

            private void ShowDrawn()
            {
                _host.classList.add("tss-flint-drawn");
                _pre.classList.remove("tss-flint-source");
                _pre.removeAttribute("title");
            }

            //The fence as Flint reads it: the spec, less what the sanitization does not allow.
            private object Parse()
            {
                object input;
                try
                {
                    input = Script.Write<object>("JSON.parse({0})", _source);
                }
                catch (Exception e)
                {
                    throw new Exception("The chart spec is not valid JSON: " + e.Message);
                }

                if (!Script.Write<bool>("!!{0} && typeof {0} === 'object' && !Array.isArray({0})", input))
                {
                    throw new Exception("The chart spec must be a JSON object with data, semantic_types and chart_spec.");
                }

                if (_sanitization == MarkdownSanitization.NoLinksOrEmbeddedContent)
                {
                    //Remote data is a fetch to a third party and a source link navigates to one, like the URLs the
                    //strict mode takes out of the prose.
                    if (Script.Write<bool>("!!({0}.data && {0}.data.url)", input))
                    {
                        throw new Exception("A chart in this Markdown cannot load its data from a URL.");
                    }

                    Script.Write("var s = {0}.chart_spec && {0}.chart_spec.source; if (s && typeof s === 'object') { {0}.chart_spec.source = s.text; }", input);
                }

                return input;
            }

            //The theme the chart is drawn in, read where it will be drawn.
            private void ApplyTheme(object input, HTMLElement plot)
            {
                _themeJson = ThemeJson(plot);

                var fenceHasTheme = Script.Write<bool>("{0}.theme_spec !== undefined && {0}.theme_spec !== null", input);

                if (!fenceHasTheme || _options.OverrideFenceTheme)
                {
                    if (_themeJson is null) Script.Write("delete {0}.theme_spec;", input);
                    else Script.Write("{0}.theme_spec = JSON.parse({1});", input, _themeJson);
                }

                //A theme whose surface is the host's: the frame draws it, and the chart is left unpainted over it.
                _hostSurface = Script.Write<bool>("(function (t) { return !!(t && t.ink && t.ink.surface && t.ink.surface.source === 'host'); })({0}.theme_spec)", input);
            }

            //Asked of the element Flint draws into, so a theme that reads the page sees the frame the chart sits on.
            private string ThemeJson(HTMLElement plot)
            {
                var json = _options.Theme.ThemeSpecJson(plot);
                return string.IsNullOrWhiteSpace(json) || json.Trim() == "null" ? null : json;
            }

            //The content width of the chart's frame, which is the width of the block it sits in less its own padding.
            private static double AvailableWidth(HTMLElement frame)
            {
                var style  = window.getComputedStyle(frame);
                var insets = Script.Write<double>("(parseFloat({0}.paddingLeft) || 0) + (parseFloat({0}.paddingRight) || 0) + (parseFloat({0}.borderLeftWidth) || 0) + (parseFloat({0}.borderRightWidth) || 0)", style);
                var width  = frame.getBoundingClientRect().As<DOMRect>().width - insets;
                return Math.Max(MinWidth, Math.Floor(width));
            }

            private void WatchWidth()
            {
                if (_resizeObserver != null) return;

                _resizeObserver = new ResizeObserver((entries, observer) =>
                {
                    window.clearTimeout(_relayoutTimer);
                    _relayoutTimer = (int)window.setTimeout(_ => RelayoutAsync().FireAndForget(), RelayoutDelayMs);
                });

                _resizeObserver.observe(_host);
            }

            private void StopWatchingWidth()
            {
                window.clearTimeout(_relayoutTimer);
                _resizeObserver?.disconnect();
                _resizeObserver = null;
            }

            //In place when only the layout changes, so a selection or zoom survives; mounted again when the new width
            //builds a different chart (more or fewer marks, axes, keys).
            private async Task RelayoutAsync()
            {
                if (_disposed || _surface is null || !_host.IsMounted()) return;

                var width = AvailableWidth(_frame);
                if (Math.Abs(width - _laidOutWidth) <= RelayoutTolerancePx) return;

                var surface = _surface;
                _laidOutWidth = width;

                if (!await Flint.Relayout(surface, width) && surface == _surface && !_disposed)
                {
                    await MountAsync();
                }
            }

            private void DestroySurface()
            {
                if (_surface is null) return;
                Flint.Destroy(_surface);
                _surface = null;
            }

            //A first draw that failed leaves the fence as the source it was, marked, with the reason as its tooltip.
            //A redraw that failed leaves the chart that was there.
            private void Fail(string reason)
            {
                if (_surface != null) return;

                _pre.classList.add("tss-flint-source");
                _pre.title = reason;
            }
        }
    }
}
