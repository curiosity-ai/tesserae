---
name: tippy
description: Tooltip support for any IComponent (wraps Tippy.js), accepting text or component content. Use when attaching hover tooltips/popovers in a Tesserae (C#/Transpose) app.
---

# Tippy

Tooltips for components, via the `.Tooltip()` extension on `IComponent`. Content can be plain text or any Tesserae component; tooltips can be made interactive (clickable content).

## Methods

`.Tooltip(...)` (extension on `IComponent`), two overloads:

- `.Tooltip(string text, TooltipAnimation animation = Fade, TooltipPlacement placement = Top, int delay = 0, int duration = 200, bool interactive = false)`
- `.Tooltip(IComponent tooltipContent, …same options…)` — for component-based tooltips.

Set `interactive: true` to let users select/click inside the tooltip.

`Tippy.ShowFor(IComponent component, IComponent tooltipContent, out Action hide, …)` — show a tooltip programmatically; `hide` is an out-action to dismiss it.

## Example

```csharp
using static Tesserae.UI;

Stack().Children(
    Button("Simple").Tooltip("Just text"),

    Button("Rich").Tooltip(
        Stack().Children(
            TextBlock("Header").SemiBold(),
            TextBlock("A stack of components."))),

    Button("Interactive").Tooltip(
        Button("Click me").OnClick(() => Toast().Success("Clicked!")),
        interactive: true));
```

## Related

- Toast — `toast.md`
- Full docs & API: `/tesserae/utilities/tippy`
