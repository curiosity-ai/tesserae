using System;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    public class TextBreadcrumb : ComponentBase<TextBreadcrumb, HTMLSpanElement>
    {
        public TextBreadcrumb(string text)
        {
            InnerElement = Span(_("tss-css-breadcrumb tss-btn-default tss-fontweight-regular tss-fontsize-small", text: text));
            
            AttachClick();
        }
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
    public class TextBreadcrumbs : IComponent, IContainer<TextBreadcrumbs, TextBreadcrumb>
    {
        private readonly HTMLElement _childContainer;

        public TextBreadcrumbs()
        {
            _childContainer = Div(_("tss-css-breadcrumb-container"));
        }

        public void Clear()
        {
            ClearChildren(_childContainer);
        }

        public void Replace(TextBreadcrumb newComponent, TextBreadcrumb oldComponent)
        {
            _childContainer.replaceChild(newComponent.Render(), oldComponent.Render());
        }

        public void Add(TextBreadcrumb component)
        {
            if (_childContainer.childElementCount == 0)
            {
                _childContainer.appendChild(
                    Div(_("tss-css-breadcrumb-wrap"),
                        component.Render()
                    ));
            }
            else
            {
                _childContainer.appendChild(
                    Div(_("tss-css-breadcrumb-wrap"),
                        Span(_("tss-css-breadcrumb-sep"), 
                            I(_())
                        ),
                        component.Render())
                );
            }

        }

        public TextBreadcrumbs Items(params TextBreadcrumb[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        public HTMLElement Render()
        {
            return _childContainer;
        }
    }
}