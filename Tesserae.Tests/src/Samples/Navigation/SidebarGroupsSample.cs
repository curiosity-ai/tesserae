using System;
using System.Collections.Generic;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 15, Icon = UIcons.ListTree, Description = "Groups of pages in a sidebar, open and collapsed")]
    public class SidebarGroupsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarGroupsSample()
        {
            var sidebar = Sidebar();
            var title   = TextBlock("Home").Large().SemiBold();
            var body    = TextBlock("Pick a page in the sidebar. The row you picked carries the current marker; close the group over it and the marker climbs to the group's header.").Wrap();

            // One current page at a time, whichever row was picked last
            var pages = new List<SidebarButton>();

            SidebarButton Page(string identifier, UIcons icon, string name)
            {
                var page = new SidebarButton(identifier, icon, name);

                page.OnClick(() =>
                {
                    foreach (var p in pages) p.IsSelected = ReferenceEquals(p, page);

                    title.Text = name;
                    body.Text  = "This is the " + name + " page.";
                });

                pages.Add(page);
                return page;
            }

            // The brand row is also the rail's open/close control: the « on the row collapses it, the logo opens it
            sidebar.AddHeader(new SidebarBrand("brand", "Aurelia Ops", "Fleet Europe", "./assets/img/curiosity-logo.svg")
               .Separated()
               .WithSidebarControl(onOpen: () => sidebar.IsClosed = false, onClose: () => sidebar.IsClosed = true));

            var home = Page("home", UIcons.Home, "Home").Selected();

            sidebar.AddContent(home);
            sidebar.AddContent(Page("search", UIcons.Search, "Search"));

            // A group: pressing its header or its arrow opens and closes it. A group inside a group is a second level.
            var workspaces = new SidebarNav("workspaces", UIcons.Folder, "Workspaces", initiallyCollapsed: false);
            workspaces.Add(Page("engineering", UIcons.Tools,    "Engineering"));
            workspaces.Add(Page("sales",       UIcons.ChartPie, "Sales"));

            var support = new SidebarNav("support", UIcons.Headset, "Support", initiallyCollapsed: true);
            support.Add(Page("tickets", UIcons.Ticket,   "Tickets"));
            support.Add(Page("faq",     UIcons.Question, "FAQ"));
            workspaces.Add(support);

            sidebar.AddContent(workspaces);

            var sources = new SidebarNav("sources", UIcons.Database, "Data sources", initiallyCollapsed: true);
            sources.Add(Page("confluence", UIcons.BookOpenCover, "Confluence"));
            sources.Add(Page("crm",        UIcons.Users,         "CRM"));
            sources.Add(Page("drive",      UIcons.Cloud,         "Drive"));

            sidebar.AddContent(sources);
            sidebar.AddContent(Page("analytics", UIcons.ChartHistogram, "Analytics"));

            sidebar.AddFooter(Page("settings", UIcons.Settings, "Settings"));

            var content = VStack().S().P(24).Children(title.PB(8), body);

            var shell = HStack().WS().H(720.px()).Style(s =>
            {
                s.border   = "1px solid " + Theme.Default.Border;
                s.overflow = "hidden";
            }).Children(sidebar.HS(), content.W(1).Grow());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarGroupsSample), UIcons.ListTree, "Pages grouped in a sidebar")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A SidebarNav is a group of pages. Pressing its header opens and closes it, in the open rail and in the collapsed one alike, and a group may hold another group. Mark the row the user is on with .Selected() and nothing else: a group that is closed over the current page shows the marker on its own header, so the rail always shows exactly one, on the deepest visible ancestor of the page. A group that is a page of its own takes an .OnClick(...) instead, and then opens from its arrow.").Wrap(),
                        TextBlock("Collapse the rail with the « on the brand row to see the groups as tiles: a group's tile carries its open or closed state in its corner, and its children keep the same tile size, marked by a band on their leading edge for each level. The Theme nav at the foot of the gallery's own sidebar switches the look.").Wrap())).SetTitle("Overview"),
                    Card(VStack().WS().Children(shell)).SetTitle("Usage")))
               .SeeAlso(typeof(SidebarSample), typeof(SidebarPageSample), typeof(SidebarSeparatorSample), typeof(SidenavSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
