---
name: button
description: The standard clickable button with tone variants, icons, an href that makes it a real link, and inline async spinner states. Use for any action trigger — submit, confirm, navigate, run — in a Tesserae (C#/Transpose) app.
---

# Button

`Button` is the primary action control. It supports tone variants, icons, hotkeys, an inline
spinner for async actions, and an href that turns it into a real link.

## Create

`UI.Button(string text = "", string href = null)` or `UI.Button(UIcons icon)` (icon-only).
Bring factories into scope with `using static Tesserae.UI;`.

## A button that goes somewhere

Pass an `href` and the button renders as an **anchor** rather than a `button` element, so it is
middle-clickable, opens in a new tab on ctrl/cmd-click and shows where it goes in the status bar —
while looking exactly like any other button. There is no separate `Link` component: a link that
looks like a link is `Button(text, href).Class("tss-btn-link")`, and a small fact that happens to
link somewhere is an `InlineLabel` with `.SetHref(...)`.

```csharp
Button("Open documentation", href: DocsUrl).Primary().SetIcon(UIcons.ArrowUpRightFromSquare)
```

Only a button with an href underlines its label on hover (and only in the link-toned variant) — one
that merely runs a handler is a button, and underlining it would promise an address it doesn't have.

## Key configuration

Tone: `.Primary()`, `.Success()`, `.Danger()` (default is neutral).
Style: `.Compact()`, `.NoBorder()`, `.NoBackground()`, `.Class("tss-btn-link")` (reads as text
rather than as a box), `.Color(background, textColor, borderColor, iconColor)`.

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
