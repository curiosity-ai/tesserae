using System.Collections.Generic;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using H5.Core;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 31, Icon = UIcons.Command)]
    public class CommandPaletteSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CommandPaletteSample()
        {
            var palette = new CommandPalette(BuildActions());

            var openButton = Button("Open Command Palette")
               .Primary()
               .OnClick(() => palette.Open());

            _content = SectionStack()
               .Title(SampleHeader(nameof(CommandPaletteSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("CommandPalette provides a fast and efficient way for users to navigate an application and trigger commands using only their keyboard. Inspired by modern editors and tools, it allows users to search through a list of actions and execute them with a single keystroke.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Register all major application actions in the CommandPalette. Use intuitive shortcuts and keywords to make actions easy to discover. Organize related actions into sections and utilize hierarchies for a cleaner interface. Ensure that common global actions are always easily accessible via the palette.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Use the button below or press Cmd/Ctrl + K to open the palette.").Small().Secondary().PB(8),
                    openButton,
                    TextBlock("Try navigating with arrow keys, Enter, Esc, and Backspace for nested items.").Small().Secondary().PT(12)
               ));
        }

        private static IEnumerable<CommandPaletteAction> BuildActions()
        {
            var navigate = new CommandPaletteAction("navigation", "Navigate");
            var home = new CommandPaletteAction("home", "Go to Home")
            {
                ParentId = "navigation",
                Perform = () => Toast().Success("Home"),
            };
            var settings = new CommandPaletteAction("settings", "Open Settings")
            {
                ParentId = "navigation",
                Perform = () => Toast().Success("Settings"),
            };
            var help = new CommandPaletteAction("help", "Help Center")
            {
                Perform = () => Toast().Success("Help"),
                Shortcut = new[] { "?" },
                Keywords = "support docs",
                Icon = UIcons.CommentsQuestion,
            };
            var create = new CommandPaletteAction("new", "Create Item")
            {
                Perform = () => Toast().Success("Create"),
                Shortcut = new[] { "n" },
                Section = "Actions",
                Icon = UIcons.Plus,
            };
            var archive = new CommandPaletteAction("archive", "Archive Item")
            {
                Perform = () => Toast().Success("Archive"),
                Section = "Actions",
                Icon = UIcons.Archive,
            };

            return new[]
            {
                navigate,
                home,
                settings,
                help,
                create,
                archive,
            };
        }

        public HTMLElement Render() => _content.Render();
    }
}
