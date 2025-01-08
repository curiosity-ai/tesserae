using System.Threading.Tasks;

namespace Tesserae
{
    [H5.Name("tss.IDefer")]
    public interface IDefer : IComponent
    {
        /// <summary>
        /// The milliseconds must be a value of at least one, trying to disable Debounce by passing a zero (or negative) value is not supported
        IDefer Debounce(int milliseconds);

        void Refresh();
        Task RefreshAsync();

        /// <summary>
        /// By default, the component will generate an empty container and only start to initiate the data retrieval and full rendering process when it is mounted in the DOM (so that things like height calculations may be performed accurately, which require that
        /// the component exist in its expected location in the DOM) but this can be expensive if rendering many items. If it is known that the component is immediately going to be mounted then this method may be called and the DomObserver.WhenMounted logic will
        /// be bypassed and replaced with a simple setTimeout of very short duration (to allow the immediate rendering of the element to take place).
        /// </summary>
        IDefer DoNotWaitForComponentMountingBeforeRendering();
    }
}