---
name: tesserae-overview
description: Orientation for the Tesserae C# UI toolkit — what it is, how components compose, and which per-component skill to reach for. Use when starting work in this repo, picking a component or layout, or unsure which Tesserae skill applies.
---

# Tesserae overview

Tesserae is a C# UI toolkit for building web apps, compiled to JavaScript by the
**h5** compiler. You write strongly-typed C#; h5 emits the JS that runs in the
browser.

- Components live in `Tesserae/src/Components/`.
- Factory methods (`UI.Button`, `UI.TextBlock`, …) live in `Tesserae/src/Base/UI.Components.cs`.
- Fluent extensions live in `Tesserae/src/Extensions/`.
- Samples live in `Tesserae.Tests/`.

Bring the factories and DOM helpers into scope with:

```csharp
using static Tesserae.UI;
using static H5.Core.dom;
```

Every component implements `IComponent` (one method, `HTMLElement Render()`) and
is configured with fluent, `this`-returning helpers. Mount a root component with
`MountToBody(...)` or `MountCenteredToBody(...)`.

## Finding the right skill

There is one skill per component, named by its kebab-case slug (e.g. `button`,
`text-box`, `details-list`, `context-menu`). Load the skill for the component you
are using — it has the factory signature, the key fluent methods, and an example.

Skills are grouped the way the docs are:

- **Components** — plain widgets: `button`, `text-box`, `dropdown`, `toggle`,
  `slider`, `icon`, `avatar`, `badge`, `card`, charts, pickers, and more.
- **Collections** — list/layout containers: `stack`, `grid`, `details-list`,
  `items-list`, `virtualized-list`, `masonry`, `sortable-stack`, `observables`.
- **Surfaces** — overlays and tabbed surfaces: `modal`, `dialog`, `panel`,
  `layer`, `context-menu`, `pivot`, `float`.
- **Utilities** — non-visual helpers: `colors`, `theme-colors`, `gradients`,
  `uicons`, `emoji`, `gestures`, `tippy`, `toast`, `validator`, `defer`.
- **Concepts** — `core-concepts`, `styling`, `layout-alignment`, `routing`,
  `project-setup`, `accessibility`, `iconography`, `custom-styles`.

## Layout cheat-sheet

- One-axis flow (toolbar, sidebar, form) → `Stack` (see `stack`).
- Two-axis grid with named tracks → `Grid` (see `grid`).
- Two resizable panes → `SplitView` / `HorizontalSplitView`.
- Pinned overlay on a parent → `Float`.
- Variable-height tile feed → `Masonry`.
- Modal/popover that must escape clipping → `Layer` (usually via `Dialog`,
  `Modal`, `ContextMenu`).

Sizing helpers (`.Width()`, `.WS()`, `.Grow()`, `.Stretch()`, …) work on any
`IComponent` via the wrap-and-transfer protocol — if `.WS()` "doesn't work",
inspect the DOM: the style likely landed on the `tss-stack-item` wrapper. See the
repo `CLAUDE.md` "Layout system" section for the full protocol.

The sizing, spacing, alignment, text, styling, tooltip, accessibility, gesture,
binding, and lifecycle helpers are all `IComponent` extension methods — see the
`icomponent` skill for the complete catalog.

## Extending the toolkit

- `creating-a-component` — implement `IComponent` / derive from `ComponentBase`.
- `javascript-interop` — call JS and browser APIs from C# via h5.
- `wrap-a-javascript-library` — bundle and wrap a third-party JS library.
