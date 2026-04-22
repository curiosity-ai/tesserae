using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 25, Icon = UIcons.Browser)]
    public class TabbedModalSample : IComponent, ISample
    {
        private readonly IComponent content;
        private Pivot _pivot;
        private int _modalCount = 0;

        public TabbedModalSample()
        {
            _pivot = Pivot();

            content = SectionStack()
               .Title(SampleHeader(nameof(TabbedModalSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("This sample demonstrates how to host Modals within a Pivot component, as well as how to use closeable tabs. Hosting a Modal within a Pivot allows it to embed its content while taking advantage of the Pivot's caching and lifecycle, and displaying a close button in the tab title automatically.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    HStack().Children(
                        Button("Open Closeable Modal").OnClick((_, __) => AddModalTab(closeable: true)),
                        Button("Open Non-Closeable Modal").OnClick((_, __) => AddModalTab(closeable: false))
                    ),
                    _pivot.WS().H(500)
                ));

            // Add an initial modal
            AddModalTab(true);
        }

        private void AddModalTab(bool closeable)
        {
            _modalCount++;
            string id = $"modal-{_modalCount}";
            string titleText = $"Modal {_modalCount}";

            var modal = Modal(titleText).Content(
                VStack().Children(
                    TextBlock($"This is the content of {titleText}").Regular(),
                    TextBlock($"It is embedded in the pivot and {(closeable ? "can" : "cannot")} be closed.").Small()
                ).Padding(16.px())
            );

            _pivot.Host(modal, id, PivotTitle(titleText, UIcons.Browser), closeable: closeable, onClosed: () => {
                console.log($"Tab {id} was closed!");
            });

            _pivot.Select(id);
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
