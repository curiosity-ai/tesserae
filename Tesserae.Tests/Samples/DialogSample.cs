using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class DialogSample : IComponent
    {
        private IComponent content;

        public DialogSample()
        {
            var dialog = Dialog("Lorem Ipsum");
            var response = TextBlock();

            content = Stack().Children(
                TextBlock("Dialog").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("Dialogs are temporary, modal UI overlay that generally provide contextual app information or require user confirmation/input. In most cases, Dialogs block interactions with the web page or application until being explicitly dismissed, and often request action from the user. They are primarily used for lightweight creation or edit tasks, and simple management tasks."),
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().Width(40, Unit.Percents).Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Use Dialogs for quick, actionable interactions, such as making a choice or needing the user to provide information."),
                        TextBlock("When possible, try a non-blocking Dialog before resorting to a blocking Dialog."),
                        TextBlock("Only include information needed to help users make a decision."),
                        TextBlock("Button text should reflect the actions available to the user (e.g. save, delete)."),
                        TextBlock("Validate that the user's entries are acceptable before closing the Dialog. Show an inline validation error near the field they must correct.")
                    ),
                    Stack().Width(40, Unit.Percents).Children(
                        TextBlock("Don't").Medium(),
                        TextBlock("Don’t overuse Modals. In some cases they can be perceived as interrupting workflow, and too many can be a bad user experience."),
                        TextBlock("Avoid \"Are you sure ?\" or confirmation Dialogs unless the user is making an irreversible or destructive choice."),
                        TextBlock("Do not use a blocking Dialog unless absolutely necessary because they are very disruptive."),
                        TextBlock("Don’t have long sentences or complicated choices."),
                        TextBlock("Avoid generic button labels like \"Ok\" if you can be more specific about the action a user is about to complete."),
                        TextBlock("Don't dismiss the Dialog if underlying problem is not fixed. Don't put the user back into a broken/error state."),
                        TextBlock("Don't provide the user with more than 3 buttons.")
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                Button("Open Dialog").OnClick((c, ev) => dialog.Show()),
                Stack().Horizontal().Children(
                    Button("Open YesNo").OnClick((c, ev)             => Dialog("Sample Dialog").YesNo(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"))),
                    Button("Open YesNoCancel").OnClick((c, ev)       => Dialog("Sample Dialog").YesNoCancel(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), () => response.Text("Clicked Cancel"))),
                    Button("Open Ok").OnClick((c, ev)                => Dialog("Sample Dialog").Ok(() => response.Text("Clicked Ok"))),
                    Button("Open RetryCancel").OnClick((c, ev)       => Dialog("Sample Dialog").RetryCancel(() => response.Text("Clicked Retry"), () => response.Text("Clicked Cancel")))),
                    Button("Open Modal YesNo").OnClick((c, ev)       => Dialog("Sample Dialog").NoLightDismiss().Dark().YesNo(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), y => y.Success().Text("Yes!"), n => n.Danger().Text("Nope"))),
                    Button("Open Modal YesNoCancel").OnClick((c, ev) => Dialog("Sample Dialog").NoLightDismiss().Dark().YesNoCancel(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), () => response.Text("Clicked Cancel"))),
                    Button("Open Modal Ok").OnClick((c, ev)          => Dialog("Sample Dialog").NoLightDismiss().Dark().Ok(() => response.Text("Clicked Ok"))),
                    Button("Open Modal RetryCancel").OnClick((c, ev) => Dialog("Sample Dialog").NoLightDismiss().Dark().RetryCancel(() => response.Text("Clicked Retry"), () => response.Text("Clicked Cancel"))),
                    response);

                dialog.Content(Stack().Children(TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit."),
                                                Toggle("Light Dismiss").Checked().OnChange((c, ev) => dialog.CanLightDismiss = c.IsChecked),
                                                Toggle("Is draggable").OnChange((c, ev) => dialog.IsDraggable = c.IsChecked),
                                                Toggle("Is dark overlay").OnChange((c, ev) => dialog.Dark = c.IsChecked).Checked(dialog.Dark),
                                                Toggle("Is non-blocking").OnChange((c, ev) => dialog.IsNonBlocking = c.IsChecked),
                                                Toggle("Hide close button").OnChange((c, ev) => dialog.ShowCloseButton = !c.IsChecked)))
                      .Footer(Stack().HorizontalReverse()
                                     .Children(Button("Don`t send").AlignEnd().OnClick((c, ev) => dialog.Hide()), Button("Send").Primary().AlignEnd().OnClick((c, ev) => dialog.Hide())));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
