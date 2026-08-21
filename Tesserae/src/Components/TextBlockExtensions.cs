namespace Tesserae
{
    /// <summary>
    /// Extension methods for <see cref="TextBlock"/>.
    /// </summary>
    [Transpose.Name("tss.txtX")]
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

        /// <summary>
        /// Paints the text with the AI gradient - what marks a line as a model's words rather than the
        /// app's. It is for the short pieces: a title, a heading over generated output, a one-line summary.
        /// A paragraph wants <see cref="AISurface{T}"/> instead, which keeps the text readable and marks
        /// the block around it.
        /// </summary>
        public static T AI<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Render().classList.add("tss-ai");
            return textBlock;
        }

        /// <summary>
        /// Draws the text as generated prose: the theme's own text colour on a faint purple-to-blue panel
        /// with an accent edge down the left. The block is marked as the model's output without making any
        /// of the words harder to read, which is what a gradient over a paragraph would do.
        /// </summary>
        public static T AISurface<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Render().classList.add("tss-ai-surface");
            return textBlock;
        }

        /// <summary>Adds a glow effect to the text.</summary>
        public static T Glow<T>(this T textBlock, string color = null) where T : TextBlock
        {
            var el = textBlock.Render();
            el.classList.add("tss-text-glow");

            if (!string.IsNullOrEmpty(color))
            {
                el.style.setProperty("--tss-text-glow-color", color);
            }
            else
            {
                el.style.removeProperty("--tss-text-glow-color");
            }

            return textBlock;
        }
    }
}