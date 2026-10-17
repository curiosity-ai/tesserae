namespace Tesserae.Themes.RetroOS
{
    /// <summary>
    /// The original Macintosh interface (System 1 to 6, 1984-1991) as a Tesserae custom theme: strictly
    /// black and white, with the 50% grey dither standing in for every mid-tone, pinstriped title bars,
    /// hard one-pixel drop shadows, inverted selection and a pixel font where the Mac had Chicago.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(RetroOSTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>. <c>UI.Theme.Dark()</c>
    /// is reverse video: white ink on a black screen, the way the Mac's own "invert" accessibility
    /// option showed it.
    /// </para>
    /// </summary>
    public sealed class RetroOSTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-retroos";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-retroos.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-retroos.css";

        private static RetroOSTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static RetroOSTheme Instance => _instance ?? (_instance = new RetroOSTheme());

        private RetroOSTheme() : base("RetroOS", RootClass, StylesheetUrl) { }
    }
}
