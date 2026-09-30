# Tesserae.Themes.Curiosity

The Curiosity design language as a [Tesserae](https://github.com/curiosity-ai/tesserae) custom theme:
paper and ink, the deep call blue for primary actions and the electric-blue Signal as highlight,
1px hairlines instead of shadows, square corners with pill buttons and inputs, Schibsted Grotesk
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
