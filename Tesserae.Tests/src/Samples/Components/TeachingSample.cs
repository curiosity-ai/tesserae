using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 200, Icon = UIcons.ShoePrints)]
    public class TeachingSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TeachingSample()
        {
            var btn1 = Button("Feature A").Primary();
            var btn2 = Button("Feature B");
            var btn3 = Button("Feature C");

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TeachingSample), UIcons.ShoePrints, "An onboarding walkthrough that highlights UI elements one by one")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Teaching is a component for creating guided onboarding or instructional walkthroughs. It highlights specific UI elements sequentially, displaying a tooltip with contextual information at each step. Steps can require an explicit user action (clicking Next) or auto-advance after a fixed delay."),
                    TextBlock("Use Teaching to help first-time users discover key features in a structured, low-friction way."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Teaching sparingly — only for genuinely complex or non-obvious workflows. Prefer NextButton steps for interactive demos so users stay in control. Avoid triggering a walkthrough every time the page loads; use RunIf with a condition (e.g. a first-run flag) so it only appears once. Keep tooltip copy short and action-oriented."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("3-Step Walkthrough Demo"),
                    TextBlock("Click 'Start Walkthrough' to begin. Step 1 requires clicking Next, Step 2 auto-advances after 5 seconds, and Step 3 finishes with a confirmation toast.").PB(8),
                    HStack().Children(btn1, btn2, btn3),
                    Button("Start Walkthrough").SetIcon(UIcons.Play).MT(16).OnClick(() =>
                        Teaching()
                           .AddStep(btn1, TextBlock("Step 1: This is Feature A. Click Next to continue."), stepType: Teaching.StepType.NextButton)
                           .AddStep(btn2, TextBlock("Step 2: This is Feature B. It will auto-advance in 5 seconds."), stepType: Teaching.StepType.After5seconds)
                           .AddStep(btn3, TextBlock("Step 3: This is Feature C. You're done!"), stepType: Teaching.StepType.NextButton)
                           .OnComplete(() => Toast().Success("Walkthrough complete!"))
                           .RunNow())
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
