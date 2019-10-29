using System;
using Bridge;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Layer : ComponentBase<Layer, HTMLDivElement>
    {
        private IComponent content;
        private HTMLElement contentHtml;
        private HTMLElement renderedContent;
        public HTMLElement ContentHtml
        {
            get { return contentHtml; }
            set
            {
                if (value != contentHtml)
                {
                    if (IsVisible)
                    {
                        if (renderedContent == null) document.body.removeChild(renderedContent);
                        renderedContent = Div(_("mss-layer"), BuildRenderedContent());
                        document.body.appendChild(renderedContent);
                    }

                    contentHtml = value;
                }
            }
        }
        public IComponent Content
        {
            get { return content; }
            set
            {
                if (value != content)
                {
                    if (IsVisible)
                    {
                        if (renderedContent == null) document.body.removeChild(renderedContent);
                        renderedContent = Div(_("mss-layer"), BuildRenderedContent());
                        document.body.appendChild(renderedContent);
                    }

                    content = value;
                }
            }
        }

        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (value != IsVisible && (content != null || contentHtml != null))
                {
                    if (value)
                    {
                        renderedContent = Div(_("mss-layer"), Div(_("mss-layer-content"), BuildRenderedContent()));
                        document.body.appendChild(renderedContent);
                    }
                    else
                    {
                        document.body.removeChild(renderedContent);
                        renderedContent = null;
                    }
                }

                isVisible = value;
            }
        }

        public Layer()
        {
            InnerElement = Div(_("mss-layer-base"));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        private HTMLElement BuildRenderedContent()
        {
            if (contentHtml != null) return contentHtml;
            return content.Render();
        }
    }

    public static class LayerExtensions
    {
        public static Layer Content(this Layer layer, HTMLElement content)
        {
            layer.ContentHtml = content;
            return layer;
        }
        public static Layer Content(this Layer layer, IComponent content)
        {
            layer.Content = content;
            return layer;
        }
    }
}
