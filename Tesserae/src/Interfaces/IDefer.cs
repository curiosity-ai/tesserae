using System.Threading.Tasks;

namespace Tesserae
{
    /// <summary>
    /// Represents a component that defers its rendering until data is available or it is mounted in the DOM.
    /// </summary>
    [H5.Name("tss.IDefer")]
    public interface IDefer : IComponent
    {
        /// <summary>
        /// Configures debouncing for the deferred rendering.
        /// The milliseconds must be a value of at least one, trying to disable Debounce by passing a zero (or negative) value is not supported.
        /// </summary>
        /// <param name="milliseconds">The debounce delay in milliseconds.</param>
        /// <param name="millisecondsForLoadingMessage">The delay before showing the loading message.</param>
        /// <returns>The current instance of the type.</returns>
        IDefer Debounce(int milliseconds, int millisecondsForLoadingMessage = 1000);

        /// <summary>
        /// Configures debouncing for the deferred rendering with a maximum delay.
        /// The milliseconds must be a value of at least one, trying to disable Debounce by passing a zero (or negative) value is not supported.
        /// </summary>
        /// <param name="delayInMs">The debounce delay in milliseconds.</param>
        /// <param name="maxDelayInMs">The maximum delay in milliseconds.</param>
        /// <param name="millisecondsForLoadingMessage">The delay before showing the loading message.</param>
        /// <returns>The current instance of the type.</returns>
        IDefer Debounce(int delayInMs, int maxDelayInMs, int millisecondsForLoadingMessage = 1000);

        /// <summary>Triggers a refresh of the deferred content.</summary>
        void Refresh();
        /// <summary>Triggers an asynchronous refresh of the deferred content.</summary>
        /// <returns>A task representing the refresh operation.</returns>
        Task RefreshAsync();

        /// <summary>
        /// By default, the component will generate an empty container and only start to initiate the data retrieval and full rendering process when it is mounted in the DOM (so that things like height calculations may be performed accurately, which require that
        /// the component exist in its expected location in the DOM) but this can be expensive if rendering many items. If it is known that the component is immediately going to be mounted then this method may be called and the DomObserver.WhenMounted logic will
        /// be bypassed and replaced with a simple setTimeout of very short duration (to allow the immediate rendering of the element to take place).
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        IDefer DoNotWaitForComponentMountingBeforeRendering();
    }
}