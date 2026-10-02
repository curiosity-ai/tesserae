using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Utilities, Order = 70, Icon = UIcons.Navigation, Description = "Read and write URL state: paths, keys, history")]
    public class RouteStateSample : IComponent, ISample
    {
        private const string MONOSPACE_FONT_FAMILY = "var(--tss-monospace-font-family, monospace)";

        private readonly IComponent        _content;
        private readonly TextBlock         _readSummary;
        private readonly TextBlock         _runSummary;
        private readonly Stack             _readHost;
        private readonly Stack             _runHost;
        private readonly List<Expander>    _readExpanders = new List<Expander>();
        private readonly List<Expander>    _runExpanders  = new List<Expander>();
        private readonly List<RouteResult> _readResults;

        public RouteStateSample()
        {
            _readResults = RouteStateCases.Pure.Select(RouteStateRunner.Evaluate).ToList();

            _readSummary = TextBlock("").SemiBold();
            _runSummary  = TextBlock("Not run yet: these cases write to the address bar, so they wait for the button.").SemiBold();
            _readHost    = VStack().WS();
            _runHost     = VStack().WS();

            ShowSummary(_readSummary, _readResults);
            ShowResults(_readHost, _readExpanders, _readResults);

            var run = Button("Run write and router cases")
               .SetIcon(UIcons.Play)
               .Primary()
               .OnClickSpinWhile(async () =>
                {
                    var results = await RouteStateRunner.RunAsync();

                    ShowSummary(_runSummary, results);
                    ShowResults(_runHost, _runExpanders, results);
                }, "Running...");

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(RouteStateSample), UIcons.Navigation, "The address bar as the app's state: where it is, which keys it holds, what a write does to history")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("`RouteState` reads and writes the hash query string of the current page. `RouteLocation` is the same reading on any hash you give it: `RouteState.Current` is the one for the address bar, and a location can be built from a string before the first route has matched, or in a test."),
                        TextBlock("Every row below is a case: a hash (or a setup), the call, what it must give and what it gave. The page checks itself when it opens, with no browser driver; a failing row is shown red and its section opens."),
                        TextBlock("Paths compare by segment, ignoring case, empty segments and a trailing `/`. Query keys and values are case-sensitive. A malformed `%` never throws.")
                    )).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        HStack().WS().Wrap().Gap(8.px()).AlignItemsCenter().Children(
                            _readSummary,
                            Button("Expand all").OnClick(() => SetAll(true)),
                            Button("Collapse all").OnClick(() => SetAll(false))),
                        TextBlock("These build a `RouteLocation` from the hash in the row. Nothing navigates.").Secondary(),
                        _readHost)).SetTitle("Reading a hash (paths, child keys, matching, query parsing)")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        HStack().WS().Wrap().Gap(8.px()).AlignItemsCenter().Children(run, _runSummary),
                        TextBlock("These write the query of this page's own address, move between two probe routes and press the browser's Back button, then put the address bar back on this page. History is counted as calls to `pushState` and `replaceState`, and a route handler re-running as `Router.OnNavigated` firing. The run leaves a few entries in the browser's history.").Secondary(),
                        _runHost)).SetTitle("Writing, history and the router")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Not here: `Set` before the first route has matched (it does nothing, and a page that is already running has matched), and the cases that depend on an application's own routes, guards and search state."),
                        TextBlock("Route `:variables` stay on the handler's `Parameters` and are never written to the query. A handler and `GetQueryParameters()` get a copy, so changing it does not change the URL.")
                    )).SetTitle("Notes")))
               .SeeAlso(typeof(UnsavedChangesGuardSample), typeof(PivotSample), typeof(BindingSample));
        }

        private void SetAll(bool expanded)
        {
            _readExpanders.ForEach(e => e.IsExpanded = expanded);
            _runExpanders.ForEach(e => e.IsExpanded  = expanded);
        }

        private static void ShowResults(Stack host, List<Expander> expanders, List<RouteResult> results)
        {
            host.Clear();
            expanders.Clear();

            foreach (var group in results.GroupBy(r => r.Section))
            {
                var rows   = group.ToList();
                var failed = rows.Count(r => !r.Passed);

                var grid = Grid(28.px(), 56.px(), 3.fr(), 4.fr(), 3.fr(), 3.fr()).WS().Gap(4.px());

                foreach (var header in new[] { "", "Case", "Hash or setup", "Call", "Expected", "Actual" })
                {
                    grid.Add(TextBlock(header).SemiBold().Secondary());
                }

                foreach (var row in rows)
                {
                    grid.Add(Icon(row.Passed ? UIcons.Check : UIcons.Cross, color: row.Passed ? Theme.Success.Background : Theme.Danger.Background));
                    grid.Add(Cell(row.Id, false));
                    grid.Add(Cell(row.Input, false));
                    grid.Add(Cell(row.Call, false));
                    grid.Add(Cell(row.Expected, false));
                    grid.Add(Cell(row.Actual, !row.Passed));
                }

                var title = group.Key + "  (" + (rows.Count - failed) + " of " + rows.Count + (failed > 0 ? ", failing" : "") + ")";

                var expander = Expander(title, grid);
                expander.IsExpanded = failed > 0;

                expanders.Add(expander);
                host.Add(expander);
            }
        }

        private static IComponent Cell(string text, bool failing)
        {
            var cell = TextBlock(Shorten(text)).Small().Style(s =>
            {
                s.wordBreak   = "break-word";
                s.fontFamily   = MONOSPACE_FONT_FAMILY;
            });

            return failing ? cell.Foreground(Theme.Danger.Background) : cell;
        }

        private static string Shorten(string text) => text.Length > 120 ? text.Substring(0, 117) + "..." : text;

        private static void ShowSummary(TextBlock summary, List<RouteResult> results)
        {
            var passed = results.Count(r => r.Passed);

            summary.Text = passed + " of " + results.Count + " passed";
            summary.Foreground(passed == results.Count ? Theme.Success.Background : Theme.Danger.Background);
        }

        public HTMLElement Render() => _content.Render();
    }
}
