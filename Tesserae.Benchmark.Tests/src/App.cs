using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using H5;
using H5.Core;
using Tesserae;
using Tesserae.Tests;
using Tesserae.Tests.Samples;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Benchmark.Tests
{
    // Harness used by Tesserae.Benchmark.Measure to drive performance measurements.
    //
    // It exposes every sample defined in Tesserae.Tests as an isolated route
    // (#/sample/{Name}) and publishes a small JS API on `window.tssBenchmark`
    // that the Playwright driver uses to enumerate samples and trigger
    // lightweight in-page interactions (clicks, typing, toggles) without
    // pulling in any sidebar / sample-picker chrome that would otherwise
    // pollute the CPU and allocation profiles.
    internal static class App
    {
        private static Dictionary<string, Type> _samplesByName;
        private static HTMLElement _hostElement;
        private static HTMLElement _statusElement;
        private static string _currentSample;
        private static int _renderCount;
        private static int _exerciseCount;

        private static void Main()
        {
            document.body.style.margin   = "0";
            document.body.style.padding  = "0";
            document.body.style.overflow = "hidden";

            BuildShell();
            DiscoverSamples();
            RegisterRoutes();
            ExposeBenchmarkApi();

            Router.Initialize();
            Router.Refresh(onDone: Router.ForceMatchCurrent);

            SetStatus("ready");
        }

        private static void BuildShell()
        {
            _statusElement                  = document.createElement("div");
            _statusElement.id               = "tss-benchmark-status";
            _statusElement.style.position   = "fixed";
            _statusElement.style.top        = "0";
            _statusElement.style.left       = "0";
            _statusElement.style.right      = "0";
            _statusElement.style.height     = "20px";
            _statusElement.style.padding    = "2px 8px";
            _statusElement.style.background = "#111";
            _statusElement.style.color      = "#0f0";
            _statusElement.style.fontFamily = "ui-monospace, Consolas, monospace";
            _statusElement.style.fontSize   = "11px";
            _statusElement.style.lineHeight = "20px";
            _statusElement.style.zIndex     = "99999";
            _statusElement.textContent      = "initializing";
            document.body.appendChild(_statusElement);

            _hostElement                  = document.createElement("div");
            _hostElement.id               = "tss-benchmark-host";
            _hostElement.style.position   = "absolute";
            _hostElement.style.top        = "24px";
            _hostElement.style.left       = "0";
            _hostElement.style.right      = "0";
            _hostElement.style.bottom     = "0";
            _hostElement.style.overflow   = "auto";
            document.body.appendChild(_hostElement);
        }

        private static void DiscoverSamples()
        {
            var samples = typeof(ISample).Assembly.GetTypes()
                .Where(t => typeof(ISample).IsAssignableFrom(t)
                    && typeof(IComponent).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract)
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToArray();

            _samplesByName = samples.ToDictionary(t => t.Name, t => t);
            console.log("tss-benchmark: discovered " + _samplesByName.Count + " samples");
        }

        private static void RegisterRoutes()
        {
            Router.Register("benchmark-home", "/", _ => RenderHome());

            foreach (var name in _samplesByName.Keys)
            {
                var captured = name;
                Router.Register(
                    "benchmark-sample-" + captured,
                    "#/sample/" + captured,
                    _ => RenderSample(captured));
            }
        }

        private static void RenderHome()
        {
            ClearHost();
            _currentSample = null;

            var info = document.createElement("div");
            info.style.padding    = "16px";
            info.style.fontFamily = "system-ui, sans-serif";
            info.style.fontSize   = "14px";
            info.style.color      = "#444";

            var title = document.createElement("h2");
            title.textContent = "Tesserae Benchmark Harness";
            info.appendChild(title);

            var p = document.createElement("p");
            p.textContent = _samplesByName.Count + " samples discovered. Navigate via #/sample/{Name} or call window.tssBenchmark.render(name).";
            info.appendChild(p);

            _hostElement.appendChild(info);
            SetStatus("home");
        }

        private static void RenderSample(string name)
        {
            ClearHost();
            _currentSample = name;

            if (!_samplesByName.TryGetValue(name, out var type))
            {
                _hostElement.textContent = "Unknown sample: " + name;
                SetStatus("unknown:" + name);
                return;
            }

            try
            {
                var component = Activator.CreateInstance(type) as IComponent;

                if (component == null)
                {
                    _hostElement.textContent = "Sample " + name + " did not return an IComponent";
                    SetStatus("invalid:" + name);
                    return;
                }

                _hostElement.appendChild(component.Render());
                _renderCount++;
                SetStatus("rendered:" + name);
            }
            catch (Exception ex)
            {
                _hostElement.textContent = "Failed to render " + name + ": " + ex.Message;
                console.warn("tss-benchmark: render failed for " + name + " - " + ex.Message);
                SetStatus("error:" + name);
            }
        }

        private static void ClearHost()
        {
            while (_hostElement.firstChild != null)
            {
                _hostElement.removeChild(_hostElement.firstChild);
            }

            // Close any drifting overlays/toasts left behind by the previous sample so
            // they don't accumulate across the run and skew the memory profile.
            CloseLeftovers();
        }

        private static void CloseLeftovers()
        {
            string[] overlaySelectors = new[]
            {
                ".tss-toast",
                ".tss-modal-overlay",
                ".tss-modal",
                ".tss-dialog-overlay",
                ".tss-dialog",
                ".tss-panel-overlay",
                ".tss-panel",
                ".tss-context-menu",
                ".tss-tooltip",
                ".tss-tutorial-overlay",
                ".tss-command-palette",
                ".tippy-box"
            };

            for (int s = 0; s < overlaySelectors.Length; s++)
            {
                var found = document.querySelectorAll(overlaySelectors[s]);
                for (int i = 0; i < found.length; i++)
                {
                    try
                    {
                        var el = found[i].As<HTMLElement>();
                        if (el.parentNode != null) el.parentNode.removeChild(el);
                    }
                    catch { }
                }
            }
        }

        // Trigger a few low-risk interactions on whatever sample is currently
        // mounted: click a handful of visible buttons, fill the first text
        // inputs, toggle the first checkbox/switch, change the first dropdown.
        // Returns the number of distinct interactions that fired so the
        // measurement driver can verify the call did something.
        private static int Exercise()
        {
            if (_currentSample == null) return 0;

            int interactions = 0;

            interactions += ClickSafeButtons(maxClicks: 3);
            interactions += FillTextInputs("benchmark-" + _exerciseCount, maxFills: 2);
            interactions += ToggleCheckboxes(maxToggles: 1);
            interactions += ChangeSelects(maxChanges: 1);
            interactions += FocusBlurFirstInput();

            _exerciseCount++;
            SetStatus("exercised:" + _currentSample + ":" + interactions);
            return interactions;
        }

        private static int ClickSafeButtons(int maxClicks)
        {
            var nodes = _hostElement.querySelectorAll("button");
            int clicked = 0;

            for (int i = 0; i < nodes.length && clicked < maxClicks; i++)
            {
                var b = nodes[i].As<HTMLButtonElement>();

                if (b.disabled) continue;
                if (!IsVisible(b)) continue;

                var label = (b.textContent ?? string.Empty).ToLower();

                if (label.Contains("delete")
                 || label.Contains("remove")
                 || label.Contains("destroy")
                 || label.Contains("sign out")
                 || label.Contains("logout")) continue;

                try
                {
                    b.click();
                    clicked++;
                }
                catch { }
            }

            return clicked;
        }

        private static int FillTextInputs(string value, int maxFills)
        {
            var nodes = _hostElement.querySelectorAll("input, textarea");
            int filled = 0;

            for (int i = 0; i < nodes.length && filled < maxFills; i++)
            {
                var el = nodes[i];

                if (!IsVisible(el.As<HTMLElement>())) continue;

                var tagName = el.As<HTMLElement>().tagName;
                var type    = (el["type"] as string) ?? "";

                bool acceptable = tagName == "TEXTAREA"
                               || type == ""
                               || type == "text"
                               || type == "search"
                               || type == "email"
                               || type == "url";

                if (!acceptable) continue;

                try
                {
                    el["value"] = value;
                    DispatchEvent(el.As<Element>(), "input");
                    DispatchEvent(el.As<Element>(), "change");
                    filled++;
                }
                catch { }
            }

            return filled;
        }

        private static int ToggleCheckboxes(int maxToggles)
        {
            var nodes = _hostElement.querySelectorAll("input[type='checkbox'], [role='switch'], .tss-toggle, .tss-checkbox");
            int toggled = 0;

            for (int i = 0; i < nodes.length && toggled < maxToggles; i++)
            {
                var el = nodes[i].As<HTMLElement>();

                if (!IsVisible(el)) continue;

                try
                {
                    el.click();
                    toggled++;
                }
                catch { }
            }

            return toggled;
        }

        private static int ChangeSelects(int maxChanges)
        {
            var nodes = _hostElement.querySelectorAll("select");
            int changed = 0;

            for (int i = 0; i < nodes.length && changed < maxChanges; i++)
            {
                var select = nodes[i].As<HTMLSelectElement>();

                if (!IsVisible(select)) continue;
                if (select.options.length < 2) continue;

                try
                {
                    select.selectedIndex = (select.selectedIndex + 1) % (int)select.options.length;
                    DispatchEvent(select, "input");
                    DispatchEvent(select, "change");
                    changed++;
                }
                catch { }
            }

            return changed;
        }

        private static int FocusBlurFirstInput()
        {
            var nodes = _hostElement.querySelectorAll("input, textarea, [contenteditable='true']");

            for (int i = 0; i < nodes.length; i++)
            {
                var el = nodes[i].As<HTMLElement>();

                if (!IsVisible(el)) continue;

                try
                {
                    el.focus();
                    el.blur();
                    return 1;
                }
                catch { }
            }

            return 0;
        }

        private static bool IsVisible(HTMLElement el)
        {
            if (el == null) return false;
            if (el.offsetParent == null && el.tagName != "BODY") return false;

            var rect = el.getBoundingClientRect().As<DOMRect>();
            return rect.width > 0 && rect.height > 0;
        }

        private static void DispatchEvent(Element el, string type)
        {
            var evt = document.createEvent("Event");
            evt.initEvent(type, true, true);
            el.dispatchEvent(evt);
        }

        private static void SetStatus(string text)
        {
            _statusElement.textContent = "tss-benchmark: " + text + " (renders=" + _renderCount + ", exercises=" + _exerciseCount + ")";
            Script.Write("window.__tssBenchmarkStatus = {0};", text);
        }

        private static void ExposeBenchmarkApi()
        {
            // Pre-serialize the sample list once so the driver can enumerate it deterministically.
            var samplesJson = "[" + string.Join(",", _samplesByName.Keys.Select(k => "\"" + EscapeJson(k) + "\"")) + "]";

            Script.Write("if (!window.tssBenchmark) window.tssBenchmark = new Object();");
            Script.Write("window.tssBenchmark.ready = true;");
            Script.Write("window.tssBenchmark.samples = JSON.parse({0});", samplesJson);
            Script.Write("window.tssBenchmark.render = {0};",     (Action<string>)RenderSampleViaApi);
            Script.Write("window.tssBenchmark.exercise = {0};",   (Func<int>)Exercise);
            Script.Write("window.tssBenchmark.current = {0};",    (Func<string>)(() => _currentSample));
            Script.Write("window.tssBenchmark.renderCount = {0};",   (Func<int>)(() => _renderCount));
            Script.Write("window.tssBenchmark.exerciseCount = {0};", (Func<int>)(() => _exerciseCount));
        }

        private static void RenderSampleViaApi(string name)
        {
            // Update URL state for human-friendly debugging, but always force
            // a fresh render — Router.Push is a no-op when the hash is
            // unchanged, which would silently skip repeated renders of the
            // same sample (a common case for soak-style benchmark runs).
            var path = "#/sample/" + name;
            if (window.location.hash != path)
            {
                Router.Replace(path);
            }
            RenderSample(name);
        }

        private static string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
