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
                    TextBlock("TextBreadcrumbs should be used as a navigational aid in your app or site. They indicate the current page’s location within a hierarchy and help the user understand where they are in relation to the rest of that hierarchy. They also afford one-click access to higher levels of that hierarchy."),
                    TextBlock("TextBreadcrumbs are typically placed, in horizontal form, under the masthead or navigation of an experience, above the primary content area.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    HStack().Children(
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("Place TextBreadcrumbs at the top of a page, above a list of items, or above the main content of a page.")
                        ),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Don't use TextBreadcrumbs as a primary way to navigate an app or site.")))))
               .Section(Stack().WS().Children(
                    SampleTitle("Usage"),
                    Label("Selected: ").SetContent(TextBlock().Var(out var msg)),
                    TextBlock("All Visible").Medium(),
                    TextBreadcrumbs().MaxWidth(100.percent()).PaddingTop(16.px()).PaddingBottom(16.px()).Items(
                        TextBreadcrumb("Folder 1").OnClick((s, e) => msg.Text("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => msg.Text("Folder 2")),
                        TextBreadcrumb("Folder 3").OnClick((s, e) => msg.Text("Folder 3")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 5").OnClick((s, e) => msg.Text("Folder 5")),
                        TextBreadcrumb("Folder 6").OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("All Visible, Small").Medium(),
                    TextBreadcrumbs().MaxWidth(100.percent()).PaddingTop(16.px()).PaddingBottom(16.px()).Items(
                        TextBreadcrumb("Folder 1").OnClick((s, e) => msg.Text("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => msg.Text("Folder 2")),
                        TextBreadcrumb("Folder 3").OnClick((s, e) => msg.Text("Folder 3")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 5").OnClick((s, e) => msg.Text("Folder 5")),
                        TextBreadcrumb("Folder 6").OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("Collapse 200px").Medium(),
                    TextBreadcrumbs().MaxWidth(100.percent()).PaddingTop(16.px()).PaddingBottom(16.px()).MaxWidth(200.px()).Items(
                        TextBreadcrumb("Folder 1").OnClick((s, e) => msg.Text("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => msg.Text("Folder 2")),
                        TextBreadcrumb("Folder 3").OnClick((s, e) => msg.Text("Folder 3")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 5").OnClick((s, e) => msg.Text("Folder 5")),
                        TextBreadcrumb("Folder 6").OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("Collapse 200px, Small").Medium(),
                    TextBreadcrumbs().MaxWidth(100.percent()).PaddingTop(16.px()).PaddingBottom(16.px()).MaxWidth(200.px()).Items(
                        TextBreadcrumb("Folder 1").OnClick((s, e) => msg.Text("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => msg.Text("Folder 2")),
                        TextBreadcrumb("Folder 3").OnClick((s, e) => msg.Text("Folder 3")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 5").OnClick((s, e) => msg.Text("Folder 5")),
                        TextBreadcrumb("Folder 6").OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("Collapse 300px").Medium(),
                    TextBreadcrumbs().MaxWidth(100.percent()).PaddingTop(16.px()).PaddingBottom(16.px()).MaxWidth(300.px()).Items(
                        TextBreadcrumb("Folder 1").OnClick((s, e) => msg.Text("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => msg.Text("Folder 2")),
                        TextBreadcrumb("Folder 3").OnClick((s, e) => msg.Text("Folder 3")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 5").OnClick((s, e) => msg.Text("Folder 5")),
                        TextBreadcrumb("Folder 6").OnClick((s, e) => msg.Text("Folder 6"))),
                    TextBlock("Collapse 300px, from second, custom chevron").Medium(),
                    TextBreadcrumbs().MaxWidth(100.percent()).PaddingTop(16.px()).PaddingBottom(16.px()).MaxWidth(300.px()).Items(
                        TextBreadcrumb("Folder 1").OnClick((s, e) => msg.Text("Folder 1")),
                        TextBreadcrumb("Folder 2").OnClick((s, e) => msg.Text("Folder 2")),
                        TextBreadcrumb("Folder 3").OnClick((s, e) => msg.Text("Folder 3")),
                        TextBreadcrumb("Folder 4").OnClick((s, e) => msg.Text("Folder 4")),
                        TextBreadcrumb("Folder 5").OnClick((s, e) => msg.Text("Folder 5")),
                        TextBreadcrumb("Folder 6").OnClick((s, e) => msg.Text("Folder 6")))
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}