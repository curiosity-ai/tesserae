using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 21, Icon = UIcons.BorderTop, Description = "A rule across a sidebar, between any two items")]
    public class SidebarDividerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarDividerSample()
        {
            var sidebar = Sidebar();

            sidebar.AddHeader(new SidebarBrand("brand", UIcons.Rocket, "My App", "Workspace"));
            sidebar.AddHeader(new SidebarDivider("header-divider"));

            sidebar.AddContent(new SidebarButton("home",   UIcons.Home,     "Home").Selected());
            sidebar.AddContent(new SidebarButton("search", UIcons.Search,   "Search"));
            sidebar.AddContent(new SidebarDivider("content-divider"));
            sidebar.AddContent(new SidebarButton("files",  UIcons.Folder,   "Files"));
            sidebar.AddContent(new SidebarButton("inbox",  UIcons.Inbox,    "Inbox"));

            sidebar.AddFooter(new SidebarButton("users",   UIcons.Users,    "Users"));
            sidebar.AddFooter(new SidebarButton("manage",  UIcons.Settings, "Manage"));
            sidebar.AddFooter(new SidebarDivider("footer-divider"));
            sidebar.AddFooter(new SidebarProfile("profile", "Ada Lovelace", "ada@example.com"));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarDividerSample), UIcons.BorderTop, "A rule across a sidebar, between any two items")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A SidebarDivider is a horizontal rule across a Sidebar, run out to the rail's own edges. Add it between any two items in the header, the content or the footer; it draws the same line on the open and the collapsed rail, and it stays put in a sortable sidebar."),
                        TextBlock("Use a SidebarSeparator to leave a gap or label a group instead, and .Separated() on a SidebarBrand, SidebarProfile or SidebarSectionHeader when the line belongs to that row.")
                    )).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        HStack().WS().Children(
                            Button("Toggle sidebar").SetIcon(UIcons.Sidebar).OnClick(() => sidebar.Toggle())).PaddingBottom(8.px()),
                        HStack().WS().H(560).Children(
                            sidebar.HS(),
                            VStack().Grow().HS().Padding(16.px()).Children(
                                Message("Application content goes here")
                            )
                        )
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(SidebarSeparatorSample), typeof(SidebarSample), typeof(SidebarSectionHeaderSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
