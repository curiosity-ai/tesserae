using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 101, Icon = UIcons.Apps)]
    public class SidenavSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidenavSample()
        {
            // The right-side Sidebar shows context for the section selected in the left rail.
            var sectionTitle = new SidebarText("title", "Build", textSize: TextSize.Medium, textWeight: TextWeight.SemiBold);

            var sidebar = Sidebar();
            sidebar.AddHeader(sectionTitle);

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

            home.OnClick(()      => SwitchTo(sidenav, sidebar, sectionTitle, "home",      "Home",      FillHomeSection));
            operate.OnClick(()   => SwitchTo(sidenav, sidebar, sectionTitle, "operate",   "Operate",   FillOperateSection));
            build.OnClick(()     => SwitchTo(sidenav, sidebar, sectionTitle, "build",     "Build",     FillBuildSection));
            govern.OnClick(()    => SwitchTo(sidenav, sidebar, sectionTitle, "govern",    "Govern",    FillGovernSection));
            configure.OnClick(() => SwitchTo(sidenav, sidebar, sectionTitle, "configure", "Configure", FillConfigureSection));

            // Avatar at the bottom of the rail
            sidenav.AddFooter(new SidenavButton("user", UIcons.User, "Account").Tooltip("OA — Account"));

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
                        TextBlock("A Sidenav can also be used standalone as the only navigation in a smaller app.").PaddingBottom(8.px()),
                        HStack().WS().H(400).Children(
                            standaloneSidenav.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Standalone")));
        }

        private static void SwitchTo(Sidenav sidenav, Sidebar sidebar, SidebarText title, string identifier, string sectionName, Action<Sidebar> fill)
        {
            sidenav.Select(identifier);
            title.SetText(sectionName);
            sidebar.ClearContent();
            fill(sidebar);
        }

        private static void FillHomeSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("home-sep", "Home"));
            sidebar.AddContent(new SidebarButton("home-overview", UIcons.Apps, "Overview").Selected());
            sidebar.AddContent(new SidebarButton("home-news",     UIcons.Bell, "Notifications"));
        }

        private static void FillOperateSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("op-sep", "Operate"));
            sidebar.AddContent(new SidebarButton("op-monitoring", UIcons.Pulse,    "Monitoring").Selected());
            sidebar.AddContent(new SidebarButton("op-alerts",     UIcons.Bell,     "Alerts"));
            sidebar.AddContent(new SidebarButton("op-logs",       UIcons.List,     "Logs"));
        }

        private static void FillBuildSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("build-sep", "Build"));
            sidebar.AddContent(new SidebarButton("data-sources", UIcons.Database,       "Data Sources").Selected());
            sidebar.AddContent(new SidebarButton("graph-db",     UIcons.DiagramProject, "Graph DB"));
            sidebar.AddContent(new SidebarButton("search-cfg",   UIcons.Search,         "Search config"));
            sidebar.AddContent(new SidebarButton("ai-studio",    UIcons.Star,           "AI Studio"));
            sidebar.AddContent(new SidebarButton("nlp-studio",   UIcons.Edit,           "NLP Studio"));
            sidebar.AddContent(new SidebarButton("endpoints",    UIcons.Link,           "Endpoints"));
            sidebar.AddContent(new SidebarButton("integrations", UIcons.Plug,           "Integrations"));
            sidebar.AddContent(new SidebarButton("interface",    UIcons.Browser,        "Interface"));
        }

        private static void FillGovernSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("gov-sep", "Govern"));
            sidebar.AddContent(new SidebarButton("gov-policies", UIcons.Shield, "Policies").Selected());
            sidebar.AddContent(new SidebarButton("gov-audit",    UIcons.Search, "Audit log"));
        }

        private static void FillConfigureSection(Sidebar sidebar)
        {
            sidebar.AddContent(new SidebarSeparator("cfg-sep", "Configure"));
            sidebar.AddContent(new SidebarButton("cfg-general",  UIcons.Settings, "General").Selected());
            sidebar.AddContent(new SidebarButton("cfg-users",    UIcons.User,     "Users"));
            sidebar.AddContent(new SidebarButton("cfg-billing",  UIcons.Star,     "Billing"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
