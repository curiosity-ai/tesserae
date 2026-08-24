---
name: code-diff
description: Renders a unified diff (the output of git diff) as a coloured line-by-line or side-by-side view, with optional file list and syntax highlighting. Use when showing what changed between two versions of a file in a Tesserae (C#/Transpose) app.
---

# CodeDiff

`CodeDiff` renders unified-diff text — what `git diff` prints — as the familiar
red/green view, either line-by-line or side-by-side. It wraps
[diff2html](https://github.com/rtfpessoa/diff2html); the slim UI bundle and its CSS ship
with Tesserae, so there is nothing to preload.

Setting any of its properties re-draws (debounced), which is what makes it usable as a
live preview of a diff being edited or streamed in.

## Create

`UI.CodeDiff(string diff = "", CodeDiff.Format format = Format.LineByLine)`.
Bring factories into scope with `using static Tesserae.UI;`.

`Format` and `Matching` are nested types, so under `using static Tesserae.UI;` they need
qualifying — `Tesserae.CodeDiff.Format.SideBySide` — because the `CodeDiff(...)` factory
method hides the type of the same name (see the note in `SKILL.md`).

## Key configuration

All of these are properties, and each one re-draws:

- `.DiffText` — the unified diff to render.
- `.OutputFormat` — `Format.LineByLine` (default) or `Format.SideBySide`.
- `.LineMatching` — how the two sides are paired up: `Matching.None`, `Matching.Lines`
  (the default) or `Matching.Words`.
- `.DrawFileList` — a summary list of the files in the diff, above it. Off by default.
- `.HighlightCode` — asks diff2html to syntax-highlight after drawing. It calls
  `hljs` (highlight.js), which Tesserae does **not** bundle: load it yourself, or this is
  a no-op.

Long lines scroll horizontally by default. Add the `tss-codediff-wrap` class
(`.Class("tss-codediff-wrap")`) to wrap them inside the pane instead.

## Example

```csharp
using static Tesserae.UI;

const string patch = @"diff --git a/sample.js b/sample.js
--- a/sample.js
+++ b/sample.js
@@ -1,3 +1,4 @@
-function greet(name) {
-    console.log('Hello, ' + name);
+function greet(name, greeting) {
+    greeting = greeting || 'Hello';
+    console.log(greeting + ', ' + name + '!');
 }
";

var diff = CodeDiff(patch).WS();

// Editing the text re-draws the view.
var editor = TextArea(patch).WS().H(220).OnInput((ta, _) => diff.DiffText = ta.Text);

var sideBySide = Toggle("Side by side").OnChange((s, _) =>
    diff.OutputFormat = s.IsChecked
        ? Tesserae.CodeDiff.Format.SideBySide
        : Tesserae.CodeDiff.Format.LineByLine);

var ui = VStack().WS().Children(editor, sideBySide, diff);
```

## Related

- MarkdownBlock — rendered prose rather than a diff — `markdown-block.md`
- TextArea — the editor a diff is usually produced from — `text-area.md`
- Sandbox — for HTML you did not produce — `sandbox.md`
- Full docs & API: `/tesserae/components/code-diff`
