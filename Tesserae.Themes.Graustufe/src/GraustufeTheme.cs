namespace Tesserae.Themes.Graustufe
{
    /// <summary>
    /// An e-paper reader Tesserae custom theme: a 16-level grey ramp on a slightly warm paper and nothing else (the
    /// toolkit's palette, its AI tone, pictures and inline colours are all shown as greys), crisp 1px ink rules, square
    /// corners, shadows replaced by 1-bit dithered drop edges, selection inverted and focus a thick ink frame, text set
    /// in Literata with Atkinson Hyperlegible Next for chrome. Dark mode is the reader's night mode, the same ramp
    /// turned over.
    ///
    /// <para>
    /// Nothing moves: every transition and animation is off while the theme is active, and spinners, indeterminate
    /// bars and skeletons have still replacements. Tippy's popups keep a 1ms visibility transition, because Tippy
    /// unmounts a hidden popup on <c>transitionend</c>.
    /// </para>
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(GraustufeTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class GraustufeTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-graustufe";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-graustufe.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-graustufe.css";

        private static GraustufeTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static GraustufeTheme Instance => _instance ?? (_instance = new GraustufeTheme());

        private GraustufeTheme() : base("Graustufe", RootClass, StylesheetUrl) { }
    }
}
