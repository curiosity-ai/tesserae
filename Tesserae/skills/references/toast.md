---
name: toast
description: Short-lived overlay notifications (success/info/warning/error) that float a Banner over the page, with configurable position and an edge-to-edge banner mode. Use when showing transient feedback messages in a Tesserae (C#/Transpose) app.
---

# Toast

A Layer-based notification that auto-dismisses after a timeout (default 5s, paused on hover). Fire one via a fluent chain: pick options, then call a type method with a title+message or message-only.

What a toast *shows* is a `Banner` — the same notice strip that renders inline anywhere. The type
methods build one for you; `.Show(banner)` takes one you built yourself, with its own icon tile,
badge and action.

## Create

`UI.Toast()` — returns a `Toast`. Bring factories into scope with `using static Tesserae.UI;`. Set the app-wide default position via `Toast.DefaultPosition`.

## Key configuration (call before the fire method)

Position: `.TopRight()` (default), `.TopCenter()`, `.TopLeft()`, `.BottomRight()`, `.BottomCenter()`, `.BottomLeft()`, `.TopFull()`, `.BottomFull()`.

Other: `.Banner(bool showHideButton = true)` (full-width banner that shifts page content), `.Duration(TimeSpan)`, `.Width(UnitSize)` / `.Height(UnitSize)`, `.NoDismiss()` (don't dismiss on click), `.Overwrite()` (replace an existing toast with the same content), `.Class(name)` / `.RemoveClass(name)`.

## Fire methods (each shows the toast and returns it)

`.Information(...)`, `.Success(...)`, `.Warning(...)`, `.Error(...)` — each accepts `(string title, string message)`, `(string message)`, `(IComponent title, IComponent message)`, or `(IComponent message)`. They build a `Banner` in the matching tone (Primary, Success, Warning, Danger).

`.Show(Banner)` — float a banner of your own instead.

`.Content` — the `Banner` the toast is showing, so a caller that used a string helper can still reach the strip it built.

`.Hide()` / `.Remove()` dismiss programmatically.

## Banner mode and stacking

`.Banner()` pins the strip edge to edge and shrinks the page to make room for it, so it is page
chrome rather than an overlay: it stays above every layer, and a `Modal`, `Panel` or node preview
opened afterwards is stacked *under* it instead of covering it. A popover opened from inside the
banner still lands in front of the banner itself. Nothing to configure — don't hand a banner a
z-index of your own.

## Dismissing

The banner's `[x]` is hooked to the toast's own hiding, chained *after* whatever `OnDismiss` handler
you set on the banner. Whether there is one at all follows the toast's settings: an edge-to-edge
banner follows its `showHideButton`, an ordinary toast shows one unless `.NoDismiss()` said it cannot
be dismissed at all.

## Example

```csharp
using static Tesserae.UI;

Button("Show").OnClick((s, e) =>
    Toast().Information("Info", "This is an information toast."));

Toast().TopLeft().Success("Done", "Saved at top left.");
Toast().TopFull().Banner().Error("Error", "Acting as a banner.");

// A banner of your own, with an icon, a badge and an action
Toast().Show(Banner("Export finished", "18 documents, 42 MB.")
    .Success()
    .SetIcon(UIcons.Download)
    .Action("Download", () => StartDownload()));
```

## Related

- Banner — the strip a toast shows, and the same one inline — `banner.md`
- NotificationCenter (persistent inbox) — `notification-center.md`
- Full docs & API: `/tesserae/utilities/toast`
