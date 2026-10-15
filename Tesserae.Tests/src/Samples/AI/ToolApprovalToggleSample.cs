using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.AI, Order = 80, Icon = UIcons.HandPaper, Description = "How an AI tool may run: allowed, approved, blocked")]
    public class ToolApprovalToggleSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ToolApprovalToggleSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ToolApprovalToggleSample), UIcons.HandPaper, "Pick how an AI tool may run")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A ToolApprovalToggle sets the ToolApprovalMode of one AI tool: it runs on its own, it waits for the user to approve each call, or it is blocked. It is a compact IconToggle whose selected icon takes the colour of what it means, so a list of tools reads at a glance."),
                        TextBlock("ToolApprovalToggle.IconOf gives the same icons to a read-only view of the modes, so a page that only shows them matches the one that sets them."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        VStack().Children(
                            SampleDo("Default a tool that changes anything to NeedsApproval"),
                            SampleDo("Pass translated labels to the constructor when the UI is translated"),
                            SampleDont("Use it for a choice that is not about running a tool - use an IconToggle")))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A list of tools"),
                        TextBlock("One toggle per tool, each reporting its own changes.").Secondary(),
                        ToolList(),

                        SampleSubTitle("Read-only"),
                        TextBlock("IconOf and DefaultLabel draw the mode where it can only be looked at.").Secondary(),
                        ReadOnlyExample(),

                        SampleSubTitle("Disabled"),
                        TextBlock("Disabled() shows the mode without letting it change.").Secondary(),
                        ToolApprovalToggle(ToolApprovalMode.Blocked).Disabled()))));
        }

        private static IComponent ToolList()
        {
            var status = TextBlock("Pick a mode for a tool.").Secondary().PT(8);

            IComponent Row(string tool, string description, ToolApprovalMode mode)
            {
                return HStack().WS().AlignItemsCenter().PB(8).Children(
                    VStack().W(10).Grow().Children(
                        TextBlock(tool).SemiBold(),
                        TextBlock(description).XSmall().Secondary()),
                    ToolApprovalToggle(mode).OnChange((_, m) => status.Text = $"{tool}: {ToolApprovalToggle.DefaultLabel(m)}"));
            }

            return VStack().WS().MaxWidth(520.px()).Children(
                Row("List tickets",  "Lists open support tickets.",   ToolApprovalMode.AlwaysAllow),
                Row("Create ticket", "Opens a new support ticket.",   ToolApprovalMode.NeedsApproval),
                Row("Delete ticket", "Deletes a ticket permanently.", ToolApprovalMode.Blocked),
                status);
        }

        private static IComponent ReadOnlyExample()
        {
            var row = HStack().AlignItemsCenter();

            foreach (var mode in new[] { ToolApprovalMode.AlwaysAllow, ToolApprovalMode.NeedsApproval, ToolApprovalMode.Blocked })
            {
                row.Add(HStack().AlignItemsCenter().PR(16).Children(
                    Icon(ToolApprovalToggle.IconOf(mode)).PR(4),
                    TextBlock(ToolApprovalToggle.DefaultLabel(mode)).Small().Secondary()));
            }

            return row;
        }

        public HTMLElement Render() => _content.Render();
    }
}
