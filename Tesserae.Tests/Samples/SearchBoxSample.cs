using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class SearchBoxSample : IComponent
    {
        private IComponent _content;

        public SearchBoxSample()
        {
            var searchAsYouType = TextBlock("start typing");
            _content = SectionStack()
            .Title(TextBlock("Search Box").XLarge().Bold())
            .Section(Stack().Children(
                TextBlock("Overview").MediumPlus(),
                TextBlock("SearchBoxes provide an input field for searching through content, allowing users to locate specific items within the website or app.")))
            .Section(Stack().Children(
                TextBlock("Best Practices").MediumPlus(),
                Stack().Horizontal().Children(
                Stack().Width(40, Unit.Percents).Children(
                    TextBlock("Do").Medium(),
                    TextBlock("Use placeholder text in the SearchBox to describe what users can search for."),
                    TextBlock("Example: 'Search'; 'Search files'; 'Search site'"),
                    TextBlock("Once the user has clicked into the SearchBox but hasn’t entered input yet, use 'hint text' to communicate search scope."),
                    TextBlock("Examples: 'Try searching for a PDFs'; 'Search contacts list'; 'Type to find <content type> '"),
                    TextBlock("Use the Underlined SearchBox for CommandBars.")
                ),
                Stack().Width(40, Unit.Percents).Children(
                    TextBlock("Don't").Medium(),
                    TextBlock("Don't leave the SearchBox blank because it's too ambiguous."),
                    TextBlock("Don't have lengthy and unclear hint text. It should be used to clarify and set expectations."),
                    TextBlock("Don't provide inaccurate matches or bad predictions, as it will make search seem unreliable and will result in user frustration."),
                    TextBlock("Don’t provide too much information or metadata in the suggestions list; it’s intended to be lightweight."),
                    TextBlock("Don't build a custom search control based on the default text box or any other control."),
                    TextBlock("Don't use SearchBox if you cannot reliably provide accurate results.")
                )
            )))
            .Section(Stack().Children(
                TextBlock("Usage").MediumPlus(),
                TextBlock("Basic TextBox").Medium(),
                Stack().Width(40, Unit.Percents).Children(
                    Label("Default").SetContent(SearchBox("Search").OnSearch((s,e) => alert($"Searched for {e}"))),
                    Label("Disabled").Disabled().SetContent(SearchBox("Search").Disabled()),
                    Label("Underline").SetContent(SearchBox("Search").Underlined().OnSearch((s, e) => alert($"Searched for {e}"))),
                    Label("Search as you type").SetContent(SearchBox("Search").Underlined().SearchAsYouType().OnSearch((s, e) => searchAsYouType.Text = $"Searched for {e}")),
                    searchAsYouType,
                    Label("Custom Icon").Required().SetContent(SearchBox("Filter").SetIcon("fal fa-filter").OnSearch((s, e) => alert($"Filter for {e}"))),
                    Label("No Icon").SetContent(SearchBox("Search").NoIcon().OnSearch((s, e) => alert($"Searched for {e}"))),
                    Label("Fixed Width").Required().SetContent(SearchBox("Small Search").Width(200, Unit.Pixels).OnSearch((s, e) => alert($"Searched for {e}"))))));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
