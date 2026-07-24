---
name: action-button
description: A composite button with a main display area plus a separate action icon, each independently clickable. Use when you need a primary action and a secondary trigger (e.g. dropdown/callout) in one control in a Tesserae (C#/Transpose) app.
---

# ActionButton

`ActionButton` pairs a clickable display region with a smaller action icon button.
Each region raises its own click event, so the display can run the main action while
the action icon opens a menu, callout, or secondary behaviour.

## Create

`UI.ActionButton(string textContent, ...)` — text label, optional `displayIcon:` and
`actionIcon:`. Overload `UI.ActionButton(IComponent content, ...)` takes any component
as the display content. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnClickDisplay((display, evt) => ...)` — handler for the main area.
- `.OnClickAction((button, evt) => ...)` — handler for the action icon.
- `.Primary()` / `.Danger()` — tone (default is neutral).
- `.Disabled(bool = true)` — disable interaction.
- `.ModifyActionButton(Action<IComponent>)` — tweak the action icon (e.g. add a tooltip via `Raw(c).Tooltip(...)`).
- `.Background` — CSS background of both regions.

Constructor args: `displayIcon`, `actionIcon` (defaults `UIcons.AngleCircleDown`),
`actionColor`, icon weights/sizes.

## Example

```csharp
using static Tesserae.UI;

var b = ActionButton("Save", displayIcon: UIcons.Disk)
    .Primary()
    .OnClickDisplay((display, evt) => Save())
    .OnClickAction((button, evt) => ShowSaveOptions())
    .ModifyActionButton(c => Raw(c).Tooltip("More options", arrow: true));
```

## Related

- Button — `button.md`
- Full docs & API: `/tesserae/components/action-button`
