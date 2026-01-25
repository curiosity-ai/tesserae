using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.MenuDots)]
    public class CommandBarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CommandBarSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(CommandBarSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Command Bars provide a surface for common actions related to a specific context, such as a page or a selected item in a list."),
                    TextBlock("They typically contain buttons with icons and labels, and can be split into 'near' items (left-aligned) and 'far' items (right-aligned).")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Command Bars for primary actions that users perform frequently. Keep the number of items manageable; if there are too many, consider using a 'More' menu. Order items by importance or frequency of use. Group related actions together. Use 'far' items for actions that are global to the surface, such as settings or search.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Command Bar"),
                    CommandBar(
                        CommandBarItem("New", UIcons.Plus).Primary().OnClick(() => Toast().Success("New item")),
                        CommandBarItem("Edit", UIcons.Edit).OnClick(() => Toast().Success("Edit item")),
                        CommandBarItem("Share", UIcons.Share).OnClick(() => Toast().Success("Share item")),
                        CommandBarItem("Delete", UIcons.Trash).OnClick(() => Toast().Success("Delete item"))
                    ).FarItems(
                        SearchBox().SetPlaceholder("Search...").Width(200.px()),
                        CommandBarItem("Settings", UIcons.Settings).OnClick(() => Toast().Information("Settings clicked"))
                    ),
                    SampleSubTitle("Contextual Items"),
                    TextBlock("Command bars can be updated dynamically based on selection. Here is one with mixed item types."),
                    CommandBar(
                        CommandBarItem("Refresh", UIcons.Refresh).OnClick(() => Toast().Information("Refreshing...")),
                        CommandBarItem("Filter", UIcons.Filter).OnClick(() => Toast().Information("Filter opened")),
                        CommandBarItem("Download", UIcons.Download).Disabled()
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
