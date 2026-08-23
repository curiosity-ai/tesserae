using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Commands, Order = 50, Icon = UIcons.MenuDots, Description = "A row that folds what doesn't fit into a menu")]
    public class OverflowSetSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public OverflowSetSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(OverflowSetSample), UIcons.MenuDots, "A component to display an overflow set")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("OverflowSet is a container that automatically moves items that don't fit into the available space into an overflow menu."),
                    TextBlock("It is commonly used for command bars, navigation menus, or any list of actions where you want to maximize the visibility of primary items while ensuring all items are accessible."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use OverflowSet when you have a horizontal list of items that might exceed the screen width. Order items by importance so that the most critical actions are the last to be moved to the overflow menu. Provide a clear icon or label for the overflow trigger (usually a 'more' icon). Ensure that items in the overflow menu remain fully functional."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic OverflowSet"),
                    TextBlock("Resize the window or container to see items moving into the '...' menu."),
                    OverflowSet().Items(
                        Button("Action 1").Class("tss-btn-link").OnClick((s, e) => Toast().Information("Action 1")),
                        Button("Action 2").Class("tss-btn-link").OnClick((s, e) => Toast().Information("Action 2")),
                        Button("Action 3").Class("tss-btn-link").OnClick((s, e) => Toast().Information("Action 3")),
                        Button("Action 4").Class("tss-btn-link").OnClick((s, e) => Toast().Information("Action 4")),
                        Button("Action 5").Class("tss-btn-link").OnClick((s, e) => Toast().Information("Action 5")),
                        Button("Action 6").Class("tss-btn-link").OnClick((s, e) => Toast().Information("Action 6"))
                    ).PB(32),
                    SampleSubTitle("With Icons and Constraints"),
                    OverflowSet().MaxWidth(300.px()).Items(
                        Button("Edit").SetIcon(UIcons.Edit).Class("tss-btn-link"),
                        Button("Share").SetIcon(UIcons.Share).Class("tss-btn-link"),
                        Button("Delete").SetIcon(UIcons.Trash).Class("tss-btn-link"),
                        Button("Copy").SetIcon(UIcons.Copy).Class("tss-btn-link"),
                        Button("Move").SetIcon(UIcons.Arrows).Class("tss-btn-link")
                    ).PB(32),
                    SampleSubTitle("Custom Overflow Index"),
                    TextBlock("Force overflow to start after the first item:"),
                    OverflowSet().SetOverflowIndex(0).MaxWidth(400.px()).Items(
                        Button("Always Visible").Primary(),
                        Button("Option A").Class("tss-btn-link"),
                        Button("Option B").Class("tss-btn-link"),
                        Button("Option C").Class("tss-btn-link")
                    )
                )).SetTitle("Usage")))
               .SeeAlso(typeof(BreadcrumbSample), typeof(CommandBarSample), typeof(MenuSample), typeof(StackSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
