using static Transpose.Core.dom;
using Range = Transpose.Core.dom.Range;

namespace Tesserae
{
    /// <summary>
    /// One keyword match reported by <see cref="MarkHighlighter.MarkAsync(HTMLElement, string, MarkOptions, System.Action{MarkedMatch}, System.Threading.CancellationToken)"/>.
    /// Depending on the backend that painted it, the match is either wrapped in mark elements or
    /// registered as a live range in the CSS custom highlight registry - exactly one of
    /// <see cref="Elements"/> and <see cref="Range"/> is set. Hand the match back to
    /// <see cref="MarkHighlighter.FocusResult(HTMLElement, MarkedMatch, bool)"/> to focus it.
    /// </summary>
    [Transpose.Name("tss.MarkedMatch")]
    public sealed class MarkedMatch
    {
        internal MarkedMatch() { }

        /// <summary>
        /// The wrapper elements of this match, in document order - one per text node the match
        /// crosses. Null when the match is painted through the CSS highlight registry.
        /// </summary>
        public HTMLElement[] Elements { get; internal set; }

        /// <summary>
        /// The live range painted through the CSS highlight registry. Null when the match is
        /// wrapped in elements (iframes, browsers without the API, or opted out).
        /// </summary>
        public Range Range { get; internal set; }

        /// <summary>The matched text.</summary>
        public string Text { get; internal set; }
    }
}
