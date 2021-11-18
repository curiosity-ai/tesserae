using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.ISCS")]
    public interface ISpecialCaseStyling
    {
        HTMLElement StylingContainer { get; }
        bool PropagateToStackItemParent { get; }
    }
}
