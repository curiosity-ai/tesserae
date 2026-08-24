---
name: tippy
description: Tooltip support for any IComponent (wraps Tippy.js), accepting text or component content. Use when attaching hover tooltips/popovers in a Tesserae (C#/Transpose) app.
---

# Tippy

Tooltips for components, via the `.Tooltip()` extension on `IComponent`. Content can be plain text or any Tesserae component; tooltips can be made interactive (clickable content).

## Methods

`.Tooltip(...)` (extension on `IComponent`), two overloads:

- `.Tooltip(string tooltipHtml, TooltipAnimation animation = None, TooltipPlacement placement = Top, int delayShow = 250, int delayHide = 0, bool followCursor = false, int maxWidth = 350, bool arrow = false, string theme = null, IComponent parent = null)`
- `.Tooltip(IComponent tooltip, bool interactive = false, …the same options…)` — for component-based tooltips.

Set `interactive: true` (component overload only) to let users select/click inside the tooltip.
`.RemoveTooltip()` takes one away again.

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
