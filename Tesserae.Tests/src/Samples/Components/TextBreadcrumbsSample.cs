using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.MenuDots)]
    public class TextBreadcrumbsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TextBreadcrumbsSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(TextBreadcrumbsSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TextBreadcrumbs are a navigational aid that indicates the current position within a hierarchy. They allow users to understand their context and easily navigate back to higher-level pages."),
                    TextBlock("This component is typically placed at the top of a page, below the main navigation.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use breadcrumbs for applications with deep hierarchical structures. Place them consistently at the top of the content area. Use short, descriptive labels for each level. The last item in the breadcrumb should represent the current page and is typically not clickable. Breadcrumbs should complement, not replace, the primary navigation system.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Breadcrumbs"),
                    TextBreadcrumbs().Items(
                        TextBreadcrumb("Home").OnClick((s, e) => Toast().Information("Home clicked")),
                        TextBreadcrumb("Settings").OnClick((s, e) => Toast().Information("Settings clicked")),
                        TextBreadcrumb("Profile")
                    ).PB(32),
                    SampleSubTitle("Overflow and Collapsing"),
                    TextBlock("When the breadcrumbs exceed the available width, they automatically collapse into an overflow menu."),
                    TextBreadcrumbs().MaxWidth(300.px()).Items(
                        TextBreadcrumb("Root").OnClick((s, e) => Toast().Information("Root")),
                        TextBreadcrumb("Folder 1").OnClick((s, e) => Toast().Information("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => Toast().Information("Folder 2")),
                        TextBreadcrumb("Subfolder A").OnClick((s, e) => Toast().Information("Subfolder A")),
                        TextBreadcrumb("Subfolder B").OnClick((s, e) => Toast().Information("Subfolder B")),
                        TextBreadcrumb("Current File")
                    ).PB(32),
                    SampleSubTitle("Long Breadcrumb List"),
                    TextBreadcrumbs().Items(
                        TextBreadcrumb("Resources"),
                        TextBreadcrumb("Images"),
                        TextBreadcrumb("Icons"),
                        TextBreadcrumb("UIcons"),
                        TextBreadcrumb("Regular"),
                        TextBreadcrumb("Arrows"),
                        TextBreadcrumb("Chevron-Down.png")
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
