using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.UptimeBars")]
    public class UptimeBars : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _barsContainer;
        private Action _hideTooltip;

        public string Margin  { get => _container.style.margin;  set => _container.style.margin = value; }
        public string Padding { get => _container.style.padding; set => _container.style.padding = value; }

        public UptimeBars()
        {
            _barsContainer = Div(_("tss-uptime-bars"));
            _container = Div(_("tss-uptime-row"), _barsContainer);
        }

        public UptimeBars Items(IEnumerable<(UptimeStatus status, IComponent tooltipContent)> items)
        {
            ClearChildren(_barsContainer);

            foreach (var item in items)
            {
                var bar = Span(_(GetStatusClass(item.status)));

                if (item.tooltipContent != null)
                {
                    bar.onmouseenter = e => ShowTooltip(bar, item.tooltipContent);
                    bar.onmouseleave = e => HideTooltip();
                    bar.onfocus = e => ShowTooltip(bar, item.tooltipContent);
                    bar.onblur = e => HideTooltip();
                    bar.tabIndex = 0; // Make focusable
                }

                _barsContainer.appendChild(bar);
            }

            return this;
        }

        public UptimeBars Compact()
        {
            _barsContainer.classList.add("tss-uptime-bars-compact");
            return this;
        }

        private string GetStatusClass(UptimeStatus status)
        {
            switch (status)
            {
                case UptimeStatus.Operational: return "operational";
                case UptimeStatus.Minor: return "minor";
                case UptimeStatus.Major: return "major";
                case UptimeStatus.Maintenance: return "maintenance";
                case UptimeStatus.Future: return "future";
                case UptimeStatus.None:
                default:
                    return "none";
            }
        }

        private void ShowTooltip(HTMLElement target, IComponent content)
        {
            HideTooltip();
            Tippy.ShowFor(target, content.Render(), out _hideTooltip, placement: TooltipPlacement.Bottom, delayShow: 0, delayHide: 0, arrow: true, theme: "uptime", maxWidth: 320);
        }

        private void HideTooltip()
        {
            _hideTooltip?.Invoke();
            _hideTooltip = null;
        }

        public HTMLElement Render() => _container;
    }
}
