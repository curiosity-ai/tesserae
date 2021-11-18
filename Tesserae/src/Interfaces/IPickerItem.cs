namespace Tesserae
{
    [H5.Name("tss.IPickerItem")]
    public interface IPickerItem
    {
        string Name { get; }

        bool IsSelected { get; set; }

        IComponent Render();
    }
}