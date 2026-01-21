namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for components that support basic styling properties.
    /// </summary>
    [H5.Name("tss.ISX")]
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
    }
}