namespace Tesserae
{
    /// <summary>
    /// Base class of the navigation events raised by <see cref="Pivot"/>, <see cref="PivotSelector"/>,
    /// <see cref="SegmentedPivot"/> and <see cref="CardPivot"/>.
    /// </summary>
    public abstract class PivotEvent
    {
        internal PivotEvent(string currentPivot, string targetPivot)
        {
            CurrentPivot = currentPivot;
            TargetPivot  = targetPivot;
        }
        /// <summary>
        /// Gets or sets the current pivot.
        /// </summary>
        public string CurrentPivot { get; }
        /// <summary>
        /// Gets or sets the target pivot.
        /// </summary>
        public string TargetPivot { get; }
    }

    /// <summary>
    /// Raised after a pivot has switched to another tab.
    /// </summary>
    public sealed class PivotNavigateEvent : PivotEvent
    {
        internal PivotNavigateEvent(string currentPivot, string targetPivot) : base(currentPivot, targetPivot) { }
    }

    /// <summary>
    /// Raised before a pivot switches to another tab; call <see cref="Cancel"/> to stay on the current one.
    /// </summary>
    public class PivotBeforeNavigateEvent : PivotEvent
    {
        internal PivotBeforeNavigateEvent(string currentPivot, string targetPivot) : base(currentPivot, targetPivot) => Canceled = false;

        internal bool Canceled { get; private set; }

        /// <summary>
        /// Cancels the component's current operation.
        /// </summary>
        public void Cancel() => Canceled = true;
    }
}
