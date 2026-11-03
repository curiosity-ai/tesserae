---
name: ui-design
description: >-
  The UI design rules for Curiosity: Curiosity Studio (Mosaik), the Tesserae
  components and gallery, the Tesserae Themes design system and its mockups.
  No explanation text, a clean and functional look, controls that explain
  themselves, patterns from the Curiosity demos and well-known products, and
  Tesserae and Mosaik components reused before anything new is built. Use
  before designing, building or reviewing any screen, mockup, dialog, panel,
  settings page, empty state, sample or component, and whenever asked to
  "design", "build a page", "make a mockup", "add a view", "improve the UX",
  "make this look better" or "clean up this UI".
---

# UI design rules

Read this before you design or change any UI: a product screen, a Tesserae
component or sample, a design-system page recipe or a mockup.

This skill is the one place for style and UI design rules. It is kept in
`curiosity-ai/design-systems` (the source) and copied unchanged to
`curiosity-ai/mosaik` and `curiosity-ai/tesserae`. Change all three copies
together. Do not copy it into the application templates: they hold code
instructions, not design rules.

## 1. No explanation sentences in the UI

- A label is a noun or a short verb phrase: "Schedule", "Run now", "Owner".
  Not "Use this to choose when the task runs".
- No paragraph under a heading that says what the page is for. The title and
  the content say that.
- If a control needs a sentence to be understood, change the control first
  (rule 3).
- Text that stays is short and specific: a value, a unit, a count, a status, an
  error that says what to do next. One line, no "Note:", no "Please".
- Fewer words also means fewer `.t()` strings to translate into twenty
  languages.

### Description lines are a last resort

A one-line description under a setting or a section is sometimes correct, but
only when all of these are true:

1. A better control, a clearer label, a placeholder with a real example, a unit,
   a default value or a grouping cannot carry the meaning. You tried them.
2. Without the line, a person is likely to make a choice that costs them
   something: lost data, a long rebuild, a security or access change, money.
3. It is one short line, and it says the consequence, not how the control
   works: "Rebuilds the index. Takes hours on large workspaces." Not "Use this
   toggle to enable or disable reindexing."

When you add one, say in the PR description why rule 3 could not solve it. A
reviewer removes any description line that has no justification.

Other one-line exceptions: an error, a confirmation of a destructive action
(what will be lost), and an empty state (what to do first, with a button).

## 2. Clean

- One primary action per view. Other actions are secondary buttons, icon
  buttons or an overflow menu.
- Align to the 4px grid and use the spacing, size and colour tokens. No ad-hoc
  pixel values, no literal colours.
- No box inside a box inside a box. Separate with whitespace first, a hairline
  second, a card last.
- Show the common path. Put rare options behind an `Expander`, an "Advanced"
  section, a `ContextMenu` or a details panel.
- The same thing looks and behaves the same on every page.

## 3. Functional: let the design explain the function

A control that shows its own meaning is better than text or a tooltip that
explains it.

| Instead of | Use |
|---|---|
| A sentence that explains the options | A `ChoiceGroup`, `SegmentedPivot`, `Dropdown` or `Pivot` whose labels are the options |
| "Only items of type X are shown" | A visible filter chip or facet that the person can see and remove |
| "This can take a few minutes" | `LiveProgress`, `ProgressIndicator` or a `Skeleton` |
| "Enter a cron expression, e.g. ..." | `CronEditor`, a picker, or a placeholder that is a real example |
| "Click here to add your first item" | An empty state with one primary button |
| A tooltip on a disabled button | Hide the action, or show the reason as state (a badge, a lock icon) |
| A legend paragraph | Colour, icon and grouping that match the data, with a compact legend |
| A status sentence | A `Badge`, a coloured dot with a word, a count |

Tooltips are for two things only: the name of an icon-only control, and the
full value of a truncated text. A tooltip is never the only place where a
function is explained.

Show data first. Every element earns its place: if removing it changes nothing
for the person, remove it.

## 4. Copy proven patterns, and look for them aggressively

Before you design a screen, find how others already solved the same problem,
and follow the pattern people already know.

**First, our own demos.** They are complete applications built on Tesserae and
Mosaik, so their screens are references you can copy component by component:

| Repository | What to look at |
|---|---|
| `curiosity-ai/workspace-demo` | the default workspace |
| `curiosity-ai/automotive-demo` | dashboards, investigations, a findings board (Field Quality) |
| `curiosity-ai/tech-data-demo` | document search and node renderers for technical data |
| `curiosity-ai/aviation-intelligence-demo` | news monitoring: home dashboard, news and feeds, reports, chat |
| `curiosity-ai/demos-template` | the starting point every demo shares |

The Tesserae Themes design system shows several of these screens as "Pages"
recipes (`tesserae/capture/src/Recipes.Pages*.cs` in design-systems), each one
linked to its source file. Start a new screen from the closest one. Attach a
demo repository with `add_repo` when it is not in the session.

**Then, well-known products.** Use `WebSearch` / `WebFetch` for every new
screen or larger change, not only when unsure. Search for the pattern ("filter
bar design", "settings page layout", "command palette", "empty state
examples", "data table bulk actions"). Good references: Linear, GitHub, Notion,
Figma, Vercel, Stripe Dashboard, VS Code, Raycast, Apple Human Interface
Guidelines, Material Design, and pattern galleries such as Mobbin and Page
Flows.

- Prefer the pattern that two or more references share. Do not invent a new
  interaction for a problem that has a standard answer.
- Name the references you followed in the PR description, with links.

## 5. Reuse components aggressively

Look for an existing component before you build anything, and compose from it.
In this order:

1. **Mosaik, as it is.** `SearchArea` with facets for any list or search, a
   node renderer (`NodeRendererBase`) for anything that shows a record,
   `AppSettings` hooks for the home page, sidebar and chat, and the shared
   helpers in `FrontEnd/Mosaik.FrontEnd.Core/src/Components/UI.cs`,
   `FrontEnd/Mosaik.FrontEnd/src/Components/` and
   `FrontEnd/Mosaik.FrontEnd.Admin/src/Components/` (`ProgressLogModal`,
   `CodeConflicts`, `ModelDownloadRow`, `EmptyStateGlyph`, ...).
2. **Tesserae, with its fluent options.** The `tesserae` skill indexes every
   component (`Tesserae/skills/references/` in tesserae, `.claude/skills/tesserae/`
   in an app that uses the package). The gallery and the design system show
   each one live, in every theme.
3. **A composition** of the above with `VStack()` / `HStack()` / `Grid()`.
4. Fluent helpers and theme tokens for style, never raw DOM styling or
   `.Style(...)` for spacing, size, colour or font.

Do not rebuild a list, table, picker, dialog, toolbar or empty state by hand
when a component exists. If a component almost fits, extend it with a fluent
option rather than fork it.

## 6. A new component is fine when it is clearly better UX

If nothing fits and a new component gives a clearly better experience, propose
it:

1. Write the use case, the components you tried, and why they do not fit.
2. Show the closest composition beside the proposed component, in at least two
   themes, light and dark.
3. Wait for the person's approval, then build it in the right place:
   - generic (useful to any Tesserae app): in tesserae, with a factory in
     `UI.Components.cs`, a gallery sample, a skill reference and theme files;
     then its recipe in design-systems;
   - product-specific: under the matching `FrontEnd/*/src/Components/` folder in
     mosaik.

## Checklist before you finish

- [ ] No sentence in the UI that only explains how something works.
- [ ] Each description line that is left is justified in the PR.
- [ ] One primary action; rare options are out of the way.
- [ ] Filters, selectors and state show the function instead of text or tooltips.
- [ ] Tooltips only name icon-only controls or show truncated values.
- [ ] The layout follows a demo screen or a well-known product, named in the PR.
- [ ] Built from existing Mosaik and Tesserae components; a new component was approved.
- [ ] Checked in light and dark, and in more than one theme.
