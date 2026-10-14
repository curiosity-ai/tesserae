using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 70, Icon = UIcons.AppsAdd, Description = "A menu opened at the pointer on right-click")]
    public class ContextMenuSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ContextMenuSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ContextMenuSample), UIcons.Cursor, "A menu that appears on context")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ContextMenu is a flyout component that displays a list of commands triggered by user interaction, such as a right-click or a button press."),
                    TextBlock("It provides a focused set of actions relevant to the current context, helping to keep the main interface clean and uncluttered. It supports nested submenus, dividers, headers, and custom component items."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use ContextMenus to surface secondary actions that are relevant to a specific element. Group related commands using dividers. Use submenus sparingly to avoid deep nesting that can be hard to navigate. Ensure that the menu remains within the viewport when opened. Always provide clear labels and icons for common actions."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Simple Context Menu"),
                    Button("Click for Menu").Var(out var btn1).OnClick((s, e) =>
                        ContextMenu().Items(
                            ContextMenuItem(HStack().Children(Icon(UIcons.Plus), TextBlock("New Item").ML(8))).OnClick((_, __) => Toast().Success("New Item created")),
                            ContextMenuItem(HStack().Children(Icon(UIcons.FolderOpen), TextBlock("Open").ML(8))).OnClick((_, __) => Toast().Information("Opening...")),
                            ContextMenuItem().Divider(),
                            // A HorizontalSeparator in an item is the labelled divider Divider() cannot draw. The
                            // menu takes it as a separator rather than a command: no command-row height, no hover,
                            // and the arrow keys walk past it.
                            ContextMenuItem(HorizontalSeparator("Danger zone").WS()),
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
                    ).MB(16),
                    SampleSubTitle("Three levels"),
                    Button("Deep Menu").Var(out var btn3).OnClick((s, e) =>
                        ContextMenu().Items(
                            ContextMenuItem("Plain"),
                            ContextMenuItem("Level 1").SubMenu(
                                ContextMenu().Items(
                                    ContextMenuItem("Level 2").SubMenu(
                                        ContextMenu().Items(
                                            ContextMenuItem("Leaf A"),
                                            ContextMenuItem("Leaf B"),
                                            ContextMenuItem("Leaf C"))),
                                    ContextMenuItem("Other 2"))),
                            ContextMenuItem("Other 1")
                        ).ShowFor(btn3)
                    ).MB(16),
                    SampleSubTitle("Side panel"),
                    // The panel sits beside the rows in the same popup. It is not a row, so clicking in it keeps the
                    // menu open and the arrow keys walk past it; its own actions close the menu by calling Hide().
                    Button("Menu with details").Var(out var btn4).OnClick((s, e) =>
                    {
                        var menu = ContextMenu();

                        menu.Items(
                                ContextMenuItem(HStack().Children(Icon(UIcons.FolderOpen), TextBlock("Open").ML(8))).OnClick((_, __) => Toast().Information("Opening...")),
                                ContextMenuItem(HStack().Children(Icon(UIcons.Share), TextBlock("Share").ML(8))).OnClick((_, __) => Toast().Information("Sharing...")))
                           .SidePanel(VStack().W(260).P(12).Children(
                                TextBlock("report-2026.pdf").SemiBold(),
                                TextBlock("PDF document - 2.4 MB").Small().Secondary().MB(8),
                                Button("Copy link").Link().NoPadding().OnClick(() => { Toast().Success("Link copied"); menu.Hide(); })))
                           .ShowFor(btn4);
                    }).MB(16),
                    SampleSubTitle("Side panel on right-click"),
                    TextBlock("Right-click the card. The everyday commands stay menu rows; the panel carries the item's details and the rarer actions, as small buttons that wrap.").Small().MB(8),
                    Card(HStack().AlignItemsCenter().Children(
                            Icon(UIcons.FilePdf, size: TextSize.Large).MR(12),
                            VStack().Children(
                                TextBlock("quarterly-report-2026.pdf").SemiBold(),
                                TextBlock("Right-click for commands").Small().Secondary())))
                       .W(360)
                       .OnContextMenu((card, ev) =>
                        {
                            StopEvent(ev);
                            ShowFileMenu((int)ev.clientX, (int)ev.clientY);
                        })
                )).SetTitle("Usage")))
               .SeeAlso(typeof(MenuSample), typeof(PopoverSample), typeof(CommandBarSample), typeof(TippySample), typeof(LayerSample));
        }

        // A menu shaped like the one an application shows for an item it knows more about than a row can say:
        // the commands everyone uses as rows, and a side panel with what the item is and the rarer actions.
        private static void ShowFileMenu(int x, int y)
        {
            var menu = ContextMenu();

            menu.Items(
                ContextMenuItem(HStack().Children(Icon(UIcons.FolderOpen), TextBlock("Open").ML(8))).OnClick((_, __) => Toast().Information("Opening...")),
                ContextMenuItem(HStack().Children(Icon(UIcons.Download), TextBlock("Download").ML(8))).OnClick((_, __) => Toast().Information("Downloading...")),
                ContextMenuItem(HStack().Children(Icon(UIcons.Share), TextBlock("Share").ML(8))).OnClick((_, __) => Toast().Information("Sharing...")),
                ContextMenuItem(HStack().Children(Icon(UIcons.Star), TextBlock("Add to favorites").ML(8))).OnClick((_, __) => Toast().Success("Added to favorites")));

            Button copyId = null;

            copyId = Button("f3K9xQ2mVb7LpW1sZt8RcY").SetIcon(UIcons.Copy, afterText: true).TextLeft().NoMargin().WS().MT(8).Tooltip("Copy the file's id")
               .OnClick(() =>
                {
                    copyId.SetIcon(UIcons.Check, afterText: true);
                    Toast().Success("Id copied");
                });

            var actions = HStack().Wrap().Children(
                new[] { "Rename", "Move to...", "Duplicate", "Version history", "Delete" }
                   .Select(name => (IComponent)Button(name).Tiny().NoMinSize().LessPadding().NoMargin().MR(4).MB(4).OnClick(() =>
                    {
                        menu.Hide();
                        Toast().Information(name);
                    })));

            menu.SidePanel(VStack().W(320).HS().P(12).Children(
                    TextBlock("PDF DOCUMENT").Tiny().Secondary(),
                    HStack().WS().AlignItemsCenter().Children(
                        TextBlock("quarterly-report-2026.pdf").SemiBold().Ellipsis().W(10).Grow(),
                        Button().SetIcon(UIcons.Eye).Tooltip("Preview").OnClick(() => { menu.Hide(); Toast().Information("Preview"); }),
                        Button().SetIcon(UIcons.Info).Tooltip("Properties").OnClick(() => { menu.Hide(); Toast().Information("Properties"); })),
                    TextBlock("2.4 MB - edited May 25").Small().Secondary(),
                    copyId,
                    Empty().Grow(),
                    TextBlock("MORE ACTIONS").Tiny().Secondary().MT(16).MB(4),
                    actions))
               .ShowAt(x, y, 220);
        }

        public HTMLElement Render() => _content.Render();
    }
}
