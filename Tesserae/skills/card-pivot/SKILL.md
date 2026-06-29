---
name: card-pivot
description: A pivot whose tabs render as connected cards with a shared border, suited to dashboard-style metric switching. Use when selectable metric cards control the view below them in a Tesserae (C#/h5) app.
---

# CardPivot

A tabbed surface where each tab is a card with a shared border. Typically used
for dashboard metrics that, when clicked, switch the panel shown below.

## Create

`CardPivot()` — returns a `CardPivot`. Add tabs with the `.CardPivot(...)`
extension. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.CardPivot(id, titleCreator, contentCreator, cached = false)` — add a tab. `titleCreator`/`contentCreator` are `Func<IComponent>` (the title is usually a `Metric(...)` card).
- `.Select(id, refresh = false)` — switch tabs.
- `.OnNavigate(...)` / `.OnBeforeNavigate(...)` — callbacks; `e.Cancel()` blocks navigation.

## Example

```csharp
using static Tesserae.UI;

var pivot = CardPivot()
    .CardPivot("req",  () => Metric("Requests", "1.1k"),  () => Card(TextBlock("Requests").P(32)))
    .CardPivot("tok",  () => Metric("Tokens", "196.97k"), () => Card(TextBlock("Tokens").P(32)))
    .CardPivot("cost", () => Metric("Cost", "$0.09"),     () => Card(TextBlock("Cost").P(32)));
```

## Related

- Pivot — `../pivot/SKILL.md`
- SegmentedPivot — `../segmented-pivot/SKILL.md`
- Full docs & API: `/tesserae/surfaces/card-pivot`
