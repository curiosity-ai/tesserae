namespace Tesserae.Components
{
    public interface IPickerItem
    {
        string Name { get; }

        bool IsSelected { get; set; }

        IComponent Render();
    }
}