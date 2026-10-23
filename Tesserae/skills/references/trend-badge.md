---
name: trend-badge
description: A small pill with an arrow and the size of a change, coloured by whether the movement is good or bad news rather than by its direction. Use for week-on-week deltas in KPI tables and dashboards in a Tesserae (C#/Transpose) app.
---

# TrendBadge

An arrow plus the size of a change ("down 31", "up 1.7pp"). The direction and the colour are independent:
a falling cost is good news (green, arrow down), a falling revenue is bad news (red, arrow down), and a
figure with no target to judge it against stays neutral (grey).

## Create

`UI.TrendBadge(string text, TrendDirection direction = Flat, TrendSentiment sentiment = Neutral)`, or
`TrendBadge.For(double change, string text, bool higherIsBetter = true)` which derives both from the sign of
the change and which way is better. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Direction(TrendDirection)` — `Up`, `Down`, `Flat`.
- `.Sentiment(TrendSentiment)` / `.Good()` / `.Bad()` / `.Neutral()` — the colour.
- `.SetText(string)` — change the text.

## Example

```csharp
using static Tesserae.UI;

TrendBadge.For(change: -31, text: "\u00a331", higherIsBetter: false); // arrow down, green: cost fell
TrendBadge.For(change: +1.7, text: "1.7pp", higherIsBetter: false);   // arrow up, red: cost rose
TrendBadge("0.8pp", TrendDirection.Up).Neutral();                     // context metric, no good direction
```

## Related

- DataTable — `data-table.md`
- Metric (`.Change(...)` takes a TrendBadge) — `metric.md`
- Sparkline — `sparkline.md`
