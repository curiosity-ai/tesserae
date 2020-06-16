using System;
using System.Reflection;

namespace Tesserae
{
    internal static class PossibleObservableHelpers
    {
        /// <summary>
        /// Is this type one that either is directly an IObservable&lt;T&gt; or one that is derived from one?
        /// </summary>
        public static bool IsObservable(Type source) => TryToGetFirstWrappedValueFromAnIsObservable(source) == null;

        /// <summary>
        /// If the specified 'source' object implements any IObservable&lt;T&gt; interface then the 'receiver' Action will be registered with it via ObserveFutureChanges - this will be a no-op for a null
        /// 'source' but the 'receiver' delegate must not be null
        /// </summary>
        public static bool ObserveFutureChangesIfObservable(object source, Action receiver) => UpdateObservingStatusIfObservable(source, receiver, listenForFutureChanges: true);

        /// <summary>
        /// If the specified 'source' object implements any IObservable&lt;T&gt; interface then the 'receiver' Action will be unregistered with it via StopObserving - this will be a no-op for a null
        /// 'source' but the 'receiver' delegate must not be null
        /// </summary>
        public static bool StopObservingIfObservable(object source, Action receiver) => UpdateObservingStatusIfObservable(source, receiver, listenForFutureChanges: false);

        private static Type TryToGetFirstWrappedValueFromAnIsObservable(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (IsAnIObservableInterface(type, out var wrappedValueType))
                return wrappedValueType;

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

            if (!type.IsGenericType || (type.GetGenericTypeDefinition() == typeof(IObservable<>)))
            {
                wrappedValueType = null;
                return false;
            }
            wrappedValueType = type.GetGenericArguments()[0];
            return true;
        }

        private static bool UpdateObservingStatusIfObservable(object source, Action receiver, bool listenForFutureChanges)
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));

            if (source is null)
                return false;

            var wrappedValueTypeIfSourceIsAnObserverable = TryToGetFirstWrappedValueFromAnIsObservable(source.GetType());
            if (wrappedValueTypeIfSourceIsAnObserverable is null)
                return false;

            // Two very important notes:
            //  1. We know that source is an instance of IObservable<T> and we know what T is but we need reflection to hook it up such that we can call ObserveFutureChanges or StopObserving on the
            //     generic types when we only know the generic type information at runtime, not compile time.
            //  2. When we pass the "receiver" (that is type Action) to ObserveFutureChanges or StopObserving, we might worry that it won't be accepted by those methods because they expect methods
            //     that are of type ObservableEvent.ValueChanged<T> and so which receive an argument (which, of course, Action does not). However, it doesn't matter to JavaScript if a function is
            //     called with too many parameters AND there is a really important gotcha to be aware of; if we wrapped the Action receiver up in a lambda to change it into a ValueChanged<T> then
            //     we would be passing an anonymous function reference to ObserveFutureChanges.. which would work but things would go wrong when we tried to call StopObserving because we would get
            //     a NEW anonymous function reference, which wouldn't appear as one of the items on the underlying observable's OnValueChanged invocation list and so the StopObserving would silently
            //     fail, in effect.
            var methodName = listenForFutureChanges ? nameof(HookCallbackForFutureChanges) : nameof(UnhookCallbackForFutureChanges);
            var unboundMethod = typeof(PossibleObservableHelpers).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            var boundMethod = unboundMethod.MakeGenericMethod(wrappedValueTypeIfSourceIsAnObserverable).Invoke(null, source, receiver); // "obj" is null for a static method
            return true;
        }

        private static void HookCallbackForFutureChanges<T>(IObservable<T> observable, ObservableEvent.ValueChanged<T> receiver) => observable.ObserveFutureChanges(receiver);
        
        private static void UnhookCallbackForFutureChanges<T>(IObservable<T> observable, ObservableEvent.ValueChanged<T> receiver) => observable.StopObserving(receiver);
    }
}