using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ButtonSample : IComponent
    {
        private IComponent content;

        public ButtonSample()
        {
            content = SectionStack()
                .Title(TextBlock("Button").XLarge().Bold())
                .Section(Stack().Children(
                TextBlock("Overview").MediumPlus(),
                TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."),
                TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements."),
                TextBlock("While buttons can technically be used to navigate a user to another part of the experience, this is not recommended unless that navigation is part of an action or their flow."))
                )
                .Section(Stack().Children(
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Make sure the label conveys a clear purpose of the button to the user."),
                        TextBlock("Button labels must describe the action the button will perform and should include a verb. Use concise, specific, self-explanatory labels, usually a single word."),
                        TextBlock("Buttons should always include a noun if there is any room for interpretation about what the verb operates on."),
                        TextBlock("Consider the affect localization will have on the button and what will happen to components around it."),
                        TextBlock("If the button’s label content is dynamic, consider how the button will resize and what will happen to components around it."),
                        TextBlock("Use only a single line of text in the label of the button."),
                        TextBlock("Expose only one or two buttons to the user at a time, for example, \"Accept\" and \"Cancel\". If you need to expose more actions to the user, consider using checkboxes or radio buttons from which the user can select actions, with a single command button to trigger those actions."),
                        TextBlock("Show only one primary button that inherits theme color at rest state. In the event there are more than two buttons with equal priority, all buttons should have neutral backgrounds."),
                        TextBlock("\"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons. When \"Reset\" or \"Cancel\" buttons appear alongside one of the above, they should be styled as secondary buttons."),
                        TextBlock("Default buttons should always perform safe operations. For example, a default button should never delete."),
                        TextBlock("Use task buttons to cause actions that complete a task or cause a transitional task. Do not use buttons to toggle other UX in the same context. For example, a button may be used to open an interface area but should not be used to open an additional set of components in the same interface.")
                    ),
                    Stack().Children(
                        TextBlock("Don't").Medium(),
                        TextBlock("Don't use generic labels like \"Ok, \" especially in the case of an error; errors are never \"Ok.\""),
                        TextBlock("Don’t place the default focus on a button that destroys data. Instead, place the default focus on the button that performs the \"safe act\" and retains the content (i.e. \"Save\") or cancels the action (i.e. \"Cancel\")."),
                        TextBlock("Don’t use a button to navigate to another place, use a link instead. The exception is in a wizard where \"Back\" and \"Next\" buttons may be used."),
                        TextBlock("Don’t put too much text in a button - try to keep the length of your text to a minimum."),
                        TextBlock("Don't put anything other than text in a button.")
                    ))
                ))
                .Section(
                    Stack().Children(
                        TextBlock("Usage").MediumPlus(),
                        TextBlock("Default Button").Medium(),
                        Stack().Horizontal().Children(
                            Button().Var(out var btn1).Text("Standard").OnClicked((s, e) => alert("Clicked!")),
                            Button().Var(out var btn2).Text("Primary").Primary().OnClicked((s, e) => alert("Clicked!"))
                        ),
                        TextBlock("Icon Button").Medium(),
                        Stack().Horizontal().Children(
                            Button().Var(out var iconBtn1).Text("Confirm").Icon("far fa-check").Success().OnClicked((s, e) => alert("Clicked!")),
                            Button().Var(out var iconBtn2).Text("Delete").Icon("far fa-trash-alt").Danger().OnClicked((s, e) => alert("Clicked!")),
                            Button().Var(out var iconBtn3).Text("Primary").Icon("far fa-minus").Primary().OnClicked((s, e) => alert("Clicked!"))
                        ),
                        Toggle("Disable buttons").Checked().OnChanged((s, e) =>
                        {
                            btn1.IsEnabled = btn2.IsEnabled = iconBtn1.IsEnabled = iconBtn2.IsEnabled  = iconBtn3.IsEnabled = s.IsChecked;
                        })));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
