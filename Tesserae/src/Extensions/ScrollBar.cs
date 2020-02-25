using Bridge;
using System;
using System.Collections.Generic;
using System.Linq;
using static Retyped.dom;

namespace Tesserae.Components
{
    public static class ScrollBar
    {
        private static List<Handle> LiveHandles = new List<Handle>();

        public class Handle
        {
            private object scrollbar;
            internal HTMLElement _element;
            public Handle(HTMLElement element)
            {
                element.style.setProperty("overflow-y", "scroll", "important");
                element.classList.add("hide-scrollbar");
                _element = element;
                scrollbar = Script.Write<object>("new SimpleBar({0})", element);
            }
            public void Recalculate()
            {
                try
                {
                    Script.Write("{0}.recalculate()", scrollbar);
                }
                catch
                {
                    //Do nothing
                }
            }

            internal bool IsAlive()
            {
                return document.body.contains(Script.Write<Node>("{0}.el", scrollbar));
            }

            internal void Disable()
            {
                try
                {
                    Script.Write("{0}.unMount()", scrollbar);
                }
                catch
                {
                    //Do nothing
                }
            }
        }
        public static Handle EnableInvisibleScroll(HTMLElement element)
        {
            var h = new Handle(element);
            LiveHandles.Add(h);
            return h;
        }

        public static void DisableInvisibleScroll(HTMLElement element)
        {
            var previous = LiveHandles.Where(i => i._element == element).FirstOrDefault();
            
            if(previous is object)
            {
                previous.Disable();
            }

            LiveHandles.RemoveAll(i => i._element == element);
        }

        public static T InvisibleScroll<T>(this T component) where T : IComponent
        {
            var element = component.Render();
            EnableInvisibleScroll(element);
            return component;
        }

        public static T RemoveInvisibleScroll<T>(this T component) where T : IComponent
        {
            var element = component.Render();
            RemoveInvisibleScroll(element);
            element.style.setProperty("overflow-y", "auto");
            element.classList.remove("hide-scrollbar");
            return component;
        }

        public static HTMLElement InvisibleScroll(this HTMLElement element)
        {
            EnableInvisibleScroll(element);
            return element;
        }

        public static HTMLElement RemoveInvisibleScroll(this HTMLElement element)
        {
            DisableInvisibleScroll(element);
            return element;
        }

        public static HTMLDivElement InvisibleScroll(this HTMLDivElement element)
        {
            EnableInvisibleScroll(element);
            return element;
        }

        public static void ForceRecalculateAll()
        {
            LiveHandles.RemoveAll(h => !h.IsAlive());
            LiveHandles.ForEach(h => h.Recalculate());
        }
    }
}
