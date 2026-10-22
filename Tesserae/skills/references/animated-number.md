---
name: animated-number
description: A number that animates to each new value, counting through the values in between or ticking the new value in. Use for totals, percentages, counters and metrics that change in a Tesserae (C#/Transpose) app.
---

# AnimatedNumber

Shows a number that animates when it changes, so the change is seen rather than
missed. `Count()` (default) runs through the values in between, fast then
settling (ease-out over `Duration`); `Tick()` slides the new value in, from
below when it went up and from above when it went down. Digits are tabular, so
the text does not jitter sideways. A value set mid-count carries on from what
is on screen. Under reduced motion the new value just appears.

## Create

`UI.AnimatedNumber(double value = 0, string format = "N0")` returns an
`AnimatedNumber` (an `IBindableComponent<double>`). The starting value is shown
without animation.

## Key configuration

- `.SetValue(double)` / `.Value` — animate to a new value.
- `.Bind(IObservable<double>)` — follow an observable; `AsObservable()` exposes the value.
- `.Format(string)` (a .NET numeric format: `"N0"`, `"0.0"`, `"P0"`), `.Prefix(string)`, `.Suffix(string)`.
- `.Duration(int ms)` (default 650), `.Count()`, `.Tick()`.
- Style the text with `.Style(s => s.fontSize = "28px")` or wrap it in a `TextBlock`-styled container.

## Example

```csharp
using static Tesserae.UI;

var total = AnimatedNumber(100).Prefix("$");
CheckBox("Audit log (+$60)").OnChange((cb, _) => total.SetValue(total.Value + (cb.IsChecked ? 60 : -60)));

var likes = AnimatedNumber(128).Tick();
var progress = AnimatedNumber(0, "0").Suffix("%").Bind(uploadPercent);
```

## Related

- Metric tiles — `metric.md`
- Progress — `progress-indicator.md`
- Pop on change and other feedback — `micro-interactions.md`
- Full docs & API: `/tesserae/text/animated-number`
