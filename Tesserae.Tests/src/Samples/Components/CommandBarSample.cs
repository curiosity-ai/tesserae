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
               .SampleTitle(nameof(CommandBarSample), UIcons.MenuBurger, "A toolbar for housing commands")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Command Bars provide a surface for common actions related to a specific context, such as a page or a selected item in a list."),
                    TextBlock("They typically contain buttons with icons and labels, and can be split into 'near' items (left-aligned) and 'far' items (right-aligned)."))).SetTitle("Overview")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Command Bars for primary actions that users perform frequently. Keep the number of items manageable; if there are too many, consider using a 'More' menu. Order items by importance or frequency of use. Group related actions together. Use 'far' items for actions that are global to the surface, such as settings or search."))).SetTitle("Best Practices")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Command Bar"),
                    CommandBar(
                        CommandBarItem("New", UIcons.Plus).Primary().OnClick(() => Toast().Success("New item")),
                        CommandBarItem("Edit", UIcons.Edit).OnClick(() => Toast().Success("Edit item")),
                        CommandBarItem("Share", UIcons.Share).OnClick(() => Toast().Success("Share item")),
                        CommandBarItem("Delete", UIcons.Trash).OnClick(() => Toast().Success("Delete item"))
                    ).FarItems(
                        SearchBox().SetPlaceholder("Search...").Width(200.px()),
                        CommandBarItem("Settings", UIcons.Settings).OnClick(() => Toast().Information("Settings clicked"))
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
