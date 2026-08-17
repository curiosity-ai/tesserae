using System;

namespace Tesserae
{
    /// <summary>
    /// Provides helper methods for working with objects that may implement the IObservable interface.
    /// </summary>
    [Transpose.Name("tss.PossibleObservableHelpers")]
    internal static class PossibleObservableHelpers
    {
        /// <summary>
        /// Is this type one that either is directly an IObservable&lt;T&gt; or one that is derived from one?
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an observable, false otherwise.</returns>
        public static bool IsObservable(Type type) => TryToGetFirstWrappedValueFromAnIsObservable(type) is object;

        /// <summary>
        /// If the specified 'source' object implements any IObservable&lt;T&gt; interface then the 'receiver' Action will be registered with it via ObserveFutureChanges - this will be a no-op for a null
        /// 'source' but the 'receiver' delegate must not be null
        /// </summary>
        /// <param name="source">The source object to observe.</param>
        /// <param name="receiver">The callback to execute on changes.</param>
        /// <returns>True if the source was an observable and the receiver was registered, false otherwise.</returns>
        public static bool ObserveFutureChangesIfObservable(object source, Action receiver) => UpdateObservingStatusIfObservable(source, receiver, listenForFutureChanges: true);

        /// <summary>
        /// If the specified 'source' object implements any IObservable&lt;T&gt; interface then the 'receiver' Action will be unregistered with it via StopObserving - this will be a no-op for a null
        /// 'source' but the 'receiver' delegate must not be null
        /// </summary>
        /// <param name="source">The source object to stop observing.</param>
        /// <param name="receiver">The callback to unregister.</param>
        /// <returns>True if the source was an observable and the receiver was unregistered, false otherwise.</returns>
        public static bool StopObservingIfObservable(object source, Action receiver) => UpdateObservingStatusIfObservable(source, receiver, listenForFutureChanges: false);

        private static Type TryToGetFirstWrappedValueFromAnIsObservable(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (IsAnIObservableInterface(type, out var wrappedValueType)) return wrappedValueType;

            foreach (var i in type.GetInterfaces())
            {
                if (IsAnIObservableInterface(i, out wrappedValueType))
                    return wrappedValueType;
            }
            return null;
        }

        private static bool IsAnIObservableInterface(Type type, out Type wrappedValueType)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IObservable<>)))
            {
                wrappedValueType = type.GetGenericArguments()[0];
                return true;
            }
            wrappedValueType = null;
            return false;
        }

        private static bool UpdateObservingStatusIfObservable(object source, Action receiver, bool listenForFutureChanges)
        {
            if (receiver is null) throw new ArgumentNullException(nameof(receiver));
            if (source is null) return false;

            if (TryToGetFirstWrappedValueFromAnIsObservable(source.GetType()) is null)
                return false;

            // The source's T is only known at runtime, but it does not need to be known at all: every
            // member of IObservable<T> compiles to a single name that does not carry the type argument
            // (tss$IOBS$1$ObserveFutureChanges), so a call through any closed IObservable<> reaches the
            // same method on the instance. As<T>() is the compiler's no-op reinterpretation, which
            // erases the type argument here without a runtime check.
            //
            // This used to go through GetMethod + MakeGenericMethod + Invoke on a pair of generic
            // helpers. That never worked: binding the ObservableEvent.ValueChanged<T> parameter needs
            // the generic delegate as a runtime type, and reflection cannot produce one, so every call
            // threw and constructing an ObservableList<T> over an observable T threw with it.
            //
            // The receiver is handed over as-is rather than wrapped in a lambda that drops the value
            // argument: JavaScript does not mind a callback declared with fewer parameters than it is
            // called with, and a wrapper would be a fresh function reference on each call, so
            // StopObserving would never find the registration it was asked to remove.
            var observable = source.As<IObservable<object>>();
            var callback   = receiver.As<ObservableEvent.ValueChanged<object>>();

            if (listenForFutureChanges) observable.ObserveFutureChanges(callback);
            else observable.StopObserving(callback);

            return true;
        }
    }
}