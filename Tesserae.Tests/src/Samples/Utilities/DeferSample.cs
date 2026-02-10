using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 0, Icon = UIcons.Spinner)]
    public class DeferSample : IComponent, ISample
    {
        private readonly IComponent content;

        public DeferSample()
        {
            var stack       = SectionStack();
            var countSlider = Slider(5, 0, 10, 1);

            content = SectionStack()
               .Title(SampleHeader(nameof(DeferSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Defer is a powerful utility for handling asynchronous UI rendering. It allows you to wrap a component that depends on an async task (like a network request). While the task is in progress, a loading message or a skeleton loader can be shown. Once the task completes, the actual component is seamlessly rendered in its place.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Defer for any component that requires remote data or slow computations. Provide a 'loadMessage'—ideally a skeleton loader—that mimics the final layout to reduce visual flickering. Leverage the lazy-loading nature of Defer, as the async task is only triggered when the component is first rendered. Combine Defer with observables to create highly reactive and responsive interfaces.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Stack().Children(
                        HStack().Children(
                            Stack().Children(
                                Label("Number of items:").SetContent(countSlider.OnInput((s, e) => SetChildren(stack, s.Value)))
                            ))),
                    stack.HeightAuto()
                ));
            SetChildren(stack, 5);
        }

        private void SetChildren(SectionStack stack, int count)
        {
            stack.Clear();

            for (int i = 0; i < count; i++)
            {
                var delay = (i + 1) * 1_000;

                stack.Section(Stack().Children(
                    TextBlock($"Section {i} - delayed {i + 1} seconds").MediumPlus().SemiBold(),
                    Defer(async () =>
                        {
                            await Task.Delay(delay);

                            return HStack().WS().HS().Children(Image("./assets/img/curiosity-logo.svg").W(40).H(40), VStack().W(50).Grow().PL(8).Children(
                                TextBlock("Wrap (Default)").SmallPlus(),
                                TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").Width(50.percent()).PT(4),
                                TextBlock("No Wrap").SmallPlus().PT(4),
                                Button("Click Me").Primary(),
                                Label("Icon:").Inline().SetContent(Icon(UIcons.HelicopterSide, weight: UIconsWeight.Bold, size: TextSize.Large)),
                                TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").NoWrap().Width(50.percent()).PT(4)
                            ));
                        }, loadMessage:
                        HStack().WS().HS().Children(Image("").W(40).H(40), VStack().W(50).Grow().PL(8).Children(
                            TextBlock("Wrap (Default)").SmallPlus(),
                            TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").Width(50.percent()).PT(4),
                            TextBlock("No Wrap").SmallPlus().PT(4),
                            Button("Click Me").Primary(),
                            Label("Icon:").Inline().SetContent(Icon(UIcons.HelicopterSide, weight: UIconsWeight.Bold, size: TextSize.Large)),
                            TextBlock("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.").NoWrap().Width(50.percent()).PT(4)
                        )).Skeleton()
                    )));
            }
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}