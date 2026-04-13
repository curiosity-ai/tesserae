using System.Collections.Generic;
using H5;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.DeepResearchStepItem")]
    public sealed class DeepResearchStepItem : IComponent
    {
        private readonly Stack _root;
        private readonly Stack _header;
        private readonly Stack _indicator;
        private readonly Icon _indicatorIcon;
        private readonly Stack _content;
        private readonly TextBlock _titleText;
        private readonly Stack _meta;
        private readonly TextBlock _stageText;
        private readonly TextBlock _progressText;
        private readonly Badge _statusBadge;
        private readonly ProgressIndicator _progressIndicator;
        private readonly Stack _progressHost;
        private readonly Button _toggleButton;
        private readonly Stack _substepsHost;
        private readonly List<DeepResearchSubstepItem> _substepItems = new List<DeepResearchSubstepItem>();
        private DeepResearchStepModel _model;

        public DeepResearchStepItem(DeepResearchStepModel model = null)
        {
            _indicatorIcon = Icon(UIcons.Clock).Class("tss-deepresearch-step-indicator-icon");
            _indicator = Stack().AlignItemsCenter().JustifyContent(ItemJustify.Center).NoShrink().Class("tss-deepresearch-step-indicator").Children(_indicatorIcon).W(28).H(28);

            _titleText = TextBlock("").SmallPlus().SemiBold().Class("tss-deepresearch-step-title");
            _stageText = TextBlock("").XSmall().Class("tss-deepresearch-step-stage");
            _progressText = TextBlock("").XSmall().SemiBold().Class("tss-deepresearch-step-progress-text");
            _statusBadge = Badge().Pill().Outline().Class("tss-deepresearch-step-badge");

            _meta = Stack().Horizontal().Wrap().Gap(8.px()).AlignItems(ItemAlign.Center).Class("tss-deepresearch-step-meta").Children(
                _stageText,
                _progressText,
                _statusBadge
            );

            _progressIndicator = ProgressIndicator().Class("tss-deepresearch-step-progress-indicator");
            _progressHost = Stack().Class("tss-deepresearch-step-progress").WS().Children(_progressIndicator);

            _content = Stack().Vertical().Gap(8.px()).Grow().WS().Class("tss-deepresearch-step-content").Children(
                _titleText,
                _meta,
                _progressHost
            );

            _toggleButton = Button()
               .SetIcon(UIcons.AngleSmallDown)
               .SetTitle("Show substeps")
               .NoBorder()
               .NoBackground()
               .NoPadding()
               .NoMinSize()
               .NoMargin()
               .Compact()
               .Class("tss-deepresearch-step-toggle");
            _toggleButton.OnClick(() => SetExpanded(!IsExpanded));

            _header = Stack().Horizontal().Gap(12.px()).AlignItems(ItemAlign.Start).WS().Class("tss-deepresearch-step-header").Children(
                _indicator,
                _content,
                _toggleButton.NoShrink()
            );

            _substepsHost = Stack().Vertical().Gap(8.px()).Class("tss-deepresearch-step-substeps").WS().ML(40);

            _root = Stack().Vertical().Gap(10.px()).Class("tss-deepresearch-step").WS().Children(
                _header,
                _substepsHost
            );

            SetModel(model);
        }

        public DeepResearchStepModel Model => _model;

        public string Key { get; private set; }

        public bool IsExpanded { get; private set; }

        public DeepResearchStepItem SetModel(DeepResearchStepModel model)
        {
            _model = model ?? new DeepResearchStepModel();

            var status = _model.Status;
            Key = DeepResearchVisuals.GetStepKey(_model, _model.Index);

            DeepResearchVisuals.ApplyStatusClass(_root.Render(), status);
            DeepResearchVisuals.ApplyStatusClass(_indicator.Render(), status);
            DeepResearchVisuals.ConfigureStatusBadge(_statusBadge, status);

            _indicatorIcon.SetIcon(DeepResearchVisuals.GetStatusIcon(status)).SetTitle(DeepResearchVisuals.GetStatusText(status));
            _titleText.Text = DeepResearchVisuals.FormatStepTitle(_model);

            _stageText.Text = _model.CurrentStage ?? string.Empty;
            if (DeepResearchVisuals.HasText(_model.CurrentStage))
            {
                _stageText.Show();
            }
            else
            {
                _stageText.Collapse();
            }

            var progress = DeepResearchVisuals.ClampProgress(_model.Progress);
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

            RenderSubsteps();

            var hasSubsteps = _model.Substeps != null && _model.Substeps.Count > 0;
            if (!hasSubsteps)
            {
                _toggleButton.Collapse();
                SetExpanded(false);
            }
            else
            {
                _toggleButton.Show();
                var shouldExpand = _model.IsExpanded ?? IsExpanded;

                if (!_model.IsExpanded.HasValue && !IsExpanded)
                {
                    shouldExpand = DeepResearchVisuals.ShouldExpandSubsteps(_model);
                }

                SetExpanded(shouldExpand);
            }

            return this;
        }

        public DeepResearchStepItem SetExpanded(bool expanded)
        {
            IsExpanded = expanded;

            if (IsExpanded && _substepItems.Count > 0)
            {
                _substepsHost.Show();
                _toggleButton.SetIcon(UIcons.AngleSmallUp).SetTitle("Hide substeps");
            }
            else
            {
                _substepsHost.Collapse();
                _toggleButton.SetIcon(UIcons.AngleSmallDown).SetTitle("Show substeps");
            }

            _root.Render().UpdateClassIf(IsExpanded, "tss-deepresearch-step-expanded");
            return this;
        }

        public H5.Core.dom.HTMLElement Render() => _root.Render();

        private void RenderSubsteps()
        {
            _substepItems.Clear();
            _substepsHost.Clear();

            foreach (var substep in _model.Substeps ?? new List<DeepResearchSubstepModel>())
            {
                var item = new DeepResearchSubstepItem(substep);
                _substepItems.Add(item);
                _substepsHost.Add(item);
            }
        }
    }
}
