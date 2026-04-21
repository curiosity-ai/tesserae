using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Metric")]
    public class Metric : ComponentBase<Metric, HTMLElement>
    {
        private readonly HTMLElement _container;
        private readonly HTMLElement _titleContainer;
        private readonly HTMLElement _valueContainer;
        private readonly HTMLElement _changeContainer;

        public Metric(string title, string value)
        {
            _titleContainer = Div(_("tss-metric-title"), TextBlock(title).SmallPlus().SemiBold().Foreground(Theme.Secondary.Foreground).Render());
            _valueContainer = Div(_("tss-metric-value"), TextBlock(value).XLarge().SemiBold().Render());
            _changeContainer = Div(_("tss-metric-change"));

            _container = Div(_("tss-metric"), _titleContainer, _valueContainer, _changeContainer);
            InnerElement = _container;
        }

        public Metric(IComponent title, IComponent value)
        {
            _titleContainer = Div(_("tss-metric-title"), title.Render());
            _valueContainer = Div(_("tss-metric-value"), value.Render());
            _changeContainer = Div(_("tss-metric-change"));

            _container = Div(_("tss-metric"), _titleContainer, _valueContainer, _changeContainer);
            InnerElement = _container;
        }

        public Metric Change(IComponent change)
        {
            ClearChildren(_changeContainer);
            _changeContainer.appendChild(change.Render());
            return this;
        }

        public override HTMLElement Render()
        {
            return _container;
        }
    }
}
