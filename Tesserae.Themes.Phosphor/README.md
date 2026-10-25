# Tesserae.Themes.Phosphor

A [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the character terminal: the glass
video terminals of the 1970s and 80s, and the line printers they fed.

Two editions of one machine:

- **Dark (`UI.Theme.Dark()`) is the screen.** Green P1 phosphor on near-black glass. Every glyph blooms a
  little in its own colour, fine horizontal scanlines and a gentle vignette lie over the whole tube, and amber,
  the P3 phosphor, is the second colour: warnings, links, the second chart series.
- **Light is the printout.** Continuous-feed paper with pale green-bar bands, a tractor-feed margin of sprocket
  holes down the edge of the sidebar, the same characters struck in dark carbon-ribbon ink. The same terminal,
  printed: the same frames and the same cursor blocks, inked instead of lit, and nothing glows.

And one build shared by both: one monospace face for everything, box-drawing frames (a single line for a field
or a cell, a double line for a window, a window's title set into its top border), square corners, and the
cursor as the one way the theme says "here": focus, selection, the current tab, the current page, the
highlighted row are all inverse video.

```csharp
using Tesserae;
using Tesserae.Themes.Phosphor;

await UI.Theme.SetCustomTheme(PhosphorTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                        // back to the default look
```

The theme adds `tss-theme-phosphor` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-phosphor.css`) and the fonts
(`assets/fonts/phosphor/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **IBM Plex Mono** (all text, chrome and code; 400, 500, 600 and a 400 italic, Latin and
Latin Extended) and **VT323** (the display face, traced from a glass terminal's bitmap characters, for page and
window titles and the numbers that are the point).

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header
of `tss.common.css`; `tss.button.css`, `tss.textbox.css`, `tss.card.css`, `tss.modal.css`, `tss.pivot.css`,
`tss.sidebar.css` and `tss.progressindicator.css` are the worked examples.

1. **Two editions, one set of tokens.** Write every rule in terms of the `--tss-*` roots and the `--tss-ph-*`
   tokens, never a hex value for something that changes between modes. Text is `--tss-ph-ink`, secondary text
   `--tss-ph-dim`, titles and lit states `--tss-ph-hot` (the brightest phosphor; on paper the blackest ink), the
   glass or the sheet `--tss-ph-paper`, a window's fill `--tss-ph-surface`. Each has a `-root` for `rgba()`.
2. **One face, one cell width.** Everything is IBM Plex Mono (`--tss-ph-mono`); ligatures are off. Headlines
   (TextBlock Large and up, page titles, markdown h1 to h3, metric values) switch to VT323
   (`--tss-ph-display`), a size larger to make up for its small x-height. Chrome legends (sidebar sections, card
   and modal titles, column heads, metric titles) are 11 to 12px capitals with `--tss-ph-tracking`. Content
   stays in its own case.
3. **Frames are box-drawing lines.** A field, a cell, a card, a list item: `--tss-ph-single` (1px). A window
   (a titled card, a modal, a dialog, a menu, a dropdown list, a toast, the command palette):
   `--tss-ph-double` (3px double). Separators inside a window are `--tss-ph-faint`, often dashed. A titled
   window carries its title in its top border, `[ TITLE ]`, on a patch of the surface colour.
4. **Character cells have square corners.** No radius anywhere (`tss.common.css` squares every `tss-*`
   element), except the radio, which a terminal draws as `( )`.
5. **The cursor is inverse video.** `--tss-ph-cursor` with `--tss-ph-on-cursor` type and no text glow: keyboard
   focus on a button or menu row, `::selection`, the current tab (Pivot's sliding indicator grown into a block),
   the current sidebar entry, the selected tree or nav row, the active page number, a solid button, a tooltip.
   Under the pointer a row is only lit (`--tss-ph-tint`); a ghost button inverts.
6. **Buttons are bracketed fields.** A labelled button reads `[ OK ]`; solid buttons are the inverse block with
   the brackets inside. Links are underlined in the link colour and invert under the pointer. Disabled is a
   dashed frame in the dim colour. Buttons that live inside other chrome (sidebar, menus, tab strips, toolbars)
   drop the brackets.
7. **Fields are prompt lines.** A `>` (`--tss-ph-prompt`, an inline SVG) in front, the faint
   `--tss-ph-field` wash over a 1px underline; with the cursor in it the prompt lights (`--tss-ph-prompt-hot`),
   the underline doubles in the cursor colour and the caret is a block (`caret-shape: block`, blinking at the
   system's rate). Invalid is the red underline and a message that starts `! `.
8. **Progress is counted in cells.** Progress bars, sliders, target bars and skeletons are rows of 6px cells
   with 2px gaps (`repeating-linear-gradient`), faint for the track and lit for the value; a slider thumb is
   a block cursor; a spinner turns in eight hard steps.
9. **The screen glows, the paper does not.** In dark mode the body sets `text-shadow: var(--tss-ph-glow)`, drawn
   in `currentColor` so amber text blooms amber; lit frames and blocks add `--tss-ph-box-glow`, floating windows
   `--tss-ph-window-shadow`. The scanlines (`--tss-ph-scanlines`, a 3px raster at 16%) and the vignette are one
   fixed `body::after` layer with `pointer-events: none`. All of it resolves to `none` on paper, where a floating
   window casts a hard 4px carbon offset instead.
10. **Status keeps its colour and its sign.** Success is the phosphor green (on paper a green ink), warning
    amber, danger red; messages and banners carry a double bar of their colour at the leading edge, never the
    hue alone.
11. **Charts read in phosphors.** `--tss-chart-series-1..8` on the screen: P1 green, P3 amber, P4 white, P7
    blue-white, a dim green, orange, red, violet; on paper: carbon, green, ribbon red, amber, blue, grey, teal,
    violet. Gridlines are a dotted graticule; traces use a square pen and bloom on the screen.
12. **Motion is stepped.** State changes snap (`steps()`, 90 to 160ms): the toggle's block jumps, the tab
    cursor steps across. The only loop is the blinking block after the sidebar's brand, which stops under
    `prefers-reduced-motion: reduce`, as does every transition the theme adds.

Every selector starts with `body.tss-theme-phosphor` (dark mode: `body.tss-theme-phosphor.tss-dark-mode`) so the
theme is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`). A
theme file restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

A 10px bar: a 1px rule for the rail and a square block for the thumb, a ghost at rest, dim while the pointer is
over the scroller, the full cursor colour under the pointer and while dragged. CSS only (`::-webkit-scrollbar*`,
and `scrollbar-color` for Firefox, which stops at the dim block): no script watches or draws a bar.
