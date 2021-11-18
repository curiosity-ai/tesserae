using System;

namespace Tesserae
{
    [H5.Name("tss.SectionStackX")]
    public static class SectionStackExtensions
    {
        public static SectionStack Section(this SectionStack stack, IComponent component, bool grow = false, string customPadding = "")
        {
            stack.AddAnimated(component, grow, customPadding);
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
