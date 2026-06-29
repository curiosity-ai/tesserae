---
name: accordion
description: A vertical stack of expand/collapse sections (Expanders), with optional single-open behaviour. Use when building collapsible FAQ-style or grouped content in a Tesserae (C#/h5) app.
---

# Accordion / Expander

`Accordion` manages a list of `Expander` sections. Each `Expander` has a clickable
header that reveals its body. Use an `Expander` on its own for a single collapsible
section; wrap several in an `Accordion` to coordinate open/close behaviour.

## Create

`UI.Accordion(params Expander[] items)` — coordinates several expanders.
`UI.Expander(string title = null, IComponent content = null)` — one section.
Bring the factories into scope with `using static Tesserae.UI;`.

## Key configuration

Accordion:

- `.AllowMultipleOpen(bool = true)` — allow several sections open at once; pass `false` for single-open.
- `.AddItem(Expander)` / `.AddItems(...)` / `.Items(...)` — add expanders.

Expander:

- `.Expanded(bool = true)` / `.Expand()` / `.Collapse()` / `.Toggle()` — open state.
- `.SetTitle(string)` / `.SetHeader(IComponent)` — header text or custom header.
- `.SetContent(IComponent)` — body content.
- `.OptionIcon(UIcons icon, string color = "", string background = "")` — leading icon.
- `.ChevronRight()` — move the chevron to the right edge.
- `.OnToggle(...)` / `.OnExpand(...)` / `.OnCollapse(Action<Expander>)` — callbacks.

## Example

```csharp
using static Tesserae.UI;

var accordion = Accordion(
    Expander("Getting started", TextBlock("Reveal details in place.")).Expanded(),
    Expander("Configuration", Stack().Children(
        TextBlock("Nest any component."),
        Button("Primary action").Primary())),
    Expander("Advanced", TextBlock("Combine with Card."))
        .OptionIcon(UIcons.Settings, Theme.Colors.Blue600, Theme.Colors.Blue100)
        .ChevronRight()
).AllowMultipleOpen(false);
```

## Related

- Full docs & API: `/tesserae/components/accordion`
- Card — `card.md`
