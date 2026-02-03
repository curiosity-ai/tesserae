using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.MenuDots)]
    public class NavbarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public NavbarSample()
        {
            var navbar = Sidebar().AsNavbar();

            navbar.AddHeader(new SidebarButton("brand", UIcons.Rocket, "My App").Primary());
            navbar.AddHeader(new SidebarButton("dashboard", UIcons.Dashboard, "Dashboard"));

            navbar.AddContent(new SidebarButton("profile", UIcons.User, "Profile"));
            navbar.AddContent(new SidebarButton("settings", UIcons.Settings, "Settings"));
            navbar.AddContent(new SidebarSeparator("sep1"));
            navbar.AddContent(new SidebarButton("logout", UIcons.SignOutAlt, "Logout"));

            navbar.AddFooter(new SidebarButton("footer", UIcons.Info, "About"));

            _content = SectionStack()
               .Title(SampleHeader(nameof(NavbarSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A Sidebar rendered as a Navbar. Header items are inline, others are in a drawer."),
                    SampleTitle("Usage"),
                    VStack().H(500.px()).Children(
                        navbar,
                        TextBlock("Page Content below the navbar...").Padding(16.px())
                    )
               ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
