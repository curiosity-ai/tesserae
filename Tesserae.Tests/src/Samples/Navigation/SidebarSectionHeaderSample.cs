using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 23, Icon = UIcons.Heading, Description = "Names the section a sidebar is showing")]
    public class SidebarSectionHeaderSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarSectionHeaderSample()
        {
            var header = new SidebarSectionHeader("section", UIcons.Database, "Build", "Data, AI and delivery").Separated();

            var sidebar = Sidebar();
            sidebar.AddHeader(header);
            FillBuild(sidebar);

            var sidenav = Sidenav();
            sidenav.AddHeader(new SidenavButton("brand", UIcons.Rocket, "App").AsBrand().Tooltip("My App"));

            var build  = new SidenavButton("build",  UIcons.Database, "Build").Selected();
            var govern = new SidenavButton("govern", UIcons.Shield,   "Govern");
            sidenav.AddContent(build);
            sidenav.AddContent(govern);
            sidenav.AddFooter(new SidenavButton("user", UIcons.User, "Account"));

            // The header stays where it is and says which section the rows below belong to
            build.OnClick(() =>
            {
                sidenav.Select("build");
                header.SetTitle("Build").SetSubtitle("Data, AI and delivery");
                sidebar.ClearContent();
                FillBuild(sidebar);
            });

            govern.OnClick(() =>
            {
                sidenav.Select("govern");
                header.SetTitle("Govern").SetSubtitle("Policies and audit");
                sidebar.ClearContent();
                sidebar.AddContent(new SidebarButton("policies", UIcons.Shield,   "Policies").Selected());
                sidebar.AddContent(new SidebarButton("audit",    UIcons.Document, "Audit log"));
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarSectionHeaderSample), UIcons.Heading, "Names the section a sidebar is showing")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A SidebarSectionHeader heads a sidebar with the name of the section it is showing: the section's glyph, its name and an optional line saying what is in it. It has the shape of a SidebarBrand, but it is a heading rather than a control, so it does not answer the pointer and carries no commands. .Separated() draws the divider between it and the rows it heads. On the collapsed rail it is the glyph, with the name in its tooltip."),
                        TextBlock("Use a SidebarBrand for the application's own row, which opens Home and carries its settings, and a SidebarStatusCard when the section has something to report that changes while the page is open.")
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        HStack().WS().Children(
                            Button("Toggle sidebar").SetIcon(UIcons.Sidebar).OnClick(() => sidebar.Toggle())).PaddingBottom(8.px()),
                        HStack().WS().H(500).Children(
                            sidenav.HS(),
                            sidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(SidenavSample), typeof(SidebarSample), typeof(SidebarStatusCardSample));
        }

        private static void FillBuild(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("data", "Data"));
            sidebar.AddContent(new SidebarButton("data-sources", UIcons.Database,       "Data Sources").Selected());
            sidebar.AddContent(new SidebarButton("graph-db",     UIcons.DiagramProject, "Graph DB"));
            sidebar.AddContent(new SidebarButton("search-cfg",   UIcons.Search,         "Search config"));
            sidebar.AddContent(new SidebarSeparator("ai", "AI"));
            sidebar.AddContent(new SidebarButton("ai-studio",    UIcons.Star,           "AI Studio"));
            sidebar.AddContent(new SidebarButton("nlp-studio",   UIcons.Edit,           "NLP Studio"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
