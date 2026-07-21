---
name: observables
description: Lightweight reactive state containers (single values, constants, lists, dictionaries) that notify subscribers on change to drive UI updates. Use when wiring reactive state into components in a Tesserae (C#/Transpose) app.
---

# Observables

Not a visual component — a family of state containers that raise change notifications.
Components and `Defer`/`ObservableStack`/`ItemsList` subscribe to them and re-render when
values change.

## The observable types

- `SettableObservable<T>` — mutable single value. `SettableObservable.For(value)` creates one (type inferred).
- `ConstantObservable<T>` — fixed value wrapped as an `IObservable<T>`; emits once, never changes. `new ConstantObservable<T>(value)`.
- `ObservableList<T>` — observable ordered collection; implements `IList<T>` and `IObservable<IReadOnlyList<T>>`. `new ObservableList<T>(initialValues: …)`.
- `ObservableDictionary<TKey,TValue>` — observable keyed collection; implements `IDictionary<…>` and `IObservable<IReadOnlyDictionary<…>>`.

Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Read / subscribe / set:

- `.Value` — get (and, for `SettableObservable`, set) the current value.
- `.Observe(callback)` — subscribe; called immediately with the current value, then on each change.
- `.ObserveFutureChanges(callback)` — subscribe to changes only (no immediate call).
- `.StopObserving(callback)` — unsubscribe.
- `SettableObservable.Update(Action<T>)` — mutate a reference value in place and force a notification.
- `ObservableList`: `.Add`, `.AddRange`, `.Insert`, `.Remove`, `.RemoveAt`, `.RemoveAll`, `.ReplaceAll`, `.Clear`; `.Delay` debounces notifications.
- `ObservableDictionary`: `.Add`, `.AddRange`, indexer assignment, `.Remove`, `.Clear`.

## Example

```csharp
using static Tesserae.UI;

var status = SettableObservable.For("Idle");
var items  = new ObservableList<string>("Alpha", "Beta");

var output = TextBlock();
status.Observe(value => output.Text($"Status: {value}"));   // fires now and on change

var addButton = Button("Add item").OnClick(() =>
{
    items.Add($"Item {items.Count + 1}");
    status.Value = $"{items.Count} items";                  // notifies subscribers
});

return Stack().Children(output, addButton).Render();
```

## Related

- ObservableStack — `observable-stack.md` (renders an `ObservableList`)
- Defer — `/tesserae/utilities/defer`
- Full docs & API: `/tesserae/collections/observables`
