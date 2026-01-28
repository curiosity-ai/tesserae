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
        public static T Background<T>(this T element, string color) where T : IComponent
        {
            if (element is IHasBackgroundColor hasBg)
            {
                hasBg.Background = color ?? "";
            }
            else
            {
                var (el, _) = Stack.GetCorrectItemToApplyStyle(element);
                el.style.background = color ?? "";
            }
            return element;
        }

        /// <summary>
        /// Sets the foreground color for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="color">The foreground color.</param>
        /// <returns>The component instance.</returns>
        public static T Foreground<T>(this T element, string color) where T : IComponent
        {
            if (element is IHasForegroundColor hasFg)
            {
                hasFg.Foreground = color ?? "";
            }
            else
            {
                var (el, _) = Stack.GetCorrectItemToApplyStyle(element);
                el.style.color = color ?? "";
            }
            return element;
        }

        /// <summary>
        /// Sets the padding for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="padding">The padding value.</param>
        /// <returns>The component instance.</returns>
        public static T Padding<T>(this T element, string padding) where T : IComponent
        {
            if (element is IHasMarginPadding hasMarginPadding)
            {
                hasMarginPadding.Padding = padding ?? "";
            }
            else
            {
                var (el, _) = Stack.GetCorrectItemToApplyStyle(element);
                el.style.padding = padding ?? "";
            }
            return element;
        }

        /// <summary>
        /// Sets the margin for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="margin">The margin value.</param>
        /// <returns>The component instance.</returns>
        public static T Margin<T>(this T element, string margin) where T : IComponent
        {
            if (element is IHasMarginPadding hasMarginPadding)
            {
                hasMarginPadding.Margin = margin ?? "";
            }
            else
            {
                var (el, _) = Stack.GetCorrectItemToApplyStyle(element);
                el.style.margin = margin ?? "";
            }
            return element;
        }

        /// <summary>
        /// Sets the border radius for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="element">The component.</param>
        /// <param name="radius">The border radius value.</param>
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