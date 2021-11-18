namespace Tesserae
{
    [H5.Name("tss.txtX")]
    public static class TextBlockExtensions
    {
        public static T Text<T>(this T textBlock, string text) where T : TextBlock
        {
            textBlock.Text = text;
            return textBlock;
        }

        public static T Title<T>(this T textBlock, string title) where T : TextBlock
        {
            textBlock.Title = title;
            return textBlock;
        }

        public static T Required<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsRequired = true;
            return textBlock;
        }

        public static T Wrap<T>(this T textBlock) where T : TextBlock
        {
            textBlock.CanWrap = true;
            return textBlock;
        }

        public static T Ellipsis<T>(this T textBlock) where T : TextBlock
        {
            textBlock.EnableEllipsis = true;
            return textBlock;
        }

        public static T BreakSpaces<T>(this T textBlock) where T : TextBlock
        {
            textBlock.EnableBreakSpaces = true;
            return textBlock;
        }

        public static T NoWrap<T>(this T textBlock) where T : TextBlock
        {
            textBlock.CanWrap = false;
            return textBlock;
        }

        public static T Disabled<T>(this T textBlock, bool value = true) where T : TextBlock
        {
            textBlock.IsEnabled = !value;
            return textBlock;
        }

        public static T NonSelectable<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSelectable = false;
            return textBlock;
        }

        public static T Primary<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsPrimary = true;
            return textBlock;
        }

        public static T Success<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSuccess = true;
            return textBlock;
        }

        public static T Danger<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsDanger = true;
            return textBlock;
        }

        public static T Secondary<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSecondary = true;
            return textBlock;
        }
    }
}