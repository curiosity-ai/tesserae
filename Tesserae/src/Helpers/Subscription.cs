using System;

namespace Tesserae
{
    /// <summary>
    /// A lightweight idempotent IDisposable returned from <see cref="IObservable{T}.Subscribe"/>. The release
    /// action runs at most once; subsequent Dispose calls are no-ops.
    /// </summary>
    [H5.Name("tss.Subscription")]
    internal sealed class Subscription : IDisposable
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
    }
}
