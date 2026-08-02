using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Transpose.Core;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 31, Icon = UIcons.Command)]
    public class CommandPaletteSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CommandPaletteSample()
        {
            var stack = SectionStack().Secondary();
            var palette = new CommandPalette(stack, BuildActions());

            var openButton = Button("Open Command Palette")
               .Primary()
               .OnClick(() => palette.Open());

            //A second palette that searches instead of listing: the rows are the host's own, so a result is
            //drawn as the OmniResult it would be anywhere else rather than squeezed into a name and a subtitle.
            var searchPalette = new CommandPalette(stack)
            {
                Placeholder          = "Search files",
                EnableGlobalShortcut = false,
            };

            searchPalette.OnSearch(query => Task.FromResult(SearchFiles(query, searchPalette)));

            var openSearchButton = Button("Open Search Palette")
               .OnClick(() => searchPalette.Open());

            _content = stack
               .SampleTitle(typeof(CommandPaletteSample), UIcons.Keyboard, "A command palette utility")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("CommandPalette provides a fast and efficient way for users to navigate an application and trigger commands using only their keyboard. Inspired by modern editors and tools, it allows users to search through a list of actions and execute them with a single keystroke."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Register all major application actions in the CommandPalette. Use intuitive shortcuts and keywords to make actions easy to discover. Organize related actions into sections and utilize hierarchies for a cleaner interface. Ensure that common global actions are always easily accessible via the palette."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use the button below or press Cmd/Ctrl + K to open the palette.").Small().Secondary().PB(8),
                    openButton,
                    TextBlock("Try navigating with arrow keys, Enter, Esc, and Backspace for nested items.").Small().Secondary().PT(12)
               )).SetTitle("Usage")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("SetResults puts rows of your own above the actions, and OnSearch refreshes them as the query changes — so a palette can answer a question rather than only list commands. The rows here are OmniResults, the same component a search page draws, and the last one is the way out to the full result list.").Small().Secondary().PB(8),
                    openSearchButton,
                    TextBlock("Type to filter, walk the rows with the arrow keys, and press Enter on one.").Small().Secondary().PT(12)
               )).SetTitle("Searching, with results of your own")))
               .SeeAlso(typeof(KeyboardShortcutSample), typeof(OmniBoxSample), typeof(SearchBoxSample), typeof(MenuSample), typeof(ContextMenuSample));
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

        private static readonly (string Name, string Extension, string Color, string Owner)[] Files =
        {
            ("BRK-SEN-447 calibration procedure", "PDF",  "#ef4444", "Marie Lang"),
            ("Q3 line review",                    "PPTX", "#f97316", "Pius Neuhaus"),
            ("Sensor drift analysis",             "XLSX", "#16a34a", "Marie Lang"),
            ("brake-calibration-log",             "TXT",  "#94a3b8", "Ana Ferreira"),
            ("Supplier agreement 2024",           "DOCX", "#2563eb", "Pius Neuhaus"),
        };

        private static IEnumerable<CommandPaletteResult> SearchFiles(string query, CommandPalette palette)
        {
            var matches = Files.Where(f => string.IsNullOrEmpty(query) || f.Name.ToLower().Contains(query.ToLower())).Take(4).ToList();

            var rows = matches.Select(f => new CommandPaletteResult(
                OmniResult(f.Name, $"{f.Name}.{f.Extension.ToLower()}")
                   .SetIcon(f.Extension, f.Color)
                   .SetSource("#0061d5", "Box")
                   .SetFooterEntries(InlineLabel(f.Owner).SetIcon(UIcons.User))
                   .Highlight(query),
                () => Toast().Success($"Opened {f.Name}"))
            {
                Section = "Results",
            }).ToList();

            //The way out of the palette: what the user typed, on the page that can show all of it.
            rows.Add(new CommandPaletteResult(
                HStack().AlignItemsCenter().Gap(8.px()).PL(12).PT(8).PB(8).Children(
                    Icon(UIcons.Search),
                    TextBlock(string.IsNullOrEmpty(query) ? "Show all files" : $"Show all results for \"{query}\"").Small()),
                () => Toast().Information("Would navigate to the search page"))
            {
                Section = "Results",
            });

            return rows;
        }

        public HTMLElement Render() => _content.Render();
    }
}
