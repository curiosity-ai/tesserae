using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.InputText)]
    public class TextBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TextBoxSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(TextBoxSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("TextBoxes allow users to enter and edit text. They are used in forms, search queries, and anywhere text input is required."),
                    TextBlock("They support various modes like password input, read-only states, and built-in validation.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Always label your TextBoxes so users know what information is expected. Use placeholder text to provide a hint about the format or content. Mark required fields clearly. Use validation to provide immediate feedback on the correctness of the input. Use the appropriate input type (e.g., Password) for sensitive information. Provide a clear way to submit or clear the data.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
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
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
