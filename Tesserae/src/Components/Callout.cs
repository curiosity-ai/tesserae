using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Callout")]
    public class Callout : Layer<Callout>
    {
        private readonly HTMLDivElement _container;
        private readonly HTMLElement _header;
        private readonly HTMLElement _content;

        public Callout()
        {
            _header = Div(_("tss-callout-header"));
            _content = Div(_("tss-callout-content"));
            _container = Div(_("tss-callout-container"), _header, _content);
            _contentHtml = _container;
        }

        public Callout SetHeader(IComponent header)
        {
            ClearChildren(_header);
            _header.appendChild(header.Render());
            return this;
        }

        public Callout SetContent(IComponent content)
        {
            ClearChildren(_content);
            _content.appendChild(content.Render());
            return this;
        }

        public override HTMLElement Render()
        {
            return _container;
        }

        protected override HTMLElement BuildRenderedContent()
        {
            return _container;
        }

        public void ShowFor(IComponent component)
        {
            var rect = (DOMRect)component.Render().getBoundingClientRect();
            _container.style.top = (rect.bottom + window.pageYOffset) + "px";
            _container.style.left = (rect.left + window.pageXOffset) + "px";
            Show();
        }
    }
}
