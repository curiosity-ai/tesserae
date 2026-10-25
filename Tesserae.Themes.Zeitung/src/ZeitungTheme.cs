namespace Tesserae.Themes.Zeitung
{
    /// <summary>
    /// A broadsheet-newspaper Tesserae custom theme. The light mode is the morning edition: warm off-white newsprint with a
    /// faint fibre, black ink and one spot red printed sparingly (the primary action, the "you are here" marks, focus). The
    /// dark mode is the night edition: the same pages on dark grey-black stock with ink-white type and a brighter red.
    ///
    /// <para>
    /// Typography carries the design: a variable serif with real small caps and old-style figures for text (Source Serif 4,
    /// bundled as "Zeitung Serif"), a condensed gothic for headlines and kickers (Oswald), a blackletter nameplate
    /// (UnifrakturMaguntia) and a typewriter for code (Courier Prime), all OFL and bundled. There are no shadows and no
    /// rounded corners: hairline column rules and the thick-and-thin double rule do the work, cards are boxed articles,
    /// tables are market listings with tabular figures, Markdown opens with a drop cap and sets blockquotes as pull quotes.
    /// </para>
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
