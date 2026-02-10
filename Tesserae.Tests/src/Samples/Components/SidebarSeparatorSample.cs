using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 99, Icon = UIcons.MenuDots)]
    public class SidebarSeparatorSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarSeparatorSample()
        {
            var sidebar = Sidebar();

            sidebar.AddHeader(new SidebarText("header", "Header"));
            sidebar.AddContent(new SidebarButton("1", UIcons.Home, "Home"));
            sidebar.AddContent(new SidebarSeparator("sep1"));
            sidebar.AddContent(new SidebarButton("2", UIcons.User, "Profile"));
            sidebar.AddContent(new SidebarSeparator("sep2", "More Options"));
            sidebar.AddContent(new SidebarButton("3", UIcons.Settings, "Settings"));

            _content = SectionStack()
               .Title(SampleHeader(nameof(SidebarSeparatorSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("A separator for the Sidebar component to visually group items."),
                    SampleTitle("Usage"),
                    TextBlock("Basic separator:"),
                    Stack().Children(
                        sidebar.S().H(500.px())
                    )
               ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
