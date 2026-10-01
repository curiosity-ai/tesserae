using System;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 50, Icon = UIcons.Apps, Description = "A narrow icon-only navigation rail")]
    public class SidenavSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidenavSample()
        {
            // The right-side Sidebar shows context for the section selected in the left rail. Its header is the
            // section's brand row: the section's glyph, its name and what is in it.
            var sidebar = Sidebar();
            sidebar.AddHeader(SectionBrand(UIcons.Database, "Build", "Data, AI and delivery"));

            // Initial sidebar content (Build section)
            FillBuildSection(sidebar);

            // The left-side icon-only navigation rail (Sidenav)
            var sidenav = Sidenav();

            // Brand / logo at the top
            sidenav.AddHeader(new SidenavButton("brand", UIcons.Rocket, "App").AsBrand().Tooltip("My App"));

            var home      = new SidenavButton("home",      UIcons.Home,      "Home");
            var operate   = new SidenavButton("operate",   UIcons.Pulse,     "Operate").ShowDot().DotDanger();
            var build     = new SidenavButton("build",     UIcons.Database,  "Build").Selected();
            var govern    = new SidenavButton("govern",    UIcons.Shield,    "Govern");
            var configure = new SidenavButton("configure", UIcons.Settings,  "Configure");

            sidenav.AddContent(home);
            sidenav.AddContent(operate);
            sidenav.AddContent(build);
            sidenav.AddContent(govern);
            sidenav.AddContent(configure);

            home.OnClick(()      => SwitchTo(sidenav, sidebar, "home", SectionBrand(UIcons.Home, "Home", "Overview and notifications"), FillHomeSection));
            operate.OnClick(()   => SwitchTo(sidenav, sidebar, "operate", SectionBrand(UIcons.Pulse, "Operate", "Monitoring, alerts and logs"), FillOperateSection));
            build.OnClick(()     => SwitchTo(sidenav, sidebar, "build", SectionBrand(UIcons.Database, "Build", "Data, AI and delivery"), FillBuildSection));
            govern.OnClick(()    => SwitchTo(sidenav, sidebar, "govern", SectionBrand(UIcons.Shield, "Govern", "Policies and audit"), FillGovernSection));
            configure.OnClick(() => SwitchTo(sidenav, sidebar, "configure", SectionBrand(UIcons.Settings, "Configure", "Workspace and account"), FillConfigureSection));

            // Avatar at the bottom of the rail
            sidenav.AddFooter(new SidenavButton("user", UIcons.User, "Account").Tooltip("OA — Account"));

            // The same pair, with a status card heading the sidebar in place of the brand row
            var statusSidenav = Sidenav();
            statusSidenav.AddHeader(new SidenavButton("brand3", UIcons.Rocket, "App").AsBrand().Tooltip("My App"));
            statusSidenav.AddContent(new SidenavButton("home3",      UIcons.Home,     "Home"));
            statusSidenav.AddContent(new SidenavButton("operate3",   UIcons.Pulse,    "Operate"));
            statusSidenav.AddContent(new SidenavButton("build3",     UIcons.Database, "Build").Selected());
            statusSidenav.AddContent(new SidenavButton("govern3",    UIcons.Shield,   "Govern"));
            statusSidenav.AddContent(new SidenavButton("configure3", UIcons.Settings, "Configure"));
            statusSidenav.AddFooter(new SidenavButton("user3", UIcons.User, "Account"));

            var statusSidebar = Sidebar();
            statusSidebar.AddHeader(new SidebarStatusCard("build-status", UIcons.Database, "Build", "Index healthy · 2.4M nodes")
               .Success()
               .Progress(72, "Re-index 72% · 4 min left"));
            FillBuildSection(statusSidebar);

            // Standalone Sidenav demo
            var standaloneSidenav = Sidenav();
            standaloneSidenav.AddHeader(new SidenavButton("brand2", UIcons.Rocket, "App").AsBrand().Tooltip("My App"));
            standaloneSidenav.AddContent(new SidenavButton("dash",  UIcons.Dashboard,      "Dashboard").Selected());
            standaloneSidenav.AddContent(new SidenavButton("docs",  UIcons.Document,       "Docs"));
            standaloneSidenav.AddContent(new SidenavButton("chart", UIcons.ChartHistogram, "Charts"));
            standaloneSidenav.AddContent(new SidenavButton("inbox", UIcons.Envelope,       "Inbox").ShowDot());
            standaloneSidenav.AddFooter(new SidenavButton("settings", UIcons.Settings, "Settings"));
            standaloneSidenav.AddFooter(new SidenavButton("user2",    UIcons.User,     "User"));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidenavSample), UIcons.Apps, "A vertical icon-only navigation rail")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A Sidenav is a narrow, vertical icon navigation rail intended to be used as the leftmost navigation in an application. Each item shows an icon with a small label below it. The Sidenav can be combined with a Sidebar to its right to create a two-level navigation experience.")
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        TextBlock("The Sidenav (left rail) selects the high-level section, and the Sidebar shows context for that section.").PaddingBottom(8.px()),
                        HStack().WS().H(600).Children(
                            sidenav.HS(),
                            sidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Sidenav + Sidebar"),
                    Card(VStack().WS().Children(
                        TextBlock("A SidebarStatusCard can head the Sidebar instead, to say what the section is doing right now.").PaddingBottom(8.px()),
                        HStack().WS().H(600).Children(
                            statusSidenav.HS(),
                            statusSidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("With a status card"),
                    Card(VStack().WS().Children(
                        TextBlock("A Sidenav can also be used standalone as the only navigation in a smaller app.").PaddingBottom(8.px()),
                        HStack().WS().H(400).Children(
                            standaloneSidenav.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Standalone")))
               .SeeAlso(typeof(SidebarSample), typeof(NavbarSample), typeof(MenuSample), typeof(SidebarSeparatorSample), typeof(SidebarStatusCardSample));
        }

        private static SidebarBrand SectionBrand(UIcons icon, string name, string description)
        {
            return new SidebarBrand("section", icon, name, description).Separated();
        }

        private static void SwitchTo(Sidenav sidenav, Sidebar sidebar, string identifier, SidebarBrand brand, Action<Sidebar> fill)
        {
            sidenav.Select(identifier);
            sidebar.ClearHeader();
            sidebar.AddHeader(brand);
            sidebar.ClearContent();
            fill(sidebar);
        }

        // The brand row already names the section, so separators group items within it
        // rather than repeating the section name. Short sections need no separator at all.
        private static void FillHomeSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarButton("home-overview", UIcons.Apps, "Overview").Selected());
            sidebar.AddContent(new SidebarButton("home-news",     UIcons.Bell, "Notifications"));
        }

        private static void FillOperateSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarButton("op-monitoring", UIcons.Pulse,    "Monitoring").Selected());
            sidebar.AddContent(new SidebarButton("op-alerts",     UIcons.Bell,     "Alerts"));
            sidebar.AddContent(new SidebarButton("op-logs",       UIcons.List,     "Logs"));
        }

        private static void FillBuildSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("build-data-sep", "Data"));
            sidebar.AddContent(new SidebarButton("data-sources", UIcons.Database,       "Data Sources").Selected());
            sidebar.AddContent(new SidebarButton("graph-db",     UIcons.DiagramProject, "Graph DB"));
            sidebar.AddContent(new SidebarButton("search-cfg",   UIcons.Search,         "Search config"));
            sidebar.AddContent(new SidebarSeparator("build-ai-sep", "AI"));
            sidebar.AddContent(new SidebarButton("ai-studio",    UIcons.Star,           "AI Studio"));
            sidebar.AddContent(new SidebarButton("nlp-studio",   UIcons.Edit,           "NLP Studio"));
            sidebar.AddContent(new SidebarSeparator("build-delivery-sep", "Delivery"));
            sidebar.AddContent(new SidebarButton("endpoints",    UIcons.Link,           "Endpoints"));
            sidebar.AddContent(new SidebarButton("integrations", UIcons.Plug,           "Integrations"));
            sidebar.AddContent(new SidebarButton("interface",    UIcons.Browser,        "Interface"));
        }

        private static void FillGovernSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarButton("gov-policies", UIcons.Shield, "Policies").Selected());
            sidebar.AddContent(new SidebarButton("gov-audit",    UIcons.Search, "Audit log"));
        }

        private static void FillConfigureSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("cfg-workspace-sep", "Workspace"));
            sidebar.AddContent(new SidebarButton("cfg-general",  UIcons.Settings, "General").Selected());
            sidebar.AddContent(new SidebarButton("cfg-users",    UIcons.User,     "Users"));
            sidebar.AddContent(new SidebarSeparator("cfg-account-sep", "Account"));
            sidebar.AddContent(new SidebarButton("cfg-billing",  UIcons.Star,     "Billing"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
