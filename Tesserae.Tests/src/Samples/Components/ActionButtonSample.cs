using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 5, Icon = UIcons.CursorFingerClick)]
    public class ActionButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ActionButtonSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ActionButtonSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ActionButtons are a variation of the standard Button component that split the interaction into two distinct parts: a display area (typically the label and an icon) and a specific action area (typically a secondary icon on the right)."),
                    TextBlock("They are useful when you want to provide a primary action while also offering a secondary, related action like opening a menu, showing a tooltip, or triggering a specific sub-task.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use ActionButtons when a component needs to perform more than one related task. The primary area should trigger the most common action, while the secondary area (the action icon) should trigger a complementary one. Clearly distinguish between the two areas visually if they perform very different tasks. Ensure that both interaction points have appropriate tooltips or labels if their purpose isn't immediately obvious.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Action Buttons"),
                    VStack().Children(
                        ActionButton("Standard Action").Var(out var btn1)
                           .OnClickDisplay((s, e) => Toast().Information("Clicked display!"))
                           .OnClickAction((s,  e) => Toast().Information("Clicked action icon!")),
                        ActionButton("Primary with Calendar Icon", actionIcon: UIcons.Calendar).Primary()
                           .OnClickDisplay((s, e) => Toast().Success("Main area clicked"))
                           .OnClickAction((s,  e) => Toast().Success("Calendar icon clicked")),
                        ActionButton("Danger Action", displayIcon: UIcons.TriangleWarning).Danger()
                           .OnClickDisplay((s, e) => Toast().Error("Danger area clicked"))
                           .OnClickAction((s,  e) => Toast().Error("Warning icon clicked"))
                    ),
                    SampleSubTitle("Complex Content"),
                    VStack().Children(
                        ActionButton(VStack().Children(
                            HStack().AlignItemsCenter().Children(Icon(UIcons.Arrows), TextBlock("Move Item").SemiBold().PL(8)),
                            TextBlock("Use this to reorganize your workspace").Tiny().PT(4)
                        ))
                        .OnClickDisplay((s, e) => Toast().Information("Moving..."))
                        .OnClickAction((s,  e) => Toast().Information("Configure move...")),
                        ActionButton("Action with Custom Tooltip").ModifyActionButton(btn =>
                        {
                            Raw(btn).Tooltip("This tooltip is applied to the entire component");
                        })
                    ),
                    SampleSubTitle("Dropdown Simulation"),
                    ActionButton("Show Options", actionIcon: UIcons.AngleDown).Primary()
                       .OnClickAction((s, e) =>
                        {
                            Action hideAction = null;
                            var menu = VStack().Children(
                                Button("Option 1").OnClick(() => { Toast().Information("Option 1"); hideAction?.Invoke(); }),
                                Button("Option 2").OnClick(() => { Toast().Information("Option 2"); hideAction?.Invoke(); }),
                                Button("Option 3").OnClick(() => { Toast().Information("Option 3"); hideAction?.Invoke(); })
                            ).Render();
                            Tippy.ShowFor(s, menu, out hideAction, TooltipAnimation.None, TooltipPlacement.BottomEnd, 0, 0, 350, true, null);
                        })
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
