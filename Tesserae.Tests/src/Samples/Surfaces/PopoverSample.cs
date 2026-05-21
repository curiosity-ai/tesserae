using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 20, Icon = UIcons.Comment)]
    public class PopoverSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PopoverSample()
        {
            // Reusable popover — configure once, show on demand.
            Popover info = null;
            info = Popover()
                .Content(Card(VStack().Children(
                    TextBlock("This popover is reusable — the same instance is shown for any anchor.").MaxWidth(260.px()),
                    Button("Close").Compact().OnClick(() => info.Hide()).MT(8))))
                .Placement(TooltipPlacement.Bottom)
                .Arrow();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PopoverSample), UIcons.Comment, "A reusable, anchored overlay surface")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Popover is the general-purpose primitive on top of which menus, comboboxes, date pickers and similar transient surfaces are built. Unlike Tooltip — which is hover-triggered — a Popover stays visible until explicitly closed, the user clicks outside, or the anchor leaves the DOM."),
                    TextBlock("Positioning, click-outside dismissal, arrows and animation are delegated to Tippy/Popper, so the same placements available to Tooltip apply to Popover."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Reusable popover, shown against different anchors"),
                    HStack().Children(
                        Button("Anchor A").Var(out var aA).OnClick((s, _) => info.ShowFor(aA)).MR(8),
                        Button("Anchor B").Var(out var aB).OnClick((s, _) => info.ShowFor(aB)).MR(8),
                        Button("Anchor C").Var(out var aC).OnClick((s, _) => info.ShowFor(aC))
                    ),
                    SampleSubTitle("Configured inline"),
                    Button("Show inline popover").Var(out var btn).OnClick((s, _) =>
                        Popover()
                            .Content(Card(VStack().Children(
                                TextBlock("Inline-built popover. Try clicking outside or pressing Escape."),
                                Label("Pick something").SetContent(Dropdown().Items(
                                    DropdownItem("Option A"),
                                    DropdownItem("Option B"),
                                    DropdownItem("Option C"))))))
                            .Placement(TooltipPlacement.BottomStart)
                            .Arrow()
                            .ShowFor(btn))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
