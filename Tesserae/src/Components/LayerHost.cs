using System;
using H5;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.LayerHost")]
    public class LayerHost : ComponentBase<Layer, HTMLDivElement>
    {
        public LayerHost()
        {
            InnerElement = Div(_("tss-layer-host"));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}