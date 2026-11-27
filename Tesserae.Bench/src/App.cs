using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using Transpose.Core;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Bench
{
    /// <summary>
    /// A representative "line of business" app used to profile Tesserae's rendering cost.
    /// Everything is driven from the DOM (stable ids) plus a couple of globals exposed to
    /// the automation harness so the page can time its own work without Playwright overhead.
    /// </summary>
    internal static class App
    {
        private static readonly SettableObservable<string> _page = new SettableObservable<string>("dashboard");

        private static void Main()
        {
            document.body.style.overflow = "hidden";

            var content = DeferSync(_page, BuildPage);

            var nav = VStack().W(200).HS().ScrollY().PL(8).PR(8).PT(8).Class("bench-nav").Children(
                TextBlock("Bench").Bold().XLarge().PB(8),
                NavButton("dashboard", "Dashboard", UIcons.ChartPie),
                NavButton("data",      "Data",      UIcons.TableRows),
                NavButton("form",      "Form",      UIcons.Edit),
                NavButton("list",      "List",      UIcons.List),
                NavButton("surfaces",  "Surfaces",  UIcons.Layers),
                TextBlock("App-shaped").Secondary().Tiny().PT(12).PB(4),
                NavButton("search",    "Search",    UIcons.Search),
                NavButton("tooltips",  "Tooltips",  UIcons.Comment),
                NavButton("defer",     "Defer",     UIcons.Refresh),
                NavButton("chat",      "Chat",      UIcons.Comments),
                NavButton("admin",     "Admin",     UIcons.Settings));

            MountToBody(HStack().S().Children(nav, content.HS().W(1).Grow()));

            ExposeHarness();
        }

        private static Button NavButton(string id, string text, UIcons icon) =>
            Button(text).SetIcon(icon).NoBorder().NoBackground().WS().AlignStart()
               .Id("nav-" + id)
               .OnClick(() => _page.Value = id);

        private static IComponent BuildPage(string page)
        {
            switch (page)
            {
                case "data":     return DataPage();
                case "form":     return FormPage();
                case "list":     return ListPage();
                case "surfaces": return SurfacesPage();
                case "search":   return SearchPage();
                case "tooltips": return TooltipsPage();
                case "defer":    return DeferPage();
                case "chat":     return ChatPage();
                case "admin":    return AdminPage();
                default:         return DashboardPage();
            }
        }

        // ------------------------------------------------------------------ pages

        private static IComponent DashboardPage()
        {
            var grid = Grid(1.fr(), 1.fr(), 1.fr()).Gap(12.px()).WS();

            for (int i = 0; i < 12; i++)
            {
                var n = i;

                grid.Add(Card(VStack().WS().Children(
                    HStack().WS().AlignItemsCenter().Children(
                        Icon(UIcons.ChartHistogram).PR(8),
                        TextBlock($"Metric {n}").SemiBold(),
                        Badge($"{n * 7 % 43}%").Success()),
                    Metric($"Series {n}", $"{n * 1234}"),
                    LineChart(Enumerable.Range(0, 40).Select(x => (double)((x * (n + 3)) % 37)).ToArray()).H(80),
                    BarChart(Enumerable.Range(0, 12).Select(x => (double)((x * (n + 5)) % 19)).ToArray()).H(60),
                    ProgressIndicator().Progress(n * 100f / 12f))).WS());
            }

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Dashboard").XLarge().Bold().PB(8),
                grid);
        }

        private static IComponent DataPage()
        {
            var all = Rows(400);

            var list = DetailsList<Row>(
                    DetailsListColumn(title: "Name",     width: 260.px(), enableColumnSorting: true, sortingKey: "Name", isRowHeader: true),
                    DetailsListColumn(title: "Owner",    width: 160.px(), enableColumnSorting: true, sortingKey: "Owner"),
                    DetailsListColumn(title: "Modified", width: 160.px(), enableColumnSorting: true, sortingKey: "Modified"),
                    DetailsListColumn(title: "Size",     width: 120.px(), enableColumnSorting: true, sortingKey: "Size"),
                    DetailsListColumn(title: "State",    width: 140.px()))
               .WS().Height(70.percent())
               .WithListItems(all)
               .SortedBy("Name");

            var search = SearchBox("Filter rows…").WS().Id("data-search");

            search.OnSearch((s, term) => list.WithListItems(string.IsNullOrEmpty(term)
                ? all
                : all.Where(r => r.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || r.Owner.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).ToArray()));

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Data").XLarge().Bold().PB(8),
                search,
                list);
        }

        private static IComponent FormPage()
        {
            var stack = VStack().WS();

            for (int i = 0; i < 24; i++)
            {
                var n = i;

                stack.Add(HStack().WS().Children(
                    Label($"Field {n}").SetContent(TextBox($"value {n}").WS()).W(50.percent()),
                    Label($"Choice {n}").SetContent(Dropdown().Items(
                        DropdownItem("Alpha").Selected(),
                        DropdownItem("Beta"),
                        DropdownItem("Gamma"),
                        DropdownItem("Delta"))).W(50.percent())));

                stack.Add(HStack().WS().Children(
                    CheckBox($"Enabled {n}").Checked(n % 2 == 0),
                    Toggle($"Toggle {n}"),
                    Slider(n * 4 % 100).W(200),
                    ChoiceGroup().Horizontal().Choices(Choice("A"), Choice("B"), Choice("C"))));
            }

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Form").XLarge().Bold().PB(8),
                Card(stack).WS());
        }

        private static IComponent ListPage()
        {
            var items = Enumerable.Range(0, 300).Select(n =>
                (IComponent)Card(VStack().WS().Children(
                    HStack().WS().AlignItemsCenter().Children(
                        Icon(UIcons.User).PR(8),
                        TextBlock($"Item {n}").SemiBold(),
                        Badge(n % 3 == 0 ? "new" : "old")),
                    TextBlock($"Row {n} — a short description used to give the layout something to measure.").Secondary(),
                    HStack().Children(
                        Button("Open").Primary().Small(),
                        Button("Dismiss").Small()))).WS()).ToArray();

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("List").XLarge().Bold().PB(8),
                ItemsList(items, 1.fr(), 1.fr()).WS());
        }

        private static IComponent SurfacesPage()
        {
            var modal = Modal("Bench modal").Content(VStack().WS()
               .Children(Enumerable.Range(0, 30).Select(n => (IComponent)TextBlock($"Modal line {n}")).ToArray()));

            var panel = Panel("Bench panel").Content(VStack().WS()
               .Children(Enumerable.Range(0, 30).Select(n => (IComponent)TextBlock($"Panel line {n}")).ToArray()));

            var pivot = Pivot().WS()
               .Pivot("p1", PivotTitle("Tab 1"), () => DashboardPage())
               .Pivot("p2", PivotTitle("Tab 2"), () => ListPage())
               .Pivot("p3", PivotTitle("Tab 3"), () => FormPage());

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Surfaces").XLarge().Bold().PB(8),
                HStack().Children(
                    Button("Open modal").Primary().Id("open-modal").OnClick(() => modal.Show()),
                    Button("Open panel").Id("open-panel").OnClick(() => panel.Show())),
                pivot.H(600));
        }

        // ------------------------------------------------------- app-shaped pages
        //
        // These five mirror how the Mosaik front end actually composes Tesserae, taken from a
        // component census of that codebase: TextBlock (2257 uses), Button (1312), HStack (1017),
        // VStack (964), Tooltip (892), Toast (571), Label (547), Icon (390), Defer/DeferSync (503),
        // Raw (298), Modal (200), Dropdown (136), Dialog (115), Pivot (109). The first five bench
        // pages exercised almost none of the tooltip, defer or raw surface, which is where the real
        // app spends much of its build time.

        /// <summary>
        /// The search screen: a resizable facets rail beside a page of OmniResult rows. Modelled on
        /// FacetsRenderer + the node-result rows — every command button carries a tooltip, every row
        /// an icon, badge, source marker and highlighted text.
        /// </summary>
        private static IComponent SearchPage()
        {
            var facets = VStack().WS().ScrollY();

            facets.Add(HStack().WS().AlignItemsCenter().Children(
                TextBlock("Filters").SemiBold().Grow(),
                Button().SetIcon(UIcons.FolderOpen).Tooltip("View saved filters"),
                Button().SetIcon(UIcons.Disk).Tooltip("Save filters"),
                Button().SetIcon(UIcons.Lock).Tooltip("Lock filters to keep them when the search terms change"),
                Button().SetIcon(UIcons.Cross).Tooltip("Clear filters")));

            for (int g = 0; g < 8; g++)
            {
                var group = VStack().WS();

                for (int f = 0; f < 10; f++)
                {
                    var n = g * 10 + f;

                    group.Add(HStack().WS().AlignItemsCenter().Children(
                        CheckBox($"Value {n}").Checked(n % 5 == 0),
                        TextBlock($"{(n * 37) % 900}").Secondary().Tiny().ML(4).Tooltip($"{(n * 37) % 900} matching documents")));
                }

                facets.Add(Expander($"Facet group {g}", group).Expanded(g < 2).WS());
            }

            var results = VStack().WS().ScrollY();

            for (int i = 0; i < 60; i++)
            {
                var n = i;

                results.Add(OmniResult<string>($"result-{n}", $"Quarterly report {n}.docx")
                       .SetIcon(UIcons.FileWord)
                       .SetBadge($"{(n * 13) % 100}%")
                       .SetText($"Section {n} of the quarterly report covering revenue, headcount and the outlook for the following quarter.")
                       .SetSource(Theme.Colors.Blue600, Owners[n % Owners.Length])
                       .HighlightWords("quarterly", "revenue")
                       .WS()
                       .Tooltip($"Open Quarterly report {n}.docx"));
            }

            var search = SearchBox("Search everything…").WS().Id("search-box");

            return VStack().S().PL(16).PR(16).PT(16).Children(
                HStack().WS().AlignItemsCenter().Children(search.Grow(), Button("Search").Primary().Tooltip("Run the search")),
                SplitView().WS().Grow().Resizable().LeftIsSmaller(320.px())
                   .Left(facets)
                   .Right(VStack().S().Children(results.Grow(), Pagination(600, 60, 1).WS())));
        }

        /// <summary>
        /// Tooltip density on its own. Mosaik attaches a tooltip to nearly every command button and
        /// icon, so a toolbar-heavy screen builds hundreds of them; this isolates that cost from the
        /// rest of a page.
        /// </summary>
        private static IComponent TooltipsPage()
        {
            var grid = Grid(1.fr(), 1.fr(), 1.fr(), 1.fr(), 1.fr(), 1.fr()).Gap(6.px()).WS();

            for (int i = 0; i < 300; i++)
            {
                var n = i;

                grid.Add(HStack().WS().AlignItemsCenter().Children(
                    Icon(UIcons.Bell).Tooltip($"Notification {n}"),
                    Button($"Cmd {n}").Small().Tooltip($"Run command {n} against the current selection"),
                    Badge($"{n % 9}").Tooltip($"{n % 9} pending")));
            }

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Tooltips").XLarge().Bold().PB(8),
                TextBlock("900 tooltip attachments across 300 rows.").Secondary().PB(8),
                grid);
        }

        /// <summary>
        /// Observable-driven re-rendering. Defer/DeferSync is how the app swaps whole regions when
        /// state changes; the harness flips the observables to force repeated rebuilds.
        /// </summary>
        private static IComponent DeferPage()
        {
            var stack = VStack().WS();

            for (int i = 0; i < 60; i++)
            {
                var n = i;

                stack.Add(Card(DeferSync(_deferTick, tick => VStack().WS().Children(
                    HStack().WS().AlignItemsCenter().Children(
                        Icon(UIcons.Refresh).PR(6),
                        TextBlock($"Panel {n} — revision {tick}").SemiBold(),
                        Badge(tick % 2 == 0 ? "even" : "odd")),
                    TextBlock($"Rebuilt {tick} times. Panels re-render whenever the observable they watch changes.").Secondary(),
                    HStack().Children(
                        Button("Open").Small().Primary().Tooltip($"Open panel {n}"),
                        Button("Dismiss").Small())))).WS());
            }

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Defer").XLarge().Bold().PB(8),
                stack);
        }

        /// <summary>
        /// The chat surface: markdown-rendered assistant turns with context cards and per-message
        /// command buttons, which is what CurrentChatComponent builds.
        /// </summary>
        private static IComponent ChatPage()
        {
            var chat = ChatArea().S();

            for (int i = 0; i < 30; i++)
            {
                var n = i;

                chat.Add(ChatMessage(TextBlock($"Question {n}: how do I configure the {Owners[n % Owners.Length]} connector?"), Avatar(null, "U"))
                   .RightAligned().MaxWidth());

                chat.Add(ChatMessage(
                        VStack().WS().Children(
                            MarkdownBlock($"### Answer {n}\n\nOpen **Settings → Connectors** and pick the `{Owners[n % Owners.Length]}` entry.\n\n1. Set the endpoint\n2. Paste the token\n3. Save\n\n> Tokens are stored encrypted."),
                            ContextCards(
                                ContextCard($"connectors-{n}.md", UIcons.File),
                                ContextCard($"setup-{n}.pdf", UIcons.FilePdf))),
                        Avatar(null, "AI"),
                        HStack().Children(
                            Button().SetIcon(UIcons.Copy).Small().Tooltip("Copy answer"),
                            Button().SetIcon(UIcons.ThumbsUp).Small().Tooltip("Good answer"),
                            Button().SetIcon(UIcons.ThumbsDown).Small().Tooltip("Bad answer")))
                   .MaxWidth());
            }

            return VStack().S().PL(16).PR(16).PT(16).Children(
                TextBlock("Chat").XLarge().Bold().PB(8),
                chat.Grow());
        }

        /// <summary>
        /// A dense admin table built from stacks rather than DetailsList — rows of status, toggles,
        /// dropdowns and command buttons, the shape EndpointsView and AdminBuildView use.
        /// </summary>
        private static IComponent AdminPage()
        {
            var rows = VStack().WS();

            rows.Add(HStack().WS().AlignItemsCenter().Class("bench-admin-head").Children(
                TextBlock("Endpoint").SemiBold().W(240),
                TextBlock("Owner").SemiBold().W(120),
                TextBlock("State").SemiBold().W(120),
                TextBlock("Enabled").SemiBold().W(100),
                TextBlock("Mode").SemiBold().W(160),
                TextBlock("Actions").SemiBold().Grow()));

            for (int i = 0; i < 120; i++)
            {
                var n = i;

                rows.Add(HStack().WS().AlignItemsCenter().Children(
                    HStack().W(240).AlignItemsCenter().Children(
                        Icon(UIcons.Globe).PR(6),
                        TextBlock($"/api/v1/service-{n}").Tooltip($"https://cluster.internal/api/v1/service-{n}")),
                    TextBlock(Owners[n % Owners.Length]).W(120),
                    Badge(n % 4 == 0 ? "degraded" : "healthy").W(120),
                    Toggle().Checked(n % 3 != 0).W(100),
                    Dropdown().Items(
                        DropdownItem("Automatic").Selected(),
                        DropdownItem("Manual"),
                        DropdownItem("Disabled")).W(160),
                    HStack().Grow().Children(
                        Button().SetIcon(UIcons.Refresh).Small().Tooltip($"Restart service-{n}"),
                        Button().SetIcon(UIcons.ChartHistogram).Small().Tooltip($"Metrics for service-{n}"),
                        Button().SetIcon(UIcons.Trash).Small().Tooltip($"Delete service-{n}"))));
            }

            return VStack().S().ScrollY().PL(16).PR(16).PT(16).Children(
                TextBlock("Admin").XLarge().Bold().PB(8),
                Card(rows).WS());
        }

        private static readonly SettableObservable<int> _deferTick = new SettableObservable<int>(0);

        // ------------------------------------------------------------------ data

        private static Row[] Rows(int count) =>
            Enumerable.Range(0, count)
               .Select(n => new Row($"Document_{n:0000}.docx", Owners[n % Owners.Length], DateTime.Today.AddDays(-n), n * 3.7))
               .ToArray();

        private static readonly string[] Owners = { "alice", "bob", "carol", "dave", "erin", "frank" };

        // ------------------------------------------------------------------ harness

        /// <summary>
        /// Exposes the scenarios to the automation harness. The page times its own work so the
        /// numbers exclude CDP/Playwright round-trips.
        /// </summary>
        private static void ExposeHarness()
        {
            Action<string> go = p => _page.Value = p;

            Func<string, int, double> build = (p, times) =>
            {
                var start = Script.Write<double>("performance.now()");

                for (int i = 0; i < times; i++)
                {
                    var host = document.createElement("div");
                    host.style.position = "absolute";
                    host.style.left     = "-10000px";
                    document.body.appendChild(host);
                    host.appendChild(BuildPage(p).Render());
                    document.body.removeChild(host);
                }
                return Script.Write<double>("performance.now()") - start;
            };

            // Drives the observable every Defer panel watches, so the harness can force N rebuilds
            // of a whole page region the way a state change does in the real app.
            Action<int> churnDefer = times =>
            {
                for (int i = 0; i < times; i++) _deferTick.Value = _deferTick.Value + 1;
            };

            Action<int> burstToasts = count =>
            {
                for (int i = 0; i < count; i++) Toast().Information("Job finished", $"Indexing batch {i} completed");
            };

            Script.Write("window.__bench = { go: {0}, build: {1}, churnDefer: {2}, burstToasts: {3} }",
                go, build, churnDefer, burstToasts);
        }

        private sealed class Row : IDetailsListItem<Row>
        {
            public Row(string name, string owner, DateTime modified, double size)
            {
                Name     = name;
                Owner    = owner;
                Modified = modified;
                Size     = size;
            }

            public string   Name     { get; }
            public string   Owner    { get; }
            public DateTime Modified { get; }
            public double   Size     { get; }

            public bool EnableOnListItemClickEvent => true;
            public void OnListItemClick(int index) { }

            public int CompareTo(Row other, string key)
            {
                switch (key)
                {
                    case "Name":     return string.Compare(Name,  other.Name,  StringComparison.OrdinalIgnoreCase);
                    case "Owner":    return string.Compare(Owner, other.Owner, StringComparison.OrdinalIgnoreCase);
                    case "Modified": return Modified.CompareTo(other.Modified);
                    case "Size":     return Size.CompareTo(other.Size);
                    default:         return 0;
                }
            }

            public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> cell)
            {
                yield return cell(columns[0], () => TextBlock(Name));
                yield return cell(columns[1], () => TextBlock(Owner));
                yield return cell(columns[2], () => TextBlock(Modified.ToShortDateString()));
                yield return cell(columns[3], () => TextBlock(Size.ToString("N1") + " KB"));
                yield return cell(columns[4], () => Badge(Size > 500 ? "large" : "small"));
            }
        }
    }
}
