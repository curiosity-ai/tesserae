using System;
using System.Collections.Generic;
using System.Globalization;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.PlanStatus")]
    public enum PlanStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Canceled
    }

    [H5.Name("tss.PlanModel")]
    public sealed class PlanModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public PlanStatus Status { get; set; }
        public float? Progress { get; set; }
        public string CurrentStage { get; set; }
        public IList<PlanStepModel> Steps { get; set; } = new List<PlanStepModel>();
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string FooterMessage { get; set; }
    }

    [H5.Name("tss.PlanStepModel")]
    public sealed class PlanStepModel
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public PlanStatus Status { get; set; }
        public float? Progress { get; set; }
        public string CurrentStage { get; set; }
        public IList<PlanSubstepModel> Substeps { get; set; } = new List<PlanSubstepModel>();
        public bool? IsExpanded { get; set; }
    }

    [H5.Name("tss.PlanSubstepModel")]
    public sealed class PlanSubstepModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public PlanStatus Status { get; set; }
        public string Message { get; set; }
    }

    [H5.Name("tss.Plan")]
    public sealed class Plan : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _innerElement;
        private readonly Card _card;
        private readonly Stack _mainStack;

        // Header
        private readonly Stack _headerStack;
        private readonly Stack _titleColumn;
        private readonly TextBlock _title;
        private readonly Badge _statusBadge;
        private readonly Stack _metaRow;
        private readonly TextBlock _summaryText;
        private readonly TextBlock _stageText;
        private readonly Stack _headerCommandsStack;

        // Task List
        private readonly Stack _tasksStack;
        private readonly TextBlock _emptyText;
        private readonly List<Task> _tasks = new List<Task>();

        // Footer Message
        private readonly Stack _footerMessageStack;
        private readonly TextBlock _footerMessage;
        private readonly TextBlock _timestampText;
        private readonly Stack _footerCommandsStack;

        // Progress
        private readonly Stack _progressStack;
        private readonly ProgressIndicator _progressIndicator;
        private readonly Button _startStopButton;

        private bool _indeterminateExplicit;
        private PlanStatus _status = PlanStatus.Pending;
        private int _positionalTaskKey;

        public Plan(string title)
        {
            _title = TextBlock(title).SemiBold().MediumPlus().Class("tss-plan-title").Grow();
            _statusBadge = Badge().Pill().Outline().Class("tss-plan-badge").Collapse();
            _headerCommandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center);

            var titleRow = Stack().Horizontal().JustifyContent(ItemJustify.Between).AlignItems(ItemAlign.Center).Width(100.percent()).Children(
                _title,
                _statusBadge.NoShrink(),
                _headerCommandsStack
            );

            _summaryText = TextBlock("").Small().Class("tss-plan-summary").Collapse();
            _stageText = TextBlock("").Small().SemiBold().Class("tss-plan-stage").Collapse();

            _metaRow = Stack().Horizontal().Wrap().Gap(8.px()).AlignItems(ItemAlign.Center).Width(100.percent()).Class("tss-plan-meta").Children(
                _summaryText,
                _stageText
            );

            _titleColumn = Stack().Vertical().Gap(6.px()).Width(100.percent()).Children(
                titleRow,
                _metaRow
            );

            _headerStack = Stack().Vertical().Width(100.percent()).Children(_titleColumn);

            _tasksStack = Stack().Vertical().Gap(16.px()).Width(100.percent()).PaddingBottom(16.px()).PaddingTop(16.px()).Class("tss-plan-steps");
            _emptyText = TextBlock("No steps yet.").Small().Secondary().Class("tss-plan-empty");

            _footerMessage = TextBlock("").Small();
            _timestampText = TextBlock("").XSmall().Secondary().Class("tss-plan-timestamps").Collapse();
            _footerCommandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center);

            var footerTopRow = Stack().Horizontal().JustifyContent(ItemJustify.Between).AlignItems(ItemAlign.Center).Width(100.percent()).Children(
                _footerMessage,
                _footerCommandsStack
            );

            _footerMessageStack = Stack().Vertical().Gap(4.px()).Width(100.percent()).Children(
                footerTopRow,
                _timestampText
            );

            _progressIndicator = ProgressIndicator().Indeterminated();
            _startStopButton = Button().SetIcon(UIcons.SquareSmall).NoBorder().NoBackground();
            _startStopButton.Render().classList.remove("tss-btn-default");
            _startStopButton.Render().classList.add("tss-btn-icon-only");

            _startStopButton.Render().style.width = "24px";
            _startStopButton.Render().style.height = "24px";
            _startStopButton.Render().style.minWidth = "24px";
            _startStopButton.Render().style.padding = "0";

            var progressContainer = Stack().Horizontal().AlignItems(ItemAlign.Center).Width(100.percent()).Grow().Children(_progressIndicator.Width(100.percent()));

            _progressStack = Stack().Horizontal().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Between).Width(100.percent()).Gap(16.px()).Children(
                progressContainer,
                _startStopButton
            );

            _mainStack = Stack().Vertical().Gap(16.px()).Padding(24.px()).Width(100.percent()).Class("tss-plan").Children(
                _headerStack,
                _tasksStack,
                _footerMessageStack,
                _progressStack
            );

            _card = Card(_mainStack).NoPadding().Class("tss-plan-card");
            _innerElement = _card.Render();
        }

        public HTMLElement Render() => _innerElement;

        public string Margin { get => _innerElement.style.margin; set => _innerElement.style.margin = value; }
        public string Padding { get => _innerElement.style.padding; set => _innerElement.style.padding = value; }

        public Plan Title(string title)
        {
            _title.Text = title;
            return this;
        }

        public Plan HeaderCommands(params IComponent[] commands)
        {
            _headerCommandsStack.Clear();
            foreach (var cmd in commands)
            {
                _headerCommandsStack.Add(cmd);
            }
            return this;
        }

        public Plan FooterMessage(string message)
        {
            _footerMessage.Text = message;
            return this;
        }

        public Plan FooterCommands(params IComponent[] commands)
        {
            _footerCommandsStack.Clear();
            foreach (var cmd in commands)
            {
                _footerCommandsStack.Add(cmd);
            }
            return this;
        }

        public Plan AddTask(string title, bool completed) => AddTask(title, completed ? PlanStatus.Completed : PlanStatus.Pending);

        public Plan AddTask(string title, PlanStatus status)
        {
            var model = new PlanStepModel
            {
                Id = $"tss-plan-step-{_positionalTaskKey++}",
                Title = title,
                Status = status
            };

            var task = new Task(model);
            _tasks.Add(task);

            if (_emptyText.Render().parentElement != null)
            {
                _tasksStack.Remove(_emptyText);
            }

            _tasksStack.Add(task);
            return this;
        }

        public Plan Progress(int position, int total)
        {
            _indeterminateExplicit = false;
            _progressIndicator.Progress(position, total);
            _progressStack.Show();
            return this;
        }

        public Plan Progress(float percent)
        {
            _indeterminateExplicit = false;
            _progressIndicator.Progress(percent);
            _progressStack.Show();
            return this;
        }

        public Plan Indeterminate()
        {
            _indeterminateExplicit = true;
            _progressIndicator.Indeterminated();
            _progressStack.Show();
            return this;
        }

        public Plan StartStopButton(Action<Button> onStartStop)
        {
            _startStopButton.OnClick((b, _) => onStartStop(b));
            return this;
        }

        public Plan HideStartStopButton()
        {
            _startStopButton.Collapse();
            return this;
        }

        public Plan ShowStartStopButton()
        {
            _startStopButton.Show();
            return this;
        }

        public Plan Start()
        {
            _startStopButton.SetIcon(UIcons.Play);
            return this;
        }

        public Plan Stop()
        {
            _startStopButton.SetIcon(UIcons.SquareSmall);
            return this;
        }

        /// <summary>
        /// Data-driven update path. Reconciles the plan, its steps and substeps in-place
        /// using stable keys so live updates do not tear down the DOM. The start/stop button
        /// and any header/footer commands set via the fluent API are preserved.
        /// </summary>
        public Plan SetModel(PlanModel model)
        {
            var m = model ?? new PlanModel();

            _status = m.Status;

            PlanVisuals.ApplyStatusClass(_mainStack.Render(), _status);
            _card.Border(PlanVisuals.GetBorderColor(_status));

            _statusBadge.Show();
            PlanVisuals.ConfigureStatusBadge(_statusBadge, _status);

            _title.Text = PlanVisuals.HasText(m.Title) ? m.Title : (PlanVisuals.HasText(_title.Text) ? _title.Text : "Plan");
            _summaryText.Text = PlanVisuals.BuildPlanSummary(m);
            _summaryText.Show();

            _stageText.Text = m.CurrentStage ?? string.Empty;
            if (PlanVisuals.HasText(m.CurrentStage)) { _stageText.Show(); } else { _stageText.Collapse(); }

            RenderSteps(m);

            var progress = PlanVisuals.ResolvePlanProgress(m);
            if (_status == PlanStatus.Running && !progress.HasValue)
            {
                _progressIndicator.Indeterminated();
                _progressStack.Show();
            }
            else if (progress.HasValue)
            {
                _progressIndicator.Progress(progress.Value);
                _progressStack.Show();
            }
            else
            {
                _progressStack.Collapse();
            }

            _footerMessage.Text = m.FooterMessage ?? string.Empty;

            _timestampText.Text = PlanVisuals.BuildTimestampText(m) ?? string.Empty;
            if (PlanVisuals.HasText(_timestampText.Text)) { _timestampText.Show(); } else { _timestampText.Collapse(); }

            return this;
        }

        private void RenderSteps(PlanModel model)
        {
            var steps = model.Steps ?? new List<PlanStepModel>();

            if (steps.Count == 0)
            {
                if (_tasks.Count > 0)
                {
                    foreach (var t in _tasks)
                    {
                        _tasksStack.Remove(t);
                    }
                    _tasks.Clear();
                }

                if (_emptyText.Render().parentElement == null)
                {
                    _tasksStack.Add(_emptyText);
                }

                return;
            }

            if (_emptyText.Render().parentElement != null)
            {
                _tasksStack.Remove(_emptyText);
            }

            var existingByKey = new Dictionary<string, Task>();
            foreach (var item in _tasks)
            {
                existingByKey[item.Key] = item;
            }

            var newTasks = new List<Task>(steps.Count);
            var reusedKeys = new HashSet<string>();

            for (var i = 0; i < steps.Count; i++)
            {
                var stepModel = steps[i] ?? new PlanStepModel();
                var stepKey = PlanVisuals.GetStepKey(stepModel, i);

                if (existingByKey.TryGetValue(stepKey, out var existing))
                {
                    existing.SetModel(stepModel);
                    newTasks.Add(existing);
                    reusedKeys.Add(stepKey);
                }
                else
                {
                    var task = new Task(stepModel);
                    newTasks.Add(task);
                }
            }

            foreach (var item in _tasks)
            {
                if (!reusedKeys.Contains(item.Key))
                {
                    _tasksStack.Remove(item);
                }
            }

            foreach (var item in newTasks)
            {
                if (item.Render().parentElement == null)
                {
                    _tasksStack.Add(item);
                }
            }

            _tasks.Clear();
            _tasks.AddRange(newTasks);
        }

        public sealed class Task : IComponent
        {
            private readonly Stack _root;
            private readonly Stack _header;
            private readonly Icon _icon;
            private readonly Stack _content;
            private readonly TextBlock _text;
            private readonly Stack _meta;
            private readonly TextBlock _stageText;
            private readonly Badge _statusBadge;
            private readonly ProgressIndicator _progressIndicator;
            private readonly Stack _progressHost;
            private readonly Button _toggleButton;
            private readonly Stack _substepsHost;
            private readonly List<Substep> _substepItems = new List<Substep>();
            private PlanStepModel _model;

            public string Key { get; private set; }
            public bool IsExpanded { get; private set; }

            public string Title
            {
                get => _text.Text;
                set => _text.Text = value;
            }

            public PlanStatus Status => _model.Status;

            public bool Completed
            {
                get => _model.Status == PlanStatus.Completed;
                set => SetModel(new PlanStepModel
                {
                    Id = _model.Id,
                    Index = _model.Index,
                    Title = _model.Title,
                    Status = value ? PlanStatus.Completed : PlanStatus.Pending,
                    Progress = _model.Progress,
                    CurrentStage = _model.CurrentStage,
                    Substeps = _model.Substeps,
                    IsExpanded = _model.IsExpanded
                });
            }

            public Task(PlanStepModel model)
            {
                _icon = Icon(UIcons.Circle).Class("tss-plan-step-icon").NoShrink();
                _text = TextBlock("").Regular().Class("tss-plan-step-title");

                _stageText = TextBlock("").XSmall().Class("tss-plan-step-stage").Collapse();
                _statusBadge = Badge().Pill().Outline().Class("tss-plan-step-badge").Collapse();

                _meta = Stack().Horizontal().Wrap().Gap(8.px()).AlignItems(ItemAlign.Center).Class("tss-plan-step-meta").Children(
                    _stageText,
                    _statusBadge
                );

                _progressIndicator = ProgressIndicator().Class("tss-plan-step-progress-indicator");
                _progressHost = Stack().Class("tss-plan-step-progress").Width(100.percent()).Children(_progressIndicator).Collapse();

                _content = Stack().Vertical().Gap(6.px()).Grow().Width(100.percent()).Class("tss-plan-step-content").Children(
                    _text,
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
                   .Class("tss-plan-step-toggle");
                _toggleButton.OnClick(() => SetExpanded(!IsExpanded));

                _header = Stack().Horizontal().Gap(12.px()).AlignItems(ItemAlign.Center).Width(100.percent()).Class("tss-plan-step-header").Children(
                    _icon,
                    _content,
                    _toggleButton.NoShrink()
                );

                _substepsHost = Stack().Vertical().Gap(8.px()).Class("tss-plan-step-substeps").Width(100.percent()).ML(36).Collapse();

                _root = Stack().Vertical().Gap(10.px()).Class("tss-plan-step").Width(100.percent()).Children(
                    _header,
                    _substepsHost
                );

                SetModel(model);
            }

            public PlanStepModel Model => _model;

            public Task SetModel(PlanStepModel model)
            {
                _model = model ?? new PlanStepModel();

                var status = _model.Status;
                Key = PlanVisuals.GetStepKey(_model, _model.Index);

                PlanVisuals.ApplyStatusClass(_root.Render(), status);
                PlanVisuals.ApplyStatusClass(_icon.Render(), status);

                _icon.SetIcon(PlanVisuals.GetStatusIcon(status)).SetTitle(PlanVisuals.GetStatusText(status));
                _text.Text = PlanVisuals.FormatStepTitle(_model);

                // Status badge only shown for non-default states to keep simple plans clean.
                if (status == PlanStatus.Pending)
                {
                    _statusBadge.Collapse();
                }
                else
                {
                    _statusBadge.Show();
                    PlanVisuals.ConfigureStatusBadge(_statusBadge, status);
                }

                _stageText.Text = _model.CurrentStage ?? string.Empty;
                if (PlanVisuals.HasText(_model.CurrentStage)) { _stageText.Show(); } else { _stageText.Collapse(); }

                var progress = PlanVisuals.ClampProgress(_model.Progress);
                if (status == PlanStatus.Running && !progress.HasValue)
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
                        shouldExpand = PlanVisuals.ShouldExpandSubsteps(_model);
                    }

                    SetExpanded(shouldExpand);
                }

                return this;
            }

            public Task SetExpanded(bool expanded)
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

                _root.Render().UpdateClassIf(IsExpanded, "tss-plan-step-expanded");
                return this;
            }

            public HTMLElement Render() => _root.Render();

            private void RenderSubsteps()
            {
                var substeps = _model.Substeps ?? new List<PlanSubstepModel>();

                var existingById = new Dictionary<string, Substep>();
                foreach (var item in _substepItems)
                {
                    var id = item.Model?.Id;
                    if (!string.IsNullOrEmpty(id))
                    {
                        existingById[id] = item;
                    }
                }

                var newSubstepItems = new List<Substep>(substeps.Count);
                var reusedIds = new HashSet<string>();

                foreach (var substep in substeps)
                {
                    var id = substep?.Id;
                    if (!string.IsNullOrEmpty(id) && existingById.TryGetValue(id, out var existing))
                    {
                        existing.SetModel(substep);
                        newSubstepItems.Add(existing);
                        reusedIds.Add(id);
                    }
                    else
                    {
                        var item = new Substep(substep);
                        newSubstepItems.Add(item);
                    }
                }

                foreach (var item in _substepItems)
                {
                    var id = item.Model?.Id;
                    if (string.IsNullOrEmpty(id) || !reusedIds.Contains(id))
                    {
                        _substepsHost.Remove(item);
                    }
                }

                foreach (var item in newSubstepItems)
                {
                    if (item.Render().parentElement == null)
                    {
                        _substepsHost.Add(item);
                    }
                }

                _substepItems.Clear();
                _substepItems.AddRange(newSubstepItems);
            }
        }

        public sealed class Substep : IComponent
        {
            private readonly Stack _root;
            private readonly Stack _header;
            private readonly Icon _statusIcon;
            private readonly TextBlock _titleText;
            private readonly TextBlock _statusText;
            private readonly TextBlock _messageText;
            private PlanSubstepModel _model;

            public Substep(PlanSubstepModel model = null)
            {
                _statusIcon = Icon(UIcons.Clock).Class("tss-plan-substep-icon").NoShrink();
                _titleText = TextBlock("").Small().SemiBold().Class("tss-plan-substep-title").Grow();
                _statusText = TextBlock("").XSmall().SemiBold().Class("tss-plan-substep-status").NoShrink();
                _messageText = TextBlock("").XSmall().Secondary().Class("tss-plan-substep-message").Collapse();

                _header = Stack().Horizontal().AlignItems(ItemAlign.Center).Gap(8.px()).Width(100.percent()).Children(
                    _statusIcon,
                    _titleText,
                    _statusText
                );

                _root = Stack().Vertical().Gap(4.px()).Class("tss-plan-substep").Width(100.percent()).Children(
                    _header,
                    _messageText.ML(24)
                );

                SetModel(model);
            }

            public PlanSubstepModel Model => _model;

            public Substep SetModel(PlanSubstepModel model)
            {
                _model = model ?? new PlanSubstepModel();

                var status = _model.Status;

                PlanVisuals.ApplyStatusClass(_root.Render(), status);
                _statusIcon.SetIcon(PlanVisuals.GetStatusIcon(status)).SetTitle(PlanVisuals.GetStatusText(status));
                _titleText.Text = PlanVisuals.FormatSubstepTitle(_model);
                _statusText.Text = PlanVisuals.GetStatusText(status);
                _messageText.Text = _model.Message ?? string.Empty;

                if (PlanVisuals.HasText(_model.Message)) { _messageText.Show(); } else { _messageText.Collapse(); }

                return this;
            }

            public HTMLElement Render() => _root.Render();
        }
    }

    internal static class PlanVisuals
    {
        private static readonly string[] StatusClasses =
        {
            "tss-plan-pending",
            "tss-plan-running",
            "tss-plan-completed",
            "tss-plan-failed",
            "tss-plan-canceled"
        };

        public static string GetStatusClass(PlanStatus status) => $"tss-plan-{status.ToString().ToLower()}";

        public static void ApplyStatusClass(HTMLElement element, PlanStatus status)
        {
            if (element == null)
            {
                return;
            }

            element.classList.remove(StatusClasses);
            element.classList.add(GetStatusClass(status));
        }

        public static void ConfigureStatusBadge(Badge badge, PlanStatus status)
        {
            badge.SetText(GetStatusText(status))
               .SetIcon(GetStatusIcon(status).ToString())
               .Pill()
               .Outline()
               .Neutral();

            switch (status)
            {
                case PlanStatus.Running:
                    badge.Primary();
                    break;
                case PlanStatus.Completed:
                    badge.Success();
                    break;
                case PlanStatus.Failed:
                    badge.Danger();
                    break;
                case PlanStatus.Canceled:
                    badge.Warning();
                    break;
            }
        }

        public static UIcons GetStatusIcon(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:
                    return UIcons.ArrowProgressAlt;
                case PlanStatus.Completed:
                    return UIcons.CheckCircle;
                case PlanStatus.Failed:
                    return UIcons.CircleXmark;
                case PlanStatus.Canceled:
                    return UIcons.Ban;
                default:
                    return UIcons.Circle;
            }
        }

        public static string GetStatusText(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:
                    return "Running";
                case PlanStatus.Completed:
                    return "Completed";
                case PlanStatus.Failed:
                    return "Failed";
                case PlanStatus.Canceled:
                    return "Canceled";
                default:
                    return "Pending";
            }
        }

        public static string GetBorderColor(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:
                    return "var(--tss-primary-background-color)";
                case PlanStatus.Completed:
                    return "var(--tss-success-background-color)";
                case PlanStatus.Failed:
                    return "var(--tss-danger-background-color)";
                case PlanStatus.Canceled:
                    return "var(--tss-warning-background-color)";
                default:
                    return "var(--tss-default-border-color)";
            }
        }

        public static float? ClampProgress(float? progress)
        {
            if (!progress.HasValue)
            {
                return null;
            }

            return Math.Max(0f, Math.Min(100f, progress.Value));
        }

        public static float? ResolvePlanProgress(PlanModel model)
        {
            var explicitProgress = ClampProgress(model?.Progress);

            if (explicitProgress.HasValue)
            {
                return explicitProgress;
            }

            if (model == null)
            {
                return null;
            }

            if (model.Status == PlanStatus.Completed)
            {
                return 100f;
            }

            var totalSteps = model.Steps?.Count ?? 0;

            if (totalSteps == 0)
            {
                return null;
            }

            var completed = 0;

            foreach (var step in model.Steps)
            {
                if ((step?.Status ?? PlanStatus.Pending) == PlanStatus.Completed)
                {
                    completed++;
                }
            }

            return 100f * completed / totalSteps;
        }

        public static string FormatProgress(float? progress)
        {
            var value = ClampProgress(progress);
            return value.HasValue ? $"{Math.Round(value.Value)}%" : null;
        }

        public static string BuildPlanSummary(PlanModel model)
        {
            var totalSteps = model?.Steps?.Count ?? 0;

            if (totalSteps == 0)
            {
                return "No steps yet";
            }

            var completed = 0;
            var running = 0;
            var failed = 0;
            var canceled = 0;

            foreach (var step in model.Steps)
            {
                switch (step?.Status ?? PlanStatus.Pending)
                {
                    case PlanStatus.Completed:
                        completed++;
                        break;
                    case PlanStatus.Running:
                        running++;
                        break;
                    case PlanStatus.Failed:
                        failed++;
                        break;
                    case PlanStatus.Canceled:
                        canceled++;
                        break;
                }
            }

            var summary = $"{completed} of {totalSteps} steps complete";

            if (running > 0)
            {
                summary += $" | {running} running";
            }

            if (failed > 0)
            {
                summary += $" | {failed} failed";
            }

            if (canceled > 0)
            {
                summary += $" | {canceled} canceled";
            }

            return summary;
        }

        public static string BuildTimestampText(PlanModel model)
        {
            if (model == null)
            {
                return null;
            }

            var started = model.StartedAt.HasValue ? $"Started {FormatTimestamp(model.StartedAt.Value)}" : null;
            var completed = model.CompletedAt.HasValue ? $"Completed {FormatTimestamp(model.CompletedAt.Value)}" : null;

            if (HasText(started) && HasText(completed))
            {
                return started + " | " + completed;
            }

            return started ?? completed;
        }

        public static bool ShouldExpandSubsteps(PlanStepModel model)
        {
            if (model == null || model.Substeps == null || model.Substeps.Count == 0)
            {
                return false;
            }

            return model.Status == PlanStatus.Running || model.Status == PlanStatus.Failed;
        }

        public static string GetStepKey(PlanStepModel model, int fallbackIndex)
        {
            if (model != null && HasText(model.Id))
            {
                return model.Id;
            }

            return $"tss-plan-step-{fallbackIndex}";
        }

        public static string FormatStepTitle(PlanStepModel model)
        {
            var title = HasText(model?.Title) ? model.Title : "Untitled step";

            if ((model?.Index ?? 0) > 0)
            {
                return $"{model.Index}. {title}";
            }

            return title;
        }

        public static string FormatSubstepTitle(PlanSubstepModel model)
        {
            return HasText(model?.Title) ? model.Title : "Update";
        }

        public static bool HasText(string value) => !string.IsNullOrWhiteSpace(value);

        private static string FormatTimestamp(DateTimeOffset value)
        {
            return value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
