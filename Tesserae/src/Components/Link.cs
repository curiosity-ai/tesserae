using System;
using Retyped;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Link : IComponent
    {
        private HTMLAnchorElement _anchor;
        public Link(string url, IComponent component)
        {
            _anchor = A(_(href: url), component.Render());
        }

        public string Target { get { return _anchor.target; } set { _anchor.target = value; } }
        public string URL { get { return _anchor.href; } set { _anchor.href = value; } }

        public HTMLElement Render()
        {
            return _anchor;
        }

        public Link OpenInNewTab()
        {
            Target = "_blank";
            return this;
        }

        public Link OnClick(Action onClicked)
        {
            if (onClicked is null)
            {
                throw new ArgumentNullException(nameof(onClicked));
            }

            _anchor.onclick = (e) =>
            {
                StopEvent(e);
                onClicked();
            };

            return this;
        }
    }
}
