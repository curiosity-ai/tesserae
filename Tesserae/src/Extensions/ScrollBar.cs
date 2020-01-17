using Bridge;
using System.Collections.Generic;
using static Retyped.dom;

namespace Tesserae.Components
{
    public static class ScrollBar
    {
        private static List<Handle> LiveHandles = new List<Handle>();

        public class Handle
        {
            private object scrollbar;
            public Handle(HTMLElement element)
            {
                element.style.setProperty("overflow-y", "scroll", "important");
                element.classList.add("hide-scrollbar");
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
        }
        public static Handle EnableInvisibleScroll(HTMLElement element)
        {
            var h = new Handle(element);
            LiveHandles.Add(h);
            return h;
        }

        public static T InvisibleScroll<T>(this T component) where T : IComponent
        {
            var element = component.Render();
            EnableInvisibleScroll(element);
            return component;
        }
        
        public static void ForceRecalculateAll()
        {
            LiveHandles.RemoveAll(h => !h.IsAlive());
            LiveHandles.ForEach(h => h.Recalculate());
        }
    }
}
