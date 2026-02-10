using System;

namespace Tesserae
{
    /// <summary>
    /// Extension methods for <see cref="SectionStack"/>.
    /// </summary>
    [H5.Name("tss.SectionStackX")]
    public static class SectionStackExtensions
    {
        /// <summary>
        /// Adds a section to the stack.
        /// </summary>
        /// <param name="stack">The stack.</param>
        /// <param name="component">The component.</param>
        /// <param name="grow">Whether the section should grow.</param>
        /// <param name="shrink">Whether the section should shrink.</param>
        /// <param name="customPadding">Custom padding.</param>
        /// <returns>The stack instance.</returns>
        public static SectionStack Section(this SectionStack stack, IComponent component, bool grow = false, bool shrink = false, string customPadding = "")
        {
            stack.AddAnimated(component, grow, shrink, customPadding);
            return stack;
        }

        /// <summary>
        /// Adds a title to the stack.
        /// </summary>
        /// <param name="stack">The stack.</param>
        /// <param name="component">The title component.</param>
        /// <returns>The stack instance.</returns>
        public static SectionStack Title(this SectionStack stack, IComponent component)
        {
            stack.AddAnimatedTitle(component);
            return stack;
        }

        /// <summary>
        /// Adds multiple sections to the stack.
        /// </summary>
        /// <param name="stack">The stack.</param>
        /// <param name="children">The components.</param>
        /// <returns>The stack instance.</returns>
        public static SectionStack Children(this SectionStack stack, params IComponent[] children)
        {
            children.ForEach(x => stack.Section(x));
            return stack;
        }
    }
}