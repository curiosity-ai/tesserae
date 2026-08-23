---
name: shortcut-guide
description: A modal listing an application's keyboard shortcuts in titled sections, each row a description and its keys as chips, optionally answering the presses it lists. Use for the "keyboard shortcuts" sheet of a Tesserae (C#/Transpose) app.
---

# ShortcutGuide

The "keyboard shortcuts" sheet an app opens from its help menu or with `Ctrl+/`. A
`Modal` whose content is a list of sections; each row is a description on the left and
the keys on the right, drawn by `KeyboardShortcut`. Because it takes the same key names
`KeyboardShortcut.Matches` tests, a shortcut is declared once and what is listed cannot
drift from what is bound.

## Create

`ShortcutGuide(string title = "Keyboard shortcuts")` — also via `UI.ShortcutGuide()`.
Bring factories into scope with `using static Tesserae.UI;`.

## Building the list

- `.Section(string title)` — starts a section, e.g. `"General"`. Every shortcut added
  after it is listed under it, until the next `.Section(...)`. Pass null or empty for an
  untitled one.
- `.Shortcut(string description, params string[] keys)` — a row. The keys are the names
  `KeyboardShortcut` takes (`"Ctrl"`, `"Shift"`, `"Enter"`, `"Escape"`, `"ArrowUp"`, …),
  so the modifiers show as ⌘/⇧ on macOS and Ctrl/Shift elsewhere. Calling it before any
  `.Section(...)` opens an untitled section.
- Describe what the shortcut *does* ("Quick chat or search"), not what the command is
  called in the code, and group by where it applies.

## Answering the shortcuts

- `.OnPressed(Action)` — the action the shortcut added *last* runs. A row without one is
  listed but not answered, which is right when the key belongs to some other component.
- `.Handle(KeyboardEvent e)` — runs the first listed shortcut that `e` matches and
  returns whether one did, so the caller stops the event only when the press was taken.
  Call it from whatever keydown the app already has.

```csharp
window.addEventListener("keydown", ev =>
{
    if (guide.Handle(ev.As<KeyboardEvent>())) StopEvent(ev);
});
```

## Showing it

- `.Show()` / `.Hide(Action onHidden = null)` / `.Toggle()` — `Toggle` is what the
  shortcut that opens the guide usually calls. `.IsVisible` reads the current state.
- `.ShowEmbedded()` — returns a component to place in the page instead of floating it,
  e.g. inside a settings or help page.
- `.SetTitle(string)`, `.Width(UnitSize)` / `.W(...)` (560px by default),
  `.LightDismiss()` (on by default) / `.NoLightDismiss()`, `.OnShow(...)` / `.OnHide(...)`.
- It is a regular `IComponent`, and the sizing helpers apply to the modal itself, so
  `.MaxHeight(80.vh())` and friends work as they do on `Modal`.

## Example

```csharp
using static Tesserae.UI;

var guide = ShortcutGuide()
   .Var(out var self)                 // so a row can open the guide it is listed in
   .Section("General")
       .Shortcut("Quick chat or search", "Ctrl", "K").OnPressed(() => palette.Show())
       .Shortcut("Toggle sidebar",       "Ctrl", ".").OnPressed(() => sidebar.Toggle())
       .Shortcut("Keyboard shortcuts",   "Ctrl", "/").OnPressed(() => self.Toggle())
   .Section("In chats")
       .Shortcut("Send message",        "Enter")
       .Shortcut("New line in message", "Shift", "Enter")
       .Shortcut("Stop the response",   "Escape");
```

`.Var(out var self)` hands the guide to its own rows, which is how the `Ctrl+/` entry
both advertises and performs the toggle. Rows for keys another component already owns
(`Enter` in a chat box) are listed without an action.

## Related

- KeyboardShortcut (the key chips, and `Matches`) — `keyboard-shortcut.md`
- Modal (the surface underneath) — `modal.md`
- CommandPalette (the Ctrl/Cmd-K launcher a guide usually lists) — `command-palette.md`
- Sidebar (`SidebarButton.SetKeyboardShortcut(...)`) — `sidebar.md`
- Full docs & API: `/tesserae/surfaces/shortcut-guide`
