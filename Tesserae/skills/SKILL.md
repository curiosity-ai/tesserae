---
name: tesserae
description: Build web UIs in C# with the Tesserae toolkit (compiled to JavaScript by the Transpose compiler) — components, fluent configuration, layout with Stack/Grid, and sizing/spacing helpers. Use when writing or editing a Tesserae app, picking a component, laying out a UI, or looking up a component's API. Per-component references live in references/.
---

# Tesserae

Tesserae is a C# UI toolkit for building web applications. You write strongly-typed
C#; the **Transpose** compiler translates it to JavaScript that runs in the browser. The
API is fluent and component-based, loosely inspired by Microsoft's Fluent UI.

Bring the factories and DOM helpers into scope at the top of a file:

```csharp
using static Tesserae.UI;     // component factories: Button(), Stack(), TextBlock()…
using static Transpose.Core.dom;     // browser globals: document, window, console…
```

## Using components

Every component implements `IComponent` (one method, `HTMLElement Render()`).
You create components with `UI.` factory methods, configure them with fluent
`this`-returning helpers, compose them inside containers, and mount a root
component to the page.

```csharp
private static void Main()
{
    var ui = Stack().Children(
        TextBlock("Hello").Large().SemiBold(),
        Button("Click me").Primary().OnClick((_, __) => alert("clicked"))
    );

    document.body.style.overflow = "hidden";
    MountCenteredToBody(ui);          // or MountToBody(ui)
}
```

- **Create**: `Button("Save")`, `TextBox()`, `Stack()`, … (some components are also
  `new`-ed, e.g. `new Modal(...)`).
- **Configure**: chain fluent methods — `.Primary()`, `.Disabled()`, `.OnClick(...)`.
- **Compose**: containers take children via `.Children(...)` or `.Add(...)`.
- **Mount**: `MountToBody` / `MountCenteredToBody` attach the root to the DOM.

## Layout: Stack and Grid

`Stack` and `Grid` are the two workhorse containers. Most layouts are nested
stacks.

**Stack** — a flexbox container, vertical by default. Use `HStack()`/`VStack()`
for explicit orientation.

```csharp
HStack()                          // horizontal
    .AlignItemsCenter()           // center children on the cross axis
    .Gap(8.px())                  // space between children
    .Children(
        Icon(UIcons.User),
        TextBlock("Profile").Grow(),   // claims leftover main-axis space
        Button("Edit")
    );
```

Key `Stack` methods: `Horizontal()` / `Vertical()` (and `…Reverse()`),
`Wrap()` / `NoWrap()`, `Gap(size)`, `AlignItems(ItemAlign)` /
`AlignItemsCenter()`, `JustifyContent(ItemJustify)`, `AlignContent(...)`.

**Grid** — a CSS-Grid container with explicit tracks. Place children with the
`.GridColumn(...)` / `.GridRow(...)` extensions (call them before `.Add`).

```csharp
Grid()
    .Columns(200.px(), 1.fr())    // two columns: fixed sidebar + flexible body
    .Rows(auto, 1.fr())
    .Gap(12.px())
    .AlignItemsCenter();
```

Key `Grid` methods: `Columns(...)`, `Rows(...)`, `Gap`/`RowGap`/`ColumnGap`,
`AutoRows`/`AutoColumn`, `FlowColumn()`, `AlignItems(...)`, `Relative()`.

See `references/stack.md`, `references/grid.md`, and `references/layout-alignment.md`
for the full story, and `references/split-view.md` / `references/masonry.md` /
`references/float.md` for the other layout containers.

## Sizing, spacing, and alignment (extension methods on any component)

These fluent helpers work on **any** `IComponent` (they are generic extensions,
not per-component members). Call them before adding the component to a container.

| Concern | Methods |
| --- | --- |
| Width | `.Width(size)` / `.W(size)`, `.MinWidth`, `.MaxWidth`, `.WidthStretch()` / `.WS()` (100%) |
| Height | `.Height(size)` / `.H(size)`, `.MinHeight`, `.MaxHeight`, `.HeightStretch()` / `.HS()` (100%) |
| Both | `.Stretch()` / `.S()` (width + height 100%) |
| Flex grow/shrink | `.Grow(int = 1)`, `.Shrink()`, `.NoShrink()` |
| Margin | `.Margin(size)` / `.M(size)`, `.ML` `.MR` `.MT` `.MB` (left/right/top/bottom) |
| Padding | `.Padding(size)` / `.P(size)`, `.PL` `.PR` `.PT` `.PB` |
| Self-alignment | `.AlignStart()`, `.AlignCenter()`, `.AlignEnd()`, `.AlignStretch()`, `.JustifyStart/Center/End()` |

Sizes are `UnitSize` values from numeric helpers: `100.px()`, `50.percent()`,
`1.em()`, `1.fr()`.

> **Container-level vs. item-level alignment.** `AlignItemsCenter()` is a method on
> `Stack`/`Grid` that centers *all* children. `.AlignCenter()` is an extension on a
> single child (align-self). `.Grow()` only matters inside a `Stack`.

> **Wrap-and-transfer:** when you call `.WS()`/`.W()`/`.Grow()` on a child, the
> container moves those styles onto the item wrapper (`tss-stack-item`) it creates.
> If a sizing helper "doesn't work", inspect the DOM — the style usually landed on
> the wrapper, not the element you called it on. Full catalog in
> `references/icomponent.md`; the protocol is in this repo's `CLAUDE.md`.

## Picking a layout

- One-axis flow (toolbar, sidebar, form) → `Stack` (`references/stack.md`).
- Two-axis grid with named tracks → `Grid` (`references/grid.md`).
- Two resizable panes → `SplitView` / `HorizontalSplitView`.
- Pinned overlay on a parent → `Float`.
- Variable-height tile feed → `Masonry`.
- Modal / popover that must escape clipping → `Layer` (usually via `Dialog`,
  `Modal`, `ContextMenu`).

## Reference library — `references/`

Detailed, per-topic docs live in `references/`, one file per component or topic,
named by its kebab-case slug: **`references/<slug>.md`** (e.g.
`references/dropdown.md`, `references/details-list.md`). Each reference has the
factory signature, the key fluent methods, an example, and links to related
references. Open the reference for whatever you are working with. The full set:

**Concepts & extending** (read these to understand the model)
- `references/core-concepts.md` — IComponent, fluent API, layout, reactive state.
- `references/icomponent.md` — the `IComponent` interface and **every** sizing/
  spacing/styling/event extension method.
- `references/styling.md`, `references/layout-alignment.md`,
  `references/custom-styles.md`, `references/colors.md`,
  `references/theme-colors.md`, `references/iconography.md`,
  `references/accessibility.md`, `references/project-setup.md`,
  `references/routing.md`.
- `references/creating-a-component.md` — build your own `IComponent`.
- `references/javascript-interop.md` — call JS/browser APIs from C# via Transpose.
- `references/wrap-a-javascript-library.md` — wrap a third-party JS library.

**Components** (plain widgets)
accordion · action-button · annotated-text-editor · avatar · background-area ·
badge · button · card · carousel · charts · chat · check-box · choice-group ·
color-picker · command-bar · contribution-bar · cron-editor · date-picker ·
date-range-picker · date-time-picker · delta-component · dropdown ·
editable-area · editable-label · expander · grid-picker · horizontal-separator ·
icon · icon-toggle · image · label · link · markdown-block · menu · message ·
metric · month-picker · navbar · number-picker · omni-box · option · pagination ·
picker · plan · popover · progress-indicator · progress-ring · property-grid ·
rating · resource-card · sandbox · save-button · saving-toast · search-box ·
section-title · sidebar · sidebar-separator · sidenav · skeleton · slider ·
sparkline · spinner · stepper · steps-slider · tags-input · task-board ·
teaching · text-area · text-block · text-box · text-breadcrumbs ·
time-histogram-picker · time-picker · toggle · toggle-button · tool-call · tree ·
uptime · visibility-sensor · week-picker

**Collections** (list-like / layout containers)
breadcrumb · details-list · grid · infinite-scrolling-list · items-list ·
masonry · observable-stack · observables · overflow-set ·
searchable-grouped-list · searchable-list · sortable-stack · stack · timeline ·
virtualized-list

**Surfaces** (overlays, tabs, panels)
card-pivot · context-menu · dialog · float · horizontal-split-view · layer ·
modal · panel · pivot · pivot-selector · progress-modal · section-stack ·
segmented-pivot · split-view · tabbed-modal · tool-agent-selector · tutorial-modal

**Utilities** (non-visual helpers, theming, gestures)
color-palette · command-palette · defer · defer-with-progress · emoji ·
file-selector-and-drop-area · gestures · gradients · keyboard-shortcut ·
node-view · notification-center · tippy · toast · uicons · validator

To find the reference for a component, lowercase-kebab its name and open
`references/<that>.md` (e.g. `DetailsList` → `references/details-list.md`). If you
are unsure which component fits, scan the lists above by category, then open the
candidate's reference.
