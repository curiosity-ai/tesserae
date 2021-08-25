namespace Tesserae
{
    public interface IPickerItem
    {
        string Name { get; }

        bool IsSelected { get; set; }

        IComponent Render();
    }
}