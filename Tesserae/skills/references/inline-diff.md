---
name: inline-diff
description: A compact diff for the middle of a conversation - what one step of an assistant changed in a file - drawn in plain C# from a unified diff or from before/after text, with the changed words marked and long diffs folded. Use when showing an edit between a tool call and the reply in a Tesserae (C#/Transpose) chat or assistant UI.
---

# InlineDiff

`InlineDiff` draws a small diff inline: a header with the file name and the lines added and
removed, then the changed lines in a gutter layout (old and new line numbers, `+`/`−`), tinted
from the theme's own success and danger colours. A line edited in place marks only the words that
changed. Nothing is loaded - it is plain C#, unlike `CodeDiff` (diff2html), which is the choice
for a full-size review surface with side-by-side and syntax highlighting.

## Create

- `UI.InlineDiff(string diff = "")` - a unified diff, one file or several (each gets a header). The
  `diff --git` / `---` / `+++` lines are optional for one file.
- `UI.InlineDiff(string before, string after, string fileName = null)` - compares two texts itself
  (empty `before` = a new file, empty `after` = a removed one).
- `InlineDiff.Compare(before, after, fileName)` - the unified diff text alone, empty when equal.

`using static Tesserae.UI;`. The `InlineDiff(...)` factory hides the type of the same name, so
the constant is `Tesserae.InlineDiff.DEFAULT_MAX_LINES`.

## Key configuration

- `.DiffText` - get or set the diff; setting redraws.
- `.SetMaxLines(n)` / `.ShowAllLines()` - past `n` lines (default 14) the rest sits behind a
  "Show N more lines" row; zero or less shows everything.
- `.Collapsible()` / `.Collapsed()` - the header folds the lines away on click or Enter/Space.
- `.LineNumbers(false)` - drop the gutter numbers. `.NoWrap()` - scroll long lines sideways.
- `.FileName(name)` - the header name for a diff that carries none.
- `.Added`, `.Removed`, `.ChangedLines` - the counts, e.g. to decide whether a diff is "small".

## Example

```csharp
using static Tesserae.UI;

ChatMessage(VStack().WS().Children(
    ToolCall(UIcons.FileEdit, "Edit src/Greeter.cs"),
    InlineDiff(before, after, "src/Greeter.cs").WS(),
    TextBlock("Greet now takes an optional greeting.")),
    Avatar(null, "AI")).MaxWidth();
```

## Related

- CodeDiff - full-size diff with side-by-side - `code-diff.md`
- ToolCall - the row an edit usually sits under - `tool-call.md`
- Chat - the transcript it goes in - `chat.md`
