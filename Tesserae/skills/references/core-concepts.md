---
name: core-concepts
description: The four foundational ideas of Tesserae — IComponent, the fluent UI API, layout containers, and reactive state (observables + Defer). Use when learning how a Tesserae (C#/Transpose) app is structured or wiring up reactive UI.
---

# Core Concepts

Tesserae builds UIs from C# objects implementing one interface, configures them
with fluent extensions, and re-renders parts of the tree when observable state
changes. Bring factories into scope with `using static Tesserae.UI;`.

## IComponent

Every component implements `IComponent`, which has one method:

```csharp
public interface IComponent { HTMLElement Render(); }
```

Containers call `Render()` for their children, so you rarely call it directly —
the exception is the app root: `document.body.appendChild(app.Render())` (or
`MountToBody` / `MountCenteredToBody`). Create components via `UI` factories
(`Button`, `TextBlock`, `Stack`, `Grid`, …).

## The fluent API

Factories return the concrete type; configuration methods return the same type,
so calls chain. Two kinds:

- **Component methods** — `Button.Primary()`, `TextBlock.SemiBold()`.
- **Extension methods** on `IComponent` (sizing, spacing, alignment) —
  `.W(120.px())`, `.P(16)`, `.S()`, `.Grow()`.

Grab a mid-chain reference with `.Var(out var saveButton)`.

## Layout containers

`VStack()` / `HStack()` (flexbox), `Grid(...)` (CSS grid), `SplitView`,
`Float`, `Masonry`. Add children with `.Children(...)` or `.Add(...)`.

## Reactive state

Observables hold state and notify subscribers; `Defer`/`DeferSync` re-render
when an observable changes.

- `SettableObservable<T>` — read/write `.Value`; `.Update(...)` mutates in place.
  `.Subscribe(...)` fires immediately; `.ObserveFutureChanges(...)` skips the
  first call.
- `ObservableList<T>` / `ObservableDictionary<,>` / `ObservableHashSet<T>` —
  observable collections.
- `DeferSync(obs, val => component)` (sync) / `Defer(...)` (async) — re-renders
  the produced content when any of up to ten passed observables changes.
- Two-way binding: input components (`TextBox`, `CheckBox`, `Toggle`) implement
  `IBindableComponent<T>`; `.Bind(observable)` syncs both directions.

```csharp
using static Tesserae.UI;

var count = new SettableObservable<int>(0);
var layout = VStack().AlignItemsCenter().Children(
    DeferSync(count, c => TextBlock($"Count: {c}").Large().SemiBold()),
    HStack().Children(
        Button("-").OnClick(() => count.Value--),
        Button("+").Primary().OnClick(() => count.Value++)
    ));
document.body.appendChild(layout.Render());
```

Core pattern: put mutable data in an observable, render the data-dependent part
inside a `Defer` over it, and update the observable in event handlers. For large
lists prefer the purpose-built collection components (Items List, Virtualized
List, Observable Stack) over rebuilding subtrees.

## Composing reusable UI

- Factory methods returning `IComponent` — stateless fragments.
- Classes implementing `IComponent` — views that own state; build the tree once
  and return it from `Render()`.

## Related

- Styling — `.styling.md`
- Layout & Alignment — `.layout-alignment.md`
- Routing — `.routing.md`
- Full docs & API: `/tesserae/core-concepts`
