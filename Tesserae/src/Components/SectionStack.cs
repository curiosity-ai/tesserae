using System;
using System.Collections.Generic;
using Retyped;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class SectionStack : Stack
    {
        private int Count = 1;
        public SectionStack() : base(StackOrientation.Vertical)
        {
            InnerElement.classList.add("tss-session-stack");
        }

        public void AddAnimated(IComponent component)
        {
            InnerElement.appendChild(GetAnimatedItem(component));
        }

        private HTMLDivElement GetAnimatedItem(IComponent component)
        {
            var item = (component as dynamic).StackItem as HTMLDivElement;
            if (item == null)
            {
                item = Div(_("tss-stack-item", styles: s =>
                {
                    s.alignSelf = "auto";
                    s.width = "auto";
                    s.height = "auto";
                    s.flexShrink = "1";
                    s.overflow = "hidden";
                }), component.Render());
                (component as dynamic).StackItem = item;
            }
            Count++;
            item.style.transitionDelay = $"{0.05f * Count:n2}s";

            window.setTimeout((_) => item.classList.add("ismounted"), 50); //TODO use DOMObserver

            return item;
        }

        public override void Clear()
        {
            ClearChildren(InnerElement);
            Count = 0;
        }

        public override HTMLElement Render()
        {
            
            return InnerElement;
        }

    }

    public static class SectionStackExtensions
    {
        public static SectionStack Section(this SectionStack stack, IComponent component)
        {
            stack.AddAnimated(component);
            return stack;
        }

        public static SectionStack Children(this SectionStack stack, params IComponent[] children)
        {
            children.ForEach(x => stack.Section(x));
            return stack;
        }
    }
}
