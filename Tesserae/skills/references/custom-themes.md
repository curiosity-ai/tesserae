---
name: custom-themes
description: Switch the whole look of a Tesserae app with a custom theme package (ICustomTheme) that is activated and deactivated at run time, loads its own stylesheet on first activation, and scopes every rule under a root class on the body. Use to apply a packaged design language (e.g. Tesserae.Themes.Curiosity), build a theme picker, or write a theme package of your own for a Tesserae (C#/Transpose) app.
---

# Custom themes (`ICustomTheme`)

A custom theme is a NuGet package that restyles every Tesserae component while it is
active. It works by exactly one mechanism: it owns a **root class name**, which
activating adds to `document.body` and deactivating removes. Every rule in the theme's
stylesheet is scoped under that class, so the stylesheet can stay loaded after the theme
is switched off and simply stops matching. Because the class is on the body it also
reaches layers (modals, panels, tooltips, menus), which mount on the body.

The stylesheet is **not** linked from `index.html`: the package declares it with
`"load": false` in its `tps.json`, so it is copied into the app's output and fetched the
first time the theme is activated.

## Use a theme

```csharp
using Tesserae;
using Tesserae.Themes.Curiosity;   // PackageReference Tesserae.Themes.Curiosity

await UI.Theme.SetCustomTheme(CuriosityTheme.Instance);   // activate (loads the CSS once)
await UI.Theme.ClearCustomTheme();                         // back to the default look
var current = UI.Theme.CustomTheme;                        // null when none is active
```

- `UI.Theme.SetCustomTheme(ICustomTheme theme)` — makes `theme` the one active custom
  theme (or clears it when `null`). The new theme's stylesheet is loaded **before** the
  previous one is removed, so switching never flashes the default look. Overlapping calls
  resolve to the last one. Raises `UI.Theme.OnThemeChanged`.
- `UI.Theme.ClearCustomTheme()` — same as `SetCustomTheme(null)`.
- `UI.Theme.CustomTheme` — the theme currently selected.
- Dark mode is orthogonal: `UI.Theme.Dark()` / `Light()` keep working, and a theme styles
  both (`body.<root>.tss-dark-mode`).
- Your own colours still win. A custom theme redefines the `--tss-*` variables on the body,
  and `Theme.Build()…Apply()`, `Theme.SetPrimary(...)`, `SetBackground(...)` and
  `SetHighlight(...)` restate what they set as `!important` under `body.tss-custom-theme`,
  the marker class present while any custom theme is active — so an app can run a packaged
  theme and still recolour its primary (`theme-builder.md`, `theme-colors.md`). Without a
  custom theme the marker is absent and those setters behave exactly as before.

To avoid painting the default look for a frame on first load, hide the body until the
theme is in:

```csharp
document.body.style.visibility = "hidden";
async Task ShowThemed()
{
    try     { await UI.Theme.SetCustomTheme(CuriosityTheme.Instance); }
    finally { document.body.style.visibility = ""; }
}
ShowThemed().FireAndForget();
```

## The interface

```csharp
public interface ICustomTheme
{
    string Name { get; }            // for a picker
    string RootClassName { get; }   // added to document.body while active
    bool   IsActive { get; }        // the class is on the body
    Task   Activate();              // load the CSS on first call, then add the class
    void   Deactivate();            // remove the class; the CSS stays loaded
}
// UI.Theme.SetCustomTheme also adds UI.Theme.CustomThemeMarkerClass ("tss-custom-theme")
// to the body while any theme is active; CustomTheme does the same when used directly.
```

`CustomTheme` (abstract) implements all of it: pass a name, the root class and the
stylesheet URL(s). It also exposes `LoadAsync()` (fetch without activating, e.g. to warm
a picker), `IsLoaded`, `Stylesheets`, and `OnActivated()` / `OnDeactivated()` hooks.

## Write a theme package

1. A Transpose library project referencing Tesserae (`<ProjectReference>` or
   `<PackageReference Include="Tesserae" />`).
2. One class:

   ```csharp
   public sealed class MyTheme : CustomTheme
   {
       public static MyTheme Instance { get; } = new MyTheme();
       private MyTheme() : base("My Theme", "tss-theme-mine", "assets/css/tss-theme-mine.css") { }
   }
   ```

3. CSS files named after the Tesserae stylesheet they restyle (`tss.button.css`,
   `tss.dropdown.css`, …) plus a `tss.common.css` that redefines the `--tss-*`
   variables. A few things a component would otherwise write inline read a variable a theme can
   set instead: chart series without an explicit colour (`--tss-chart-series-1` … `-8`), the
   focused MarkHighlighter match (`--tss-mark-focus-color`), and an initials-only Avatar, which
   is drawn by `.tss-avatar.tss-avatar-generated` from `--tss-avatar-hue`.
   `AI(strongEffect: true)` adds `tss-ai-strong` beside `tss-ai`: redefine the `--tss-ai-*`
   variables under it for a louder AI form (the default theme has no rule for it).
   Scope every selector under the root class: `body.tss-theme-mine .tss-btn`
   (and `body.tss-theme-mine.tss-dark-mode …` for dark). Redefine a derived variable
   together with its `-root` value (`--tss-x-color-root` **and**
   `--tss-x-color: rgb(var(--tss-x-color-root))`): a derived value is resolved where it
   is declared, so changing only the root on the body leaves the derived one as it was.
4. Combine them in `tps.json` into one stylesheet that is not auto-linked:

   ```json
   "resources": [
       { "name": "tss-theme-mine.css", "load": false, "output": "assets/css",
         "files": [ "tps/assets/css/tss.common.css", "tps/assets/css/tss.button.css" ] },
       { "name": "fonts", "files": [ "tps/assets/fonts/*" ], "output": "assets/fonts/mine/" }
   ]
   ```

   The URL passed to `CustomTheme` is `output` + `/` + `name`. Fonts referenced from the
   CSS use paths relative to it (`url("../fonts/mine/x.woff2")`).

## Available themes

- **Tesserae.Themes.Curiosity** — `CuriosityTheme.Instance`, root class
  `tss-theme-curiosity`. Paper and ink, the deep call blue for primary actions and
  selection, the electric-blue Signal as highlight, 1px hairlines instead of shadows,
  radius 0 with pill buttons and single-line inputs, a square toggle, hairline-rail scrollbars
  (a 1px rule for the track and a square 4px ink thumb that widens on hover in Chrome,
  Edge and Safari; a thin bar in Firefox, which only supports `scrollbar-color`). Like
  Tesserae's own scrollbar they hide at rest, a ghost thumb and no rail until the pointer is
  over the scroller, and the thumb only turns ink while that scroller is moving (Signal
  blue while dragged); the theme tracks that with one `scroll` listener, so scrollers in
  your own app need nothing declared. Schibsted Grotesk
  for text and Geist Mono (small, uppercase) for labels, headers, numbers and code. Fonts
  are bundled (OFL). A modal gets a 12px inset between its ring and everything inside it;
  `NoContentPadding()` / `NoPadding()` modals (class `tss-modal-no-inset`), `Dialog`, `ModalStack`
  sheets and a modal in a drawer opt out, so a theme of your own can key off the same class.
  The package also ships the website's signature elements as components (namespace
  `Tesserae.Themes.Curiosity`, usable with or without the theme active): `PixelIntro`
  (the first-load animation, `PlayOnce()`), `FlowField` (the hero's dash field, its pixel
  following the pointer), `PixelGlyph` (the animated 4 by 4 glyphs, `PixelGlyphKind` plus
  custom cells, palettes and a compact frame notation) and `DashMosaic` (the animated dash
  tiles, four schemes). The package README has the API.

## Related

`theme-builder.md` (palette variables), `theme-colors.md` (`SetPrimary` & co.),
`styling.md`, `custom-styles.md`, `colors.md`.
