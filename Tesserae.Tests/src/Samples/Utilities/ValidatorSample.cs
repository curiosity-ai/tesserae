using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 0, Icon = UIcons.Check)]
    public class ValidatorSample : IComponent, ISample
    {
        private readonly IComponent content;
        public ValidatorSample()
        {
            var looksValidSoFar = TextBlock("?");
            var validator       = Validator().OnValidation(validity => looksValidSoFar.Text = validity == ValidationState.Invalid ? "Something is not ok ❌" : "Everything is fine so far ✔");

            // Note: The "Required()" calls on these components only marks them visually as being required - if they must have values then that must be accounted for in their Validation(..) logic
            var textBoxThatMustBeNonEmpty        = TextBox("").Required();
            var textBoxThatMustBePositiveInteger = TextBox("").Required();
            textBoxThatMustBeNonEmpty.Validation(tb => tb.Text.Length == 0 ? "must enter a value" : textBoxThatMustBeNonEmpty.Text == textBoxThatMustBePositiveInteger.Text ? "duplicated  values" : null, validator);
            textBoxThatMustBePositiveInteger.Validation(tb => Validation.NonZeroPositiveInteger(tb) ?? (textBoxThatMustBeNonEmpty.Text == textBoxThatMustBePositiveInteger.Text ? "duplicated values" : null), validator);


            var dropdown = Dropdown().Items(DropdownItem(""), DropdownItem("Item 1"), DropdownItem("Item 2")).Required().Validation(dd => string.IsNullOrWhiteSpace(dd.SelectedText) ? "must select an item" : null, validator);

            content = SectionStack()
               .Title(SampleHeader(nameof(ValidatorSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Validator is a utility component that aggregates the validation state of multiple UI components. It provides a centralized way to monitor whether a form or a set of inputs is valid, making it easy to provide real-time feedback to users and prevent the submission of incorrect data.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Register all related input components with a single Validator. Use clear and descriptive validation messages that help users correct errors. Avoid showing validation errors immediately on form load; instead, allow users to interact with the fields first. Use the Validator's state to enable or disable primary actions like 'Submit' or 'Save' to ensure only valid data is processed.")))
               .Section(
                    Stack().Children(
                        SampleTitle("Usage"),
                        TextBlock("Basic TextBox").Medium(),
                        Stack().Width(40.percent()).Padding(8.px()).Children(
                            Label("Non-empty").SetContent(textBoxThatMustBeNonEmpty),
                            Label("Integer > 0 (must not match the value above)").SetContent(textBoxThatMustBePositiveInteger),
                            Label("Pre-filled Integer > 0 (initially valid)").SetContent(TextBox("123").Required().Validation(Validation.NonZeroPositiveInteger, validator)),
                            Label("Pre-filled Integer > 0 (initially i  nvalid)").SetContent(TextBox("xyz").Required().Validation(Validation.NonZeroPositiveInteger, validator)),
                            Label("Not empty with forced instant validation").SetContent(TextBox("").Required().Validation(tb => string.IsNullOrWhiteSpace(tb.Text) ? "Can't be empty" : null, validator, forceInitialValidation: true)),
                            Label("Please select something").SetContent(dropdown)
                        ),
                        TextBlock("Results Summary").Medium(),
                        Stack().Width(40.percent()).Padding(8.px()).Children(
                            Label("Validity (this only checks fields that User has interacted with so far)").Inline().SetContent(looksValidSoFar),
                            Label("Test revalidation (will fail if repeated)").SetContent(Button("Validate").OnClick((s, b) => validator.Revalidate()))
                        )
                    )
                );

            // 2020-09-16 DWR: The form here follows the pattern of not disabling the submit button (the "Validate" button in this case), so they can enter as much or as little of it as they want and then try to submit and if they
            // have left required fields unfilled (or not corrected any pre-filled invalid values) then they will THEN be informed of it. But if we wanted to disable the form submit button until the form was known to be in a valid
            // state then the "AreCurrentValuesAllValid" method can be called after the form is rendered and the button enabled state set accordingly and then updated after each ValidationOccured event.
            console.log("Is form initially in a valid state: " + validator.AreCurrentValuesAllValid());
        }

        public HTMLElement Render() => content.Render();
    }
}