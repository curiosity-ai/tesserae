using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Provides two-way binding between a component implementing
    /// <see cref="IBindableComponent{T}"/> and a <see cref="SettableObservable{T}"/>.
    /// </summary>
    [Transpose.Name("tss.Bind")]
    public static class BindingExtensions
    {
        /// <summary>
        /// Wires the component to a single source of truth: user input flows into <paramref name="source"/>,
        /// and programmatic changes to <paramref name="source"/> are reflected in the component.
        /// The component is seeded from <paramref name="source"/>.Value at call time — any prior
        /// SetValue/SetText/IsChecked configuration on the component is overwritten.
        /// The binding lives for the lifetime of <paramref name="component"/> and <paramref name="source"/>;
        /// use the <see cref="SubscriptionScope"/> overload if you need to tear it down earlier.
        /// </summary>
        public static TComponent Bind<TComponent, T>(this TComponent component, SettableObservable<T> source)
            where TComponent : IBindableComponent<T>
            => Bind(component, source, scope: null);

        /// <summary>
        /// Two-way binding overload that registers both subscriptions with <paramref name="scope"/>;
        /// disposing the scope releases the binding.
        /// </summary>
        public static TComponent Bind<TComponent, T>(this TComponent component, SettableObservable<T> source, SubscriptionScope scope)
            where TComponent : IBindableComponent<T>
        {
            var fromComponent = component.AsObservable().Subscribe(v => source.Value = v, fireImmediately: false);
            var fromSource    = source.Subscribe(v => component.SetBoundValue(v), fireImmediately: true);

            if (scope != null)
            {
                scope.Add(fromComponent);
                scope.Add(fromSource);
            }

            return component;
        }

        /// <summary>
        /// List-shaped two-way binding: wires a component implementing <see cref="IBindableListComponent{T}"/>
        /// to a <c>SettableObservable&lt;IReadOnlyList&lt;T&gt;&gt;</c>. User edits flow into <paramref name="source"/>,
        /// and programmatic changes to <paramref name="source"/> replace the component's items.
        /// The component is seeded from <paramref name="source"/>.Value at call time.
        /// </summary>
        public static TComponent Bind<TComponent, T>(this TComponent component, SettableObservable<IReadOnlyList<T>> source)
            where TComponent : IBindableListComponent<T>
            => Bind(component, source, scope: null);

        /// <summary>
        /// List-shaped two-way binding overload that registers both subscriptions with <paramref name="scope"/>;
        /// disposing the scope releases the binding.
        /// </summary>
        public static TComponent Bind<TComponent, T>(this TComponent component, SettableObservable<IReadOnlyList<T>> source, SubscriptionScope scope)
            where TComponent : IBindableListComponent<T>
        {
            var fromComponent = component.AsObservable().Subscribe(v => source.Value = v,     fireImmediately: false);
            var fromSource    = source.Subscribe(            v => component.SetBoundValues(v), fireImmediately: true);

            if (scope != null)
            {
                scope.Add(fromComponent);
                scope.Add(fromSource);
            }

            return component;
        }
    }
}