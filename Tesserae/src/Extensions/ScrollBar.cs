using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.ScrollBar")]
    public static class ScrollBar
    {
        private static readonly List<Handle> LiveHandles = new List<Handle>();

        public class Handle
        {
            private readonly object scrollbar;
            internal HTMLElement _element;
            public Handle(HTMLElement element, bool horizontal)
            {
                if (horizontal)
                {
                    element.style.setProperty("overflow-x", "none", "important");
                }
                else
                {
                    element.style.setProperty("overflow-y", "none", "important");
                }

                element.classList.add("tss-invisible-scrollbar");
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

        public static HTMLElement GetCorrectContainer(HTMLElement element)
        {
            if(element.classList.contains("tss-invisible-scrollbar"))
            {
                //try finding the new container created by the scrollbar class
                var sbc = element.getElementsByClassName("simplebar-content");
                if(sbc.length > 0)
                {
                    return (HTMLElement)sbc[0];
                }
                else
                {
                    return element;
                }
            }
            else
            {
                return element;
            }
        }

        public static Handle EnableInvisibleScroll(HTMLElement element, bool horizontal = false)
        {
            var h = new Handle(element, horizontal);
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

        public static T InvisibleScroll<T>(this T component, bool horizontal = false) where T : IComponent
        {
            var element = component.Render();

            DomObserver.WhenMounted(element, () =>
            {
                var targetElement = Stack.GetItem(component);
                EnableInvisibleScroll(targetElement, horizontal);
            });

            return component;
        }

        public static T Scroll<T>(this T component) where T : IComponent
        {
            var element = component.Render();

            DomObserver.WhenMounted(element, () =>
            {
                var targetElement = Stack.GetItem(component);
                targetElement.style.overflowY = "auto";
            });

            return component;
        }

        public static T ScrollY<T>(this T component) where T : IComponent
        {
            var element = component.Render();

            DomObserver.WhenMounted(element, () =>
            {
                var targetElement = Stack.GetItem(component);
                targetElement.style.overflowY = "auto";
                targetElement.style.overflowX = "hidden";
            });

            return component;
        }

        public static T ScrollX<T>(this T component) where T : IComponent
        {
            var element = component.Render();

            DomObserver.WhenMounted(element, () =>
            {
                var targetElement = Stack.GetItem(component);
                targetElement.style.overflowY = "hidden";
                targetElement.style.overflowX = "auto";
            });

            return component;
        }
        public static T ScrollBoth<T>(this T component) where T : IComponent
        {
            var element = component.Render();

            DomObserver.WhenMounted(element, () =>
            {
                var targetElement = Stack.GetItem(component);
                targetElement.style.overflowY = "auto";
                targetElement.style.overflowX = "auto";
            });

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
