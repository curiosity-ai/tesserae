using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 1, Icon = UIcons.CodeCompare)]
    public class CodeDiffSample : IComponent, ISample
    {
        private readonly IComponent _content;

        private const string SampleDiff =
@"diff --git a/sample.js b/sample.js
index 0123456..abcdef0 100644
--- a/sample.js
+++ b/sample.js
@@ -1,9 +1,12 @@
-function greet(name) {
-    console.log('Hello, ' + name);
+function greet(name, greeting) {
+    greeting = greeting || 'Hello';
+    console.log(greeting + ', ' + name + '!');
 }

-greet('World');
+greet('World');
+greet('Tesserae', 'Hi');
+greet('diff2html', 'Hey');

-// TODO: support multiple greetings
+// Now supports custom greetings via the second argument.
diff --git a/README.md b/README.md
index 1111111..2222222 100644
--- a/README.md
+++ b/README.md
@@ -1,5 +1,7 @@
 # Sample

-A tiny sample script.
+A tiny sample script demonstrating the **CodeDiff** Tesserae component,
+which is backed by [diff2html](https://github.com/rtfpessoa/diff2html).

-Run it with: `node sample.js`
+Run it with:    `node sample.js`
+Or try it out: `node sample.js Tesserae`
";

        public CodeDiffSample()
        {
            var diff = CodeDiff(SampleDiff).WS();

            var editor = TextArea(SampleDiff).WS().H(220)
                .OnInput((ta, _) => diff.DiffText = ta.Text);

            var formatChoice = ChoiceGroup("Output format").Horizontal()
                .Choices(
                    Choice("Line by line").Selected(),
                    Choice("Side by side"))
                .OnChange((s, _) =>
                {
                    diff.OutputFormat = s.SelectedOption.Text == "Side by side"
                        ? CodeDiff.Format.SideBySide
                        : CodeDiff.Format.LineByLine;
                });

            var matchingChoice = ChoiceGroup("Line matching").Horizontal()
                .Choices(
                    Choice("None"),
                    Choice("Lines").Selected(),
                    Choice("Words"))
                .OnChange((s, _) =>
                {
                    switch (s.SelectedOption.Text)
                    {
                        case "None":  diff.LineMatching = CodeDiff.Matching.None;  break;
                        case "Words": diff.LineMatching = CodeDiff.Matching.Words; break;
                        default:      diff.LineMatching = CodeDiff.Matching.Lines; break;
                    }
                });

            var fileListToggle = Toggle("Show file list").Checked(false)
                .OnChange((s, _) => diff.DrawFileList = s.IsChecked);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(CodeDiffSample), UIcons.CodeCompare, "Render unified diffs with diff2html")
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("CodeDiff renders a unified diff string (the output of git diff, hg diff, etc.) using the diff2html library. The slim UI bundle and stylesheet are fetched from jsDelivr the first time a CodeDiff is shown, so no bundled dependency is required."),
                        TextBlock("Set the Diff property to update the rendering, or toggle Format between LineByLine and SideBySide. DrawFileList controls the file summary above the diff; MatchingLines (none/lines/words) tweaks diff2html's intra-line matching heuristic.").MT(8))).SetTitle("Overview")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Pass a valid unified diff. CodeDiff itself does not perform diffing - it visualizes a diff produced elsewhere (e.g. by a git command, a server endpoint, or a JavaScript diff library)."),
                        TextBlock("Side-by-side layout is best for wide screens and reviews; line-by-line is friendlier on narrow viewports and inside split panels. Use DrawFileList when rendering multi-file patches.").MT(8))).SetTitle("Best Practices")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Try it"),
                        TextBlock("Edit the unified diff on the left; the rendered diff updates live on the right. Use the controls to switch between layouts and matching strategies, mirroring the diff2html demo."),
                        HStack().AlignItemsCenter().WS().MT(8).Children(
                            formatChoice.MR(24),
                            matchingChoice.MR(24),
                            fileListToggle),
                        HStack().AlignItemsCenter().WS().MT(16).Children(
                            VStack().Grow().PR(8).Children(
                                SampleSubTitle("Unified diff input"),
                                editor),
                            VStack().Grow().PL(8).Children(
                                SampleSubTitle("Rendered output"),
                                diff))
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
