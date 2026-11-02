using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 45, Icon = UIcons.CodeMerge, Description = "A compact diff for the middle of a chat")]
    public class InlineDiffSample : IComponent, ISample
    {
        private const string BEFORE =
@"using System;

namespace Demo
{
    public static class Greeter
    {
        public static string Greet(string name)
        {
            return ""Hello, "" + name;
        }
    }
}
";

        private const string AFTER =
@"using System;

namespace Demo
{
    public static class Greeter
    {
        public static string Greet(string name, string greeting = ""Hello"")
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(""A name is needed"", nameof(name));

            return greeting + "", "" + name + ""!"";
        }
    }
}
";

        private const string TWO_FILES =
@"diff --git a/src/Greeter.cs b/src/Greeter.cs
--- a/src/Greeter.cs
+++ b/src/Greeter.cs
@@ -7,3 +7,4 @@
     {
-        return ""Hello, "" + name;
+        var greeting = ""Hello"";
+        return greeting + "", "" + name;
     }
diff --git a/README.md b/README.md
new file mode 100644
--- /dev/null
+++ b/README.md
@@ -0,0 +1,3 @@
+# Demo
+
+A tiny greeter.
";

        private static readonly string LONG_BEFORE = BuildLong(false);
        private static readonly string LONG_AFTER  = BuildLong(true);

        private readonly IComponent _content;

        public InlineDiffSample()
        {
            var beforeBox = TextArea(BEFORE).WS().H(190);
            var afterBox  = TextArea(AFTER).WS().H(190);
            var live      = VStack().WS();

            void Refresh()
            {
                live.Clear();
                live.Add(InlineDiff(beforeBox.Text, afterBox.Text, "Greeter.cs").WS());
            }

            beforeBox.OnInput((_, __) => Refresh());
            afterBox.OnInput((_, __) => Refresh());
            Refresh();

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(InlineDiffSample), UIcons.CodeMerge, "A compact diff for the middle of a conversation")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("InlineDiff draws what one step of an assistant changed in a file, small enough to sit between a tool call and the reply after it. It reads a unified diff (one file or several), or works one out from the text before and after, and draws it in plain C# - no script is loaded."),
                        TextBlock("The header carries the file name and the lines added and removed. A line that was edited in place marks only the words that changed. A diff longer than MaxLines shows its first lines and a row to reveal the rest. For a full-size review surface - side by side, syntax highlighting - use CodeDiff instead.")
                    )).SetTitle("Overview")))
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("From two texts"),
                        TextBlock("InlineDiff(before, after, fileName) compares the texts itself. Edit either one and the diff follows."),
                        HStack().WS().Children(
                            VStack().Grow().Children(TextBlock("Before").SemiBold(), beforeBox),
                            VStack().Grow().Children(TextBlock("After").SemiBold(), afterBox)),
                        live,

                        SampleSubTitle("From a unified diff"),
                        TextBlock("Several files in one diff each get their own header. A new file is marked as such."),
                        InlineDiff(TWO_FILES).WS(),

                        SampleSubTitle("Long diffs fold"),
                        TextBlock("Past MaxLines (14 by default) the rest sits behind a row. SetMaxLines changes it and ShowAllLines turns folding off."),
                        InlineDiff(LONG_BEFORE, LONG_AFTER, "settings.json").WS(),

                        SampleSubTitle("Collapsible, without line numbers"),
                        TextBlock("Collapsible lets the header fold the lines away. LineNumbers(false) drops the gutter."),
                        InlineDiff(BEFORE, AFTER, "Greeter.cs").Collapsible().LineNumbers(false).WS(),
                        InlineDiff(BEFORE, AFTER, "Greeter.cs").Collapsed().WS(),

                        SampleSubTitle("A file that was created, and one that was removed"),
                        InlineDiff("", "line one\nline two\n", "notes/new.txt").WS(),
                        InlineDiff("line one\nline two\n", "", "notes/old.txt").WS(),

                        SampleSubTitle("No wrapping"),
                        TextBlock("NoWrap lets a long line scroll sideways instead of wrapping."),
                        InlineDiff("const url = 'https://example.com/a/very/long/path/that/keeps/going/and/going/and/going?with=many&query=parameters&that=never&seem=to&end=here';\n", "const url = 'https://example.com/a/very/long/path/that/keeps/going/and/going/and/going?with=many&query=parameters&that=never&seem=to&end=there';\n", "url.js").NoWrap().W(380)
                    )).SetTitle("Examples")))
                .SeeAlso(typeof(ChatSample), typeof(ToolCallSample), typeof(CodeDiffSample));
        }

        private static string BuildLong(bool changed)
        {
            var rows = new System.Collections.Generic.List<string> { "{" };

            for (var i = 1; i <= 24; i++)
            {
                var value = changed && (i % 4 == 0 || i == 3) ? i * 10 : i;

                rows.Add("  \"option" + i + "\": " + value + (i < 24 ? "," : ""));
            }

            rows.Add("}");

            return string.Join("\n", rows) + "\n";
        }

        public HTMLElement Render() => _content.Render();
    }
}
