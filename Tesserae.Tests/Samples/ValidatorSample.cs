using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests.Samples
{
    public class ValidatorSample : IComponent
    {
        private IComponent content;

        public ValidatorSample()
        {
            var validator = new Validator();
            var isAllValid = TextBlock("?");
            validator.OnValidation += (_, valid) => isAllValid.Text = valid ? "Valid ✔" : "Invalid ❌";

            content = Stack().Children(
                TextBlock("Validator").XLarge(),
                TextBlock("Overview").MediumPlus(),
                TextBlock("The validator helper allows you to capture the state of multiple components registered on it."),
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().WidthPercents(40).Children(
                        TextBlock("Do: TODO").Medium()
                    ),
                    Stack().WidthPercents(40).Children(
                        TextBlock("Don't: TODO").Medium()
                    )
                ),
                TextBlock("Usage").MediumPlus(),
                TextBlock("Basic TextBox").Medium(),
                Stack().WidthPercents(40).Children(
                    Label("Non-empty").Content(TextBox().Validation((tb) => tb.Text.Length == 0 ? "Empty" : null, validator)),
                    Label("Integer > 0").Content(TextBox().Validation(Validation.NonZeroPositiveInteger, validator)),
                    Label("Is all valid").Content(isAllValid)
                )
            );
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
