using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Provides fluent extension methods for accessibility (ARIA) properties on components.
    /// </summary>
    [H5.Name("tss.IACCX")]
    public static class IAccessibilityExtensions
    {
        /// <summary>Sets the ARIA role for the component.</summary>
        public static T AriaRole<T>(this T component, string role) where T : IComponent
        {
            if (component is IAccessibility accessibility)
            {
                accessibility.AriaRole = role;
            }
            else
            {
                component.Render().setAttribute("role", role);
            }
            return component;
        }

        /// <summary>Sets the ARIA label for the component.</summary>
        public static T AriaLabel<T>(this T component, string label) where T : IComponent
        {
            if (component is IAccessibility accessibility)
            {
                accessibility.AriaLabel = label;
            }
            else
            {
                component.Render().setAttribute("aria-label", label);
            }
            return component;
        }

        /// <summary>Sets the ID of the element that labels this component.</summary>
        public static T AriaLabelledBy<T>(this T component, string labelledBy) where T : IComponent
        {
            if (component is IAccessibility accessibility)
            {
                accessibility.AriaLabelledBy = labelledBy;
            }
            else
            {
                component.Render().setAttribute("aria-labelledby", labelledBy);
            }
            return component;
        }

        /// <summary>Sets the ID of the element that describes this component.</summary>
        public static T AriaDescribedBy<T>(this T component, string describedBy) where T : IComponent
        {
            if (component is IAccessibility accessibility)
            {
                accessibility.AriaDescribedBy = describedBy;
            }
            else
            {
                component.Render().setAttribute("aria-describedby", describedBy);
            }
            return component;
        }

        /// <summary>Sets whether the component is hidden from accessibility tools.</summary>
        public static T AriaHidden<T>(this T component, bool hidden = true) where T : IComponent
        {
            component.Render().setAttribute("aria-hidden", hidden ? "true" : "false");
            return component;
        }

        /// <summary>Sets the ARIA live region policy for the component.</summary>
        public static T AriaLive<T>(this T component, string live = "polite") where T : IComponent
        {
            component.Render().setAttribute("aria-live", live);
            return component;
        }

        /// <summary>Sets whether the ARIA live region is atomic.</summary>
        public static T AriaAtomic<T>(this T component, bool atomic = true) where T : IComponent
        {
            component.Render().setAttribute("aria-atomic", atomic ? "true" : "false");
            return component;
        }

        /// <summary>Sets whether the component is currently busy.</summary>
        public static T AriaBusy<T>(this T component, bool busy = true) where T : IComponent
        {
            component.Render().setAttribute("aria-busy", busy ? "true" : "false");
            return component;
        }

        /// <summary>Sets whether the component is expanded or collapsed.</summary>
        public static T AriaExpanded<T>(this T component, bool expanded = true) where T : IComponent
        {
            component.Render().setAttribute("aria-expanded", expanded ? "true" : "false");
            return component;
        }

        /// <summary>Sets the checked state of the component.</summary>
        public static T AriaChecked<T>(this T component, bool isChecked = true) where T : IComponent
        {
            component.Render().setAttribute("aria-checked", isChecked ? "true" : "false");
            return component;
        }

        /// <summary>Sets the selected state of the component.</summary>
        public static T AriaSelected<T>(this T component, bool selected = true) where T : IComponent
        {
            component.Render().setAttribute("aria-selected", selected ? "true" : "false");
            return component;
        }

        /// <summary>Sets the disabled state of the component for accessibility tools.</summary>
        public static T AriaDisabled<T>(this T component, bool disabled = true) where T : IComponent
        {
            component.Render().setAttribute("aria-disabled", disabled ? "true" : "false");
            return component;
        }

        /// <summary>Sets the current state of the component (e.g., "page", "step", "location", "date", "time", "true", "false").</summary>
        public static T AriaCurrent<T>(this T component, string current = "true") where T : IComponent
        {
            component.Render().setAttribute("aria-current", current);
            return component;
        }

        /// <summary>Sets the ID(s) of the element(s) controlled by this component.</summary>
        public static T AriaControls<T>(this T component, string controls) where T : IComponent
        {
            component.Render().setAttribute("aria-controls", controls);
            return component;
        }

        /// <summary>Sets whether the component has a popup (e.g., "true", "menu", "listbox", "tree", "grid", "dialog").</summary>
        public static T AriaHasPopup<T>(this T component, string hasPopup = "true") where T : IComponent
        {
            component.Render().setAttribute("aria-haspopup", hasPopup);
            return component;
        }
    }
}
