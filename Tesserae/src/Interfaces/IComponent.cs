using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Defines the base interface for all components in the Tesserae framework.
    /// </summary>
    [Transpose.Name("tss.IC")]
    public interface IComponent
    {
        /// <summary>Renders the component into an HTMLElement.</summary>
        /// <returns>The rendered HTMLElement.</returns>
        HTMLElement Render();
    }
}