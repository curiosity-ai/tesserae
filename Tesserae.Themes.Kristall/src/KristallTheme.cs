namespace Tesserae.Themes.Kristall
{
    /// <summary>
    /// A desktop-glass Tesserae custom theme after the translucent macOS desktop look: the page is a soft, static
    /// wallpaper with the window's content pane laid over it, the sidebar a frosted source list with an accent
    /// selection pill, menus, popovers, toasts and sheets frosted rounded panels lit by a 1px inner stroke, and the
    /// controls native: capsule push buttons with a blue default button, borderless toolbar buttons that answer the
    /// pointer with a glass capsule, pop-up buttons with a double chevron, the accent switch, segmented controls and a
    /// translucent accent focus halo. Dark mode is the dark appearance: graphite glass over a night wallpaper. Inter
    /// and JetBrains Mono are bundled (SIL OFL 1.1).
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
