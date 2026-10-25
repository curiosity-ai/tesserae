namespace Tesserae.Themes.Phosphor
{
    /// <summary>
    /// A character-terminal Tesserae custom theme. Dark mode is the screen: green P1 phosphor on near-black glass,
    /// every glyph blooming slightly, faint scanlines and a vignette over the tube, amber P3 as the second colour.
    /// Light mode is the printout: green-bar continuous-feed paper with a tractor-feed margin, the same characters in
    /// carbon-ribbon ink. Both share one monospace face (IBM Plex Mono, with VT323 for headlines), box-drawing frames
    /// (single for fields and cells, double for windows, a window's title set into its top border), square corners,
    /// bracketed "[ OK ]" buttons, prompt-line fields with a block caret, and inverse video for focus, selection and
    /// every "current" state.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(PhosphorTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class PhosphorTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-phosphor";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-phosphor.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-phosphor.css";

        private static PhosphorTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static PhosphorTheme Instance => _instance ?? (_instance = new PhosphorTheme());

        private PhosphorTheme() : base("Phosphor", RootClass, StylesheetUrl) { }
    }
}
