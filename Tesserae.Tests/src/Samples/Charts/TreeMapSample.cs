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

        // Fields of mathematics the made-up library's modules are filed under, in legend colour order.
        private static readonly string[] Fields =
        {
            "Arithmetic and algebraic geometry",
            "Automorphic forms",
            "Analytic number theory",
            "Harmonic analysis",
            "Algebra",
            "Combinatorics",
        };

        // Terms a module can mention, each with how likely a module is to.
        private static readonly (string Term, double Chance)[] Terms =
        {
            ("Theta lift",            0.12),
            ("Seesaw identity",       0.07),
            ("Siegel–Weil formula",   0.3),
            ("Mass formula",          0.17),
            ("Chowla–Selberg formula", 0.012),
            ("Bost slope inequality", 0.004),
            ("Behrend sets",          0.003),
        };

        private static readonly string[] Words =
        {
            "Eisenstein", "Theta", "Kernel", "Divisor", "Height", "Lattice", "Spectrum", "Weil", "Bound", "Hecke", "Operator",
            "Modular", "Curve", "Period", "Residue", "Zeta", "Series", "Trace", "Formula", "Quadratic", "Form", "Genus",
            "Local", "Global", "Measure", "Integral", "Fourier", "Coefficient", "Character", "Sum", "Estimate", "Density",
            "Galois", "Ideal", "Class", "Group", "Norm", "Orbit", "Cusp", "Petersson", "Rankin", "Selberg", "Shimura",
        };

        public TreeMapSample()
        {
            var library = BuildLibrary(new SampleRandom(4711));

            var readout = TextBlock("Click a module to read about it, or a group's name to zoom into it.").Secondary();
            var count   = TextBlock("").Secondary();

            var treeMap = TreeMap(library).Unit("lines").H(560.px());

            for (var i = 0; i < Fields.Length; i++) treeMap.Category(Fields[i], null);

            treeMap.OnNodeClick(node =>
            {
                if (node.IsLeaf)
                {
                    readout.Text = $"{node.Path} — {node.Total:N0} lines, {node.Category}";
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
                count.Text = map.IsHighlighted ? $"{map.HighlightedCount:N0} modules highlighted: {map.HighlightLabel}" : "";
                foreach (var chip in chips) chip.IsPrimary = map.IsHighlighted && map.HighlightLabel == chip.Title;
            }

            treeMap.OnHighlightChanged(Sync);

            var find = SearchBox("e.g. Eisenstein").SearchAsYouType().W(280.px()).OnSearch((_, value) =>
            {
                var query = (value ?? "").Trim();
                if (query.Length == 0) treeMap.ClearHighlight();
                else treeMap.Highlight(n => n.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0, $"names containing “{query}”");
            });

            var chipRow = HStack().Wrap().Children(Terms.Select(t =>
            {
                var term     = t.Term;
                var mentions = CountMentions(library, term);
                var chip     = Button($"{term} {mentions:N0}").SetTitle(term);
                chip.OnClick(() =>
                {
                    if (treeMap.HighlightLabel == term) treeMap.ClearHighlight();
                    else treeMap.Highlight(n => n.Data is string[] mentioned && mentioned.Contains(term), term);
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
                        SampleSubTitle("A library of 5,000 modules, grouped by directory and sized by lines"),
                        treeMap,
                        readout.PT(8),
                        HStack().AlignItemsCenter().PT(16).Children(TextBlock("Find in module names").SemiBold().PR(12), find, count.PL(12)),
                        SampleSubTitle("Highlight modules that mention"),
                        chipRow
                    )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Disk usage, with chosen colours and zoom turned off"),
                        diskMap
                    )).SetTitle("Small trees")))
               .SeeAlso(typeof(ChartsSample), typeof(TreeSample), typeof(ActivityCalendarSample), typeof(DashboardSample));
        }

        private static int CountMentions(TreeMapNode node, string term)
        {
            if (node.IsLeaf) return node.Data is string[] mentioned && mentioned.Contains(term) ? 1 : 0;
            return node.Children.Sum(c => CountMentions(c, term));
        }

        // Top-level groups of varying size, each with sub-directories of modules. Every directory leans towards one
        // field, so the colours cluster the way real code does, with a scatter of others mixed in.
        private static TreeMapNode BuildLibrary(SampleRandom rnd)
        {
            var root = new TreeMapNode("library");
            var tops = new (string Name, int Modules)[]
            {
                ("Development", 2200), ("Automorphic Arithmetic", 520), ("Automorphic Spectrum", 420), ("Weil Bound", 260),
                ("Formalism", 640), ("Ground Truth", 380), ("Foundations", 300), ("Interfaces", 280),
            };

            var subNames = new[] { "Proofs", "Divisor Proofs", "Heights", "Local Factors", "Kernels", "Periods", "Lemmas", "Estimates", "Lifts", "Spectral Side", "Geometric Side", "Constants" };
            var used     = new HashSet<string>();

            foreach (var (name, modules) in tops)
            {
                var top  = new TreeMapNode(name);
                var left = modules;

                while (left > 0)
                {
                    var dirSize = Math.Min(left, rnd.Next(20, Math.Max(40, modules / 3)));
                    left -= dirSize;
                    var dir = new TreeMapNode(rnd.Pick(subNames));
                    AddModules(dir, dirSize, rnd, used);
                    top.Add(dir);
                }

                root.Add(top);
            }

            return root;
        }

        private static void AddModules(TreeMapNode dir, int count, SampleRandom rnd, HashSet<string> used)
        {
            // Big directories get a level of sub-directories of their own.
            if (count > 120)
            {
                var parts = rnd.Next(2, 5);
                for (var p = 0; p < parts; p++)
                {
                    var first = rnd.Pick(Words);
                    var second = rnd.Pick(Words.Where(w => w != first).ToArray());
                    var sub = new TreeMapNode(first + " " + second);
                    AddModules(sub, count / parts, rnd, used);
                    dir.Add(sub);
                }
                return;
            }

            var lean = PickField(rnd);

            for (var i = 0; i < count; i++)
            {
                string label;
                do
                {
                    label = rnd.Pick(Words) + rnd.Pick(Words) + (rnd.NextDouble() < 0.5 ? rnd.Pick(Words) : "");
                } while (!used.Add(label) && used.Count < 60_000);

                // Log-normal sizes: most modules are short, a few are very long.
                var u1    = Math.Max(1e-9, rnd.NextDouble());
                var u2    = rnd.NextDouble();
                var gauss = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
                var lines = Math.Round(Math.Exp(4.6 + 1.1 * gauss)) + 5;

                var field = rnd.NextDouble() < 0.55 ? lean : PickField(rnd);
                var terms = Terms.Where(t => rnd.NextDouble() < t.Chance).Select(t => t.Term).ToArray();

                dir.Add(new TreeMapNode(label + ".lean", lines, field) { Data = terms, Tooltip = terms.Length > 0 ? "Mentions " + string.Join(", ", terms) : null });
            }
        }

        // Some fields are much more common than others, as in a real library.
        private static readonly double[] FieldWeights = { 0.3, 0.22, 0.17, 0.13, 0.1, 0.08 };

        private static string PickField(SampleRandom rnd)
        {
            var r = rnd.NextDouble();
            for (var i = 0; i < Fields.Length; i++)
            {
                r -= FieldWeights[i];
                if (r < 0) return Fields[i];
            }
            return Fields[Fields.Length - 1];
        }

        public HTMLElement Render() => _content.Render();
    }
}
