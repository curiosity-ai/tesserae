using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.UptimeCalendar")]
    public class UptimeCalendar : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _grid;
        private Action _hideTooltip;

        public string Margin  { get => _container.style.margin;  set => _container.style.margin = value; }
        public string Padding { get => _container.style.padding; set => _container.style.padding = value; }

        public UptimeCalendar(string title, string subtitle)
        {
            _grid = Div(_("tss-uptime-month-grid"));

            var header = Div(_("tss-uptime-month-header"),
                Span(_(text: title)),
                Span(_(text: subtitle))
            );

            _container = Div(_("tss-uptime-month-card"), header, _grid);
        }

        public UptimeCalendar Items(IEnumerable<(UptimeStatus status, IComponent tooltipContent)> items)
        {
            ClearChildren(_grid);

            foreach (var item in items)
            {
                var day = Div(_(GetStatusClass(item.status)));

                if (item.tooltipContent != null && item.status != UptimeStatus.Future && item.status != UptimeStatus.None)
                {
                    day.onmouseenter = e => ShowTooltip(day, item.tooltipContent);
                    day.onmouseleave = e => HideTooltip();
                    day.onfocus = e => ShowTooltip(day, item.tooltipContent);
                    day.onblur = e => HideTooltip();
                    day.tabIndex = 0; // Make focusable
                }

                _grid.appendChild(day);
            }

            return this;
        }

        private string GetStatusClass(UptimeStatus status)
        {
            var baseClass = status == UptimeStatus.None ? "tss-uptime-month-empty" : "tss-uptime-month-day";

            switch (status)
            {
                case UptimeStatus.Operational: return $"{baseClass} operational";
                case UptimeStatus.Minor: return $"{baseClass} minor";
                case UptimeStatus.Major: return $"{baseClass} major";
                case UptimeStatus.Maintenance: return $"{baseClass} maintenance";
                case UptimeStatus.Future: return $"{baseClass} future";
                case UptimeStatus.None:
                default:
                    return baseClass;
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
