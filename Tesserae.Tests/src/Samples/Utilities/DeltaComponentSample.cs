using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Utilities, Order = 40, Icon = UIcons.Refresh, Description = "Patch the DOM instead of re-rendering it")]
    public class DeltaComponentSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DeltaComponentSample()
        {
            var deltaContainer = document.createElement("div");
            var deltaComponent = DeltaComponent(Raw(deltaContainer)).Animated();

            var html = "";
            int step = 1;

            var typing = Button("Type Lorem Ipsum").OnClick(() =>
            {
                var lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris  nisi ut aliquip ex ea commodo consequat.";

                var d1 = document.createElement("div");
                d1.innerHTML = "<div><span></span><b>Starting...</b></div>";
                deltaComponent.ReplaceContent(Raw(d1));

                int index = 0;

                void TypeNextChar()
                {
                    if (index > lorem.Length)
                    {
                        var dFinal = document.createElement("div");
                        dFinal.innerHTML = $"<div><span>{lorem}</span><b> Done ✔</b></div>";
                        deltaComponent.ReplaceContent(Raw(dFinal));
                        return;
                    }

                    var currentText = lorem.Substring(0, index);

                    var d = document.createElement("div");
                    d.innerHTML = $"<div><span>{currentText}</span><b>Typing... {index}/{lorem.Length}</b></div>";
                    deltaComponent.ReplaceContent(Raw(d));

                    index++;
                    window.setTimeout(_ => TypeNextChar(), 25);
                }

                TypeNextChar();
            });

            var typingWithComponents = Button("Type Lorem Ipsum 2").OnClick(() =>
            {
                var lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris  nisi ut aliquip ex ea commodo consequat.".ToArray();

                var stack = HStack().WS().Children(TextBlock("Starting..."));

                deltaComponent.ReplaceContent(stack);

                int index = 0;

                void TypeNextChar()
                {
                    if (index > lorem.Length)
                    {
                        stack = HStack().WS().Children(lorem.Select(t => TextBlock(t.ToString()).PR(t == ' ' ? 4 : 0)).ToArray(), Icon(UIcons.Check).PR(8));
                        deltaComponent.ReplaceContent(stack);
                        return;
                    }

                    var currentText = lorem.Take(index).ToArray();
                    stack = HStack().WS().Children(currentText.Select(t => TextBlock(t.ToString()).PR(t == ' ' ? 4 : 0)).ToArray());
                    deltaComponent.ReplaceContent(stack);

                    index++;
                    window.setTimeout(_ => TypeNextChar(), 25);
                }

                TypeNextChar();
            });

            var resetBtn = Button("Reset").OnClick(() =>
            {
                html = "";
                step = 1;
                var d = document.createElement("div");
                deltaComponent.ReplaceContent(Raw(d));
            });

            // Shadow DOM Sample
            var shadowContainer = document.createElement("div");
            var shadowDeltaComponent = DeltaComponent(Raw(shadowContainer), useShadowDom: true).Animated();

            var shadowTyping = Button("Type in Shadow DOM").OnClick(() =>
            {
                var lorem = "This text is inside a Shadow DOM!";

                var d1 = document.createElement("div");
                d1.innerHTML = "<div><span></span><b>Shadow Starting...</b></div>";
                shadowDeltaComponent.ReplaceContent(Raw(d1));

                int index = 0;

                void TypeNextChar()
                {
                    if (index > lorem.Length)
                    {
                        var dFinal = document.createElement("div");
                        dFinal.innerHTML = $"<div><span>{lorem}</span><b> Shadow Done ✔</b></div>";
                        shadowDeltaComponent.ReplaceContent(Raw(dFinal));
                        return;
                    }

                    var currentText = lorem.Substring(0, index);

                    var d = document.createElement("div");
                    d.innerHTML = $"<div><span>{currentText}</span><b>Shadow Typing... {index}/{lorem.Length}</b></div>";
                    shadowDeltaComponent.ReplaceContent(Raw(d));

                    index++;
                    window.setTimeout(_ => TypeNextChar(), 25);
                }

                TypeNextChar();
            });

             var shadowResetBtn = Button("Reset Shadow").OnClick(() =>
            {
                var d = document.createElement("div");
                d.textContent = "Shadow DOM Initial Content";
                shadowDeltaComponent.ReplaceContent(Raw(d));
            });


            //A node keeps the listeners its component gave it, so the reconciler only ever patches a
            //node into a node of the same component. This is the case that proves it: the content
            //alternates between a ToolCall and a ToolsUsed - both a div, so the old nodeName check
            //reconciled them and the group was left answering to the chip's click handler. The
            //DeltaComponent is stretched, so the swap also has to keep the .WS() written on its root.
            var swapState  = 0;
            var swapDelta  = DeltaComponent(BuildSwappable(0)).WS().Animated();

            var swapBtn = Button("Swap component").OnClick(() =>
            {
                swapState = (swapState + 1) % 2;
                swapDelta.ReplaceContent(BuildSwappable(swapState));
            });

            //The pair above are Tesserae's own. These two are defined in this project, which is a
            //consumer of the package like any other: two classes that both return a Stack's element,
            //both with a handler on it, and nothing on either of them saying which component it is.
            //Clicking after a swap has to run the handler of the component now on screen.
            var pickedLabel = TextBlock("nothing clicked yet").Class("tss-delta-picked");
            var consumerState = 0;

            IComponent BuildConsumer(int state) => state == 0
                ? (IComponent)new ConsumerChip(name => pickedLabel.Text = name + " handled the click")
                : new ConsumerGroup(name => pickedLabel.Text = name + " handled the click");

            //Everything applied to the DeltaComponent itself rather than to its content: these land
            //on the element the content rendered, which a swap throws away, so they are recorded and
            //applied again to whatever replaces it.
            var consumerDelta = DeltaComponent(BuildConsumer(0))
               .WS()
               .Animated()
               .Id("tss-delta-consumer")
               .Class("tss-delta-outer-class")
               .Style(css => css.outline = "1px dashed var(--tss-colors-primary-background)")
               .Tooltip("Applied to the DeltaComponent, not to its content");

            var consumerBtn = Button("Swap consumer component").OnClick(() =>
            {
                consumerState = (consumerState + 1) % 2;
                consumerDelta.ReplaceContent(BuildConsumer(consumerState));
            });

            _content = SectionStack().Secondary()
                .SampleTitle(typeof(DeltaComponent), UIcons.Refresh, "A component that animates changes")
                .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("DeltaComponent updates its DOM tree to match a new component's DOM tree using a diff algorithm. It detects text appends and adds them as new spans to avoid full re-rendering."),
                    HStack().Children(typing, typingWithComponents, resetBtn))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                    deltaComponent)).SetTitle("Output"),
                    Card(VStack().WS().Children(
                    TextBlock("This DeltaComponent renders its content inside a Shadow DOM root."),
                    HStack().Children(shadowTyping, shadowResetBtn),
                    shadowDeltaComponent
                )).SetTitle("Shadow DOM"),
                    Card(VStack().WS().Children(
                    TextBlock("Swapping one component for another of the same tag replaces the node instead of patching it, so the new component answers to its own handlers and not to the ones its predecessor left behind. Expand a row, swap, and expand again: the content is the new component's."),
                    HStack().Children(swapBtn),
                    swapDelta
                )).SetTitle("Swapping components"),
                    Card(VStack().WS().Children(
                    TextBlock("The same guarantee for a component the toolkit has never seen. ConsumerChip and ConsumerGroup are declared in this sample project, both return a Stack's element and both put a click handler on it, and neither declares anything about itself. The component is taken from the container that added it, so they are still told apart."),
                    HStack().Children(consumerBtn),
                    consumerDelta,
                    pickedLabel
                )).SetTitle("A component from outside the toolkit")))
                .SeeAlso(typeof(MetricSample), typeof(SparklineSample), typeof(ChartsSample), typeof(ContributionBarSample), typeof(BadgeSample));
        }

        //Two components that render the same tag, so only their identity tells the reconciler that
        //one may not be patched into the other.
        private static IComponent BuildSwappable(int state) =>
            state == 0
                ? (IComponent)ToolCall(UIcons.Database, "Fetch index statistics", () => TextBlock("4 indexes, 1.2M documents.").BreakSpaces())
                : ToolsUsed(
                        ToolCall(UIcons.Search, "Grep \"Delta\" Tesserae/src/", () => TextBlock("Tesserae/src/Components/DeltaComponent.cs").BreakSpaces()),
                        ToolCall(UIcons.Terminal, "Bash dotnet build", () => TextBlock("Build succeeded.").BreakSpaces()))
                   .Inline();

        public HTMLElement Render() => _content.Render();

        //Two components written the way an application writes them: no base class, a Stack for a
        //root, and a listener on that root. Their first CSS class is "tss-stack" for both.
        private sealed class ConsumerChip : IComponent
        {
            private readonly Stack _stack;

            public ConsumerChip(Action<string> onPicked)
            {
                _stack = VStack().WS().Class("tss-delta-consumer-chip").Children(TextBlock("Chip - click me"));
                _stack.Render().addEventListener("click", _ => onPicked("ConsumerChip"));
            }

            public HTMLElement Render() => _stack.Render();
        }

        private sealed class ConsumerGroup : IComponent
        {
            private readonly Stack _stack;

            public ConsumerGroup(Action<string> onPicked)
            {
                _stack = VStack().WS().Class("tss-delta-consumer-group").Children(TextBlock("Group - click me"));
                _stack.Render().addEventListener("click", _ => onPicked("ConsumerGroup"));
            }

            public HTMLElement Render() => _stack.Render();
        }
    }
}
