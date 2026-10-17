namespace Tesserae.Themes.Stratus
{
    /// <summary>
    /// An aerospace-grade design language as a Tesserae custom theme: a deep navy accent on cool-grey
    /// neutrals, filled inputs that carry their state on a bottom rule, small 3px corners, a tiered
    /// elevation scale instead of hairlines, and Inter for text with Roboto Mono for code. The palette,
    /// scale and component behaviour follow an open-source (Apache-2.0) aerospace design system, re-expressed as
    /// Tesserae stylesheets; the name and everything else here is this package's own.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(StratusTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, and follows
    /// <c>UI.Theme.Dark()</c> / <c>Light()</c>.
    /// </para>
    /// </summary>
    public sealed class StratusTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-stratus";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-stratus.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-stratus.css";

        private static StratusTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static StratusTheme Instance => _instance ?? (_instance = new StratusTheme());

        private StratusTheme() : base("Stratus", RootClass, StylesheetUrl) { }
    }
}
