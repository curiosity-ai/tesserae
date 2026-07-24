---
name: accessibility
description: ARIA/accessibility fluent helpers that add roles, labels, and states to any Tesserae component. Use when adding ARIA attributes, labels, or screen-reader support in a Tesserae (C#/Transpose) app.
---

# Accessibility

Tesserae components ship with keyboard support and sensible ARIA defaults. The
`IAccessibilityExtensions` fluent methods let you add or override ARIA
attributes on any `IComponent`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key APIs / patterns

All methods are chainable extensions on `IComponent` and return the component.

Roles and labels:

- `.AriaRole(string role)` — sets `role`.
- `.AriaLabel(string label)` — accessible name.
- `.AriaLabelledBy(string id)` / `.AriaDescribedBy(string id)` — point at the
  id of a labelling/describing element.

State and interaction:

- `.AriaExpanded(bool)` / `.AriaSelected(bool)` / `.AriaChecked(bool)`
- `.AriaDisabled(bool)` / `.AriaBusy(bool)`
- `.AriaCurrent(string)` — e.g. `"page"`, `"step"`, `"location"`.

Visibility and live regions:

- `.AriaHidden(bool)` — hide from assistive tech.
- `.AriaLive(string)` — `"polite"`, `"assertive"`, or `"off"`.
- `.AriaAtomic(bool)` — announce the region as a whole.

Popups and controls:

- `.AriaHasPopup(string)` — e.g. `"menu"`, `"listbox"`, `"dialog"`.
- `.AriaControls(string id)` — id(s) of controlled element(s).

## Example

```csharp
using static Tesserae.UI;

var submit = Button("Click Me")
    .AriaRole("button")
    .AriaLabel("Submit the form");

var toggle = Button("Toggle")
    .AriaExpanded(true)
    .AriaControls("content-id");

var toast = Message("Success!")
    .AriaLive("polite")
    .AriaAtomic(true);
```

## Principles to follow

- Never rely on color alone; keep text contrast at least 4.5:1.
- Provide alt text and visible labels; prefer semantic HTML elements.
- Respect reduced-motion and reflow at all screen sizes.

## Related

- Button — `.button.md`
- Full docs & API: `/tesserae/get-started/accessibility`
