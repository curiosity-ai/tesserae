namespace Tesserae.Themes.Rechenwerk
{
    /// <summary>
    /// The record sleeves of the German electronic pioneers of the 1970s and 80s as a Tesserae custom theme:
    /// the red, black and cream constructivism of <i>Die Mensch-Maschine</i> in light mode, the black terminal
    /// and signal yellow of <i>Computerwelt</i> in dark mode. Square machine-made geometry, heavy ink rules, hard
    /// offset shadows, keys that press in, tracked capitals for every piece of chrome and dot-matrix numerals.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(RechenwerkTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>. <c>UI.Theme.Dark()</c> switches
    /// from the poster (cream paper, red primary) to the terminal (black screen, yellow primary).
    /// </para>
    /// </summary>
    public sealed class RechenwerkTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-rechenwerk";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-rechenwerk.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-rechenwerk.css";

        private static RechenwerkTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static RechenwerkTheme Instance => _instance ?? (_instance = new RechenwerkTheme());

        private RechenwerkTheme() : base("Rechenwerk", RootClass, StylesheetUrl) { }
    }
}
