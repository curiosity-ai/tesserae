using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
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

        /// <summary>
        /// In progress, but without work streaming in: styled like <see cref="Running"/>
        /// (primary accent, bold text) but with a static circle instead of a spinner.
        /// Appended last so the existing values keep their numeric ordinals.
        /// </summary>
        Active,
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
        /// <summary>
        /// Optional short count shown on the right of the footer strip
        /// (e.g. <c>"117 searches"</c>).
        /// </summary>
        public string Searches;
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

        /// <summary>
        /// Optional component rendered at the end of the step's title row — typically a
        /// <see cref="Tesserae.Badge"/>, but any <see cref="IComponent"/> works.
        /// </summary>
        public IComponent Badge;
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
        /// <summary>
        /// Plan-level status class applied to the card root and used to drive
        /// the <c>--plan-accent</c> custom properties (e.g. <c>tss-plan-running</c>).
        /// </summary>
        public static string StatusClass(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return "tss-plan-running";
                case PlanStatus.Active:    return "tss-plan-active";
                case PlanStatus.Completed: return "tss-plan-completed";
                case PlanStatus.Failed:    return "tss-plan-failed";
                case PlanStatus.Canceled:  return "tss-plan-canceled";
                default:                   return "tss-plan-pending";
            }
        }

        /// <summary>
        /// Step/substep status class (e.g. <c>is-running</c>) that drives the
        /// timeline rail segment color, icon color and active-panel styling.
        /// </summary>
        public static string StepStatusClass(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return "is-running";
                case PlanStatus.Active:    return "is-active";
                case PlanStatus.Completed: return "is-completed";
                case PlanStatus.Failed:    return "is-failed";
                case PlanStatus.Canceled:  return "is-canceled";
                default:                   return "is-pending";
            }
        }

        // ---- Status glyphs ------------------------------------------------
        //
        // Status glyphs come from the bundled UIcons font set — the same icon
        // set used across the rest of the toolkit — so they inherit `color`
        // from the per-status `.is-*` CSS rules like any other icon, and there
        // is no inline SVG in the component.
        //
        // The Running glyph is a spinner rotated by CSS. For a font glyph to
        // spin *in place* instead of orbiting off-center, the icon element must
        // be a glyph-centered square with `transform-origin: 50% 50%`; the
        // `.tss-plan-step-icon .tss-icon` rules in tss.plan.css pin it to a
        // fixed size, collapse line-height to 1 and flex-center the glyph so the
        // rotation pivot sits exactly on the glyph's center.
        public static UIcons StatusIcon(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return UIcons.Spinner;     // rotated via CSS
                case PlanStatus.Active:    return UIcons.Circle;      // in progress, but nothing to spin
                case PlanStatus.Completed: return UIcons.CheckCircle;
                case PlanStatus.Failed:    return UIcons.CrossCircle;
                case PlanStatus.Canceled:  return UIcons.Ban;
                default:                   return UIcons.Circle;      // Pending
            }
        }

        /// <summary>
        /// The class list for a status glyph. In compact mode every status but
        /// <see cref="PlanStatus.Running"/> collapses to a plain circle — the status
        /// then reads from the color alone, which is the point of the compact rail —
        /// and the running spinner is kept because its motion is the information.
        /// </summary>
        private static string StatusIconClass(PlanStatus status, bool compact)
        {
            if (compact && status != PlanStatus.Running)
            {
                // The straight-style circle, in the one weight that draws it filled: every
                // other weight (including RegularStraight) draws a ring, and a ring at the
                // compact rail's 12px reads as a smudge rather than a status mark.
                return "tss-icon tss-plan-dot " + Icon.Transform(UIcons.Circle, UIconsWeight.SolidStraight);
            }

            return "tss-icon " + StatusIcon(status);
        }

        /// <summary>
        /// Creates the status-glyph element (a UIcons <c>&lt;i&gt;</c>) for
        /// <paramref name="status"/>.
        /// </summary>
        public static HTMLElement CreateStatusIcon(PlanStatus status, bool compact = false)
        {
            return I(Att(StatusIconClass(status, compact)));
        }

        /// <summary>
        /// Points an existing status-glyph element at the glyph for
        /// <paramref name="status"/>. Only the icon class changes, so a running
        /// spinner's CSS animation is not restarted when unrelated fields update.
        /// </summary>
        public static void SetStatusIcon(HTMLElement iconEl, PlanStatus status, bool compact = false)
        {
            iconEl.className = StatusIconClass(status, compact);
        }

        public static string StatusText(PlanStatus status)
        {
            switch (status)
            {
                case PlanStatus.Running:   return "Running";
                case PlanStatus.Active:    return "Active";
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
            // Auto-expand when in progress / failed; auto-collapse otherwise.
            return step.Status == PlanStatus.Running
                || step.Status == PlanStatus.Active
                || step.Status == PlanStatus.Failed;
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
                    else if (s.Status == PlanStatus.Running || s.Status == PlanStatus.Active) running++;
                    else if (s.Status == PlanStatus.Failed) failed++;
                }
            }
            var parts = new List<string>();
            parts.Add(completed + " of " + total + " steps complete");
            if (running > 0) parts.Add(running + " running");
            if (failed > 0)  parts.Add(failed + " failed");
            return string.Join(" · ", parts);
        }

        /// <summary>Removes every <c>is-*</c> status class from an element.</summary>
        public static void RemoveStepStatusClasses(HTMLElement el)
        {
            el.classList.remove("is-pending");
            el.classList.remove("is-running");
            el.classList.remove("is-active");
            el.classList.remove("is-completed");
            el.classList.remove("is-failed");
            el.classList.remove("is-canceled");
        }

        /// <summary>Removes all child nodes from an element.</summary>
        public static void ClearChildren(HTMLElement el)
        {
            while (el.childNodes.length > 0)
            {
                el.removeChild(el.childNodes[0]);
            }
        }

        private static string Pad2(int n) => (n < 10 ? "0" : "") + n;

        /// <summary>Formats a timestamp as <c>MM/dd/yyyy HH:mm:ss</c> (local time).</summary>
        public static string FormatTimestamp(DateTimeOffset value)
        {
            var d = value.LocalDateTime;
            return Pad2(d.Month) + "/" + Pad2(d.Day) + "/" + d.Year +
                   " " + Pad2(d.Hour) + ":" + Pad2(d.Minute) + ":" + Pad2(d.Second);
        }
    }

    /// <summary>
    /// A timeline-style display for showing a multi-step plan, with optional
    /// per-step status / progress / substeps. Can be driven imperatively via
    /// the fluent <see cref="AddTask(string,bool)"/> API or declaratively via
    /// <see cref="SetModel"/>; <see cref="SetModel"/> performs an in-place
    /// reconcile keyed by step/substep <c>Id</c> so DOM nodes are reused.
    /// </summary>
    [Transpose.Name("tss.Plan")]
    public sealed class Plan : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _innerElement;

        // Header
        private readonly HTMLElement _headerEl;
        private readonly HTMLElement _titleEl;
        private readonly HTMLElement _badge;
        private readonly HTMLElement _badgeText;
        private readonly HTMLElement _headerCommandsEl;

        // Task list / timeline
        private readonly HTMLElement _stepsEl;
        private readonly List<Task> _tasks = new List<Task>();
        private readonly Dictionary<string, Task> _tasksByKey = new Dictionary<string, Task>();

        // Footer status strip
        private readonly HTMLElement _footerEl;
        private readonly HTMLElement _summaryEl;
        private readonly HTMLElement _countEl;
        private readonly HTMLElement _progressEl;
        private readonly HTMLElement _progressBarEl;
        private readonly HTMLElement _pctEl;
        private readonly Button _startStopButton;
        private readonly HTMLElement _timestampsEl;

        private string _footerMessage;
        private bool   _compact;

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
            // ---- Header ----
            _titleEl = Div(Att("tss-plan-title"));
            _titleEl.innerText = title ?? string.Empty;

            _badgeText = Span(Att());
            _badge = Span(Att("tss-plan-status-badge"), Span(Att("tss-plan-badge-dot")), _badgeText);
            _badge.style.display = "none";

            _headerCommandsEl = Div(Att("tss-plan-header-commands"));

            var titleWrap = Div(Att("tss-plan-titlewrap"), _titleEl, _badge);
            _headerEl = Div(Att("tss-plan-header"), titleWrap, _headerCommandsEl);

            // ---- Steps / timeline ----
            _stepsEl = Div(Att("tss-plan-steps"));

            // ---- Footer status strip ----
            _summaryEl = Div(Att("tss-plan-summary"));
            _countEl = Div(Att("tss-plan-count"));
            _countEl.style.display = "none";
            var footerTop = Div(Att("tss-plan-footer-top"), _summaryEl, _countEl);

            _progressBarEl = Div(Att("tss-plan-progress-bar"));
            _progressEl = Div(Att("tss-plan-progress"), _progressBarEl);
            _pctEl = Div(Att("tss-plan-pct"));

            _startStopButton = Button().SetIcon(UIcons.SquareSmall);
            var ssEl = _startStopButton.Render();
            ssEl.classList.remove("tss-btn");
            ssEl.classList.remove("tss-btn-default");
            ssEl.classList.remove("tss-default-component-margin");
            ssEl.classList.add("tss-plan-startstop");

            var progressRow = Div(Att("tss-plan-progress-row"), _progressEl, _pctEl, ssEl);

            _timestampsEl = Div(Att("tss-plan-timestamps"));

            _footerEl = Div(Att("tss-plan-footer"), footerTop, progressRow, _timestampsEl);

            // Default determinate progress at 0%.
            SetDeterminate(0f);

            _innerElement = Div(Att("tss-plan tss-plan-pending"), _headerEl, _stepsEl, _footerEl);
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
            _titleEl.innerText = title ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Switches the timeline to its compact rail: tighter rows and a plain
        /// status dot in place of the check / cross / ban glyphs, so the status
        /// reads from the dot's color alone. A running step keeps its spinner,
        /// because there the motion is the information.
        /// </summary>
        public Plan Compact(bool compact = true)
        {
            _compact = compact;
            _innerElement.UpdateClassIf(compact, "tss-plan-compact");
            foreach (var task in _tasks) task.SetCompact(compact);
            return this;
        }

        /// <summary>
        /// Hides the header strip (title, status badge and header commands), for
        /// when the plan is embedded in something that already has a heading —
        /// a <see cref="Card"/> with a title, say.
        /// </summary>
        public Plan NoHeader(bool noHeader = true)
        {
            _innerElement.UpdateClassIf(noHeader, "tss-plan-no-header");
            _headerEl.style.display = noHeader ? "none" : "";
            return this;
        }

        /// <summary>
        /// Hides the footer status strip (summary, progress bar and start / stop button).
        /// </summary>
        public Plan NoFooter(bool noFooter = true)
        {
            _innerElement.UpdateClassIf(noFooter, "tss-plan-no-footer");
            _footerEl.style.display = noFooter ? "none" : "";
            return this;
        }

        /// <summary>
        /// Drops the card chrome — border, rounded corners and shadow — leaving the
        /// plan transparent so it sits flush inside whatever contains it.
        /// </summary>
        public Plan NoBorder(bool noBorder = true)
        {
            _innerElement.UpdateClassIf(noBorder, "tss-plan-no-border");
            return this;
        }

        /// <summary>
        /// Sets the components shown in the header command area.
        /// </summary>
        public Plan HeaderCommands(params IComponent[] commands)
        {
            PlanVisuals.ClearChildren(_headerCommandsEl);
            foreach (var cmd in commands)
            {
                _headerCommandsEl.appendChild(cmd.Render());
            }
            return this;
        }

        /// <summary>
        /// Sets a message shown in the footer; it is rendered as a bold prefix
        /// in front of the auto-derived progress summary.
        /// </summary>
        public Plan FooterMessage(string message)
        {
            _footerMessage = message;
            UpdateSummary();
            return this;
        }

        /// <summary>
        /// Sets the components shown on the right of the footer strip (the
        /// "count" slot, e.g. a "117 searches" label).
        /// </summary>
        public Plan FooterCommands(params IComponent[] commands)
        {
            PlanVisuals.ClearChildren(_countEl);
            foreach (var cmd in commands)
            {
                _countEl.appendChild(cmd.Render());
            }
            _countEl.style.display = commands.Length > 0 ? "" : "none";
            return this;
        }

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
        public Plan Progress(int position, int total)
        {
            SetDeterminate(total > 0 ? (float)position / total * 100f : 0f);
            return this;
        }

        /// <summary>
        /// Configures the component to progress (0..100).
        /// </summary>
        public Plan Progress(float percent)
        {
            SetDeterminate(percent);
            return this;
        }

        /// <summary>
        /// Configures the component to indeterminate.
        /// </summary>
        public Plan Indeterminate()
        {
            SetIndeterminate();
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
            _startStopButton.Render().classList.add("is-hidden");
            return this;
        }

        /// <summary>
        /// Shows the start stop button.
        /// </summary>
        public Plan ShowStartStopButton()
        {
            _startStopButton.Render().classList.remove("is-hidden");
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
        /// Adds the given task to the component.
        ///
        /// Note: completed tasks render with the unified "success" status
        /// color (green check) instead of the previous primary-tone treatment;
        /// this matches the generalized status styling shared with SetModel.
        /// </summary>
        public Plan AddTask(string title, bool completed)
        {
            return AddTask(title, completed ? PlanStatus.Completed : PlanStatus.Pending);
        }

        /// <summary>
        /// Adds a task with an explicit <see cref="PlanStatus"/> and an optional
        /// badge component rendered at the end of its title row.
        /// Auto-generates a positional key so a later <see cref="SetModel"/>
        /// call still reconciles correctly.
        /// </summary>
        public Plan AddTask(string title, PlanStatus status, IComponent badge = null)
        {
            var key = "tss-plan-step-" + _tasks.Count;
            var task = new Task(key, title, status, _compact);
            if (badge != null) task.SetBadge(badge);
            _tasks.Add(task);
            _tasksByKey[key] = task;
            _stepsEl.appendChild(task.Render());
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
            if (model.Title != null) _titleEl.innerText = model.Title;

            // Plan-level status (card accent + badge)
            ApplyCardStatusClass(model.Status);
            ApplyBadge(model.Status);

            // Plan-level progress + percent readout
            if (model.Progress.HasValue)
            {
                SetDeterminate(PlanVisuals.ClampProgress(model.Progress.Value) * 100f);
            }
            else if (model.Status == PlanStatus.Running || model.Status == PlanStatus.Active)
            {
                SetIndeterminate();
            }
            else if (model.Status == PlanStatus.Completed)
            {
                SetDeterminate(100f);
            }
            else
            {
                SetDeterminate(0f);
            }

            // Start/stop icon reflects the running state.
            var inProgress = model.Status == PlanStatus.Running || model.Status == PlanStatus.Active;
            _startStopButton.SetIcon(inProgress ? UIcons.SquareSmall : UIcons.Play);

            // Footer message + summary
            _footerMessage = model.FooterMessage;
            UpdateSummary();

            // Count slot (e.g. "117 searches")
            if (!string.IsNullOrEmpty(model.Searches))
            {
                PlanVisuals.ClearChildren(_countEl);
                _countEl.innerText = model.Searches;
                _countEl.style.display = "";
            }
            else
            {
                PlanVisuals.ClearChildren(_countEl);
                _countEl.style.display = "none";
            }

            // Timestamps
            var timestampParts = new List<string>();
            if (model.StartedAt.HasValue) timestampParts.Add("Started " + PlanVisuals.FormatTimestamp(model.StartedAt.Value));
            if (model.CompletedAt.HasValue) timestampParts.Add("Completed " + PlanVisuals.FormatTimestamp(model.CompletedAt.Value));
            _timestampsEl.innerText = string.Join("   ·   ", timestampParts);

            // Steps reconcile-by-key
            ReconcileSteps(model.Steps ?? new List<PlanStepModel>());

            return this;
        }

        private void SetDeterminate(float percent)
        {
            if (float.IsNaN(percent) || percent < 0f) percent = 0f;
            if (percent > 100f) percent = 100f;
            _progressEl.classList.remove("is-indeterminate");
            _progressBarEl.style.width = percent + "%";
            _pctEl.innerText = Math.Round(percent) + "%";
            _pctEl.style.display = "";
        }

        private void SetIndeterminate()
        {
            _progressEl.classList.add("is-indeterminate");
            _pctEl.style.display = "none";
        }

        private void UpdateSummary()
        {
            PlanVisuals.ClearChildren(_summaryEl);

            if (_model != null)
            {
                if (!string.IsNullOrEmpty(_footerMessage))
                {
                    var msgSpan = Span(Att("tss-plan-footer-msg"));
                    msgSpan.innerText = _footerMessage;
                    _summaryEl.appendChild(msgSpan);
                    var sep = Span(Att());
                    sep.innerText = " · ";
                    _summaryEl.appendChild(sep);
                }
                var summarySpan = Span(Att());
                summarySpan.innerText = PlanVisuals.Summary(_model);
                _summaryEl.appendChild(summarySpan);
            }
            else
            {
                _summaryEl.innerText = _footerMessage ?? string.Empty;
            }
        }

        private void ApplyCardStatusClass(PlanStatus status)
        {
            _innerElement.classList.remove("tss-plan-pending");
            _innerElement.classList.remove("tss-plan-running");
            _innerElement.classList.remove("tss-plan-active");
            _innerElement.classList.remove("tss-plan-completed");
            _innerElement.classList.remove("tss-plan-failed");
            _innerElement.classList.remove("tss-plan-canceled");
            _innerElement.classList.add(PlanVisuals.StatusClass(status));
        }

        private void ApplyBadge(PlanStatus status)
        {
            // Colors are inherited from the card's --plan-accent custom props,
            // so only the label text needs to change here.
            _badgeText.innerText = PlanVisuals.StatusText(status);
            _badge.style.display = "";
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
                    task = new Task(key, stepModel.Title ?? string.Empty, stepModel.Status, _compact);
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
            // appendChild moves a node if it already has a parent, which is
            // exactly what we want here.
            _tasks.Clear();
            foreach (var key in newKeys)
            {
                var t = _tasksByKey[key];
                _tasks.Add(t);
                _stepsEl.appendChild(t.Render());
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
            private readonly HTMLElement _iconSlot;
            private readonly HTMLElement _statusIcon;
            private readonly HTMLElement _textEl;
            private readonly HTMLElement _badgeEl;
            private readonly HTMLElement _stageEl;
            private readonly HTMLElement _toggleEl;
            private readonly HTMLElement _progressEl;
            private readonly HTMLElement _progressBar;
            private readonly HTMLElement _substeps;
            private readonly HTMLElement _substepsInner;
            private readonly Dictionary<string, HTMLElement> _substepsByKey = new Dictionary<string, HTMLElement>();
            private readonly List<string> _substepOrder = new List<string>();
            private PlanStatus _status;
            private bool _expanded;
            private bool _compact;
            private IComponent _badge;

            internal string Key { get; private set; }

            /// <summary>
            /// Gets or sets the title of the component.
            /// </summary>
            public string Title
            {
                get => _textEl.innerText;
                set => _textEl.innerText = value;
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

            internal Task(string key, string title, PlanStatus status, bool compact = false)
            {
                Key = key;
                _status = status;
                _compact = compact;

                // Rail / status node (UIcons glyph; the spinner spins in place
                // when running — see the centering note in PlanVisuals).
                _statusIcon = PlanVisuals.CreateStatusIcon(status, compact);
                _iconSlot = Div(Att("tss-plan-step-icon"), _statusIcon);
                var rail = Div(Att("tss-plan-step-rail"), _iconSlot);

                // Title row (text + badge + expand/collapse chevron).
                _textEl = Span(Att("tss-plan-step-text"));
                _textEl.innerText = title ?? string.Empty;

                _badgeEl = Span(Att("tss-plan-step-badge"));
                _badgeEl.style.display = "none";

                _toggleEl = Button(Att("tss-plan-step-toggle"), Icon(UIcons.AngleDown).Render());
                _toggleEl.style.display = "none";
                _toggleEl.onclick = e => { ToggleExpanded(); };

                var titleRow = Div(Att("tss-plan-step-title"), _textEl, _badgeEl, _toggleEl);

                _stageEl = Div(Att("tss-plan-step-stage"));
                _stageEl.style.display = "none";

                _progressBar = Div(Att("tss-plan-step-progress-bar"));
                _progressEl = Div(Att("tss-plan-step-progress"), _progressBar);
                _progressEl.style.display = "none";

                _substepsInner = Div(Att("tss-plan-substeps-inner"));
                _substeps = Div(Att("tss-plan-substeps is-collapsed"), _substepsInner);

                var body = Div(Att("tss-plan-step-body"), titleRow, _stageEl, _progressEl, _substeps);

                _root = Div(Att("tss-plan-step " + PlanVisuals.StepStatusClass(status)), rail, body);
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

            /// <summary>
            /// Sets (or clears, when <paramref name="badge"/> is <c>null</c>) the component
            /// rendered at the end of this task's title row.
            /// </summary>
            public Task SetBadge(IComponent badge)
            {
                // Same component instance means the same DOM node: re-rendering it would
                // drop any state it holds for no reason.
                if (ReferenceEquals(_badge, badge)) return this;

                _badge = badge;
                PlanVisuals.ClearChildren(_badgeEl);

                if (badge == null)
                {
                    _badgeEl.style.display = "none";
                }
                else
                {
                    _badgeEl.appendChild(badge.Render());
                    _badgeEl.style.display = "";
                }

                return this;
            }

            /// <summary>Switches this task between the default and the compact rail.</summary>
            internal void SetCompact(bool compact)
            {
                if (_compact == compact) return;
                _compact = compact;
                PlanVisuals.SetStatusIcon(_statusIcon, _status, compact);
                foreach (var key in _substepOrder) RefreshSubstepIcon(_substepsByKey[key], compact);
            }

            internal void UpdateFromModel(PlanStepModel model)
            {
                if (model.Title != null) _textEl.innerText = model.Title;

                ApplyStatus(model.Status);
                SetBadge(model.Badge);

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
                    _progressEl.classList.remove("is-indeterminate");
                    _progressBar.style.width = (p * 100f) + "%";
                    _progressEl.style.display = "";
                }
                else if (model.Status == PlanStatus.Running)
                {
                    _progressEl.classList.add("is-indeterminate");
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
                // Only swap the glyph when the status actually changes, so the
                // running spinner's animation isn't restarted on every update.
                // Color + spin come from the parent `.is-*` class via CSS.
                if (status != _status)
                {
                    PlanVisuals.SetStatusIcon(_statusIcon, status, _compact);
                }
                _status = status;
                PlanVisuals.RemoveStepStatusClasses(_root);
                _root.classList.add(PlanVisuals.StepStatusClass(status));
            }

            private void ToggleExpanded()
            {
                SetExpanded(!_expanded);
            }

            private void SetExpanded(bool expanded)
            {
                _expanded = expanded;
                _root.UpdateClassIf(expanded, "is-expanded");
                _substeps.UpdateClassIf(!expanded, "is-collapsed");
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
                        UpdateSubstepEl(el, sub, _compact);
                    }
                    else
                    {
                        el = BuildSubstepEl(sub, _compact);
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
                    _substepsInner.appendChild(_substepsByKey[k]);
                }
            }

            private static HTMLElement BuildSubstepEl(PlanSubstepModel sub, bool compact)
            {
                var iconWrap = Div(Att("tss-plan-substep-icon"));
                var titleEl = Div(Att("tss-plan-substep-title"));
                var msgEl = Div(Att("tss-plan-substep-message"));
                var body = Div(Att("tss-plan-substep-body"), titleEl, msgEl);
                var root = Div(Att("tss-plan-substep"), iconWrap, body);
                UpdateSubstepEl(root, sub, compact);
                return root;
            }

            /// <summary>
            /// Re-draws a substep's glyph for the current rail density, keeping the status
            /// it already has. Used when the whole plan is switched to (or out of) compact.
            /// </summary>
            private static void RefreshSubstepIcon(HTMLElement root, bool compact)
            {
                var iconWrap = (HTMLElement)root.childNodes[0];
                var icon = iconWrap.childNodes.length > 0 ? (HTMLElement)iconWrap.childNodes[0] : null;
                if (icon == null) return;

                var statusName = root.dataset["pstatus"].As<string>();
                var status = PlanStatus.Pending;
                if (statusName == "Running")        status = PlanStatus.Running;
                else if (statusName == "Active")    status = PlanStatus.Active;
                else if (statusName == "Completed") status = PlanStatus.Completed;
                else if (statusName == "Failed")    status = PlanStatus.Failed;
                else if (statusName == "Canceled")  status = PlanStatus.Canceled;

                PlanVisuals.SetStatusIcon(icon, status, compact);
            }

            private static void UpdateSubstepEl(HTMLElement root, PlanSubstepModel sub, bool compact)
            {
                // Children: [iconWrap, body[titleEl, msgEl]]
                var iconWrap = (HTMLElement)root.childNodes[0];
                var body = (HTMLElement)root.childNodes[1];
                var titleEl = (HTMLElement)body.childNodes[0];
                var msgEl = (HTMLElement)body.childNodes[1];

                // Status class + UIcons glyph (color/size via CSS). Only swap
                // when the status changes so a running spinner isn't reset.
                var statusName = sub.Status.ToString();
                if (root.dataset["pstatus"].As<string>() != statusName)
                {
                    PlanVisuals.RemoveStepStatusClasses(root);
                    root.classList.add(PlanVisuals.StepStatusClass(sub.Status));
                    PlanVisuals.ClearChildren(iconWrap);
                    iconWrap.appendChild(PlanVisuals.CreateStatusIcon(sub.Status, compact));
                    root.dataset["pstatus"] = statusName;
                }

                // Title
                titleEl.innerText = sub.Title ?? string.Empty;

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
