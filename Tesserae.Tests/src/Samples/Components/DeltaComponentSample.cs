using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.Refresh)]
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


            _content = SectionStack()
                .Title(SampleHeader(nameof(DeltaComponent)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("DeltaComponent updates its DOM tree to match a new component's DOM tree using a diff algorithm. It detects text appends and adds them as new spans to avoid full re-rendering."),
                    HStack().Children(typing, typingWithComponents, resetBtn),
                    SampleTitle("Output"),
                    deltaComponent,
                    SampleTitle("Shadow DOM"),
                    TextBlock("This DeltaComponent renders its content inside a Shadow DOM root."),
                    HStack().Children(shadowTyping, shadowResetBtn),
                    shadowDeltaComponent
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
