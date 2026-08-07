using System;
using System.Collections.Generic;

using static Transpose.Core.dom;
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
            // --- Section 1: existing fluent API (unchanged behavior) ---
            var plan1 = Plan("SCIM user provisioning deep dive")
                .HeaderCommands(Button("Update").NoBorder().Rounded())
                .AddTask("Collect official SCIM RFCs and specifications from IETF and RFC repositories.", true)
                .AddTask("Survey major identity providers' SCIM documentation and authentication methods.", false)
                .AddTask("Gather sample SCIM request and response payloads and endpoint patterns.", false)
                .AddTask("Identify open-source SCIM implementations and C# libraries with examples.", false)
                .AddTask("Compile technical explanation covering endpoints, auth, schemas, and examples.", false)
                .FooterMessage("Finalizing details for licenses and attribution...")
                .FooterCommands(TextBlock("117 searches").Small().SemiBold())
                .Progress(1, 5)
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

            // --- Section 2: data-driven / streaming updates ---
            // A single Plan instance cycled through Pending -> Running ->
            // Completed / Failed / Canceled. Each click calls SetModel on the
            // same instance, demonstrating in-place DOM reconciliation.
            var streamedPlan = Plan("Deep research: SCIM provisioning")
                .HideStartStopButton()
                .MaxWidth(800.px());
            streamedPlan.Render().style.maxWidth = "800px";

            // Initial model
            streamedPlan.SetModel(BuildModel(PlanStatus.Pending, runningStep: -1));

            var pendingBtn   = Button("Pending").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Pending, runningStep: -1)));
            var runningBtn   = Button("Running").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Running, runningStep: 1)));
            var activeBtn    = Button("Active").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Active, runningStep: 1, activeInsteadOfRunning: true)));
            var completedBtn = Button("Completed").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Completed, runningStep: -1, allComplete: true)));
            var failedBtn    = Button("Failed").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Failed, runningStep: 1, failStep: 2)));
            var canceledBtn  = Button("Canceled").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Canceled, runningStep: -1)));

            var streamedControls = HStack().Gap(8.px()).Children(
                pendingBtn, runningBtn, activeBtn, completedBtn, failedBtn, canceledBtn
            );

            // --- Section 3: every status side by side ---
            // One row per PlanStatus so the color coding is easy to compare:
            // completed is green + semibold, running/active are primary + bold,
            // failed is red + bold on danger-colored text, pending is secondary
            // throughout, and canceled is a warning glyph on default text.
            var statusPlan = Plan("Status color coding")
                .AddTask("Completed — green icon, semibold text", PlanStatus.Completed)
                .AddTask("Running — primary spinner, bold text", PlanStatus.Running)
                .AddTask("Active — primary circle, bold text", PlanStatus.Active)
                .AddTask("Failed — red icon, bold danger text", PlanStatus.Failed)
                .AddTask("Pending — secondary icon and text", PlanStatus.Pending)
                .AddTask("Canceled — warning icon, semibold text", PlanStatus.Canceled)
                .HideStartStopButton()
                .FooterMessage("One row per PlanStatus")
                .Progress(1, 6)
                .MaxWidth(800.px());

            // --- Section 4: per-step badges ---
            var badgedPlan = Plan("Nightly build")
                .AddTask("Restore packages", PlanStatus.Completed, Badge("12s").Success())
                .AddTask("Compile", PlanStatus.Completed, Badge("1m 04s").Success())
                .AddTask("Run unit tests", PlanStatus.Running, Badge("318 / 902").Primary())
                .AddTask("Publish artifacts", PlanStatus.Pending, Badge("queued").Neutral())
                .AddTask("Deploy to staging", PlanStatus.Canceled, Badge("skipped").Warning())
                .FooterMessage("Any IComponent works as a badge, not just Badge(...)")
                .Progress(2, 5)
                .Start()
                .MaxWidth(800.px());

            // --- Section 5: compact mode ---
            // Compact() applied *after* the tasks, so the switch is exercised on
            // rows that already exist — it re-draws their glyphs in place.
            var compactPlan = Plan("Compact")
                .AddTask("Read the source spreadsheet", PlanStatus.Completed)
                .AddTask("Normalize the column names", PlanStatus.Completed)
                .AddTask("Reconcile against the ledger", PlanStatus.Running, Badge("428 rows").Primary())
                .AddTask("Write the summary", PlanStatus.Pending)
                .FooterMessage("Same plan, compact rail")
                .Progress(2, 4)
                .Start()
                .Compact()
                .MaxWidth(800.px());

            // The same plan with each piece of chrome dropped, so the effect of
            // each option is visible on its own.
            var noHeaderPlan = Plan("Hidden title")
                .NoHeader()
                .AddTask("Fetch", PlanStatus.Completed)
                .AddTask("Transform", PlanStatus.Running)
                .AddTask("Load", PlanStatus.Pending)
                .FooterMessage("NoHeader()")
                .Progress(1, 3)
                .HideStartStopButton()
                .MaxWidth(800.px());

            var noFooterPlan = Plan("NoFooter()")
                .NoFooter()
                .AddTask("Fetch", PlanStatus.Completed)
                .AddTask("Transform", PlanStatus.Running)
                .AddTask("Load", PlanStatus.Pending)
                .MaxWidth(800.px());

            var noBorderPlan = Plan("NoBorder()")
                .NoBorder()
                .AddTask("Fetch", PlanStatus.Completed)
                .AddTask("Transform", PlanStatus.Running)
                .AddTask("Load", PlanStatus.Pending)
                .FooterMessage("No border, radius or shadow")
                .Progress(1, 3)
                .HideStartStopButton()
                .MaxWidth(800.px());

            // --- Section 6: embedded, all chrome off ---
            // The shape to reach for when the plan is a detail of something else:
            // compact, no header, no footer, no border — the container supplies
            // the title and the frame.
            var embeddedInCard = Card(
                Plan("unused — the card supplies the title")
                    .Compact()
                    .NoHeader()
                    .NoFooter()
                    .NoBorder()
                    .AddTask("Collect the RFCs", PlanStatus.Completed)
                    .AddTask("Survey the providers", PlanStatus.Completed)
                    .AddTask("Gather sample payloads", PlanStatus.Running, Badge("3 of 8").Primary())
                    .AddTask("Write the explanation", PlanStatus.Pending)
                    .WS()
            ).SetTitle("Research progress").MaxWidth(480.px());

            var embeddedInResourceCard = ResourceCard()
                .SetIcon(Icon(UIcons.FileImport, size: TextSize.Large))
                .SetTitle("scim-provisioning.md")
                .SetSubtitle("Draft · 12 KB")
                .SetTags(Badge("research").Info())
                .SetDescription(
                    Plan("unused — the resource card supplies the title")
                        .Compact()
                        .NoHeader()
                        .NoFooter()
                        .NoBorder()
                        .AddTask("Collect the RFCs", PlanStatus.Completed)
                        .AddTask("Survey the providers", PlanStatus.Completed)
                        .AddTask("Gather sample payloads", PlanStatus.Running, Badge("3 of 8").Primary())
                        .AddTask("Write the explanation", PlanStatus.Pending)
                        .WS())
                .SetDate("Updated 4 minutes ago")
                .SetFooterCommands(Button("Open").Primary().Small())
                .MaxWidth(480.px());

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(PlanSample), UIcons.Map, "A component to display a plan")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("The Plan component displays a complex task with its sub-tasks and overall progress."))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        TextBlock("Default usage showing a running plan with partial progress.").SemiBold().PT(16).PB(8),
                        plan1,
                        TextBlock("A completed plan, with the stop button hidden.").SemiBold().PT(16).PB(8),
                        plan2,
                        TextBlock("A plan with indeterminate progress.").SemiBold().PT(16).PB(8),
                        plan3
                    )).SetTitle("Usage"),
                    Card(VStack().WS().Children(
                        TextBlock("Each PlanStatus has its own icon, icon color, text weight and text color.").PT(16).PB(8),
                        statusPlan
                    )).SetTitle("Status color coding"),
                    Card(VStack().WS().Children(
                        TextBlock("AddTask(title, status, badge) — or PlanStepModel.Badge — puts any component at the end of a step's title row.").PT(16).PB(8),
                        badgedPlan
                    )).SetTitle("Per-step badges"),
                    Card(VStack().WS().Children(
                        TextBlock("Compact() swaps the per-status glyphs for a plain status dot on a tighter grid. A running step keeps its spinner.").PT(16).PB(8),
                        compactPlan,
                        TextBlock("NoHeader() hides the title strip.").SemiBold().PT(16).PB(8),
                        noHeaderPlan,
                        TextBlock("NoFooter() hides the progress strip.").SemiBold().PT(16).PB(8),
                        noFooterPlan,
                        TextBlock("NoBorder() drops the border, radius and shadow.").SemiBold().PT(16).PB(8),
                        noBorderPlan
                    )).SetTitle("Compact mode and chrome options"),
                    Card(VStack().WS().Children(
                        TextBlock("Compact + NoHeader + NoFooter + NoBorder makes the plan a detail inside another component, which supplies the title and the frame.").PT(16).PB(8),
                        HStack().Gap(16.px()).Wrap().Children(
                            embeddedInCard,
                            embeddedInResourceCard
                        )
                    )).SetTitle("Embedded in a Card and a ResourceCard"),
                    Card(VStack().WS().Children(
                        TextBlock("The same Plan instance is updated by calling SetModel(model) — DOM nodes are reused across updates so animations, focus and scroll position are preserved.").PT(16).PB(8),
                        streamedControls,
                        TextBlock("").PT(8),
                        streamedPlan
                    )).SetTitle("Data-driven / streaming updates")
                ))
                .SeeAlso(typeof(ChatSample), typeof(ToolCallSample), typeof(StepperSample), typeof(TimelineSample), typeof(TreeSample));
        }

        private static PlanModel BuildModel(PlanStatus planStatus, int runningStep, bool allComplete = false, int failStep = -1, bool activeInsteadOfRunning = false)
        {
            var inProgress = activeInsteadOfRunning ? PlanStatus.Active : PlanStatus.Running;

            // Stable ids — the reconciler will reuse DOM nodes across calls.
            var steps = new List<PlanStepModel>
            {
                new PlanStepModel { Id = "s-collect",   Index = 0, Title = "Collect official SCIM RFCs",     Status = PlanStatus.Pending },
                new PlanStepModel { Id = "s-survey",   Index = 1, Title = "Survey identity providers",      Status = PlanStatus.Pending },
                new PlanStepModel { Id = "s-payloads", Index = 2, Title = "Gather sample payloads",         Status = PlanStatus.Pending },
                new PlanStepModel { Id = "s-impl",     Index = 3, Title = "Identify open-source libraries", Status = PlanStatus.Pending },
                new PlanStepModel { Id = "s-write",    Index = 4, Title = "Write the explanation",          Status = PlanStatus.Pending },
            };

            if (allComplete)
            {
                foreach (var s in steps) s.Status = PlanStatus.Completed;
            }
            else if (runningStep >= 0)
            {
                // Steps before runningStep are completed, step at runningStep is in progress.
                for (int i = 0; i < runningStep && i < steps.Count; i++) steps[i].Status = PlanStatus.Completed;
                if (runningStep < steps.Count)
                {
                    steps[runningStep].Status = inProgress;
                    steps[runningStep].Progress = 0.6f;
                    steps[runningStep].CurrentStage = "fetching documents...";
                    steps[runningStep].Badge = Badge("60%").Primary();
                    steps[runningStep].Substeps = new List<PlanSubstepModel>
                    {
                        new PlanSubstepModel { Id = "sub-okta",  Title = "okta.com",   Status = PlanStatus.Completed, Message = "scim/v2/Users (200)" },
                        new PlanSubstepModel { Id = "sub-azure", Title = "azure.com",  Status = inProgress,           Message = "fetching..." },
                        new PlanSubstepModel { Id = "sub-jump",  Title = "jumpcloud.com", Status = PlanStatus.Pending },
                    };
                }
            }

            if (failStep >= 0 && failStep < steps.Count)
            {
                steps[failStep].Status = PlanStatus.Failed;
                steps[failStep].CurrentStage = "HTTP 503 from upstream";
                steps[failStep].Badge = Badge("503").Danger();
            }

            int completed = 0;
            foreach (var s in steps) if (s.Status == PlanStatus.Completed) completed++;

            return new PlanModel
            {
                Id = "deep-research-1",
                Title = "Deep research: SCIM provisioning",
                Status = planStatus,
                Progress = allComplete ? (float?)1f : (runningStep >= 0 ? (float?)((float)completed / steps.Count) : null),
                CurrentStage = planStatus == PlanStatus.Running ? "step " + (runningStep + 1) + " of " + steps.Count : null,
                Steps = steps,
                StartedAt = planStatus == PlanStatus.Pending ? (DateTimeOffset?)null : DateTimeOffset.Now.AddMinutes(-3),
                CompletedAt = allComplete ? DateTimeOffset.Now : (DateTimeOffset?)null,
                FooterMessage = planStatus == PlanStatus.Failed  ? "Aborted after error" :
                                planStatus == PlanStatus.Canceled ? "Canceled by user" : null,
                Searches = planStatus == PlanStatus.Pending ? null : (allComplete ? "142 searches" : "117 searches"),
            };
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
