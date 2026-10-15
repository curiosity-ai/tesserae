using System;

namespace Tesserae
{
    /// <summary>
    /// A lightweight idempotent IDisposable returned from <see cref="IObservable{T}.Subscribe"/>. The release
    /// action runs at most once; subsequent Dispose calls are no-ops.
    /// </summary>
    [Transpose.Name("tss.Subscription")]
    public sealed class Subscription : IDisposable
    {
        /// <summary>An IDisposable whose Dispose is a no-op. Returned from observables that never change.</summary>
        public static readonly IDisposable Empty = new Subscription(() => { });

        private Action _release;

        public Subscription(Action release)
        {
            _release = release;
        }

        public void Dispose()
        {
            var r = _release;
            _release = null;
            if (r != null) r();
        }

        /// <summary>
        /// Calls <paramref name="onChange"/> on every future change to <paramref name="source"/> (through
        /// ObserveFutureChanges) until the returned subscription is disposed, which passes the same handler to
        /// StopObserving.
        /// </summary>
        internal static IDisposable OnFutureChanges<T>(IObservable<T> source, Action onChange)
        {
            ObservableEvent.ValueChanged<T> handler = _ => onChange();
            source.ObserveFutureChanges(handler);
            return new Subscription(() => source.StopObserving(handler));
        }
    }
}
