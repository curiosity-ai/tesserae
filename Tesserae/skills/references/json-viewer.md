---
name: json-viewer
description: A JSON document as a collapsible, syntax-coloured tree that builds a container's rows when opened and shows them a batch at a time. Use when showing JSON to a person - an API response, a stored document, a JSON file - in a Tesserae (C#/Transpose) app.
---

# JsonViewer

A tree of a JSON document: keys, strings, numbers, booleans and `null` each in their own colour, a collapsed
object or array summarised by its size (`{… 3 keys`, `[… 12 items`), and an indent guide between an open
container's braces. Built for large documents: a container's rows are only created the first time it is opened,
and each shows its first `MaxChildren` entries with a row that shows the next batch. Every value is inserted as
text, so JSON from anywhere is safe to show; text that does not parse is shown as the parser's error.

## Create

`UI.JsonViewer(string json = null)` — the JSON text, or null for an empty viewer.
Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Json(string json)` — show other JSON text (re-renders).
- `.Value(object value)` — show a value that is already parsed (a plain JavaScript object/array/scalar, as `JSON.parse` returns).
- `.ExpandToDepth(int depth)` — levels open when drawn: `0` root collapsed, `1` (default) root open, `int.MaxValue` everything.
- `.ExpandAll()` / `.CollapseAll()` — open every container / close everything below the root. Each container still shows only its first `MaxChildren` entries.
- `.MaxChildren(int count)` — entries of one object or array shown before the "show more" row (default 100; `0` shows all).
- `.ShowMore(bool showMore = true)` — `false` turns that row into a plain "… N more" count, for a preview whose whole is available another way (a download).
- `.MaxStringLength(int length)` — longer strings are cut and shown whole on click (default 300; `0` never cuts).
- `.Copyable(bool copyable = true)` — the per-row button that copies the row's value (a string as itself, anything else as indented JSON).
- `.RootLabel(string label)` — a name for the root row (a file or field name).

## Behaviour

- **Keyboard** (WAI-ARIA tree, one tab stop): Up/Down move between visible rows, Right opens or steps into a container, Left closes it or steps out to its parent, Enter/Space toggle, Home/End go to the first/last row.
- **Paths**: a key's tooltip is its path from the root, `$.lines[1].sku`; keys that are not identifiers are written `$["a key"]`.
- **Numbers** are JavaScript numbers, so an integer past 2^53 shows rounded — the same as `JSON.parse` gives you.

## Styling

`tss.jsonviewer.css`. The colours are variables on `.tss-jsonviewer`, defaulting to the palette with lighter shades in dark mode:
`--tss-jsonviewer-key`, `-string`, `-number`, `-boolean`, `-null`, `-punctuation`, `-guide`, and `--tss-jsonviewer-indent` (16px).
The font is `--tss-monospace-font-family`. The viewer does not scroll itself: give it a `MaxHeight` and `overflow-y: auto`, or put it in a scrolling container.

## Example

```csharp
using static Tesserae.UI;

var viewer = JsonViewer(responseText)
   .RootLabel("response.json")
   .ExpandToDepth(2)
   .MaxStringLength(120);

var toolbar = HStack().Children(
    Button("Expand all").OnClick(() => viewer.ExpandAll()),
    Button("Collapse all").OnClick(() => viewer.CollapseAll()));

// A preview of part of a document, with the whole one a download away:
var preview = JsonViewer(json).MaxChildren(50).ShowMore(false);
```

## Related

- Tree — a general hierarchical list of your own items — `tree.md`
- PropertyGrid — editing an object's properties — `property-grid.md`
- DetailsGrid — a handful of label/value rows — `details-grid.md`
