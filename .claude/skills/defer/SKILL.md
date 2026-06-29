---
name: defer
description: A placeholder that asynchronously loads its real content from a Task, optionally observing observables to refresh. Use when rendering heavy or async content without blocking initial UI in a Tesserae (C#/h5) app.
---

# Defer

Renders a loading placeholder, then swaps in content produced by an async function. The task runs on first render (when the container mounts). Overloads can observe one or more `IObservable<T>` values and re-run the generator when they change.

## Create

`UI.Defer(Func<Task<IComponent>> asyncGenerator)` or `UI.Defer(asyncGenerator, IComponent loadMessage)` — returns an `IDefer`. Bring factories into scope with `using static Tesserae.UI;`.

Observable overloads: `UI.Defer(observable, async val => …, loadMessage)` (up to 10 observables) re-render on change. (`DeferSync` exists for synchronous generators.)

## Key configuration

`IDefer` members:
- `.Refresh()` / `.RefreshAsync()` — re-run the generator.
- `.Debounce(delayInMs, millisecondsForLoadingMessage = 1000)` and `.Debounce(delayInMs, maxDelayInMs, …)` — coalesce rapid refreshes.
- `.DoNotWaitForComponentMountingBeforeRendering()` — skip the mount wait (cheaper when many items render and immediate mounting is known).

The `loadMessage` is any `IComponent` (e.g. `Spinner(...)`, `TextBlock("Loading...")`); shown after a short delay. If the task faults, an error block is rendered.

## Example

```csharp
using static Tesserae.UI;

var content = Defer(async () =>
{
    await Task.Delay(1000);
    return TextBlock("Loaded!").Success();
},
loadMessage: Spinner("Loading content..."));
```

## Related

- DeferWithProgress (progress-reporting variant) — `../defer-with-progress/SKILL.md`
- Spinner / ProgressIndicator — `/tesserae/components/spinner`, `/tesserae/components/progress-indicator`
- Full docs & API: `/tesserae/utilities/defer`
