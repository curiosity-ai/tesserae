using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Text, Order = 60, Icon = UIcons.Paragraph, Description = "Markdown rendered as sanitized HTML")]
    public class MarkdownBlockSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public MarkdownBlockSample()
        {
            const string staticSample =
@"# MarkdownBlock

`MarkdownBlock` renders **Markdown** as sanitized HTML.

- It is backed by the [marked](https://marked.js.org/) parser
- The output is run through [DOMPurify](https://github.com/cure53/DOMPurify) before being inserted
- A blockquote:

> Markdown source goes in, safe HTML comes out.

```csharp
MarkdownBlock(""# Hello"");
```
";

            const string startingMarkdown =
@"## Try editing this!

Type some markdown below to see it rendered live.

- **Bold**, *italic*, and ~~strike~~
- Lists, [links](https://github.com/curiosity-ai/tesserae), and code

| Feature  | Supported |
| -------- | --------- |
| GFM      | yes       |
| Sanitize | yes       |
";

            const string paranoidSample =
@"A [link to somewhere](https://example.com/phishing) reads as its label only, an autolink like
https://example.com/tracker is plain text, and this image never loads:

![a pixel that would call home](https://example.com/pixel.png)
";

            const string tableSample =
@"| Region | Q1    | Q2    | Notes                |
| :----- | ----: | ----: | -------------------- |
| North  | 1,204 | 1,388 | Up on ""new"" accounts |
| South  |   980 |   912 | Flat, one churn      |
| West   | 1,530 | 1,702 | Best quarter so far  |
";

            const string diagramSample =
@"Each of these is a mermaid code fence in the source:

```mermaid
flowchart LR
  A[Markdown] --> B{Has a mermaid fence?}
  B -- yes --> C[Pele draws it]
  B -- no --> D[Plain HTML]
  C --> E([Themed SVG])
  D --> E
```

```mermaid
sequenceDiagram
  participant U as User
  participant M as MarkdownBlock
  U->>M: Text = ""..."" 
  M-->>U: Rendered diagram
```

```mermaid
pie title Where the time goes
  ""Parsing"" : 12
  ""Layout"" : 30
  ""Painting"" : 58
```
";

            const string chartSample =
@"Each chart below is a `flint` code fence holding a Flint chart spec: the data, a semantic type per field, and the chart.

```flint
{
  ""data"": { ""values"": [
    { ""Region"": ""North"", ""Revenue"": 1204 },
    { ""Region"": ""South"", ""Revenue"": 980 },
    { ""Region"": ""East"", ""Revenue"": 1530 },
    { ""Region"": ""West"", ""Revenue"": 1388 }
  ] },
  ""semantic_types"": { ""Region"": ""Category"", ""Revenue"": ""Amount"" },
  ""chart_spec"": {
    ""chartType"": ""Bar Chart"",
    ""title"": ""East leads the quarter"",
    ""subtitle"": ""Revenue by region, Q2, thousands of euros"",
    ""encodings"": { ""x"": { ""field"": ""Region"" }, ""y"": { ""field"": ""Revenue"" } }
  }
}
```

An `interaction_spec` makes a chart interactive - hover the line chart to read every series at a month:

```flint
{
  ""data"": { ""values"": [
    { ""Month"": ""2026-01"", ""Product"": ""Search"", ""Users"": 42 }, { ""Month"": ""2026-01"", ""Product"": ""Chat"", ""Users"": 12 },
    { ""Month"": ""2026-02"", ""Product"": ""Search"", ""Users"": 48 }, { ""Month"": ""2026-02"", ""Product"": ""Chat"", ""Users"": 15 },
    { ""Month"": ""2026-03"", ""Product"": ""Search"", ""Users"": 51 }, { ""Month"": ""2026-03"", ""Product"": ""Chat"", ""Users"": 21 },
    { ""Month"": ""2026-04"", ""Product"": ""Search"", ""Users"": 57 }, { ""Month"": ""2026-04"", ""Product"": ""Chat"", ""Users"": 26 },
    { ""Month"": ""2026-05"", ""Product"": ""Search"", ""Users"": 63 }, { ""Month"": ""2026-05"", ""Product"": ""Chat"", ""Users"": 34 },
    { ""Month"": ""2026-06"", ""Product"": ""Search"", ""Users"": 70 }, { ""Month"": ""2026-06"", ""Product"": ""Chat"", ""Users"": 41 },
    { ""Month"": ""2026-07"", ""Product"": ""Search"", ""Users"": 74 }, { ""Month"": ""2026-07"", ""Product"": ""Chat"", ""Users"": 50 },
    { ""Month"": ""2026-08"", ""Product"": ""Search"", ""Users"": 79 }, { ""Month"": ""2026-08"", ""Product"": ""Chat"", ""Users"": 58 },
    { ""Month"": ""2026-09"", ""Product"": ""Search"", ""Users"": 85 }, { ""Month"": ""2026-09"", ""Product"": ""Chat"", ""Users"": 64 },
    { ""Month"": ""2026-10"", ""Product"": ""Search"", ""Users"": 88 }, { ""Month"": ""2026-10"", ""Product"": ""Chat"", ""Users"": 73 },
    { ""Month"": ""2026-11"", ""Product"": ""Search"", ""Users"": 94 }, { ""Month"": ""2026-11"", ""Product"": ""Chat"", ""Users"": 80 },
    { ""Month"": ""2026-12"", ""Product"": ""Search"", ""Users"": 101 }, { ""Month"": ""2026-12"", ""Product"": ""Chat"", ""Users"": 92 }
  ] },
  ""semantic_types"": { ""Month"": ""YearMonth"", ""Product"": ""Category"", ""Users"": ""Count"" },
  ""chart_spec"": {
    ""chartType"": ""Line Chart"",
    ""title"": ""Chat catches up with search"",
    ""subtitle"": ""Monthly active users, thousands"",
    ""encodings"": { ""x"": { ""field"": ""Month"" }, ""y"": { ""field"": ""Users"" }, ""color"": { ""field"": ""Product"" } }
  },
  ""interaction_spec"": { ""interactions"": [ { ""type"": ""inspect-index"" } ] }
}
```
";

            const string stackedChart =
@"```flint
{
  ""data"": { ""values"": [
    { ""Quarter"": ""Q1"", ""Segment"": ""Hardware"", ""Sales"": 30 }, { ""Quarter"": ""Q1"", ""Segment"": ""Software"", ""Sales"": 45 }, { ""Quarter"": ""Q1"", ""Segment"": ""Services"", ""Sales"": 20 },
    { ""Quarter"": ""Q2"", ""Segment"": ""Hardware"", ""Sales"": 28 }, { ""Quarter"": ""Q2"", ""Segment"": ""Software"", ""Sales"": 52 }, { ""Quarter"": ""Q2"", ""Segment"": ""Services"", ""Sales"": 24 },
    { ""Quarter"": ""Q3"", ""Segment"": ""Hardware"", ""Sales"": 35 }, { ""Quarter"": ""Q3"", ""Segment"": ""Software"", ""Sales"": 58 }, { ""Quarter"": ""Q3"", ""Segment"": ""Services"", ""Sales"": 27 },
    { ""Quarter"": ""Q4"", ""Segment"": ""Hardware"", ""Sales"": 40 }, { ""Quarter"": ""Q4"", ""Segment"": ""Software"", ""Sales"": 66 }, { ""Quarter"": ""Q4"", ""Segment"": ""Services"", ""Sales"": 31 }
  ] },
  ""semantic_types"": { ""Quarter"": ""Category"", ""Segment"": ""Category"", ""Sales"": ""Amount"" },
  ""chart_spec"": {
    ""chartType"": ""Stacked Bar Chart"",
    ""title"": ""Software carries the year"",
    ""subtitle"": ""Sales by segment, millions"",
    ""encodings"": { ""x"": { ""field"": ""Quarter"" }, ""y"": { ""field"": ""Sales"" }, ""color"": { ""field"": ""Segment"" } }
  }
}
```";

            const string brokenChart =
@"A fence that cannot be drawn stays the code block it is, with Flint's reason as its tooltip:

```flint
{
  ""data"": { ""values"": [ { ""Team"": ""Core"", ""Commits"": 120 } ] },
  ""semantic_types"": { ""Team"": ""Category"", ""Commits"": ""Count"" },
  ""chart_spec"": { ""chartType"": ""Bar Chart"", ""encodings"": { ""x"": { ""field"": ""Team"" }, ""y"": { ""field"": ""Missing"" } } }
}
```";

            var themedChart  = MarkdownBlock(stackedChart).Flint(FlintThemes.Tesserae);
            var themeOptions = new (string Label, IFlintTheme Theme)[]
            {
                ("Tesserae (follows the page)", FlintThemes.Tesserae),
                ("Tesserae over the Economist", FlintThemes.TesseraeOver("economist")),
                ("Flint's own defaults",        FlintThemes.None),
                ("The Economist",               FlintThemes.Economist),
                ("The New York Times",          FlintThemes.Nyt),
                ("Swiss",                       FlintThemes.Swiss),
                ("Nature",                      FlintThemes.Nature),
                ("McKinsey",                    FlintThemes.McKinsey),
                ("Datawrapper",                 FlintThemes.Datawrapper),
                ("Power BI",                    FlintThemes.PowerBI),
                ("Power BI (light)",            FlintThemes.PowerBILight),
                ("Pop",                         FlintThemes.Pop),
                ("Cartoon",                     FlintThemes.Cartoon),
                ("Apple",                       FlintThemes.Apple),
                ("De Stijl",                    FlintThemes.DeStijl),
                ("BBC",                         FlintThemes.Bbc),
                ("Material",                    FlintThemes.Material)
            };

            var themeItems = new Dropdown.Item[themeOptions.Length];
            for (int i = 0; i < themeOptions.Length; i++)
            {
                var theme = themeOptions[i].Theme;
                themeItems[i] = DropdownItem(themeOptions[i].Label).OnSelected(_ => themedChart.Flint(theme));
            }
            themeItems[0].Selected();

            var themePicker = Dropdown().W(280).Items(themeItems);

            var live   = MarkdownBlock(startingMarkdown);
            var editor = TextArea(startingMarkdown).WS().H(220).OnInput((ta, _) => live.Text = ta.Text);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(MarkdownBlockSample), UIcons.Paragraph, "A component that renders Markdown as sanitized HTML")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("MarkdownBlock turns a Markdown source string into safely-rendered HTML. The bundled marked parser produces the HTML and DOMPurify strips anything unsafe before it ever reaches the DOM, so it is safe to feed user-authored content."),
                        TextBlock("The component exposes a single Text property: assigning to it re-renders the output, which makes it easy to drive from a TextArea, an observable, or streamed assistant output."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Prefer MarkdownBlock over treating raw HTML strings as text - the sanitization step protects against script injection from third-party content. Keep MarkdownBlock inside a width-constrained container so wide tables and code blocks scroll instead of breaking the layout. For purely static labels, plain TextBlock is cheaper - reach for MarkdownBlock when the input genuinely contains Markdown."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Static Markdown"),
                        MarkdownBlock(staticSample),
                        SampleSubTitle("Live editing"),
                        TextBlock("Edit the Markdown on the left, the rendered output updates on the right."),
                        HStack().WS().Children(
                            editor.Grow(),
                            VStack().Grow().PL(16).Children(live)
                        ),
                        SampleSubTitle("Copying tables"),
                        TextBlock("Every table gets two buttons beside it: Copy Table puts it on the clipboard as HTML, so it pastes as a table into a document, a spreadsheet or an email, and Copy as CSV puts it there as comma-separated text. Turn them off with TableCopyButtons(false)."),
                        MarkdownBlock(tableSample),
                        SampleSubTitle("Mermaid diagrams"),
                        TextBlock("A mermaid code fence is drawn as a diagram by Pele, which renders Mermaid syntax to SVG and takes its colors and fonts from the Tesserae theme, so it follows light and dark mode and any custom theme without being redrawn. Pele is loaded the first time a diagram appears; a block that cannot be drawn stays a code block."),
                        MarkdownBlock(diagramSample),
                        SampleSubTitle("Flint charts"),
                        TextBlock("With .Flint() on, a flint code fence (or flint-chart, as Flint's own documentation names it) is drawn as a chart by Flint, Microsoft's chart language: the fence says what the data means and which chart to draw, and Flint works out the scales, labels, layout and colours, drawing it with Vega-Lite. Flint and Vega-Lite are loaded the first time a chart appears, and a fence that cannot be drawn stays a code block. Charts are drawn in FlintThemes.Tesserae unless you pass another theme, which takes its colours and fonts from the Tesserae tokens around the chart and redraws when the page theme changes."),
                        MarkdownBlock(chartSample).Flint(),
                        SampleSubTitle("Flint themes"),
                        TextBlock("Pass an IFlintTheme to Flint(...) to pick the look: one of the fourteen presets Flint ships, TesseraeOver(preset) for a preset's layout in the page's colours, or FromJson(...) for a ThemeSpec of your own. A theme_spec in the fence wins over the one passed here unless Flint(theme, overrideFenceTheme: true) is used."),
                        Label("Theme").SetContent(themePicker),
                        themedChart,
                        MarkdownBlock(brokenChart).Flint(),
                        SampleSubTitle("Sanitization"),
                        TextBlock("MarkdownBlock will strip dangerous HTML even when it is embedded inside Markdown:"),
                        MarkdownBlock("This `<script>alert('xss')</script>` will not run, and this <img src=x onerror=alert(1)> attribute is stripped too."),
                        SampleSubTitle("No links or embedded content"),
                        TextBlock("For Markdown you don't trust to link or to load a remote URL - an LLM reply, say - pass MarkdownSanitization.NoLinksOrEmbeddedContent. Anchors keep their label as plain text and images are dropped:"),
                        MarkdownBlock(paranoidSample, MarkdownSanitization.NoLinksOrEmbeddedContent)
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(TextBlockSample), typeof(ChatSample), typeof(CodeDiffSample), typeof(SanitizeHTMLSample), typeof(AnnotatedTextEditorSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
