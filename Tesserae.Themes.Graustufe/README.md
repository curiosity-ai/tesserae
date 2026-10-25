# Tesserae.Themes.Graustufe

A [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the display of an e-paper reader.
*Graustufe* is German for "grey level": the panel this theme imitates can show sixteen of them and nothing else.

Everything is drawn in a 16-step grey ramp on a slightly warm paper: crisp 1px ink rules, square corners, text set
like a book, chrome in a sans built for legibility. Shadows are replaced by what a 1-bit panel does to fake a grey:
dithered drop edges, ordered-dither screens and hatching. Selection is inverted (solid ink, paper type), focus is a
thick ink frame. Dark mode (`UI.Theme.Dark()`) is the reader's night mode: the same ramp turned over, light grey type
on near-black.

And nothing moves. An e-ink panel redraws; it does not animate. Under this theme every transition and every
animation is off, and what exists only as motion has a still replacement: the spinner is a dotted ring, an
indeterminate progress bar is hatched, a skeleton is a block of dither.

```csharp
using Tesserae;
using Tesserae.Themes.Graustufe;

await UI.Theme.SetCustomTheme(GraustufeTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                         // back to the default look
```

The theme adds `tss-theme-graustufe` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-graustufe.css`) and the fonts
(`assets/fonts/graustufe/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **Literata** (the reading face, a book serif drawn for screens and e-readers),
**Atkinson Hyperlegible Next** (chrome: buttons, fields, tabs, menus, labels) and **Atkinson Hyperlegible Mono**
(code). All three are variable, Latin subset, about 160 KB together.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header
of `tss.common.css`; `tss.button.css`, `tss.card.css`, `tss.modal.css`, `tss.banner.css`, `tss.charts.css` and
`tss.sidebar.css` are the worked examples.

1. **Sixteen greys and nothing else.** Every colour is a level of the ramp, `--tss-gs-0` (ink) to `--tss-gs-15`
   (paper), interpolated in OKLab with a trace of warmth. The ramp follows the mode: `gs-0` is always the ink and
   `gs-15` always the paper, so a rule written once is right in both. `--tss-gs-abs-N-root` is the light ramp,
   fixed, for things that must not flip. Never write a hex value in a component file.
2. **Tesserae's colours are levels too.** Every `--tss-*-color-root` is mapped onto the ramp (primary is ink with
   paper type, success a dark grey, warning a light one, danger ink), every `--tss-colors-*` swatch becomes the level
   of the same lightness (from Tesserae's light and dark blocks separately), the named gradients run between two
   levels, and the AI tone (`--tss-ai-*`) is ink fills and a dither texture instead of purple and blue.
3. **Colour from outside is greyed.** Pictures, video, canvas, SVG, emoji, and any element whose inline style carries
   a literal colour (`rgb(`, `hsl(`, `#`, a gradient) get `filter: var(--tss-gs-panel)`: grey of the same lightness,
   toned back to the paper's warmth so it lands on the ramp's own hue. A filtered element becomes the containing
   block of its fixed-position descendants; Tesserae's layers mount on the body and are unaffected.
4. **Rules are ink.** Frames and fields are 1px `--tss-gs-rule`; separators between rows are the faint rule
   (`gs-11`); a current tab or a table head stands on `--tss-gs-rule-heavy` (2px).
5. **Square.** No radius anywhere except 2px (`--tss-gs-radius`) on fields and keys, and what is round by meaning: a
   radio, an avatar, a status dot, a pill-shaped token.
6. **No shadow survives; edges are dithered.** A surface lifted off the page draws its frame and a drop edge with a
   `border-image` (`--tss-gs-edge-sm` 2px for a key or a tooltip, `--tss-gs-edge` 3px for a card or a menu,
   `--tss-gs-edge-lg` 5px for a modal or a toast): the inner half a 50% checkerboard, the outer half a sparse 25% dot,
   generated pixel by pixel, so it takes no layout and stays crisp. Tesserae's shadow tokens become an ink ring
   (plus a flat grey offset for the large ones) for anything that drew its edge with them.
7. **Patterns tell apart what greys cannot.** `--tss-gs-dither` (50% checkerboard), `--tss-gs-bayer-12/-25/-50/-75`
   (4x4 ordered dither) and `--tss-gs-hatch` (45-degree hatch). A modal or panel screens the page with the 25%
   dither; a banner's tone is a band down its edge (info solid, success dither, warning hatch, danger solid with an
   inverted icon); a minor incident is hatched, a major one solid.
8. **Selection is inversion.** Selected text, the current sidebar page, a chosen dropdown row, the chosen segment, a
   menu row under the pointer, a pressed ghost button: solid ink with paper type, the way a reader marks a passage.
   Hover on a ghost control is an ink frame; hover on a row is the wash (`gs-13`).
9. **Focus is a thick frame.** `--tss-gs-focus` (3px solid ink) at a 2px offset; a field draws it on its own frame
   (1px border plus a 2px ring, so nothing moves).
10. **Two faces.** Text is the book face (`--tss-gs-text`, also `--tss-sansserif-font-family`): body copy, headings,
    titles, markdown, captions in italic. Chrome is the sans (`--tss-gs-ui`): buttons, fields, tabs, menus, labels,
    table heads, chart labels. Code is the mono (`--tss-gs-mono`).
11. **Status keeps its sign.** Without hue, a state is a grey plus a shape or a pattern: danger buttons and tokens
    are framed twice, warnings dashed or hatched, presence is a filled, half-filled, barred or empty dot. Keep the
    icon or the word.
12. **Charts read in greys and pens.** `--tss-chart-series-1..8` are levels a few steps apart (ink first); each
    line series has its own dash pattern, bars and slices carry an ink outline, a heat map runs from paper to ink.
13. **Nothing moves.** `transition: none !important; animation: none !important` on every element and pseudo-element
    under the root class, `scroll-behavior: auto`, no backdrop blur. A state change is a redraw.

Every selector starts with `body.tss-theme-graustufe` (dark mode: `body.tss-theme-graustufe.tss-dark-mode`) so the
theme is inert while it is off. A theme file restates only what differs from Tesserae's own stylesheet of the same
name; several end with a generated block that squares every corner and flattens every soft shadow the base file
declares.

## Motion off, and the one exception

With every transition off, an overlay that waits for `transitionend` to finish closing would stay in the document.
This was checked in a browser, against the default theme: Modal, Panel, Dialog, Drawer, Toast, ContextMenu,
Dropdown, Popover, Tooltip, Accordion/Expander, the sidebar's collapse and Carousel all open and close (and leave
nothing behind that blocks the page) with motion off, and Teaching behaves as it does in the default theme. One thing in the toolkit does depend on the event: **Tippy** (`tippy.min.js`, used by `Tooltip()`, `Popover`,
`Teaching` and the sidebar flyouts) unmounts a hidden popup on its box's `transitionend`. Without it every popup
ever shown stayed in the DOM, hidden, and its `onHidden` callback never ran. So `.tippy-box` keeps a 1ms transition
on `visibility` (which changes on every hide): not motion anyone can see, and enough for the event to fire.

## Scrollbars

A 9px bar with no track and a square thumb: pale grey at rest, a 1px rail and a mid-grey thumb while the pointer is
over the scroller, solid ink under the pointer and while dragged. CSS only (`::-webkit-scrollbar*`, and
`scrollbar-color` for Firefox, which stops at mid grey): no script watches or draws a bar.
