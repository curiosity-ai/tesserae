using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    public class ContextMenuSample : IComponent
    {
        private readonly IComponent _content;

        public ContextMenuSample()
        {
            var d    = ContextMenu();
            var msg  = TextBlock();
            var msg2 = TextBlock();

            var cmsub = ContextMenu().Items(
                ContextMenuItem().Divider(),
                ContextMenuItem("Edit Sub").OnClick((s2,       e2) => Toast().Information("Clicked: Edit Sub")),
                ContextMenuItem("Properties Sub").OnClick((s2, e2) => Toast().Information("Clicked: Properties Sub")),
                ContextMenuItem("Header Sub").Header(),
                ContextMenuItem("Disabled Sub").Disabled(),
                ContextMenuItem("Link Sub").OnClick((s2, e2) => Toast().Information("Clicked: Link Sub")));

            var cmsub2 = ContextMenu().Items(
                ContextMenuItem().Divider(),
                ContextMenuItem("Edit Sub2").OnClick((s2, e2) => Toast().Information("Clicked: Edit Sub2")),
                ContextMenuItem("Properties Sub2"),
                ContextMenuItem("Header Sub2").Header(),
                ContextMenuItem("Disabled Sub2").Disabled(),
                ContextMenuItem("Link Sub2").OnClick((s2, e2) => Toast().Information("Clicked: Link Sub2")));


            var cmcm = ContextMenu().Items(
                ContextMenuItem(Link("#", "New")).SubMenu(cmsub),
                ContextMenuItem().Divider(),
                ContextMenuItem(Button("All").Compact().Link())
                   .OnClick((_, __) =>
                    {
                        Toast().Information("Clicked: All");
                    }),
                ContextMenuItem(Button("Edit").Compact().Link().SetIcon(LineAwesome.Edit)).OnClick((s2, e2) => Toast().Information("Clicked: Edit")),
                ContextMenuItem(Button("Properties").Compact().Link().SetIcon(LineAwesome.ExpandArrowsAlt)).SubMenu(cmsub2),
                ContextMenuItem("Header").Header(),
                ContextMenuItem("Disabled").Disabled(),
                ContextMenuItem("Link").OnClick((s2, e2) => Toast().Information("Clicked: Link"))
            );


            _content = SectionStack()
               .Title(SampleHeader(nameof(ContextMenuSample)))
               .Section(Stack().Children(SampleTitle("Overview"),
                    TextBlock("ContextualMenus are lists of commands that are based on the context of selection, mouse hover or keyboard focus. They are one of the most effective and highly used command surfaces, and can be used in a variety of places.")))
               .Section(Stack().Children(SampleTitle("Best Practices"),
                    Stack().Horizontal().Children(Stack().Width(40.percent()).Children(
                            SampleSubTitle("Do"),
                            SampleDo("Use to display commands."),
                            SampleDo("Divide groups of commands with rules."),
                            SampleDo("Use selection checks without icons."),
                            SampleDo("Provide submenus for sets of related commands that aren’t as critical as others.")),
                        Stack().Width(40.percent()).Children(
                            SampleSubTitle("Don't"),
                            SampleDont("Use them to display content."),
                            SampleDont("Show commands as one large group."),
                            SampleDont("Mix checks and icons."),
                            SampleDont("Create submenus of submenus.")))))
               .Section(Stack().Children(SampleTitle("Usage"),
                    TextBlock("Basic ContextMenus").Medium(),
                    HStack().Children(
                        Stack().Children(
                            Label("Standard with Headers").SetContent(
                                Button("Open").Var(out var btn2).OnClick((s, e) =>
                                    ContextMenu().Items(
                                        ContextMenuItem("New").OnClick((s2, e2) => Toast().Information("Clicked: New")),
                                        ContextMenuItem().Divider(),
                                        ContextMenuItem("Edit").OnClick((s2,       e2) => Toast().Information("Clicked: Edit")),
                                        ContextMenuItem("Properties").OnClick((s2, e2) => Toast().Information("Clicked: Properties")),
                                        ContextMenuItem("Header").Header(),
                                        ContextMenuItem("Disabled").Disabled(),
                                        ContextMenuItem("Link").OnClick((s2, e2) => Toast().Information("Clicked: Link"))
                                    ).ShowFor(btn2)
                                )), msg),
                        Stack().Children(
                            Label("Standard with Submenus").SetContent(
                                Button("Open").Var(out var btn3).OnClick((s, e) =>
                                    cmcm.ShowFor(btn3)
                                )), msg2
                        )
                    )
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}