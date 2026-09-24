using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 60, Icon = UIcons.MenuBurger, Description = "A dropdown menu with nested submenus")]
    public class MenuSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MenuSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MenuSample), UIcons.MenuDots, "A dropdown menu with nested submenus")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Menu is built on the Popover primitive and is the recommended component for application-style dropdown menus opened from a button click. Unlike the right-click ContextMenu (which uses precise mouse-tracking), Menu has no artificial limit on submenu depth."),
                    TextBlock("It supports headers, dividers, icons, disabled items and arbitrarily deep nested submenus."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("File menu"),
                    Button("File").Var(out var fileBtn).OnClick((s, _) =>
                        Menu().Items(
                            MenuHeader("File"),
                            MenuItem("New", UIcons.Plus).OnClick(() => Toast().Information("New")),
                            MenuItem("Open…", UIcons.FolderOpen).OnClick(() => Toast().Information("Open…")),
                            MenuItem("Save", UIcons.Disk).OnClick(() => Toast().Information("Save")),
                            MenuDivider(),
                            MenuItem("Export").SubMenu(
                                Menu().Items(
                                    MenuItem("PDF").OnClick(() => Toast().Information("Export PDF")),
                                    MenuItem("CSV").OnClick(() => Toast().Information("Export CSV")),
                                    MenuItem("More formats").SubMenu(
                                        Menu().Items(
                                            MenuItem("Markdown").OnClick(() => Toast().Information("Export Markdown")),
                                            MenuItem("JSON").OnClick(() => Toast().Information("Export JSON")),
                                            MenuItem("XML").OnClick(() => Toast().Information("Export XML"))
                                        )))),
                            MenuDivider(),
                            MenuItem("Exit").Disabled()
                        ).ShowFor(fileBtn))
                    ,
                    SampleSubTitle("Sibling submenus"),
                    TextBlock("Three rows that each open a submenu. Moving diagonally from a row into its submenu crosses the rows below it without switching; coming to rest on one of them switches after a moment."),
                    Button("Siblings").Var(out var sibBtn).OnClick((s, _) =>
                        Menu().Items(
                            MenuItem("Alpha").SubMenu(Menu().Items(MenuItem("Alpha 1"), MenuItem("Alpha 2"), MenuItem("Alpha 3"), MenuItem("Alpha 4"), MenuItem("Alpha 5"), MenuItem("Alpha 6"))),
                            MenuItem("Beta").SubMenu(Menu().Items(MenuItem("Beta 1"), MenuItem("Beta 2"), MenuItem("Beta 3"))),
                            MenuItem("Gamma").SubMenu(Menu().Items(MenuItem("Gamma 1"), MenuItem("Gamma 2"), MenuItem("Gamma 3")))
                        ).ShowFor(sibBtn))
                )).SetTitle("Usage")))
               .SeeAlso(typeof(ContextMenuSample), typeof(DropdownSample), typeof(CommandBarSample), typeof(NavbarSample), typeof(SidebarSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
