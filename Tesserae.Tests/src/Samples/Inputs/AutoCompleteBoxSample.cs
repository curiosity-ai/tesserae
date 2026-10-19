using System;
using System.Linq;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Inputs, Order = 15, Icon = UIcons.Keyboard, Description = "Free-text input with suggestions and an asynchronous check")]
    public class AutoCompleteBoxSample : IComponent, ISample
    {
        private static readonly string[] Models =
        {
            "gpt-4.1", "gpt-4.1-mini", "gpt-4o", "gpt-4o-mini", "o3", "o4-mini",
            "claude-opus-4-1", "claude-sonnet-4-5", "claude-haiku-4-5",
            "ggml-org/Qwen3-0.6B-GGUF:Q8_0", "ggml-org/Qwen3-8B-GGUF:Q4_K_M", "ggml-org/gemma-3-4b-it-GGUF:Q4_K_M",
            "mistral-large-latest", "mistral-small-latest",
        };

        private static readonly string[] Fruits = { "Apple", "Apricot", "Banana", "Blackberry", "Blueberry", "Cherry", "Grape", "Grapefruit", "Lemon", "Lime", "Mango", "Orange", "Peach", "Pear", "Pineapple", "Plum" };

        private readonly IComponent _content;

        public AutoCompleteBoxSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(AutoCompleteBoxSample), UIcons.Keyboard, "A text box that suggests values and checks what was typed")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("An AutoCompleteBox is a TextBox with a list of suggestions that narrows as you type. The text stays free: a suggestion is a shortcut, not a constraint, which is what sets it apart from a searchable Dropdown."),
                    TextBlock("It can also check the typed value asynchronously, debounced after each edit, and show the answer as an icon and a short label at the end of the box: valid, a warning, or invalid. An invalid answer also turns the border red, and an answer can carry a tooltip that explains it."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use it when the set of good values is known but not closed: model ids a server lists, tags, e-mail domains. If the value must be one of the list, use a Dropdown. Prefer a warning over invalid when the check cannot know every good value."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Fixed suggestions"),
                    Label("Fruit").SetContent(AutoCompleteBox().SetPlaceholder("Start typing a fruit...").Suggestions(Fruits)),
                    SampleSubTitle("Loaded suggestions and an asynchronous check"),
                    TextBlock("The list is loaded once, on first focus, after a simulated 600 ms round trip. The check takes 500 ms and warns about a value the list does not have.").Small().Secondary(),
                    Label("Model ID").SetContent(AutoCompleteBox("gpt-4o")
                       .SetPlaceholder("The model id this endpoint answers to")
                       .Suggestions(async () =>
                        {
                            await Task.Delay(600);
                            return Models;
                        })
                       .CheckAsync(async text =>
                        {
                            await Task.Delay(500);

                            return Models.Contains(text)
                                ? InputCheck.Valid("Listed", tooltip: "This endpoint lists the model id in its /models answer.")
                                : InputCheck.Warning("Not listed", tooltip: $"This endpoint does not list \"{text}\". It may still answer to it, if the server accepts ids it does not advertise.");
                        })),
                    SampleSubTitle("Invalid values"),
                    Label("Fruit, checked").SetContent(AutoCompleteBox()
                       .Suggestions(Fruits)
                       .CheckAsync(async text =>
                        {
                            await Task.Delay(300);
                            return Fruits.Contains(text)
                                ? InputCheck.Valid()
                                : InputCheck.Invalid(tooltip: $"\"{text}\" is not in the list of fruits this field accepts.");
                        }, debounceMs: 250))
                )).SetTitle("Usage")))
               .SeeAlso(typeof(TextBoxSample), typeof(DropdownSample), typeof(PickerSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
