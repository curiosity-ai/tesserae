using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 10, Icon = UIcons.MenuDots)]
    public class OverflowSetSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public OverflowSetSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(OverflowSetSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("OverflowSet is a container that automatically moves items that don't fit into the available space into an overflow menu."),
                    TextBlock("It is commonly used for command bars, navigation menus, or any list of actions where you want to maximize the visibility of primary items while ensuring all items are accessible.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use OverflowSet when you have a horizontal list of items that might exceed the screen width. Order items by importance so that the most critical actions are the last to be moved to the overflow menu. Provide a clear icon or label for the overflow trigger (usually a 'more' icon). Ensure that items in the overflow menu remain fully functional.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic OverflowSet"),
                    TextBlock("Resize the window or container to see items moving into the '...' menu."),
                    OverflowSet().Items(
                        Button("Action 1").Link().OnClick((s, e) => Toast().Information("Action 1")),
                        Button("Action 2").Link().OnClick((s, e) => Toast().Information("Action 2")),
                        Button("Action 3").Link().OnClick((s, e) => Toast().Information("Action 3")),
                        Button("Action 4").Link().OnClick((s, e) => Toast().Information("Action 4")),
                        Button("Action 5").Link().OnClick((s, e) => Toast().Information("Action 5")),
                        Button("Action 6").Link().OnClick((s, e) => Toast().Information("Action 6"))
                    ).PB(32),
                    SampleSubTitle("With Icons and Constraints"),
                    OverflowSet().MaxWidth(300.px()).Items(
                        Button("Edit").SetIcon(UIcons.Edit).Link(),
                        Button("Share").SetIcon(UIcons.Share).Link(),
                        Button("Delete").SetIcon(UIcons.Trash).Link(),
                        Button("Copy").SetIcon(UIcons.Copy).Link(),
                        Button("Move").SetIcon(UIcons.Arrows).Link()
                    ).PB(32),
                    SampleSubTitle("Custom Overflow Index"),
                    TextBlock("Force overflow to start after the first item:"),
                    OverflowSet().SetOverflowIndex(0).MaxWidth(400.px()).Items(
                        Button("Always Visible").Primary(),
                        Button("Option A").Link(),
                        Button("Option B").Link(),
                        Button("Option C").Link()
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
