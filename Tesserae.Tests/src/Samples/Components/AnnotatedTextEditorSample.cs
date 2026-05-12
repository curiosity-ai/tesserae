using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 22, Icon = UIcons.Highlighter)]
    public class AnnotatedTextEditorSample : IComponent, ISample
    {
        private readonly IComponent _content;

        // (canonical-form, label, background, color, border)
        private static readonly (string Phrase, string Label, string Background, string Color, string Border)[] _vocabulary = new[]
        {
            ("Curiosity GmbH",     "ORG",     "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Curiosity",          "ORG",     "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Anthropic",          "ORG",     "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("OpenAI",             "ORG",     "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Microsoft",          "ORG",     "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Tesserae",           "PRODUCT", "var(--tss-colors-blue-100)",   "var(--tss-colors-blue-900)",   "var(--tss-colors-blue-500)"),
            ("Claude",             "PRODUCT", "var(--tss-colors-blue-100)",   "var(--tss-colors-blue-900)",   "var(--tss-colors-blue-500)"),
            ("GPT-4",              "PRODUCT", "var(--tss-colors-blue-100)",   "var(--tss-colors-blue-900)",   "var(--tss-colors-blue-500)"),
            ("Berlin",             "GPE",     "var(--tss-colors-green-100)",  "var(--tss-colors-green-900)",  "var(--tss-colors-green-500)"),
            ("Munich",             "GPE",     "var(--tss-colors-green-100)",  "var(--tss-colors-green-900)",  "var(--tss-colors-green-500)"),
            ("San Francisco",      "GPE",     "var(--tss-colors-green-100)",  "var(--tss-colors-green-900)",  "var(--tss-colors-green-500)"),
            ("Germany",            "GPE",     "var(--tss-colors-green-100)",  "var(--tss-colors-green-900)",  "var(--tss-colors-green-500)"),
            ("Europe",             "GPE",     "var(--tss-colors-green-100)",  "var(--tss-colors-green-900)",  "var(--tss-colors-green-500)"),
            ("Alice",              "PERSON",  "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"),
            ("Bob",                "PERSON",  "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"),
            ("Jules",              "PERSON",  "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"),
            ("2024",               "DATE",    "var(--tss-colors-magenta-100)",   "var(--tss-colors-magenta-900)",   "var(--tss-colors-magenta-500)"),
            ("2025",               "DATE",    "var(--tss-colors-magenta-100)",   "var(--tss-colors-magenta-900)",   "var(--tss-colors-magenta-500)"),
            ("2026",               "DATE",    "var(--tss-colors-magenta-100)",   "var(--tss-colors-magenta-900)",   "var(--tss-colors-magenta-500)"),
            ("January",            "DATE",    "var(--tss-colors-magenta-100)",   "var(--tss-colors-magenta-900)",   "var(--tss-colors-magenta-500)"),
            ("$1.5 billion",       "MONEY",   "var(--tss-colors-yellow-100)", "var(--tss-colors-yellow-900)", "var(--tss-colors-yellow-500)"),
            ("$200",               "MONEY",   "var(--tss-colors-yellow-100)", "var(--tss-colors-yellow-900)", "var(--tss-colors-yellow-500)")
        };

        private const string SampleText =
@"Curiosity GmbH, based in Munich, Germany, is the company behind Tesserae.
In January 2026, Alice and Bob met in San Francisco to discuss a $1.5 billion partnership between Anthropic and OpenAI.
Claude and GPT-4 are large language models. Jules built a demo using Tesserae for Microsoft in 2025.";

        public AnnotatedTextEditorSample()
        {
            var entityCountLabel = TextBlock("0 entities").Small().Foreground(Theme.Secondary.Foreground);

            var editor = AnnotatedTextEditor(
                annotator:   AnnotateAsync,
                initialText: SampleText,
                debounceMs:  500,
                placeholder: "Type some text and entities will be highlighted automatically...")
                .MinHeight(160.px())
                .OnAnnotationsChanged((s, entities) =>
                {
                    entityCountLabel.Text = entities.Length == 1 ? "1 entity" : $"{entities.Length} entities";
                })
                .OnEntityClick((s, entity, e) =>
                {
                    Toast().Information($"Clicked entity: \"{s.Text.Substring(entity.Start, entity.Length)}\" ({entity.Label})");
                });
            
            var readOnlyEditor = AnnotatedTextEditor(
                annotator:   AnnotateAsync,
                initialText: "In 2025, Alice and Bob visited Munich to demo Tesserae for Curiosity GmbH.",
                debounceMs:  500)
                .MinHeight(60.px())
                .ReadOnly()
                .OnEntityClick((s, entity, e) =>
                {
                    Toast().Success($"{entity.Label}: {s.Text.Substring(entity.Start, entity.Length)}");
                });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(AnnotatedTextEditorSample), UIcons.Highlighter, "Annotated text editor")
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("AnnotatedTextEditor is a multi-line editable text field that highlights NLP entities in place, like the OmniBox token rendering. A debounced async lambda (default 500ms) is called after the user stops typing and returns the entities found in the text."),
                        TextBlock("Try editing the text below — entities are re-detected after you pause typing.").MT(8).Small().Foreground(Theme.Secondary.Foreground)
                    )).SetTitle("Overview")))
               .FlatSection(VStack().WS().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Multi-line, editable, with entity highlighting"),
                        entityCountLabel.MB(8),
                        editor.WS(),
                        SampleSubTitle("Read-only with clickable entities").MT(16),
                        TextBlock("Text cannot be edited, but entities still react to clicks.").Small().Foreground(Theme.Secondary.Foreground).MB(8),
                        readOnlyEditor.WS()
                    )).SetTitle("Usage")));
        }

        private static async Task<AnnotatedTextEditor.Entity[]> AnnotateAsync(string text)
        {
            // Simulate latency (network / model inference)
            await Task.Delay(150);

            if (string.IsNullOrEmpty(text)) return new AnnotatedTextEditor.Entity[0];

            var found = new List<AnnotatedTextEditor.Entity>();

            // Match longest phrases first so "Curiosity GmbH" wins over "Curiosity"
            foreach (var v in _vocabulary.OrderByDescending(v => v.Phrase.Length))
            {
                int idx = 0;
                while (idx < text.Length)
                {
                    int found_idx = text.IndexOf(v.Phrase, idx, StringComparison.OrdinalIgnoreCase);
                    if (found_idx < 0) break;

                    // Skip if overlaps an already-found entity
                    int end = found_idx + v.Phrase.Length;
                    bool overlaps = found.Any(e => found_idx < e.End && end > e.Start);
                    if (!overlaps)
                    {
                        found.Add(new AnnotatedTextEditor.Entity(found_idx, v.Phrase.Length, v.Label, v.Background, v.Color, v.Border));
                    }
                    idx = end;
                }
            }

            return found.OrderBy(e => e.Start).ToArray();
        }

        public HTMLElement Render() => _content.Render();
    }
}
