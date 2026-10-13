---
name: accessibility
description: ARIA/accessibility fluent helpers that add roles, labels, and states to any Tesserae component. Use when adding ARIA attributes, labels, or screen-reader support in a Tesserae (C#/Transpose) app.
---

# Accessibility

Tesserae components ship with keyboard support and sensible ARIA defaults. The
`IAccessibilityExtensions` fluent methods let you add or override ARIA
attributes on any `IComponent`. Bring factories into scope with
`using static Tesserae.UI;`.

## What the toolkit does for you

- **Icons are hidden from assistive tech.** Every `<i>` the toolkit creates (icons, separators,
  chevrons) carries `aria-hidden="true"`, so an icon's CSS glyph is not read out and does not end up
  in the accessible name of the button around it - `Button("Save").SetIcon(UIcons.Disk)` is named
  "Save", which is also what `getByRole('button', { name: 'Save', exact: true })` finds in browser
  automation. Remove the attribute from an `<i>` that carries meaning of its own and label it.
- **An icon-only control is named after its tooltip.** `.Tooltip("Delete")` on an element with no
  text, no `aria-label` and no `aria-labelledby` also sets `aria-label="Delete"` - otherwise a screen
  reader (or a test) sees an unnamed button. It is decided when the tooltip is set: give a control
  that gains text later an explicit `.AriaLabel(...)`.

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

- Button — `button.md`
- Full docs & API: `/tesserae/get-started/accessibility`
