using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for configuring scrollbars on components.
    /// </summary>
    [H5.Name("tss.ScrollBar")]
    public static class ScrollBar
    {
        /// <summary>Enables automatic vertical scrolling.</summary>
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

        /// <summary>Enables automatic vertical scrolling and hides horizontal scrolling.</summary>
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

        /// <summary>Enables automatic horizontal scrolling and hides vertical scrolling.</summary>
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
        /// <summary>Enables automatic scrolling in both directions.</summary>
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
    }
}