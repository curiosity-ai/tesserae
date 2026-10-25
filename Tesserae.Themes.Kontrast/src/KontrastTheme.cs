namespace Tesserae.Themes.Kontrast
{
    /// <summary>
    /// An accessibility-first Tesserae custom theme, drawn like a public wayfinding system: black ink on white board in
    /// light mode, white on black with one signal yellow in dark mode.
    ///
    /// <para>
    /// Every text pair is at least 7:1 (WCAG AAA) and every control edge and state at least 3:1. Edges are 2px, never
    /// hairlines; focus is a 6px two-tone ring (yellow and black) that shows on any background and is drawn inside a
    /// control's edge wherever a container would clip it; links are always underlined; selected, checked, current and
    /// invalid states change weight, shape or carry a bar, an icon or a pattern, not only a colour; disabled controls
    /// are hatched with a dashed edge and stay readable. Text is set in Atkinson Hyperlegible Next at 15px / 1.5, code in
    /// Atkinson Hyperlegible Mono (both bundled, SIL Open Font License). Motion is cut to quick state changes, and the
    /// theme answers <c>prefers-reduced-motion</c>, <c>prefers-contrast: more</c> and <c>forced-colors: active</c>.
    /// </para>
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
