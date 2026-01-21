namespace Tesserae
{
    /// <summary>
    /// Extension methods for <see cref="TextBlock"/>.
    /// </summary>
    [H5.Name("tss.txtX")]
    public static class TextBlockExtensions
    {
        /// <summary>
        /// Sets the text of the text block.
        /// </summary>
        /// <typeparam name="T">The text block type.</typeparam>
        /// <param name="textBlock">The text block.</param>
        /// <param name="text">The text.</param>
        /// <returns>The text block instance.</returns>
        public static T Text<T>(this T textBlock, string text) where T : TextBlock
        {
            textBlock.Text = text;
            return textBlock;
        }

        /// <summary>
        /// Sets the title of the text block.
        /// </summary>
        /// <typeparam name="T">The text block type.</typeparam>
        /// <param name="textBlock">The text block.</param>
        /// <param name="title">The title.</param>
        /// <returns>The text block instance.</returns>
        public static T Title<T>(this T textBlock, string title) where T : TextBlock
        {
            textBlock.Title = title;
            return textBlock;
        }

        /// <summary>Sets the text block as required.</summary>
        public static T Required<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsRequired = true;
            return textBlock;
        }

        /// <summary>Enables text wrapping.</summary>
        public static T Wrap<T>(this T textBlock) where T : TextBlock
        {
            textBlock.CanWrap = true;
            return textBlock;
        }

        /// <summary>Enables ellipsis for overflowing text.</summary>
        public static T Ellipsis<T>(this T textBlock) where T : TextBlock
        {
            textBlock.EnableEllipsis = true;
            return textBlock;
        }

        /// <summary>Enables break-spaces.</summary>
        public static T BreakSpaces<T>(this T textBlock) where T : TextBlock
        {
            textBlock.EnableBreakSpaces = true;
            return textBlock;
        }

        /// <summary>Disables text wrapping.</summary>
        public static T NoWrap<T>(this T textBlock) where T : TextBlock
        {
            textBlock.CanWrap = false;
            return textBlock;
        }

        /// <summary>Sets whether the text block is disabled.</summary>
        public static T Disabled<T>(this T textBlock, bool value = true) where T : TextBlock
        {
            textBlock.IsEnabled = !value;
            return textBlock;
        }

        /// <summary>Sets the text as non-selectable.</summary>
        public static T NonSelectable<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSelectable = false;
            return textBlock;
        }

        /// <summary>Sets the text to primary color.</summary>
        public static T Primary<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsPrimary = true;
            return textBlock;
        }

        /// <summary>Sets the text to success color.</summary>
        public static T Success<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSuccess = true;
            return textBlock;
        }

        /// <summary>Sets the text to danger color.</summary>
        public static T Danger<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsDanger = true;
            return textBlock;
        }

        /// <summary>Sets the text to secondary color.</summary>
        public static T Secondary<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSecondary = true;
            return textBlock;
        }
    }
}