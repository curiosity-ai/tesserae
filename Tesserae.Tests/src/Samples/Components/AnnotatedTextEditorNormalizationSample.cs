using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    /// <summary>
    /// Self-checking tests for AnnotatedTextEditor's newline normalization. A &lt;textarea&gt;
    /// stores "\r\n"/"\r" as "\n", but token-detection engines return offsets into the raw,
    /// un-normalized text. Each case first asserts that applying the raw offsets straight to the
    /// normalized text would be wrong (i.e. there really is something to fix), then asserts the
    /// editor remaps them so the highlight covers the right characters.
    /// </summary>
    [SampleDetails(Group = "Components", Order = 23, Icon = UIcons.Highlighter)]
    public class AnnotatedTextEditorNormalizationSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private readonly Stack      _results;

        // (case name, raw text with explicit CR/LF, phrases an engine would detect in the RAW text)
        private static readonly (string Name, string Raw, string[] Phrases)[] _cases = new[]
        {
            ("No newlines (identity)",          "Alice met Bob in Berlin",     new[] { "Alice", "Bob", "Berlin" }),
            ("Single CRLF shifts later text",   "Alice\r\nmet Bob in Berlin",  new[] { "Alice", "Bob", "Berlin" }),
            ("Multiple CRLFs compound",         "A\r\nBB\r\nCCC\r\nDDDD",       new[] { "BB", "CCC", "DDDD" }),
            ("Lone CR -> LF (no shift)",        "Alice\rmet Bob",              new[] { "Bob" }),
            ("Mixed CRLF and LF",               "A\r\nBob\nCarol\r\nDave",      new[] { "Bob", "Carol", "Dave" }),
            ("Entity spans a CRLF",             "Hello\r\nWorld done",         new[] { "Hello\r\nWorld" }),
            ("CRLF at the very start",          "\r\nXYZ here",                new[] { "XYZ" })
        };

        public AnnotatedTextEditorNormalizationSample()
        {
            _results = VStack().WS();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(AnnotatedTextEditorNormalizationSample), UIcons.Highlighter, "AnnotatedTextEditor — newline normalization tests")
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("These run automatically. The detection engine returns offsets into the raw text (with \"\\r\\n\"/\"\\r\"); the textarea normalizes those line endings to \"\\n\". Each case checks that (a) the raw offset applied directly would be wrong — so there is something to fix — and (b) the editor remaps the offset so the highlight still lands on the right characters.")
                           .Small().Foreground(Theme.Secondary.Foreground).MB(8),
                        _results
                    )).SetTitle("Newline normalization tests")));

            RunAllAsync().FireAndForget();
        }

        private async Task RunAllAsync()
        {
            int passed = 0;

            foreach (var c in _cases)
            {
                var result = await RunCaseAsync(c.Name, c.Raw, c.Phrases);
                if (result.Pass) passed++;
                _results.Add(result.Row);
            }

            var allPass = passed == _cases.Length;
            _results.Add(TextBlock($"{passed}/{_cases.Length} cases passed")
               .Bold()
               .Foreground(allPass ? "var(--tss-colors-green-700)" : "var(--tss-colors-red-700)")
               .MT(8));
        }

        private static async Task<(bool Pass, IComponent Row)> RunCaseAsync(string name, string raw, string[] phrases)
        {
            // Offsets into the RAW text, exactly as an engine running on the un-normalized string
            // would produce them.
            var rawStarts = phrases.Select(p => raw.IndexOf(p, StringComparison.Ordinal)).ToArray();

            var editor = AnnotatedTextEditor(
                annotator: _ => Task.FromResult(
                    phrases
                       .Select((p, i) => new AnnotatedTextEditor.Entity(rawStarts[i], p.Length, p))
                       .ToArray()),
                debounceMs: 1);

            var tcs = new TaskCompletionSource<AnnotatedTextEditor.Entity[]>();
            editor.OnAnnotationsChanged((s, ents) => tcs.TrySetResult(ents));
            editor.SetText(raw);

            var entities       = await tcs.Task;
            var normalizedText = editor.Text; // textarea-normalized value the highlights render against

            var pass     = true;
            var needsFix = false;
            var details  = new List<string>();

            // Text exposed by the editor must be the normalized form of what we set.
            if (normalizedText != Normalize(raw)) { pass = false; details.Add("Text not normalized"); }
            if (entities.Length != phrases.Length) { pass = false; details.Add($"expected {phrases.Length} entities, got {entities.Length}"); }

            for (int i = 0; i < phrases.Length && i < entities.Length; i++)
            {
                var expected = Normalize(phrases[i]);                          // what the highlight should cover
                var naive    = Safe(normalizedText, rawStarts[i], phrases[i].Length); // raw offset, applied as-is
                var actual   = Safe(normalizedText, entities[i].Start, entities[i].Length);

                if (naive  != expected) needsFix = true; // the raw offset would land on the wrong characters
                if (actual != expected) pass     = false;

                details.Add($"want \"{Vis(expected)}\" · editor \"{Vis(actual)}\" · raw-as-is \"{Vis(naive)}\"");
            }

            var icon  = pass ? "✓" : "✗";
            var color = pass ? "var(--tss-colors-green-700)" : "var(--tss-colors-red-700)";
            var tag   = needsFix ? "raw offsets would be wrong → remap exercised" : "no shift (identity)";

            var row = VStack().WS().Children(
                TextBlock($"{icon}  {name}  —  {tag}").Foreground(color),
                TextBlock(string.Join("    |    ", details)).XSmall().Foreground(Theme.Secondary.Foreground)
            ).MB(8);

            return (pass, row);
        }

        private static string Normalize(string s) => (s ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');

        // Substring that never throws, so a wrong offset shows up as visibly-wrong text or "<oob>"
        // instead of crashing the test runner.
        private static string Safe(string s, int start, int length)
        {
            if (start < 0 || length < 0 || start > s.Length) return "<oob>";
            if (start + length > s.Length) length = s.Length - start;
            return s.Substring(start, length);
        }

        // Render line breaks visibly so a drifted highlight is obvious in the report.
        private static string Vis(string s) => (s ?? string.Empty).Replace("\n", "⏎");

        public HTMLElement Render() => _content.Render();
    }
}
