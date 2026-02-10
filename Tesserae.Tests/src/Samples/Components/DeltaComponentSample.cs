using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.Refresh)]
    public class DeltaComponentSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DeltaComponentSample()
        {
            var deltaContainer = document.createElement("div");
            var deltaComponent = new DeltaComponent(Raw(deltaContainer));

            var html = "";
            int step = 1;

            var appendTextBtn = Button("Append Text (Auto-Split)").OnClick(() =>
            {
                html += "Step " + step++;
                var d = document.createElement("div");
                d.textContent = html; // This sets text content, no HTML parsing
                // Wait, if I append "Step 1" then "Step 1Step 2", I want "Step 2" to be a span?
                // Yes, my DeltaComponent logic handles text prefix matching.
                deltaComponent.ReplaceContent(Raw(d));
            });

            var appendSpanBtn = Button("Append Span (Explicit)").OnClick(() =>
            {
                html += "<span>Step " + step++ + "</span>";
                var d = document.createElement("div");
                d.innerHTML = html;
                deltaComponent.ReplaceContent(Raw(d));
            });

            var appendMixedBtn = Button("Append Mixed (Text + Span)").OnClick(() =>
            {
                // This resets strict text mode if used after text
                html += "Step " + step++ + "<span>Span " + step++ + "</span>";
                var d = document.createElement("div");
                d.innerHTML = html;
                deltaComponent.ReplaceContent(Raw(d));
            });

            var resetBtn = Button("Reset").OnClick(() =>
            {
                html = "";
                step = 1;
                var d = document.createElement("div");
                deltaComponent.ReplaceContent(Raw(d));
            });

            _content = SectionStack()
                .Title(SampleHeader(nameof(DeltaComponent)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("DeltaComponent updates its DOM tree to match a new component's DOM tree using a diff algorithm. It detects text appends and adds them as new spans to avoid full re-rendering."),
                    HStack().Children(appendTextBtn, appendSpanBtn, appendMixedBtn, resetBtn),
                    SampleTitle("Output"),
                    deltaComponent
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
