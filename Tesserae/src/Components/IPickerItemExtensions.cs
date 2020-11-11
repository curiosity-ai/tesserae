namespace Tesserae
{
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