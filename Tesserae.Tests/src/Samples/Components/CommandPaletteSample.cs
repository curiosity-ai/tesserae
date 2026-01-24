using System.Collections.Generic;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 31, Icon = UIcons.Search)]
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
                    SampleTitle("Command Palette"),
                    TextBlock("Use the button below or press Cmd/Ctrl + K to open the palette.").Small().Muted(),
                    openButton,
                    TextBlock("Try navigating with arrow keys, Enter, Esc, and Backspace for nested items.").Small().Muted().PT(12)
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
