using static H5.Core.dom;

namespace Tesserae
{
    public interface ISpecialCaseStyling
    {
        HTMLElement StylingContainer { get; }
        bool PropagateToStackItemParent { get; }
    }
}
