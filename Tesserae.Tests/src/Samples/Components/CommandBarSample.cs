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
                                CommandBarItem("New",    UIcons.Plus).Primary().OnClick(() => Toast().Success("New item")),
                                CommandBarItem("Edit",   UIcons.Edit).OnClick(() => Toast().Success("Edit item")),
                                CommandBarItem("Share",  UIcons.Share).OnClick(() => Toast().Success("Share item")),
                                CommandBarItem("Delete", UIcons.Trash).OnClick(() => Toast().Success("Delete item")))
                           .FarItems(
                                SearchBox().SetPlaceholder("Search"),
                                CommandBarItem("Settings", UIcons.Settings));

            _content = SectionStack()
               .Title(SampleHeader(nameof(CommandBarSample)))
               .Section(Stack().Children(
                    SampleTitle("Command Bar"),
                    commandBar));
        }

        public HTMLElement Render() => _content.Render();
    }
}
