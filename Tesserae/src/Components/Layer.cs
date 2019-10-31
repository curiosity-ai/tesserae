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

        private LayerHost host;

        public LayerHost Host
        {
            get { return host; }
            set
            {
                if (value != host)
                {
                    if (IsVisible) Hide();
                    host = value;
                    if (IsVisible) Show();
                }
            }
        }

        public HTMLElement ContentHtml
        {
            get { return contentHtml; }
            set
            {
                if (value != contentHtml)
                {
                    contentHtml = value;
                    if (IsVisible)
                    {
                        Hide();
                        Show();
                    }
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
                    content = value;
                    if (IsVisible)
                    {
                        Hide();
                        Show();
                    }
                }
            }
        }

        private bool isVisible;
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (value != IsVisible)
                {
                    if (value) Show();
                    else Hide();
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
            return Div(_("mss-layer-content"), content.Render());
        }

        private void Show()
        {
            if (content != null || contentHtml != null)
            {
                if (host == null)
                {
                    renderedContent = Div(_("mss-layer"), BuildRenderedContent());
                    document.body.appendChild(renderedContent);
                }
                else
                {
                    renderedContent = BuildRenderedContent();
                    host.InnerElement.appendChild(renderedContent);
                }
            }
        }

        private void Hide()
        {
            if (renderedContent != null)
            {
                if (host == null) document.body.removeChild(renderedContent);
                else host.InnerElement.removeChild(renderedContent);
                renderedContent = null;
            }
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
