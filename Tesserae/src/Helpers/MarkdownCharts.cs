using System;
using System.Collections.Generic;
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
    /// (<c>.tss-flint-chart</c>), drawn by the same <see cref="FlintChartDrawing"/> the <see cref="FlintChart"/> component uses. The code block shows until the chart is drawn, and stays for good when it cannot be
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

        //How long a drawn chart that left the page waits for a host of the same spec to take it over (see Park).
        private const int ParkedForMs = 10000;

        private static readonly Dictionary<string, FlintChartInMarkdown> _parked = new Dictionary<string, FlintChartInMarkdown>();

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
                DomObserver.WhenMounted(host, chart.Draw);
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

        /// <summary>
        /// One fence's chart: the code block it replaces, and the <see cref="FlintChartDrawing"/> that draws it, which
        /// can pass to another fence of the same spec when the Markdown is rendered again.
        /// </summary>
        private sealed class FlintChartInMarkdown
        {
            private readonly HTMLElement          _host;
            private readonly HTMLElement          _pre;
            private readonly string               _source;
            private readonly string               _key;
            private readonly MarkdownSanitization _sanitization;
            private          FlintChartDrawing    _drawing;
            private          int                  _parkedTimer;
            private          bool                 _watchingRemoval;
            private          bool                 _awaitingRemount;
            private          bool                 _disposed;

            public FlintChartInMarkdown(HTMLElement host, HTMLElement pre, string source, string key, MarkdownSanitization sanitization, MarkdownChartOptions options)
            {
                _host         = host;
                _pre          = pre;
                _source       = source;
                _key          = key;
                _sanitization = sanitization;

                _drawing = new FlintChartDrawing(host, "tss-flint-chart")
                {
                    Theme             = options.Theme,
                    OverrideSpecTheme = options.OverrideFenceTheme,
                    Input             = Parse,
                };

                Own(_drawing);
            }

            public void Draw()
            {
                //Mounted again after a move (a DeltaComponent swap, a re-parent): what was drawn is still there.
                if (_disposed || _drawing is null || _drawing.IsDrawing || _drawing.IsDrawn) return;

                if (!_watchingRemoval)
                {
                    _watchingRemoval = true;
                    DomObserver.WhenRemoved(_host, Park);
                }

                if (TryAdopt()) return;

                _drawing.DrawAsync().FireAndForget();
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
                if (!_drawing.IsDrawn && !_drawing.IsDrawing)
                {
                    Destroy();
                    return;
                }

                if (!IsParked)
                {
                    if (_parked.TryGetValue(_key, out var waiting)) waiting.Destroy();

                    _parked[_key] = this;
                    _drawing.Suspend();

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
                    _drawing.Resume();
                    DomObserver.WhenRemoved(_host, Park);
                });
            }

            private bool IsParked => _parked.TryGetValue(_key, out var parked) && parked == this;

            private void Destroy()
            {
                Unpark();
                _disposed = true;
                _drawing?.Destroy();
                _drawing = null;
            }

            private void Unpark()
            {
                window.clearTimeout(_parkedTimer);
                if (IsParked) _parked.Remove(_key);
            }

            //Takes over the chart a host of the same spec parked, drawn or still being drawn: its frames move here,
            //and it reports to this fence from now on.
            private bool TryAdopt()
            {
                if (!_parked.TryGetValue(_key, out var parked) || parked == this || parked._host.IsMounted()) return false;
                if (!parked._drawing.IsDrawn && !parked._drawing.IsDrawing) return false;

                parked.Unpark();

                var drawing = parked._drawing;
                parked._drawing  = null;
                parked._disposed = true;

                _drawing.Destroy();
                _drawing = drawing;
                Own(drawing);
                drawing.Rehost(_host);
                return true;
            }

            private void Own(FlintChartDrawing drawing)
            {
                drawing.OnDrawn  = ShowDrawn;
                drawing.OnFailed = Fail;
            }

            private void ShowDrawn()
            {
                _host.classList.add("tss-flint-drawn");
                _pre.classList.remove("tss-flint-source");
                _pre.removeAttribute("title");
            }

            //A first draw that failed leaves the fence as the source it was, marked, with the reason as its tooltip.
            //A redraw that failed leaves the chart that was there.
            private void Fail(string reason, bool stillDrawn)
            {
                if (stillDrawn) return;

                _pre.classList.add("tss-flint-source");
                _pre.title = reason;
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
        }
    }
}
