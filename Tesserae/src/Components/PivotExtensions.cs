using System;

namespace Tesserae.Components
{
    public static class PivotExtensions
    {
        public static Pivot Pivot(this Pivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new Pivot.Tab(id, titleCreator, contentCreator, cached));
        }
    }
}