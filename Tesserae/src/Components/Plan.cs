using System;
using System.Collections.Generic;
using System.Linq;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Status of a <see cref="Plan"/> (or one of its steps / substeps).
    /// </summary>
    public enum PlanStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Canceled,
    }

    /// <summary>
    /// Plain data-model used by <see cref="Plan.SetModel"/> to drive the
    /// component from a streaming/server-side source. All fields are
    /// optional except <see cref="Steps"/> (which must not be null).
    /// </summary>
    public sealed class PlanModel
    {
        public string Id;
        public string Title;
        public PlanStatus Status;
        public float? Progress;
        public string CurrentStage;
        public IList<PlanStepModel> Steps = new List<PlanStepModel>();
        public DateTimeOffset? StartedAt;
        public DateTimeOffset? CompletedAt;
        public string FooterMessage;
    }

    /// <summary>
    /// Plain data-model for a single step inside a <see cref="PlanModel"/>.
    /// </summary>
    public sealed class PlanStepModel
    {
        public string Id;
        public int Index;
        public string Title;
        public PlanStatus Status;
        public float? Progress;
        public string CurrentStage;
        public IList<PlanSubstepModel> Substeps;
        public bool? IsExpanded;
    }

    /// <summary>
    /// Plain data-model for a substep inside a <see cref="PlanStepModel"/>.
    /// </summary>
    public sealed class PlanSubstepModel
    {
        public string Id;
        public string Title;
        public PlanStatus Status;
        public string Message;
    }

    /// <summary>
    /// Internal helpers shared across the Plan component: status →
    /// css class / icon / display text, progress clamping, summary
    /// derivation and step-key derivation.
    /// </summary>
    internal static class PlanVisuals
    {
        public static string StatusClass(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return "tss-plan-running";
                case PlanStatus.Completed: return "tss-plan-completed";
                case PlanStatus.Failed:    return "tss-plan-failed";
                case PlanStatus.Canceled:  return "tss-plan-canceled";
                default:                   return "tss-plan-pending";
            }
        }

        public static UIcons StatusIcon(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return UIcons.Loading;
                case PlanStatus.Completed: return UIcons.CheckCircle;
                case PlanStatus.Failed:    return UIcons.CrossCircle;
                case PlanStatus.Canceled:  return UIcons.Ban;
                default:                   return UIcons.Circle;
            }
        }

        public static string StatusText(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return "Running";
                case PlanStatus.Completed: return "Completed";
                case PlanStatus.Failed:    return "Failed";
                case PlanStatus.Canceled:  return "Canceled";
                default:                   return "Pending";
            }
        }

        public static float ClampProgress(float value)
        {
            if (float.IsNaN(value)) return 0f;
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }

        public static bool DefaultExpanded(PlanStepModel step)
        {
            if (step.IsExpanded.HasValue) return step.IsExpanded.Value;
            // Auto-expand when running / failed; auto-collapse otherwise.
            return step.Status == PlanStatus.Running || step.Status == PlanStatus.Failed;
        }

        public static string StepKey(PlanStepModel step, int positionalIndex)
        {
            if (!string.IsNullOrEmpty(step.Id)) return step.Id;
            return "tss-plan-step-" + positionalIndex;
        }

        public static string SubstepKey(PlanSubstepModel substep, int positionalIndex)
        {
            if (!string.IsNullOrEmpty(substep.Id)) return substep.Id;
            return "tss-plan-substep-" + positionalIndex;
        }

        public static string Summary(PlanModel model)
        {
            int total = model.Steps == null ? 0 : model.Steps.Count;
            int completed = 0, running = 0, failed = 0;
            if (model.Steps != null)
            {
                foreach (var s in model.Steps)
                {
                    if (s.Status == PlanStatus.Completed) completed++;
                    else if (s.Status == PlanStatus.Running) running++;
                    else if (s.Status == PlanStatus.Failed) failed++;
                }
            }
            var parts = new List<string>();
            parts.Add(completed + " of " + total + " steps complete");
            if (running > 0) parts.Add(running + " running");
            if (failed > 0)  parts.Add(failed + " failed");
            return string.Join(" | ", parts);
        }
    }

    /// <summary>
    /// A timeline-style display for showing a multi-step plan, with optional
    /// per-step status / progress / substeps. Can be driven imperatively via
    /// the fluent <see cref="AddTask(string,bool)"/> API or declaratively via
    /// <see cref="SetModel"/>; <see cref="SetModel"/> performs an in-place
    /// reconcile keyed by step/substep <c>Id</c> so DOM nodes are reused.
    /// </summary>
    [H5.Name("tss.Plan")]
    public sealed class Plan : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _innerElement;
        private readonly Card _card;
        private readonly Stack _mainStack;

        // Header
        private readonly Stack _headerStack;
        private readonly TextBlock _title;
        private readonly HTMLElement _statusBadge;
        private readonly Stack _headerCommandsStack;

        // Task List
        private readonly Stack _tasksStack;
        private readonly List<Task> _tasks = new List<Task>();
        private readonly Dictionary<string, Task> _tasksByKey = new Dictionary<string, Task>();

        // Footer Message
        private readonly Stack _footerMessageStack;
        private readonly TextBlock _footerMessage;
        private readonly HTMLElement _summaryLine;
        private readonly HTMLElement _timestampsLine;
        private readonly Stack _footerCommandsStack;

        // Progress
        private readonly Stack _progressStack;
        private readonly ProgressIndicator _progressIndicator;
        private readonly Button _startStopButton;

        // Model
        private PlanModel _model;

        /// <summary>
        /// Gets the last <see cref="PlanModel"/> passed to <see cref="SetModel"/>,
        /// or <c>null</c> if the component has only been driven by the fluent API.
        /// </summary>
        public PlanModel Model => _model;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Plan(string title)
        {
            _title = TextBlock(title).SemiBold().MediumPlus();

            _statusBadge = Span(_("tss-plan-status-badge tss-plan-pending"));
            _statusBadge.style.display = "none";

            _headerCommandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center);

            var titleRow = Stack().Horizontal().AlignItems(ItemAlign.Center).Gap(8.px()).Children(
                _title,
                Raw(_statusBadge)
            );

            _headerStack = Stack().Horizontal().JustifyContent(ItemJustify.Between).AlignItems(ItemAlign.Center).Width(100.percent()).Children(
                titleRow,
                _headerCommandsStack
            );

            _tasksStack = Stack().Vertical().Gap(16.px()).Width(100.percent()).PaddingBottom(16.px()).PaddingTop(16.px());

            _footerMessage = TextBlock("").Small();
            _summaryLine = Div(_("tss-plan-summary"));
            _summaryLine.style.display = "none";
            _timestampsLine = Div(_("tss-plan-timestamps"));
            _timestampsLine.style.display = "none";

            var footerLeft = Stack().Vertical().AlignItems(ItemAlign.Start).Gap(2.px()).Children(
                _footerMessage,
                Raw(_summaryLine),
                Raw(_timestampsLine)
            );

            _footerCommandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center);

            _footerMessageStack = Stack().Horizontal().JustifyContent(ItemJustify.Between).AlignItems(ItemAlign.Center).Width(100.percent()).Children(
                footerLeft,
                _footerCommandsStack
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

            _mainStack = Stack().Vertical().Gap(16.px()).Padding(24.px()).Width(100.percent()).Children(
                _headerStack,
                _tasksStack,
                _footerMessageStack,
                _progressStack
            );

            _card = Card(_mainStack).NoPadding();
            _innerElement = _card.Render();
            _innerElement.classList.add("tss-plan");
            _innerElement.classList.add("tss-plan-pending");
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _innerElement;

        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string Margin { get => _innerElement.style.margin; set => _innerElement.style.margin = value; }
        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string Padding { get => _innerElement.style.padding; set => _innerElement.style.padding = value; }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public Plan Title(string title)
        {
            _title.Text = title;
            return this;
        }

        /// <summary>
        /// Sets the components shown in the header command area.
        /// </summary>
        public Plan HeaderCommands(params IComponent[] commands)
        {
            _headerCommandsStack.Clear();
            foreach (var cmd in commands)
            {
                _headerCommandsStack.Add(cmd);
            }
            return this;
        }

        /// <summary>
        /// Sets a message shown in the footer.
        /// </summary>
        public Plan FooterMessage(string message)
        {
            _footerMessage.Text = message;
            return this;
        }

        /// <summary>
        /// Sets the components shown in the footer command area.
        /// </summary>
        public Plan FooterCommands(params IComponent[] commands)
        {
            _footerCommandsStack.Clear();
            foreach (var cmd in commands)
            {
                _footerCommandsStack.Add(cmd);
            }
            return this;
        }

        /// <summary>
        /// Adds the given task to the component.
        ///
        /// Note: completed tasks now render with the unified "success" status
        /// color (green check) instead of the previous primary-tone treatment;
        /// this matches the generalized status styling shared with SetModel.
        /// </summary>
        public Plan AddTask(string title, bool completed)
        {
            return AddTask(title, completed ? PlanStatus.Completed : PlanStatus.Pending);
        }

        /// <summary>
        /// Adds a task with an explicit <see cref="PlanStatus"/>.
        /// Auto-generates a positional key so a later <see cref="SetModel"/>
        /// call still reconciles correctly.
        /// </summary>
        public Plan AddTask(string title, PlanStatus status)
        {
            var key = "tss-plan-step-" + _tasks.Count;
            var task = new Task(key, title, status);
            _tasks.Add(task);
            _tasksByKey[key] = task;
            _tasksStack.Add(task);
            return this;
        }

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
        public Plan Progress(int position, int total)
        {
            _progressIndicator.Progress(position, total);
            return this;
        }

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
        public Plan Progress(float percent)
        {
            _progressIndicator.Progress(percent);
            return this;
        }

        /// <summary>
        /// Configures the component to indeterminate.
        /// </summary>
        public Plan Indeterminate()
        {
            _progressIndicator.Indeterminated();
            return this;
        }

        /// <summary>
        /// Adds a start / stop toggle button wired up via the supplied callback.
        /// </summary>
        public Plan StartStopButton(Action<Button> onStartStop)
        {
            _startStopButton.OnClick((b, _) => onStartStop(b));
            return this;
        }

        /// <summary>
        /// Hides the start stop button.
        /// </summary>
        public Plan HideStartStopButton()
        {
            _startStopButton.Collapse();
            return this;
        }

        /// <summary>
        /// Shows the start stop button.
        /// </summary>
        public Plan ShowStartStopButton()
        {
            _startStopButton.Show();
            return this;
        }

        /// <summary>
        /// Starts the component's operation.
        /// </summary>
        public Plan Start()
        {
            _startStopButton.SetIcon(UIcons.Play);
            return this;
        }

        /// <summary>
        /// Stops the component's operation.
        /// </summary>
        public Plan Stop()
        {
            _startStopButton.SetIcon(UIcons.SquareSmall);
            return this;
        }

        /// <summary>
        /// Applies the supplied <see cref="PlanModel"/> to the component, updating
        /// the DOM in place. Steps and substeps are matched by their <c>Id</c>
        /// (or a derived positional key when no id is provided) so existing DOM
        /// nodes are reused across calls — animations, focus and scroll position
        /// are preserved as long as the keys are stable.
        /// </summary>
        public Plan SetModel(PlanModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            _model = model;

            // Title
            if (model.Title != null) _title.Text = model.Title;

            // Plan-level status (card border + badge)
            ApplyCardStatusClass(model.Status);
            ApplyBadge(model.Status);

            // Plan-level progress
            if (model.Progress.HasValue)
            {
                var p = PlanVisuals.ClampProgress(model.Progress.Value);
                _progressIndicator.Progress(p * 100f);
            }
            else if (model.Status == PlanStatus.Running)
            {
                _progressIndicator.Indeterminated();
            }
            else if (model.Status == PlanStatus.Completed)
            {
                _progressIndicator.Progress(100f);
            }

            // Footer message + summary + timestamps
            if (model.FooterMessage != null) _footerMessage.Text = model.FooterMessage;

            var summary = PlanVisuals.Summary(model);
            _summaryLine.innerText = summary;
            _summaryLine.style.display = string.IsNullOrEmpty(summary) ? "none" : "";

            var timestampParts = new List<string>();
            if (model.StartedAt.HasValue) timestampParts.Add("started " + model.StartedAt.Value.LocalDateTime.ToString());
            if (model.CompletedAt.HasValue) timestampParts.Add("completed " + model.CompletedAt.Value.LocalDateTime.ToString());
            if (timestampParts.Count > 0)
            {
                _timestampsLine.innerText = string.Join(" | ", timestampParts);
                _timestampsLine.style.display = "";
            }
            else
            {
                _timestampsLine.style.display = "none";
            }

            // Steps reconcile-by-key
            ReconcileSteps(model.Steps ?? new List<PlanStepModel>());

            return this;
        }

        private void ApplyCardStatusClass(PlanStatus status)
        {
            _innerElement.classList.remove("tss-plan-pending");
            _innerElement.classList.remove("tss-plan-running");
            _innerElement.classList.remove("tss-plan-completed");
            _innerElement.classList.remove("tss-plan-failed");
            _innerElement.classList.remove("tss-plan-canceled");
            _innerElement.classList.add(PlanVisuals.StatusClass(status));
        }

        private void ApplyBadge(PlanStatus status)
        {
            _statusBadge.classList.remove("tss-plan-pending");
            _statusBadge.classList.remove("tss-plan-running");
            _statusBadge.classList.remove("tss-plan-completed");
            _statusBadge.classList.remove("tss-plan-failed");
            _statusBadge.classList.remove("tss-plan-canceled");
            _statusBadge.classList.add(PlanVisuals.StatusClass(status));
            _statusBadge.innerText = PlanVisuals.StatusText(status);
            _statusBadge.style.display = "";
        }

        private void ReconcileSteps(IList<PlanStepModel> steps)
        {
            var newKeys = new List<string>(steps.Count);
            var seen = new HashSet<string>();

            // Pass 1: update existing or create new, in the requested order.
            for (int i = 0; i < steps.Count; i++)
            {
                var stepModel = steps[i];
                var key = PlanVisuals.StepKey(stepModel, i);
                newKeys.Add(key);
                seen.Add(key);

                Task task;
                if (_tasksByKey.TryGetValue(key, out task))
                {
                    task.UpdateFromModel(stepModel);
                }
                else
                {
                    task = new Task(key, stepModel.Title ?? string.Empty, stepModel.Status);
                    task.UpdateFromModel(stepModel);
                    _tasksByKey[key] = task;
                }
            }

            // Pass 2: drop tasks whose key is no longer present.
            var stale = _tasks.Where(t => !seen.Contains(t.Key)).ToList();
            foreach (var t in stale)
            {
                _tasks.Remove(t);
                _tasksByKey.Remove(t.Key);
                var el = t.Render();
                if (el != null && el.parentNode != null)
                {
                    el.parentNode.removeChild(el);
                }
            }

            // Pass 3: re-order DOM to match the new ordering, reusing nodes.
            // We append in the new order; appendChild moves the node if it
            // already has a parent, which is exactly what we want.
            var stackEl = _tasksStack.Render();
            _tasks.Clear();
            foreach (var key in newKeys)
            {
                var t = _tasksByKey[key];
                _tasks.Add(t);
                stackEl.appendChild(t.Render());
            }
        }

        /// <summary>
        /// A single row in a <see cref="Plan"/>'s task list. Backed by a
        /// stable key so the parent <see cref="Plan.SetModel"/> reconciler
        /// can reuse the DOM across updates.
        /// </summary>
        public class Task : IComponent
        {
            private readonly HTMLElement _root;
            private readonly HTMLElement _stepRow;
            private readonly Icon _icon;
            private readonly TextBlock _text;
            private readonly HTMLElement _stageEl;
            private readonly HTMLElement _toggleEl;
            private readonly HTMLElement _progressEl;
            private readonly HTMLElement _progressBar;
            private readonly HTMLElement _substepsContainer;
            private readonly Dictionary<string, HTMLElement> _substepsByKey = new Dictionary<string, HTMLElement>();
            private readonly List<string> _substepOrder = new List<string>();
            private PlanStatus _status;
            private bool _expanded;

            internal string Key { get; private set; }

            /// <summary>
            /// Gets or sets the title of the component.
            /// </summary>
            public string Title
            {
                get => _text.Text;
                set => _text.Text = value;
            }

            /// <summary>
            /// Gets or sets the current status of the task.
            /// </summary>
            public PlanStatus Status
            {
                get => _status;
                set => ApplyStatus(value);
            }

            /// <summary>
            /// Backwards-compatible flag. <c>true</c> when the status is
            /// <see cref="PlanStatus.Completed"/>; setting <c>true</c> moves
            /// the task to Completed and setting <c>false</c> moves it to
            /// Pending.
            /// </summary>
            public bool Completed
            {
                get => _status == PlanStatus.Completed;
                set => Status = value ? PlanStatus.Completed : PlanStatus.Pending;
            }

            internal Task(string key, string title, PlanStatus status)
            {
                Key = key;
                _status = status;

                _icon = Icon(PlanVisuals.StatusIcon(status));
                _icon.Render().classList.add("tss-plan-step-icon");
                _icon.Render().classList.add(PlanVisuals.StatusClass(status));

                _text = TextBlock(title).Regular();

                _stageEl = Div(_("tss-plan-step-stage"));
                _stageEl.style.display = "none";

                _toggleEl = Span(_("tss-plan-step-toggle"));
                _toggleEl.innerText = "▸";
                _toggleEl.style.display = "none";
                _toggleEl.onclick = e => { ToggleExpanded(); };

                var titleRow = Div(_("tss-plan-step-title"));
                titleRow.appendChild(_text.Render());
                titleRow.appendChild(_toggleEl);

                _progressBar = Div(_("tss-plan-step-progress-bar"));
                _progressEl = Div(_("tss-plan-step-progress"), _progressBar);
                _progressEl.style.display = "none";

                var stepBody = Div(_("tss-plan-step-body"));
                stepBody.appendChild(titleRow);
                stepBody.appendChild(_stageEl);
                stepBody.appendChild(_progressEl);

                _stepRow = Div(_("tss-plan-step-row"));
                _stepRow.appendChild(_icon.Render());
                _stepRow.appendChild(stepBody);

                _substepsContainer = Div(_("tss-plan-substeps tss-plan-collapsed"));

                _root = Div(_("tss-plan-step"));
                _root.appendChild(_stepRow);
                _root.appendChild(_substepsContainer);
            }

            /// <summary>
            /// Backwards-compatible ctor used when callers built tasks via the
            /// old `.AddTask(string, bool)` shape directly.
            /// </summary>
            public Task(string title, bool completed)
                : this("tss-plan-step-task-" + Guid.NewGuid().ToString("N"), title, completed ? PlanStatus.Completed : PlanStatus.Pending)
            {
            }

            /// <summary>
            /// Renders the component's root HTML element.
            /// </summary>
            public HTMLElement Render() => _root;

            internal void UpdateFromModel(PlanStepModel model)
            {
                if (model.Title != null) _text.Text = model.Title;

                ApplyStatus(model.Status);

                if (!string.IsNullOrEmpty(model.CurrentStage))
                {
                    _stageEl.innerText = model.CurrentStage;
                    _stageEl.style.display = "";
                }
                else
                {
                    _stageEl.style.display = "none";
                }

                // Progress: explicit value wins, otherwise indeterminate when running.
                if (model.Progress.HasValue)
                {
                    var p = PlanVisuals.ClampProgress(model.Progress.Value);
                    _progressEl.classList.remove("tss-plan-indeterminate");
                    _progressBar.style.width = (p * 100f).ToString() + "%";
                    _progressEl.style.display = "";
                }
                else if (model.Status == PlanStatus.Running)
                {
                    _progressEl.classList.add("tss-plan-indeterminate");
                    _progressEl.style.display = "";
                }
                else
                {
                    _progressEl.style.display = "none";
                }

                // Substeps reconcile by key
                ReconcileSubsteps(model.Substeps ?? new List<PlanSubstepModel>());

                // Expansion (auto unless overridden)
                var hasSubsteps = model.Substeps != null && model.Substeps.Count > 0;
                _toggleEl.style.display = hasSubsteps ? "" : "none";
                SetExpanded(hasSubsteps && PlanVisuals.DefaultExpanded(model));
            }

            private void ApplyStatus(PlanStatus status)
            {
                _status = status;
                _icon.SetIcon(PlanVisuals.StatusIcon(status));
                _icon.Render().classList.add("tss-plan-step-icon");
                _icon.Render().classList.remove("tss-plan-pending");
                _icon.Render().classList.remove("tss-plan-running");
                _icon.Render().classList.remove("tss-plan-completed");
                _icon.Render().classList.remove("tss-plan-failed");
                _icon.Render().classList.remove("tss-plan-canceled");
                _icon.Render().classList.add(PlanVisuals.StatusClass(status));
            }

            private void ToggleExpanded()
            {
                SetExpanded(!_expanded);
            }

            private void SetExpanded(bool expanded)
            {
                _expanded = expanded;
                if (expanded)
                {
                    _substepsContainer.classList.remove("tss-plan-collapsed");
                    _toggleEl.innerText = "▾";
                }
                else
                {
                    _substepsContainer.classList.add("tss-plan-collapsed");
                    _toggleEl.innerText = "▸";
                }
            }

            private void ReconcileSubsteps(IList<PlanSubstepModel> substeps)
            {
                var newKeys = new List<string>(substeps.Count);
                var seen = new HashSet<string>();

                for (int i = 0; i < substeps.Count; i++)
                {
                    var sub = substeps[i];
                    var key = PlanVisuals.SubstepKey(sub, i);
                    newKeys.Add(key);
                    seen.Add(key);

                    HTMLElement el;
                    if (_substepsByKey.TryGetValue(key, out el))
                    {
                        UpdateSubstepEl(el, sub);
                    }
                    else
                    {
                        el = BuildSubstepEl(sub);
                        _substepsByKey[key] = el;
                    }
                }

                // Drop stale
                var stale = _substepOrder.Where(k => !seen.Contains(k)).ToList();
                foreach (var k in stale)
                {
                    var el = _substepsByKey[k];
                    _substepsByKey.Remove(k);
                    if (el.parentNode != null) el.parentNode.removeChild(el);
                }

                // Re-order
                _substepOrder.Clear();
                foreach (var k in newKeys)
                {
                    _substepOrder.Add(k);
                    _substepsContainer.appendChild(_substepsByKey[k]);
                }
            }

            private static HTMLElement BuildSubstepEl(PlanSubstepModel sub)
            {
                var iconEl = I(_("tss-icon tss-plan-substep-icon"));
                var titleEl = Div(_("tss-plan-substep-title"));
                titleEl.innerText = sub.Title ?? string.Empty;
                var msgEl = Div(_("tss-plan-substep-message"));
                var body = Div(_("tss-plan-substep-body"), titleEl, msgEl);
                var root = Div(_("tss-plan-substep"), iconEl, body);
                UpdateSubstepEl(root, sub);
                return root;
            }

            private static void UpdateSubstepEl(HTMLElement root, PlanSubstepModel sub)
            {
                // Children: [iconEl, body[titleEl, msgEl]]
                var iconEl = (HTMLElement)root.childNodes[0];
                var body = (HTMLElement)root.childNodes[1];
                var titleEl = (HTMLElement)body.childNodes[0];
                var msgEl = (HTMLElement)body.childNodes[1];

                // Title
                titleEl.innerText = sub.Title ?? string.Empty;

                // Icon (UIcons font class) — clear previous icon classes
                // and re-apply just our marker + the new icon class.
                var newIconClass = Tesserae.Icon.Transform(PlanVisuals.StatusIcon(sub.Status), UIconsWeight.Regular);
                iconEl.className = "tss-icon tss-plan-substep-icon " + PlanVisuals.StatusClass(sub.Status) + " " + newIconClass + " " + TextSize.Small.ToString();

                // Message
                if (!string.IsNullOrEmpty(sub.Message))
                {
                    msgEl.innerText = sub.Message;
                    msgEl.style.display = "";
                }
                else
                {
                    msgEl.style.display = "none";
                }
            }
        }
    }
}
