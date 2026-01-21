using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Defines a component that has a click handler.
    /// </summary>
    public interface IHasClickHandler
    {
        /// <summary>Sets the click handler for the component.</summary>
        /// <param name="onClick">The click event handler.</param>
        /// <param name="clearPrevious">Whether to clear previous click handlers.</param>
        void OnClickBase(ComponentEventHandler<IComponent, MouseEvent>       onClick,       bool clearPrevious = true);
        /// <summary>Sets the context menu handler for the component.</summary>
        /// <param name="onContextMenu">The context menu event handler.</param>
        /// <param name="clearPrevious">Whether to clear previous handlers.</param>
        void OnContextMenuBase(ComponentEventHandler<IComponent, MouseEvent> onContextMenu, bool clearPrevious = true);
    }
}