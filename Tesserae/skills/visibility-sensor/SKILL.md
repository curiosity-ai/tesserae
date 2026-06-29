---
name: visibility-sensor
description: An invisible marker that fires a callback when it scrolls into the viewport. Use when triggering lazy loading, telemetry, or infinite-scroll pagination in a Tesserae (C#/h5) app.
---

# VisibilitySensor

Fires an action the moment it becomes visible in the viewport (uses an `IntersectionObserver`). By default the callback fires once; set `singleCall: false` to receive every visibility transition. Place it at the bottom of a scrollable list to drive infinite scroll.

## Create

`UI.VisibilitySensor(Action<VisibilitySensor> onVisible, bool singleCall = true, IComponent message = null)` — the optional `message` renders inside the sensor (e.g. a "loading" placeholder).
`using static Tesserae.UI;`.

## Key configuration

- `onVisible` — callback; the argument is the sensor itself.
- `singleCall` — `true` fires once then disconnects; `false` fires on each transition.
- `message` — optional visible content.
- `.Reset()` — re-arm a single-call sensor so it can fire again.

## Example

```csharp
using static Tesserae.UI;

var stack = Stack().Children(
    Stack().Height(800.px()),
    VisibilitySensor(
        onVisible: _ => LoadMoreItems(),
        singleCall: true,
        message: TextBlock("Loading more…")
    )
);
```

## Related

- InfiniteScrollingList — `.../infinite-scrolling-list/SKILL.md`
- VirtualizedList — `.../virtualized-list/SKILL.md`
- Full docs & API: `/tesserae/components/visibility-sensor`
