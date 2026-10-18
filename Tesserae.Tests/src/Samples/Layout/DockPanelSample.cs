using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Layout, Order = 45, Icon = UIcons.WindowFrame, Description = "IDE-style dockable, tabbed and auto-hide panes")]
    public class DockPanelSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private          string     _savedLayout;

        public DockPanelSample()
        {
            var layoutText = TextBlock("Move, resize, pin or close a pane to see its layout here.").Small().Secondary().Wrap().Class("tss-dockpanel-sample-layout");

            var dock = DockPanel()
               .Document(Editor())
               .Panes(
                    DockPane("explorer", "Explorer", FileList()).Icon(UIcons.Folder).Left().SetSize(240).InTabGroup("left-tools"),
                    DockPane("search",   "Search",   SearchPane()).Icon(UIcons.Search).Left().InTabGroup("left-tools"),
                    DockPane("outline",  "Outline",  Outline()).Icon(UIcons.List).Left().SetWeight(0.6),
                    DockPane("properties", "Properties", Properties()).Icon(UIcons.Settings).Right().SetSize(250),
                    DockPane("toolbox",  "Toolbox",  Toolbox()).Icon(UIcons.Apps).Right().Unpinned(),
                    DockPane("output",   "Output",   Output()).Icon(UIcons.Terminal).Bottom().SetSize(150).InTabGroup("bottom"),
                    DockPane("problems", "Problems", Problems()).Icon(UIcons.TriangleWarning).Bottom().InTabGroup("bottom"),
                    DockPane("history",  "History",  TextBlock("No earlier versions.").Secondary().P(12)).Icon(UIcons.Clock).Right().Closed())
               .OnLayoutChanged(json => layoutText.Text = json);

            var priority = HStack().AlignItemsCenter().Gap(4.px()).Children(
                TextBlock("Corners:").Small(),
                Button("Top & bottom span").OnClick(() => dock.EdgePriority(DockEdge.Top, DockEdge.Bottom, DockEdge.Left, DockEdge.Right)),
                Button("Sides span").OnClick(() => dock.EdgePriority(DockEdge.Left, DockEdge.Right, DockEdge.Top, DockEdge.Bottom)));

            // The bar along the top of the panel: a title, a toggle per pane (PaneToggle, pressed while the pane is in
            // view) and commands, on one line.
            dock.Title(HStack().AlignItemsCenter().Gap(8.px()).Children(Icon(UIcons.Folder), TextBlock("Workspace").SemiBold()))
               .PaneToggles()
               .HeaderCommands(
                Button().SetIcon(UIcons.Disk).Tooltip("Save layout").OnClick(() =>
                {
                    _savedLayout = dock.GetLayout();
                    Toast().Success("Layout saved");
                }),
                Button().SetIcon(UIcons.Refresh).Tooltip("Restore layout").OnClick(() =>
                {
                    if (_savedLayout is null) Toast().Warning("Save a layout first");
                    else dock.LoadLayout(_savedLayout);
                }));

            var reopen = Button("Reopen all").OnClick(() =>
            {
                foreach (var pane in dock.CurrentPanes.Where(p => !p.IsOpen).ToList()) dock.Open(pane.Id);
            });

            _content = SectionStack().Secondary()
               .S()
               .SampleTitle(typeof(DockPanelSample), UIcons.WindowFrame, "A document with panes docked around it")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("DockPanel arranges a document with panes docked to its four edges, the way an IDE does. Panes on one edge stack with a splitter between them, and become tabs of one slot when they share a tab group. Every seam can be dragged to resize."),
                        TextBlock("Drag a pane by its title or tab: markers appear on the borders to dock it to another edge, and a compass over the pane under the pointer joins it as a tab or places it above, below or beside. The pin sends a pane to a rail on its edge, from where it slides out over the document; the cross closes it, and the menu at the top right of the document reopens it. Below 600px every pane goes to its rail. The bar along the top carries a title, a toggle per pane and commands on one line (Title, PaneToggles, HeaderCommands); PaneToggle alone puts a pane's toggle in a toolbar of your own."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Give every pane a stable id: it is what a saved layout refers to. Save the layout from OnLayoutChanged (for example to local storage) and put it back with LoadLayout when the page opens, so people find their workspace as they left it. Limit AllowEdges for panes that only make sense in one orientation, and keep the document the thing people came for: start with few panes pinned and the rest on the rails."))).SetTitle("Best Practices")))
               .FlatSection(Card(VStack().WS().Children(
                        HStack().AlignItemsCenter().Gap(24.px()).Children(priority, reopen).MB(8),
                        dock.WS().H(560),
                        layoutText.MT(8))).SetTitle("Usage"))
               .SeeAlso(typeof(SplitViewSample), typeof(HorizontalSplitViewSample), typeof(GridSample), typeof(PanelSample), typeof(PivotSample));
        }

        private static IComponent Editor()
        {
            var lines = new[]
            {
                "using static Tesserae.UI;",
                "",
                "public static class Program",
                "{",
                "    public static void Main()",
                "    {",
                "        var dock = DockPanel()",
                "            .Document(Editor())",
                "            .Panes(",
                "                DockPane(\"explorer\", \"Explorer\", FileList()).Left(),",
                "                DockPane(\"output\", \"Output\", Output()).Bottom());",
                "",
                "        MountToBody(dock);",
                "    }",
                "}",
            };

            var code = VStack().P(12).Children(lines.Select((l, i) => (IComponent)HStack().Children(
                TextBlock((i + 1).ToString()).Secondary().W(28).NoShrink().TextRight().PR(12),
                TextBlock(l.Length == 0 ? " " : l).Class("tss-dockpanel-sample-code"))).ToArray());

            return VStack().S().Children(
                HStack().AlignItemsCenter().Gap(8.px()).PL(12).PR(40).PT(8).PB(8).Children(Icon(UIcons.File), TextBlock("Program.cs").SemiBold()),
                code);
        }

        private static IComponent FileList()
        {
            var files = new[] { ("src", UIcons.Folder), ("Program.cs", UIcons.File), ("App.cs", UIcons.File), ("Styles.css", UIcons.File), ("tps.json", UIcons.File), ("README.md", UIcons.File) };
            return VStack().P(8).Children(files.Select(f => (IComponent)HStack().AlignItemsCenter().Gap(8.px()).PT(4).PB(4).Children(Icon(f.Item2).Small(), TextBlock(f.Item1).Small())).ToArray());
        }

        private static IComponent SearchPane() => VStack().P(8).Gap(8.px()).Children(SearchBox("Search files").WS(), TextBlock("Type to search the workspace.").Small().Secondary());

        private static IComponent Outline()
        {
            var members = new[] { "Program", "Main()", "Editor()", "FileList()", "Output()" };
            return VStack().P(8).Children(members.Select((m, i) => (IComponent)TextBlock(m).Small().PL(i == 0 ? 0 : 16).PT(3).PB(3)).ToArray());
        }

        private static IComponent Properties()
        {
            var rows = new[] { ("Name", "Program.cs"), ("Size", "1.2 KB"), ("Encoding", "UTF-8"), ("Line endings", "LF"), ("Language", "C#") };
            return VStack().P(10).Gap(6.px()).Children(rows.Select(r => (IComponent)HStack().Children(TextBlock(r.Item1).Small().Secondary().W(96).NoShrink(), TextBlock(r.Item2).Small())).ToArray());
        }

        private static IComponent Toolbox()
        {
            var tools = new[] { "Button", "TextBox", "Stack", "Grid", "DetailsList", "Scheduler" };
            return VStack().P(8).Gap(4.px()).Children(tools.Select(t => (IComponent)Button(t).WS()).ToArray());
        }

        private static IComponent Output()
        {
            var lines = new[] { "Build started...", "Restoring packages", "Compiling 214 files", "Writing assets/js/app.js", "Build succeeded in 7.8s" };
            return VStack().P(8).Children(lines.Select(l => (IComponent)TextBlock(l).Small().Class("tss-dockpanel-sample-code")).ToArray());
        }

        private static IComponent Problems() => VStack().P(8).Gap(4.px()).Children(
            HStack().AlignItemsCenter().Gap(6.px()).Children(Icon(UIcons.TriangleWarning).Small().Foreground(Theme.Warning.Background), TextBlock("CS0168: The variable 'x' is declared but never used").Small()),
            HStack().AlignItemsCenter().Gap(6.px()).Children(Icon(UIcons.Info).Small(), TextBlock("Consider a seeded Random in samples").Small()));

        public HTMLElement Render() => _content.Render();
    }
}
