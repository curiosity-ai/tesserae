using System;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Helpers for working with the viewport.
    /// </summary>
    [H5.Name("tss.Viewport")]
    public static class Viewport
    {
        /// <summary>
        /// Returns a task that completes the first time the element intersects the viewport.
        /// May be awaited before the element is mounted: the IntersectionObserver is hooked up
        /// once <see cref="DomObserver.WhenMounted"/> fires, and the observer is disconnected as
        /// soon as the element becomes visible. If the element is removed from the DOM before
        /// becoming visible, the task is cancelled (a <see cref="TaskCanceledException"/> is thrown
        /// when awaited).
        /// </summary>
        public static Task IsVisibleAsync(this HTMLElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            var tcs = new TaskCompletionSource<bool>();
            IntersectionObserver observer = null;

            void Cleanup()
            {
                if (observer is object)
                {
                    observer.disconnect();
                    observer = null;
                }
            }

            DomObserver.WhenMounted(element, () =>
            {
                if (tcs.Task.IsCompleted) return;

                observer = new IntersectionObserver((entries, _) =>
                {
                    if (entries.Any(e => e.isIntersecting))
                    {
                        Cleanup();
                        tcs.TrySetResult(true);
                    }
                });
                observer.observe(element);
            });

            DomObserver.WhenRemoved(element, () =>
            {
                Cleanup();
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }

        /// <summary>
        /// Returns a task that completes the first time the component intersects the viewport.
        /// See <see cref="IsVisibleAsync(HTMLElement)"/> for full semantics.
        /// </summary>
        public static Task IsVisibleAsync(this IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            return IsVisibleAsync(component.Render());
        }
    }
}
