using System;
using System.Collections.Generic;

using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.ListCheck)]
    public class PlanSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PlanSample()
        {
            var plan1 = Plan("SCIM user provisioning deep dive")
                .HeaderCommands(Button("Update").NoBorder().Rounded())
                .AddTask("Collect official SCIM RFCs and specifications from IETF and RFC repositories.", true)
                .AddTask("Survey major identity providers' SCIM documentation and authentication methods.", false)
                .AddTask("Gather sample SCIM request and response payloads and endpoint patterns.", false)
                .AddTask("Identify open-source SCIM implementations and C# libraries with examples.", false)
                .AddTask("Compile technical explanation covering endpoints, auth, schemas, and examples.", false)
                .FooterMessage("Finalizing details for licenses and attribution...")
                .FooterCommands(TextBlock("117 searches").Small().SemiBold())
                .Progress(1, 5) // Determinate style as per screenshot
                .Stop()
                .MaxWidth(800.px());

            var plan2 = Plan("Database Migration Plan")
                .AddTask("Backup database", true)
                .AddTask("Run schema update", true)
                .AddTask("Migrate data", true)
                .FooterMessage("Migration complete")
                .Progress(100)
                .HideStartStopButton()
                .MaxWidth(800.px());

            plan2.Render().style.maxWidth = "800px";

            var plan3 = Plan("Analyzing Log Files")
                .AddTask("Download logs from S3", true)
                .AddTask("Parse JSON structure", true)
                .AddTask("Find error patterns", false)
                .FooterMessage("Scanning file 45 of 200...")
                .Indeterminate()
                .Start()
                .MaxWidth(800.px());

            plan3.Render().style.maxWidth = "800px";

            // Data-driven / streaming plan: same instance updated through several snapshots.
            var livePlan = Plan("Deep research")
                .HideStartStopButton()
                .MaxWidth(800.px());
            livePlan.Render().style.maxWidth = "800px";
            livePlan.SetModel(BuildPendingPlan());

            _content = SectionStack()
                .Title(SampleHeader(nameof(PlanSample)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The Plan component displays a complex task with its sub-tasks and overall progress. It can be driven either through the fluent API or through a data model that is reconciled in place via SetModel."),
                    SampleTitle("Usage"),
                    TextBlock("Default usage showing a running plan with partial progress.").SemiBold().PT(16).PB(8),
                    plan1,
                    TextBlock("A completed plan, with the stop button hidden.").SemiBold().PT(16).PB(8),
                    plan2,
                    TextBlock("A plan with indeterminate progress.").SemiBold().PT(16).PB(8),
                    plan3))
                .Section(Stack().Gap(12.px()).Children(
                    SampleTitle("Data-driven / streaming updates"),
                    TextBlock("Host applications can map their own streaming or process payloads into PlanModel and update the same component over time via SetModel. Steps and substeps are reconciled in place using stable keys, so live updates do not tear down the DOM. Status drives the badge, border color, summary line, per-step icons and substep expansion."),
                    Stack().Horizontal().Wrap().Gap(8.px()).Children(
                        Button("Pending").OnClick(() => livePlan.SetModel(BuildPendingPlan())).Compact(),
                        Button("Running").OnClick(() => livePlan.SetModel(BuildRunningPlan())).Compact(),
                        Button("Completed").OnClick(() => livePlan.SetModel(BuildCompletedPlan())).Compact(),
                        Button("Failed").OnClick(() => livePlan.SetModel(BuildFailedPlan())).Compact(),
                        Button("Canceled").OnClick(() => livePlan.SetModel(BuildCanceledPlan())).Compact()
                    ),
                    livePlan
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private static PlanModel BuildPendingPlan()
        {
            return new PlanModel
            {
                Id = "plan-pending",
                Title = "Research the 2026 browser automation landscape",
                Status = PlanStatus.Pending,
                CurrentStage = "Waiting to start",
                FooterMessage = "Plan ready. Execution will begin when the host app starts the research run.",
                Steps = new List<PlanStepModel>
                {
                    new PlanStepModel
                    {
                        Id = "scope",
                        Index = 1,
                        Title = "Define scope and key comparison axes",
                        Status = PlanStatus.Pending,
                        Substeps = new List<PlanSubstepModel>
                        {
                            new PlanSubstepModel { Id = "scope-1", Title = "List candidate frameworks", Status = PlanStatus.Pending, Message = "Playwright, Selenium, Cypress, WebdriverIO" }
                        }
                    },
                    new PlanStepModel { Id = "gather", Index = 2, Title = "Collect primary sources and release notes", Status = PlanStatus.Pending },
                    new PlanStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = PlanStatus.Pending }
                }
            };
        }

        private static PlanModel BuildRunningPlan()
        {
            return new PlanModel
            {
                Id = "plan-running",
                Title = "Research the 2026 browser automation landscape",
                Status = PlanStatus.Running,
                Progress = 48,
                CurrentStage = "Gathering release notes and API references",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                FooterMessage = "Primary-source sweep in progress. Findings summary will appear once synthesis starts.",
                Steps = new List<PlanStepModel>
                {
                    new PlanStepModel
                    {
                        Id = "scope",
                        Index = 1,
                        Title = "Define scope and key comparison axes",
                        Status = PlanStatus.Completed,
                        Progress = 100,
                        CurrentStage = "Scope approved",
                        Substeps = new List<PlanSubstepModel>
                        {
                            new PlanSubstepModel { Id = "scope-1", Title = "List candidate frameworks", Status = PlanStatus.Completed, Message = "Playwright, Selenium, Cypress, WebdriverIO" },
                            new PlanSubstepModel { Id = "scope-2", Title = "Agree comparison rubric", Status = PlanStatus.Completed, Message = "Coverage, DX, reliability, CI support, ecosystem" }
                        }
                    },
                    new PlanStepModel
                    {
                        Id = "gather",
                        Index = 2,
                        Title = "Collect primary sources and release notes",
                        Status = PlanStatus.Running,
                        Progress = 45,
                        CurrentStage = "Reviewing vendor changelogs",
                        IsExpanded = true,
                        Substeps = new List<PlanSubstepModel>
                        {
                            new PlanSubstepModel { Id = "gather-1", Title = "Read Playwright releases", Status = PlanStatus.Completed, Message = "Captured breaking changes and new tracing/debugging capabilities" },
                            new PlanSubstepModel { Id = "gather-2", Title = "Read Selenium project updates", Status = PlanStatus.Running, Message = "Collecting WebDriver BiDi support details" },
                            new PlanSubstepModel { Id = "gather-3", Title = "Review Cypress roadmap", Status = PlanStatus.Pending, Message = "Queued after Selenium pass completes" }
                        }
                    },
                    new PlanStepModel
                    {
                        Id = "report",
                        Index = 3,
                        Title = "Synthesize findings into a final report",
                        Status = PlanStatus.Pending,
                        Substeps = new List<PlanSubstepModel>
                        {
                            new PlanSubstepModel { Id = "report-1", Title = "Draft recommendations", Status = PlanStatus.Pending, Message = "Blocked until collection step finishes" }
                        }
                    }
                }
            };
        }

        private static PlanModel BuildCompletedPlan()
        {
            return new PlanModel
            {
                Id = "plan-completed",
                Title = "Research the 2026 browser automation landscape",
                Status = PlanStatus.Completed,
                Progress = 100,
                CurrentStage = "Final report delivered",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                CompletedAt = new DateTimeOffset(2026, 04, 01, 10, 42, 00, TimeSpan.Zero),
                FooterMessage = "Summary: Playwright leads on end-to-end workflow coverage, while Selenium remains strongest for broad protocol compatibility.",
                Steps = new List<PlanStepModel>
                {
                    new PlanStepModel { Id = "scope", Index = 1, Title = "Define scope and key comparison axes", Status = PlanStatus.Completed, Progress = 100 },
                    new PlanStepModel { Id = "gather", Index = 2, Title = "Collect primary sources and release notes", Status = PlanStatus.Completed, Progress = 100 },
                    new PlanStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = PlanStatus.Completed, Progress = 100, CurrentStage = "Recommendations published" }
                }
            };
        }

        private static PlanModel BuildFailedPlan()
        {
            return new PlanModel
            {
                Id = "plan-failed",
                Title = "Research the 2026 browser automation landscape",
                Status = PlanStatus.Failed,
                Progress = 55,
                CurrentStage = "Source verification failed",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                FooterMessage = "Execution stopped after a primary-source fetch failed repeatedly and the host app marked the run as failed.",
                Steps = new List<PlanStepModel>
                {
                    new PlanStepModel { Id = "scope", Index = 1, Title = "Define scope and key comparison axes", Status = PlanStatus.Completed, Progress = 100 },
                    new PlanStepModel
                    {
                        Id = "gather",
                        Index = 2,
                        Title = "Collect primary sources and release notes",
                        Status = PlanStatus.Failed,
                        Progress = 65,
                        CurrentStage = "Vendor documentation unavailable",
                        IsExpanded = true,
                        Substeps = new List<PlanSubstepModel>
                        {
                            new PlanSubstepModel { Id = "gather-1", Title = "Read Playwright releases", Status = PlanStatus.Completed, Message = "Captured release notes successfully" },
                            new PlanSubstepModel { Id = "gather-2", Title = "Read Selenium project updates", Status = PlanStatus.Failed, Message = "Remote source timed out after repeated retries" }
                        }
                    },
                    new PlanStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = PlanStatus.Pending }
                }
            };
        }

        private static PlanModel BuildCanceledPlan()
        {
            return new PlanModel
            {
                Id = "plan-canceled",
                Title = "Research the 2026 browser automation landscape",
                Status = PlanStatus.Canceled,
                Progress = 30,
                CurrentStage = "Canceled by user",
                StartedAt = new DateTimeOffset(2026, 04, 01, 10, 15, 00, TimeSpan.Zero),
                FooterMessage = "The run was canceled before source collection completed. Partial findings remain available to the host app.",
                Steps = new List<PlanStepModel>
                {
                    new PlanStepModel { Id = "scope", Index = 1, Title = "Define scope and key comparison axes", Status = PlanStatus.Completed, Progress = 100 },
                    new PlanStepModel { Id = "gather", Index = 2, Title = "Collect primary sources and release notes", Status = PlanStatus.Canceled, Progress = 20, CurrentStage = "Canceled during vendor doc review" },
                    new PlanStepModel { Id = "report", Index = 3, Title = "Synthesize findings into a final report", Status = PlanStatus.Pending }
                }
            };
        }
    }
}
