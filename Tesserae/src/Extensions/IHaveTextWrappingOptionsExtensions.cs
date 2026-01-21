namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for components that support text wrapping options.
    /// </summary>
    [H5.Name("tss.IHTWOX")]
    public static class IHaveTextWrappingOptionsExtensions
    {
        /// <summary>
        /// Sets whether the text within the component can wrap.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="component">The component.</param>
        /// <param name="canWrap">Whether text can wrap.</param>
        /// <returns>The component instance.</returns>
        public static T SetCanWrap<T>(this T component, bool canWrap) where T : ICanWrap
        {
            component.CanWrap = canWrap;
            return component;
        }
    }
}