using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Gives a <see cref="TextBlock"/>, a <see cref="Label"/>'s caption or an <see cref="InlineLabel"/> whose text is cut
    /// short - an ellipsis, a line clamp - a tooltip with the whole text, built the first time the pointer rests on it.
    /// Text that fits gets nothing, and so does an element that already has a tooltip (or sits inside one that does).
    /// </summary>
    /// <remarks>
    /// <para>One capture-phase <c>mouseenter</c> listener on the document sees every element the pointer enters, so a
    /// block costs nothing until somebody hovers it - no listener, no closure, no property per instance, which on a list
    /// of a few thousand rows is the difference that matters (see the note on the Attach* methods in ComponentBase).
    /// Everything that is not one of the three is turned away after a class test.</para>
    /// <para>The tippy it builds stays on the element and asks again on every show, so a block that is cut short in a
    /// narrow window and fits in a wide one only shows the tooltip while it is cut short, and always shows the text it
    /// holds now rather than what it held when the tooltip was built.</para>
    /// </remarks>
    internal static class OverflowTooltip
    {
        /// <summary>
        /// The element property a component can put the text it shows under, when the element's own text is not it -
        /// an <see cref="InlineLabel"/> path draws its separator as a glyph, so its text content reads "BoxFiles".
        /// </summary>
        internal const string TextProperty = "_tssOverflowText";

        /// <summary>The element property the tippy built here is kept under, so it can be told apart from a real tooltip.</summary>
        private const string InstanceProperty = "_tssOverflowTippy";

        private const int DelayShow = 250;

        /// <summary>
        /// How far past its box the text has to reach to count as cut short, in CSS pixels. Chromium lays out in 1/64px
        /// units and draws no ellipsis for text one unit too wide, but does for two (measured): this sits between them.
        /// </summary>
        private const double Tolerance = 0.02;

        private static bool _listening;

        /// <summary>Starts listening, the first time a component that can be cut short is built.</summary>
        internal static void EnsureListening()
        {
            if (_listening) return;

            _listening = true;

            document.addEventListener("mouseenter", (Action<Event>)OnMouseEnter, true);
        }

        private static void OnMouseEnter(Event e)
        {
            //Every element the pointer enters comes through here: anything else goes back after a class test
            var element = e.target.As<HTMLElement>();

            if (element is null || element.classList is null) return;

            if (!element.classList.contains("tss-textblock") && !element.classList.contains("tss-inlinelabel") && !IsLabelCaption(element)) return;

            //Built already: its own hover handlers take it from here
            if (IsOurs(element)) return;

            //A button held down is a drag or a selection, not somebody reading
            if (e.As<MouseEvent>().buttons != 0) return;

            if (HasTooltipOfItsOwn(element)) return;

            var text = CutShortText(element);

            if (text is null) return;

            Attach(element, text);
        }

        /// <summary>
        /// The caption of a <see cref="Label"/> is its <c>label</c> element, not the label's root: the root holds the
        /// labelled component too, and hovering a text box should not bring up its label's tooltip.
        /// </summary>
        private static bool IsLabelCaption(HTMLElement element)
        {
            return element.tagName == "LABEL" && element.parentElement is object && element.parentElement.classList.contains("tss-label");
        }

        private static bool IsOurs(HTMLElement element)
        {
            //Tippy deletes _tippy on destroy, and a real tooltip destroys ours before it attaches, so a stale marker
            //reads as not ours and the element is asked again
            return element.HasOwnProperty(InstanceProperty) && element.HasOwnProperty("_tippy") && element["_tippy"] == element[InstanceProperty];
        }

        /// <summary>
        /// Whether the element, or anything around it, already says something on hover: a <c>Tooltip(...)</c>, a tippy
        /// somebody else put there, or a native <c>title</c>. Two tooltips for one pointer is worse than none.
        /// </summary>
        private static bool HasTooltipOfItsOwn(HTMLElement element)
        {
            for (var current = element; current is object; current = current.parentElement)
            {
                //Set by Tooltip(...) before its tippy exists, and nulled by RemoveTooltip()
                if (current["tooltipMarker"] is object) return true;

                if (current.HasOwnProperty("_tippy") && !IsOurs(current)) return true;

                if (current.hasAttribute("title")) return true;
            }

            return false;
        }

        /// <summary>The text to show when some of it is cut off, or null when all of it is on screen.</summary>
        private static string CutShortText(HTMLElement element)
        {
            var cut = FirstCutShort(element);

            if (cut is null) return null;

            var text = element.HasOwnProperty(TextProperty) ? element[TextProperty].As<string>() : cut.innerText;

            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        /// <summary>
        /// The element itself and its direct children: a block with after-text ellipsizes its first part, a label its
        /// text span, while a plain block is cut short itself.
        /// </summary>
        private static HTMLElement FirstCutShort(HTMLElement element)
        {
            if (IsCutShort(element)) return element;

            for (var child = element.firstElementChild; child is object; child = child.nextElementSibling)
            {
                var childElement = child.As<HTMLElement>();

                if (IsCutShort(childElement)) return childElement;
            }

            return null;
        }

        /// <summary>
        /// Whether the element clips its own text: it does not let it overflow (an ellipsis needs that too), and the text
        /// is wider than the room it has - or taller, for a line clamp.
        /// </summary>
        internal static bool IsCutShort(HTMLElement element)
        {
            var style = window.getComputedStyle(element);

            var lineClamp = style.getPropertyValue("-webkit-line-clamp");

            if (!string.IsNullOrEmpty(lineClamp) && lineClamp != "none" && element.scrollHeight > element.clientHeight) return true;

            if (style.getPropertyValue("overflow-x") == "visible") return false;

            return IsWiderThanItsBox(element, style);
        }

        /// <summary>
        /// Whether the element's content is wider than its content box, to the precision the engine draws an ellipsis at.
        /// </summary>
        /// <remarks>
        /// <c>scrollWidth &gt; clientWidth</c> is the usual test and it is right when it says yes, but both are rounded to
        /// whole pixels: text 0.05px too wide reads 276 against 276, with the ellipsis already drawn. A range over the
        /// content measures the text at its full width even while it is ellipsized, so when the rounded numbers agree
        /// that is compared with the content box instead. (<c>offsetWidth &lt;= scrollWidth</c> does not work at all: the
        /// two are equal for text that fits, and the border makes offsetWidth the larger for text that does not.)
        /// </remarks>
        internal static bool IsWiderThanItsBox(HTMLElement element, CSSStyleDeclaration style = null)
        {
            if (element.scrollWidth > element.clientWidth) return true;

            if (element.offsetWidth <= 0) return false;

            style = style ?? window.getComputedStyle(element);

            var range = document.createRange();
            range.selectNodeContents(element);

            var textWidth = range.getBoundingClientRect().As<DOMRect>().width;
            var boxWidth  = element.getBoundingClientRect().As<DOMRect>().width;

            //Both rects come back after any transform, the padding and border before it
            var scale = boxWidth / element.offsetWidth;

            var insets = Pixels(style, "padding-left") + Pixels(style, "padding-right") + Pixels(style, "border-left-width") + Pixels(style, "border-right-width");

            return (textWidth - (boxWidth - insets * scale)) / scale > Tolerance;
        }

        private static double Pixels(CSSStyleDeclaration style, string property)
        {
            return double.TryParse(style.getPropertyValue(property).Replace("px", ""), out var value) ? value : 0;
        }

        private static void Attach(HTMLElement element, string text)
        {
            var content = UI.DIV();
            content.textContent = text;

            Func<bool> onShow = () =>
            {
                //A real tooltip added since, or text that fits now: show nothing and stay, so the next hover asks again
                if (HasTooltipOfItsOwn(element)) return false;

                var current = CutShortText(element);

                if (current is null) return false;

                content.textContent = current;

                return true;
            };

            // Same z-index lane as every other lazy tooltip, so it sits above any Layer that is open (see Tooltip(...)).
            if (!int.TryParse(Layers.AboveCurrent(), out var zIndex)) zIndex = 9999;

            var instance = Transpose.Script.Write<object>("tippy({0}, { content: {1}, placement: 'top', delay: [{2}, 0], maxWidth: 350, arrow: false, appendTo: document.body, zIndex: {3}, onShow: function() { return {4}(); } })", element, content, DelayShow, zIndex, onShow);

            element[InstanceProperty] = instance;

            //The pointer is already on it, so tippy's own mouseenter has been and gone: show it after the usual delay
            //if the pointer is still there by then
            window.setTimeout(_ =>
            {
                if (IsOurs(element) && element.matches(":hover")) Transpose.Script.Write("{0}.show();", instance);
            }, DelayShow);

            //A block taken out of the document while its tooltip is up would leave the tooltip hanging
            DomObserver.WhenRemoved(element, () =>
            {
                if (IsOurs(element)) Transpose.Script.Write("{0}.destroy();", instance);

                Transpose.Script.Write("delete {0}[{1}];", element, InstanceProperty);
            });
        }
    }
}
