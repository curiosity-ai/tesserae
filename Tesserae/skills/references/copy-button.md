---
name: copy-button
description: A button that copies text to the clipboard and confirms in place (copy icon morphs to a check, label to "Copied"). Use for copy-to-clipboard actions in a Tesserae (C#/Transpose) app.
---

# CopyButton

Copies a text and says so where the user is looking: the copy icon turns into a
check, a label (if any) slides to "Copied", and both turn back after a moment.
The two labels share one grid cell, so the button keeps the width of the longer.
The confirmation is announced to screen readers, so no toast is shown. Without a
label it is an icon-only button named "Copy".

## Create

- `UI.CopyButton(string textToCopy, string label = null)`
- `UI.CopyButton(Func<string> getTextToCopy, string label = null)` — the text is read at the moment of the click.

## Key configuration

- `.OnCopied(Action<string>)` — runs after a copy, with the text copied.
- `.CopiedText(string)` — the confirmation label (default "Copied").
- `.ResetAfter(int ms)` — how long the confirmation shows (default 1600).
- `.Primary()`, `.NoBackground()` (for use inside a code block or field), `.Copy()` (copy from code).

## Example

```csharp
using static Tesserae.UI;

var link = TextBox(shareUrl).ReadOnly().Grow();
HStack().AlignItemsCenter().Children(link, CopyButton(() => link.Text).Tooltip("Copy link"));

CopyButton("dotnet add package Tesserae", "Copy command");
```

## Related

- `Clipboard.Copy(text, showMessage)` — the copy itself, without a button
- Button — `button.md`
- Text box — `text-box.md`
- Full docs & API: `/tesserae/commands/copy-button`
