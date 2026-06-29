---
name: defer-with-progress
description: A Defer variant whose async generator reports progress (0–1) and a status message while loading. Use for long-running tasks that should show a progress bar in a Tesserae (C#/h5) app.
---

# DeferWithProgress

Like `Defer`, but the async generator receives a `reportProgress(float fraction, string message)` callback so the loading UI can show progress. The default loading view is a `ProgressIndicator` plus a message; override it with a load-message generator.

## Create

`UI.DeferWithProgress(Func<Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessage = null)` — returns an `IDefer`. Bring factories into scope with `using static Tesserae.UI;`.

Observable overloads: `UI.DeferWithProgress(observable, async (val, progress) => …)` re-runs when the observable changes (up to 10 observables); the progress callback is the last parameter.

## Key configuration

Same `IDefer` surface as Defer:
- `.Refresh()` / `.RefreshAsync()`.
- `.Debounce(delayInMs, …)` / `.Debounce(delayInMs, maxDelayInMs, …)`.
- `.DoNotWaitForComponentMountingBeforeRendering()`.

Progress fraction is `0f`–`1f` (the default indicator multiplies by 100).

## Example

```csharp
using static Tesserae.UI;

DeferWithProgress(async reportProgress =>
{
    for (int i = 0; i <= 100; i += 10)
    {
        reportProgress(i / 100f, $"Processing step {i / 10} of 10...");
        await Task.Delay(500);
    }
    return TextBlock("Done!").Success().SemiBold();
});
```

## Related

- Defer (no progress) — `defer.md`
- ProgressIndicator — `/tesserae/components/progress-indicator`
- Full docs & API: `/tesserae/utilities/defer-with-progress`
