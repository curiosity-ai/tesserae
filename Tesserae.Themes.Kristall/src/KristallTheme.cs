namespace Tesserae.Themes.Kristall
{
    /// <summary>
    /// A desktop-glass Tesserae custom theme after the translucent macOS look: vibrancy sidebars and toolbars over a wallpaper, window chrome, system-blue accent and native-feeling controls.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(KristallTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, light and dark.
    /// </para>
    /// </summary>
    public sealed class KristallTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-kristall";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-kristall.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-kristall.css";

        private static KristallTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static KristallTheme Instance => _instance ?? (_instance = new KristallTheme());

        private KristallTheme() : base("Kristall", RootClass, StylesheetUrl) { }
    }
}
