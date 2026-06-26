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
            ("Curiosity GmbH", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Curiosity", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Anthropic", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("OpenAI", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Microsoft", "ORG", "var(--tss-colors-purple-100)", "var(--tss-colors-purple-900)", "var(--tss-colors-purple-500)"),
            ("Tesserae", "PRODUCT", "var(--tss-colors-blue-100)", "var(--tss-colors-blue-900)", "var(--tss-colors-blue-500)"),
            ("Claude", "PRODUCT", "var(--tss-colors-blue-100)", "var(--tss-colors-blue-900)", "var(--tss-colors-blue-500)"),
            ("GPT-4", "PRODUCT", "var(--tss-colors-blue-100)", "var(--tss-colors-blue-900)", "var(--tss-colors-blue-500)"),
            ("Berlin", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"),
            ("Munich", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"),
            ("San Francisco", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"),
            ("Germany", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"),
            ("Europe", "GPE", "var(--tss-colors-green-100)", "var(--tss-colors-green-900)", "var(--tss-colors-green-500)"),
            ("Alice", "PERSON", "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"),
            ("Bob", "PERSON", "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"),
            ("Jules", "PERSON", "var(--tss-colors-orange-100)", "var(--tss-colors-orange-900)", "var(--tss-colors-orange-500)"),
            ("2024", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"),
            ("2025", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"),
            ("2026", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"),
            ("January", "DATE", "var(--tss-colors-magenta-100)", "var(--tss-colors-magenta-900)", "var(--tss-colors-magenta-500)"),
            ("$1.5 billion", "MONEY", "var(--tss-colors-yellow-100)", "var(--tss-colors-yellow-900)", "var(--tss-colors-yellow-500)"),
            ("$200", "MONEY", "var(--tss-colors-yellow-100)", "var(--tss-colors-yellow-900)", "var(--tss-colors-yellow-500)")
        };

        private const string SampleText =
            @"Curiosity GmbH, based in Munich, Germany, is the company behind Tesserae.
In January 2026, Alice and Bob met in San Francisco to discuss a $1.5 billion partnership between Anthropic and OpenAI.
Claude and GPT-4 are large language models. Jules built a demo using Tesserae for Microsoft in 2025.";

        public AnnotatedTextEditorSample()
        {
            var entityCountLabel = TextBlock("0 entities").Small().Foreground(Theme.Secondary.Foreground);

            var editor = AnnotatedTextEditor(
                    annotator: AnnotateAsync,
                    initialText: SampleText + "\n" + SampleText + "\n",
                    debounceMs: 500,
                    placeholder: "Type some text and entities will be highlighted automatically...")
               .MinHeight(160.px())
               .OnAnnotationsChanged((s, entities) =>
                {
                    entityCountLabel.Text = entities.Length == 1 ? "1 entity" : $"{entities.Length} entities";
                })
               .OnEntityClick((s, entity, e) =>
                {
                    Toast().Information($"Clicked entity: \"{s.Text.Substring(entity.Start, entity.Length)}\"");
                });

            var allTokensEditor = AnnotatedTextEditor(
                    annotator: AnnotateAllTokensAsync,
                    initialText: SampleText,
                    debounceMs: 500)
               .MinHeight(120.px())
               .OnEntityClick((s, entity, e) =>
                {
                    Toast().Information($"Token: \"{s.Text.Substring(entity.Start, entity.Length)}\"");
                });

            var readOnlyEditor = AnnotatedTextEditor(
                    annotator: AnnotateAsync,
                    initialText: "In 2025, Alice and Bob visited Munich to demo Tesserae for Curiosity GmbH.",
                    debounceMs: 500)
               .MinHeight(60.px())
               .ReadOnly()
               .OnEntityClick((s, entity, e) =>
                {
                    Toast().Success(s.Text.Substring(entity.Start, entity.Length));
                });

            // CRLF robustness: this text uses explicit Windows ("\r\n") line endings. A
            // <textarea> stores those as "\n", but AnnotateAsync (standing in for a real
            // detection engine) runs over the raw "\r\n" string and returns offsets in that
            // raw coordinate space. The editor remaps them into the normalized space, so the
            // highlights must still land exactly on the matched words even though every offset
            // after a line break is shifted by the dropped "\r".
            var crlfText =
                "Curiosity GmbH is based in Munich, Germany.\r\n" +
                "Anthropic, OpenAI and Microsoft build Claude and GPT-4.\r\n" +
                "Jules demoed Tesserae in San Francisco in 2025.";

            var crlfEditor = AnnotatedTextEditor(
                    annotator: AnnotateAsync,
                    initialText: crlfText,
                    debounceMs: 500)
               .MinHeight(80.px())
               .OnEntityClick((s, entity, e) =>
                {
                    Toast().Information($"Clicked entity: \"{s.Text.Substring(entity.Start, entity.Length)}\"");
                });

            // Multi-line hover tooltip: the string-label convenience constructor wraps the
            // text in a `.tss-annotated-text-default-label` div whose `white-space: pre-line`
            // renders '\n' as line breaks and whose `max-width` wraps long lines.
            var multiLineTooltipEditor = AnnotatedTextEditor(
                    annotator: AnnotateMultiLineAsync,
                    initialText: "Hover \"Curiosity\" for a short multi-line tooltip; hover \"Tesserae\" for a longer one that should wrap. Hover \"ghost\" — its inline highlight is transparent, but the tooltip must stay readable.",
                    debounceMs: 200,
                    placeholder: "Hover an entity to see a multi-line tooltip...")
               .MinHeight(80.px());

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
                        SampleSubTitle("Every token annotated, matched ones colored").MT(16),
                        TextBlock("Every word is wrapped in a gray pill, and the vocabulary matches override the gray with their typed color.").Small().Foreground(Theme.Secondary.Foreground).MB(8),
                        allTokensEditor.WS(),
                        SampleSubTitle("Read-only with clickable entities").MT(16),
                        TextBlock("Text cannot be edited, but entities still react to clicks.").Small().Foreground(Theme.Secondary.Foreground).MB(8),
                        readOnlyEditor.WS(),
                        SampleSubTitle("Windows (CRLF) line endings").MT(16),
                        TextBlock("The detection engine runs on the raw text (with \"\\r\\n\"), but the textarea normalizes line endings to \"\\n\". Highlights stay aligned because the editor remaps the raw offsets into the normalized text.").Small().Foreground(Theme.Secondary.Foreground).MB(8),
                        crlfEditor.WS(),
                        SampleSubTitle("Multi-line hover tooltips").MT(16),
                        TextBlock("String-label entities can contain '\\n' to render as multi-line hover tooltips. Long lines wrap at the tooltip's max-width. \"ghost\" exercises the transparent-background case — the inline highlight is invisible, but the tooltip must still be readable (default styling kicks in). For rich content set Entity.LabelContent to your own IComponent.").Small().Foreground(Theme.Secondary.Foreground).MB(8),
                        multiLineTooltipEditor.WS()
                    )).SetTitle("Usage")));
        }

        private static async Task<AnnotatedTextEditor.Entity[]> AnnotateMultiLineAsync(string text)
        {
            await Task.Delay(50);

            if (string.IsNullOrEmpty(text)) return new AnnotatedTextEditor.Entity[0];

            var result = new List<AnnotatedTextEditor.Entity>();

            // Short multi-line label (3 lines).
            int shortIdx = text.IndexOf("Curiosity", StringComparison.Ordinal);
            if (shortIdx >= 0)
            {
                const string shortLabel =
                    "ORG — Curiosity\n" +
                    "Founded: 2019\n" +
                    "Location: Munich, Germany";

                result.Add(new AnnotatedTextEditor.Entity(
                    shortIdx,
                    "Curiosity".Length,
                    shortLabel,
                    "var(--tss-colors-purple-100)",
                    "var(--tss-colors-purple-900)",
                    "var(--tss-colors-purple-500)"));
            }

            // Long multi-line label that should wrap at max-width.
            int longIdx = text.IndexOf("Tesserae", StringComparison.Ordinal);
            if (longIdx >= 0)
            {
                const string longLabel =
                    "PRODUCT — Tesserae\n" +
                    "A C# UI toolkit compiled to JavaScript via h5.\n" +
                    "This line is intentionally long so it has to wrap onto a second line inside the tooltip's max-width box — confirming the hover tag handles wrapping cleanly.\n" +
                    "Last line: short.";

                result.Add(new AnnotatedTextEditor.Entity(
                    longIdx,
                    "Tesserae".Length,
                    longLabel,
                    "var(--tss-colors-blue-100)",
                    "var(--tss-colors-blue-900)",
                    "var(--tss-colors-blue-500)"));
            }

            // Transparent inline highlight, but multi-line tooltip should still render in the
            // default styling — confirms that the hover tag falls back to a readable
            // background/color/border when the entity passes "transparent" (and "rgba(...,0)").
            int ghostIdx = text.IndexOf("ghost", StringComparison.Ordinal);
            if (ghostIdx >= 0)
            {
                const string ghostLabel =
                    "GHOST — invisible chip\n" +
                    "Inline highlight: transparent (no chip is drawn).\n" +
                    "Tooltip: should keep the default readable styling, not inherit transparent.";

                result.Add(new AnnotatedTextEditor.Entity(
                    ghostIdx,
                    "ghost".Length,
                    ghostLabel,
                    background: "transparent",
                    color:      "rgba(0, 0, 0, 0)",
                    border:     "transparent"));
            }

            return result.OrderBy(e => e.Start).ToArray();
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
                    int  end      = found_idx + v.Phrase.Length;
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

        private static async Task<AnnotatedTextEditor.Entity[]> AnnotateAllTokensAsync(string text)
        {
            var colored = await AnnotateAsync(text);

            if (string.IsNullOrEmpty(text)) return colored;

            var all = new List<AnnotatedTextEditor.Entity>(colored);

            int i = 0;
            while (i < text.Length)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    i++;
                    continue;
                }

                int start = i;

                if (char.IsLetterOrDigit(text[i]))
                {
                    while (i < text.Length && char.IsLetterOrDigit(text[i])) i++;
                }
                else
                {
                    // Punctuation / symbol — emit as its own single-char token.
                    i++;
                }

                int end = i;

                bool overlapsColored = colored.Any(e => start < e.End && end > e.Start);
                if (!overlapsColored)
                {
                    all.Add(new AnnotatedTextEditor.Entity(
                        start,
                        end - start,
                        "TOKEN",
                        "var(--tss-colors-neutral-200)",
                        "var(--tss-colors-neutral-900)",
                        "var(--tss-colors-neutral-500)"));
                }
            }

            return all.OrderBy(e => e.Start).ToArray();
        }

        public HTMLElement Render() => _content.Render();
    }
}