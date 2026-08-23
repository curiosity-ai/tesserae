---
name: keyboard-shortcut
description: Renders key names as styled <kbd> chips, auto-adapting modifier labels to the current OS. Use when displaying keyboard shortcuts (e.g. Ctrl+K) in a Tesserae (C#/Transpose) app.
---

# KeyboardShortcut

Renders one or more keys as styled chips joined by `+`. Modifier labels adapt to the OS (⌘/⌃/⌥/⇧ on macOS, Ctrl/Win/Alt/Shift elsewhere) and special keys are normalized (Enter→↵, Escape→Esc, ArrowUp→↑, Backspace→⌫, Tab→⇥, etc.). The component itself is display-only; `KeyboardShortcut.Matches` (below) is how a `keydown` is tested against the same key names.

## Create

`new KeyboardShortcut(params string[] keys)` — also via `UI.KeyboardShortcut("Ctrl", "K")`. Bring factories into scope with `using static Tesserae.UI;`.

## Key points

- Pass raw key names; normalization to OS-correct glyphs is automatic.
- A key's plate is one fixed grey per theme, so a chip looks the same wherever it is
  drawn — on a card, inside a search field, or on a filled `.Primary()` button.
- Recognized special keys: `Ctrl`, `Control`, `Alt`, `Shift`, `Meta`/`Cmd`, `Enter`, `Escape`/`Esc`, `ArrowUp/Down/Left/Right`, `Backspace`, `Delete`, `Tab`. Anything else renders verbatim.
- It is a regular `IComponent` — place it inline in a `Stack` alongside `TextBlock`.

## The command modifier

`Ctrl` (and its aliases `Mod` / `CmdOrCtrl`) means *the platform's command modifier*: it
shows as `Ctrl` on Windows and Linux and as **⌘** on macOS, and `Matches` accepts
either Cmd or Ctrl there — a keyboard carried over from Windows still reaches the
shortcut. Declare it once as `("Ctrl", "Shift", "O")` and both platforms are right.

Use the explicit `Control` for the rare shortcut that means the Control key on a Mac
too; it shows as ⌃ and only matches `ctrlKey`.

## Matching a key press

`KeyboardShortcut.Matches(KeyboardEvent e, params string[] keys)` — `true` when `e` is
that shortcut. It takes the same key names the chips display, so what is shown and what
is bound are declared once and cannot drift apart. Modifiers must match exactly (a
shortcut without `Shift` does not fire on Shift), and the main key is compared
case-insensitively, so a shifted letter still matches.

Write it **qualified** as `Tesserae.KeyboardShortcut.Matches(...)`: with
`using static Tesserae.UI;` in scope, the bare name resolves to the `KeyboardShortcut(...)`
factory method and the compiler reports `CS0119: ... is a method, which is not valid in
the given context`.

```csharp
window.addEventListener("keydown", ev =>
{
    if (!Tesserae.KeyboardShortcut.Matches(ev.As<KeyboardEvent>(), "Ctrl", "Shift", "O")) return;
    StopEvent(ev);
    NewDocument();
});
```

Components that show a shortcut and answer it — `SearchBox.SetKeyboardShortcut(...)`,
`SidebarButton.SetKeyboardShortcut(...)`, `SidebarSearchBox`, `OmniBox` — do exactly
this internally, so prefer those when one of them is what you are building.

## Example

```csharp
using static Tesserae.UI;

var row = HStack().AlignItems(ItemAlign.Center).Gap(4.px()).Children(
    TextBlock("Press").Small(),
    KeyboardShortcut("Ctrl", "K"),
    TextBlock("to open, or").Small(),
    KeyboardShortcut("Escape"),
    TextBlock("to dismiss.").Small());
```

## Related

- ShortcutGuide (the modal listing every shortcut) — `shortcut-guide.md`
- CommandPalette (actual Ctrl/Cmd-K launcher) — `command-palette.md`
- Sidebar (`.AsSearchBox(...)`, `SidebarButton.SetKeyboardShortcut(...)`) — `sidebar.md`
- Full docs & API: `/tesserae/utilities/keyboard-shortcut`
