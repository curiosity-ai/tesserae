# Tesserae.Themes.Stratus

An aerospace-grade design language as a [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme:
a deep navy accent on cool-grey neutrals, filled text fields that carry their state on a bottom rule,
small 3px corners, a tiered elevation scale instead of hairlines, and Inter with Roboto Mono. Light and
dark (`UI.Theme.Dark()`) are both covered.

The palette, type scale, spacing, radii, elevation levels and component behaviour follow the
an open-source (Apache-2.0) aerospace design system, re-expressed as Tesserae
stylesheets. The package is independent of that project: it ships none of its code, fonts or logos, and
it carries a name of its own.

```csharp
using Tesserae;
using Tesserae.Themes.Stratus;

await UI.Theme.SetCustomTheme(StratusTheme.Instance);   // activate
await UI.Theme.ClearCustomTheme();                       // back to the default look
```

The theme adds `tss-theme-stratus` to `document.body` while it is active, and every rule in its stylesheet
is scoped under that class. The stylesheet (`assets/css/tss-theme-stratus.css`) and the fonts
(`assets/fonts/stratus/`) are copied into your app's output on build, but the stylesheet is not linked from
`index.html`: it is fetched the first time the theme is activated.

Inter and Roboto Mono are bundled as variable fonts (SIL Open Font License 1.1, license texts beside them),
so the theme makes no request to a third-party font host.

## The language

| | Light | Dark |
|---|---|---|
| Page / text | `#ffffff` / `#14171d` | `#14171d` / `#ffffff` |
| Accent (primary action, selection) | navy `#00205b` | soft blue `#86a8e9` |
| Link, focus, information | blue `#255fcc` | `#86a8e9` |
| Neutrals | cool grey `#eff1f4` … `#63728a` … `#14171d` | dark greys `#181c21` … `#32353b` |
| Success / warning / error | `#08875b` / `#ffc929` / `#e4002b` | `#27e7a7` / `#ffd557` / `#f86471` |

- **Type**: Inter, 14px body (16px large), headings 20 to 56px, weights 400 / 500 / 700. Roboto Mono for code.
- **Corners**: 3px on controls, 6px on cards, 8px on floating surfaces, fully round for avatars and tags.
- **Elevation**: levels 1 to 6. A card rests at level 1 and lifts under the pointer; a dialog sits at level 6.
- **Fields**: a filled slab with a 1px rule under it. Hover shifts the fill, focus tints it and thickens the rule
  to a 3px ring, and error / success / information colour the rule.
- **Focus**: a 2px blue ring set 2px clear of the control.
- **Scrollbars**: a slim rounded thumb, CSS only (no script touches a scroller).
