using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 20, Icon = UIcons.Search)]
    public class SearchBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchBoxSample()
        {
            var searchAsYouType = TextBlock("Start typing in the 'Search as you type' box below...");

            _content = SectionStack()
               .Title(SampleHeader(nameof(SearchBoxSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("SearchBoxes provide an input field for searching through content, allowing users to locate specific items within the website or app."),
                    TextBlock("They include a search icon and a clear button, and support both 'on search' (e.g., when Enter is pressed) and 'search as you type' behaviors.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Always use placeholder text to describe the search scope (e.g., 'Search files'). Use the 'Underlined' style for CommandBars or other minimalist surfaces. Enable 'Search as you type' for small to medium datasets where results can be filtered instantly. Provide a clear visual cue when no results are found. Don't use a SearchBox if you cannot reliably provide accurate results.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic SearchBoxes"),
                    VStack().Children(
                        Label("Default Search").SetContent(SearchBox("Search...").OnSearch((s, e) => Toast().Information($"Searched for: {e}"))),
                        Label("Underlined").SetContent(SearchBox("Search site").Underlined().OnSearch((s, e) => Toast().Information($"Searched for: {e}"))),
                        Label("Disabled").Disabled().SetContent(SearchBox("Search disabled").Disabled())
                    ),
                    SampleSubTitle("Search Behaviors"),
                    VStack().Children(
                        Label("Search as you type").SetContent(
                            SearchBox("Type something...")
                                .SearchAsYouType()
                                .OnSearch((s, e) => searchAsYouType.Text = string.IsNullOrEmpty(e) ? "Waiting for input..." : $"Current search: {e}")
                        ),
                        searchAsYouType
                    ),
                    SampleSubTitle("Customization"),
                    VStack().Children(
                        Label("Custom Icon (Filter)").SetContent(SearchBox("Filter items...").SetIcon(UIcons.Filter)),
                        Label("No Icon").SetContent(SearchBox("Iconless search").NoIcon()),
                        Label("Fixed Width (250px)").SetContent(SearchBox("Small search").Width(250.px()))
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
