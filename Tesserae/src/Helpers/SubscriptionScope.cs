using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// An explicit container for IDisposable subscriptions returned by <see cref="IObservable{T}.Subscribe"/>.
    /// Dispose the scope to release every subscription added to it; the scope itself is idempotent. Adding to
    /// a disposed scope disposes the new subscription immediately.
    /// </summary>
    [Transpose.Name("tss.SubscriptionScope")]
    public sealed class SubscriptionScope : IDisposable
    {
        private List<IDisposable> _items = new List<IDisposable>();
        private bool              _disposed;

        /// <summary>
        /// Adds a subscription handle to the scope. Returns the same handle for chaining. If the scope is
        /// already disposed, the handle is disposed immediately and returned.
        /// </summary>
        public IDisposable Add(IDisposable sub)
        {
            if (sub == null) return null;
            if (_disposed)
            {
                sub.Dispose();
                return sub;
            }
            _items.Add(sub);
            return sub;
        }

        /// <summary>
        /// Releases all subscriptions currently in the scope, leaving the scope usable for new subscriptions.
        /// </summary>
        public void Clear()
        {
            if (_disposed) return;
            var items = _items;
            _items = new List<IDisposable>();
            for (int i = 0; i < items.Count; i++) items[i].Dispose();
        }

        /// <summary>
        /// Releases all subscriptions in the scope. Subsequent adds will be disposed immediately.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            var items = _items;
            _items = null;
            for (int i = 0; i < items.Count; i++) items[i].Dispose();
        }
    }

    /// <summary>
    /// Extension methods for working with <see cref="SubscriptionScope"/>.
    /// </summary>
    [Transpose.Name("tss.SSX")]
    public static class SubscriptionScopeExtensions
    {
        /// <summary>
        /// Subscribes to <paramref name="observable"/> and registers the resulting disposable with
        /// <paramref name="scope"/>. The subscription is released when the scope is disposed or cleared.
        /// </summary>
        public static IDisposable SubscribeTo<T>(this IObservable<T> observable, SubscriptionScope scope, Action<T> callback, bool fireImmediately = true)
        {
            return scope.Add(observable.Subscribe(callback, fireImmediately));
        }
    }
}
