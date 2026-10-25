namespace Tesserae.Themes.Blaupause
{
    /// <summary>
    /// Engineering drawings as a Tesserae custom theme. Dark mode is the classic cyanotype blueprint: Prussian-blue
    /// paper with a fine and a coarse drafting grid and a little chalky grain, white linework. Light mode is the
    /// whiteprint (diazo) of the same sheet: off-white drafting paper with blue-ink lines and grid. Nothing casts a
    /// shadow: borders are 1px technical lines, panels carry registration ticks in their corners, separators, tabs,
    /// sliders and progress bars are dimension lines and scales, disabled and ghost parts are drawn in the dashed
    /// hidden-line style, card headers and modal footers are title blocks, and anything lifted off the page repeats
    /// its own outline 5px down and right. Lettering is a squared technical sans (Saira) in capitals for chrome,
    /// stencil capitals (Saira Stencil One) for titles and IBM Plex Mono for every measurement. The red pencil marks
    /// danger and focus; the yellow highlighter marks the current selection.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(BlaupauseTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>. <c>UI.Theme.Dark()</c> switches
    /// from the whiteprint to the blueprint; the red pencil and the highlighter stay.
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
