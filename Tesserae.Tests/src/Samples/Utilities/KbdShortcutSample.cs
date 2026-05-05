using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 11, Icon = UIcons.Keyboard)]
    public class KbdShortcutSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public KbdShortcutSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(KbdShortcutSample), UIcons.Keyboard, "Render keyboard shortcuts as styled key chips")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("KbdShortcut renders one or more key names as styled chips that look like physical keyboard keys. It automatically adapts modifier key labels to the current OS (⌘ on macOS, Ctrl on Windows/Linux) and handles special keys like Enter (↵), Escape, and arrow keys."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use KbdShortcut inline inside tooltips, help text, and command-palette descriptions to make shortcuts easy to scan. Keep shortcuts short — a maximum of 3 keys is recommended."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Common Shortcuts"),
                    VStack().Gap(12.px()).Children(
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Search").W(120.px()).Small(),
                            KbdShortcut("Ctrl", "K")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Save").W(120.px()).Small(),
                            KbdShortcut("Ctrl", "S")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Undo").W(120.px()).Small(),
                            KbdShortcut("Ctrl", "Z")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Redo").W(120.px()).Small(),
                            KbdShortcut("Ctrl", "Shift", "Z")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Copy").W(120.px()).Small(),
                            KbdShortcut("Ctrl", "C")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Select all").W(120.px()).Small(),
                            KbdShortcut("Ctrl", "A")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Close / Cancel").W(120.px()).Small(),
                            KbdShortcut("Escape")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Confirm").W(120.px()).Small(),
                            KbdShortcut("Enter")
                        )
                    ),
                    SampleSubTitle("Special Keys"),
                    HStack().Gap(16.px()).AlignItems(ItemAlign.Center).Children(
                        KbdShortcut("ArrowUp"),
                        KbdShortcut("ArrowDown"),
                        KbdShortcut("ArrowLeft"),
                        KbdShortcut("ArrowRight"),
                        KbdShortcut("Tab"),
                        KbdShortcut("Backspace"),
                        KbdShortcut("Delete")
                    ),
                    SampleSubTitle("Inline Usage"),
                    HStack().AlignItems(ItemAlign.Center).Gap(4.px()).Children(
                        TextBlock("Press").Small(),
                        KbdShortcut("Ctrl", "K"),
                        TextBlock("to open the command palette, or").Small(),
                        KbdShortcut("Escape"),
                        TextBlock("to dismiss it.").Small()
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
