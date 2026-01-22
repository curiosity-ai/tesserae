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
            var commandBar = CommandBar(
                CommandBarItem("New", Icon.Transform(UIcons.Plus, UIconsWeight.Regular)).Primary().OnClick(() => Toast().Success("New item")),
                CommandBarItem("Edit", Icon.Transform(UIcons.Edit, UIconsWeight.Regular)).OnClick(() => Toast().Success("Edit item")),
                CommandBarItem("Share", Icon.Transform(UIcons.Share, UIconsWeight.Regular)).OnClick(() => Toast().Success("Share item")),
               CommandBarItem("Delete", Icon.Transform(UIcons.Trash, UIconsWeight.Regular)).OnClick(() => Toast().Success("Delete item")))
               .FarItems(
                    SearchBox().SetPlaceholder("Search"),
                    CommandBarItem("Settings", Icon.Transform(UIcons.Settings, UIconsWeight.Regular)));

            _content = SectionStack()
               .Title(SampleHeader(nameof(CommandBarSample)))
               .Section(Stack().Children(
                    SampleTitle("Command Bar"),
                    commandBar));
        }

        public HTMLElement Render() => _content.Render();
    }
}
