---
name: metric
description: A KPI tile showing a large value with a title plus optional trend indicator and sparkline chart. Use when building dashboards or stat cards in a Tesserae (C#/Transpose) app.
---

# Metric

A numeric KPI tile: a title, a large value, and optional change indicator and
chart. Usually placed inside a `Card`.

## Create

- `Metric(string title, string value)` — text title and value.
- `Metric(IComponent title, IComponent value)` — component title/value (e.g. title with an info-tooltip icon).

Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Change(IComponent)` — trend/delta indicator (typically an icon + colored text).
- `.Chart(IComponent)` — inline chart, e.g. `Sparkline(double[])`.

## Example

```csharp
using static Tesserae.UI;

var dashboard = HStack().Children(
    Card(Metric("Requests", "1.1k")).W(200.px()),
    Card(Metric("Web traffic", "1,234,567")
        .Chart(Sparkline(new double[] { 10, 20, 15, 30, 25, 40 }))
        .Change(HStack().AlignItemsCenter().Children(
            Icon(UIcons.ArrowUp).Foreground(Theme.Colors.Green600).S(),
            TextBlock("+12.3%").Foreground(Theme.Colors.Green600)))
    ).W(250.px())
);
```

## Related

- Card — `/tesserae/components/card`
- Sparkline — `/tesserae/components/sparkline`
- Full docs & API: `/tesserae/components/metric`
