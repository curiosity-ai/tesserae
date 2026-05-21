using System;
using System.Globalization;

namespace Tesserae
{
    internal static class DeepResearchVisuals
    {
        private static readonly string[] StatusClasses =
        {
            "tss-deepresearch-pending",
            "tss-deepresearch-running",
            "tss-deepresearch-completed",
            "tss-deepresearch-failed",
            "tss-deepresearch-canceled"
        };

        public static string GetStatusClass(DeepResearchStatus status) => $"tss-deepresearch-{status.ToString().ToLower()}";

        public static void ApplyStatusClass(H5.Core.dom.HTMLElement element, DeepResearchStatus status)
        {
            if (element == null)
            {
                return;
            }

            element.classList.remove(StatusClasses);
            element.classList.add(GetStatusClass(status));
        }

        public static void ConfigureStatusBadge(Badge badge, DeepResearchStatus status)
        {
            badge.SetText(GetStatusText(status))
               .SetIcon(GetStatusIcon(status).ToString())
               .Pill()
               .Outline()
               .Neutral();

            switch (status)
            {
                case DeepResearchStatus.Running:
                    badge.Primary();
                    break;
                case DeepResearchStatus.Completed:
                    badge.Success();
                    break;
                case DeepResearchStatus.Failed:
                    badge.Danger();
                    break;
                case DeepResearchStatus.Canceled:
                    badge.Warning();
                    break;
            }
        }

        public static UIcons GetStatusIcon(DeepResearchStatus status)
        {
            switch (status)
            {
                case DeepResearchStatus.Running:
                    return UIcons.ArrowProgressAlt;
                case DeepResearchStatus.Completed:
                    return UIcons.CheckCircle;
                case DeepResearchStatus.Failed:
                    return UIcons.CircleXmark;
                case DeepResearchStatus.Canceled:
                    return UIcons.Ban;
                default:
                    return UIcons.Clock;
            }
        }

        public static string GetStatusText(DeepResearchStatus status)
        {
            switch (status)
            {
                case DeepResearchStatus.Running:
                    return "Running";
                case DeepResearchStatus.Completed:
                    return "Completed";
                case DeepResearchStatus.Failed:
                    return "Failed";
                case DeepResearchStatus.Canceled:
                    return "Canceled";
                default:
                    return "Pending";
            }
        }

        public static string GetBorderColor(DeepResearchStatus status)
        {
            switch (status)
            {
                case DeepResearchStatus.Running:
                    return "var(--tss-primary-background-color)";
                case DeepResearchStatus.Completed:
                    return "var(--tss-success-background-color)";
                case DeepResearchStatus.Failed:
                    return "var(--tss-danger-background-color)";
                case DeepResearchStatus.Canceled:
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

        public static float? ResolvePlanProgress(DeepResearchPlanModel model)
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

            if (model.Status == DeepResearchStatus.Completed)
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
                if ((step?.Status ?? DeepResearchStatus.Pending) == DeepResearchStatus.Completed)
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

        public static string BuildPlanSummary(DeepResearchPlanModel model)
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
                switch (step?.Status ?? DeepResearchStatus.Pending)
                {
                    case DeepResearchStatus.Completed:
                        completed++;
                        break;
                    case DeepResearchStatus.Running:
                        running++;
                        break;
                    case DeepResearchStatus.Failed:
                        failed++;
                        break;
                    case DeepResearchStatus.Canceled:
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

        public static string BuildTimestampText(DeepResearchPlanModel model)
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

        public static bool ShouldExpandSubsteps(DeepResearchStepModel model)
        {
            if (model == null || model.Substeps == null || model.Substeps.Count == 0)
            {
                return false;
            }

            return model.Status == DeepResearchStatus.Running || model.Status == DeepResearchStatus.Failed;
        }

        public static string GetStepKey(DeepResearchStepModel model, int fallbackIndex)
        {
            if (model != null && HasText(model.Id))
            {
                return model.Id;
            }

            return $"deep-research-step-{fallbackIndex}";
        }

        public static string FormatStepTitle(DeepResearchStepModel model)
        {
            var title = HasText(model?.Title) ? model.Title : "Untitled step";

            if ((model?.Index ?? 0) > 0)
            {
                return $"{model.Index}. {title}";
            }

            return title;
        }

        public static string FormatSubstepTitle(DeepResearchSubstepModel model)
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
