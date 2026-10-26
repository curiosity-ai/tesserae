using System.Collections.Generic;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Finds the <see cref="IDictationTarget"/>s on the page. A target puts itself in a hidden field of its root element
    /// (<see cref="Register"/>, which the <see cref="TextBox"/>, <see cref="TextArea"/> and <see cref="OmniBox"/> call on
    /// construction) and marks the element with <see cref="ElementAttribute"/>, so a query is one
    /// <c>querySelectorAll</c> and the answer is whatever is rendered right now: a field that was removed from the page
    /// is not in the list, and one that was never built by the toolkit but registered itself is.
    /// </summary>
    [Transpose.Name("tss.DictationTargets")]
    public static class DictationTargets
    {
        /// <summary>The attribute that marks the root element of a target, for a query or a style.</summary>
        public const string ElementAttribute = "data-tss-dictation";

        private const string TargetField = "__tssDictationTarget";

        /// <summary>
        /// Records <paramref name="target"/> in a hidden field of <paramref name="element"/> and marks the element, so the
        /// queries here find it for as long as it is on the page. Safe to call again for the same element.
        /// </summary>
        /// <param name="element">The root element of the field: the one that holds it and its microphone button.</param>
        /// <param name="target">The field.</param>
        public static void Register(HTMLElement element, IDictationTarget target)
        {
            if (element is null || target is null) return;

            element.setAttribute(ElementAttribute, string.Empty);
            element[TargetField] = target;
        }

        /// <summary>The target an element was registered with, or null when it is not a target's root element.</summary>
        public static IDictationTarget Of(HTMLElement element)
        {
            if (element is null || !element.HasOwnProperty(TargetField)) return null;

            return element[TargetField] as IDictationTarget;
        }

        /// <summary>The root elements of every target on the page, in document order.</summary>
        public static IReadOnlyList<HTMLElement> Elements()
        {
            var result = new List<HTMLElement>();

            foreach (var node in document.querySelectorAll("[" + ElementAttribute + "]"))
            {
                if (node is HTMLElement element && Of(element) is object) result.Add(element);
            }

            return result;
        }

        /// <summary>Every target currently on the page, in document order, visible or not.</summary>
        public static IReadOnlyList<IDictationTarget> All()
        {
            var result = new List<IDictationTarget>();

            foreach (var element in Elements()) result.Add(Of(element));

            return result;
        }

        /// <summary>The targets that can be seen right now: rendered, not hidden, and within the viewport.</summary>
        public static IReadOnlyList<IDictationTarget> Visible()
        {
            var result = new List<IDictationTarget>();

            foreach (var element in Elements())
            {
                if (IsVisible(element)) result.Add(Of(element));
            }

            return result;
        }

        /// <summary>The target holding keyboard focus (focus is in its element), or null when focus is elsewhere.</summary>
        public static IDictationTarget Focused()
        {
            var active = document.activeElement;

            if (active is null) return null;

            // The nearest registered ancestor, so a text box inside a bigger target (the OmniBox's input) answers for the bigger one.
            for (var node = active as HTMLElement; node is object; node = node.parentElement)
            {
                var target = Of(node);

                if (target is object) return target;
            }

            return null;
        }

        /// <summary>
        /// The target to dictate into: <see cref="Focused"/> when there is one, otherwise the first <see cref="Visible"/> target that
        /// is <see cref="IDictationTarget.IsEditable"/>. Null when there is none.
        /// </summary>
        public static IDictationTarget Current()
        {
            var focused = Focused();

            if (focused is object) return focused;

            foreach (var target in Visible())
            {
                if (target.IsEditable) return target;
            }

            return null;
        }

        private static bool IsVisible(HTMLElement element)
        {
            var rect = (DOMRect)element.getBoundingClientRect();

            // No box at all means it is not rendered (display: none, or a hidden ancestor, leaves none).
            if (rect.width == 0 && rect.height == 0) return false;

            // visibility is inherited, so the element's own computed value covers its ancestors.
            if (window.getComputedStyle(element).visibility == "hidden") return false;

            return rect.bottom > 0 && rect.right > 0 && rect.top < window.innerHeight && rect.left < window.innerWidth;
        }
    }
}
