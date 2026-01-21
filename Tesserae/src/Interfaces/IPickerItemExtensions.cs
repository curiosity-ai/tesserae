namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for IPickerItem instances.
    /// </summary>
    [H5.Name("tss.IPickerItemX")]
    public static class IPickerItemExtensions
    {
        /// <summary>
        /// Selects the picker item if the specified condition is met.
        /// </summary>
        /// <typeparam name="T">The type of the picker item.</typeparam>
        /// <param name="source">The picker item.</param>
        /// <param name="shouldSelect">Whether the item should be selected.</param>
        /// <returns>The current instance of the type.</returns>
        public static T SelectedIf<T>(this T source, bool shouldSelect) where T : IPickerItem
        {
            if (shouldSelect)
            {
                source.IsSelected = true;
            }
            return source;
        }
    }
}