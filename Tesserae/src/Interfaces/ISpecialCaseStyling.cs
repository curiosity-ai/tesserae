using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Defines a component that requires special case styling,
    /// often involving exposing its styling container and controlling style propagation.
    /// </summary>
    [H5.Name("tss.ISCS")]
    public interface ISpecialCaseStyling
    {
        /// <summary>Gets the HTMLElement that should receive styling.</summary>
        HTMLElement StylingContainer           { get; }
        /// <summary>Gets whether styling should propagate to the stack item parent.</summary>
        bool        PropagateToStackItemParent { get; }
    }
}