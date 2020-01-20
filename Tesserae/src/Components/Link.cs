using System;
using Retyped;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Link : ComponentBase<Link, HTMLAnchorElement>
    {
        public Link(string url, IComponent component)
        {
            InnerElement = A(_(href:url), component.Render());
            AttachClick();
            AttachBlur();
            AttachFocus();
        }

        public string Target { get { return InnerElement.target; } set { InnerElement.target = value; } }
        public string URL    { get { return InnerElement.href; }   set { InnerElement.href   = value; } }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public static class LinkExtensions
    {
        public static Link OpenInNewTab(this Link link)
        {
            link.Target = "_blank";
            return link;
        }
        
        public static Link OnClicked(this Link link, Action onClick)
        {
            link.Target = "_blank";
            return link;
        }
    }
}