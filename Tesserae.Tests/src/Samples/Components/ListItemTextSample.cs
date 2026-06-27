using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.List)]
    public class ListItemTextSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ListItemTextSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ListItemTextSample), UIcons.List, "A simple title + subtitle list item, with an optional icon")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ListItemText displays a bold title with a lighter subtitle underneath. It is handy for list rows, settings entries and notification items where a short heading needs a secondary line of context."),
                    TextBlock("An optional leading icon can be shown inside a rounded-square background to add visual identity to each row."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Title and subtitle"),
                    ListItemText("SB A320-78-1042", "Thrust reverser — blocker door hinge fitting inspection"),

                    SampleSubTitle("Title only"),
                    ListItemText("SB A320-78-1042"),

                    SampleSubTitle("With an icon in a rounded square"),
                    VStack().Children(
                        ListItemText("SB A320-78-1042", "Thrust reverser — blocker door hinge fitting inspection")
                            .SetIcon(UIcons.WrenchSimple),
                        ListItemText("Inspection passed", "All hinge fittings within tolerance")
                            .SetIcon(UIcons.Check)
                            .IconForeground("var(--tss-success-foreground-color)")
                            .IconBackground("var(--tss-success-background-color)"),
                        ListItemText("Action required", "Re-torque fitting and re-inspect within 48h")
                            .SetIcon(UIcons.TriangleWarning)
                            .IconForeground("var(--tss-danger-foreground-color)")
                            .IconBackground("var(--tss-danger-background-color)")
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
