---
name: timeline
description: A vertical timeline that arranges items in sequence, alternating left/right by default or pinned to one side, with an optional per-item marker color. Use when visualising events or steps in chronological order in a Tesserae (C#/h5) app.
---

# Timeline

Renders items down a vertical line. By default consecutive items alternate left/right of
the line; you can pin all items to one side and constrain the timeline width.

## Create

`Timeline()` — returns a `Timeline`. Add items with `.Add(component)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Add(IComponent)` — append an item (alternating side).
- `.Add(IComponent, string color)` — append with a custom marker-circle color.
- `.SameSide()` — render every item on the same (left) side instead of alternating.
- `.SameSideIf(int minWidthPixels)` — switch to same-side automatically below a width.
- `.TimelineWidth(UnitSize)` — set the timeline's max width.
- `.Clear()` / `.Replace(new, old)` — mutate items.
- `.Background` / `.Margin` / `.Padding` — CSS string properties.

## Example

```csharp
using static Tesserae.UI;

var timeline = Timeline()
    .TimelineWidth(600.px())
    .SameSide();

for (int i = 1; i <= 5; i++)
{
    timeline.Add(TextBlock($"Event {i}: details about the event."));
}

return timeline.Render();
```

## Related

- Stack — `../stack/SKILL.md`
- Grid — `/tesserae/collections/grid`
- Full docs & API: `/tesserae/collections/timeline`
