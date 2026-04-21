using System;

namespace Tesserae
{
    [H5.Name("tss.SegmentedPivotX")]
    public static class SegmentedPivotExtensions
    {
        public static SegmentedPivot SegmentedPivot(this SegmentedPivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new SegmentedPivot.Tab(id, titleCreator, contentCreator, cached));
        }
    }
}
