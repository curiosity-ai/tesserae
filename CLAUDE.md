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

The UIcons webfonts and the `UIcons` enum are generated, not hand-written. Two
console projects own them, and both are in the solution:

- **`Build.UpdateInterfaceIcons`** — downloads the woff2/css for every UIcons
  weight from Flaticon into `Tesserae/tps/assets/`, and regenerates
  `Tesserae/src/Icons/UIcons.cs`. Run it from its own folder (it checks the
  working directory). Bumping the icon set is this project's job.
- **`Build.UIconsOpticalCentering`** — renders every glyph of every bundled
  weight in headless Chromium (Playwright), measures how far each one is from
  being optically centred in the box the browser lays it out in, and writes
  `Tesserae/tps/assets/css/tss.uicons.adjustments.css`. That file is generated
  output — regenerate it, never edit it:

  ```bash
  dotnet run --project Build.UIconsOpticalCentering            # rewrite the stylesheet
  dotnet run --project Build.UIconsOpticalCentering -- --help  # tuning knobs
  ```

  It reads the `uicons-*.css` files for the codepoints, so **run it after
  `Build.UpdateInterfaceIcons`** whenever the icon set is bumped — the
  codepoints change with every UIcons release, and stale adjustments would land
  on the wrong glyphs. It exits non-zero if its own checks fail (icons that must
  overlap drifting apart, generated selectors not matching real icon markup), so
  it is safe to wire into CI.

  `--preview` also writes annotated before/after screenshots under the project's
  `bin/.../preview/`. Those are local artefacts; don't commit them.

  Known limitation, measured: a `position: relative` offset is rounded to a whole
  CSS pixel when the icon is painted, so an offset only takes effect once it
  reaches half a pixel. At `TextSize.Small` (13px, the `Icon()` default) that is
  6% of the emitted offsets; at 24px it is 78%. The generated file declares each
  offset twice — the em value, then `round(<value>, 1px)` — so the pixel is
  chosen from the font size rather than by paint-time snapping, which otherwise
  makes the same icon shift in one container and not in another depending on
  where it lands on the pixel grid. Baking the offsets into the glyph outlines
  instead would survive at sub-pixel sizes and remove this stylesheet entirely;
  that would have to happen inside `Build.UpdateInterfaceIcons`, right after the
  woff2 files are downloaded.

  The adjustments are deliberately conservative: only offsets between the dead
  zone and the cap are emitted, so an icon that is a long way off centre (a half
  circle, an empty crate drawn at the bottom of its box) is left as drawn rather
  than half-corrected.

  Icons that compose with each other must stay registered, and three mechanisms
  keep them that way — in increasing order of priority:

  - **Lookalikes** are pinned to one shared offset when they have matching ink
    boxes *and* already agree on where their centre is, so rounding cannot
    separate them. Agreement is what identifies a lookalike: thousands of icons
    are drawn edge to edge and so share an ink box without being related
    (`circle` and `square` have identical ink boxes), and pinning those to each
    other would drag well-centred icons off centre.
  - **State variants** — `X-slash`, `X-crossed`, `X-off`, `X-mute`,
    `X-muted`, `X-disabled` — take the offset of the `X` they are a state of,
    since a UI swaps one for the other in place. 464 such pairs exist and 93 of
    them disagreed before the rule.
  - **Frame families** — icons sharing an ink box *and* a shape word
    (`square`, `circle`, `rectangle`, `hexagon`, `octagon`, `diamond`,
    `triangle`) are drawn on the same frame, so if they cannot agree on one
    offset none of them is moved. The name is essential here: the ink box alone
    cannot tell a circle-framed icon from any other icon that fills its box.

  On top of those, `AlignmentGroups` names the handful a rule cannot derive,
  because their names have nothing in common: the `square`/`checkbox`/`square-a`
  set the toolkit swaps through `--uicon-var-*`, `toggle-on`/`toggle-off`,
  `lock`/`unlock`/`lock-open-alt`, the mirrored pairs, and `slash`, which is
  composited over other icons and so is never moved at all.

  The run enforces the outcome: no icon may end up further off centre than
  rounding explains, the only exceptions being the icons that deliberately give
  up their own centering to stay registered with another one.

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

`Stack.CopyStylesDefinedWithExtension` ([Stack.cs](Tesserae/src/Components/Stack.cs))
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

## Testing

Playwright scripts under `Tesserae.Tests/playwright/` are local-only — use them
to verify components in the browser during development, but do **not** commit
them. The same applies to any screenshots or other artifacts produced by those
runs.
