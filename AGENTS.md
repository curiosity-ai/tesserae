# AGENTS

## Repository overview
- Tesserae is a C# UI toolkit compiled to JavaScript via the **h5** compiler (see `Tesserae/Tesserae.csproj` and `Tesserae/h5*.json`).
- Core UI components live under `Tesserae/src`, with component factories and helpers in `Tesserae/src/Base/UI.Components.cs`.
- Samples and demos live in `Tesserae.Tests` (referenced in `README.md`).

## Build & packaging notes
- The main library project uses the `h5.Target` SDK and references `h5`, `h5.core`, and `h5.Newtonsoft.Json` packages.
- H5 build configuration and resource bundling are defined in `Tesserae/h5.json` (includes JS/CSS bundling, minified and non-minified outputs, and resource packaging).
- Local dev hosting is documented in `README.md` (build output under `bin/Debug/netstandard2.0/h5` and serve via `dotnet serve`).

## UI composition patterns
- UI creation is centered around the static `UI` class in `UI.Components.cs`, which provides factory methods for components (e.g., `UI.Button`, `UI.TextBlock`, etc.).
- **Static constructor pattern**: `UI` is a static partial class with a static constructor used as a central, static entry point for component creation and helpers.
- **Fluent APIs**: Many components are configured via fluent-style extension methods (see helpers like `UI.Id`, `UI.Class`, `UI.RemoveClass`, `UI.Do`, etc., in `UI.Components.cs`, plus additional extensions in `Tesserae/src/Extensions`).

## Conventions
- When adding a new component, consider adding:
  - The component implementation under `Tesserae/src/Components`.
  - A factory method in `UI.Components.cs` for consistency with existing usage.
  - Any fluent helper or extension methods in `Tesserae/src/Extensions` if needed.

## Layout system

Tesserae has a small set of layout containers and a unified set of sizing
helpers that work across all of them. Understanding the wrap-and-transfer
protocol below is the key to debugging layout problems.

### Sizing helpers (apply to any `IComponent`)

Defined in `Tesserae/src/Extensions/IComponentExtensions.cs`:

- `.Width(unitSize)` / `.Height(unitSize)` — fixed size (e.g. `.W(100)`, `.W(50.percent())`).
- `.MinWidth` / `.MaxWidth` / `.MinHeight` / `.MaxHeight` — bounds.
- `.WidthStretch()` / `.WS()` — `width: 100%`.
- `.HeightStretch()` / `.HS()` — `height: 100%`.
- `.Stretch()` / `.S()` — both width and height `100%`.
- `.Grow(int = 1)` — sets `flex-grow` (only meaningful inside a `Stack`).
- `.Shrink()` / `.NoShrink()` — sets `flex-shrink` to `1` or `0`.
- `.GridColumn(start, end)` / `.GridColumnStretch()` / `.GridRow(...)` /
  `.GridRowStretch()` — placement inside a `Grid` (call before `Add`).
- `.AlignStretch()` — `align-self: stretch` on the stack item.

All of these write the CSS property to the element, tag it with a marker
attribute (`tss-stk-w`, `tss-stk-h`, `tss-stk-fg`, `tss-grd-c`, …), and — if
the component has already been wrapped — mirror the value onto its wrapper.

### The wrap-and-transfer protocol

Flexbox/Grid only obey sizing properties on the **direct child** of the
container, but users naturally call `.WS()` on the rendered component before
adding it. To bridge this, every container's `GetItem(component)` wraps the
child in an item div (`tss-stack-item` for Stack/Grid, `tss-masonry-item` for
Masonry) and then calls `CopyStylesDefinedWithExtension`, which:

1. Looks for the marker attributes set by the fluent helpers.
2. For each one found, moves the relevant CSS property from the inner element
   onto the wrapper.
3. For width/height markers, sets the inner element to `100%` so it fills the
   now-correctly-sized wrapper.

`Stack.CopyStylesDefinedWithExtension` in `Tesserae/src/Components/Stack.cs`
is the canonical implementation; `Grid` and `Masonry` delegate to it and add
their own marker handling for grid placement.

A component can opt out of wrapping by implementing `ISpecialCaseStyling` and
exposing a `StylingContainer` — the sizing helpers then write directly onto
that container instead of a wrapper. This is how nested containers (e.g. a
`Grid` inside a `Stack`) avoid an extra wrapper layer.

**Debugging tip:** if `.WS()` "doesn't work", inspect the rendered DOM. The
sizing styles likely live on the `tss-stack-item` wrapper, not on the element
you called the helper on.

### Layout containers

- **`Stack`** (`Tesserae/src/Components/Stack.cs`) — the workhorse. A flexbox
  container with `Orientation.Vertical` (default), `Horizontal`,
  `VerticalReverse`, `HorizontalReverse`. Use `.Grow()` on children to claim
  leftover main-axis space; cross-axis stretches by default.
- **`Grid`** (`Tesserae/src/Components/Grid.cs`) — CSS Grid container with
  explicit `Columns(...)` and `Rows(...)` tracks (`UnitSize[]`), `.Gap()`,
  `.RowGap()`, `.ColumnGap()`, `.AutoRows()`, `.AutoColumn()`, `.FlowColumn()`.
  Children position with `.GridColumn(s, e)` / `.GridRow(s, e)` or stretch
  with `.GridColumnStretch()`.
- **`SplitView`** (vertical split, left/right) and **`HorizontalSplitView`**
  (top/bottom) — two-pane resizable layouts. `Left(...)` / `Right(...)` (or
  `Top` / `Bottom`) take the panes; `LeftIsSmaller(size, max, min)` /
  `RightIsSmaller(...)` pin one pane to a fixed size; `SplitInMiddle()` is the
  default 50/50; `.Resizable(onResizeEnd)` enables the drag handle.
- **`Float`** (`Tesserae/src/Components/Float.cs`) — corner/edge-anchored
  overlay. Takes a child and a `Position` enum (TopLeft, TopRight, Center, …).
  Parent must be position-relative (`Grid.Relative()` exists for this).
- **`Masonry`** (`Tesserae/src/Components/Masonry.cs`) — Pinterest-style
  variable-height columns. Wraps the `masonry-layout` JS library; relayout is
  debounced. Use only when CSS Grid can't model the layout you need.
- **`BackgroundArea`** — full-bleed app-shell wrapper around a single child.
- **`Layer`** / **`LayerHost`** / **`Layers`** — overlay infrastructure
  (modals, dialogs, popovers). Layers render outside the normal DOM tree to
  escape `overflow: hidden` and z-index stacking contexts. `Layers.PushLayer`
  hands out monotonically increasing z-indices that also account for Tippy
  popovers. Use a `LayerHost` to confine layers to a sub-tree of the layout.

### Picking a layout

- One-axis flow (toolbar, sidebar, form) → `Stack`.
- Two-axis grid with named tracks → `Grid`.
- Two resizable panes → `SplitView` / `HorizontalSplitView`.
- Pinned overlay on a parent → `Float`.
- Variable-height tile feed → `Masonry`.
- Modal/popover that must escape clipping → `Layer` (usually wrapped by
  higher-level components like `Dialog`, `Modal`, `ContextMenu`).
