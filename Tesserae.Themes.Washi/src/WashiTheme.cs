namespace Tesserae.Themes.Washi
{
    /// <summary>
    /// A Japanese paper-and-ink Tesserae custom theme. Light mode is a sheet of kozo washi (warm off-white with a faint
    /// fibrous grain), sumi ink for type and lines, and one accent: vermilion, the red of a seal stamp, pressed onto
    /// selections and primary actions as a small square hanko with an uneven edge. Lines are drawn as tapering brush
    /// strokes, the sidebar is a shoji panel with a fine kumiko lattice, sheets lift on soft warm shadows, and the
    /// traditional dyes (indigo, moss, tea-gold, crimson) carry meaning sparingly. Dark mode is urushi lacquerware:
    /// black-brown lacquer with a sheen, rice-paper type, a brighter vermilion and gold (kintsugi) for focus.
    /// Headings in Shippori Mincho, text in Zen Kaku Gothic New, code in IBM Plex Mono (all OFL, bundled).
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(WashiTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class WashiTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-washi";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-washi.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-washi.css";

        private static WashiTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static WashiTheme Instance => _instance ?? (_instance = new WashiTheme());

        private WashiTheme() : base("Washi", RootClass, StylesheetUrl) { }
    }
}
