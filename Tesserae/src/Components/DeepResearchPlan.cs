using System.Collections.Generic;
using H5;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.DeepResearchPlan")]
    public sealed class DeepResearchPlan : IComponent, IHasMarginPadding
    {
        private readonly Card _card;
        private readonly Stack _layout;
        private readonly Stack _header;
        private readonly Stack _titleRow;
        private readonly TextBlock _titleText;
        private readonly Badge _statusBadge;
        private readonly Stack _metaRow;
        private readonly TextBlock _summaryText;
        private readonly TextBlock _stageText;
        private readonly TextBlock _progressText;
        private readonly ProgressIndicator _progressIndicator;
        private readonly Stack _progressHost;
        private readonly Stack _stepsHost;
        private readonly TextBlock _emptyText;
        private readonly Stack _footer;
        private readonly TextBlock _footerText;
        private readonly TextBlock _timestampText;
        private readonly List<DeepResearchStepItem> _stepItems = new List<DeepResearchStepItem>();
        private DeepResearchPlanModel _model;

        public DeepResearchPlan(DeepResearchPlanModel model = null)
        {
            _titleText = TextBlock("").MediumPlus().SemiBold().Class("tss-deepresearch-plan-title").Grow();
            _statusBadge = Badge().Pill().Outline().Class("tss-deepresearch-plan-badge");

            _titleRow = Stack().Horizontal().Gap(12.px()).AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Between).WS().Children(
                _titleText,
                _statusBadge.NoShrink()
            );

            _summaryText = TextBlock("").Small().Class("tss-deepresearch-plan-summary");
            _stageText = TextBlock("").Small().SemiBold().Class("tss-deepresearch-plan-stage");
            _progressText = TextBlock("").Small().SemiBold().Class("tss-deepresearch-plan-progress-text");

            _metaRow = Stack().Horizontal().Wrap().Gap(8.px()).AlignItems(ItemAlign.Center).WS().Class("tss-deepresearch-plan-meta").Children(
                _summaryText,
                _stageText,
                _progressText
            );

            _header = Stack().Vertical().Gap(8.px()).WS().Children(
                _titleRow,
                _metaRow
            );

            _progressIndicator = ProgressIndicator().Class("tss-deepresearch-plan-progress-indicator");
            _progressHost = Stack().Class("tss-deepresearch-plan-progress").WS().Children(_progressIndicator);

            _stepsHost = Stack().Vertical().Gap(12.px()).WS().Class("tss-deepresearch-plan-steps");
            _emptyText = TextBlock("No research steps yet.").Small().Secondary().Class("tss-deepresearch-plan-empty");

            _footerText = TextBlock("").Small().Class("tss-deepresearch-plan-footer-text");
            _timestampText = TextBlock("").XSmall().Secondary().Class("tss-deepresearch-plan-timestamps");
            _footer = Stack().Vertical().Gap(4.px()).WS().Class("tss-deepresearch-plan-footer").Children(
                _footerText,
                _timestampText
            );

            _layout = Stack().Vertical().Gap(14.px()).P(16).WS().Class("tss-deepresearch-plan").Children(
                _header,
                _progressHost,
                _stepsHost,
                _footer
            );

            _card = Card(_layout).NoPadding().Class("tss-deepresearch-plan-card");
            _card.Border("var(--tss-default-border-color)");

            SetModel(model);
        }

        public DeepResearchPlanModel Model => _model;

        public string Margin
        {
            get => _card.Render().style.margin;
            set => _card.Render().style.margin = value;
        }

        public string Padding
        {
            get => _card.Render().style.padding;
            set => _card.Render().style.padding = value;
        }

        public DeepResearchPlan SetModel(DeepResearchPlanModel model)
        {
            _model = model ?? new DeepResearchPlanModel();

            var status = _model.Status;
            DeepResearchVisuals.ApplyStatusClass(_layout.Render(), status);
            _card.Border(DeepResearchVisuals.GetBorderColor(status));
            DeepResearchVisuals.ConfigureStatusBadge(_statusBadge, status);

            _titleText.Text = DeepResearchVisuals.HasText(_model.Title) ? _model.Title : "Deep research";
            _summaryText.Text = DeepResearchVisuals.BuildPlanSummary(_model);

            _stageText.Text = _model.CurrentStage ?? string.Empty;
            if (DeepResearchVisuals.HasText(_model.CurrentStage))
            {
                _stageText.Show();
            }
            else
            {
                _stageText.Collapse();
            }

            var progress = DeepResearchVisuals.ResolvePlanProgress(_model);
            _progressText.Text = DeepResearchVisuals.FormatProgress(progress);
            if (progress.HasValue)
            {
                _progressText.Show();
            }
            else
            {
                _progressText.Collapse();
            }

            if (status == DeepResearchStatus.Running && !progress.HasValue)
            {
                _progressIndicator.Indeterminated();
                _progressHost.Show();
            }
            else if (progress.HasValue)
            {
                _progressIndicator.Progress(progress.Value);
                _progressHost.Show();
            }
            else
            {
                _progressHost.Collapse();
            }

            RenderSteps();

            _footerText.Text = _model.FooterMessage ?? string.Empty;
            _timestampText.Text = DeepResearchVisuals.BuildTimestampText(_model) ?? string.Empty;

            if (DeepResearchVisuals.HasText(_footerText.Text))
            {
                _footerText.Show();
            }
            else
            {
                _footerText.Collapse();
            }

            if (DeepResearchVisuals.HasText(_timestampText.Text))
            {
                _timestampText.Show();
            }
            else
            {
                _timestampText.Collapse();
            }

            if (DeepResearchVisuals.HasText(_footerText.Text) || DeepResearchVisuals.HasText(_timestampText.Text))
            {
                _footer.Show();
            }
            else
            {
                _footer.Collapse();
            }

            return this;
        }

        public H5.Core.dom.HTMLElement Render() => _card.Render();

        private void RenderSteps()
        {
            var expandedStates = new Dictionary<string, bool>();

            foreach (var item in _stepItems)
            {
                expandedStates[item.Key] = item.IsExpanded;
            }

            _stepItems.Clear();
            _stepsHost.Clear();

            var steps = _model.Steps ?? new List<DeepResearchStepModel>();

            if (steps.Count == 0)
            {
                _stepsHost.Add(_emptyText);
                return;
            }

            for (var i = 0; i < steps.Count; i++)
            {
                var stepModel = steps[i] ?? new DeepResearchStepModel();
                var stepKey = DeepResearchVisuals.GetStepKey(stepModel, i);
                var stepItem = new DeepResearchStepItem(stepModel);

                if (!stepModel.IsExpanded.HasValue && expandedStates.TryGetValue(stepKey, out var wasExpanded))
                {
                    stepItem.SetExpanded(wasExpanded);
                }

                _stepItems.Add(stepItem);
                _stepsHost.Add(stepItem);
            }
        }
    }
}
