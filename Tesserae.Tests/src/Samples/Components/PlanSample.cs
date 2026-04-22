using System;

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

            _content = SectionStack()
                .SampleTitle(nameof(PlanSample), UIcons.Map, "A component to display a plan")
                .Section(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("The Plan component displays a complex task with its sub-tasks and overall progress."))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                    TextBlock("Default usage showing a running plan with partial progress.").SemiBold().PT(16).PB(8),
                    plan1,
                    TextBlock("A completed plan, with the stop button hidden.").SemiBold().PT(16).PB(8),
                    plan2,
                    TextBlock("A plan with indeterminate progress.").SemiBold().PT(16).PB(8),
                    plan3
                )).SetTitle("Usage")));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
