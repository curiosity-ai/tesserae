using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// One <see href="https://github.com/microsoft/flint-chart">Flint</see> chart drawn into a host element: the part
    /// the <see cref="FlintChart"/> component and the charts of <see cref="Markdown"/> share. It loads the library,
    /// applies the theme where the chart will be drawn, validates the spec, mounts it out of sight and swaps it in once
    /// drawn (so a redraw never flashes), lays it out again when the host's width changes, draws it again when a page
    /// theme change changes its theme, and reports clicks when asked to.
    /// </summary>
    /// <remarks>
    /// What happens when a chart cannot be drawn is the owner's: <see cref="OnFailed"/> says why, and whether a chart
    /// drawn before is still on screen. A drawing can move to another host, drawn or still drawing
    /// (<see cref="Rehost"/>): that is how Markdown keeps a chart across a re-render.
    /// </remarks>
    internal sealed class FlintChartDrawing
    {
        //Below this, a width reading is a transient layout pass rather than room to lay a chart out into.
        private const double MinWidth  = 120;
        private const double MinHeight = 80;

        //A resize re-lays the chart out once it has settled, and only by more than scrollbar jitter.
        private const int    RelayoutDelayMs     = 150;
        private const double RelayoutTolerancePx = 4;

        private const string HostAttribute = "data-tss-flint";

        private readonly string _frameClass;
        private          HTMLElement    _host;
        private          HTMLElement    _frame;
        private          HTMLElement    _pending;
        private          object         _surface;
        private          string         _themeJson;
        private          bool           _hostSurface;
        private          double         _laidOutWidth;
        private          double         _laidOutHeight;
        private          double         _chromeHeight;
        private          ResizeObserver _resizeObserver;
        private          int            _relayoutTimer;
        private          int            _version;
        private          bool           _drawing;
        private          bool           _destroyed;

        static FlintChartDrawing()
        {
            UI.Theme.OnThemeChanged += RedrawWhereTheThemeChanged;
        }

        /// <param name="host">The element the chart is drawn in. It is positioned relative, so a chart being drawn can sit out of sight over it.</param>
        /// <param name="frameClass">Classes the chart's frame carries besides <c>tss-flint-frame</c> (Markdown's plate is <c>tss-flint-chart</c>).</param>
        public FlintChartDrawing(HTMLElement host, string frameClass = null)
        {
            _frameClass = frameClass;
            Attach(host);
        }

        /// <summary>A fresh Flint chart spec (a parsed <c>ChartAssemblyInput</c>) for each draw; throws with the reason a spec cannot be made.</summary>
        public Func<object> Input { get; set; }

        /// <summary>The theme a chart is drawn in when its spec names none.</summary>
        public IFlintTheme Theme { get; set; } = FlintThemes.Tesserae;

        /// <summary>Whether <see cref="Theme"/> replaces a <c>theme_spec</c> the spec states.</summary>
        public bool OverrideSpecTheme { get; set; }

        /// <summary>Whether the chart is laid out for the host's height as well as its width, rather than choosing its own height.</summary>
        public bool FillHeight { get; set; }

        /// <summary>Called with every click Flint reports on a mark, a key entry or an axis label; null leaves clicks unreported.</summary>
        public Action<FlintChartClick> OnClick { get; set; }

        /// <summary>Called each time a chart is on screen: drawn, redrawn, or moved here drawn.</summary>
        public Action OnDrawn { get; set; }

        /// <summary>Called with the reason a draw failed, and whether a chart drawn before it is still on screen.</summary>
        public Action<string, bool> OnFailed { get; set; }

        public HTMLElement Host => _host;

        public bool IsDrawn => _surface != null;

        public bool IsDrawing => _drawing;

        public bool IsDestroyed => _destroyed;

        /// <summary>Draws the chart (again), replacing the one on screen once the new one is drawn.</summary>
        public async Task DrawAsync()
        {
            if (_destroyed) return;

            if (!Flint.IsLoaded)
            {
                _drawing = true;

                try
                {
                    await Flint.LoadAsync();
                }
                catch (Exception e)
                {
                    _drawing = false;
                    //Retried by the next chart to be drawn.
                    OnFailed?.Invoke("Flint could not be loaded: " + e.Message, IsDrawn);
                    return;
                }

                if (_destroyed) return;

                //Left the page while the library was on its way: drawn when it is back (Resume) or by whoever takes it over (Rehost).
                if (!_host.IsMounted())
                {
                    _drawing = false;
                    return;
                }
            }

            await MountAsync();
        }

        /// <summary>Draws the chart again when the theme it is drawn in reads differently now (a page theme change).</summary>
        public void RedrawIfThemeChanged()
        {
            if (_destroyed || _surface is null || !_host.IsMounted()) return;

            if (ThemeJson(_frame.firstElementChild.As<HTMLElement>()) != _themeJson) MountAsync().FireAndForget();
        }

        /// <summary>Moves the chart, drawn or still being drawn, into <paramref name="host"/>.</summary>
        public void Rehost(HTMLElement host)
        {
            if (host == _host) return;

            var watching = _resizeObserver != null;
            StopWatchingWidth();
            Detach();
            Attach(host);

            if (_frame != null)   host.appendChild(_frame);
            if (_pending != null) host.appendChild(_pending);

            //Observing reports the size once, so a host of another width relays the chart out from here.
            if (watching || _surface != null) WatchWidth();
            if (_surface != null) OnDrawn?.Invoke();
            else if (!_drawing && !_destroyed && host.IsMounted()) DrawAsync().FireAndForget();
            RedrawIfThemeChanged();
        }

        /// <summary>Stops following the host's width, and drops a redraw on its way; a first draw is let finish.</summary>
        public void Suspend()
        {
            StopWatchingWidth();
            if (_surface != null) _version++;
        }

        /// <summary>Follows the host's width again, and catches up with a theme change missed meanwhile.</summary>
        public void Resume()
        {
            if (_destroyed) return;

            if (_surface != null) WatchWidth();
            else if (!_drawing && _host.IsMounted())
            {
                DrawAsync().FireAndForget();
                return;
            }

            RedrawIfThemeChanged();
        }

        /// <summary>Tears the chart down: its view, its listeners and its frames.</summary>
        public void Destroy()
        {
            if (_destroyed) return;

            _destroyed = true;
            _version++;
            StopWatchingWidth();
            DestroySurface();
            _frame?.remove();
            _pending?.remove();
            _frame   = null;
            _pending = null;
            Detach();
        }

        private void Attach(HTMLElement host)
        {
            _host = host;
            _host.setAttribute(HostAttribute, "");
            Script.Write("{0}._tssFlintDrawing = {1};", _host, this);
        }

        private void Detach()
        {
            if (_host is null) return;
            _host.removeAttribute(HostAttribute);
            Script.Write("if ({0}._tssFlintDrawing === {1}) { {0}._tssFlintDrawing = null; }", _host, this);
        }

        //Which charts are on the page is read off the DOM, so there is no registry to keep in step with it.
        private static void RedrawWhereTheThemeChanged()
        {
            foreach (var found in document.querySelectorAll("[" + HostAttribute + "]"))
            {
                Script.Write<FlintChartDrawing>("{0}._tssFlintDrawing || null", found)?.RedrawIfThemeChanged();
            }
        }

        private async Task MountAsync(int fitAttempt = 0)
        {
            var version = ++_version;
            _drawing = true;

            try
            {
                //Made before anything is built: a spec still being streamed fails here, once per chunk.
                object input;
                try
                {
                    input = Input();
                }
                catch (Exception e)
                {
                    Fail(version, e.Message);
                    return;
                }

                //Drawn out of sight beside the chart it replaces, so a redraw never flashes or shifts the page.
                var frame = Div(Att("tss-flint-frame tss-flint-pending" + (FillHeight ? " tss-flint-fill" : "") + (string.IsNullOrEmpty(_frameClass) ? "" : " " + _frameClass)));
                var plot  = Div(Att("tss-flint-plot"));
                frame.appendChild(plot);
                _pending?.remove();
                _pending = frame;
                _host.appendChild(frame);

                string themeJson;
                bool   hostSurface;
                try
                {
                    themeJson   = ApplyTheme(input, plot, out hostSurface);
                    Flint.Validate(input);
                }
                catch (Exception e)
                {
                    DropPending(frame);
                    Fail(version, e.Message);
                    return;
                }

                var width  = AvailableWidth(frame);
                var height = FillHeight ? AvailableHeight(frame) : 0;

                //The height is only a ceiling to Flint, over a target of its own (320px by default): a chart that fills its
                //host targets the height it is given.
                if (height > 0) Script.Write("(function (c, h) { var b = c.baseSize || {}; c.baseSize = { width: b.width || 400, height: h }; })({0}.chart_spec, {1})", input, Requested(height));

                object surface;
                try
                {
                    surface = Flint.Mount(plot, input, width, Requested(height), reportClicks: OnClick != null);
                }
                catch (Exception e)
                {
                    DropPending(frame);
                    Fail(version, e.Message);
                    return;
                }

                try
                {
                    await Flint.WhenReady(surface);
                }
                catch (Exception e)
                {
                    Flint.Destroy(surface);
                    DropPending(frame);
                    Fail(version, e.Message);
                    return;
                }

                //A chart that cannot be laid out again in place for the corrected height is mounted again for it, once.
                if (height > 0 && version == _version && !await FitHeightAsync(surface, plot, width, height) && fitAttempt < 1 && version == _version)
                {
                    await MountAsync(fitAttempt + 1);
                }

                if (version != _version)
                {
                    //Superseded (redrawn, re-themed or torn down) while it was being drawn.
                    Flint.Destroy(surface);
                    DropPending(frame);
                    return;
                }

                DestroySurface();
                _frame?.remove();

                _pending      = null;
                _frame        = frame;
                _surface      = surface;
                _themeJson    = themeJson;
                _hostSurface  = hostSurface;
                _laidOutWidth  = width;
                _laidOutHeight = height;

                if (OnClick != null) Flint.OnInteraction(surface, detail => ReportClick(detail));

                frame.classList.remove("tss-flint-pending");
                frame.UpdateClassIf(_hostSurface, "tss-flint-host-surface");
                WatchWidth();
                OnDrawn?.Invoke();
            }
            finally
            {
                if (version == _version) _drawing = false;
            }
        }

        private void DropPending(HTMLElement frame)
        {
            frame.remove();
            if (_pending == frame) _pending = null;
        }

        private void Fail(int version, string reason)
        {
            if (version != _version) return;
            OnFailed?.Invoke(reason, IsDrawn);
        }

        private void ReportClick(object detail)
        {
            var click = FlintChartClick.From(detail);
            if (click != null) OnClick?.Invoke(click);
        }

        //The theme the chart is drawn in, read where it will be drawn. Answers the theme JSON it read.
        private string ApplyTheme(object input, HTMLElement plot, out bool hostSurface)
        {
            var themeJson = ThemeJson(plot);

            var specHasTheme = Script.Write<bool>("{0}.theme_spec !== undefined && {0}.theme_spec !== null", input);

            if (!specHasTheme || OverrideSpecTheme)
            {
                if (themeJson is null) Script.Write("delete {0}.theme_spec;", input);
                else Script.Write("{0}.theme_spec = JSON.parse({1});", input, themeJson);
            }

            //A theme whose surface is the host's: the frame draws it, and the chart is left unpainted over it.
            hostSurface = Script.Write<bool>("(function (t) { return !!(t && t.ink && t.ink.surface && t.ink.surface.source === 'host'); })({0}.theme_spec)", input);
            return themeJson;
        }

        //Asked of the element Flint draws into, so a theme that reads the page sees the frame the chart sits on.
        private string ThemeJson(HTMLElement plot)
        {
            var json = (Theme ?? FlintThemes.Tesserae).ThemeSpecJson(plot);
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

        //The content height of the host, for a chart that fills it; the pending frame is stretched over the host to read it.
        private static double AvailableHeight(HTMLElement frame)
        {
            var style  = window.getComputedStyle(frame);
            var insets = Script.Write<double>("(parseFloat({0}.paddingTop) || 0) + (parseFloat({0}.paddingBottom) || 0) + (parseFloat({0}.borderTopWidth) || 0) + (parseFloat({0}.borderBottomWidth) || 0)", style);
            var height = frame.getBoundingClientRect().As<DOMRect>().height - insets;
            return Math.Max(MinHeight, Math.Floor(height));
        }

        private void WatchWidth()
        {
            if (_resizeObserver != null || _destroyed) return;

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
            if (_destroyed || _surface is null || !_host.IsMounted()) return;

            var width  = AvailableWidth(_frame);
            var height = FillHeight ? AvailableHeight(_frame) : 0;
            if (Math.Abs(width - _laidOutWidth) <= RelayoutTolerancePx && Math.Abs(height - _laidOutHeight) <= RelayoutTolerancePx) return;

            //A taller host is a new target, which only a new mount sets.
            if (height > _laidOutHeight + RelayoutTolerancePx)
            {
                await MountAsync();
                return;
            }

            var surface = _surface;
            _laidOutWidth  = width;
            _laidOutHeight = height;

            if (!await Flint.Relayout(surface, width, Requested(height)) && surface == _surface && !_destroyed)
            {
                await MountAsync();
                return;
            }

            if (height > 0 && surface == _surface && !await FitHeightAsync(surface, _frame.firstElementChild.As<HTMLElement>(), width, height) && surface == _surface)
            {
                await MountAsync();
            }
        }

        //The height Flint is asked for, so that what it draws around that (titles, a key below) fits the height there is.
        private double Requested(double height) => height <= 0 ? 0 : Math.Max(MinHeight, height - _chromeHeight);

        //Flint lays its canvas out for the height it is given and draws its titles and keys around it, by an amount that
        //depends on the chart. That amount is measured on the chart as drawn, and the chart laid out once more without it.
        //Answers false when the chart has to be mounted again to fit. What Flint draws around its canvas does not grow
        //in step with it, so the measurement is repeated, a few times at most.
        private async Task<bool> FitHeightAsync(object surface, HTMLElement plot, double width, double height)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                var drawn = plot.getBoundingClientRect().As<DOMRect>().height;

                //Any overflow is corrected (it would be clipped or push the page); a chart a little short of the room is left.
                if (drawn <= height + 0.5 && height - drawn <= RelayoutTolerancePx) return true;

                _chromeHeight = Math.Max(0, _chromeHeight + drawn - height);
                if (!await Flint.Relayout(surface, width, Requested(height))) return false;
            }

            return true;
        }

        private void DestroySurface()
        {
            if (_surface is null) return;
            Flint.Destroy(_surface);
            _surface = null;
        }
    }
}
