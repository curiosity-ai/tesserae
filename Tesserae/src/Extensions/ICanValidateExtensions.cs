namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for ICanValidate components.
    /// </summary>
    [H5.Name("tss.ICVX")]
    public static class ICanValidateExtensions
    {
        /// <summary>
        /// Sets the validation error message for the component.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="component">The component.</param>
        /// <param name="error">The error message.</param>
        /// <returns>The component instance.</returns>
        public static T Error<T>(this T component, string error) where T : ICanValidate
        {
            component.Error = error;
            return component;
        }

        /// <summary>
        /// Sets whether the component is in an invalid state.
        /// </summary>
        /// <typeparam name="T">The type of the component.</typeparam>
        /// <param name="component">The component.</param>
        /// <param name="isInvalid">Whether the component is invalid.</param>
        /// <returns>The component instance.</returns>
        public static T IsInvalid<T>(this T component, bool isInvalid = true) where T : ICanValidate
        {
            component.IsInvalid = isInvalid;
            return component;
        }
    }
}