# Tesserae.Themes.Weniger

A skeuomorphic [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the West German industrial
design of the 1950s to 80s: the radios, record players, pocket calculators and clocks of the *Weniger, aber besser*
("less, but better") school associated with Dieter Rams. (An homage: the package has no connection to any
manufacturer, and uses none of their names, logos or lettering.)

Every surface is a moulded part. Warm light-grey plastic lit from above, with a hairline groove set in from the edge
of each panel; turned and brushed aluminium for knobs, thumbs and caps; keys that are thick slabs of plastic with
their side wall showing, pushed in when pressed; fields that are recessed wells; numbers that read off a grey-green LCD. One colour is allowed to speak:
signal orange, for the control that matters. A green LED says "on". Dark mode (`UI.Theme.Dark()`) is the charcoal
edition of the same devices: graphite plastic, gunmetal metal, the same orange and the same LCD.

```csharp
using Tesserae;
using Tesserae.Themes.Weniger;

await UI.Theme.SetCustomTheme(WenigerTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                       // back to the default look
```

The theme adds `tss-theme-weniger` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-weniger.css`) and the fonts
(`assets/fonts/weniger/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **Hanken Grotesk** (text, a clean grotesque in the Akzidenz and Helvetica spirit),
**DSEG7 / DSEG14 Classic** (7- and 14-segment LCD figures) and **JetBrains Mono** (code).

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header
of `tss.common.css`; `tss.button.css`, `tss.card.css`, `tss.textbox.css`, `tss.checkbox.css`, `tss.toggle.css`,
`tss.slider.css`, `tss.sidebar.css` and `tss.metric.css` are the worked examples.

1. **Two editions, one set of tokens.** Write every rule in terms of the `--tss-*` roots and the `--tss-wg-*`
   tokens, never a hex value for something that changes between modes. Text is
   `--tss-default-foreground-color-root`, secondary text `--tss-secondary-foreground-color-root`, the page (the
   table the devices stand on) `--tss-default-background-color-root`, a panel `--tss-secondary-background-color-root`.
   Fixed colours (`--tss-wg-orange`, `-orange-hi`, `-orange-lo`, `--tss-wg-red`, `--tss-wg-led`) are for meaning
   that does not change with the mode.
2. **Surfaces are moulded plastic.** A panel (card, modal, panel, dialog, popover body, section card) is
   `background: var(--tss-wg-plastic)`, `border-radius: var(--tss-wg-radius-panel)` (14px),
   `box-shadow: var(--tss-wg-panel-shadow)`, and carries the **groove**: `outline: 1px solid var(--tss-wg-groove);
   outline-offset: calc(-1 * var(--tss-wg-groove-inset))`, a hairline 6px inside the edge that follows the
   radius. Anything lifted off the page (menu, dropdown, tooltip-like popover, toast, modal) uses
   `--tss-wg-float-shadow` instead. Separators inside a panel are 1px `--tss-wg-groove` lines, never a heavy rule.
3. **Soft rounded squares, never sharp, rarely pills.** Panels 14px, keys, fields and menus 8px
   (`--tss-wg-radius-key`), chips and small keys 5px (`--tss-wg-radius-small`). Only knobs, round push buttons,
   LEDs, avatars and toggle tracks are fully round.
4. **Controls are keys.** A button is an extruded slab, like the transport keys of a tape deck: a flat face with a
   line of light along its top edge and a hairline rim, and below it the key's side wall, the same colour a shade
   darker, 4px proud of the panel on a soft shadow (`--tss-wg-slab-*` in `tss.button.css`). Under the pointer it
   lifts a pixel; held down it is pushed in until a hair of the wall is left. Light, rim and wall are the key's own
   colour made lighter or darker in OKLCH, never white or black washed over it. Panel-level keys (checkboxes,
   pivots, rows) use `--tss-wg-key`, `--tss-wg-raised` and `--tss-wg-pressed`. A ghost control (Tesserae's default
   button, toolbar and row commands) is flat at rest and rises into a grey slab under the pointer, so a dense
   toolbar does not become a keyboard. An IconToggle is a strip of square caps in one dark socket, each with a
   shallow round dish and its icon engraved in it; the latched key's engraving is filled orange.
5. **Orange is the one voice.** `--tss-wg-orange` marks the primary action, the checked state (checkbox latched,
   toggle slot, slider scale), the current item's pilot lamp, focus, and links (`--tss-link-color-root`, a darker
   orange for contrast). Do not use it for decoration. White type on orange, weight 600.
6. **Fields are recessed wells.** `background: var(--tss-wg-well-fill)`, 1px `--tss-wg-key-edge`, 8px radius,
   `box-shadow: var(--tss-wg-well)`. Focused: the edge turns orange and an orange halo appears
   (`box-shadow: var(--tss-wg-well), 0 0 0 3px rgba(236, 88, 26, 0.24)`). Invalid: the invalid red edge and halo,
   message in small red type. Disabled: flat `--tss-disabled-background-color-root`, no shadow. Tracks, grooves
   and slots (slider, progress, scroll rails, toggle tracks) are wells too.
7. **Metal for parts you turn or slide.** Knobs, toggle thumbs, round handles and the icon disc of a section title
   are `--tss-wg-alu-knob` (turned, conic) with `--tss-wg-knob-shadow`; slider caps, strips and plates are
   `--tss-wg-alu` (brushed). The sidebar is a brushed plate. Metal is never orange and never carries text larger
   than an index line.
8. **Lamps say state.** A lit indicator is a 5-6px round dot: green `--tss-wg-led` with `--tss-wg-led-glow` for
   "on / live / connected / success", orange with `--tss-wg-led-orange-glow` for "current / selected / attention".
   An unlit lamp is `--tss-wg-led-off`. Radio buttons are round push keys with a lamp in the middle when chosen;
   a selected list row, tab or nav entry is the pressed key with an orange lamp at its leading edge.
9. **Numbers that matter read off an LCD.** A metric, counter, clock, timer, a date in a picker's header, a
   progress percentage: `font-family: var(--tss-wg-lcd)` (7-segment figures; letters and units fall back to
   JetBrains Mono so "1.1k" stays legible) in `--tss-wg-lcd-ink`, on a window of `--tss-wg-lcd-bg` behind the
   `--tss-wg-lcd-well` bezel, radius 6px. The LCD is the same grey-green in both editions. Words on a display use
   `--tss-wg-lcd-alpha` (14-segment). A terminal-like surface (a log, a console) may use the CRT tokens
   (`--tss-wg-crt-bg`, `--tss-wg-crt-ink`) instead.
10. **Labels are printed legends.** Small chrome captions (sidebar section legends, metric titles, field-group
    captions, table headers) are 11-12px, weight 500-600, muted, lowercase or sentence case with a little
    letter-spacing (`0.04em` to `0.08em`), like "photo 1" on a case. Never tracked capitals. Content (what the user
    wrote, list items, cell values, button labels) is left in its own case.
11. **Perforation is ornament, sparingly.** `--tss-wg-grille` (square holes) and `--tss-wg-grille-dots` (round
    holes) are for an empty state, a footer, a decorative strip: never behind text.
12. **Status keeps its colour and its sign.** Danger the red of a record key, success the green, warning amber
    with dark type, info the ink. Never rely on the hue alone: keep the icon or the word, and the lamp.
13. **Motion is a part settling.** Key travel `var(--tss-wg-quick)` (90ms), other state changes 120-180ms on
    `var(--tss-wg-ease)`, no bounce, no spin. Under `prefers-reduced-motion: reduce` nothing travels or animates.

Every selector starts with `body.tss-theme-weniger` (dark mode: `body.tss-theme-weniger.tss-dark-mode`) so the theme
is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`). A theme
file restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

A 10px slot with no track and a rounded thumb: a ghost at rest, graphite while the pointer is over the scroller,
orange under the pointer and while dragged. CSS only (`::-webkit-scrollbar*`, and `scrollbar-color` for Firefox,
which stops at graphite): no script watches or draws a bar.
