namespace Tesserae.Themes.Zeitung
{
    /// <summary>
    /// A broadsheet-newspaper Tesserae custom theme: off-white newsprint, serif text, hairline column rules, small-caps labels and one spot red.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(ZeitungTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class ZeitungTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-zeitung";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-zeitung.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-zeitung.css";

        private static ZeitungTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static ZeitungTheme Instance => _instance ?? (_instance = new ZeitungTheme());

        private ZeitungTheme() : base("Zeitung", RootClass, StylesheetUrl) { }
    }
}
