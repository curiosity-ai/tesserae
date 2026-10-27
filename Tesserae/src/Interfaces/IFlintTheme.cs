using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// The visual theme a <see href="https://github.com/microsoft/flint-chart">Flint</see> chart in Markdown is drawn
    /// with: what Flint calls its <c>theme_spec</c>. <see cref="FlintThemes"/> has the ones Tesserae provides - the
    /// presets Flint ships, a theme drawn from the page's own Tesserae tokens, and a theme written as JSON.
    /// </summary>
    /// <remarks>
    /// The theme is asked again whenever the Tesserae theme changes (light/dark mode, a custom theme), and the chart
    /// is drawn again only when the answer is different, so a theme that reads the page's tokens follows the page and
    /// a fixed one costs nothing.
    /// </remarks>
    public interface IFlintTheme
    {
        /// <summary>
        /// The <c>theme_spec</c> for a chart drawn inside <paramref name="host"/>, as JSON: a quoted preset id
        /// (<c>"economist"</c>) or a ThemeSpec object, which may <c>extends</c> a preset and state only what differs.
        /// Flint reads colours as hex only (<c>#rrggbb</c>), so a theme that takes them from the page resolves them first.
        /// </summary>
        /// <param name="host">The element the chart is drawn in, already on the page, for a theme that reads computed styles.</param>
        string ThemeSpecJson(HTMLElement host);
    }
}
