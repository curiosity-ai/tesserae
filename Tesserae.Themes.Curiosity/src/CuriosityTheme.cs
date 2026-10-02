namespace Tesserae.Themes.Curiosity
{
    /// <summary>
    /// The Curiosity design language as a Tesserae custom theme: paper and ink, one electric blue (the
    /// Signal) and the deep call blue for primary actions, 1px hairlines, square corners with pill
    /// buttons and inputs, Schibsted Grotesk for text and Geist Mono for labels, numbers and code.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(CuriosityTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>, and follows
    /// <c>UI.Theme.Dark()</c> / <c>Light()</c>: the dark variant is the brand's ink canvas.
    /// </para>
    /// </summary>
    public sealed class CuriosityTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-curiosity";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-curiosity.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-curiosity.css";

        private static CuriosityTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static CuriosityTheme Instance => _instance ?? (_instance = new CuriosityTheme());

        private CuriosityTheme() : base("Curiosity", RootClass, StylesheetUrl) { }

        /// <summary>The scrollbar is ink only while its scroller moves, which the stylesheet cannot see on its own.</summary>
        protected override void OnActivated() => ScrollActivity.Start();

        protected override void OnDeactivated() => ScrollActivity.Stop();
    }
}
