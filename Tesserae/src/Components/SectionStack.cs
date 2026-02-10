using System.Collections.Generic;
using H5;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A stack component that supports animated sections.
    /// </summary>
    [H5.Name("tss.SectionStack")]
    public class SectionStack : Stack
    {
        private int Count = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionStack"/> class.
        /// </summary>
        public SectionStack() : base(Stack.Orientation.Vertical)
        {
            InnerElement.classList.add("tss-sectionstack");
        }

        /// <summary>
        /// Adds a component as an animated section.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="grow">Whether the section should grow.</param>
        /// <param name="shrink">Whether the section should shrink.</param>
        /// <param name="customPadding">Custom padding for the section.</param>
        public void AddAnimated(IComponent component, bool grow = false, bool shrink = false, string customPadding = "")
        {
            InnerElement.appendChild(GetAnimatedItem(component, false, grow, shrink, customPadding));
        }

        /// <summary>
        /// Adds a component as an animated title.
        /// </summary>
        /// <param name="component">The component.</param>
        public void AddAnimatedTitle(IComponent component)
        {
            InnerElement.appendChild(GetAnimatedItem(component, true));
        }

        private HTMLDivElement GetAnimatedItem(IComponent component, bool isTitle, bool grow = false, bool shrink = false, string customPadding = "")
        {
            if (!((component as dynamic).SectionStackItem is HTMLDivElement item))
            {
                item = Div(_(isTitle ? "tss-sectionstack-title tss-stack-item tss-sectionstack-item" : "tss-sectionstack-card tss-stack-item tss-sectionstack-item"), component.Render());

                item.style.alignSelf = "auto";
                item.style.width     = "auto";
                item.style.overflow  = "hidden";

                (component as dynamic).SectionStackItem = item;
            }

            if (component.HasOwnProperty("StackItem"))
            {
                H5.Script.Delete(component["StackItem"]);
            }

            item.style.height     = grow ? "10px" : "auto";
            item.style.flexShrink = shrink ? "1" : "0";
            item.style.flexGrow   = grow ? "1" : "";
            item.style.padding    = customPadding;

            Count++;

            item.style.transitionDelay = $"{0.05f * Count:n2}s";

            DomObserver.WhenMounted(item, () => item.classList.add("tss-ismounted"));

            return item;
        }

        /// <summary>
        /// Clears all sections from the stack.
        /// </summary>
        public override void Clear()
        {
            ClearChildren(InnerElement);
            Count = 0;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}