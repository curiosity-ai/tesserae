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

> **Reaching a nested type.** `using static Tesserae.UI;` imports a factory *method* for
> almost every component, and a method hides a same-named type. So inside your own
> namespace `Float.Position.TopLeft`, `Dialog.Response.Yes`, `OmniBox.Config`,
> `SaveButton.State`, `Teaching.StepType` and friends do not compile
> (`CS0119: … is a method, which is not valid in the given context`) — qualify them:
> `Tesserae.Float.Position.TopLeft`. The same applies to statics such as
> `Tesserae.KeyboardShortcut.Matches(...)` and `Tesserae.Icon.Transform(...)`.

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
    .Rows(new[] { UnitSize.Auto(), 1.fr() })
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
`1.fr()`, `100.vw()`, `100.vh()` — plus `UnitSize.Auto()`, `UnitSize.FitContent()`
and raw `new UnitSize("calc(100% - 32px)")` for anything else.

> **Container-level vs. item-level alignment.** `AlignItemsCenter()` is a method on
> `Stack`/`Grid` that centers *all* children. `.AlignCenter()` is an extension on a
> single child (align-self). `.Grow()` only matters inside a `Stack`.

> **Where sizing lands:** `.WS()`/`.W()`/`.Grow()` write to the child's own element,
> which is the flex/grid item the container measures (it is tagged `tss-stack-item`).
> An explicit `.Width()`/`.Height()` also clears the component's own intrinsic
> `min-width`/`min-height`, so the size you asked for wins; ask for `.MinWidth()`/
> `.MinHeight()` if you want a floor as well. Full catalog in
> `references/icomponent.md`.

> **What stretches by default.** A stack stretches its children across the cross
> axis, so a `TextBox`, `Card` or nested `Stack` fills the width of a vertical
> stack without being asked. Components that are inline by nature — `Button`,
> `Toggle`, `Avatar`, `Icon`, `Rating` and friends — hug their content instead;
> call `.WS()` on one to make it fill the row.

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
- `references/observables.md` — the reactive state containers `Defer` and the
  collection components read from.
- `references/creating-a-component.md` — build your own `IComponent`.
- `references/javascript-interop.md` — call JS/browser APIs from C# via Transpose.
- `references/wrap-a-javascript-library.md` — wrap a third-party JS library.

**Layout** — containers you build a page out of
accordion · background-area · card · expander · grid · horizontal-separator ·
horizontal-split-view · masonry · section-stack · split-view · stack

**Text & Content** — text, labels and rich content blocks
badge · icon-tile · inline-label · keyboard-shortcut · label · markdown-block
· section-title · text-block

**Buttons & Commands** — things the user clicks to do something
action-button · button · command-bar · command-palette · context-menu ·
icon-toggle · menu · overflow-set · toggle-button

**Inputs** — form controls that capture a value
annotated-text-editor · check-box · choice-group · color-picker · dropdown ·
editable-area · editable-label · file-selector-and-drop-area · grid-picker ·
number-picker · option · picker · rating · slider · steps-slider · tags-input
· text-area · text-box · toggle

**Date & Time** — calendar, clock and schedule pickers
cron-editor · date-picker · date-range-picker · date-time-picker ·
month-picker · time-histogram-picker · time-picker · week-picker

**Forms & Validation** — binding a form to data, validating and saving it
property-grid · save-button · saving-toast · unsaved-changes-guard · validator

**Navigation** — moving between pages, sections and tabs
breadcrumb · card-pivot · inline-pagination · navbar · pagination · pivot ·
pivot-selector · segmented-pivot · sidebar · sidebar-separator · sidenav ·
stepper · text-breadcrumbs

**Lists & Data** — rendering a collection of items
details-grid · details-list · infinite-scrolling-list · items-list ·
observable-stack · sortable-stack · task-board · timeline · tree ·
virtualized-list

**Search** — search inputs and their result surfaces
omni-box · omni-result · search-box · searchable-grouped-list ·
searchable-list

**Charts & Visualization** — numbers and relationships, drawn
charts · contribution-bar · metric · node-view · sparkline · uptime

**Feedback & Status** — progress, notifications and empty states
banner · live-progress · message · notification-center · progress-indicator ·
progress-modal · progress-ring · skeleton · spinner · tippy · toast

**Overlays & Dialogs** — surfaces that float above the page
dialog · float · layer · modal · modal-stack · panel · popover · shortcut-guide
· tabbed-modal · teaching · tutorial-modal

**AI & Chat** — conversation, tool calls and their context
ai-variants · chat · context-card · context-cards · plan · resource-card ·
tool-agent-selector · tool-call

**Media & Graphics** — images, avatars and embedded content
avatar · carousel · image · pages-stack · pixel-avatar · sandbox

**Theming & Icons** — colours, gradients, icons and emoji
color-palette · emoji · gradients · icon · uicons

**Utilities & Behaviors** — helpers that render little or nothing on their own
defer · defer-with-progress · delta-component · gestures · visibility-sensor

To find the reference for a component, lowercase-kebab its name and open
`references/<that>.md` (e.g. `DetailsList` → `references/details-list.md`). If you
are unsure which component fits, find the category above that matches what you are
trying to do, then open the candidate's reference. The categories are the same ones
the sample gallery and the online documentation use, so a component sits in the same
place in all three.
