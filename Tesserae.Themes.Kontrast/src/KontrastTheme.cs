namespace Tesserae.Themes.Kontrast
{
    /// <summary>
    /// A high-contrast, accessibility-first Tesserae custom theme: AAA contrast, thick focus rings, underlined links, no meaning by colour alone, forced-colors aware.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(KontrastTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class KontrastTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-kontrast";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-kontrast.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-kontrast.css";

        private static KontrastTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static KontrastTheme Instance => _instance ?? (_instance = new KontrastTheme());

        private KontrastTheme() : base("Kontrast", RootClass, StylesheetUrl) { }
    }
}
