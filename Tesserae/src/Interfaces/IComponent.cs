using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.IC")]
    public interface IComponent
    {
        HTMLElement Render();
    }
}
