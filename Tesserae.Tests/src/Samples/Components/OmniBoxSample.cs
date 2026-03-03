using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 21, Icon = UIcons.Search)]
    public class OmniBoxSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public OmniBoxSample()
        {
            var searchOutput = TextBlock("Waiting for input...");

            var defaultSearch = OmniBox("Type something like: potato AND ( tomato OR banana) AND NOT apple")
                .Width(100.percent())
                .WithHistory(async () => {
                    // Simulate fetching history from some server/storage
                    await Task.Delay(200);
                    return OmniBox.ParseQuery("potato AND ( tomato OR banana) AND NOT apple");
                })
                .OnSearch((s, q) =>
                {
                    searchOutput.Text = string.IsNullOrEmpty(q.RawQuery)
                        ? "Cleared."
                        : $"Searched for: {q.RawQuery} (Parsed into {q.Tokens.Count} tokens)";
                });

            // Set the prefilled content explicitly to demonstrate functionality
            defaultSearch.Text = "potato AND ( tomato OR banana) AND NOT apple";

            _content = SectionStack()
               .Title(SampleHeader(nameof(OmniBoxSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("OmniBox provides a powerful input field for searching through content, supporting parsing and visual rendering of logical operators like AND, OR, NOT, parenthesis, and quotes."),
                    TextBlock("It includes a search history button, an internal clear button, and a search trigger.")))
               .Section(Stack().Width(100.percent()).Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Usage with History"),
                    VStack().Width(100.percent()).Children(
                        Label("Advanced Search").SetContent(defaultSearch),
                        searchOutput
                    ),
                    SampleSubTitle("Customization"),
                    VStack().Width(100.percent()).Children(
                        Label("Disabled").Disabled().SetContent(OmniBox("Search disabled").Disabled()),
                        Label("Small Text Size").SetContent(OmniBox("Small search...").Do(s => s.Size = TextSize.Small)),
                        Label("Medium Text Size").SetContent(OmniBox("Medium search...").Do(s => s.Size = TextSize.Medium))
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
