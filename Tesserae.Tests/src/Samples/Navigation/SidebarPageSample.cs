using System;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 37, Icon = UIcons.Smartphone, Description = "A sidebar and its content taking turns on a phone")]
    public class SidebarPageSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarPageSample()
        {
            var sidebar = Sidebar().AsPage();
            var pageBar = SidebarPageBar(sidebar).Brand(Icon(UIcons.Rocket)).SetTitle("Inbox");
            var body    = TextBlock("Pick something in the sidebar, then use the back button to return to it.").Wrap();

            void Page(string identifier, UIcons icon, string title)
            {
                sidebar.AddContent(new SidebarButton(identifier, icon, title).OnClick(() =>
                {
                    pageBar.SetTitle(title);
                    body.Text = "This is the " + title + " page. The sidebar stepped aside when it was picked.";
                }));
            }

            sidebar.AddHeader(new SidebarText("brand", "My App", "MA", textSize: TextSize.Large, textWeight: TextWeight.Bold));
            Page("inbox",    UIcons.Inbox,    "Inbox");
            Page("calendar", UIcons.Calendar, "Calendar");
            Page("contacts", UIcons.User,     "Contacts");

            // A group opens its children as a panel over the sidebar instead of expanding in place
            var projects = new SidebarNav("projects", UIcons.Folder, "Projects", initiallyCollapsed: true);
            sidebar.AddContent(projects);

            foreach (var project in new[] { "Atlas", "Beacon", "Compass" })
            {
                projects.Add(new SidebarButton("project-" + project, UIcons.Document, project).OnClick(() =>
                {
                    pageBar.SetTitle(project);
                    body.Text = "This is the " + project + " project. Picking it closed the panel and the sidebar.";
                }));
            }
            sidebar.AddFooter(new SidebarButton("settings", UIcons.Settings, "Settings"));

            var content = VStack().S().Children(pageBar, body.Padding(16.px()));

            // A phone-sized frame, so the two pages take turns inside the sample instead of across the gallery
            var phone = HStack().W(360.px()).H(560.px()).Style(s =>
            {
                s.border       = "1px solid " + Theme.Default.Border;
                s.borderRadius = "16px";
                s.overflow     = "hidden";
            }).Children(sidebar.HS(), content.W(1).Grow());

            var mode = Toggle("Render as a page").Checked().OnChange((t, _) => sidebar.AsPage(t.IsChecked));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarPageSample), UIcons.Smartphone, "A sidebar that is a page of its own")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("On a phone there is no room for a rail beside the content. Sidebar.AsPage() makes the sidebar fill its container and hide what follows it; picking one of its buttons steps it aside, a group opens its children as a panel over the sidebar rather than expanding in place, and the SidebarPageBar at the top of the content brings it back. Follow Theme.OnMobileModeChanged to switch between the two layouts.").Wrap())).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        mode.PB(16),
                        phone
               )).SetTitle("Usage")))
               .SeeAlso(typeof(SidebarSample), typeof(SidebarShiftSample), typeof(NavbarSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
