namespace Tesserae
{
    [H5.Name("tss.IPickerItemX")]
    public static class IPickerItemExtensions
    {
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