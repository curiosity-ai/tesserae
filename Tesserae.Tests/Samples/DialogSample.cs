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
                    Stack().WidthPercents(40).Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Use Dialogs for quick, actionable interactions, such as making a choice or needing the user to provide information."),
                        TextBlock("When possible, try a non-blocking Dialog before resorting to a blocking Dialog."),
                        TextBlock("Only include information needed to help users make a decision."),
                        TextBlock("Button text should reflect the actions available to the user (e.g. save, delete)."),
                        TextBlock("Validate that the user's entries are acceptable before closing the Dialog. Show an inline validation error near the field they must correct.")
                    ),
                    Stack().WidthPercents(40).Children(
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
                Button("Open Dialog").OnClicked((s, e) => dialog.Show()),
                Stack(StackOrientation.Horizontal).Children(
                    Button("Open YesNo").OnClicked((s, e)             => Dialog("Sample Dialog").YesNo(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"))),
                    Button("Open YesNoCancel").OnClicked((s, e)       => Dialog("Sample Dialog").YesNoCancel(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), () => response.Text("Clicked Cancel"))),
                    Button("Open Ok").OnClicked((s, e)                => Dialog("Sample Dialog").Ok(() => response.Text("Clicked Ok"))),
                    Button("Open RetryCancel").OnClicked((s, e)       => Dialog("Sample Dialog").RetryCancel(() => response.Text("Clicked Retry"), () => response.Text("Clicked Cancel")))),
                    Button("Open Modal YesNo").OnClicked((s, e)       => Dialog("Sample Dialog").NoLightDismiss().Dark().YesNo(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"))),
                    Button("Open Modal YesNoCancel").OnClicked((s, e) => Dialog("Sample Dialog").NoLightDismiss().Dark().YesNoCancel(() => response.Text("Clicked Yes"), () => response.Text("Clicked No"), () => response.Text("Clicked Cancel"))),
                    Button("Open Modal Ok").OnClicked((s, e)          => Dialog("Sample Dialog").NoLightDismiss().Dark().Ok(() => response.Text("Clicked Ok"))),
                    Button("Open Modal RetryCancel").OnClicked((s, e) => Dialog("Sample Dialog").NoLightDismiss().Dark().RetryCancel(() => response.Text("Clicked Retry"), () => response.Text("Clicked Cancel"))),
                    response);

                dialog.Content(Stack().Children(TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit."),
                                                Toggle("Light Dismiss").Checked().OnChanged((s, e) => dialog.CanLightDismiss = e.IsChecked),
                                                Toggle("Is draggable").OnChanged((s, e) => dialog.IsDraggable = e.IsChecked),
                                                Toggle("Is dark overlay").OnChanged((s, e) => dialog.Dark = e.IsChecked).Checked(dialog.Dark),
                                                Toggle("Is non-blocking").OnChanged((s, e) => dialog.IsNonBlocking = e.IsChecked),
                                                Toggle("Hide close button").OnChanged((s, e) => dialog.ShowCloseButton = !e.IsChecked)))
                      .Footer(Stack().HorizontalReverse()
                                     .Children(Button("Don`t send").AlignEnd().OnClicked((s, e) => dialog.Hide()), Button("Send").Primary().AlignEnd().OnClicked((s, e) => dialog.Hide())));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
