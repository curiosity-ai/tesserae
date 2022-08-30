using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.tippy")]
    public static class Tippy
    {
        public static void ShowFor(IComponent component, IComponent tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.ShiftAway, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false)
        {
            var rendered = component.Render();
            if (!rendered.IsMounted())
            {
                hide = () => { };
                return; //Only show tooltips for mounted objects
            }

            var renderedTooltip = UI.DIV(tooltip.Render());
            renderedTooltip.style.display = "block";
            renderedTooltip.style.overflow = "hidden";
            renderedTooltip.style.textOverflow = "ellipsis";
            document.body.appendChild(renderedTooltip);

            var (element, _) = Stack.GetCorrectItemToApplyStyle(component);

            Action onHidden = () =>
            {
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
            };

            hide = onHidden;

            onHidden(); //Remove previous tooltips

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}});", element, renderedTooltip, placement.ToString(), document.body.As<object>(), maxWidth, onHidden, delayShow, delayHide, arrow);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}});", element, renderedTooltip, placement.ToString(), animation.ToString(), document.body.As<object>(), maxWidth, onHidden, delayShow, delayHide, arrow);
            }


            H5.Script.Write("{0}._tippy.show();", element);

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            component.WhenRemoved(() =>
            {
                onHidden();
            });
        }

        public static void HideAll()
        {
            H5.Script.Write("tippy.hideAll()");
        }
    }
}
