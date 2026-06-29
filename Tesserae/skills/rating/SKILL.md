---
name: rating
description: A star-rating control for collecting or displaying a 1-to-N value. Use when capturing or showing a star score in a Tesserae (C#/h5) app.
---

# Rating

A row of clickable stars on a 1-to-N scale. Supports interactive selection (clicking the current value again clears it to 0), read-only display, and a custom star count and colour.

## Create

`UI.Rating(int maxStars = 5)` — returns a `Rating`. Also `new Rating(maxStars)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetValue(int)` / `.Value` — set/read the rating (0 = unrated, clamped to `maxStars`).
- `.OnChange(Action<int>)` — fired when the value changes; receives the new value.
- `.ReadOnly(bool = true)` / `.IsReadOnly` — non-interactive display mode.
- `.Color(string)` — custom fill colour for active stars.

## Example

```csharp
using static Tesserae.UI;

var rating = Rating(5)
    .SetValue(3)
    .OnChange(v => Toast().Information(v == 0 ? "Cleared" : $"Rated {v}"));

// Read-only display
var stars = Rating(5).SetValue(4).ReadOnly();
```

## Related

- Full docs & API: `/tesserae/components/rating`
