namespace Tesserae.Themes.Memphis
{
    /// <summary>
    /// A loud, joyful, geometric Tesserae custom theme after the 1980s Memphis design movement: a box of clashing
    /// accents (bubblegum, mint, sunflower, cobalt, tomato, lilac) on cream, thick black outlines on every control,
    /// hard offset shadows in a second colour that buttons press into, accents rotating from card to card like
    /// confetti, and squiggle, confetti, zig-zag and speckle patterns on the surfaces around the content. Big type is
    /// set in the chunky Bungee display face, text in Outfit. Dark mode is the night club: deep aubergine, the
    /// accents turned to neon, the black lines turned cream.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(MemphisTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class MemphisTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-memphis";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-memphis.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-memphis.css";

        private static MemphisTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static MemphisTheme Instance => _instance ?? (_instance = new MemphisTheme());

        private MemphisTheme() : base("Memphis", RootClass, StylesheetUrl) { }
    }
}
