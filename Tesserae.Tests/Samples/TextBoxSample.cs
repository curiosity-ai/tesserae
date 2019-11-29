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
            var btn1 = Button();
            var btn2 = Button();
            var iconBtn1 = Button();
            var iconBtn2 = Button();
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
                        TextBlock("Don’t use a TextBox to render basic copy as part of a body element of a page."),
                        TextBlock("Don’t provide an unlabeled TextBox and expect that users will know what to do with it."),
                        TextBlock("Don’t place a TextBox inline with body copy."),
                        TextBlock("Don’t be overly verbose with helper text."),
                        TextBlock("Don’t occlude the entry or allow entry when the active content is not visible.")
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                TextBlock("Default Button").Medium(),
                Stack().Children(
                    TextBlock("Standard").SemiBold(),
                    TextBox().WidthPercents(40),
                    TextBlock("Disabled").SemiBold(),
                    TextBox("I am disabled").WidthPercents(40).Disabled(),
                    TextBlock("Read-only").SemiBold(),
                    TextBox("I am read-only").WidthPercents(40).ReadOnly(),
                    TextBlock("Required").AlignBaseline().Required().SemiBold(),
                    TextBox("").WidthPercents(40),
                    TextBox("").WidthPercents(40).Required(),
                    TextBlock("With error message").SemiBold(),
                    TextBox().Error("Error message").WidthPercents(40).Invalid(),
                    TextBlock("With placeholder").SemiBold(),
                    TextBox().WidthPercents(40).Placeholder("Please enter text here"),
                    TextBlock("Disabled with placeholder").SemiBold(),
                    TextBox().WidthPercents(40).Placeholder("I am disabled").Disabled()
                )
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
