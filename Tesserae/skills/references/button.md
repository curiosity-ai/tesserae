---
name: button
description: The standard clickable button with tone variants, icons, link styling, and inline async spinner states. Use for any action trigger — submit, confirm, navigate, run — in a Tesserae (C#/Transpose) app.
---

# Button

`Button` is the primary action control. It supports tone variants, icons, link styling,
hotkeys, and an inline spinner for async actions.

## Create

`UI.Button(string text = "")` or `UI.Button(UIcons icon)` (icon-only).
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Tone: `.Primary()`, `.Success()`, `.Danger()` (default is neutral).
Style: `.Link()`, `.DefaultLink()`, `.DangerLink()`, `.Compact()`, `.NoBorder()`,
`.NoBackground()`, `.Color(background, textColor, borderColor, iconColor)`.

Content:

- `.SetText(string)` / `.SetTitle(string)` (hover title) / `.Tooltip(string)`.
- `.SetIcon(UIcons icon, string color = "", ..., bool afterText = false)`.
- `.Wrap()` / `.NoWrap()` / `.Ellipsis()`.

Behaviour:

- `.OnClick((sender, evt) => ...)` or `.OnClick(Action)`.
- `.OnClickSpinWhile(Func<Task> action, string text = null, ...)` — show a spinner while the async action runs.
- `.Disabled(bool = true)`.
- `.WithHotKey(string keys)` — bind a keyboard shortcut.
- `.Focus()` — move focus to the button.

Spinner control: `.ToSpinner(text)` / `.UndoSpinner()` / `.SpinWhile(Func<Task>)`.

## Example

```csharp
using static Tesserae.UI;

var actions = HStack().Children(
    Button("Confirm").SetIcon(UIcons.Check).Success().OnClick(() => alert("ok")),
    Button("Delete").SetIcon(UIcons.Trash).Danger().OnClick(() => alert("deleted")),
    Button("Save").Primary().OnClickSpinWhile(async () => await SaveAsync(), "saving..."));
```

## Related

- ActionButton — `action-button.md`
- Full docs & API: `/tesserae/components/button`
