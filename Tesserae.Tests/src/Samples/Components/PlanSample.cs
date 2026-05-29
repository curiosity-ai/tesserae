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
            var completedBtn = Button("Completed").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Completed, runningStep: -1, allComplete: true)));
            var failedBtn    = Button("Failed").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Failed, runningStep: 1, failStep: 2)));
            var canceledBtn  = Button("Canceled").OnClick(() => streamedPlan.SetModel(BuildModel(PlanStatus.Canceled, runningStep: -1)));

            var streamedControls = HStack().Gap(8.px()).Children(
                pendingBtn, runningBtn, completedBtn, failedBtn, canceledBtn
            );

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
                        TextBlock("The same Plan instance is updated by calling SetModel(model) — DOM nodes are reused across updates so animations, focus and scroll position are preserved.").PT(16).PB(8),
                        streamedControls,
                        TextBlock("").PT(8),
                        streamedPlan
                    )).SetTitle("Data-driven / streaming updates")
                ));
        }

        private static PlanModel BuildModel(PlanStatus planStatus, int runningStep, bool allComplete = false, int failStep = -1)
        {
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
                // Steps before runningStep are completed, step at runningStep is running.
                for (int i = 0; i < runningStep && i < steps.Count; i++) steps[i].Status = PlanStatus.Completed;
                if (runningStep < steps.Count)
                {
                    steps[runningStep].Status = PlanStatus.Running;
                    steps[runningStep].Progress = 0.6f;
                    steps[runningStep].CurrentStage = "fetching documents...";
                    steps[runningStep].Substeps = new List<PlanSubstepModel>
                    {
                        new PlanSubstepModel { Id = "sub-okta",  Title = "okta.com",   Status = PlanStatus.Completed, Message = "scim/v2/Users (200)" },
                        new PlanSubstepModel { Id = "sub-azure", Title = "azure.com",  Status = PlanStatus.Running,   Message = "fetching..." },
                        new PlanSubstepModel { Id = "sub-jump",  Title = "jumpcloud.com", Status = PlanStatus.Pending },
                    };
                }
            }

            if (failStep >= 0 && failStep < steps.Count)
            {
                steps[failStep].Status = PlanStatus.Failed;
                steps[failStep].CurrentStage = "HTTP 503 from upstream";
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
                FooterMessage = planStatus == PlanStatus.Running ? "117 searches" :
                                planStatus == PlanStatus.Failed  ? "Aborted after error" :
                                planStatus == PlanStatus.Canceled ? "Canceled by user" : null,
            };
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
