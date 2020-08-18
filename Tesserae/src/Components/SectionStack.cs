using System;
using System.Collections.Generic;
using H5;
using static Tesserae.UI;
using static H5.Core.dom;
using Tesserae.HTML;

namespace Tesserae.Components
{
    public class SectionStack : Stack
    {
        private int Count = 1;
        public SectionStack() : base(Stack.Orientation.Vertical)
        {
            InnerElement.classList.add("tss-sectionstack");
        }

        public void AddAnimated(IComponent component, bool grow = false)
        {
            InnerElement.appendChild(GetAnimatedItem(component, false, grow));
        }

        public void AddAnimatedTitle(IComponent component)
        {
            InnerElement.appendChild(GetAnimatedItem(component, true));
        }

        private HTMLDivElement GetAnimatedItem(IComponent component, bool isTitle, bool grow = false)
        {
            if (!((component as dynamic).StackItem is HTMLDivElement item))
            {
                item = Div(_(isTitle ? "tss-sectionstack-title tss-stack-item tss-sectionstack-item" : "tss-sectionstack-card tss-stack-item tss-sectionstack-item", styles: s =>
                {
                    s.alignSelf = "auto";
                    s.width = "auto";
                    s.height = "auto";
                    s.flexShrink = "1";
                    s.overflow = "hidden";
                }), component.Render());
                (component as dynamic).StackItem = item;
            }
            if (grow)
            {
                item.style.flexGrow = "1";
                item.style.height = "10px";
            }

            Count++;
            item.style.transitionDelay = $"{0.05f * Count:n2}s";

            DomObserver.WhenMounted(item, () => item.classList.add("tss-ismounted"));

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
        public static SectionStack Section(this SectionStack stack, IComponent component, bool grow = false)
        {
            stack.AddAnimated(component, grow);
            return stack;
        }

        public static SectionStack Title(this SectionStack stack, IComponent component)
        {
            stack.AddAnimatedTitle(component);
            return stack;
        }

        public static SectionStack Children(this SectionStack stack, params IComponent[] children)
        {
            children.ForEach(x => stack.Section(x));
            return stack;
        }
    }
}
