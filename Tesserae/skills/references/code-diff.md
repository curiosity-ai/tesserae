---
name: code-diff
description: A rendered unified diff, line-by-line or side-by-side, from git-diff text. Use to show what changed between two versions of a file in a Tesserae (C#/Transpose) app.
---

# CodeDiff

`CodeDiff` renders unified-diff text — the output of `git diff`, or anything else in that
format — using the [diff2html](https://github.com/rtfpessoa/diff2html) library. The slim UI
bundle and its CSS ship with Tesserae, so there is no preload step and nothing to add to the
page.

## Create

`UI.CodeDiff(string diff = "", CodeDiff.Format format = CodeDiff.Format.LineByLine)`
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `DiffText` — the diff to render. Assigning it re-renders.
- `OutputFormat` — `Format.LineByLine` (additions and deletions one after the other, in a
  single column) or `Format.SideBySide` (original on the left, new file on the right).
- `DrawFileList` — a summary list of the files in the diff, above it.
- `LineMatching` — how diff2html pairs lines between the two sides: `Matching.None`,
  `Matching.Lines` or `Matching.Words`.
- `HighlightCode` — asks diff2html to syntax-highlight the result. That relies on a globally
  available `hljs` ([highlight.js](https://highlightjs.org/)) and is a no-op when highlight.js
  is not loaded.

A diff is as wide as its longest line, so give it the width it should take (`.WS()`) rather
than letting it size a row it sits in.

## Example

```csharp
using static Tesserae.UI;

var diff = "--- a/greeting.cs\n" +
           "+++ b/greeting.cs\n" +
           "@@ -1,3 +1,3 @@\n" +
           " public static string Greet(string name)\n" +
           "-    => \"Hello \" + name;\n" +
           "+    => $\"Hello, {name}!\";\n";

var review = CodeDiff(diff, CodeDiff.Format.SideBySide).WS();

review.DrawFileList = true;
review.LineMatching = CodeDiff.Matching.Words;
```

## Related

- MarkdownBlock — rendered markdown, for prose around a diff — `markdown-block.md`
- Sandbox — an isolated iframe for third-party HTML — `sandbox.md`
- Full docs & API: `/tesserae/components/code-diff`
