# Tesserae.Themes.Kontrast

An accessibility-first [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme, drawn like a public
wayfinding system: the signs in a station or an airport that have to be read by everyone, at a glance, at a
distance, in poor light. Light mode is black ink on white board. Dark mode is white on black with one signal
yellow. It is also a test instrument: every text pair it sets is at least 7:1 (WCAG AAA), every control edge
and state at least 3:1, focus is always visible, and no state is carried by colour alone.

```csharp
using Tesserae;
using Tesserae.Themes.Kontrast;

await UI.Theme.SetCustomTheme(KontrastTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                        // back to the default look
```

The theme adds `tss-theme-kontrast` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-kontrast.css`) and the fonts
(`assets/fonts/kontrast/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **Atkinson Hyperlegible Next** (text; designed for low-vision readers, every letter
distinct from its look-alikes) and **Atkinson Hyperlegible Mono** (code).

## The design rules

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each is for, in the header of
`tss.common.css`; `tss.button.css`, `tss.textbox.css`, `tss.checkbox.css`, `tss.toggle.css`, `tss.sidebar.css`,
`tss.pivot.css` and `tss.banner.css` are the worked examples.

1. **Contrast is a number, not a feeling.** Text is `--tss-kt-ink` (21:1) or `--tss-kt-muted` (8.9:1 light,
   11:1 dark), never lighter. Status text is `--tss-kt-danger` / `--tss-kt-success` (7:1 or better on the page).
   Yellow is never text on white: in light, warning is ink type on a yellow plate. Placeholders are italic and at
   7:1. Disabled text stays at AA (`--tss-kt-disabled`); disabled is shown by a dashed edge and the
   `--tss-kt-hatch` pattern, never by fading.
2. **One accent.** `--tss-kt-accent` is ink in light and signal yellow in dark, with `--tss-kt-on-accent` on it.
   It marks the primary action, the checked / chosen / current thing, and the travelled part of a track.
   `--tss-kt-signal` (yellow, fixed) is for selection, marks, the inner band of the focus ring and caution.
3. **No hairlines.** Every edge is `--tss-kt-border` (2px); a focused field or a floating layer
   `--tss-kt-border-strong` (3px). A passive divider is `--tss-kt-line` (7:1); a control's edge is
   `--tss-kt-edge` (the ink). Corners are 4px (`--tss-kt-radius`), chips 2px; only toggles are pills.
4. **State is a shape.** Selected, current, checked, invalid and active each change something besides colour:
   weight (bold 750), a 6px bar (`--tss-kt-bar`) at the leading edge or foot, a fill that reverses the plate, a
   tick, a dot, an underline. Hover adds a fill and an ink edge. Pressed adds a 3px inner rule along the bottom.
5. **Focus is never removed.** `:focus-visible` draws a two-tone ring: a 3px inner band
   (`--tss-kt-focus-inner`) and a 3px outer band (`--tss-kt-focus-outer`), yellow then black in light, black then
   yellow in dark, so one band always shows. Anything that sits in a clipping scroller (a row, a tab, a sidebar
   entry, a field) draws the same ring inside its own edge instead. A control whose focus lands on a hidden
   input (checkbox, radio, toggle, slider) draws the ring on the visible mark from the container.
6. **Links are underlined.** Always, 2px, 4px under the pointer, in `--tss-kt-link` (deep blue in light, yellow in
   dark). A link-styled button is underlined too; a button that is not a link is never underlined at rest.
7. **Targets are big enough.** Buttons are 40px tall (28px small), fields 40px, rows 36px, checkboxes and radios
   22px with a 24px hit area, small remove buttons 20 to 24px.
8. **Type is larger and looser.** Body text 15px (`--tss-font-size-small`) at line height 1.5, the scale one step
   up throughout, labels bold, long-form text capped near 75 characters.
9. **Floating things are slabs.** Menus, popups, dropdowns, toasts and modals are paper with a 2px (modal 3px)
   ink edge and a hard 6px offset shadow (`--tss-kt-float-shadow`); no blur, no soft shadow anywhere. Surfaces
   that sit in the page use a 2px edge instead of a shadow.
10. **Motion is reduced.** State changes take 60ms; nothing slides, zooms or bounces. Under
    `prefers-reduced-motion: reduce` transitions are cut and only spinners keep turning, slowly.
11. **Colour is a bonus.** Status keeps its icon and its word; warning carries black-and-yellow caution stripes;
    chart lines after the first are dashed; rating stars are hollow or solid; presence lamps have a shape per
    state.
12. **The system wins when it should.** Under `@media (forced-colors: active)` the theme gives up its palette and
    makes sure what it drew with fills and shadows is still there as a system-colour border or outline
    (`Highlight`, `CanvasText`, `LinkText`, `GrayText`). Under `@media (prefers-contrast: more)` passive lines and
    secondary text turn to ink and edges go to 3px.

Every selector starts with `body.tss-theme-kontrast` (dark mode: `body.tss-theme-kontrast.tss-dark-mode`) so the
theme is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`).
A theme file restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

A 12px gutter with a 2px rule for the track and a square thumb: mid grey at rest (3:1), ink while the pointer is
over the scroller, the link blue (yellow in dark) under the pointer and while dragged. CSS only
(`::-webkit-scrollbar*`, and `scrollbar-color` for Firefox, which stops at ink): no script watches or draws a bar.
