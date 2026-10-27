---
name: flint-charts
description: Draws flint code fences in Markdown as charts with Microsoft's Flint chart language, themed through IFlintTheme and FlintThemes. Use when a MarkdownBlock (or Markdown.RenderMarkdownSanitized) should render charts written as JSON specs - by hand or by an LLM - in a Tesserae (C#/Transpose) app.
---

# Flint charts in Markdown

[Flint](https://github.com/microsoft/flint-chart) is a chart language: a spec says what
the data means (a semantic type per field) and which chart to draw, and Flint works out
the scales, axes, labels, layout and colours. Tesserae draws a `flint` fence (or
`flint-chart`, the name Flint's own documentation uses) as a chart once it is asked to.
There is no chart component: it is part of Markdown rendering.

## Enable

```csharp
using static Tesserae.UI;

MarkdownBlock(text).Flint();                                    // FlintThemes.Tesserae
MarkdownBlock(text).Flint(FlintThemes.Economist);               // a preset
MarkdownBlock(text).Flint(FlintThemes.Tesserae, overrideFenceTheme: true);
MarkdownBlock(text).Flint(false);                               // off again (the default)

Markdown.RenderMarkdownSanitized(text, MarkdownSanitization.Default, FlintThemes.Tesserae);
```

- `MarkdownBlock.Flint(bool enabled = true)` — on in `FlintThemes.Tesserae`. Re-renders.
- `MarkdownBlock.Flint(IFlintTheme theme, bool overrideFenceTheme = false)` — on in a
  theme (`null` is `FlintThemes.Tesserae`). A `theme_spec` in the fence wins over it
  unless `overrideFenceTheme` is `true`, which keeps every chart alike whatever its
  author (or a model) asked for.
- `Markdown.RenderMarkdownSanitized(text, sanitization, IFlintTheme flintTheme, bool overrideFenceTheme = false)` —
  the same for the static helper. The two-argument overload does not draw charts.

## The fence

The body is a Flint `ChartAssemblyInput` in JSON: `data`, `semantic_types`,
`chart_spec`, and optionally `theme_spec` and `interaction_spec`.

````markdown
```flint
{
  "data": { "values": [ { "Region": "North", "Revenue": 1204 }, { "Region": "East", "Revenue": 1530 } ] },
  "semantic_types": { "Region": "Category", "Revenue": "Amount" },
  "chart_spec": {
    "chartType": "Bar Chart",
    "title": "East leads the quarter",
    "encodings": { "x": { "field": "Region" }, "y": { "field": "Revenue" } }
  },
  "interaction_spec": { "interactions": [ { "type": "inspect-index" } ] }
}
```
````

Flint's authoring guide lists the chart types, semantic types and interaction presets:
<https://github.com/microsoft/flint-chart/blob/main/agent-skills/flint-chart-author/SKILL.md>
(hand it to a model that writes these fences).

## Behaviour

- **Loaded on demand.** Flint and the Vega-Lite renderer it draws with (Vega, Vega-Lite,
  vega-tooltip) ship as one unlinked file, `assets/js/tss-flint.min.js` (~1.8 MB,
  ~630 KB gzipped), fetched the first time a chart appears. A page with no chart never
  fetches it.
- **Vega-Lite only.** It is the backend Flint's themes and interactions apply to; the
  ECharts, Chart.js and Plotly backends are not bundled.
- **A fence that cannot be drawn stays a code block** (marked with a warning border,
  `pre.tss-flint-source`) with the reason as its tooltip: invalid JSON (a fence still being
  streamed), a field that is not in the data, an unknown chart type. Specs are checked
  with Flint's `validateChartInput` before they are drawn.
- **Laid out for its width**, again when the width changes (in place, so a selection or
  zoom survives), and drawn again when a page theme change changes its theme.
- **Streaming-safe.** Each fence is one `div.tss-flint` carrying a
  `DeltaComponent.KeyAttribute` made from the spec and the options, so a reply streamed
  through `DeltaComponent.ReplaceContent(MarkdownBlock(...).Flint())` draws each chart
  once and keeps it while later chunks arrive. A drawn chart whose element leaves the
  page (re-rendered `Text`, a swap) is kept for ten seconds and taken over by the next
  element of the same spec, so re-rendering does not redraw it.
- **Sanitization.** Under `MarkdownSanitization.NoLinksOrEmbeddedContent` a chart may not
  load `data.url` (it stays a code block) and `chart_spec.source` loses its link.

## Themes

`IFlintTheme` has one member, `string ThemeSpecJson(HTMLElement host)`: Flint's
`theme_spec` for a chart drawn inside `host`, as JSON (a quoted preset id or a ThemeSpec
object; `null` for Flint's own defaults). Colours must be hex: Flint parses nothing else.
It is asked again on every page theme change, and the chart redrawn only when the
answer differs.

`FlintThemes`:

- `FlintThemes.Tesserae` — the default. Colours, fonts and series palette from the
  Tesserae tokens around the chart (`--tss-default-foreground-color`,
  `--tss-secondary-foreground-color`, borders, `--tss-chart-series-1…8`,
  `--tss-sansserif-font-family`, …), resolved to hex at the chart, on the surface the
  chart actually sits on. Follows light/dark mode and every custom theme. Its surface is
  `"source": "host"`, so the frame's own background (a pattern, a frosted pane) shows
  through the chart.
- `FlintThemes.TesseraeOver("economist")` — a preset's structure (axes, gridlines,
  marks, spacing) in the page's colours and type.
- Presets, each a fixed house style with its own canvas: `Nyt`, `Economist`, `Swiss`,
  `Nature`, `McKinsey`, `Datawrapper`, `PowerBI`, `PowerBILight`, `Pop`, `Cartoon`,
  `Apple`, `DeStijl`, `Bbc`, `Material`; or `FlintThemes.Preset(id)` with an id from
  `FlintThemes.PresetIds`.
- `FlintThemes.None` — Flint's own defaults.
- `FlintThemes.FromJson("{ \"extends\": \"nyt\", \"ink\": { \"accent\": \"#0b57d0\" } }")` —
  a ThemeSpec of your own.

Each preset id and `TesseraeOver` id returns the same instance every time: Markdown
tells two charts' themes apart by instance, so a custom `IFlintTheme` should be kept
in a field rather than created per render.

## Styling

`tss.flint.css` draws the frame (`.tss-flint-chart`, the same plate as a Mermaid diagram),
hides the source once drawn, and restyles vega-tooltip's `#vg-tooltip-element` with the
tokens. A custom theme package restyles `.tss-flint-chart` next to its `.tss-mermaid`
rules; to colour a chart differently inside its frame, restate tokens on the frame
(the Weniger package does, for its terminal-glass plate).

## Related

- MarkdownBlock — `markdown-block.md`
- DeltaComponent — `delta-component.md` (the keyed elements streaming relies on)
- Mermaid — diagrams in the same Markdown — `mermaid.md`
- Charts — the toolkit's own chart components — `charts.md`
- Custom themes — `custom-themes.md`
