using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A large numeric KPI tile used inside dashboards, showing a value with optional label and trend indicator.
    /// </summary>
    [H5.Name("tss.Metric")]
    public class Metric : ComponentBase<Metric, HTMLElement>
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _titleContainer;
        private readonly HTMLElement _valueContainer;
        private readonly HTMLElement _changeContainer;
        private readonly HTMLElement _chartContainer;

        public Metric(string title, string value)
        {
            _titleContainer = Div(_("tss-metric-title"), TextBlock(title).SmallPlus().SemiBold().Foreground(Theme.Secondary.Foreground).Render());
            _valueContainer = Div(_("tss-metric-value"), TextBlock(value).XLarge().SemiBold().Render());
            _changeContainer = Div(_("tss-metric-change"));
            _chartContainer = Div(_("tss-metric-chart"));

            _container = Div(_("tss-metric"), _titleContainer, _valueContainer, _chartContainer, _changeContainer);
            InnerElement = _container;
        }

        public Metric(IComponent title, IComponent value)
        {
            _titleContainer = Div(_("tss-metric-title"), title.Render());
            _valueContainer = Div(_("tss-metric-value"), value.Render());
            _changeContainer = Div(_("tss-metric-change"));
            _chartContainer = Div(_("tss-metric-chart"));

            _container = Div(_("tss-metric"), _titleContainer, _valueContainer, _chartContainer, _changeContainer);
            InnerElement = _container;
        }

        public Metric Chart(IComponent chart)
        {
            ClearChildren(_chartContainer);
            if (chart != null)
            {
                _chartContainer.appendChild(chart.Render());
            }
            return this;
        }

        public Metric Change(IComponent change)
        {
            ClearChildren(_changeContainer);
            _changeContainer.appendChild(change.Render());
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return _container;
        }
    }
}
