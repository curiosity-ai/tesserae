---
name: tabbed-modal
description: A pattern (not a distinct class) that hosts Modals as tabs inside a Pivot, using closeable tabs and the Pivot's caching/lifecycle. Use to present several modal-style panels as switchable, closeable tabs in a Tesserae (C#/h5) app.
---

# TabbedModal

Not a standalone component — it is the pattern of hosting `Modal` instances as
tabs inside a `Pivot` via `Pivot.Host(...)`. The pivot manages caching and
lifecycle, and closeable tabs show a close button automatically.

## Create

Build a `Pivot()` and add modals with the `.Host(...)` extension. Bring
factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `pivot.Host(Modal modal, string id, Func<IComponent> titleCreator, bool closeable = true, Action onClosed = null)` — embed a modal as a tab.
- `PivotTitle("Text", UIcons.Browser)` — tab title `Func<IComponent>`.
- `pivot.Select(id)` — focus a tab.
- Closeable tabs render a close icon; `onClosed` fires when removed.

## Example

```csharp
using static Tesserae.UI;

var pivot = Pivot();

void AddTab(int n, bool closeable)
{
    var id = $"modal-{n}";
    var modal = Modal($"Modal {n}").Content(
        VStack().Children(TextBlock($"Content of Modal {n}")).Padding(16.px()));

    pivot.Host(modal, id, PivotTitle($"Modal {n}", UIcons.Browser),
               closeable: closeable, onClosed: () => console.log($"{id} closed"));
    pivot.Select(id);
}

AddTab(1, closeable: true);
```

## Related

- Pivot — `pivot.md`
- Modal — `modal.md`
- Full docs & API: `/tesserae/surfaces/tabbed-modal`
