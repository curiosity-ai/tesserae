using System;
using System.Collections.Generic;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "AI", Order = 10, Icon = UIcons.ResearchArrowsCircle)]
    public sealed class DeepResearchPlanSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DeepResearchPlanSample()
        {
            var livePlan = DeepResearchPlan(BuildPendingPlan());

            _content = SectionStack()
               .Title(SampleHeader(nameof(DeepResearchPlanSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("DeepResearchPlan provides a compact, chat-friendly execution view for research plans, ordered steps, substeps, progress, and terminal states. Host applications can map their own streaming or process payloads into the public models and update the component over time via SetModel.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use the plan component for execution state, not for configuration. Keep stage text concise, use substeps only when they add signal, and prefer passing explicit progress when available. If progress is unknown while running, the component will render an indeterminate progress bar.")))
               .Section(Stack().Gap(12.px()).Children(
                    SampleTitle("Interactive States"),
                    TextBlock("This example switches the same plan instance through pending, running, completed, failed, and canceled snapshots to demonstrate incremental updates."),
                    Stack().Horizontal().Wrap().Gap(8.px()).Children(
                        Button("Pending").OnClick(() => livePlan.SetModel(BuildPendingPlan())).Compact(),
                        Button("Running").OnClick(() => livePlan.SetModel(BuildRunningPlan())).Compact(),
                        Button("Completed").OnClick(() => livePlan.SetModel(BuildCompletedPlan())).Compact(),
                        Button("Failed").OnClick(() => livePlan.SetModel(BuildFailedPlan())).Compact(),
                        Button("Canceled").OnClick(() => livePlan.SetModel(BuildCanceledPlan())).Compact()
                    ),
                    livePlan
               ))
               .Section(Stack().Gap(12.px()).Children(
                    SampleTitle("Static Variants"),
                    DeepResearchPlan(BuildRunningPlan()),
                    DeepResearchPlan(BuildCompletedPlan())
               ));
        }

        public HTMLElement Render() => _content.Render();

        private static DeepResearchPlanModel BuildPendingPlan()
        {
            return new DeepResearchPlanModel
            {
                Id = "plan-pending",
                Title = "Research the 2026 browser automation landscape",
                Status = DeepResearchStatus.Pending,
                CurrentStage = "Waiting to start",
                FooterMessage = "Plan ready. Execution will begin when the host app starts the research run.",
                Steps = new List<DeepResearchStepModel>
                {
                    new DeepResearchStepModel
                    {
                        Id = "scope",
                        Index = 1,
                        Title = "Define scope and key comparison axes",
                        Status = DeepResearchStatus.Pending,
                        Substeps = new List<DeepResearchSubstepModel>
                        {
                            new DeepResearchSubstepModel { Id = "scope-1", Title = "List candidate frameworks", Status = DeepResearchStatus.Pending, Message = "Playwright, Selenium, Cypress, WebdriverIO" }
                        }
                    },
                    new DeepResearchStepModel { Id = "gather", Index = 2, Title = "Collect primary sources and release notes", Status = DeepResearchStatus.Pending },
                    new DeepResearchStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = DeepResearchStatus.Pending }
                }
            };
        }

        private static DeepResearchPlanModel BuildRunningPlan()
        {
            return new DeepResearchPlanModel
            {
                Id = "plan-running",
                Title = "Research the 2026 browser automation landscape",
                Status = DeepResearchStatus.Running,
                Progress = 48,
                CurrentStage = "Gathering release notes and API references",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                FooterMessage = "Primary-source sweep in progress. Findings summary will appear once synthesis starts.",
                Steps = new List<DeepResearchStepModel>
                {
                    new DeepResearchStepModel
                    {
                        Id = "scope",
                        Index = 1,
                        Title = "Define scope and key comparison axes",
                        Status = DeepResearchStatus.Completed,
                        Progress = 100,
                        CurrentStage = "Scope approved",
                        Substeps = new List<DeepResearchSubstepModel>
                        {
                            new DeepResearchSubstepModel { Id = "scope-1", Title = "List candidate frameworks", Status = DeepResearchStatus.Completed, Message = "Playwright, Selenium, Cypress, WebdriverIO" },
                            new DeepResearchSubstepModel { Id = "scope-2", Title = "Agree comparison rubric", Status = DeepResearchStatus.Completed, Message = "Coverage, DX, reliability, CI support, ecosystem" }
                        }
                    },
                    new DeepResearchStepModel
                    {
                        Id = "gather",
                        Index = 2,
                        Title = "Collect primary sources and release notes",
                        Status = DeepResearchStatus.Running,
                        Progress = 45,
                        CurrentStage = "Reviewing vendor changelogs",
                        IsExpanded = true,
                        Substeps = new List<DeepResearchSubstepModel>
                        {
                            new DeepResearchSubstepModel { Id = "gather-1", Title = "Read Playwright releases", Status = DeepResearchStatus.Completed, Message = "Captured breaking changes and new tracing/debugging capabilities" },
                            new DeepResearchSubstepModel { Id = "gather-2", Title = "Read Selenium project updates", Status = DeepResearchStatus.Running, Message = "Collecting WebDriver BiDi support details" },
                            new DeepResearchSubstepModel { Id = "gather-3", Title = "Review Cypress roadmap", Status = DeepResearchStatus.Pending, Message = "Queued after Selenium pass completes" }
                        }
                    },
                    new DeepResearchStepModel
                    {
                        Id = "report",
                        Index = 3,
                        Title = "Synthesize findings into a final report",
                        Status = DeepResearchStatus.Pending,
                        Substeps = new List<DeepResearchSubstepModel>
                        {
                            new DeepResearchSubstepModel { Id = "report-1", Title = "Draft recommendations", Status = DeepResearchStatus.Pending, Message = "Blocked until collection step finishes" }
                        }
                    }
                }
            };
        }

        private static DeepResearchPlanModel BuildCompletedPlan()
        {
            return new DeepResearchPlanModel
            {
                Id = "plan-completed",
                Title = "Research the 2026 browser automation landscape",
                Status = DeepResearchStatus.Completed,
                Progress = 100,
                CurrentStage = "Final report delivered",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                CompletedAt = new DateTimeOffset(2026, 04, 01, 10, 42, 00, TimeSpan.Zero),
                FooterMessage = "Summary: Playwright leads on end-to-end workflow coverage, while Selenium remains strongest for broad protocol compatibility.",
                Steps = new List<DeepResearchStepModel>
                {
                    new DeepResearchStepModel { Id = "scope", Index = 1, Title = "Define scope and key comparison axes", Status = DeepResearchStatus.Completed, Progress = 100 },
                    new DeepResearchStepModel { Id = "gather", Index = 2, Title = "Collect primary sources and release notes", Status = DeepResearchStatus.Completed, Progress = 100 },
                    new DeepResearchStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = DeepResearchStatus.Completed, Progress = 100, CurrentStage = "Recommendations published" }
                }
            };
        }

        private static DeepResearchPlanModel BuildFailedPlan()
        {
            return new DeepResearchPlanModel
            {
                Id = "plan-failed",
                Title = "Research the 2026 browser automation landscape",
                Status = DeepResearchStatus.Failed,
                Progress = 55,
                CurrentStage = "Source verification failed",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                FooterMessage = "Execution stopped after a primary-source fetch failed repeatedly and the host app marked the run as failed.",
                Steps = new List<DeepResearchStepModel>
                {
                    new DeepResearchStepModel { Id = "scope", Index = 1, Title = "Define scope and key comparison axes", Status = DeepResearchStatus.Completed, Progress = 100 },
                    new DeepResearchStepModel
                    {
                        Id = "gather",
                        Index = 2,
                        Title = "Collect primary sources and release notes",
                        Status = DeepResearchStatus.Failed,
                        Progress = 65,
                        CurrentStage = "Vendor documentation unavailable",
                        IsExpanded = true,
                        Substeps = new List<DeepResearchSubstepModel>
                        {
                            new DeepResearchSubstepModel { Id = "gather-1", Title = "Read Playwright releases", Status = DeepResearchStatus.Completed, Message = "Captured release notes successfully" },
                            new DeepResearchSubstepModel { Id = "gather-2", Title = "Read Selenium project updates", Status = DeepResearchStatus.Failed, Message = "Remote source timed out after repeated retries" }
                        }
                    },
                    new DeepResearchStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = DeepResearchStatus.Pending }
                }
            };
        }

        private static DeepResearchPlanModel BuildCanceledPlan()
        {
            return new DeepResearchPlanModel
            {
                Id = "plan-canceled",
                Title = "Research the 2026 browser automation landscape",
                Status = DeepResearchStatus.Canceled,
                Progress = 30,
                CurrentStage = "Canceled by user",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                FooterMessage = "The run was canceled before source collection completed. Partial findings remain available to the host app.",
                Steps = new List<DeepResearchStepModel>
                {
                    new DeepResearchStepModel { Id = "scope", Index = 1, Title = "Define scope and key comparison axes", Status = DeepResearchStatus.Completed, Progress = 100 },
                    new DeepResearchStepModel { Id = "gather", Index = 2, Title = "Collect primary sources and release notes", Status = DeepResearchStatus.Canceled, Progress = 20, CurrentStage = "Canceled during vendor doc review" },
                    new DeepResearchStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = DeepResearchStatus.Pending }
                }
            };
        }
    }
}
