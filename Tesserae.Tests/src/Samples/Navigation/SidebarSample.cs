using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 10, Icon = UIcons.Sidebar, Description = "The app shell's collapsible side navigation")]
    public class SidebarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarSample()
        {
            // Sortable: every item in the middle section can be dragged into a new order, and the
            // sidebar reports the new order through OnSortingChanged. Nothing is remembered unless the
            // app remembers it — see the wiring below the items, and the "Remembering the order" card.
            var sidebar = Sidebar(sortable: true);

            // A rounded, primary "pill" button (see .Rounded()), matching the rounded search box below,
            // with the shortcut that presses it shown at its far end (see .SetKeyboardShortcut()).
            var newDocument = new SidebarButton("new-doc", UIcons.Plus, "New document")
                .Primary()
                .Rounded()
                .SetKeyboardShortcut("Ctrl", "Shift", "O")
                .OnClick(() => Toast().Success("New document"));

            sidebar.AddHeader(newDocument);

            // A rounded search box with a keyboard shortcut chip (⌘K / Ctrl+K) that focuses it.
            var searchBox = new SidebarSearchBox("search", "Search docs, parts, records...")
                .Rounded()
                .SetKeyboardShortcut("Ctrl", "K")
                .OnSearch((term) => sidebar.Search(term));

            sidebar.AddHeader(searchBox);

            // Rows that each carry a key: every chip at once would be a column of noise beside the labels, so
            // they wait for the pointer (or for keyboard focus) — see .ShortcutOnlyOnHover().
            sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home")
                .SetKeyboardShortcut("Ctrl", "Shift", "H")
                .ShortcutOnlyOnHover()
                .OnClick(() => Toast().Success("Home")));

            sidebar.AddContent(new SidebarButton("profile", UIcons.User, "Profile")
                .SetKeyboardShortcut("Ctrl", "Shift", "P")
                .ShortcutOnlyOnHover()
                .OnClick(() => Toast().Success("Profile")));

            sidebar.AddContent(new SidebarSeparator("sep1", "Grouping"));

            var tabs = new SidebarPivot("tabs")
                .Add("tab1", SegmentTitle("Tab 1", UIcons.Rocket),
                    new SidebarButton("t1_btn1", UIcons.Rocket, "Launch"),
                    new SidebarButton("t1_btn2", UIcons.Rocket, "Launch 2"))
                .Add("tab2", SegmentTitle("Tab 2", UIcons.Plane),
                    new SidebarButton("t2_btn1", UIcons.Globe, "World"),
                    new SidebarButton("t2_btn2", UIcons.Globe, "World 2"));

            sidebar.AddContent(tabs);

            var settingsNav = new SidebarNav("settings", UIcons.Settings, "Settings", true);


            settingsNav.Add(new SidebarButton("general", UIcons.Settings, "General"));
            settingsNav.Add(new SidebarButton("security", UIcons.Lock, "Security"));
            settingsNav.Add(new SidebarButton("privacy", UIcons.Eye, "Privacy"));

            sidebar.AddContent(settingsNav);

            sidebar.AddContent(new SidebarSeparator("sep2"));

            sidebar.AddContent(new SidebarButton("help", UIcons.Question, "Help"));
            sidebar.AddContent(new SidebarButton("link", "https://bing.com", UIcons.Link, "External Link"));

            // --- Moved from App.cs ---

            var lightDark = new SidebarCommand(UIcons.Sun).Tooltip("Light Mode");

            lightDark.OnClick(() =>
            {
                if (Theme.IsDark)
                {
                    Theme.Light();
                    lightDark.SetIcon(UIcons.Sun).Tooltip("Light Mode");
                }
                else
                {
                    Theme.Dark();
                    lightDark.SetIcon(UIcons.Moon).Tooltip("Dark Mode");
                }
            });

            var toast  = new SidebarCommand(Emoji.Bread).Tooltip("Toast !").OnClick(() => Toast().Success("Here is your toast 🍞"));
            var pizza  = new SidebarCommand(Emoji.Pizza).Tooltip("Pizza!").OnClick(() => Toast().Success("Here is your pizza 🍕"));
            var cheese = new SidebarCommand(Emoji.Cheese).Tooltip("Cheese !").OnClick(() => Toast().Success("Here is your cheese 🧀"));

            var commands = new SidebarCommands("TOASTS", lightDark, toast, pizza, cheese);


            var fireworks = new SidebarCommand(Emoji.ConfettiBall).Tooltip("Confetti !").OnClick(() => Toast().Success("🎊"));
            var happy     = new SidebarCommand(Emoji.Smile).Tooltip("I like this !").OnClick(() => Toast().Success("Thanks for your feedback"));
            var sad       = new SidebarCommand(Emoji.Disappointed).Tooltip("I don't like this!").OnClick(() => Toast().Success("Thanks for your feedback"));

            var dotsMenu = new SidebarCommand(UIcons.MenuDots).OnClickMenu(() => new ISidebarItem[]
            {
                new SidebarButton("MANAGE_ACCOUNT", UIcons.User,     "Manage Account"),
                new SidebarButton("PREFERENCES",    UIcons.Settings, "Preferences"),
                new SidebarButton("DELETE",         UIcons.Trash,    "Delete Account"),
                new SidebarCommands("EMOTIONS", new SidebarCommand(Emoji.Smile), new SidebarCommand(Emoji.Disappointed), new SidebarCommand(Emoji.Angry)),
                new SidebarCommands("ADD_DELETE", new SidebarCommand(UIcons.Plus).Primary(), new SidebarCommand(UIcons.Trash).Danger()).AlignEnd(),
                new SidebarButton("SIGNOUT", UIcons.SignOutAlt, "Sign Out"),
            });

            var commandsEndAligned = new SidebarCommands("SETTINGS", fireworks, dotsMenu).AlignEnd();

            sidebar.AddFooter(new SidebarNav("DEEP_NAV", Emoji.EvergreenTree, "Multi-Depth Nav", true).Sortable(sortableGroup: "trees").AddRange(CreateDeepNav("root")));

            sidebar.AddFooter(new SidebarNav("EMPTY_NAV", Emoji.MailboxWithNoMail, "Empty Nav", true).OnOpenIconClick((e, m) => Toast().Success("You clicked on the icon!")));


            sidebar.AddFooter(commands);
            sidebar.AddFooter(commandsEndAligned);

            sidebar.AddFooter(new SidebarButton("CURIOSITY_REF",
                "https://curiosity.ai",
                new ImageIcon("/assets/img/curiosity-logo.svg"),
                "By Curiosity",
                new SidebarBadge("+3").Foreground(Theme.Primary.Foreground).Background(Theme.Primary.Background),
                new SidebarCommand("https://github.com/curiosity-ai/tesserae", UIcons.ArrowUpRightFromSquare)).Tooltip("Made with ❤ by Curiosity"));


            // --- Remembering a dragged order ---

            // The order the sample itself declares, captured before anything is restored, so
            // "Forget saved order" below has something to put back.
            var declaredOrder = sidebar.GetCurrentSorting();

            // LoadSorting has to run after every item is in place: it reorders what is there, and an
            // item added afterwards would land at the end regardless of what was saved. This is the one
            // thing on the page that reads browser state, so a profile someone has dragged in renders
            // this sidebar in that order; a fresh profile always shows the order declared above.
            var savedOrder = LoadSavedOrder();

            if (savedOrder is object) sidebar.LoadSorting(savedOrder);

            // A drag reports continuously, so the write is debounced: without it a single drag across
            // the sidebar writes to localStorage on every row it crosses.
            var savingTimeout = 0d;

            sidebar.OnSortingChanged(itemOrder =>
            {
                window.clearTimeout(savingTimeout);

                savingTimeout = window.setTimeout(_ =>
                {
                    SaveOrder(itemOrder);
                    Toast().Information("Sidebar order saved");
                }, 1000);
            });

            var forget = Button("Forget saved order").SetIcon(UIcons.Trash).OnClick(() =>
            {
                localStorage.removeItem(_orderKey);
                sidebar.LoadSorting(declaredOrder);
                Toast().Success("Back to the declared order");
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarSample), UIcons.Apps, "A sidebar navigation component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A fully featured Sidebar with Search, Navigation, Buttons, and Separators. The header shows a rounded (pill) primary button and a rounded search box, each with the shortcut that reaches it at its far end — enable the pill with .Rounded() and the shortcut with .SetKeyboardShortcut(). Home and Profile carry keys too, but add .ShortcutOnlyOnHover(), so their chips wait for the pointer instead of standing in a column beside the labels."))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        TextBlock("This sidebar is built with Sidebar(sortable: true), so its items can be dragged into a new order. The sidebar only reports that order — through .OnSortingChanged(itemOrder), a map of group identifier to the identifiers it now holds, in order — and remembering it is the app's job. This sample writes the map to localStorage (debounced by a second, because a drag reports on every row it crosses) and calls .LoadSorting(...) on startup to put it back. Call LoadSorting only once every item has been added: it reorders what is there, so an item added afterwards lands at the end whatever was saved. .GetCurrentSorting() reads the order out at any time, which is how the button below can restore the order this page declares."),
                        forget)).SetTitle("Remembering the order"),
                    Card(VStack().WS().Children(
                        SplitView().WS().H(800).LeftIsSmaller(400.px()).Resizable()
                                   .Left(sidebar.S())
                                   .Right(CenteredCardWithBackground(Message("Your application content goes here")))
               )).SetTitle("Usage")))
               .SeeAlso(typeof(SidebarSeparatorSample), typeof(SidenavSample), typeof(NavbarSample), typeof(MenuSample), typeof(BreadcrumbSample));
        }

        // Where this sample keeps the order the user dragged the sidebar into. An app would use one key
        // per sidebar it wants to remember.
        private const string _orderKey = "tss-sidebar-sample-order";

        /// <summary>
        /// Writes the order reported by <see cref="Sidebar.OnSortingChanged"/> to localStorage. The map
        /// is a plain JSON object of group identifier to the item identifiers it holds, in order.
        /// </summary>
        private static void SaveOrder(Dictionary<string, string[]> itemOrder)
        {
            // Built through the indexer rather than as a typed model: the shape is one entry per group,
            // and the group identifiers are only known at run time, so there is no class to name here.
            var asJsObject = new { };

            foreach (var group in itemOrder)
            {
                asJsObject[group.Key] = group.Value;
            }

            localStorage.setItem(_orderKey, es5.JSON.stringify(asJsObject));
        }

        /// <summary>
        /// Reads back what <see cref="SaveOrder"/> wrote, or null when nothing has been saved yet.
        /// </summary>
        private static Dictionary<string, string[]> LoadSavedOrder()
        {
            var json = localStorage.getItem(_orderKey);

            if (json is null) return null;

            var parsed = es5.JSON.parse(json).As<object>();

            if (parsed is null) return null;

            var itemOrder = new Dictionary<string, string[]>();

            foreach (var groupIdentifier in GetOwnPropertyNames(parsed))
            {
                itemOrder[groupIdentifier] = parsed[groupIdentifier].As<string[]>();
            }

            return itemOrder;
        }

        private static IEnumerable<ISidebarItem> CreateDeepNav(string path, int currentDepth = 0, int maxDepth = 3)
        {
            if (currentDepth < maxDepth)
            {
                Action<SidebarNav.ParentChangedEvent> HandleChange = (e)=>
                {
                    Dialog($"Move element {e.Item.OwnIdentifier} from {e.From.OwnIdentifier} to {e.To.OwnIdentifier}?").YesNo(onNo: e.Cancel);
                };
                yield return new SidebarNav($"{path}/{currentDepth + 1}.1", Emoji.DeciduousTree, $"{path}/{currentDepth + 1}.1", true).Sortable(sortableGroup: "trees").AddRange(CreateDeepNav($"{path}/{currentDepth + 1}.1", currentDepth + 1, maxDepth)).OnParentChanged(HandleChange);
                yield return new SidebarNav($"{path}/{currentDepth + 1}.2", Emoji.DeciduousTree, $"{path}/{currentDepth + 1}.2", true).Sortable(sortableGroup: "trees").AddRange(CreateDeepNav($"{path}/{currentDepth + 1}.2", currentDepth + 1, maxDepth)).OnParentChanged(HandleChange);
                yield return new SidebarNav($"{path}/{currentDepth + 1}.3", Emoji.DeciduousTree, $"{path}/{currentDepth + 1}.3", true).Sortable(sortableGroup: "trees").AddRange(CreateDeepNav($"{path}/{currentDepth + 1}.3", currentDepth + 1, maxDepth)).OnParentChanged(HandleChange);
            }
        }


        public HTMLElement Render() => _content.Render();
    }
}
