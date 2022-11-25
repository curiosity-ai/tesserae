using static H5.Core.dom;

namespace Tesserae
{
    public interface IHasClickHandler
    {
        void OnClickBase(ComponentEventHandler<IComponent, MouseEvent> onClick, bool clearPrevious = true);
        void OnContextMenuBase(ComponentEventHandler<IComponent, MouseEvent> onContextMenu, bool clearPrevious = true);
    }
}