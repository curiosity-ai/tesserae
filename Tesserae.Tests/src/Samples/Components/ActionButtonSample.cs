using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.Cursor)]
    public class ActionButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ActionButtonSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ActionButtonSample)))
               .Section(Stack().Children(
                        SampleTitle("Overview")
//                    TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."),
//                    TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements."),
//                    TextBlock("While buttons can technically be used to navigate a user to another part of the experience, this is not recommended unless that navigation is part of an action or their flow.")
                    )
                )
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Best Practices"),
                    Stack().WidthStretch().Horizontal().Children(
                        Stack().WidthStretch().Children(
                            SampleSubTitle("Do")
//                            SampleDo("Make sure the label conveys a clear purpose of the button to the user."),
//                            SampleDo("Button labels must describe the action the button will perform and should include a verb. Use concise, specific, self-explanatory labels, usually a single word."),
//                            SampleDo("Buttons should always include a noun if there is any room for interpretation about what the verb operates on."),
//                            SampleDo("Consider the affect localization will have on the button and what will happen to components around it."),
//                            SampleDo("If the button’s label content is dynamic, consider how the button will resize and what will happen to components around it."),
//                            SampleDo("Use only a single line of text in the label of the button."),
//                            SampleDo("Expose only one or two buttons to the user at a time, for example, \"Accept\" and \"Cancel\". If you need to expose more actions to the user, consider using checkboxes or radio buttons from which the user can select actions, with a single command button to trigger those actions."),
//                            SampleDo("Show only one primary button that inherits theme color at rest state. In the event there are more than two buttons with equal priority, all buttons should have neutral backgrounds."),
//                            SampleDo("\"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons. When \"Reset\" or \"Cancel\" buttons appear alongside one of the above, they should be styled as secondary buttons."),
//                            SampleDo("Default buttons should always perform safe operations. For example, a default button should never delete."),
//                            SampleDo("Use task buttons to cause actions that complete a task or cause a transitional task. Do not use buttons to toggle other UX in the same context. For example, a button may be used to open an interface area but should not be used to open an additional set of components in the same interface.")
                        ),
                        Stack().WidthStretch().Children(
                            SampleSubTitle("Don't")
//                            SampleDont("Don't use generic labels like \"Ok, \" especially in the case of an error; errors are never \"Ok.\""),
//                            SampleDont("Don’t place the default focus on a button that destroys data. Instead, place the default focus on the button that performs the \"safe act\" and retains the content (i.e. \"Save\") or cancels the action (i.e. \"Cancel\")."),
//                            SampleDont("Don’t use a button to navigate to another place, use a link instead. The exception is in a wizard where \"Back\" and \"Next\" buttons may be used."),
//                            SampleDont("Don’t put too much text in a button - try to keep the length of your text to a minimum."),
//                            SampleDont("Don't put anything other than text in a button.")
                        ))
                ))
               .Section(
                    Stack().Children(
                        SampleTitle("Usage"),
                        TextBlock("Default ActionButton").Medium(),
                        VStack().Children(
                            ActionButton("Action1").Var(out var btn1) //.SetText("Standard").Tooltip("This is a standard button")
                               .OnClickDisplay((s, e) => alert("Clicked1 display!"))
                               .OnClickAction((s,  e) => alert("Clicked1 action!")),
                            ActionButton("Action11", "fi-rr-calendar").Var(out var btn11).Primary() //.SetText("Standard").Tooltip("This is a standard button")
                               .OnClickDisplay((s, e) => alert("Clicked1 display!"))
                               .OnClickAction((s,  e) => alert("Clicked1 action!")),
                            ActionButton("Action  2", displayIcon: UIcons.Calendar).Var(out var btn2).Danger() //.SetText("Standard").Tooltip("This is a standard button")
                               .OnClickDisplay((s, e) => alert("Clicked2 display!"))
                               .OnClickAction((s,  e) => alert("Clicked2 action!")),
                            ActionButton("Action 3 but it is a really long button text").Var(out var btn3).Primary() //.SetText("Standard").Tooltip("This is a standard button")
                               .OnClickDisplay((s, e) => alert("Clicked3 display!"))
                               .OnClickAction((s, e) =>
                                {

                                    Action hideAction = null;

                                    var tt = VStack().Children(
                                        Button("1").OnClick(() =>
                                        {
                                            Toast().Information("1");
                                            hideAction?.Invoke();
                                        }),
                                        Button("2").OnClick(() =>
                                        {
                                            Toast().Information("2");
                                            hideAction?.Invoke();
                                        }),
                                        Button("3").OnClick(() =>
                                        {
                                            Toast().Information("3");
                                            hideAction?.Invoke();
                                        })).Render();
                                    Tippy.ShowFor(s, tt, out hideAction, TooltipAnimation.None, TooltipPlacement.BottomEnd, 0, 0, 350, true, null);

                                }),
                            ActionButton(VStack().Children(HStack().Children(Icon(UIcons.Arrows), TextBlock("Lorem ipsum")), TextBlock("Subtitle").Tiny(), TextBlock("Subtitle2").Tiny(), TextBlock("Subtitle4").Tiny())).Var(out var btn4) //.SetText("Standard").Tooltip("This is a standard button")
                               .OnClickDisplay((s, e) => alert("Clicked4 display!"))
                               .OnClickAction((s,  e) => alert("Clicked4 action!")),
                            Button("test").Primary(),
                            Button("test").Danger().SetIcon(UIcons.Trash),
                            Button("test").SetIcon(UIcons.Arrows)
//                            ActionButton("action2").Var(out var btn2) //.SetText("Primary").Tooltip("This is a primary button").Primary()
//                               .OnClick((s, e) => alert("Clicked2!")),
//                            ActionButton("action3").Var(out var btn3) //.SetText("Link").Tooltip(Button("This is a link button with a button tooltip").Primary()
//                               .OnClick((_, __) => Toast().Success("You clicked here3"))
                        )
                    )
//                        TextBlock("Icon ActionButton").Medium(),
//                        HStack().Children(
//                             ActionButton().Var(out var iconBtn1).SetText("Confirm").SetIcon("las la-check").Success().OnClick((s,   e) => alert("Clicked!")),
//                             ActionButton().Var(out var iconBtn2).SetText("Delete").SetIcon("las la-trash-alt").Danger().OnClick((s, e) => alert("Clicked!")),
//                             ActionButton().Var(out var iconBtn3).SetText("Primary").SetIcon("las la-minus").Primary().OnClick((s,   e) => alert("Clicked!")),
//                             ActionButton().Var(out var iconBtn4).SetText("Copy date").SetIcon("las la-calendar-alt").OnClick((s,    e) => Clipboard.Copy(DateTime.Now.ToString()))
//                        ),
//                        TextBlock("Spinner ActionButton").Medium(),
//                        HStack().Children(
//                             ActionButton().Var(out var spinBtn1).SetText("Spin").OnClickSpinWhile(async () => await Task.Delay(1000)),
//                             ActionButton().Var(out var spinBtn2).SetText("Spin with text").OnClickSpinWhile(async () => await Task.Delay(1000), "loading..."),
//                             ActionButton().Var(out var spinBtn3).SetText("Spin & Error").OnClickSpinWhile(async () =>
//                            {
//                                await Task.Delay(1000);
//                                throw new Exception("Error!");
//                            }, onError: (b, e) => spinBtn3.SetText("Failed: " + e.Message).SetIcon(UIcons.TriangleWarning).DangerLink()),
//                             ActionButton().Var(out var spinBtn4).SetText("Spin with text & Error").OnClickSpinWhile(async () =>
//                            {
//                                await Task.Delay(1000);
//                                throw new Exception("Error!");
//                            }, "loading...", onError: (b, e) => spinBtn4.SetText("Failed: " + e.Message).SetIcon(UIcons.TriangleWarning).DangerLink())
//                        ),
//                        Toggle("Disable buttons").Checked().OnChange((s, e) =>
//                        {
//                            btn1.IsEnabled = btn2.IsEnabled = btn3.IsEnabled = iconBtn1.IsEnabled = iconBtn2.IsEnabled = iconBtn3.IsEnabled = iconBtn4.IsEnabled = s.IsChecked;
//                        })
//                )
                );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}