---
name: metric
description: A KPI tile showing a large value with a title, an optional leading icon tile, a trend indicator and an inline chart such as a sparkline or contribution bar. Use when building dashboards or stat cards in a Tesserae (C#/Transpose) app.
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
- `.ChangeInHeader(bool = true)` — pull the change up level with the title, pushed to the far end of its line, so the value, the chart and its legend read straight down the card.
- `.Chart(IComponent)` — inline chart under the value: a `Sparkline`, a `ContributionBar`, anything renderable. It takes the full width of the tile.
- `.SetIcon(UIcons, color, weight)` / `.SetIcon(string text, color, TextSize?)` / `.SetIcon(IComponent, color)` — an `IconTile` in front of the title and the value: the same rounded, tinted square an `OmniResult` row leads with. Pass the full-strength colour the glyph should be; the wash behind it is computed from it. A null component takes the tile away.
- `.IconSize(UnitSize)` — how big that tile is (44px by default).
- `.ValueFirst(bool = true)` — draw the value above the title, so the number reads first and the words under it only say what was counted ("5 / In my scope").

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

## Counter tiles and breakdowns

```csharp
using static Tesserae.UI;

// A counter: the tile, then the number, then what was counted
Card(Metric("In my scope", "5").SetIcon(UIcons.Inbox, Theme.Colors.Purple600).ValueFirst()).W(240.px());

// A percentage broken down by a ContributionBar under it
Card(Metric(TextBlock("Rejection by DOV").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground),
            TextBlock("11%").XXLarge().Bold())
    .ChangeInHeader()
    .Change(TextBlock("-3pt").Small().SemiBold().Foreground(Theme.Colors.Green600))
    .Chart(ContributionBar()
        .Max(47).Thickness(8.px()).ShowValues(false)
        .Add("38 validated", 38, Theme.Colors.Green600)
        .Add("5 rej.",        5, Theme.Colors.Red500)
        .Add("4 deleg.",      4, Theme.Colors.Blue500))).W(360.px());
```

## Related

- Card — `card.md`
- IconTile — the tile `SetIcon` puts in front — `icon-tile.md`
- ContributionBar — a breakdown under the value — `contribution-bar.md`
- Sparkline — `sparkline.md`
- Full docs & API: `/tesserae/components/metric`
