using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 30, Icon = UIcons.Sitemap, Description = "A tree of links, its branches loaded on demand")]
    public class NavSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public NavSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(NavSample), UIcons.Sitemap, "A tree of navigation links")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Nav is a vertical tree of links: each link can hold child links, which it shows when expanded. One link at a time is selected, and selecting a link raises its OnSelected handler."),
                    TextBlock("A branch whose children come from somewhere slow can load them on demand with LinksAsync: the children are fetched the first time the link is expanded, and until they arrive the branch shows a loading row with the toolkit's spinner."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Keep the tree shallow, two or three levels, and give each link a short label. Load a branch with LinksAsync when its children are expensive to compute or fetch, so the tree opens immediately and only the branches a user opens cost anything. A sidebar of pages for an application shell is usually a Sidebar rather than a Nav."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Nav"),
                    Nav().Links(
                        NavLink("Home").Selected().OnSelected(s => Toast().Information("Home")),
                        NavLink("Documents").Expanded().Links(
                            NavLink("Reports").OnSelected(s => Toast().Information("Reports")),
                            NavLink("Invoices").OnSelected(s => Toast().Information("Invoices"))),
                        NavLink("Settings").Links(
                            NavLink("Profile"),
                            NavLink("Security"))
                    ).MaxWidth(320.px()).PB(16),
                    SampleSubTitle("Children loaded on demand (LinksAsync)"),
                    TextBlock("Expand \"Projects\": its children take two seconds to arrive, and the branch shows a loading row until they do. They are fetched once, the first time the link is expanded."),
                    Nav().Links(
                        NavLink("Projects").LinksAsync(async () =>
                        {
                            await Task.Delay(2_000);
                            return new[]
                            {
                                NavLink("Tesserae"),
                                NavLink("Transpose"),
                                NavLink("Documentation")
                            };
                        }),
                        NavLink("Archive")
                    ).MaxWidth(320.px()).PB(16),
                    SampleSubTitle("Expanded from the start"),
                    TextBlock("A branch that starts expanded loads its children straight away (here it takes a few seconds, so the loading row stays in view)."),
                    Nav().Links(
                        NavLink("Recent searches").LinksAsync(async () =>
                        {
                            await Task.Delay(6_000);
                            return new[]
                            {
                                NavLink("Shipments arriving late"),
                                NavLink("Open invoices over 30 days")
                            };
                        }).Expanded()
                    ).MaxWidth(320.px()).PB(16),
                    SampleSubTitle("Compact"),
                    Nav().Compact().Links(
                        NavLink("Overview").Selected(),
                        NavLink("Activity"),
                        NavLink("Members")
                    ).MaxWidth(320.px())
                )).SetTitle("Usage")))
               .SeeAlso(typeof(SidebarSample), typeof(TreeSample), typeof(BreadcrumbSample), typeof(NavbarSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
