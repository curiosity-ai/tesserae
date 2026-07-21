using System;

namespace Tesserae
{
    /// <summary>
    /// Fluent extension methods for <see cref="SegmentedPivot"/>.
    /// </summary>
    [Transpose.Name("tss.SegmentedPivotX")]
    public static class SegmentedPivotExtensions
    {
        /// <summary>
        /// Configures the segmented pivot on the component.
        /// </summary>
        public static SegmentedPivot SegmentedPivot(this SegmentedPivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new SegmentedPivot.Tab(id, titleCreator, contentCreator, cached));
        }
    }
}
