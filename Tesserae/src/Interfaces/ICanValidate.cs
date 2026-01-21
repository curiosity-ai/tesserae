namespace Tesserae
{
    /// <summary>
    /// Defines a typed component that supports validation.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    [H5.Name("tss.ICVT")]
    public interface ICanValidate<T> : ICanValidate where T : IComponent
    {
        /// <summary>Attaches a validation handler to the component.</summary>
        /// <param name="handler">The event handler.</param>
        void Attach(ComponentEventHandler<T> handler);
    }

    /// <summary>
    /// Defines a component that supports validation.
    /// </summary>
    [H5.Name("tss.ICV")]
    public interface ICanValidate : IComponent
    {
        /// <summary>Gets or sets the validation error message.</summary>
        string Error     { get; set; }
        /// <summary>Gets or sets whether the component is in an invalid state.</summary>
        bool   IsInvalid { get; set; }
    }
}