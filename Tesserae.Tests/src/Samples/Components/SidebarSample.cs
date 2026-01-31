using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.MenuDots)]
    public class SidebarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarSample()
        {
            var sidebar = Sidebar();

            var searchBox = new SidebarSearchBox("search", "Search Sidebar...");
            searchBox.OnSearch((term) => sidebar.Search(term));

            sidebar.AddHeader(searchBox);

            sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home"));
            sidebar.AddContent(new SidebarButton("profile", UIcons.User, "Profile"));

            sidebar.AddContent(new SidebarSeparator("sep1", "Grouping"));

            var settingsNav = new SidebarNav("settings", UIcons.Settings, "Settings", true);
            settingsNav.Add(new SidebarButton("general", UIcons.Settings, "General"));
            settingsNav.Add(new SidebarButton("security", UIcons.Lock, "Security"));
            settingsNav.Add(new SidebarButton("privacy", UIcons.Eye, "Privacy"));

            sidebar.AddContent(settingsNav);

            sidebar.AddContent(new SidebarSeparator("sep2"));

            sidebar.AddContent(new SidebarButton("help", UIcons.Question, "Help"));

            _content = SectionStack()
               .Title(SampleHeader(nameof(SidebarSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A fully featured Sidebar with Search, Navigation, Buttons, and Separators."),
                    SampleTitle("Usage"),
                    Stack().Children(
                        sidebar.S().H(600.px())
                    )
               ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
