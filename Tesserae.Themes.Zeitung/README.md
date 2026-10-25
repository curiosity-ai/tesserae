# Tesserae.Themes.Zeitung

A [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the broadsheet newspaper: the front page of a
serious daily, set in metal and printed on newsprint.

- **Light is the morning edition.** Warm off-white newsprint with a faint fibre in it, black ink, and one spot red that
  is printed sparingly: on the primary action, on the marks that say "you are here", on focus, and on the folio rule
  under a tab.
- **Dark (`UI.Theme.Dark()`) is the night edition.** The same pages on dark grey-black stock with ink-white type and the
  red a little brighter, so it carries.

Typography is the whole design. There are no shadows, no floating boxes and no rounded corners: rules do the work.
Hairline column rules divide things, the thick-and-thin double rule runs across the head of a page, a boxed article
and a table, and everything else is the choice of face, size, figure style and spacing.

```csharp
using Tesserae;
using Tesserae.Themes.Zeitung;

await UI.Theme.SetCustomTheme(ZeitungTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                       // back to the default look
```

The theme adds `tss-theme-zeitung` to `document.body` while it is active, and every rule in its stylesheet is scoped
under that class. The stylesheet (`assets/css/tss-theme-zeitung.css`) and the fonts (`assets/fonts/zeitung/`) are
copied into your app's output on build, but the stylesheet is not linked from `index.html`: it is fetched the first
time the theme is activated.

## Fonts

All bundled, all under the SIL Open Font License 1.1 (the license texts sit beside the files in `tps/assets/fonts/`),
so the theme makes no request to a font host:

| Face | Role | File |
|------|------|------|
| **Zeitung Serif** (Source Serif 4, Adobe) | text, everything by default | `zeitung-serif-roman.woff2`, `zeitung-serif-italic.woff2` |
| **Oswald** | headlines, kickers, column heads | `oswald-latin-wght-normal.woff2` |
| **UnifrakturMaguntia** | the nameplate, one line per page | `unifrakturmaguntia-latin-400-normal.woff2` |
| **Courier Prime** | code and wire copy | `courier-prime-latin-400/700-normal.woff2` |

Source Serif 4 is variable in weight (200 to 900) and in optical size (8 to 60), so text sizes get the sturdier
caption cut and headlines the finer display cut automatically (`font-optical-sizing: auto`). The bundled files are a
Latin subset made from Adobe's own release with the OpenType features the theme relies on kept in: **real small caps**
(`smcp`, `c2sc`), **old-style and lining figures** (`onum`, `lnum`), **tabular figures** (`tnum`), fractions and
superiors. (The webfont builds on the usual font hosts drop small caps and old-style figures, which is why the theme
does not use them.) Because a subset is a modified version and "Source" is a Reserved Font Name under the OFL, the
family is renamed **Zeitung Serif** inside the files. Oswald, UnifrakturMaguntia and Courier Prime are the Fontsource
Latin builds.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header of
`tss.common.css`; `tss.card.css`, `tss.sectiontitle.css`, `tss.sidebar.css`, `tss.button.css`, `tss.textbox.css`,
`tss.markdown.css`, `tss.detailslist.css` and `tss.metric.css` are the worked examples.

1. **Two editions, one set of tokens.** Write every rule in terms of the `--tss-*` roots and the `--tss-zt-*` tokens,
   never a hex value for something that changes between editions: `--tss-zt-ink` (type), `--tss-zt-muted` (captions,
   datelines), `--tss-zt-paper` (the page), `--tss-zt-surface` (a field's panel), `--tss-zt-hair` and `--tss-zt-rule`
   (hairlines), `--tss-zt-tint` / `-tint-strong` (washes of ink), `--tss-zt-red` / `--tss-zt-red-ink` (the spot colour
   as a fill and as type).
2. **The hierarchy is the face.** Five voices, each with one job:
   - *Headline*: the condensed gothic (`--tss-zt-head`), weight 600, tight leading (1.05 to 1.15): page titles, modal
     and panel titles, the big figure of a metric, Tesserae's `xlarge` sizes and up.
   - *Kicker*: the same gothic in capitals, 11 to 13px, tracked `--tss-zt-caps` (0.1em): card titles, section labels,
     column heads, menu headers, metric titles. A kicker that introduces a group may be spot red (menu and dropdown
     headers, the markdown `h4`).
   - *Small caps*: the serif's real small caps (`font-variant-caps: all-small-caps`), tracked `--tss-zt-sc` (0.05em),
     lining figures: button labels, field labels, badges and tags, fact-box labels. Never fake them with
     `text-transform` and a smaller size.
   - *Text*: the serif at 14 to 15px on a 1.5 leading, old-style proportional figures (set on the body), italic for a
     deck, a standfirst, a placeholder or a caption.
   - *Nameplate*: the blackletter (`--tss-zt-mast`), only for the masthead line at the head of the sidebar or a section
     header. Nowhere else.
3. **Figures follow the job.** Old-style figures in running text; **lining tabular** figures wherever numbers are
   compared or read down a column (tables, metrics, pagination, counts, charts, dates in a picker). Write
   `font-variant-numeric: lining-nums tabular-nums` on those components rather than relying on the body.
4. **Rules, not boxes or shadows.** Separation is a 1px hairline (`--tss-zt-hair`, or ink at a lower opacity for a
   column rule). The **thick-and-thin rule** (`--tss-zt-band`, 3px ink, 2px paper, 1px ink, drawn as a background layer
   `top / 100% var(--tss-zt-band-h) no-repeat` with the box padded by `--tss-zt-band-h`) heads a page title, a card, a
   modal, a panel, a table and a pull quote; `--tss-zt-band-thin` heads a small box. Nothing casts a shadow except what
   floats over the page (menu, dropdown, modal, toast), and that gets `--tss-zt-float`: an ink frame and the faintest
   lift.
5. **Square corners.** Every radius is 0. The only round things are a portrait (avatar), a radio button, a spinner and a
   timeline or scheduler dot. Where Tesserae rounds with a fixed pixel value, the file of the same name ends with a
   "Square corners" block that squares it.
6. **Boxed articles.** A card is a 1px ink frame on the paper with the thick-and-thin rule across its head and its title
   set as a kicker over a hairline; its footer is ruled off by a hairline. A hoverable card thickens its frame to 2px;
   nothing lifts.
7. **The spot red is for marks, not decoration.** The primary action (a solid red block), the current item (a 6px red
   square before it in the sidebar, a dropdown or a tree; a 3px red rule under the open tab or down the leading edge of
   a selected row), focus (a 2px red outline, or a red underline in a field), the drop cap's quote mark, a required
   asterisk, a link's underline. Selection itself (text, the keyboard's row in a menu, the chosen segment, the current
   page) is **reversed out**: paper type on ink.
8. **Fields are coupons.** A text field, dropdown, search box or tags input is a pale `--tss-zt-surface` panel framed in
   a grey hairline, standing on a line of ink, the rule you write on. Focused, the frame turns ink and the line becomes
   2px of spot red. Invalid is that line in the invalid red with the message in small italic. Disabled anything is a
   dotted rule and grey type.
9. **Buttons are type.** A ghost button is bare small caps, ruled round with an ink hairline under the pointer and
   washed with ink when pressed. A solid button is a square block of its tone; pressed, it is inked over. A link button
   is a word underlined with a red hairline. Choices are ink: a checkbox is a ballot square that fills with ink, a
   toggle a square slide that fills with ink, a slider a halftone scale with a square paper thumb.
10. **Tables are market listings.** Thick-and-thin rule over the head, column heads as kickers on a line of ink, rows
    divided by hairlines, a closing ink rule, lining tabular figures, a wash of ink under the pointer.
11. **Markdown is an article.** The first paragraph opens with a two-line drop cap; following paragraphs run on with
    a 1.5em indent and no space between; `h1` is a headline, `h2` a bold serif head under an ink hairline, `h4` to `h6`
    kickers; a blockquote is a pull quote (large italic between the thick-and-thin rule and a hairline, a red quote
    mark hung in the margin); a horizontal rule is a centred asterism; code is set in the typewriter face.
12. **Halftone for fills.** A bar, a track, a placeholder or an empty cell is a dot screen of ink
    (`--tss-zt-halftone`, `-light`), the way a paper prints a tint. Solid ink is for the measured part.
13. **Status keeps its colour and its sign.** Success the green ink, warning ochre with ink type, danger the red, info
    ink. A toned notice is a hairline box with its tone as a 3px rule down its leading edge and the faintest wash of
    it behind. Never rely on the hue alone: keep the icon or the word.
14. **Motion is a page turning, not a bounce.** State changes in 120ms on `--tss-zt-ease`; a modal fades in a few pixels
    up. Under `prefers-reduced-motion: reduce` nothing moves.
15. **Charts are business-page graphics.** No frame, dotted hairline gridlines, axes ruled in ink, labels in the serif
    with lining tabular figures, the series inks from `--tss-chart-series-1..8`: ink, the spot red, warm grey, a
    printer's blue, ochre, a green, a plum and a light grey.

Every selector starts with `body.tss-theme-zeitung` (night edition: `body.tss-theme-zeitung.tss-dark-mode`) so the
theme is inert while it is off. Derived variables are redefined together with their `-root` (see `tss.common.css`). A
theme file restates only what differs from Tesserae's own stylesheet of the same name. AI-toned variants (`.tss-ai`)
keep Tesserae's purple-to-blue language.

## Scrollbars

A 9px slot: a 1px rule down the middle for the track (shown while the pointer is over the scroller) and a square thumb,
a ghost at rest, grey with the rule, solid ink under the pointer and the spot red while dragged. CSS only
(`::-webkit-scrollbar*`, and `scrollbar-color` for Firefox, which stops at grey): no script watches or draws a bar.
