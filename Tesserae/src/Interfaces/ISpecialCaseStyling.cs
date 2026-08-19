using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Defines a component that requires special case styling,
    /// often involving exposing its styling container and controlling style propagation.
    /// </summary>
    [Transpose.Name("tss.ISCS")]
    public interface ISpecialCaseStyling
    {
        /// <summary>Gets the HTMLElement that should receive styling.</summary>
        HTMLElement StylingContainer         { get; }
        // Stack and Grid no longer wrap their children, so there is no stack-item parent to
        // propagate to — this now only decides whether a sizing helper also tags the element with
        // the tss-stk-*/tss-grd-* markers. Masonry, SectionStack and KeyedObservableStack still
        // build real wrappers, and CopyStylesDefinedWithExtension reads those markers to move the
        // property onto the wrapper. A component that sizes its own container says false.
        /// <summary>Gets whether a sizing helper applied to this component should also tag it, so a wrapper-building container hoists the style onto the wrapper.</summary>
        bool        PropagateStylesToWrapper { get; }
    }
}