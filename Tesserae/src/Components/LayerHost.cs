using System;
using H5;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A container that hosts overlay <see cref="Layer{T}"/> content (modals, dialogs, popovers) within its bounding
    /// box rather than the document body.
    /// </summary>
    [H5.Name("tss.LayerHost")]
    public class LayerHost : ComponentBase<Layer, HTMLDivElement>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public LayerHost()
        {
            InnerElement = Div(_("tss-layer-host"));
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