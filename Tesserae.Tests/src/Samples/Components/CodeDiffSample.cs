using static Transpose.Core.dom;
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

        // Repro for the misaligned side-by-side gutter bug: a diff whose lines are long
        // prose sentences (much wider than the 50% pane), so they wrap when the
        // tss-codediff-wrap class is applied.
        private const string LongProseDiff =
@"diff --git a/lorem-notes.md b/lorem-notes.md
index 3333333..4444444 100644
--- a/lorem-notes.md
+++ b/lorem-notes.md
@@ -1,8 +1,8 @@
 Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
-Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat in voluptate velit?
+Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat?
 Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur excepteur sint occaecat.
 Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
-Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa?
+Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam?
-Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione?
+Nemo enim ipsam voluptatem quia voluptas sit magni dolores aspernatur aut odit aut fugit, sed quia consequuntur eos qui ratione?
 At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum fuga.
-Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint?
+Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae?
diff --git a/draft-notes.md b/final-notes.md
similarity index 55%
rename from draft-notes.md
rename to final-notes.md
index 5555555..6666666 100644
--- a/draft-notes.md
+++ b/final-notes.md
@@ -1,5 +1,5 @@
 Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus.
-Omnis voluptas assumenda est, omnis dolor repellendus, et harum quidem rerum facilis est et expedita distinctio nam libero?
+Omnis voluptas assumenda est, omnis dolor repellendus, et harum quidem rerum facilis est et expedita distinctio?
 Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis.
-Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur vel illum qui dolorem?
+Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur?
 Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi.
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

            var longProseDiff = CodeDiff(LongProseDiff, CodeDiff.Format.SideBySide).WS().MT(8);
            longProseDiff.Id("code-diff-long-prose").Class("tss-codediff-wrap");

            var wrapToggle = Toggle("Wrap long lines (triggers the bug)").Checked(true)
                .OnChange((s, _) =>
                {
                    if (s.IsChecked) longProseDiff.Class("tss-codediff-wrap");
                    else longProseDiff.RemoveClass("tss-codediff-wrap");
                });

            // Several CodeDiff instances on one page, including two rendering the *same*
            // diff text: diff2html derives element ids from the file names, so multiple
            // views of the same files produce duplicate DOM ids on the page.
            var multiA = CodeDiff(LongProseDiff, CodeDiff.Format.SideBySide).WS().MT(8);
            multiA.Id("code-diff-multi-a");
            var multiB = CodeDiff(LongProseDiff, CodeDiff.Format.SideBySide).WS().MT(8);
            multiB.Id("code-diff-multi-b");
            var multiC = CodeDiff(LongProseDiff, CodeDiff.Format.LineByLine).WS().MT(8);
            multiC.Id("code-diff-multi-c");

            // Line-by-line diff inside a fixed-height scroll container. diff2html's
            // line-number cells are position:absolute; .tss-codediff is position:relative
            // (tss.codediff.css) so they stay attached to their rows while scrolling.
            var scrolledDiff = CodeDiff(LongProseDiff, CodeDiff.Format.LineByLine).WS();
            scrolledDiff.Id("code-diff-scrolled");

            var scrollHost = Div(Att("code-diff-scroll-host"));
            scrollHost.style.height    = "320px";
            scrollHost.style.overflowY = "auto";
            scrollHost.style.marginTop = "8px";
            scrollHost.appendChild(scrolledDiff.Render());

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
                    )).SetTitle("Usage")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Repro: side-by-side diff of long prose lines, with line wrapping enabled (the tss-codediff-wrap class in tss-samples.css). diff2html positions the line-number cells absolutely and assumes every row is exactly one text line tall, so once lines wrap, the gutter numbers and their red/green backgrounds drift out of alignment with the content rows."),
                        wrapToggle.MT(8),
                        longProseDiff
                    )).SetTitle("Bug repro: wrapped long lines misalign the gutter (side-by-side)")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Repro: several CodeDiff instances on the same page, two of them rendering the same diff text (side-by-side) plus one line-by-line view. diff2html derives element ids from the file names, so multiple views of the same files create duplicate DOM ids - check whether the gutters of these instances stay aligned with their content rows (no wrap class applied here)."),
                        SampleSubTitle("Instance A (side-by-side)"),
                        multiA,
                        SampleSubTitle("Instance B (same diff, side-by-side)"),
                        multiB,
                        SampleSubTitle("Instance C (same diff, line-by-line)"),
                        multiC
                    )).SetTitle("Bug repro: multiple diff views on the same page")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Regression check: line-by-line diff inside a fixed-height scrollable container (320px, overflow-y auto). diff2html's line-number cells are absolutely positioned, so without a positioned ancestor inside the scroller they would stay frozen (and escape the clip) while the content scrolls. Tesserae fixes this by making the CodeDiff root position:relative (tss.codediff.css) - scroll inside the box below and the gutter must move with its rows."),
                        Raw(scrollHost)
                    )).SetTitle("Line-by-line diff inside a scroll container")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
