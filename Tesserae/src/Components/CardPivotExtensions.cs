using System;

namespace Tesserae
{
    [H5.Name("tss.CardPivotX")]
    public static class CardPivotExtensions
    {
        public static CardPivot CardPivot(this CardPivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new CardPivot.Tab(id, titleCreator, contentCreator, cached));
        }
    }
}
