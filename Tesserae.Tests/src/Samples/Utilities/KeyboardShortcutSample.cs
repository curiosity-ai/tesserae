using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 11, Icon = UIcons.Keyboard)]
    public class KeyboardShortcutSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public KeyboardShortcutSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(KeyboardShortcutSample), UIcons.Keyboard, "Render keyboard shortcuts as styled key chips")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("KeyboardShortcut renders one or more key names as styled chips that look like physical keyboard keys. It automatically adapts modifier key labels to the current OS (⌘ on macOS, Ctrl on Windows/Linux) and handles special keys like Enter (↵), Escape, and arrow keys."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use KeyboardShortcut inline inside tooltips, help text, and command-palette descriptions to make shortcuts easy to scan. Keep shortcuts short — a maximum of 3 keys is recommended."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Common Shortcuts"),
                    VStack().Gap(12.px()).Children(
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Search").W(120.px()).Small(),
                            KeyboardShortcut("Ctrl", "K")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Save").W(120.px()).Small(),
                            KeyboardShortcut("Ctrl", "S")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Undo").W(120.px()).Small(),
                            KeyboardShortcut("Ctrl", "Z")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Redo").W(120.px()).Small(),
                            KeyboardShortcut("Ctrl", "Shift", "Z")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Copy").W(120.px()).Small(),
                            KeyboardShortcut("Ctrl", "C")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Select all").W(120.px()).Small(),
                            KeyboardShortcut("Ctrl", "A")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Close / Cancel").W(120.px()).Small(),
                            KeyboardShortcut("Escape")
                        ),
                        HStack().AlignItems(ItemAlign.Center).Gap(16.px()).Children(
                            TextBlock("Confirm").W(120.px()).Small(),
                            KeyboardShortcut("Enter")
                        )
                    ),
                    SampleSubTitle("Special Keys"),
                    HStack().Gap(16.px()).AlignItems(ItemAlign.Center).Children(
                        KeyboardShortcut("ArrowUp"),
                        KeyboardShortcut("ArrowDown"),
                        KeyboardShortcut("ArrowLeft"),
                        KeyboardShortcut("ArrowRight"),
                        KeyboardShortcut("Tab"),
                        KeyboardShortcut("Backspace"),
                        KeyboardShortcut("Delete")
                    ),
                    SampleSubTitle("Inline Usage"),
                    HStack().AlignItems(ItemAlign.Center).Gap(4.px()).Children(
                        TextBlock("Press").Small(),
                        KeyboardShortcut("Ctrl", "K"),
                        TextBlock("to open the command palette, or").Small(),
                        KeyboardShortcut("Escape"),
                        TextBlock("to dismiss it.").Small()
                    )
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
