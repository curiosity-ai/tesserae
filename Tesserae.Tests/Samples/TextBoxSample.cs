using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class TextBoxSample : IComponent
    {
        private IComponent content;

        public TextBoxSample()
        {
            content = Stack().Children(
                TextBlock("TextBox").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("The TextBox component enables a user to type text into an app. The text displays on the screen in a simple, uniform format."),
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().WidthPercents(40).Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Use the TextBox to accept data input on a form or page."),
                        TextBlock("Label the TextBox with a helpful name."),
                        TextBlock("Provide concise helper text that specifies what content is expected to be entered."),
                        TextBlock("When part of a form, provide clear designations for which TextBox are required vs. optional."),
                        TextBlock("Provide all appropriate methods for submitting provided data (e.g. dedicated ‘Submit’ button)."),
                        TextBlock("Provide all appropriate methods of clearing provided data (‘X’ or something similar)."),
                        TextBlock("Allow for selection, copy and paste of field data."),
                        TextBlock("Ensure that the TextBox is functional through use of mouse/keyboard or touch when available.")
                    ),
                    Stack().WidthPercents(40).Children(
                        TextBlock("Don't").Medium(),
                        TextBlock("Don't use a TextBox to render basic copy as part of a body element of a page."),
                        TextBlock("Don't provide an unlabeled TextBox and expect that users will know what to do with it."),
                        TextBlock("Don't place a TextBox inline with body copy."),
                        TextBlock("Don't be overly verbose with helper text."),
                        TextBlock("Don't occlude the entry or allow entry when the active content is not visible.")
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                TextBlock("Basic TextBox").Medium(),
                Stack().WidthPercents(40).Children(
                    Label("Standard").Content(TextBox()),
                    Label("Disabled").Disabled().Content(TextBox("I am disabled").Disabled()),
                    Label("Read-only").Content(TextBox("I am read-only").ReadOnly()),
                    Label("Required").Required().Content(TextBox("")),
                    TextBox("").Required(),
                    Label("With error message").Content(TextBox().Error("Error message").IsInvalid()),
                    Label("With placeholder").Content(TextBox().Placeholder("Please enter text here")),
                    Label("With validation").Content(TextBox().Validation((tb) => tb.Text.Length == 0 ? "Empty" : null)),
                    Label("With validation on type").Content(TextBox().Validation(Validation.NonZeroPositiveInteger)),
                    Label("Disabled with placeholder").Disabled().Content(TextBox().Placeholder("I am disabled").Disabled())
                )
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
