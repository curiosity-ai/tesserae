# Tesserae.Themes.RetroOS

The original Macintosh interface (System 1 to 6, 1984-1991) as a
[Tesserae](https://github.com/curiosity-ai/tesserae) custom theme: one bit of colour, black on white, every
mid-tone a dither of the two, pinstriped title bars, hard one-pixel drop shadows, inverted selection, and
pixel fonts where the Mac had Chicago, Geneva and Monaco. Light and dark (`UI.Theme.Dark()`, which is
reverse video: white on black) are both covered.

```csharp
using Tesserae;
using Tesserae.Themes.RetroOS;

await UI.Theme.SetCustomTheme(RetroOSTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                       // back to the default look
```

The theme adds `tss-theme-retroos` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-retroos.css`) and the fonts
(`assets/fonts/retroos/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to
a third-party font host: **Jersey 15** is Chicago (menus, buttons, titles), **DotGothic16** is Geneva (text,
lists), **VT323** is Monaco (code, numbers).

## The design language

The rules every file in `tps/assets/css/` follows. They are what makes a screen read as a Mac and not as a
black-and-white version of something else.

1. **One bit.** Ink (`--tss-default-foreground-color`) on paper (`--tss-default-background-color`) and nothing
   else. No hues, no opacity, no gradients, no blur. In dark mode the two swap, so write every rule in terms of
   the two tokens and it is right in both. The only grey is `--tss-retro-gray`, for disabled text and edges.
2. **A mid-tone is a dither.** Where the Mac needed a grey it drew a checkerboard of black and white pixels:
   `--tss-retro-dither` (50%) for the desktop, tracks and disabled fills, `--tss-retro-dither-light` (25%) for
   quiet fills. Paint patterns with `image-rendering: pixelated`-safe integer sizes (2px tiles).
3. **A line is 1px, black.** Every surface is framed by a 1px ink line. A surface that floats (window, menu,
   dialog, popover, toast) also has a hard drop shadow: `--tss-retro-shadow` (2px 2px, no blur). Corners are
   square except push buttons and the like, which are rounded rectangles (8px at button height).
4. **Selection inverts.** A selected item, a highlighted menu row, a pressed button, a hovered ghost control:
   ink background, paper text, and its icons follow (`color` is inherited, so set it on the row and the
   `.tss-fontcolor-*` children). Hover is not a tone change on a control that had none on the Mac.
5. **Windows have pinstripes.** A title bar is `--tss-retro-stripes` with the title centred in a paper-coloured
   gap, and a small square close box at the left. A window's body is paper with an ink frame.
6. **Chicago for chrome, Geneva for content.** Menus, buttons, tabs, titles, badges and headings use
   `font-family: var(--tss-retro-chicago)` (Jersey 15, small: use 16px where the base is 13px). Everything else
   inherits the body's Geneva. Code and numbers use `var(--tss-monospace-font-family)`.
   Never set `font-variant-ligatures` back on: the fonts are drawn without them.
7. **No motion that is not a snap.** `transition: none`. A state changes in one frame. Spinners may step.
8. **Focus is a thicker line, not a glow.** 2px ink outline with a 1px offset.
9. **Colour has no meaning to spend.** Danger, success, warning and info are told apart by words, icons and
   shape (a dashed or double border, an inverted block), never by hue.
10. **Charts read in patterns.** Series use black, white with a black outline, and dithers; series colours come
    from `--tss-chart-series-N`.

Every selector starts with `body.tss-theme-retroos` (dark mode: `body.tss-theme-retroos.tss-dark-mode`) so the
theme is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`).
A theme file restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

The System 6 bar: 16px wide, an arrow box at each end, a dithered track and a white thumb with a black edge.
It is CSS only (`::-webkit-scrollbar*`, and `scrollbar-color` for Firefox): no script watches or draws a bar.
