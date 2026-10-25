namespace Tesserae.Themes.Weniger
{
    /// <summary>
    /// West German industrial design of the 1950s to 80s as a skeuomorphic Tesserae custom theme: the radios,
    /// record players, calculators and clocks of the "less, but better" school. Warm light-grey plastic with a
    /// hairline groove inside every panel, brushed aluminium for knobs, thumbs and caps, keys that stand proud
    /// and press in, a signal orange for the one control that matters, green indicator LEDs and LCD displays with
    /// segment numerals. Dark mode is the charcoal version of the same devices.
    ///
    /// <para>
    /// Activate it through <see cref="UI.Theme.SetCustomTheme(ICustomTheme)"/>:
    /// </para>
    /// <code>
    /// await UI.Theme.SetCustomTheme(WenigerTheme.Instance);
    /// </code>
    /// <para>
    /// Every rule is scoped under <see cref="RootClass"/> on <c>document.body</c>. <c>UI.Theme.Dark()</c> switches
    /// from the light-grey plastic to the charcoal one; the orange and the LCDs stay.
    /// </para>
    /// </summary>
    public sealed class WenigerTheme : CustomTheme
    {
        /// <summary>The class this theme puts on <c>document.body</c> while it is active.</summary>
        public const string RootClass = "tss-theme-weniger";

        /// <summary>
        /// Where the combined stylesheet lands in the application's output: the resource group
        /// <c>tss-theme-weniger.css</c> in this package's <c>tps.json</c>, written to <c>assets/css</c>
        /// and not linked from <c>index.html</c> (<c>"load": false</c>).
        /// </summary>
        public const string StylesheetUrl = "assets/css/tss-theme-weniger.css";

        private static WenigerTheme _instance;

        /// <summary>The one instance: a theme is a switch on the body, so there is nothing to have two of.</summary>
        public static WenigerTheme Instance => _instance ?? (_instance = new WenigerTheme());

        private WenigerTheme() : base("Weniger", RootClass, StylesheetUrl) { }
    }
}
