# CLAUDE.md

## Repository overview

Tesserae is a C# UI toolkit for building web applications, compiled to JavaScript via the **Transpose** compiler.

- Core UI components: `Tesserae/src/Components`
- Component factories and helpers: `Tesserae/src/Base/UI.Components.cs`
- Fluent extensions: `Tesserae/src/Extensions`
- Samples and demos: `Tesserae.Tests/`
- Project and build config: `Tesserae/Tesserae.csproj`, `Tesserae/tps.json`

## Skills

`Tesserae/skills/` is a **single Claude skill** named `tesserae`, structured per
Anthropic's skill guidance (one root `SKILL.md`, with detail layered into a
`references/` folder for progressive disclosure):

- `Tesserae/skills/SKILL.md` — the root skill. Explains the library basics, using
  components, layout with `Stack`/`Grid`, the key sizing/spacing/alignment helpers,
  and an index of every reference file and how to find it.
- `Tesserae/skills/references/<slug>.md` — one reference per component or topic
  (kebab-case slug, e.g. `button.md`, `details-list.md`, `context-menu.md`), plus
  the cross-cutting references `icomponent.md` (the `IComponent` interface and every
  fluent extension method), `core-concepts.md`, `creating-a-component.md`,
  `javascript-interop.md`, `wrap-a-javascript-library.md`, and the styling/layout
  topic docs. Each has the factory signature, key fluent methods, and an example.

These skills are written for **consumers of the Tesserae NuGet package**, not for
this repo, which is why they live under the project folder (`Tesserae/skills/`)
rather than this repo's `.claude/`.

### How the skills ship to consumers

The skill is packed into the Tesserae NuGet package and extracted into a
referencing project's `.claude/skills/tesserae/` on build. The plumbing lives in
[`Tesserae/Tesserae.csproj`](Tesserae/Tesserae.csproj) and
[`Tesserae/buildTransitive/Tesserae.targets`](Tesserae/buildTransitive/Tesserae.targets):

- The csproj packs everything under `Tesserae/skills/**` (the root `SKILL.md` and
  the whole `references/` tree) into the package's `skills/` folder, and a
  `_WriteSkillsVersion` target stamps the package version into
  `skills/.skills-version` (`NoDefaultExcludes` lets the dot-file pack).
- `Tesserae.targets` (auto-imported via `buildTransitive/`, so it reaches both
  direct and transitive consumers) walks up from the consuming project to find a
  `.claude` folder. If one exists, it compares the shipped `.skills-version`
  against the installed marker and, when they differ, wipes and re-copies the
  whole `skills/` payload into `.claude/skills/tesserae/`. No `.claude` folder →
  it does nothing.

So a Tesserae app that has a `.claude` folder automatically gets the skill
refreshed whenever it upgrades the package. Changes here reach consumers on the
next package version bump (the version marker is what triggers the re-copy). Note
the install folder is `tesserae` (the skill `name`, lowercase kebab-case); the
targets *filename* must stay `Tesserae.targets` (= the csproj `<PackageId>`) so
NuGet auto-imports it, and `_SkillsPackageId` inside the targets sets the install
folder name.

### Keep skills in sync with the code

The skill is documentation that drifts out of date if the code changes underneath
it. Whenever you change the public surface of the toolkit, update the skill in the
same change:

- **New component** — add `Tesserae/skills/references/<slug>.md` (slug = the
  doc/kebab-case name), link it from related references, and add it to the index in
  `Tesserae/skills/SKILL.md`.
- **Changed factory or fluent method** (renamed, new/removed parameters, new
  configuration method, changed default) — update that component's reference so the
  signatures and examples still compile.
- **New or changed `IComponent` extension method** (under
  `Tesserae/src/Extensions/`) — update `references/icomponent.md` (and the sizing
  cheat-sheet in `SKILL.md` if it's a common one).
- **Removed component** — delete its `references/<slug>.md`, drop it from the
  `SKILL.md` index, and fix any "Related" links that pointed at it.

The root `SKILL.md` `name` must be `tesserae` and its `description` must state what
it does and when to use it (no `<`/`>`). Keep `SKILL.md` a focused overview and
push detail into `references/`. The same applies to the matching pages in the
`documentation` repo under `tesserae/` — update them alongside the references.

## Icon fonts

Everything about the bundled icon set is generated: the woff2 files under
`Tesserae/tps/assets/fonts/`, the `uicons-*.css` files, and
`Tesserae/src/Icons/UIcons.cs`. **`Build.UpdateInterfaceIcons` owns all of it** —
never edit those files by hand.

```bash
dotnet run --project Build.UpdateInterfaceIcons   # download, regenerate, re-centre
dotnet run --project Build.UpdateInterfaceIcons -- --help
```

One run, four stages in a fixed order: download the nine weights, rewrite the
stylesheets and the enum, measure every glyph in headless Chromium, then bake the
optical centering into the glyph outlines and re-measure to prove it landed. It exits
non-zero if any check fails and only then writes the `uicons-source.txt` marker, so a
failed run does not look finished. The only prerequisite is a Chromium for Playwright
(`bin/Debug/net10.0/playwright.ps1 install chromium`); the font surgery is C#
(`Woff2File.cs`, `TransformedGlyf.cs`, `CmapLookup.cs`) and needs nothing installed.

Bumping icons is rare and the run takes minutes, so it is gated: the downloaded
version plus a hash of every woff2 is compared against
`Build.UpdateInterfaceIcons/uicons-source.txt`, and an unchanged set stops the run.
`--force` overrides; `--centre-only` re-centres what is already in the tree
without downloading. The fonts in the tree no longer match the vendor bytes, which
is why the marker records what was *downloaded* rather than hashing the tree.

The centering used to be a generated stylesheet that nudged icons with
`position: relative`. It was removed because measurement showed it could not work
at Tesserae's sizes: a paint-time offset is rounded to a whole CSS pixel, so at the
13px `Icon()` default a 0.02–0.035em nudge did nothing, and the rounding applies to
the accumulated position, so the same icon moved a pixel in one container and not in
another. An offset baked into the outline is part of the shape the rasterizer draws
and survives at any size.

**When touching any of this, read `.claude/skills/uicons-fonts/SKILL.md`** — it
covers the two traps in these fonts (declared bboxes that disagree with the
outlines, and the 300-unit em square), the rules that keep composed icons
registered with each other, and which checks fail the run.

## Installing Transpose

Install or update the Transpose compiler and the dotnet serve tool globally before getting started:

```bash
dotnet tool update --global Transpose.Compiler
dotnet tool update --global dotnet-serve
```

## Build

```bash
dotnet build
```

The Transpose compiler translates C# to JavaScript. Output lands in `bin/Debug/netstandard2.0/tps/` (or `bin/Release/...`).

To serve locally:

```bash
cd bin/Debug/netstandard2.0/tps/
dotnet serve --port 5000
```

## UI composition patterns

- Component creation goes through the static `UI` class (`UI.Components.cs`), which exposes factory methods like `UI.Button`, `UI.TextBlock`, etc.
- `UI` is a static partial class with a static constructor used as the central entry point.
- Components are configured via fluent-style extension methods (e.g., `UI.Id`, `UI.Class`, `UI.Do`).

## Conventions

### Type safety

Favor strong, static typing. Avoid `dynamic` unless absolutely necessary
(e.g. untyped JavaScript interop that can't be modeled otherwise); keep its
use narrow, convert back to a concrete type ASAP, and add a brief comment
explaining why a typed alternative isn't possible.

### Adding a component

When adding a new component:

1. Add the implementation under `Tesserae/src/Components`.
2. Add a factory method in `UI.Components.cs`.
3. Add fluent helpers or extension methods in `Tesserae/src/Extensions` if needed.
4. Add a sample in `Tesserae.Tests` demonstrating usage.

## Layout system

Tesserae has a small set of layout containers and a unified set of sizing
helpers that work across all of them. Understanding how a child becomes a
stack/grid item, below, is the key to debugging layout problems.

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

### How a child becomes a stack/grid item

Flexbox/Grid only obey sizing properties on the **direct child** of the
container, and that direct child is the component's own rendered element:
`Stack.GetItem` / `Grid.GetItem` add the `tss-stack-item` class to it and add it
as-is. So `.WS()` and friends write to exactly the box the container measures,
and nothing has to be moved afterwards.

`Masonry`, `SectionStack` and `KeyedObservableStack` are the exceptions — they
still build a real wrapper element because their item carries its own structure
(a masonry cell, a section card). Those go on calling
`CopyStylesDefinedWithExtension`, which reads the marker attributes the fluent
helpers set (`tss-stk-w`, `tss-stk-h`, `tss-grd-c`, …) and moves the matching
CSS property from the inner element onto the wrapper. That copy is a no-op when
the source and target are the same element, which is the ordinary Stack/Grid case.

Stack and Grid used to wrap every child in an item div too. It was between a
quarter and a third of every node in a component-heavy page, and it hid a class
of bug: because the size landed on the wrapper and the component was stretched to
fill it, a component's own `min-width`/`min-height` could silently beat an
explicit `.Width()`/`.Height()`. `SetWidth`/`SetHeight` now clear that intrinsic
floor (unless `.MinWidth()`/`.MinHeight()` was asked for), so an explicit size
wins — see the note on `ExplicitMinWidth` in [Stack.cs](Tesserae/src/Components/Stack.cs).

A component can still take charge of its own styling by implementing
`ISpecialCaseStyling` and exposing a `StylingContainer` — the sizing helpers then
write onto that container. This is how nested containers (e.g. a `Grid` inside a
`Stack`) route sizing to the right element.

**Debugging tip:** if `.WS()` "doesn't work", inspect the rendered DOM — the
sizing styles are on the element you called the helper on, unless the component
is inside a Masonry/SectionStack, where they are on its wrapper.

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

## Testing

Playwright scripts under `Tesserae.Tests/playwright/` are local-only — use them
to verify components in the browser during development, but do **not** commit
them. The same applies to any screenshots or other artifacts produced by those
runs.

The committed harness lives in `Tesserae.Bench/` instead: a ten-page app shaped
like a real product, plus the Playwright scripts that measure its build cost and
prove a change did not alter what renders. Use it whenever you touch `Stack`,
`Grid`, the sizing extensions or anything under `Tesserae/tps/assets/css` — the
sample gallery is the real surface, and `textdiff-samples.js` is what tells you
whether it still renders the same. See
[`Tesserae.Bench/README.md`](Tesserae.Bench/README.md) and the
`tesserae-benchmarking` skill. One-off probe scripts go in
`Tesserae.Bench/playwright/_*.js`, which is gitignored.
