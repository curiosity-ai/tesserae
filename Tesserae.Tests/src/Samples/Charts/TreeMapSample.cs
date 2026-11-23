using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 47, Icon = UIcons.ChartTreeMap, Description = "Nested rectangles sized by value, zoomable")]
    public class TreeMapSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // The made-up company's cloud bill: teams own services, services own resources, and every resource is one of
        // these types. Listed in legend colour order; the weights say how common each type is.
        private static readonly (string Name, double Weight, string[] Kinds)[] ResourceTypes =
        {
            ("Compute",          0.30, new[] { "vm", "node", "worker", "fn" }),
            ("Databases",        0.22, new[] { "db", "replica", "cache" }),
            ("Storage",          0.17, new[] { "bucket", "volume", "backup" }),
            ("Networking",       0.13, new[] { "lb", "gateway", "nat", "cdn" }),
            ("Analytics",        0.10, new[] { "warehouse", "stream", "etl" }),
            ("Machine learning", 0.08, new[] { "gpu", "training", "inference" }),
        };

        // Findings a cost review can attach to a resource, each with how likely a resource is to have it.
        private static readonly (string Finding, double Chance)[] Findings =
        {
            ("Spot eligible",     0.18),
            ("No owner tag",      0.12),
            ("Over-provisioned",  0.10),
            ("Idle for 30+ days", 0.08),
            ("Public endpoint",   0.05),
            ("Legacy region",     0.03),
            ("No backups",        0.02),
        };

        // Teams and the services each one runs, with a rough share of the bill.
        private static readonly (string Team, int Resources, string[] Services)[] Teams =
        {
            ("Storefront",         1100, new[] { "catalog", "cart", "pricing", "reviews", "recommendations", "web-frontend" }),
            ("Payments",           700,  new[] { "checkout", "ledger", "fraud-scoring", "payouts", "invoicing" }),
            ("Data Platform",      900,  new[] { "lakehouse", "event-bus", "feature-store", "reporting", "ingest" }),
            ("Search",             550,  new[] { "indexer", "query", "ranking", "autocomplete" }),
            ("Logistics",          600,  new[] { "warehouse-sync", "routing", "tracking", "returns" }),
            ("Identity",           350,  new[] { "auth", "sessions", "profiles" }),
            ("Mobile Backend",     450,  new[] { "push", "api-gateway", "media", "sync" }),
            ("Internal Tools",     300,  new[] { "ci-runners", "wiki", "dashboards", "admin" }),
        };

        private static readonly string[] Environments = { "prod", "staging", "dev" };

        public TreeMapSample()
        {
            var bill = BuildBill(new SampleRandom(4711));

            var readout = TextBlock("Click a resource to read about it, or a service's name to zoom into it.").Secondary();
            var count   = TextBlock("").Secondary();

            var treeMap = TreeMap(bill).FormatValues(v => "$" + v.ToString("N0"), "per month").H(560.px());

            foreach (var type in ResourceTypes) treeMap.Category(type.Name, null);

            treeMap.OnNodeClick(node =>
            {
                if (node.IsLeaf)
                {
                    readout.Text = $"{node.Path} — ${node.Total:N0} per month, {node.Category}";
                }
                else
                {
                    readout.Text = $"{node.Path}/ — zoomed in";
                    treeMap.ZoomTo(node);
                }
            });

            var chips = new List<Button>();

            void Sync(TreeMap map)
            {
                count.Text = map.IsHighlighted ? $"{map.HighlightedCount:N0} resources highlighted: {map.HighlightLabel}" : "";
                foreach (var chip in chips) chip.IsPrimary = map.IsHighlighted && map.HighlightLabel == chip.Title;
            }

            treeMap.OnHighlightChanged(Sync);

            var find = SearchBox("e.g. ledger").SearchAsYouType().W(280.px()).OnSearch((_, value) =>
            {
                var query = (value ?? "").Trim();
                if (query.Length == 0) treeMap.ClearHighlight();
                else treeMap.Highlight(n => n.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0, $"names containing “{query}”");
            });

            var chipRow = HStack().Wrap().Children(Findings.Select(f =>
            {
                var finding = f.Finding;
                var flagged = CountFlagged(bill, finding);
                var chip    = Button($"{finding} {flagged:N0}").SetTitle(finding);
                chip.OnClick(() =>
                {
                    if (treeMap.HighlightLabel == finding) treeMap.ClearHighlight();
                    else treeMap.Highlight(n => n.Data is string[] found && found.Contains(finding), finding);
                });
                chips.Add(chip);
                return (IComponent)chip.MR(6).MB(6);
            }).ToArray());

            // A small tree with its own colours: what is taking up a disk.
            var disk = new TreeMapNode("disk").Add(
                new TreeMapNode("Videos").Add(
                    new TreeMapNode("Holiday 2025.mp4", 18_400, "Video"),
                    new TreeMapNode("Talk recording.mov", 9_200, "Video"),
                    new TreeMapNode("Screen captures").Add(
                        new TreeMapNode("demo-1.mp4", 1_900, "Video"),
                        new TreeMapNode("demo-2.mp4", 1_400, "Video"),
                        new TreeMapNode("demo-3.mp4", 800, "Video"))),
                new TreeMapNode("Photos").Add(
                    new TreeMapNode("2024", 7_600, "Images"),
                    new TreeMapNode("2025", 9_800, "Images"),
                    new TreeMapNode("Scans", 1_200, "Documents")),
                new TreeMapNode("Projects").Add(
                    new TreeMapNode("node_modules", 6_300, "Code"),
                    new TreeMapNode("src", 450, "Code"),
                    new TreeMapNode("build", 2_100, "Code"),
                    new TreeMapNode("datasets", 5_200, "Documents")),
                new TreeMapNode("Music").Add(
                    new TreeMapNode("Albums", 4_100, "Audio"),
                    new TreeMapNode("Podcasts", 2_600, "Audio")),
                new TreeMapNode("Documents", 1_800, "Documents"));

            var diskMap = TreeMap(disk)
               .Category("Video",     Theme.Colors.Purple600)
               .Category("Images",    Theme.Colors.Teal600)
               .Category("Code",      Theme.Colors.Orange600)
               .Category("Documents", Theme.Colors.Blue600)
               .Category("Audio",     Theme.Colors.Green600)
               .FormatValues(v => (v / 1000).ToString("0.0"), "GB")
               .Zoomable(false)
               .H(320.px());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TreeMapSample), UIcons.ChartTreeMap, "Nested rectangles sized by value, zoomable")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("TreeMap draws a hierarchy as nested rectangles: each leaf's area is its value, and each group is the sum of what it holds. Leaves are coloured by category, groups are separated by outlines and named on a plate in their top-left corner, and the legend shows each category's share of the total."),
                        TextBlock("It is drawn on a canvas, so it stays smooth with thousands of leaves. Scroll over the map to zoom, drag to move, double click to zoom in, and use + / - / 0 and the arrows from the keyboard. A highlight keeps the leaves that match at full colour and fades the rest, which is how a search shows where its results live.")
                    )).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Give the map a height: it fills its box and lays itself out again when the box changes size. Keep categories to eight or fewer so the colours stay distinguishable, and name them up front with .Category(...) when their order or colour should not depend on the data. Use the highlight to answer a question (\"where is X?\") rather than adding more colours.")
                    )).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A month's cloud bill: about 5,000 resources, grouped by team and service, sized by cost"),
                        treeMap,
                        readout.PT(8),
                        HStack().AlignItemsCenter().PT(16).Children(TextBlock("Find resources by name").SemiBold().PR(12), find, count.PL(12)),
                        SampleSubTitle("Highlight resources flagged by the cost review"),
                        chipRow
                    )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Disk usage, with chosen colours and zoom turned off"),
                        diskMap
                    )).SetTitle("Small trees")))
               .SeeAlso(typeof(ChartsSample), typeof(TreeSample), typeof(ActivityCalendarSample), typeof(DashboardSample));
        }

        private static int CountFlagged(TreeMapNode node, string finding)
        {
            if (node.IsLeaf) return node.Data is string[] found && found.Contains(finding) ? 1 : 0;
            return node.Children.Sum(c => CountFlagged(c, finding));
        }

        // Team → service → environment → resources. Each service leans towards one resource type (a database-heavy
        // ledger, a compute-heavy frontend), so the colours cluster the way a real bill does, with others mixed in.
        private static TreeMapNode BuildBill(SampleRandom rnd)
        {
            var root = new TreeMapNode("bill");

            foreach (var (teamName, resources, services) in Teams)
            {
                var team = new TreeMapNode(teamName);

                // Split the team's resources unevenly between its services.
                var weights = services.Select(_ => rnd.NextDouble(0.3, 1.6)).ToArray();
                var total   = weights.Sum();

                for (var s = 0; s < services.Length; s++)
                {
                    var serviceName = services[s];
                    var service     = new TreeMapNode(serviceName);
                    var count       = Math.Max(6, (int)Math.Round(resources * weights[s] / total));
                    var lean        = PickType(rnd);

                    // Production carries most of the bill; staging and dev are smaller copies.
                    var shares = new[] { 0.65, 0.22, 0.13 };
                    for (var e = 0; e < Environments.Length; e++)
                    {
                        var env     = new TreeMapNode(Environments[e]);
                        var inEnv   = Math.Max(2, (int)Math.Round(count * shares[e]));
                        var serial  = new Dictionary<string, int>();
                        var envCost = e == 0 ? 1.0 : e == 1 ? 0.45 : 0.25;

                        for (var i = 0; i < inEnv; i++)
                        {
                            var type = rnd.NextDouble() < 0.6 ? lean : ResourceTypes[PickTypeIndex(rnd)];
                            var kind = rnd.Pick(type.Kinds);
                            serial.TryGetValue(kind, out var n);
                            serial[kind] = ++n;

                            // Log-normal monthly cost: most resources are cheap, a few dominate the bill.
                            var u1    = Math.Max(1e-9, rnd.NextDouble());
                            var u2    = rnd.NextDouble();
                            var gauss = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                            var cost  = Math.Round(Math.Exp(5.2 + 1.2 * gauss) * envCost) + 4;

                            var found = Findings.Where(f => rnd.NextDouble() < f.Chance).Select(f => f.Finding).ToArray();
                            var label = $"{serviceName}-{kind}-{Environments[e]}-{n:00}";

                            env.Add(new TreeMapNode(label, cost, type.Name)
                            {
                                Data    = found,
                                Tooltip = found.Length > 0 ? "Flagged: " + string.Join(", ", found) : null,
                            });
                        }

                        service.Add(env);
                    }

                    team.Add(service);
                }

                root.Add(team);
            }

            return root;
        }

        private static (string Name, double Weight, string[] Kinds) PickType(SampleRandom rnd) => ResourceTypes[PickTypeIndex(rnd)];

        private static int PickTypeIndex(SampleRandom rnd)
        {
            var r = rnd.NextDouble();
            for (var i = 0; i < ResourceTypes.Length; i++)
            {
                r -= ResourceTypes[i].Weight;
                if (r < 0) return i;
            }
            return ResourceTypes.Length - 1;
        }

        public HTMLElement Render() => _content.Render();
    }
}
