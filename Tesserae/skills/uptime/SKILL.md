---
name: uptime
description: Status-history visualisations — UptimeBars (a horizontal strip of per-period status bars) and UptimeCalendar (a month grid of daily status). Use when showing service availability/health over time in a Tesserae (C#/h5) app.
---

# Uptime (UptimeBars / UptimeCalendar)

Two components for visualising status over time, each with hover tooltips. `UptimeBars` is a status-page-style strip of bars; `UptimeCalendar` is a month-grid of day cells. Both take `(UptimeStatus status, IComponent tooltipContent)` items.

## Create

`UI.UptimeBars()` — the bar strip.
`UI.UptimeCalendar(string title, string subtitle)` — a titled month card.
`using static Tesserae.UI;`.

`UptimeStatus` values: `Operational`, `Minor`, `Major`, `Maintenance`, `None`, `Future`.

## Key configuration

- `.Items(IEnumerable<(UptimeStatus, IComponent)>)` — set the periods/days; the `IComponent` is the hover tooltip (pass `null` for none).
- `.Compact()` (UptimeBars only) — denser bars.
- `Margin` / `Padding` — spacing (both implement `IHasMarginPadding`).

## Example

```csharp
using static Tesserae.UI;

var items = new List<(UptimeStatus, IComponent)>();
for (int i = 0; i < 90; i++)
{
    var status = UptimeStatus.Operational;
    items.Add((status, Raw(TextBlock($"Day {i}: {status}").Small().Render())));
}

var bars = UptimeBars().Items(items);

var month = UptimeCalendar("July 2024", "99.8%").Items(items.Take(30));
```

## Related

- Components overview — `/tesserae/components/`
- Full docs & API: `/tesserae/components/uptime`
