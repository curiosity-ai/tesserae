namespace Tesserae
{
    public interface ISidebarItem 
    {
        IComponent RenderClosed();
        IComponent RenderOpen();
        bool IsSelected { get; set; }
        IComponent CurrentRendered { get; }
    }
}