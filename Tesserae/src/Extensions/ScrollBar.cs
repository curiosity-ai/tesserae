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

        /// <summary>
        /// Gets the current vertical scroll offset of the element that
        /// <see cref="Scroll"/> / <see cref="ScrollY"/> / <see cref="ScrollBoth"/>
        /// made scrollable. Those helpers attach the overflow to
        /// <c>Stack.GetItem(component)</c> rather than <c>component.Render()</c>
        /// (which differ once the component is nested inside a Stack), so this
        /// reads from the same element and stays correct in both cases.
        /// </summary>
        public static double GetScrollTop<T>(this T component) where T : IComponent
            => Stack.GetItem(component).scrollTop;

        /// <summary>
        /// Sets the vertical scroll offset of the element that
        /// <see cref="Scroll"/> / <see cref="ScrollY"/> / <see cref="ScrollBoth"/>
        /// made scrollable. See <see cref="GetScrollTop"/> for why this targets
        /// <c>Stack.GetItem(component)</c> instead of <c>component.Render()</c>.
        /// </summary>
        public static T ScrollTop<T>(this T component, double scrollTop) where T : IComponent
        {
            Stack.GetItem(component).scrollTop = scrollTop;
            return component;
        }
    }
}