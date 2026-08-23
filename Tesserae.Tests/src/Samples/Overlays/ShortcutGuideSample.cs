using Tesserae;
using Tesserae.Tests;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Overlays, Order = 100, Icon = UIcons.KeyboardFinger, Description = "The app's keyboard shortcuts, listed")]
    public class ShortcutGuideSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ShortcutGuideSample()
        {
            var container = Raw();
            var lastFired = TextBlock("Nothing pressed yet").Small();

            var guide = SampleGuide()
               .Var(out var self)
               .Section("Try it here")
                   .Shortcut("Open this guide", "Ctrl", "/").OnPressed(() => self.Toggle())
                   .Shortcut("Say hello",       "Ctrl", "Shift", "H").OnPressed(() => lastFired.Text = "Ctrl+Shift+H — hello!")
                   .Shortcut("Clear",           "Escape").OnPressed(() => lastFired.Text = "Nothing pressed yet");

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ShortcutGuideSample), UIcons.KeyboardFinger, "A modal listing the app's keyboard shortcuts")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ShortcutGuide is the \"keyboard shortcuts\" sheet an application opens from its help menu or with Ctrl+/. It lists shortcuts in titled sections — a description on the left, the keys on the right as KeyboardShortcut chips — and takes the same key names KeyboardShortcut.Matches tests, so a shortcut is declared once and what is listed cannot drift from what is bound."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Group shortcuts by where they apply (\"General\", \"In chats\") and describe what each one does, not what it is called in the code. Keep the list to the shortcuts a user can actually reach from the surface they are on. Give an entry an action with OnPressed and route the app's keydown through Handle to have the guide answer the presses it advertises; leave the action off for a key some other component already handles."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    Button("Open Shortcut Guide").OnClick((s, e) => SampleGuide().Show()),
                    Button("Open a narrow one").OnClick((s, e) => SampleGuide().W(360.px()).SetTitle("Shortcuts").Show()))).SetTitle("Usage"),
                    Card(VStack().WS().Children(
                    TextBlock("A guide whose entries carry actions can answer them too: this text box routes its keydown through Handle."),
                    TextBox().SetPlaceholder("Click here, then press Ctrl+/, Ctrl+Shift+H or Escape").WS()
                       .OnKeyDown((s, e) => { if (guide.Handle(e)) StopEvent(e); }),
                    lastFired)).SetTitle("Handling the shortcuts")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    Button("Show Guide Below").OnClick((s, e) => container.Content(SampleGuide().ShowEmbedded())),
                    container)).SetTitle("Embedded Guide")))
               .SeeAlso(typeof(KeyboardShortcutSample), typeof(CommandPaletteSample), typeof(ModalSample), typeof(TutorialModalSample), typeof(SidebarSample));
        }

        private static ShortcutGuide SampleGuide()
        {
            return ShortcutGuide()
               .Section("General")
                   .Shortcut("Quick chat or search", "Ctrl", "K")
                   .Shortcut("Incognito chat",       "Ctrl", "Shift", "I")
                   .Shortcut("Toggle sidebar",       "Ctrl", ".")
                   .Shortcut("Keyboard shortcuts",   "Ctrl", "/")
                   .Shortcut("Settings",             "Ctrl", "Shift", ",")
               .Section("In chats")
                   .Shortcut("Send message",         "Enter")
                   .Shortcut("New line in message",  "Shift", "Enter")
                   .Shortcut("Toggle thinking",      "Ctrl", "Shift", "E")
                   .Shortcut("Open model menu",      "Ctrl", "Shift", ".")
                   .Shortcut("Upload file",          "Ctrl", "U")
                   .Shortcut("Stop the response",    "Escape");
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
