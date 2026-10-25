# Tesserae.Themes.Glas

A liquid-glass [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme. The page is a soft, slowly varying
field of colour (lilac, sky, peach, mint and a breath of rose; violet, teal, magenta and cobalt in the dark), and
everything that holds content is a pane of glass laid over it: a translucent tint with a fine frost of grain, a sweep of
light across it, a specular rim where the light catches its top-left edge, a faint inner glow and a soft, deep shadow.
Overlays (menus, dropdowns, modals, panels, tooltips, toasts, the command palette) are the thick glass: they blur and
saturate the page behind them and, in Chromium, refract it through a gentle displacement field. Coloured controls are
tinted glass lozenges with a gloss across their upper half and a glow of their own colour on the field. Light mode is
clear frosted glass over pastel light; dark mode (`UI.Theme.Dark()`) is smoked glass over deep, saturated colour.

```csharp
using Tesserae;
using Tesserae.Themes.Glas;

await UI.Theme.SetCustomTheme(GlasTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                    // back to the default look
```

The theme adds `tss-theme-glas` to `document.body` while it is active, and every rule in its stylesheet is scoped under
that class. The stylesheet (`assets/css/tss-theme-glas.css`) and the fonts (`assets/fonts/glas/`) are copied into your
app's output on build, but the stylesheet is not linked from `index.html`: it is fetched the first time the theme is
activated.

The fonts are bundled (SIL Open Font License 1.1, licence texts beside them), so the theme makes no request to a
third-party font host: **Plus Jakarta Sans** (text, a variable grotesque with open, friendly forms) and
**JetBrains Mono** (code). Latin and Latin Extended subsets only.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header of
`tss.common.css`; `tss.button.css`, `tss.card.css`, `tss.sidebar.css`, `tss.modal.css`, `tss.textbox.css`,
`tss.toggle.css` and `tss.segmentedpivot.css` are the worked examples.

1. **The field is the light source.** The body carries `--tss-gl-field`, layered radial gradients fixed to the viewport
   (`background-attachment: fixed`, no animation). Nothing else on the page is opaque unless it must be: the colour has
   to come through the glass. A section stack's secondary wash is reduced to a breath for the same reason.
2. **Surfaces are glass, through the tokens.** The theme's one structural trick: the *derived* surface colours
   (`--tss-default-background-color`, `--tss-secondary-background-color`, `--tss-sidebar-background-color`, the hover,
   active and disabled fills, borders and separators) are translucent, while their `-root` triplets stay opaque. Every
   component that paints `var(--tss-default-background-color)` becomes glass without a rule of its own; anything that
   mixes the root with its own alpha still gets a solid colour. A component file only restates what needs more than a
   tint.
3. **A pane is tint, frost, sweep, rim, glow, shadow.** `background: var(--tss-gl-pane)` (tint + grain + sweep),
   `box-shadow: var(--tss-gl-surface)` (rim + glow + shadow), radius `--tss-gl-radius-pane` (22px) for a large pane,
   `--tss-gl-radius-card` (16px) for a card, a menu or a toast, `--tss-gl-radius-ctrl` (12px) for a field or a row,
   `--tss-gl-radius-chip` (8px) for a chip. Separators are hairlines of light (`--tss-gl-edge`), never heavy rules.
4. **Blur only where something can pass behind.** Over the smooth field a blur is invisible, so cards, rows and list
   items are tint only (a page of 150 cards costs nothing extra). `backdrop-filter` is spent where content really
   moves or lies behind the glass: overlays (`--tss-gl-blur-thick`), sticky headers (the DetailsList header, the
   Scheduler's headers and gutters, diff file headers) and the sidebar slab (`--tss-gl-blur`).
5. **Glass inside glass never blurs again.** An element with `backdrop-filter` is the backdrop root of its descendants:
   a second blur inside it sees only its parent's tint and turns to mud. A card in a modal, a card in a card, a pane in
   a panel take `--tss-gl-pane-inner` (a flat, clearer tint) and the rim, no filter. For the same reason an overlay's
   veil never filters its own container: the modal, panel, drawer and command palette containers draw the veil's blur on
   a `::before` (or a sibling overlay), so the pane itself still blurs the page.
6. **Blur never becomes a containing block by accident.** A `backdrop-filter` makes its element the containing block of
   any `position: fixed` descendant. The sidebar's slab is therefore drawn on the sidebar's own `::before`, not on the
   sidebar; overlays that blur are layers mounted on the body, whose fixed descendants are other layers on the body.
7. **Refraction is progressive.** `--tss-gl-refract` adds an SVG displacement filter (an inline data URI: fractal noise,
   softened, then `feDisplacementMap`) to the overlay blur, so the page behind a modal, a menu, a dropdown, a toast or
   the palette is warped as well as blurred. It is applied only inside
   `@supports (backdrop-filter: url("#a")) and (not (-moz-appearance: none)) and (not (font: -apple-system-body))`,
   i.e. Chromium; Firefox and Safari keep the plain blur.
8. **Coloured controls are lozenges.** Primary, danger, success and warning buttons, the checked checkbox, the toggle's
   track, the slider's travelled part, the current page, the active step: their colour (`--tss-gl-k` in the button file)
   at ~94%, `--tss-gl-lozenge-shine` over it, `--tss-gl-lozenge-rim` and a glow of the same colour. Buttons are pills;
   pressed, a pill squeezes slightly (`scale(0.97)`), like a drop under a fingertip.
9. **The ghost condenses.** Default buttons, toolbar commands, list and sidebar rows are clear at rest and condense into
   clear frosted glass under the pointer (white at ~50% with the rim; white at ~9% in the dark). The current entry of a
   list is an accent-tinted lozenge with its label in `--tss-gl-accent-ink`.
10. **Fields are wells.** `background: var(--tss-gl-well)`, 1px `--tss-gl-edge-strong`, `--tss-gl-field-shade` (a shade
    along the top inner edge and a line of light under it). Focused: the accent edge and a 4px accent halo. Invalid:
    the same in rose. Disabled: flat frost, no shade. Tracks (slider, progress, toggle, segmented controls) are capsule
    wells; a segmented control's choice is a bead of clear glass riding in it.
11. **Text stays readable on glass.** Ink is a deep indigo-slate on the light glass and a pale lavender on the smoked;
    secondary text is held at about 4.5:1 or better against the glass over the brightest part of the field. Accent used
    as text is `--tss-gl-accent-ink` (darker in light, lighter in dark), amber used as text is darkened, and overlays
    carry a thicker tint (`--tss-gl-pane-thick`) so text over a blurred, busy page keeps its contrast.
12. **Light is soft and deep.** Shadows are wide and low-alpha, tinted towards the field's indigo; the rim is the brightest
    thing on a pane. Focus is an accent glow ring. Motion is short, with a hint of overshoot (`--tss-gl-ease`); under
    `prefers-reduced-motion: reduce` nothing squeezes, lifts or slides.

Every selector starts with `body.tss-theme-glas` (dark mode: `body.tss-theme-glas.tss-dark-mode`), so the theme is inert
while it is off; broad defaults are written through `:where()` so a component's own, more specific rule still wins.
Redefine derived variables together with their `-root` (see `tss.common.css`). A theme file restates only what differs
from Tesserae's own stylesheet of the same name.

## Scrollbars

A 10px slot with no track and a rounded thumb: a ghost at rest, a deeper tint while the pointer is over the scroller, the
accent under the pointer and while dragged. CSS only (`::-webkit-scrollbar*`, and `scrollbar-color` for Firefox, which
stops at the deeper tint): no script watches or draws a bar.

## Credits

The layering of the glass (a blurred backdrop, a tint, a "shine" of inset highlights round the edge, and an SVG
`feTurbulence` + `feDisplacementMap` filter for the liquid refraction) follows the technique shown in
[lucasromerodb/liquid-glass-effect-macos](https://github.com/lucasromerodb/liquid-glass-effect-macos) and
[obaidqatan/liquid-glass-component-library](https://github.com/obaidqatan/liquid-glass-component-library) (MIT). No code
was copied; the filter, the tokens and the stylesheets here are written for Tesserae.
