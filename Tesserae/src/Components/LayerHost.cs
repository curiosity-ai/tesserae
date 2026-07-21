using System;
using Transpose;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A container that hosts overlay <see cref="Layer{T}"/> content (modals, dialogs, popovers) within its bounding
    /// box rather than the document body.
    /// </summary>
    [Transpose.Name("tss.LayerHost")]
    public class LayerHost : ComponentBase<Layer, HTMLDivElement>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public LayerHost()
        {
            InnerElement = Div(Att("tss-layer-host"));
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}