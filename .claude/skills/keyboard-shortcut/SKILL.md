---
name: keyboard-shortcut
description: Renders key names as styled <kbd> chips, auto-adapting modifier labels to the current OS. Use when displaying keyboard shortcuts (e.g. Ctrl+K) in a Tesserae (C#/h5) app.
---

# KeyboardShortcut

Renders one or more keys as styled chips joined by `+`. Modifier labels adapt to the OS (⌘/⌃/⌥/⇧ on macOS, Ctrl/Win/Alt/Shift elsewhere) and special keys are normalized (Enter→↵, Escape→Esc, ArrowUp→↑, Backspace→⌫, Tab→⇥, etc.). This is display-only — it does not bind handlers.

## Create

`new KeyboardShortcut(params string[] keys)` — also via `UI.KeyboardShortcut("Ctrl", "K")`. Bring factories into scope with `using static Tesserae.UI;`.

## Key points

- Pass raw key names; normalization to OS-correct glyphs is automatic.
- Recognized special keys: `Ctrl`/`Control`, `Alt`, `Shift`, `Meta`/`Cmd`, `Enter`, `Escape`/`Esc`, `ArrowUp/Down/Left/Right`, `Backspace`, `Delete`, `Tab`. Anything else renders verbatim.
- It is a regular `IComponent` — place it inline in a `Stack` alongside `TextBlock`.

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

- CommandPalette (actual Ctrl/Cmd-K launcher) — `../command-palette/SKILL.md`
- Full docs & API: `/tesserae/utilities/keyboard-shortcut`
