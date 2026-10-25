namespace Tesserae.Themes.Blaupause
{
    /// <summary>
    /// An engineering-blueprint Tesserae custom theme: Prussian-blue drafting paper with a faint grid, white linework, dimension-line borders and stencil capitals.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(BlaupauseTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class BlaupauseTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-blaupause";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-blaupause.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-blaupause.css";

        private static BlaupauseTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static BlaupauseTheme Instance => _instance ?? (_instance = new BlaupauseTheme());

        private BlaupauseTheme() : base("Blaupause", RootClass, StylesheetUrl) { }
    }
}
