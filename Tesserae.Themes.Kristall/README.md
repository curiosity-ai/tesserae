# Tesserae.Themes.Kristall

A [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme after the translucent macOS desktop look of its
current generation: an application drawn as a native window standing on a desktop. (An homage: the package has no
connection to any vendor, and uses none of their names, logos, fonts or artwork.)

The page is the desktop: a soft, static field of colour, and the window's content pane is laid over it nearly
opaque, so a breath of the wallpaper's colour shows through. The sidebar is a vibrant source list, a frosted strip
through which the wallpaper glows, with the current page as a rounded selection pill in the accent. Toolbars and
command bars are unified translucent bars; grouped boxes are white panes with a hairline; menus, popovers, help tags
and notification banners are frosted rounded panels lit by a 1px inner stroke of light; sheets drop in from the
title area on a deep, soft shadow and wear the window's three traffic lights. Controls are native: capsule push
buttons, the blue default button, borderless toolbar buttons that answer the pointer with a glass capsule, pop-up
buttons with a double chevron, the accent switch, check boxes and radios, segmented controls, a capsule search
field, and a focus ring that is a translucent accent halo hugging the control. Dark mode (`UI.Theme.Dark()`) is the
dark appearance: graphite glass over a night wallpaper.

```csharp
using Tesserae;
using Tesserae.Themes.Kristall;

await UI.Theme.SetCustomTheme(KristallTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                        // back to the default look
```

The theme adds `tss-theme-kristall` to `document.body` while it is active, and every rule in its stylesheet is
scoped under that class. The stylesheet (`assets/css/tss-theme-kristall.css`) and the fonts
(`assets/fonts/kristall/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no request to a
third-party font host: **Inter** (text, variable, upright and italic) and **JetBrains Mono** (code), Latin subsets.

## The design language

The rules every file in `tps/assets/css/` follows. The tokens are listed, with what each one is for, in the header
of `tss.common.css`; `tss.sidebar.css`, `tss.button.css`, `tss.textbox.css`, `tss.dropdown.css`, `tss.pivot.css`,
`tss.modal.css` and `tss.contextmenu.css` are the worked examples.

1. **One set of tokens, two appearances.** Write every rule in terms of the `--tss-*` roots and the `--tss-kr-*`
   tokens, never a hex value for something that changes between modes. Text is the label scale
   (`--tss-kr-label` to `--tss-kr-label-4`), quiet surfaces are the fill scale (`--tss-kr-fill` to `-fill-4`), both
   translucent so they sit on any material. Lines are `--tss-kr-separator` (between items) and `--tss-kr-hairline`
   (the edge of a control or a panel), drawn at 0.5px where they are a box-shadow.
2. **Materials, from the back.** The wallpaper (`--tss-kr-wallpaper`, static, fixed to the viewport) is painted
   once on the body under the window veil (`--tss-kr-window`). The sidebar repaints the same fixed wallpaper under
   its own lighter veil (`--tss-kr-sidebar`), so it reads as vibrancy without a `backdrop-filter`. Content sits in
   grouped boxes (`--tss-kr-box`, radius 10px, `--tss-kr-shadow-box`). Floating things (menus, popovers, the
   palette, toasts, sheets) are glass: `--tss-kr-popover` or `--tss-kr-sheet`, `backdrop-filter: var(--tss-kr-blur)`
   and `box-shadow: var(--tss-kr-rim), var(--tss-kr-shadow-pop | -sheet | -toast)`.
3. **Blur only where it is free and safe.** A `backdrop-filter` makes its element the containing block of every
   fixed-position descendant and a stacking context. It goes only on surfaces that mount on the body as layers and
   hold no fixed children (menus, dropdown lists, popovers, modals, panels, toasts, the palette, tooltips), never on
   the sidebar, a list row or anything repeated, and never on a container an application lays content into.
4. **Radii by role.** 5px for a check box, a tag or a menu row; 7-8px for a field, a pop-up button, a segment;
   10px for a grouped box; 12px for a menu or popover; 18px for a sheet, a modal, a toast. Push buttons, the search
   field, switches and badges are capsules (`--tss-kr-r-pill`).
5. **Push buttons.** A coloured button is a capsule of its colour with a faint top light, a 0.5px edge and a small
   shadow; the default button is the accent (`--tss-kr-accent-face`). Nothing glows under the pointer; held down the
   face darkens a shade. Tesserae's default button is the borderless toolbar button: type at rest, a glass capsule
   (`--tss-kr-fill-3` with a light inner stroke) under the pointer. Icon-only buttons become circles.
6. **The accent marks choice.** System blue (`--tss-kr-accent`) is the default button, a checked box, an on switch,
   the slider fill, the selection pill (`--tss-kr-selection`, a wash with the text in the accent), the menu
   highlight (solid, white type) and focus. The rest of the system palette (`--tss-kr-red` to `--tss-kr-gray`) is for
   meaning: danger, success, warning, and the chart series.
7. **Focus is a halo.** `outline: 3.5px solid var(--tss-kr-focus-ring)` with no offset, which follows the
   control's own radius; a text field draws the same as `box-shadow: inset 0 0 0 1px accent, var(--tss-kr-focus)`.
   Invalid is the same halo in red.
8. **Segmented controls.** A Pivot, a SegmentedPivot and an IconToggle are one control: a 9px track in the system
   fill, and the chosen segment a white pill (a lighter glass pill in dark) with a hairline and a small shadow. The
   Pivot's sliding underline is that pill, so it glides between segments.
9. **Menus.** Frosted panel, 5px padding, 26px rows with a 5px radius; the row under the pointer or the keyboard
   is solid accent with white type (a destructive row turns red); a single select marks its row with a check, a
   multi select with the native check box; headers are 11px semibold grey in their own case; dividers are
   hairlines inset from the edge.
10. **Typography.** Inter at the desktop's sizes: 13px body, 11px captions, semibold headings, with tracking that
    tightens as the size grows (`-0.022em` at the largest). Section headers and table headers are small grey
    semibold captions in their own case: never tracked capitals.
11. **Window chrome is decoration.** The traffic lights (`--tss-kr-traffic-lights`: red, yellow, green, 12px)
    appear only before a modal's title, carry no behaviour and take no clicks.
12. **Motion settles.** Sheets drop a few pixels with a touch of scale on `var(--tss-kr-ease)`, the switch knob
    moves on a short spring (`var(--tss-kr-spring)`), everything else is a 120-180ms fade. Under
    `prefers-reduced-motion: reduce` nothing travels; under `prefers-reduced-transparency: reduce` every material
    turns opaque.

Every selector starts with `body.tss-theme-kristall` (dark mode: `body.tss-theme-kristall.tss-dark-mode`) so the
theme is inert while it is off. Redefine derived variables together with their `-root` (see `tss.common.css`). A
theme file restates only what differs from Tesserae's own stylesheet of the same name.

## Scrollbars

The desktop's overlay scroller, in CSS only: an 11px slot with a slim rounded thumb and no track, faint at rest,
darker while the pointer is over the scroller, darker still under the pointer and while dragged, when a hairline
track appears behind it. Firefox, which has no thumb part to hover, stops at the darker grey. No script watches or
draws a bar.
