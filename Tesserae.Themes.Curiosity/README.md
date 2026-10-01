# Tesserae.Themes.Curiosity

The Curiosity design language as a [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme:
paper and ink, the deep call blue for primary actions and the electric-blue Signal as highlight,
1px hairlines instead of shadows, square corners with pill buttons and inputs (search boxes are square), Schibsted Grotesk
for text and Geist Mono for labels, numbers and code. Light and dark (`UI.Theme.Dark()`) are both
covered.

```csharp
using Tesserae;
using Tesserae.Themes.Curiosity;

await UI.Theme.SetCustomTheme(CuriosityTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                         // back to the default look
```

The theme adds `tss-theme-curiosity` to `document.body` while it is active, and every rule in its
stylesheet is scoped under that class. The stylesheet (`assets/css/tss-theme-curiosity.css`) and the
fonts (`assets/fonts/curiosity/`) are copied into your app's output on build, but the stylesheet is
not linked from `index.html`: it is fetched the first time the theme is activated.

The fonts are bundled (SIL Open Font License 1.1, license texts beside them), so the theme makes no
request to a third-party font host.

## Components

The package also carries the website's signature elements as Tesserae components (namespace
`Tesserae.Themes.Curiosity`). They draw with the brand's colours directly, so they work with or
without the theme active, animate only while on screen, and stand still under reduced motion.

```csharp
PixelIntro.PlayOnce();                                   // the first-load animation, once per tab
PixelIntro.PlayOnce(PixelIntroStyle.Tiles);              // the brand style's tile sweep instead

new FlowField().H(360);                                  // the hero's dash field on ink, pixel follows the pointer
new FlowField().OnPaper().Fade(FlowFieldFade.Left).NoInteraction().NoAnimation();

new PixelGlyph(PixelGlyphKind.PlayBacklog, 96).ShowLabel();   // any of the website's 85 glyphs
PixelGlyph.Custom(new[] { 1,1,0,4, 1,1,1,0, 1,1,1,0, 0,0,0,0 });  // your own 16 cells, built in
PixelGlyph.FromAnimation("k.../..../..../.... kk../..../..../....:400")
          .Palette(new Dictionary<int, string> { [PixelGlyph.Ink] = "#0029E7" });

new DashMosaic(DashMosaicScheme.DarkBlue);               // the animated dash tiles: LightBlue, Dark, DarkBlue, Light
new DashMosaic(DashMosaicScheme.Light, DashMosaicPattern.Band).NoAnimation();
```

- **`PixelIntro`**: `PlayOnce(style)` plays in the first load of a tab (a reload replays it, as on the
  website) and `Play(style)` plays now. `SquareField` (the default) or `Tiles`.
- **`FlowField`**: the brand's dash field, redrawn at its size. `Surface`/`OnInk`/`OnPaper`, `Fade`,
  `Mark` (Signal, Tone, Hover), `Pitch`, `DotAt`, `Drift`, `Animated`/`NoAnimation`,
  `Interactive`/`NoInteraction`.
- **`PixelGlyph`**: 4 by 4 glyphs. `PixelGlyphKind` lists every glyph on the website (value, use case,
  capability and category glyphs, the three problem stories, the 54 developer loops). An animation is
  a string of frames (16 cells each, `/` between rows, `:ms`, `:label`); cells are palette keys (digits,
  or `.`, `k`, `s`, `a`, `*`, `o`, `d`, `t`). `Decode`, `Encode` and `BuildAnimation` are public.
  `Palette`, `Animated`/`NoAnimation`, `Playback`, `RestHold`, `ShowEmptyCells`, `ShowLabel`, `Size`,
  `Replay`.
- **`DashMosaic`**: the dash tiles, in four `DashMosaicScheme`s and seven `DashMosaicPattern`s (Wave,
  Rows, Chart, ChartGrid, Mountain, Band, Dither), with the website's bar spacing and 6px edge padding
  by default. `Animated`/`NoAnimation`, `Seed`, `EdgePadding`, `FramesPerSecond`.
