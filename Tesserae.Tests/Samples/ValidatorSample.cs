using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

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
            var tb1 = TextBox();
            var tb2 = TextBox();
            tb1.Validation((tb) => tb.Text.Length == 0 ? "Empty" : ((tb1.Text == tb2.Text) ? "Duplicated  values" : null), validator);
            tb2.Validation((tb) => Validation.NonZeroPositiveInteger(tb) ?? ((tb1.Text == tb2.Text) ? "Duplicated values" : null), validator);

            content = SectionStack()
                .Title(
                TextBlock("Validator").XLarge().Bold())
                .Section(Stack().Children(
                TextBlock("Overview").MediumPlus(),
                TextBlock("The validator helper allows you to capture the state of multiple components registered on it.")))
                .Section(Stack().Children(
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                    Stack().Width(40, Unit.Percents).Children(
                        TextBlock("Do: TODO").Medium()
                    ),
                    Stack().Width(40, Unit.Percents).Children(
                        TextBlock("Don't: TODO").Medium()
                    )
                )))
                .Section(Stack().Children(
                TextBlock("Usage").MediumPlus(),
                TextBlock("Basic TextBox").Medium(),
                Stack().Width(40, Unit.Percents).Children(
                    Label("Non-empty").SetContent(tb1),
                    Label("Integer > 0").SetContent(tb2),
                    Label("Is all valid").SetContent(isAllValid),
                    Label("Test revalidation (will fail if repeated)").SetContent(Button("Revalidate").OnClick((s,b) => validator.Revalidate()))
                )));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
