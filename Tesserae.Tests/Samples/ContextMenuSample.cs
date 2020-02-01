using System;
using System.Threading;
using System.Threading.Tasks;
using Retyped;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ContextMenuSample : IComponent
    {
        private IComponent _content;

        public ContextMenuSample()
        {
            var d = ContextMenu();
            var msg = TextBlock();
            _content = SectionStack().Title(TextBlock("ContextMenu").XLarge().Bold())
                        .Section(Stack().Children(TextBlock("Overview").MediumPlus(),
                                                 TextBlock("ContextualMenus are lists of commands that are based on the context of selection, mouse hover or keyboard focus. They are one of the most effective and highly used command surfaces, and can be used in a variety of places.")))
                        .Section(Stack().Children(TextBlock("Best Practices").MediumPlus(),
                                                  Stack().Horizontal().Children(Stack().Width(40, Unit.Percents).Children(
                                                    TextBlock("Do").Medium(),
                                                    TextBlock("Use to display commands."),
                                                    TextBlock("Divide groups of commands with rules."),
                                                    TextBlock("Use selection checks without icons."),
                                                    TextBlock("Provide submenus for sets of related commands that aren’t as critical as others.")),
                                                  Stack().Width(40, Unit.Percents).Children(
                                                    TextBlock("Don't").Medium(),
                                                    TextBlock("Use them to display content."),
                                                    TextBlock("Show commands as one large group."),
                                                    TextBlock("Mix checks and icons."),
                                                    TextBlock("Create submenus of submenus.")))))
                        .Section(Stack().Children(TextBlock("Usage").MediumPlus(),
                                                  TextBlock("Basic ContextMenus").Medium(),
                                                  Stack().Width(40, Unit.Percents).Children(
                                                    Label("Standard with Headers").SetContent(
                                                        Button("Open").Var(out var btn2).OnClick((s, e) =>
                                                            ContextMenu().Items(
                                                            ContextMenuItem("New").OnClick((s2,e2) => msg.Text("Clicked: New")),
                                                            ContextMenuItem().Divider(), 
                                                            ContextMenuItem("Edit").OnClick((s2, e2) => msg.Text("Clicked: Edit")),
                                                            ContextMenuItem("Properties").OnClick((s2, e2) => msg.Text("Clicked: Properties")),
                                                            ContextMenuItem("Header").Header(),
                                                            ContextMenuItem("Disabled").Disabled(),
                                                            ContextMenuItem("Link").OnClick((s2, e2) => msg.Text("Clicked: Link"))
                                                            ).ShowFor(btn2)
                                            )), msg)));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
