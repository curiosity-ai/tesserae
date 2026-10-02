namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for components that support basic styling properties.
    /// </summary>
    [Transpose.Name("tss.ISX")]
    public static class StyleExtensions
    {
        /// <summary>
        /// Sets the background color for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="color">The background color.</param>
        /// <returns>The component instance.</returns>
        public static T Background<T>(this T element, string color) where T : IHasBackgroundColor
        {
            element.Background = color ?? "";
            return element;
        }

        /// <summary>
        /// Sets the foreground color for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The component instance.</returns>
        public static T Foreground<T>(this T element, string color) where T : IHasForegroundColor
        {
            element.Foreground = color ?? "";
            return element;
        }

        /// <summary>
        /// Sets the padding for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="padding">The padding value.</param>
        /// <returns>The component instance.</returns>
        public static T Padding<T>(this T element, string padding) where T : IHasMarginPadding
        {
            element.Padding = padding ?? "";
            return element;
        }

        /// <summary>
        /// Sets the margin for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="margin">The margin value.</param>
        /// <returns>The component instance.</returns>
        public static T Margin<T>(this T element, string margin) where T : IHasMarginPadding
        {
            element.Margin = margin ?? "";
            return element;
        }

        /// <summary>
        /// Applies one of the theme's radius levels to the component. The size of a level belongs to the theme,
        /// not to this call: Tesserae.Themes.Curiosity sets <see cref="BorderRadius.Small"/> and
        /// <see cref="BorderRadius.Medium"/> to 0, so there only <see cref="BorderRadius.Full"/> is round.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="radius">The theme radius level to apply.</param>
        /// <returns>The component instance.</returns>
        public static T Rounded<T>(this T element, BorderRadius radius = BorderRadius.Medium) where T : IRoundedStyle
        {
            var htmlElement = element.Render();
            htmlElement.classList.remove("tss-rounded-sm", "tss-rounded-md", "tss-rounded-full");

            switch (radius)
            {
                case BorderRadius.Small:
                    htmlElement.classList.add("tss-rounded-sm");
                    break;
                case BorderRadius.Medium:
                    htmlElement.classList.add("tss-rounded-md");
                    break;
                case BorderRadius.Full:
                    htmlElement.classList.add("tss-rounded-full");
                    break;
            }

            return element;
        }
    }
}