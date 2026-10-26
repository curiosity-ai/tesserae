using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 10, Icon = UIcons.InputText, Description = "Single-line text input, with icons and prefixes")]
    public class TextBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TextBoxSample()
        {
            //SpinWhileAsync shows a spinner inside the field while the call runs; the value it fills in is the model's, so
            //it is marked AI() until the user types over it.
            var suggested = TextBox().SetPlaceholder("Click Suggest, or type your own");
            var suggest   = Button("Suggest").AISubtle();

            suggest.OnClickSpinWhile(() => suggested.SpinWhileAsync(async () =>
            {
                await Task.Delay(1500);
                suggested.Text = "Weekly digest of open support tickets";
                suggested.AI();
            }));

            suggested.OnInput((s, e) =>
            {
                //Setting Text raises OnInput with no event; only a keystroke takes the value over.
                if (e is object) s.AI(false);
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TextBoxSample), UIcons.Text, "A control to input text")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("TextBoxes allow users to enter and edit text. They are used in forms, search queries, and anywhere text input is required."),
                    TextBlock("They support various modes like password input, read-only states, and built-in validation."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Always label your TextBoxes so users know what information is expected. Use placeholder text to provide a hint about the format or content. Mark required fields clearly. Use validation to provide immediate feedback on the correctness of the input. Use the appropriate input type (e.g., Password) for sensitive information. Provide a clear way to submit or clear the data."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic TextBoxes"),
                    VStack().Children(
                        Label("Standard").SetContent(TextBox()),
                        Label("Placeholder").SetContent(TextBox().SetPlaceholder("Enter your name...")),
                        Label("Password").SetContent(TextBox().Password()),
                        Label("Disabled").Disabled().SetContent(TextBox("Disabled content").Disabled()),
                        Label("Read-only").SetContent(TextBox("Read-only content").ReadOnly())
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Required").Required().SetContent(TextBox()),
                        Label("Must not be empty").SetContent(TextBox().Validation(tb => string.IsNullOrWhiteSpace(tb.Text) ? "This field is required" : null)),
                        Label("Positive Integer only").SetContent(TextBox().Validation(Validation.NonZeroPositiveInteger)),
                        Label("Custom Error").SetContent(TextBox().Error("Something went wrong").IsInvalid())
                    ),
                    SampleSubTitle("Event Handling"),
                    VStack().Children(
                        TextBox().SetPlaceholder("Type and check toast...").OnChange((s, e) => Toast().Information($"Text changed to: {s.Text}")),
                        TextBox().SetPlaceholder("Search-like behavior...").OnInput((s, e) => console.log($"Current input: {s.Text}"))
                    ),
                    SampleSubTitle("Rounded TextBoxes"),
                    VStack().Children(
                        TextBox().SetPlaceholder("Small").Rounded(BorderRadius.Small),
                        TextBox().SetPlaceholder("Medium").Rounded(BorderRadius.Medium),
                        TextBox().SetPlaceholder("Full").Rounded(BorderRadius.Full)
                    ),
                    SampleSubTitle("Dictation"),
                    VStack().Children(
                        Label("With a microphone button").SetContent(TextBox().SetPlaceholder("Press the microphone and speak...").WithDictation())
                    ),
                    SampleSubTitle("AI and Loading"),
                    VStack().Children(
                        Label("Generated").SetContent(TextBox("Quarterly revenue summary for the EMEA region").AI()),
                        Label("Suggested title").SetContent(HStack().WS().AlignItemsCenter().Children(suggested.W(10).Grow(), suggest.ML(8)))
                    )
                )).SetTitle("Usage")))
               .SeeAlso(typeof(TextAreaSample), typeof(AutoCompleteBoxSample), typeof(SearchBoxSample), typeof(NumberPickerSample), typeof(ValidatorSample), typeof(LabelSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
