using System;
using System.Linq;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Wires the observables behind a <see cref="DeferedComponent"/> or <see cref="DeferedComponentWithProgress"/>
    /// to its refresh: subscribes once the container is mounted and unsubscribes once it is removed.
    /// </summary>
    [Transpose.Name("tss.DRB")]
    internal static class DeferedRefreshBinding
    {
        /// <summary>
        /// Given the refresh callback, subscribes to <paramref name="observable"/> and returns the matching unsubscribe.
        /// </summary>
        internal static Func<Action, Action> On<T>(IObservable<T> observable) => refresh =>
        {
            // One handler instance for both calls: StopObserving removes by delegate, so a second conversion would remove nothing.
            ObservableEvent.ValueChanged<T> handler = _ => refresh();
            observable.ObserveFutureChanges(handler);
            return () => observable.StopObserving(handler);
        };

        internal static void Bind(HTMLElement container, Action refresh, params Func<Action, Action>[] bindings)
        {
            DomObserver.WhenMounted(container, () =>
            {
                var unsubscribes = bindings.Select(subscribe => subscribe(refresh)).ToArray();
                DomObserver.WhenRemoved(container, () => { foreach (var unsubscribe in unsubscribes) unsubscribe(); });
            });
        }
    }
}
