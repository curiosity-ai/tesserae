using System;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    public class TextBreadcrumb : ComponentBase<TextBreadcrumb, HTMLSpanElement>
    {
        public TextBreadcrumb(string text)
        {
            InnerElement = Span(_("tss-textbreadcrumb", text: text));
            
            AttachClick();
        }
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
    public class TextBreadcrumbs : IComponent, IContainer<TextBreadcrumbs, TextBreadcrumb>, ITextFormating
    {
        private readonly HTMLElement InnerElement;

        public TextBreadcrumbs()
        {
            InnerElement = Div(_("tss-textbreadcrumb-container tss-fontsize-small tss-fontweight-regular"));
        }

        public void Clear()
        {
            ClearChildren(InnerElement);
        }

        public void Replace(TextBreadcrumb newComponent, TextBreadcrumb oldComponent)
        {
            InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
        }

        public void Add(TextBreadcrumb component)
        {
            if (InnerElement.childElementCount == 0)
            {
                InnerElement.appendChild(
                    Div(_("tss-textbreadcrumb-wrap"),
                        component.Render()
                    ));
            }
            else
            {
                InnerElement.appendChild(
                    Div(_("tss-textbreadcrumb-wrap"),
                        Span(_("tss-textbreadcrumb-sep"), 
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
            return InnerElement;
        }

        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public TextAlign TextAlign { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}