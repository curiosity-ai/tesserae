using System;
using Bridge;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class LayerHost : ComponentBase<Layer, HTMLDivElement>
    {
        public LayerHost()
        {
            InnerElement = Div(_("mss-layer-host"));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
