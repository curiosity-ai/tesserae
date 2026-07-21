using System;

namespace Tesserae
{
    /// <summary>
    /// Fluent extension methods for <see cref="CardPivot"/>.
    /// </summary>
    [Transpose.Name("tss.CardPivotX")]
    public static class CardPivotExtensions
    {
        /// <summary>
        /// Configures the card pivot on the component.
        /// </summary>
        public static CardPivot CardPivot(this CardPivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new CardPivot.Tab(id, titleCreator, contentCreator, cached));
        }
    }
}
