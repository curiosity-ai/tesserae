# Tesserae.Themes.Washi

A [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme of Japanese paper and ink. The light mode is a
sheet of kozo *washi*: a warm off-white with a faint fibrous grain and a few long fibres caught in it, *sumi* ink for
type and lines, and one colour allowed to speak, vermilion (*shu-iro*), the red of a seal stamp. The traditional dyes
appear only where they carry meaning: indigo (*ai-iro*) for links and information, moss (*koke-iro*) for success,
tea-gold (*kincha*) for warning, crimson (*enji*) for danger. Lines are drawn like brush strokes, pressed in where they
start and thinning to a dry tail; the sidebar is a shoji panel with a kumiko lattice drawn so finely it is felt more
than seen; sheets lift off the page on the softest warm shadow. Dark mode (`UI.Theme.Dark()`) is lacquerware
(*urushi*): deep black-brown lacquer with a sheen running across it, rice-paper cream type, the vermilion a little
brighter, and gold (*kin*, as in kintsugi) for focus and the lines that matter.

Restraint is the brief: quiet, precise, generous whitespace and a calm vertical rhythm.

```csharp
using Tesserae;
using Tesserae.Themes.Washi;

await UI.Theme.SetCustomTheme(WashiTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                     // back to the default look
```

The theme adds `tss-theme-washi` to `document.body` while it is active, and every rule in its stylesheet is scoped
under that class. The stylesheet (`assets/css/tss-theme-washi.css`) and the fonts (`assets/fonts/washi/`) are copied
into your app's output on build, but the stylesheet is not linked from `index.html`: it is fetched the first time the
theme is activated.

## Fonts

Bundled, Latin subsets only (under 100 KB together), all under the SIL Open Font License 1.1 with the license texts
beside them, so the theme makes no request to a third-party font host:

- **Shippori Mincho** (400, 600): headings, titles, captions and the figures that matter. A Mincho whose Latin has
  the contrast and the bracketed serifs of a brush-cut letter.
- **Zen Kaku Gothic New** (400, 500, 700): everything else. Tesserae's semibold (600) is mapped onto the 500 cut, so a
  semibold label stays quiet.
- **IBM Plex Mono**: code.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header of
`tss.common.css`; `tss.button.css`, `tss.checkbox.css`, `tss.sidebar.css`, `tss.sectiontitle.css`,
`tss.horizontalseparator.css`, `tss.pivot.css` and `tss.modal.css` are the worked examples.

1. **Paper and lacquer, one set of tokens.** Write every rule against the `--tss-*` roots and the `--tss-wa-*`
   tokens, never a hex value for something that changes with the mode. Text is `--tss-default-foreground-color-root`
   (sumi, or rice paper on lacquer), diluted ink `--tss-secondary-foreground-color-root`, the ground
   `--tss-default-background-color-root`, a sheet laid on it `--tss-secondary-background-color-root`. Hairlines are
   `--tss-wa-line` (ink at 12-13%), a rule that must be read `--tss-wa-line-strong`, quiet fills `--tss-wa-wash` and
   `--tss-wa-wash-strong`.
2. **Vermilion is the seal, and only the seal.** `--tss-wa-shu` marks the primary action, the checked state, the
   current item, the travelled part of a slider, the active tab's stroke. Never decoration. A coloured block of it
   (or of any dye) is flat and carries `--tss-wa-pores`, the fine grain of seal paste pressed on paper.
3. **The seal is a shape.** Selection and "this one" are stamped, not filled: `--tss-wa-hanko` (square, pressed,
   with an uneven edge and a few pores) and `--tss-wa-hanko-round` are SVG masks over vermilion. The checkbox, the
   current sidebar entry, the multi-select row, the active step and every page title's icon are stamped this way.
4. **Primary as information is indigo.** Where Tesserae uses its primary colour as a *tone of information* rather
   than as an action (a Primary banner, an Information toast, a "changed" row, a running plan, an unread
   notification), the theme uses indigo `--tss-wa-ai`, so vermilion keeps meaning "act" and "chosen".
5. **Lines are brush strokes.** A separator, the rule after a heading, a tab's underline, a quotation's margin and
   the stripe down a notice are `--tss-wa-stroke` (horizontal) or `--tss-wa-stroke-v` (vertical): an SVG mask over a
   colour, heavy where it starts and tapering out. Structural hairlines (field edges, table rules, frames) stay
   straight and thin.
6. **Sheets lift, they do not float.** Corners are cut almost square: `--tss-wa-radius` (3px) for sheets, fields
   and buttons, `--tss-wa-radius-small` (2px) for chips. A card, section or field is a sheet with a hairline edge;
   `--tss-wa-lift` raises it by a hair, `--tss-wa-float` holds a menu, modal or toast above the page. Shadows are
   warm, wide and very faint on paper; on lacquer they are black. Modals, panels, drawers and the command palette
   carry the paper's grain (or the lacquer's sheen) of their own.
7. **Fields are slips of smooth paper.** `background: rgb(var(--tss-secondary-background-color-root))`, 1px
   `--tss-wa-field-edge`, darkening to `--tss-wa-field-edge-hover` under the pointer. Focused: the edge becomes the
   thread and a soft halo appears (`box-shadow: 0 0 0 3px var(--tss-wa-thread-halo)`). Invalid: the crimson edge,
   a small crimson message. Disabled: blank paper, `--tss-disabled-background-color-root`.
8. **Focus is a thread.** Indigo on paper, gold on lacquer (`--tss-wa-thread`), 1.5px, never the vermilion (which
   would read as an error or a selection).
9. **Type is set like a printed page.** Mincho (`--tss-wa-serif`) for anything Tesserae draws at 20px and up, for
   titles, captions, section legends and figures (a metric, a page number, a step); Zen Kaku Gothic New for the rest.
   Small captions are sentence case with a little letter-spacing (0.04-0.06em), never tracked capitals.
10. **The ground has texture; content never does.** The page and the sidebar carry `--tss-wa-grain` (and the sidebar
    the shoji `--tss-wa-lattice`); sheets that hold content are smooth paper, so nothing sits on noise but large
    surfaces.
11. **The dyes are quiet.** Charts, Gantt bars, uptime days, mermaid series and the contribution bar read the eight
    dyes in `--tss-chart-series-1..8` (vermilion, indigo, moss, tea-gold, wisteria, diluted ink, pale indigo,
    crimson). AI variants are wisteria running into indigo. Highlighted matches are a tea-gold wash, like a reader's
    marker.
12. **Motion is ink settling.** 120-260ms on `var(--tss-wa-ease)`, no bounce; a lifted sheet rises 1px. Under
    `prefers-reduced-motion: reduce` nothing travels.

Every selector starts with `body.tss-theme-washi` (dark mode: `body.tss-theme-washi.tss-dark-mode`) so the theme is
inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`). A theme file
restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

No track: a thin thumb like a line drawn with the tip of the brush, a ghost at rest, diluted ink while the pointer is
over the scroller, full ink (gold on lacquer) under the pointer and while dragged. CSS only (`::-webkit-scrollbar*`,
and `scrollbar-color` for Firefox, which stops at diluted ink): no script watches or draws a bar.
