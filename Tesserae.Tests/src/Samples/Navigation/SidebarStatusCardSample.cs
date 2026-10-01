using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 25, Icon = UIcons.ProgressComplete, Description = "A live status card at the top of a sidebar")]
    public class SidebarStatusCardSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private          double     _timer;

        public SidebarStatusCardSample()
        {
            // The plain header: a SidebarBrand with the section's glyph, name and what is in it, for a section
            // that has no state to report
            var plainSidebar = Sidebar();
            plainSidebar.AddHeader(new SidebarBrand("build-brand", UIcons.Database, "Build", "Data, AI and delivery").Separated());
            FillSection(plainSidebar, "plain-");

            var card = new SidebarStatusCard("build-status", UIcons.Database, "Build", "Index healthy · 2.4M nodes")
               .Success()
               .Progress(72, "Re-index 72% · 4 min left");

            var sidebar = Sidebar();
            sidebar.AddHeader(card);
            FillSection(sidebar, "");

            var controls = HStack().WS().Wrap().Children(
                Button("Run re-index").SetIcon(UIcons.Refresh).Primary().OnClick(() => RunReindex(card)),
                Button("Healthy").OnClick(() => { StopReindex(); card.SetStatus("Index healthy · 2.4M nodes").Success().ClearProgress(); }),
                Button("Lagging").OnClick(() => { StopReindex(); card.SetStatus("Index 3 h behind").Warning().Indeterminate("Catching up"); }),
                Button("Failed").OnClick(() => { StopReindex(); card.SetStatus("Re-index failed").Danger().ClearProgress(); }),
                Button("Toggle sidebar").SetIcon(UIcons.Sidebar).OnClick(() => sidebar.Toggle()));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarStatusCardSample), UIcons.ProgressComplete, "A live status card at the top of a sidebar")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A SidebarStatusCard heads a sidebar with what its section is doing right now: the section's glyph and name, a status line with a dot in the status's tone, and an optional progress bar with a caption for a job that is running. Every part can be changed while it is on screen. On the collapsed rail it is the glyph with a short progress bar under it, and the text moves to its tooltip.")
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        TextBlock("When the section has no state to report, head the sidebar with a SidebarBrand instead: the section's glyph, its name and a line saying what is in it, with .Separated() drawing the divider under it. Without an .OnClick(...) the row is a label, not a button: it does not answer the pointer. It sits where the status card would, without the dot or the progress bar.").PaddingBottom(8.px()),
                        HStack().WS().H(500).Children(
                            plainSidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Plain"),
                    Card(VStack().WS().Children(
                        controls.PaddingBottom(8.px()),
                        HStack().WS().H(500).Children(
                            sidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Live status")))
               .SeeAlso(typeof(SidebarSample), typeof(SidenavSample), typeof(ProgressIndicatorSample));
        }

        private static void FillSection(Sidebar sidebar, string prefix)
        {
            sidebar.AddContent(new SidebarSeparator(prefix + "data", "Data"));
            sidebar.AddContent(new SidebarButton(prefix + "data-sources", UIcons.Database,       "Data Sources").Selected());
            sidebar.AddContent(new SidebarButton(prefix + "graph-db",     UIcons.DiagramProject, "Graph DB"));
            sidebar.AddContent(new SidebarButton(prefix + "search-cfg",   UIcons.Search,         "Search config"));
            sidebar.AddContent(new SidebarSeparator(prefix + "ai", "AI"));
            sidebar.AddContent(new SidebarButton(prefix + "ai-studio",    UIcons.Star,           "AI Studio"));
            sidebar.AddContent(new SidebarButton(prefix + "nlp-studio",   UIcons.Edit,           "NLP Studio"));
        }

        private void RunReindex(SidebarStatusCard card)
        {
            StopReindex();

            var percent = 0;
            card.SetStatus("Re-indexing · 2.4M nodes").Neutral().Progress(0, "Re-index 0%");

            _timer = window.setInterval(_ =>
            {
                percent += 4;

                if (percent >= 100)
                {
                    StopReindex();
                    card.SetStatus("Index healthy · 2.4M nodes").Success().ClearProgress();
                    return;
                }

                card.Progress(percent, $"Re-index {percent}% · {(100 - percent) / 20 + 1} min left");
            }, 200);
        }

        private void StopReindex()
        {
            if (_timer != 0) window.clearInterval(_timer);
            _timer = 0;
        }

        public HTMLElement Render() => _content.Render();
    }
}
