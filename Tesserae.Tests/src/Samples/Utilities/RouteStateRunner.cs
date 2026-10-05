using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transpose.Core;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    /// <summary>What a case produced next to what it should have produced, as text.</summary>
    internal sealed class RouteResult
    {
        public string Section  { get; }
        public string Id       { get; }
        public string Input    { get; }
        public string Call     { get; }
        public string Expected { get; }
        public string Actual   { get; }
        public bool   Passed   => Actual == Expected;

        public RouteResult(string section, string id, string input, string call, string expected, string actual)
        {
            Section  = section;
            Id       = id;
            Input    = input;
            Call     = call;
            Expected = expected;
            Actual   = actual;
        }
    }

    /// <summary>
    /// Runs the Route State sample's cases. <see cref="Evaluate"/> is the pure half: a <see cref="RouteLocation"/> built from a string.
    /// <see cref="RunAsync"/> is the half that touches the address bar: it writes the query of the sample's own route, adds and
    /// replaces history entries and navigates to two probe routes registered for the purpose.
    /// </summary>
    /// <remarks>
    /// History is measured by counting the browser's <c>pushState</c> and <c>replaceState</c> calls (the router announces both as window
    /// events) rather than by <c>history.length</c>, which stops growing once a browser's history is full. A route handler re-running is
    /// counted through <see cref="Router.OnNavigated"/>. The run leaves entries behind in the history, and puts the address bar back on the sample.
    /// </remarks>
    internal static class RouteStateRunner
    {
        private const string PROBE       = "#/route-state-probe/:id";
        private const string PROBE_ASYNC = "#/route-state-async/:id";

        private const string F = "F. Writes without a history entry";
        private const string G = "G. Writes with history";
        private const string H = "H. Router engine";

        private static readonly string Home = "#/view/" + Sample.FormatSampleName(typeof(RouteStateSample)).Replace(" ", "%20");

        private static bool                       _installed;
        private static int                        _replaced;
        private static int                        _pushed;
        private static int                        _navigated;
        private static int                        _notMatched;
        private static int                        _probeRuns;
        private static bool                       _asyncFinished;
        private static bool                       _asyncFinishedWhenNavigated;
        private static bool                       _refuse;
        private static Parameters                 _probeParams;
        private static Action<Parameters>         _onProbe;
        private static List<RouteResult>          _results;
        private static readonly Dictionary<string, bool> _isBackByPath = new Dictionary<string, bool>();

        /// <summary>Counters read before an action, so a case reports what that action did.</summary>
        private sealed class Mark
        {
            public int Replaced  { get; } = _replaced;
            public int Pushed    { get; } = _pushed;
            public int Navigated { get; } = _navigated;
        }

        public static RouteResult Evaluate(RouteCase routeCase)
        {
            var actual = Try(() => routeCase.Run(new RouteLocation(routeCase.Input)));

            return new RouteResult(routeCase.Section, routeCase.Id, routeCase.Input, routeCase.Call, routeCase.Expected, actual);
        }

        // a case that throws is a failing row with the message, not a page that stops listing
        private static string Try(Func<string> act)
        {
            try
            {
                return act();
            }
            catch (Exception e)
            {
                return "threw: " + e.Message;
            }
        }

        public static async Task<List<RouteResult>> RunAsync()
        {
            Install();

            _results = new List<RouteResult>();

            Router.OnBeforeNavigate(RecordingGuard);

            try
            {
                RunWrites();
                RunHistory();

                await RunHistoryBackAsync();
                await RunRouterAsync();
            }
            finally
            {
                _refuse    = false;
                _onProbe   = null;

                Router.OnTransformRoutes(url => url);
                Router.OnBeforeNavigate(App.GalleryNavigationGuard);
                Router.Replace(Home);
            }

            return _results;
        }

        // ------------------------------------------------------------------------------------------------
        // F: writes that replace the current entry

        private static void RunWrites()
        {
            Check(F, "F1", "?preview=abc", "Set(\"k\", \"v\")", "preview=abc&k=v | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?preview=abc");

                var mark = new Mark();
                RouteQuery.Set("k", "v");

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F2", "?preview=abc&k=v", "Set(\"k\", \"v\") again", "preview=abc&k=v | replaced 0, pushed 0, handlers 0", () =>
            {
                Arrive("?preview=abc&k=v");

                var mark = new Mark();
                RouteQuery.Set("k", "v");

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F3", "?preview=abc&k=v", "Clear(\"k\")", "preview=abc | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?preview=abc&k=v");

                var mark = new Mark();
                RouteQuery.Clear("k");

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F3", "?preview=abc", "Clear(\"absent\")", "preview=abc | replaced 0, pushed 0, handlers 0", () =>
            {
                Arrive("?preview=abc");

                var mark = new Mark();
                RouteQuery.Clear("absent");

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F4", "(no query)", "Set(\"k\", \"C++\"); Set(\"k2\", \"a&b=c\"); Get both", "k=C%2B%2B&k2=a%26b%3Dc | C++ | a&b=c", () =>
            {
                Arrive("");

                RouteQuery.Set("k", "C++");
                RouteQuery.Set("k2", "a&b=c");

                return Query() + " | " + RouteQuery.Get("k") + " | " + RouteQuery.Get("k2");
            });

            Check(F, "F5", "?preview=abc&k=v", "Update(p => p.With(\"a\", \"1\").Remove(\"k\"))", "preview=abc&a=1 | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?preview=abc&k=v");

                var mark = new Mark();
                RouteQuery.Update(p => p.With("a", "1").Remove("k"));

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F6", "?preview=abc&a=1", "Update(p => p.With(\"a\", \"1\")), no net change", "preview=abc&a=1 | replaced 0, pushed 0, handlers 0", () =>
            {
                Arrive("?preview=abc&a=1");

                var mark = new Mark();
                RouteQuery.Update(p => p.With("a", "1"));

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F7", "?toast=i%3Bhi", "Consume(\"toast\", out v), then again", "true \"i;hi\" | (none) | replaced 1; then false, replaced 0", () =>
            {
                Arrive("?toast=i%3Bhi");

                var first      = new Mark();
                var wasThere   = RouteQuery.Consume("toast", out var value);
                var firstText  = "replaced " + (_replaced - first.Replaced);
                var afterFirst = Query();

                var second     = new Mark();
                var again      = RouteQuery.Consume("toast", out _);

                return B(wasThere) + " \"" + value + "\" | " + afterFirst + " | " + firstText + "; then " + B(again) + ", replaced " + (_replaced - second.Replaced);
            });

            Check(F, "F8", "?knwnerr=x&msg=hello&keep=1", "Consume(\"knwnerr\", \"msg\")", "true | keep=1 | replaced 1", () =>
            {
                Arrive("?knwnerr=x&msg=hello&keep=1");

                var mark = new Mark();
                var any  = RouteQuery.Consume("knwnerr", "msg");

                return B(any) + " | " + Query() + " | replaced " + (_replaced - mark.Replaced);
            });

            Check(F, "F8", "?msg=hello&keep=1", "Consume(\"knwnerr\", \"msg\"), only msg there", "true | keep=1 | replaced 1", () =>
            {
                Arrive("?msg=hello&keep=1");

                var mark = new Mark();
                var any  = RouteQuery.Consume("knwnerr", "msg");

                return B(any) + " | " + Query() + " | replaced " + (_replaced - mark.Replaced);
            });

            Check(F, "F8", "?keep=1", "Consume(\"knwnerr\", \"msg\"), neither there", "false | keep=1 | replaced 0", () =>
            {
                Arrive("?keep=1");

                var mark = new Mark();
                var any  = RouteQuery.Consume("knwnerr", "msg");

                return B(any) + " | " + Query() + " | replaced " + (_replaced - mark.Replaced);
            });

            Check(F, "F9", "?su=search&query=q", "Set(\"other\", \"1\"): keys that belong to someone else", "su=search&query=q&other=1 | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?su=search&query=q");

                var mark = new Mark();
                RouteQuery.Set("other", "1");

                return Query() + " | " + Effects(mark);
            });

            Check(F, "F10", "(no query)", "Set(\"a\", \"1\"); Set(\"b\", \"2\")", "a=1&b=2", () =>
            {
                Arrive("");

                RouteQuery.Set("a", "1");
                RouteQuery.Set("b", "2");

                return Query();
            });
        }

        // ------------------------------------------------------------------------------------------------
        // G: writes that add an entry, and the ones that only sometimes do

        private static void RunHistory()
        {
            Check(G, "G1", "(no query)", "SetWithHistory(\"show\", \"a\"): arriving", "show=a | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("");

                var mark = new Mark();
                RouteQuery.SetWithHistory("show", "a");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G2", "?show=a", "SetWithHistory(\"show\", \"b\"): a change", "show=b | replaced 0, pushed 1, handlers 0", () =>
            {
                Arrive("?show=a");

                var mark = new Mark();
                RouteQuery.SetWithHistory("show", "b");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G3", "?show=b", "SetWithHistory(\"show\", \"b\"): the same value", "show=b | replaced 0, pushed 0, handlers 0", () =>
            {
                Arrive("?show=b");

                var mark = new Mark();
                RouteQuery.SetWithHistory("show", "b");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G4", "?show=", "SetWithHistory(\"show\", \"a\"): empty value", "show=a | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?show=");

                var mark = new Mark();
                RouteQuery.SetWithHistory("show", "a");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G5", "?show=%20", "SetWithHistory(\"show\", \"a\"): whitespace value", "show=a | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?show=%20");

                var mark = new Mark();
                RouteQuery.SetWithHistory("show", "a");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G6", "?preview=abc", "SetWithHistory(\"show\", \"a\") then (\"show\", \"b\")", "preview=abc&show=b | replaced 1, pushed 1, handlers 0", () =>
            {
                Arrive("?preview=abc");

                var mark = new Mark();
                RouteQuery.SetWithHistory("show", "a");
                RouteQuery.SetWithHistory("show", "b");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G7", "(no query)", "UpdateWithHistory(.., \"timeFrame\", \"period\"): neither there", "timeFrame=7&period=day | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("");

                var mark = new Mark();
                RouteQuery.UpdateWithHistory(p => p.With("timeFrame", "7").With("period", "day"), "timeFrame", "period");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G7", "?timeFrame=7", "UpdateWithHistory(.., \"timeFrame\", \"period\"): one there", "timeFrame=7&period=day | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("?timeFrame=7");

                var mark = new Mark();
                RouteQuery.UpdateWithHistory(p => p.With("period", "day"), "timeFrame", "period");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G7", "?timeFrame=7&period=day", "UpdateWithHistory(.., \"timeFrame\", \"period\"): both there, two keys change, one entry", "timeFrame=30&period=week | replaced 0, pushed 1, handlers 0", () =>
            {
                Arrive("?timeFrame=7&period=day");

                var mark = new Mark();
                RouteQuery.UpdateWithHistory(p => p.With("timeFrame", "30").With("period", "week"), "timeFrame", "period");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G8", "?show=a&tab=x", "UpdateWithHistory(p => p.With(\"show\", \"b\").Remove(\"tab\"), \"show\")", "show=b | replaced 0, pushed 1, handlers 0", () =>
            {
                Arrive("?show=a&tab=x");

                var mark = new Mark();
                RouteQuery.UpdateWithHistory(p => p.With("show", "b").Remove("tab"), "show");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G8", "(no query)", "UpdateWithHistory(p => p.With(\"show\", \"a\").With(\"tab\", \"x\"), \"show\"): arriving", "show=a&tab=x | replaced 1, pushed 0, handlers 0", () =>
            {
                Arrive("");

                var mark = new Mark();
                RouteQuery.UpdateWithHistory(p => p.With("show", "a").With("tab", "x"), "show");

                return Query() + " | " + Effects(mark);
            });

            Check(G, "G10", "#/home?preview=a,b&x=1", "ReplacePath(\"#/search\")", "#/search?preview=a%2Cb&x=1 | replaced 1, pushed 0, handlers 0", () =>
            {
                Router.Replace("#/home?preview=a,b&x=1");

                var mark = new Mark();
                RoutePath.ReplacePath("#/search");

                return Hash() + " | " + Effects(mark);
            });

            Check(G, "G11", "#/", "ReplacePath(\"#/search\") from a path with no segments", "#/search | replaced 1, pushed 0, handlers 0", () =>
            {
                Router.Replace("#/");

                var mark = new Mark();
                RoutePath.ReplacePath("#/search");

                return Hash() + " | " + Effects(mark);
            });

            Check(G, "G12", "?a=1", "ReplacePath to the path it is already on", "true; a=1 | replaced 0, pushed 0, handlers 0", () =>
            {
                Arrive("?a=1");

                var mark = new Mark();
                var ok   = RoutePath.ReplacePath(Home);

                return B(ok) + "; " + Query() + " | " + Effects(mark);
            });

            Check(G, "G13", "?a=1", "ReplacePath(\"#/search\") while a guard refuses", "false; " + Home + "?a=1 | replaced 0, pushed 0, handlers 0", () =>
            {
                Arrive("?a=1");

                var mark = new Mark();

                _refuse = true;
                var ok  = RoutePath.ReplacePath("#/search");
                _refuse = false;

                return B(ok) + "; " + Hash() + " | " + Effects(mark);
            });
        }

        private static async Task RunHistoryBackAsync()
        {
            await CheckAsync(G, "G14", "(no query)", "SetWithHistory(\"show\", \"a\"), then \"b\", then the browser's Back", "show=a; handlers 1", async () =>
            {
                Arrive("");

                RouteQuery.SetWithHistory("show", "a");
                RouteQuery.SetWithHistory("show", "b");

                var mark = new Mark();
                window.history.back();

                await WaitFor(() => RouteQuery.Get("show") == "a" && _navigated > mark.Navigated);

                return "show=" + RouteQuery.Get("show") + "; handlers " + (_navigated - mark.Navigated);
            });
        }

        // ------------------------------------------------------------------------------------------------
        // H: the router itself

        private static async Task RunRouterAsync()
        {
            Check(H, "H1", Home + "?a=1&k=v&z=1", "Router.Replace to the same path with another query", "isBack false", () =>
            {
                Arrive("?a=1");
                RouteQuery.Set("k", "v");

                _isBackByPath.Remove(Home.TrimStart('#'));

                Router.Replace(Home + "?a=1&k=v&z=1");

                return "isBack " + (_isBackByPath.TryGetValue(Home.TrimStart('#'), out var isBack) ? B(isBack) : "(guard not asked)");
            });

            await CheckAsync(H, "H2", Probe("one") + ", then " + Probe("two"), "Navigate to each, then the browser's Back", "forward isBack false; back isBack true", async () =>
            {
                Arrive("");

                await NavigateAndWait(Probe("one"));
                await NavigateAndWait(Probe("two"));

                var forward = _isBackByPath.TryGetValue("/route-state-probe/two", out var f) && f;
                var mark    = new Mark();

                window.history.back();

                await WaitFor(() => _navigated > mark.Navigated);

                var back = _isBackByPath.TryGetValue("/route-state-probe/one", out var b) && b;

                return "forward isBack " + B(forward) + "; back isBack " + B(back);
            });

            await CheckAsync(H, "H3", Probe("abc") + "?x=1", "the handler removes \"x\" from its parameters; GetQueryParameters(); Set(\"y\", \"2\")", "x kept: true | x=1&y=2", async () =>
            {
                Arrive("");

                _onProbe = p => p.Remove("x");

                await NavigateAndWait(Probe("abc") + "?x=1");

                _onProbe = null;

                var kept = Router.GetQueryParameters().ContainsKey("x");

                RouteQuery.Set("y", "2");

                return "x kept: " + B(kept) + " | " + Query();
            });

            Check(H, "H4", "(after L3)", "remove \"x\" from, and add \"z\" to, the parameters GetQueryParameters() returned", "x kept: true, z added: false | x=1&y=2", () =>
            {
                var copy = Router.GetQueryParameters();

                copy.Remove("x");
                copy.With("z", "9");

                var again = Router.GetQueryParameters();

                return "x kept: " + B(again.ContainsKey("x")) + ", z added: " + B(again.ContainsKey("z")) + " | " + Query();
            });

            await CheckAsync(H, "H5", Probe("abc") + "?x=1", "GetQueryParameters() on a route with :id; then after Router.Replace to the same path", "id=abc; after Replace has id: false", async () =>
            {
                Arrive("");

                await NavigateAndWait(Probe("abc") + "?x=1");

                var onRoute = Router.GetQueryParameters().TryGetValue("id", out var id) ? id : "(missing)";

                Router.Replace(Probe("abc") + "?x=1&r=1");

                var afterReplace = Router.GetQueryParameters().ContainsKey("id");

                return "id=" + onRoute + "; after Replace has id: " + B(afterReplace);
            });

            await CheckAsync(F, "F11", Probe("abc"), "Set(\"k\", \"v\") on a route with :id", Probe("abc") + "?k=v", async () =>
            {
                Arrive("");

                await NavigateAndWait(Probe("abc"));

                RouteQuery.Set("k", "v");

                return Hash();
            });

            await CheckAsync(H, "H6", Probe("abc") + "?id=zzz", "a query key with the name of a :variable; then Set(\"k\", \"v\")", "id=zzz; handler got zzz | id=zzz&k=v", async () =>
            {
                Arrive("");

                await NavigateAndWait(Probe("abc") + "?id=zzz");

                var seen    = Router.GetQueryParameters()["id"];
                var handler = _probeParams["id"];

                RouteQuery.Set("k", "v");

                return "id=" + seen + "; handler got " + handler + " | " + Query();
            });

            Check(H, "H7", "(registered routes)", "Router.Exists(\"#/route-state-probe/:id\"), Exists(\"/route-state-probe/:id\"), Exists(\"#/route-state-nothing\")", "true, true, false", () =>
            {
                return B(Router.Exists("#/route-state-probe/:id")) + ", " + B(Router.Exists("/route-state-probe/:id")) + ", " + B(Router.Exists("#/route-state-nothing"));
            });

            await CheckAsync(H, "H8", Probe("refuse"), "Navigate to a route whose handler returns false", "handler ran: true; hash restored: true; handlers +0", async () =>
            {
                Arrive("");

                await NavigateAndWait(Probe("one"));

                var hashBefore = Hash();
                var mark       = new Mark();
                var runs       = _probeRuns;

                Router.Navigate(Probe("refuse"));

                var ran      = await WaitFor(() => _probeRuns > runs);
                var restored = await WaitFor(() => Hash() == hashBefore);

                return "handler ran: " + B(ran) + "; hash restored: " + B(restored) + "; handlers +" + (_navigated - mark.Navigated);
            });

            await CheckAsync(H, "H9", Probe("one"), "Navigate to the address it is on, then the same with reload: true", "no-op: replaced 0, pushed 0, handlers 0; reload: replaced 0, pushed 0, handlers 1", async () =>
            {
                Arrive("");

                await NavigateAndWait(Probe("one"));

                var plain = new Mark();
                Router.Navigate(Hash());
                var plainText = Effects(plain);

                var forced = new Mark();
                Router.Navigate(Hash(), reload: true);

                return "no-op: " + plainText + "; reload: " + Effects(forced);
            });

            await CheckAsync(H, "H10", "#/ROUTE-STATE-PROBE/OnE", "a route in other letters than it was registered with", "matched: true; id=OnE", async () =>
            {
                Arrive("");

                var matched = await NavigateAndWait("#/ROUTE-STATE-PROBE/OnE");

                return "matched: " + B(matched) + "; id=" + (_probeParams is object && _probeParams.TryGetValue("id", out var id) ? id : "(none)");
            });

            await CheckAsync(H, "H11", "#/route-state-async/one", "an async handler: has it finished when OnNavigated fires?", "at OnNavigated: false; later: true", async () =>
            {
                Arrive("");

                _asyncFinished = false;

                await NavigateAndWait("#/route-state-async/one");
                await WaitFor(() => _asyncFinished);

                return "at OnNavigated: " + B(_asyncFinishedWhenNavigated) + "; later: " + B(_asyncFinished);
            });

            await CheckAsync(H, "H12", Probe("one") + "/cut/secret", "OnTransformRoutes that cuts \"/cut/...\": CurrentHash; RoutePath.Current; the handler's id", "#/route-state-probe/one; #/route-state-probe/one; id=one", async () =>
            {
                Arrive("");

                Router.OnTransformRoutes(url => url.IndexOf("/cut/") >= 0 ? url.Substring(0, url.IndexOf("/cut/")) : url);

                await NavigateAndWait(Probe("one") + "/cut/secret");

                return Router.CurrentHash + "; " + RoutePath.Current + "; id=" + (_probeParams is object && _probeParams.TryGetValue("id", out var id) ? id : "(none)");
            });

            Router.OnTransformRoutes(url => url);

            await CheckAsync(H, "H13", "#/route-state-nothing/x", "Navigate to an address no route matches", "not matched +1; handlers +0", async () =>
            {
                Arrive("");

                var mark    = new Mark();
                var missing = _notMatched;

                Router.Navigate("#/route-state-nothing/x");

                await WaitFor(() => _notMatched > missing);

                return "not matched +" + (_notMatched - missing) + "; handlers +" + (_navigated - mark.Navigated);
            });

            await CheckAsync(H, "H14", Probe("one") + "?q=%", "a stray % in the query", "matched: true; q=%", async () =>
            {
                Arrive("");

                var matched = await NavigateAndWait(Probe("one") + "?q=%");

                return "matched: " + B(matched) + "; q=" + (_probeParams is object && _probeParams.TryGetValue("q", out var q) ? q : "(none)");
            });

            Check(H, "H15", "(new Parameters)", "With returns the same instance; Clone is independent", "same instance: true; original 1, clone 2", () =>
            {
                var original = new Parameters();
                var same     = ReferenceEquals(original.With("a", "1"), original);
                var clone    = original.Clone();

                clone.With("b", "2");

                return "same instance: " + B(same) + "; original " + original.Count + ", clone " + clone.Count;
            });
        }

        // ------------------------------------------------------------------------------------------------

        private static void Install()
        {
            if (_installed) return;

            _installed = true;

            // Router.Initialize wraps both calls so that they announce themselves
            window.addEventListener("replacestate", (Action<Event>)(_ => _replaced++));
            window.addEventListener("pushstate",    (Action<Event>)(_ => _pushed++));

            Router.OnNavigated((toState, fromState) =>
            {
                _navigated++;
                _asyncFinishedWhenNavigated = _asyncFinished;
            });

            Router.OnNotMatched(parts => _notMatched++);

            Router.Register(PROBE, p =>
            {
                _probeRuns++;
                _probeParams = p;
                _onProbe?.Invoke(p);

                return !(p.TryGetValue("id", out var id) && id == "refuse");
            });

            Router.Register(PROBE_ASYNC, async p =>
            {
                await Task.Delay(60);
                _asyncFinished = true;
            });
        }

        private static bool RecordingGuard(Router.State toState, Router.State fromState, bool isBack)
        {
            _isBackByPath[toState.Path ?? ""] = isBack;

            return !_refuse && App.GalleryNavigationGuard(toState, fromState, isBack);
        }

        private static void Check(string section, string id, string input, string call, string expected, Func<string> act)
        {
            _results.Add(new RouteResult(section, id, input, call, expected, Try(act)));
        }

        private static async Task CheckAsync(string section, string id, string input, string call, string expected, Func<Task<string>> act)
        {
            string actual;

            try
            {
                actual = await act();
            }
            catch (Exception e)
            {
                actual = "threw: " + e.Message;
            }

            _results.Add(new RouteResult(section, id, input, call, expected, actual));
        }

        private static async Task<bool> NavigateAndWait(string path)
        {
            var before = _navigated;

            Router.Navigate(path);

            return await WaitFor(() => _navigated > before);
        }

        private static async Task<bool> WaitFor(Func<bool> condition)
        {
            var waited = 0;

            while (!condition())
            {
                if (waited >= 1500) return false;

                await Task.Delay(10);

                waited += 10;
            }

            return true;
        }

        // Puts the address bar and the router on the sample's own route, with this query, in place of the current entry
        private static void Arrive(string query) => Router.Replace(Home + query);

        private static string Probe(string id) => "#/route-state-probe/" + id;

        private static string Hash() => window.location.hash ?? "";

        private static string Query()
        {
            var hash  = Hash();
            var start = hash.IndexOf('?');

            return start >= 0 ? hash.Substring(start + 1) : "(none)";
        }

        private static string Effects(Mark mark) => "replaced " + (_replaced - mark.Replaced) + ", pushed " + (_pushed - mark.Pushed) + ", handlers " + (_navigated - mark.Navigated);

        private static string B(bool value) => value ? "true" : "false";
    }
}
