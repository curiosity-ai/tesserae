using H5;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.DeepResearchSubstepItem")]
    public sealed class DeepResearchSubstepItem : IComponent
    {
        private readonly Stack _root;
        private readonly Stack _header;
        private readonly Icon _statusIcon;
        private readonly TextBlock _titleText;
        private readonly TextBlock _statusText;
        private readonly TextBlock _messageText;
        private DeepResearchSubstepModel _model;

        public DeepResearchSubstepItem(DeepResearchSubstepModel model = null)
        {
            _statusIcon = Icon(UIcons.Clock).Class("tss-deepresearch-substep-icon").NoShrink();
            _titleText = TextBlock("").Small().SemiBold().Class("tss-deepresearch-substep-title").Grow();
            _statusText = TextBlock("").XSmall().SemiBold().Class("tss-deepresearch-substep-status").NoShrink();
            _messageText = TextBlock("").XSmall().Secondary().Class("tss-deepresearch-substep-message");

            _header = Stack().Horizontal().AlignItems(ItemAlign.Center).Gap(8.px()).WS().Children(
                _statusIcon,
                _titleText,
                _statusText
            );

            _root = Stack().Vertical().Gap(4.px()).Class("tss-deepresearch-substep").WS().Children(
                _header,
                _messageText.ML(24)
            );

            SetModel(model);
        }

        public DeepResearchSubstepModel Model => _model;

        public DeepResearchSubstepItem SetModel(DeepResearchSubstepModel model)
        {
            _model = model ?? new DeepResearchSubstepModel();

            var status = _model.Status;

            DeepResearchVisuals.ApplyStatusClass(_root.Render(), status);
            _statusIcon.SetIcon(DeepResearchVisuals.GetStatusIcon(status)).SetTitle(DeepResearchVisuals.GetStatusText(status));
            _titleText.Text = DeepResearchVisuals.FormatSubstepTitle(_model);
            _statusText.Text = DeepResearchVisuals.GetStatusText(status);
            _messageText.Text = _model.Message ?? string.Empty;

            if (DeepResearchVisuals.HasText(_model.Message))
            {
                _messageText.Show();
            }
            else
            {
                _messageText.Collapse();
            }

            return this;
        }

        public H5.Core.dom.HTMLElement Render() => _root.Render();
    }
}
