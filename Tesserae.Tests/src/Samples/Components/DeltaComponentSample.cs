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
                d.textContent = html;
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
                html += "Step " + step++ + "<span>Span " + step++ + "</span>";
                var d = document.createElement("div");
                d.innerHTML = html;
                deltaComponent.ReplaceContent(Raw(d));
            });

            var complexUpdateBtn = Button("Complex Nested Update").OnClick(() =>
            {
                // This simulates updating text inside a nested structure
                // Start: <div><span>Prefix</span></div>
                // Update: <div><span>PrefixSuffix</span></div>
                // Should result in: <div><span>Prefix<span>Suffix</span></span></div>

                var d1 = document.createElement("div");
                d1.innerHTML = "<div><span>Prefix</span><b>Bold</b></div>";
                deltaComponent.ReplaceContent(Raw(d1));

                window.setTimeout(_ =>
                {
                   var d2 = document.createElement("div");
                   d2.innerHTML = "<div><span>PrefixSuffix</span><b>BoldChange</b></div>";
                   deltaComponent.ReplaceContent(Raw(d2));
                }, 500);
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
                    HStack().Children(appendTextBtn, appendSpanBtn, appendMixedBtn, complexUpdateBtn, resetBtn),
                    SampleTitle("Output"),
                    deltaComponent
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
