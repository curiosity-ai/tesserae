# Tesserae.Themes.Memphis

A loud, joyful, geometric [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the 1980s Memphis
design movement (Sottsass and friends): laminate furniture in clashing pastels, black outlines, squiggles and
"bacterio" speckles. (An homage to a design movement; the package has no connection to any company or designer.)

Every control is a cut-out shape with a thick black line round it. A whole box of accents speaks instead of one
(bubblegum pink, mint, sunflower yellow, cobalt, tomato and lilac) on cream paper, and they rotate from card to card
so a page feels like confetti. Buttons are chunky pills standing on hard offset shadows in a second colour, and they
press into them. Patterns (confetti, squiggles, zig-zags, speckles, hatching) are laid on the surfaces around the
content: the page, the sidebar, card and modal headers, empty states, progress fills; never under the text you
read. Dark mode (`UI.Theme.Dark()`) is the night club: deep aubergine walls, the accents turned to neon, the black
lines turned cream.

```csharp
using Tesserae;
using Tesserae.Themes.Memphis;

await UI.Theme.SetCustomTheme(MemphisTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                       // back to the default look
```

The theme adds `tss-theme-memphis` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-memphis.css`) and the fonts
(`assets/fonts/memphis/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **Bungee** (the chunky display face for page titles, section and modal titles, big numbers),
**Outfit** (the friendly geometric text face, variable 100 to 900) and **DM Mono** (code and key chips). Latin and
Latin Extended subsets only.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header
of `tss.common.css`; `tss.button.css`, `tss.card.css`, `tss.contextcard.css`, `tss.textbox.css`, `tss.sidebar.css`,
`tss.modal.css` and `tss.dropdown.css` are the worked examples.

1. **Two moods, one set of tokens.** Write every rule in terms of the `--tss-*` roots and the `--tss-mp-*` tokens,
   never a hex value for something that changes between modes. The page is `--tss-default-background-color-root`
   (cream / aubergine), a panel `--tss-secondary-background-color-root`, the outline `--tss-mp-line` (black / cream).
2. **Bright fills, AA type.** The six accents (`--tss-mp-pink`, `-mint`, `-yellow`, `-cobalt`, `-tomato`, `-lilac`)
   are for fills, and whatever sits on them is `--tss-mp-on-accent` (ink; `--tss-mp-on-cobalt` for type on cobalt by
   day). Tesserae's own roots stay readable as text: primary is a deep magenta and success a deep teal by day,
   danger a red that passes AA on cream, links cobalt (mint at night). A component that writes text in a tone uses
   the roots; one that fills a surface uses the accents.
3. **Everything has a line.** Controls carry a 2px `--tss-mp-line` (`--tss-mp-bw`), panels 3px
   (`--tss-mp-bw-bold`), stickers and chips 1.5px. Fields, panels and menus have a 6px corner; buttons, badges,
   search boxes, segmented trays and list selections are pills; icon-only buttons, avatars, radios, icon discs and
   steps are circles.
4. **Hard shadows in a second colour.** Depth is a solid offset block, never a blur: 3px (`--tss-mp-off-sm`) for
   controls, 5px for cards and popups, 8px for modals and the command palette. The shadow is a different colour from
   the face (primary is bubblegum on cobalt, success mint on lilac, warning sunflower on tomato, danger tomato on
   ink, a popup mint, a modal bubblegum). Under the pointer a pressable thing lifts a pixel up and left onto a longer
   shadow; held down it moves into its shadow until the shadow is gone. Tesserae's `--tss-shadow*` tokens are mapped
   to a ring plus an offset block, so a component that draws its edge with a shadow token gets the Memphis edge.
5. **Confetti rotation.** Repeated things rotate through the accents by `nth-child`: card shadows and header
   strips, context-card icon discs and shadows, the small geometric ornaments on clickable cards (circle, triangle,
   half-moon, bar, ring), tag chips. A single control keeps one fixed pair, so its meaning does not change.
6. **Ghosts in toolbars.** The default button, sidebar rows, menu rows and list rows are flat at rest and become a
   sunflower pill (or sunflower-pale row) under the pointer, so a dense toolbar is not a wall of pills. The current
   item (sidebar entry, tree node, page, segment) is the bubblegum or mint pill with a line.
7. **Patterns frame the content, never sit under it.** The page wears faint confetti
   (`--tss-mp-pattern-confetti`), the sidebar the soft "bacterio" speckle, card headers a soft squiggle over the
   accent, modal headers and the palette's search strip bold confetti, rules a zig-zag, empty and disabled areas a
   soft hatch. Titles that sit on a pattern are set on a cream pill label. All patterns are inline SVG data URIs or
   CSS gradients.
8. **Big type is a sign.** XLarge and up, section and modal titles, metric values and markdown h1 to h3 use the
   display face (`--tss-mp-display`). Small chrome captions (sidebar legends, menu and dropdown headers, table
   headers) are 11 to 12px, weight 800, tracked capitals or bold ink. Body text stays in Outfit.
9. **Fields are calm slabs.** White (plum at night) with a 2px line; hover adds a 2px ink shadow, focus a 3px
   cobalt shadow (sunflower at night); invalid is a red line and red shadow with the message in bold red; disabled
   is a dashed line on a flat, faded slab.
10. **Focus is always visible.** `:focus-visible` is a 3px `--tss-mp-focus` outline (cobalt by day, sunflower at
    night) offset from the part, so it never merges with the black line. Rows that mark focus with their own fill
    (menu, dropdown) drop the ring.
11. **Status keeps its colour and its sign.** Danger is tomato (the red root for text), success mint, warning
    sunflower with ink type, info cobalt. Banners and toasts carry a dotted band of the tone down their leading edge
    and keep their icon; never rely on hue alone.
12. **Motion is a little pop.** Presses take 100ms (`--tss-mp-quick`); things that arrive or slide (modals, the
    pivot underline, toggles, chevrons) overshoot slightly (`--tss-mp-pop`). Under
    `prefers-reduced-motion: reduce` nothing moves.

Every selector starts with `body.tss-theme-memphis` (dark mode: `body.tss-theme-memphis.tss-dark-mode`) so the theme
is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`). A theme
file restates only what differs from Tesserae's own stylesheet of the same name.

## Charts and diagrams

`--tss-chart-series-1..8` are the accents (bubblegum, cobalt, sunflower, mint, tomato, lilac, ink, lime; neon at
night). `tss.charts.css` gives bars, slices and points a black outline, lines a fat round pen and the axis a solid
rule; `tss.mermaid.css` frames diagrams like a code block, on graph paper.

## Scrollbars

A 12px slot with no track and a pill thumb: a ghost at rest, outlined while the pointer is over the scroller,
bubblegum under the pointer and cobalt while dragged (mint at night). CSS only (`::-webkit-scrollbar*`, and
`scrollbar-color` for Firefox, which stops at the outlined state): no script watches or draws a bar.
