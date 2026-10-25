# Tesserae.Themes.Rechenwerk

A [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the record sleeves of Kraftwerk, the
Düsseldorf pioneers of electronic music. *Rechenwerk* is German for a computer's arithmetic unit: the part of the
machine that does the counting, in the band's own vocabulary of *Kraftwerk* (power station), *Computerwelt* and
*Taschenrechner*. (An homage: the package has no connection to the band, and uses none of its artwork, logos or
lettering.)

Two sleeves, one machine:

- **Light is *Die Mensch-Maschine* (1978).** Cream poster paper, black ink, a signal red; constructivist and loud.
- **Dark (`UI.Theme.Dark()`) is *Computerwelt* (1981).** A black screen with a faint raster, cream phosphor, and the
  yellow of the terminal on the cover.

And one build shared by both: square corners, 2px ink rules, hard offset shadows that never blur, buttons that are
calculator keys and press in, every piece of chrome in tracked capitals, headlines in a condensed grotesque, numbers
on a dot-matrix display, warnings in the black-and-yellow stripes of *Radio-Aktivität*.

```csharp
using Tesserae;
using Tesserae.Themes.Rechenwerk;

await UI.Theme.SetCustomTheme(RechenwerkTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                          // back to the default look
```

The theme adds `tss-theme-rechenwerk` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-rechenwerk.css`) and the fonts
(`assets/fonts/rechenwerk/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **Jost** (text, a geometric sans in the Futura line), **Big Shoulders Display** (headlines,
condensed and heavy), **Doto** (numerals, a dot-matrix face) and **Space Mono** (code).

## The design language

The rules every file in `tps/assets/css/` follows.

1. **Two sleeves, one set of tokens.** Write every rule in terms of the `--tss-*` roots and the `--tss-rw-*` tokens
   in `tss.common.css`, never a hex value for something that changes between modes. Ink is
   `--tss-default-foreground-color-root`, the page is `--tss-default-background-color-root`, a raised surface is
   `--tss-secondary-background-color-root`. `--tss-rw-signal` is the mode's accent (red on paper, yellow on the
   screen) and `--tss-rw-on-signal` is the type drawn on it. The fixed sleeve colours (`--tss-rw-red`,
   `--tss-rw-yellow`, `--tss-rw-blue`, `--tss-rw-green`) are for meaning that does not change with the mode.
2. **The sidebar is the terminal in both modes.** The dark tokens are restated on `.tss-sidebar`, so anything
   inside it draws itself for a black ground with no rule of its own. A component that can sit in the sidebar
   needs nothing special: use the tokens.
3. **Machine-made geometry.** No border radius, anywhere, except a dot that is meant to be round (an avatar, a
   status dot, a radio button's mark). Controls and frames are a **2px** rule in
   `--tss-default-border-color-root`; separators inside a surface are 1px in `--tss-default-separator-color-root`.
4. **Shadows are hard and offset, never blurred.** A key at rest stands on a 3px shadow (`--tss-rw-btn-shadow`); a
   panel or card on 5px (`--tss-rw-card-shadow`); anything that floats (menu, modal, popover, toast, dropdown) on
   the 2px frame plus a 6px offset (`--tss-rw-frame-shadow`). Ink on paper; yellow on the terminal.
5. **Keys press in.** A solid control moves by its shadow's offset when pressed (`transform: translate(3px, 3px)`)
   and the shadow goes. A hoverable card lifts instead.
6. **Chrome speaks in tracked capitals.** Buttons, tabs, title bars, column headers, badges, menu section titles,
   labels of fields: `text-transform: uppercase`, `letter-spacing: var(--tss-rw-tracking)`, weight 600, 11 to 12px.
   Content (what the user wrote, descriptions, list items, table cells) stays in sentence case.
7. **Three more voices.** Headlines (page, modal and panel titles, big numbers' captions) are
   `var(--tss-rw-display)`, heavy (800), capitals. Numbers that are the point (a metric, a counter, a clock, a
   progress percentage, a date in a picker's header) are `var(--tss-rw-dots)`, weight 700 or more. Code is
   `var(--tss-monospace-font-family)` (Space Mono).
8. **Selection is the terminal's cursor.** The current item (selected row, tab, menu item under the keyboard,
   chosen date, the sidebar's open page) is a solid yellow block with black type, in both modes:
   `rgb(244, 194, 13)` / `rgb(10, 10, 10)`. Hover is a quieter tint
   (`--tss-default-background-hover-color-root`), and on a ghost control the 2px ink frame appearing.
9. **Title bars are sleeve bands.** A window-like surface with a title (card, modal, dialog, panel, drawer) gets a
   bar: `--tss-rw-bar-background` with `--tss-rw-bar-foreground` tracked capitals and a 10px
   `--tss-rw-bar-marker` square in front. On paper black/cream/red, on the terminal yellow/black/black.
10. **Status has colour and shape.** Danger is the red, success the green, info the Autobahn blue, warning the
    yellow with black type; where it needs emphasis, `--tss-rw-hazard` (black and yellow stripes). Never rely on
    the hue alone: keep the icon or the word.
11. **Fields lift to be written on.** An input is a 2px ink box on the surface colour; focused, it throws a 4px
    hard shadow in the signal colour (`box-shadow: 4px 4px 0 0 var(--tss-rw-signal)`). Invalid is the red rule.
    Disabled anything is a dashed rule and the disabled grey.
12. **Motion is a snap.** State changes in 60 to 160ms, stepped (`steps(n)`) rather than eased, and nothing moves
    under `prefers-reduced-motion`.
13. **Charts read in sleeve colours.** `--tss-chart-series-1..8`: red, ink, yellow, Autobahn blue, warm grey,
    orange, teal, violet.

Every selector starts with `body.tss-theme-rechenwerk` (dark mode: `body.tss-theme-rechenwerk.tss-dark-mode`) so the
theme is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`). A
theme file restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

A 10px rail with no track and a square slab of a thumb: a ghost at rest, ink while the pointer is over the scroller,
the signal colour under the pointer and while dragged. CSS only (`::-webkit-scrollbar*`, and `scrollbar-color` for
Firefox, which stops at ink): no script watches or draws a bar.
