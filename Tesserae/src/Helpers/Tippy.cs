using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    public static class Tippy
    {
        public static void ShowFor(IComponent component, IComponent tooltip, TooltipAnimation animation = TooltipAnimation.ShiftAway, TooltipPlacement placement = TooltipPlacement.Top, int maxWidth = 350)
        {
            var rendered = component.Render();

            var renderedTooltip = UI.DIV(tooltip.Render());
            renderedTooltip.style.display = "block";
            renderedTooltip.style.overflow = "hidden";
            renderedTooltip.style.textOverflow = "ellipsis";
            document.body.appendChild(renderedTooltip);

            var (element, _) = Stack.GetCorrectItemToApplyStyle(component);
            if (element.HasOwnProperty("_tippy"))
                H5.Script.Write("{0}._tippy.destroy();", element);

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}});", element, renderedTooltip, placement.ToString(), document.body.As<object>(), maxWidth);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}});", element, renderedTooltip, placement.ToString(), animation.ToString(), document.body.As<object>(), maxWidth);
            }

            H5.Script.Write("{0}._tippy.show();", element);

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            component.WhenRemoved(() =>
            {
                // 2020-10-05 DWR: I presume that have to check this property before trying to kill it in case it's already been tidied up
                if (element.HasOwnProperty("_tippy"))
                    H5.Script.Write("{0}._tippy.destroy();", element);
            });
        }
    }
}
