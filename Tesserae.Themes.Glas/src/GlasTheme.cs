namespace Tesserae.Themes.Glas
{
    /// <summary>
    /// A liquid-glass Tesserae custom theme. The page is a soft field of colour (pastel in light mode, deep and saturated in
    /// dark mode) and every surface is a pane of glass over it: a translucent tint with a fine frost, a specular rim along the
    /// top-left edge, a faint inner glow and a soft deep shadow, with generous radii and pill-shaped buttons. Overlays (menus,
    /// dropdowns, modals, panels, tooltips, toasts, the command palette) blur and saturate the page behind them and, in
    /// Chromium, refract it through an SVG displacement filter. Coloured controls are tinted glass lozenges with a gloss and a
    /// glow of their own colour. Light mode is clear frosted glass, dark mode smoked glass. Plus Jakarta Sans and JetBrains
    /// Mono are bundled (OFL).
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(GlasTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class GlasTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-glas";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-glas.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-glas.css";

        private static GlasTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static GlasTheme Instance => _instance ?? (_instance = new GlasTheme());

        private GlasTheme() : base("Glas", RootClass, StylesheetUrl) { }
    }
}
