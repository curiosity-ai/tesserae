using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 20, Icon = UIcons.AppsAdd)]
    public class ContextMenuSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ContextMenuSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ContextMenuSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ContextMenu is a flyout component that displays a list of commands triggered by user interaction, such as a right-click or a button press."),
                    TextBlock("It provides a focused set of actions relevant to the current context, helping to keep the main interface clean and uncluttered. It supports nested submenus, dividers, headers, and custom component items.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use ContextMenus to surface secondary actions that are relevant to a specific element. Group related commands using dividers. Use submenus sparingly to avoid deep nesting that can be hard to navigate. Ensure that the menu remains within the viewport when opened. Always provide clear labels and icons for common actions.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Simple Context Menu"),
                    Button("Click for Menu").Var(out var btn1).OnClick((s, e) =>
                        ContextMenu().Items(
                            ContextMenuItem(HStack().Children(Icon(UIcons.Plus), TextBlock("New Item").ML(8))).OnClick((_, __) => Toast().Success("New Item created")),
                            ContextMenuItem(HStack().Children(Icon(UIcons.FolderOpen), TextBlock("Open").ML(8))).OnClick((_, __) => Toast().Information("Opening...")),
                            ContextMenuItem().Divider(),
                            ContextMenuItem(HStack().Children(Icon(UIcons.Trash, color: Theme.Danger.Background), TextBlock("Delete").ML(8).Danger())).OnClick((_, __) => Toast().Error("Deleted"))
                        ).ShowFor(btn1)
                    ).MB(16),
                    SampleSubTitle("Menu with Submenus and Headers"),
                    Button("Complex Menu").Var(out var btn2).OnClick((s, e) =>
                        ContextMenu().Items(
                            ContextMenuItem("Actions").Header(),
                            ContextMenuItem(HStack().Children(Icon(UIcons.Edit), TextBlock("Edit").ML(8))).SubMenu(
                                ContextMenu().Items(
                                    ContextMenuItem("Edit Name"),
                                    ContextMenuItem("Edit Permissions"),
                                    ContextMenuItem("Edit Metadata")
                                )
                            ),
                            ContextMenuItem(HStack().Children(Icon(UIcons.Share), TextBlock("Share").ML(8))).SubMenu(
                                ContextMenu().Items(
                                    ContextMenuItem("Copy Link"),
                                    ContextMenuItem("Email Link")
                                )
                            ),
                            ContextMenuItem().Divider(),
                            ContextMenuItem("Advanced").Header(),
                            ContextMenuItem("Properties").Disabled(),
                            ContextMenuItem(HStack().Children(Icon(UIcons.Settings), TextBlock("Settings").ML(8)))
                        ).ShowFor(btn2)
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
