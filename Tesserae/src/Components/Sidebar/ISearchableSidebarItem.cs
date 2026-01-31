namespace Tesserae
{
    public interface ISearchableSidebarItem : ISidebarItem
    {
        bool Search(string searchTerm);
    }
}
