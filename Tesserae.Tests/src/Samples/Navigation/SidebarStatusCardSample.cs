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
            // The plain card: a glyph, a name and a status line, with no progress bar and nothing updating it
            var plainSidebar = Sidebar();
            plainSidebar.AddHeader(new SidebarStatusCard("govern-status", UIcons.Shield, "Govern", "All policies passing").Success());
            plainSidebar.AddContent(new SidebarButton("policies", UIcons.Shield,   "Policies").Selected());
            plainSidebar.AddContent(new SidebarButton("audit",    UIcons.Document, "Audit log"));
            plainSidebar.AddContent(new SidebarButton("access",   UIcons.Lock,     "Access"));

            var card = new SidebarStatusCard("build-status", UIcons.Database, "Build", "Index healthy · 2.4M nodes")
               .Success()
               .Progress(72, "Re-index 72% · 4 min left");

            var sidebar = Sidebar();
            sidebar.AddHeader(card);
            sidebar.AddContent(new SidebarSeparator("data", "Data"));
            sidebar.AddContent(new SidebarButton("data-sources", UIcons.Database,       "Data Sources").Selected());
            sidebar.AddContent(new SidebarButton("graph-db",     UIcons.DiagramProject, "Graph DB"));
            sidebar.AddContent(new SidebarButton("search-cfg",   UIcons.Search,         "Search config"));
            sidebar.AddContent(new SidebarSeparator("ai", "AI"));
            sidebar.AddContent(new SidebarButton("ai-studio",    UIcons.Star,           "AI Studio"));
            sidebar.AddContent(new SidebarButton("nlp-studio",   UIcons.Edit,           "NLP Studio"));

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
                        TextBlock("Without a progress bar the card is a glyph, the section's name and a status line with its dot. Leave the status out of the constructor and it is the name alone.").PaddingBottom(8.px()),
                        HStack().WS().H(300).Children(
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
