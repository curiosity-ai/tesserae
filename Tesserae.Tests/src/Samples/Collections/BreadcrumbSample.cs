using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.MenuDots)]
    public class BreadcrumbSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public BreadcrumbSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(BreadcrumbSample), UIcons.AngleRight, "A breadcrumb navigation component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Breadcrumbs provide a secondary navigation system that reveals a user's location in a website or web app. They allow for one-click access to any higher level in the hierarchy."),
                    TextBlock("Unlike TextBreadcrumbs, this component supports more advanced configuration like custom chevrons, overflow indices, and different sizes."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Place breadcrumbs at the top of the page, above the primary content. Use them when the site hierarchy is at least two levels deep. Each breadcrumb item should represent a page or a container. The last item should represent the current location and be non-clickable. Ensure that the breadcrumbs collapse gracefully on smaller screens or when space is limited."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Breadcrumbs"),
                    Breadcrumb().Items(
                        Crumb("Home").OnClick((s, e) => Toast().Information("Home")),
                        Crumb("Project A").OnClick((s, e) => Toast().Information("Project A")),
                        Crumb("Subfolder 1").OnClick((s, e) => Toast().Information("Subfolder 1")),
                        Crumb("Current Page")
                    ).PB(16),
                    SampleSubTitle("Responsive and Collapsed"),
                    TextBlock("Breadcrumbs will collapse when the container width is restricted."),
                    Breadcrumb().MaxWidth(250.px()).Items(
                        Crumb("Root"),
                        Crumb("Level 1"),
                        Crumb("Level 2"),
                        Crumb("Level 3"),
                        Crumb("Final")
                    ).PB(16),
                    SampleSubTitle("Small Size and Custom Chevron"),
                    Breadcrumb().Small().SetChevron(UIcons.AngleRight).Items(
                        Crumb("Resources"),
                        Crumb("Icons"),
                        Crumb("UIcons")
                    ).PB(16),
                    SampleSubTitle("Overflow Configuration"),
                    TextBlock("You can control where the overflow starts (e.g., after the second item)."),
                    Breadcrumb().SetOverflowIndex(1).MaxWidth(200.px()).Items(
                        Crumb("Home"),
                        Crumb("App"),
                        Crumb("Module"),
                        Crumb("Feature"),
                        Crumb("Detail")
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
