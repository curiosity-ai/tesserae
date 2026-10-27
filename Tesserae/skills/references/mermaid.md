---
name: mermaid
description: Draws Mermaid diagram source (flowcharts, sequence, class, state, ER, gantt, pie and more) as themed SVG with Pele, with optional DOMPurify sanitization. Use when showing a described process, protocol or schedule as a diagram in a Tesserae (C#/Transpose) app.
---

# Mermaid

Takes Mermaid source and draws it as SVG with [Pele](https://pele.run), a small
renderer for Mermaid syntax (every diagram type of Mermaid 12.1). It is **not** the
Mermaid library. Colors, fonts and radius come from Tesserae's tokens, so a diagram
follows light/dark mode and custom themes without being redrawn.

## Create

`UI.Mermaid(string source = "", bool sanitize = true)` — draws `source`. Bring the
factory into scope with `using static Tesserae.UI;`.

```csharp
Mermaid(@"flowchart LR
  A[Draft] --> B{Review}
  B -- approved --> C([Published])
  B -- changes --> D[Rework]
  D --> A");
```

## Key configuration

- `.Source` — get/set the Mermaid source; assigning redraws in place (an empty source draws nothing).
- `.Sanitize(bool = true)` — run the SVG through DOMPurify before it is inserted (**on by
  default**, on top of Pele's own escaping of diagram text). Turn it off only for source you wrote.
- `.IsSanitized` — whether it is on.
- `.Links(bool = true)` / `.Images(bool = true)` — keep the diagram's own `click` hyperlinks /
  pictures. Turn both off for source you do not trust to link anywhere or call a remote server.
- `.Zoom(bool = true)` — a diagram that has been shrunk to fit its container can be zoomed and panned.
- `.Error` — why the current source could not be drawn, or `null`.
- `.OnError(Action<string>)` — called with the reason when a source cannot be drawn (a syntax
  error, or the library failing to load). Nothing is thrown; the diagram shows the reason.

## Loading

Pele ships as its own file (`assets/js/tss-pele.min.js`, not linked from `index.html`) and is
fetched the first time a diagram is drawn; until then the component shows the source as text.
Keep the component in a container whose width does not depend on the diagram: it redraws to
fit the width.

A `mermaid` code fence in `MarkdownBlock` is drawn by the same renderer — see `markdown-block.md`.

## Example

```csharp
var diagram = Mermaid("flowchart TD\n  A --> B");
var error   = TextBlock("");

diagram.OnError(e => error.Text = e);

var editor = TextArea("flowchart TD\n  A --> B").OnInput((ta, _) => diagram.Source = ta.Text);

// Source from a user, shown without links or remote pictures:
var untrusted = Mermaid(userSource).Links(false).Images(false);
```

## Related

- Flint charts — charts in the same Markdown — `flint-charts.md`
- MarkdownBlock — renders `mermaid` fences with the same renderer — `markdown-block.md`
- Diagram — draggable nodes you arrange yourself — `diagram.md`
- GanttChart — a timeline built from rows and bars — `gantt-chart.md`
