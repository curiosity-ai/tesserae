namespace Tesserae
{
    /// <summary>
    /// Defines an item that can be picked within a Picker component.
    /// </summary>
    [H5.Name("tss.IPickerItem")]
    public interface IPickerItem
    {
        /// <summary>Gets the name of the item.</summary>
        string Name { get; }

        /// <summary>Gets or sets whether the item is selected.</summary>
        bool IsSelected { get; set; }

        /// <summary>Renders the picker item component.</summary>
        /// <returns>The rendered component.</returns>
        IComponent Render();
    }
}