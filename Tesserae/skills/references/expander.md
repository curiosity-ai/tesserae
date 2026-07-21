---
name: expander
description: A single expand/collapse section with a clickable header that reveals its body. Use for "show more" affordances or as an item inside an Accordion in a Tesserae (C#/Transpose) app.
---

# Expander

A collapsible section: a clickable header (with chevron) toggles the body
content. Use standalone, or group several inside an `Accordion`.

## Create

`UI.Expander(string title = null, IComponent content = null)` — the expander.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.IsExpanded` — get/set expanded state.
- `.Expand()` / `.Collapse()` / `.Toggle()` / `.Expanded(bool = true)` — change state fluently.
- `.SetTitle(string)` / `.Title` — header text.
- `.SetHeader(IComponent)` — replace the header with custom content.
- `.SetContent(IComponent)` — set the body.
- `.OptionIcon(UIcons icon, string color = "", string background = "")` — leading icon.
- `.ChevronRight()` — move the chevron to the right side.
- `.OnToggle(Action<Expander>)`, `.OnExpand(...)`, `.OnCollapse(...)` — callbacks.

## Example

```csharp
using static Tesserae.UI;

var expander = Expander(
    "Advanced settings",
    Stack().Children(
        TextBlock("Hidden until expanded."),
        CheckBox("Enable telemetry")
    ).Padding(8.px())
).OnExpand(e => Console.WriteLine("opened"));
```

## Related

- Accordion — `accordion.md`
- Full docs & API: `/tesserae/components/expander`
