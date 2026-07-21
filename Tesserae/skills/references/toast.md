---
name: toast
description: Short-lived overlay notifications (success/info/warning/error) with configurable position and optional banner mode. Use when showing transient feedback messages in a Tesserae (C#/Transpose) app.
---

# Toast

A Layer-based notification that auto-dismisses after a timeout (default 5s, paused on hover). Fire one via a fluent chain: pick options, then call a type method with a title+message or message-only.

## Create

`UI.Toast()` — returns a `Toast`. Bring factories into scope with `using static Tesserae.UI;`. Set the app-wide default position via `Toast.DefaultPosition`.

## Key configuration (call before the fire method)

Position: `.TopRight()` (default), `.TopCenter()`, `.TopLeft()`, `.BottomRight()`, `.BottomCenter()`, `.BottomLeft()`, `.TopFull()`, `.BottomFull()`.

Other: `.Banner(bool showHideButton = true)` (full-width banner that shifts page content), `.Duration(TimeSpan)`, `.Width(UnitSize)` / `.Height(UnitSize)`, `.NoDismiss()` (don't dismiss on click), `.Overwrite()` (replace an existing toast with the same content), `.Class(name)` / `.RemoveClass(name)`.

## Fire methods (each shows the toast and returns it)

`.Information(...)`, `.Success(...)`, `.Warning(...)`, `.Error(...)` — each accepts `(string title, string message)`, `(string message)`, `(IComponent title, IComponent message)`, or `(IComponent message)`.

`.Hide()` / `.Remove()` dismiss programmatically.

## Example

```csharp
using static Tesserae.UI;

Button("Show").OnClick((s, e) =>
    Toast().Information("Info", "This is an information toast."));

Toast().TopLeft().Success("Done", "Saved at top left.");
Toast().TopFull().Banner().Error("Error", "Acting as a banner.");
```

## Related

- NotificationCenter (persistent inbox) — `notification-center.md`
- Full docs & API: `/tesserae/utilities/toast`
