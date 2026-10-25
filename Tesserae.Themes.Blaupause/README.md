# Tesserae.Themes.Blaupause

An engineering-drawing [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme. *Blaupause* is German for a
blueprint. Dark mode (`UI.Theme.Dark()`) is the classic cyanotype: Prussian-blue paper with a fine and a coarse
drafting grid and a little chalky grain, the linework in white. Light mode is the whiteprint (diazo) of the same
sheet: off-white drafting paper with the lines and the grid in blue ink.

Nothing casts a shadow: everything is line. A border is a 1px technical line. A panel is a boxed area of the sheet
with a registration tick in each corner. A separator, a tab marker, a slider or a progress bar is a dimension line
or a scale. A disabled or ghost part is drawn in the dashed hidden-line style. A card's header and a modal's
footer are title blocks. Anything lifted off the page (a menu, a modal, a toast) repeats its own outline 5px down
and to the right, like a sheet laid over the drawing. Chrome is lettered in technical capitals, titles in stencil
capitals, every measurement in monospace. Two accents: the checker's red pencil (redline) for danger and focus,
and a yellow highlighter for the current selection.

```csharp
using Tesserae;
using Tesserae.Themes.Blaupause;

await UI.Theme.SetCustomTheme(BlaupauseTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                         // back to the default look
```

The theme adds `tss-theme-blaupause` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-blaupause.css`) and the fonts
(`assets/fonts/blaupause/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

## Fonts

All three are bundled under the SIL Open Font License 1.1 (license texts beside the files in `tps/assets/fonts/`),
so the theme makes no request to a font host. Latin subsets (Saira also Latin Extended), about 150 KB in all.

| Face | Used for | Files |
|------|----------|-------|
| **Saira** (variable, 100 to 900, upright and italic) | all lettering: chrome in tracked capitals, content in its own case | `saira-latin-*` |
| **Saira Stencil One** | titles only: page and section titles, large bold headings, the brand | `saira-stencil-one-latin-400-normal.woff2` |
| **IBM Plex Mono** (400, 500) | measurements: metric values, chart figures, counts, page numbers, code | `ibm-plex-mono-latin-*` |

They are registered as `"Blaupause Saira"`, `"Blaupause Stencil"` and `"Blaupause Mono"` so they never collide
with an installed copy, and exposed as `--tss-bp-letter`, `--tss-bp-stencil` and `--tss-bp-mono`.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each is for, in the header of
`tss.common.css`; `tss.button.css`, `tss.card.css`, `tss.textbox.css`, `tss.horizontalseparator.css`,
`tss.slider.css`, `tss.pivot.css`, `tss.sidebar.css`, `tss.dropdown.css`, `tss.modal.css` and `tss.charts.css` are
the worked examples.

1. **One sheet, two prints.** Write every rule in terms of the `--tss-bp-*` tokens and the `--tss-*` roots, never a
   hex value for something that changes between the whiteprint and the blueprint. `--tss-bp-ink` is the colour
   every line is drawn in (blue ink on paper, white on the blueprint); `--tss-bp-text` / `--tss-bp-muted` the
   lettering; `--tss-bp-paper` the drawing board; `--tss-bp-sheet` a boxed area of it. A derived variable is
   restated with its root, light and dark (see `tss.common.css`).
2. **No shadows, only lines.** Every Tesserae shadow token is a 1px ring (`--tss-bp-ring`) or the lift. Never
   blur. Borders are `--tss-bp-rule` (1px, `--tss-bp-line`); subordinate lines (row dividers, separators inside a
   panel) `--tss-bp-line-faint`; a strong edge (focus of attention, a floating surface) `--tss-bp-rule-strong`.
3. **Corners are square.** `--tss-border-radius-*` are all 0. The only round shapes are the ones a drafter draws
   with a circle template: radio buttons, avatars, stepper balloons, a pill badge (a callout balloon), the
   spinner and progress ring.
4. **The page is the drawing board.** The page, a section stack, a background area, a chat area, a diagram or
   node canvas and a Mermaid diagram sit on `--tss-bp-grid`: a 10px fine grid, a 50px coarse grid and the chalk
   grain over the paper. A panel sits on it as solid `--tss-bp-sheet`, so the grid stops at its edge.
5. **Panels carry registration ticks.** A card, a section card, a modal, a context card, an action card, a
   timeline entry and a chart frame lay `--tss-bp-ticks` over their background: a short heavy L in each corner,
   painted on the border box (`--tss-bp-tick-len`, `--tss-bp-tick-w` tune it).
6. **Title blocks.** A card's header is a strip lettered in tracked capitals that ends in a boxed cell carrying
   the detail's number (`DET. 01`, a CSS counter down the page, hidden from assistive technology). A modal's footer
   and a panel's or drawer's footer are ruled off with a double line; a footer, a table head or a code block's
   frame may carry the fine grid (`--tss-bp-grid-small`).
7. **Dimension lines and scales.** A separator is a dimension line: extension ticks, slender filled arrowheads
   and its label in the gap (one `clip-path` polygon per half). The current tab is marked by a dimension line the
   width of the tab. A slider's track is a scale rule with minor and major graduations and a square cursor with
   a hairline index; a progress bar is a ruled box with a tick every tenth. A markdown `hr` is a dimension line;
   a drawer's handle is a short one.
8. **Fills are hatched or solid ink.** A filled region (a primary key, a checked box, the chosen segment) is
   solid `--tss-bp-ink` with `--tss-bp-on-ink` lettering. A partial or travelling fill (progress, the slider's
   travel, a toggled-on slot, a pressed key) is section hatching, `--tss-bp-hatch` or `--tss-bp-hatch-dense`.
9. **Hidden lines for what is not there.** Disabled controls, skeletons, drop areas and placeholders are drawn
   with `--tss-bp-hidden` (1px dashed) in the disabled colour. A ghost button has no box at rest and draws one in
   under the pointer.
10. **Keys.** A solid key (primary, danger, success, warning) is a filled box with a second frame drawn 2px
    outside it (`outline`), labelled in 12px tracked capitals; pressed, it is hatched over. A ghost key draws its
    box and a wash under the pointer and is hatched when pressed. Link keys and anything that names content
    (sidebar entries, dropdown options, breadcrumbs, menu rows, table cells) keep their own case.
11. **Red pencil and highlighter.** `--tss-bp-red` is for danger (danger keys, invalid fields, errors, the
    current-time line) and for focus: a 2px red outline, or on a text field a 2px red rule along its foot.
    `--tss-bp-yellow` / `--tss-bp-yellow-wash` mark the current item (sidebar entry, chosen option, tree row,
    unread notification) and selected text. On the blueprint the highlighter becomes a yellow stripe at the
    item's edge over a chalk wash, because yellow over Prussian blue reads as olive.
12. **Lettering.** Chrome captions (labels, table heads, tab titles, card titles, sidebar legends) are 11 to 12px,
    weight 600, uppercase, tracked `--tss-bp-caps` (0.09em). Titles use `--tss-bp-stencil` in capitals. Numbers
    that are read as measurements use `--tss-bp-mono`. Content stays in its own case.
13. **Floating sheets.** A menu, dropdown list, popup, modal, toast or command palette is square, inked round
    with `--tss-bp-rule-strong` and lifted with `--tss-bp-lift`: its outline repeated 5px down and right, filled
    with paper. The page behind a modal is washed back with the paper's colour (`--tss-overlay-*`).
14. **Colour that was given stays.** A tint an app chose (an icon tile, a card's icon, a chart series, a generated
    avatar) becomes the line and lettering colour of the part, never a gradient or a pastel fill. Chart series
    default to `--tss-chart-series-1..8`: inks and coloured pencils on paper, chalk and pencils on the blueprint.
15. **Motion is a pen moving.** State changes 100ms (`--tss-bp-quick`), no bounce; reduced motion turns off the
    transitions that move a part.

Every selector starts with `body.tss-theme-blaupause` (dark: `body.tss-theme-blaupause.tss-dark-mode`) so the theme
is inert while it is off. A theme file restates only what differs from Tesserae's own stylesheet of the same name.

## Charts and diagrams

Charts are drafted: a fine frame with registration ticks round the plot, dashed construction lines for the grid,
axes in full ink, figures in IBM Plex Mono, lines drawn with a technical pen (square ends, mitred corners), points
hollow, bars outlined in their series colour over a light wash of it. Mermaid diagrams (drawn by Pele) map
`--pele-*` onto the tokens: square nodes of sheet colour with an ink line, ink connectors, the drafting grid behind,
red pencil for critical paths and failures.

## Scrollbars

A hairline rail with a square slider on it: the thumb is faint at rest, ink while the pointer is over the
scroller, solid ink under the pointer and red pencil while dragged. CSS only (`::-webkit-scrollbar*`, and
`scrollbar-color` for Firefox, which stops at ink): no script watches or draws a bar.
