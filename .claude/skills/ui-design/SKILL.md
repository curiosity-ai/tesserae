---
name: ui-design
description: >-
  The UI design rules for Curiosity Studio and the Tesserae gallery: no
  explanation text, a clean and functional look, controls that explain
  themselves, patterns copied from well-known products, and Tesserae and Mosaik
  components reused before anything new is built. Use before designing,
  building or reviewing any screen, dialog, panel, settings page, empty state,
  sample or component, and whenever asked to "design", "build a page", "add a
  view", "improve the UX", "make this look better" or "clean up this UI".
---

# UI design rules

Read this before you design or change any UI. The rules apply to the product
(`FrontEnd/` in mosaik), to Tesserae components and to the gallery samples.

## 1. No explanation sentences in the UI

- A label is a noun or a short verb phrase: "Schedule", "Run now", "Owner".
  Not "Use this to choose when the task runs".
- Do not add a paragraph under a heading that says what the page is for. The
  page title and its content say that.
- Do not add a description line under every setting. If a setting needs a
  sentence to be understood, change the control (see rule 3) before you add the
  sentence.
- Text that stays is short and specific: a value, a unit, a count, a status, an
  error that says what to do next. One line, no "Note:", no "Please".
- Allowed exceptions, each one line at most: a destructive or irreversible
  action (what will be lost), an error, and an empty state (what to do first).
- Fewer words also means fewer `.t()` strings to translate into twenty
  languages.

## 2. Clean

- One primary action per view. Other actions are secondary buttons, icon
  buttons or an overflow menu.
- Align to one grid and use the spacing, size and colour tokens. No ad-hoc
  pixel values, no inline colours.
- No box inside a box inside a box. Separate with whitespace first, a hairline
  second, a card last.
- Show the common path. Put rare options behind an `Expander`, an "Advanced"
  section, a `ContextMenu` or a details panel.
- Consistent: the same thing looks and behaves the same on every page.

## 3. Functional: let the design explain the function

A control that shows its own meaning is better than text or a tooltip that
explains it. Prefer, in this order:

| Instead of | Use |
|---|---|
| A sentence that explains the options | A `ChoiceGroup`, `SegmentedPivot`, `Dropdown` or `Pivot` whose labels are the options |
| "Only items of type X are shown" | A visible filter chip or facet the user can see and remove |
| "This can take a few minutes" | A `LiveProgress`, `ProgressIndicator` or `Skeleton` |
| "Enter a cron expression, e.g. ..." | `CronEditor`, a picker, or a placeholder that is a real example |
| "Click here to add your first item" | An empty state with one primary button |
| A tooltip on a disabled button | Hide the action, or show the reason as state (a badge, a lock icon) |
| A legend paragraph | Colour, icon and grouping that match the data, with a compact legend |
| A status sentence | A `Badge`, a coloured dot, a count |

Tooltips are for two things only: the name of an icon-only control, and the full
value of a truncated text. They are never the only place where a function is
explained.

Show data first. Every element earns its place: if removing it changes nothing
for the user, remove it.

## 4. Copy proven patterns, and look for them aggressively

Before you design a screen, search for how well-known products solve the same
problem, and follow the pattern users already know.

- Use `WebSearch` / `WebFetch` for every new screen or larger change, not only
  when unsure. Search for the pattern ("filter bar design", "settings page
  layout", "command palette", "empty state examples", "data table bulk
  actions").
- Good references: Linear, GitHub, Notion, Figma, Vercel, Stripe Dashboard,
  VS Code, Raycast, Apple Human Interface Guidelines, Material Design, and
  pattern galleries such as Mobbin, Page Flows and UI Patterns.
- Prefer the pattern that two or more references share. Do not invent a new
  interaction for a problem that has a standard answer.
- Name the references you followed in the PR description, with a link.

## 5. Reuse components aggressively

Look for an existing component before you build anything, and compose from it.

1. **Tesserae.** The `tesserae` skill indexes every component
   (`.claude/skills/tesserae/` in mosaik, `Tesserae/skills/SKILL.md` and
   `Tesserae/skills/references/` in tesserae). The gallery
   (`Tesserae.Tests/src/Samples/`) shows each one live.
2. **Mosaik.** `Mosaik.Settings.UI`
   (`FrontEnd/Mosaik.FrontEnd.Core/src/Components/UI.cs`, e.g.
   `SettingsLabel`), `Mosaik.UI`, `FrontEnd/Mosaik.FrontEnd/src/Components/`,
   `FrontEnd/Mosaik.FrontEnd.Admin/src/Components/` and the shared helpers
   (`ProgressLogModal`, `CodeConflicts`, `ModelDownloadRow`, `EmptyStateGlyph`,
   ...). Grep for a similar screen and copy its structure.
3. Use the fluent helpers and theme tokens, not raw DOM styling (see "Styling
   components" in `mosaik/.claude/CLAUDE.md`).

Do not rebuild a list, table, picker, dialog, toolbar or empty state by hand
when a component exists. If a component almost fits, extend it with a fluent
option rather than fork it.

## 6. A new component is fine when it is clearly better UX

If no component fits and a new one gives a clearly better experience, add it.

- Generic (useful to any Tesserae app): add it to Tesserae, with a factory in
  `UI.Components.cs`, a gallery sample, a skill reference and the theme files
  (see "Adding a component" in `tesserae/CLAUDE.md`).
- Product-specific: add it under the matching `FrontEnd/*/src/Components/`
  folder in mosaik.
- Say in the PR description why no existing component was enough.

## Checklist before you finish

- [ ] No sentence in the UI that only explains how something works.
- [ ] Each remaining text is one short line and earns its place.
- [ ] One primary action; rare options are out of the way.
- [ ] Filters, selectors and state show the function instead of text or tooltips.
- [ ] Tooltips only name icon-only controls or show truncated values.
- [ ] The layout follows a pattern from at least one well-known product, named in the PR.
- [ ] Built from existing Tesserae and Mosaik components; any new component is justified.
- [ ] Checked in light and dark, and in more than one theme.
