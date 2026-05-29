namespace Tesserae
{
    /// <summary>
    /// Provides two-way binding between a component implementing
    /// <see cref="IBindableComponent{T}"/> and a <see cref="SettableObservable{T}"/>.
    /// </summary>
    [H5.Name("tss.Bind")]
    public static class BindingExtensions
    {
        /// <summary>
        /// Wires the component to a single source of truth: user input flows into <paramref name="source"/>,
        /// and programmatic changes to <paramref name="source"/> are reflected in the component.
        /// The component is seeded from <paramref name="source"/>.Value at call time — any prior
        /// SetValue/SetText/IsChecked configuration on the component is overwritten.
        /// </summary>
        public static TComponent Bind<TComponent, T>(this TComponent component, SettableObservable<T> source)
            where TComponent : IBindableComponent<T>
        {
            component.SetBoundValue(source.Value);

            component.AsObservable().ObserveFutureChanges(v => source.Value = v);
            source.ObserveFutureChanges(v => component.SetBoundValue(v));

            return component;
        }
    }
}
