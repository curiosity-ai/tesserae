using Tesserae.Components;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ValidatorSample : IComponent
    {
        private readonly IComponent content;
        public ValidatorSample()
        {
            var looksValidSoFar = TextBlock("?");
            var validator = Validator().OnValidation(haveEncounteredInvalidValue => looksValidSoFar.Text = haveEncounteredInvalidValue ? "Something is not ok ❌" : "Everything is fine so far ✔");

            // Note: The "Required()" calls on these components only marks them visually as being required - if they must have values then that must be accounted for in their Validation(..) logic
            var tb1 = TextBox().Required();
            var tb2 = TextBox().Required();
            tb1.Validation(tb => tb.Text.Length == 0 ? "Empty" : ((tb1.Text == tb2.Text) ? "Duplicated  values" : null), validator);
            tb2.Validation(tb => Validation.NonZeroPositiveInteger(tb) ?? ((tb1.Text == tb2.Text) ? "Duplicated values" : null), validator);

            var dropdown = Dropdown().Items(DropdownItem(""), DropdownItem("Item 1"), DropdownItem("Item 2")).Required().Validation(dd => string.IsNullOrWhiteSpace(dd.SelectedText) ? "Must select an item" : null, validator);

            content = SectionStack()
                        .Title(SampleHeader(nameof(ValidatorSample)))
                        .Section(Stack().Children(
                            SampleTitle("Overview"),
                            TextBlock("The validator helper allows you to capture the state of multiple components registered on it.")
                        ))
                        .Section(Stack().Children(
                            SampleTitle("Best Practices"),
                            Stack().Horizontal().Children(
                                Stack().Width(40.percent()).Children(
                                    SampleSubTitle("Do"),
                                    SampleDo("Display useful validation warning messages to components for when the User has left them in an invalid state or when they have tried to submit a partially-populated form")),
                                Stack().Width(40.percent()).Children(
                                    SampleSubTitle("Don't"),
                                    SampleDont("Display ALL validation warnings messages as soon as a form is rendered, give the User an opportunity to interact with it and enter valid values before shouting at them")))
                            )
                        )
                        .Section(
                            Stack().Children(
                                SampleTitle("Usage"),
                                TextBlock("Basic TextBox").Medium(),
                                Stack().Width(40.percent()).Padding(8.px()).Children(
                                    Label("Non-empty").SetContent(tb1),
                                    Label("Integer > 0").SetContent(tb2),
                                    Label("Please select something").SetContent(dropdown)
                                ),
                                TextBlock("Results Summary").Medium(),
                                Stack().Width(40.percent()).Padding(8.px()).Children(
                                    Label("Validity").Inline().SetContent(looksValidSoFar),
                                    Label("Test revalidation (will fail if repeated)").SetContent(Button("Validate").OnClick((s, b) => validator.Revalidate()))
                                )
                            )
                        );
        }

        public HTMLElement Render() => content.Render();
    }
}
