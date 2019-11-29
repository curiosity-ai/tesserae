using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class ChoiceGroupSample : IComponent
    {
        private IComponent content;

        public ChoiceGroupSample()
        {
            var btn1 = Button();
            var btn2 = Button();
            var iconBtn1 = Button();
            var iconBtn2 = Button();
            content = Stack().Children(
                TextBlock("ChoiceGroup").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("The ChoiceGroup component, also known as radio buttons, let users select one option from two or more choices. Each option is represented by one ChoiceGroup button; a user can select only one ChoiceGroup in a button group."),
                TextBlock("ChoiceGroup emphasize all options equally, and that may draw more attention to the options than necessary. Consider using other controls, unless the options deserve extra attention from the user. For example, if the default option is recommended for most users in most situations, use a Dropdown component instead."),
                TextBlock("If there are only two mutually exclusive options, combine them into a single Checkbox or Toggle switch. For example, use a Checkbox for \"I agree\" instead of ChoiceGroup buttons for \"I agree\" and \"I don't agree.\""),
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().WidthPercents(40).Children(
                        TextBlock("Do").Medium(),
                        TextBlock("Use when there are 2-7 options, if you have enough screen space and the options are important enough to be a good use of that screen space. Otherwise, use a Checkbox or Dropdown list."),
                        TextBlock("Use on wizard pages to make the alternatives clear, even if a Checkbox is otherwise acceptable."),
                        TextBlock("List the options in a logical order, such as most likely to be selected to least, simplest operation to most complex, or least risk to most. Alphabetical ordering is not recommended because it is language dependent and therefore not localizable."),
                        TextBlock("If none of the options is a valid choice, add another option to reflect this choice, such as \"None\" or \"Does not apply\"."),
                        TextBlock("Select the safest (to prevent loss of data or system access) and most secure and private option as the default. If safety and security aren't factors, select the most likely or convenient option."),
                        TextBlock("Align radio buttons vertically instead of horizontally, if possible. Horizontal alignment is harder to read and localize.")
                    ),
                    Stack().WidthPercents(40).Children(
                        TextBlock("Don't").Medium(),
                        TextBlock("Use when the options are numbers that have fixed steps, like 10, 20, 30. Use a Slider component instead."),
                        TextBlock("Use if there are more than 7 options, use a Dropdown instead."),
                        TextBlock("Nest with other ChoiceGroup or CheckBoxes. If possible, keep all the options at the same level.")
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                TextBlock("Default ChoiceGroup").Medium(),
                ChoiceGroup().Options(
                    Option("Option A"),
                    Option("Option B"),
                    Option("Option C").Disabled(),
                    Option("Option D")
                ),
                TextBlock("Required ChoiceGroup with a custom label").Medium(),
                ChoiceGroup("Custom label").Required().Options(
                    Option("Option A"),
                    Option("Option B"),
                    Option("Option C").Disabled(),
                    Option("Option D")
                ),
                TextBlock("Horizontal ChoiceGroup").Medium(),
                ChoiceGroup().Horizontal().Options(
                    Option("Option A"),
                    Option("Option B"),
                    Option("Option C").Disabled(),
                    Option("Option D")
                )
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
