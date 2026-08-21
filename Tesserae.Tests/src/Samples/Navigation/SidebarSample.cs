using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Navigation, Order = 10, Icon = UIcons.Sidebar)]
    public class SidebarSample : IComponent, ISample
    {
        private const string LOGO       = "/assets/img/curiosity-logo.svg";
        private const string SHORT_NAME = "Curiosity";
        private const string NAME       = "Technical Support";
        private const string LONG_NAME  = "Technical Support Workspace EMEA";

        /// <summary>What a product's own stylesheet does to move the commands in from the edge of the row.</summary>
        private const string INSET_SKIN = "tss-sample-commands-inset";

        private readonly IComponent _content;

        public SidebarSample()
        {
            var sidebar = Sidebar();

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
                new ImageIcon(LOGO),
                "By Curiosity",
                new SidebarBadge("+3").Foreground(Theme.Primary.Foreground).Background(Theme.Primary.Background),
                new SidebarCommand("https://github.com/curiosity-ai/tesserae", UIcons.ArrowUpRightFromSquare)).Tooltip("Made with ❤ by Curiosity"));


            var workspaceName = new SettableObservable<string>(NAME);
            var commandCount  = new SettableObservable<int>(2);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarSample), UIcons.Apps, "A sidebar navigation component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A fully featured Sidebar with Search, Navigation, Buttons, and Separators. The header shows a rounded (pill) primary button and a rounded search box, each with the shortcut that reaches it at its far end — enable the pill with .Rounded() and the shortcut with .SetKeyboardShortcut(). Home and Profile carry keys too, but add .ShortcutOnlyOnHover(), so their chips wait for the pointer instead of standing in a column beside the labels."))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        SplitView().WS().H(800).LeftIsSmaller(400.px()).Resizable()
                                   .Left(sidebar.S())
                                   .Right(CenteredCardWithBackground(Message("Your application content goes here")))
               )).SetTitle("Usage"),

                    Card(VStack().WS().Children(
                        TextBlock("A SidebarCommand is drawn over its row rather than in it, and only while the pointer is on that row — so a row costs its label nothing at rest, and gives up exactly the room its own commands take while they are on screen. .CommandsAlwaysVisible() makes the commands part of the rail's chrome instead: they are drawn at all times, so the room is given up at all times and the label truncates before them."),
                        TextBlock("The row writes how wide its strip of commands is and the stylesheet reads it back, so the room is right for one command and for three, and it is kept for as long as the strip is drawn — while the pointer is on the row, while the row is selected, or always. A skin that moves the strip further in from the edge sets --tss-sidebar-commands-inset on the row, and the label's reservation follows it."),
                        TextBlock("The same long name in four rails: commands on hover and commands always drawn, each with the default inset and with a skin that moves the strip 12px in from the edge. Hover the top two — a row keeps room for its commands for exactly as long as they are drawn, so the label steps aside as they appear instead of being covered, and it steps aside by however far the strip is inset.").Secondary().PT(8),
                        HStack().WS().PT(8).Children(
                            Rail("On hover",                  Brand("hover", LONG_NAME, 2)),
                            Rail("On hover, inset 12px",      Brand("hover-inset", LONG_NAME, 2).Class(INSET_SKIN))),
                        HStack().WS().PT(12).Children(
                            Rail("Always visible",             Brand("always", LONG_NAME, 2).CommandsAlwaysVisible()),
                            Rail("Always visible, inset 12px", Brand("always-inset", LONG_NAME, 2).CommandsAlwaysVisible().Class(INSET_SKIN))))).SetTitle("Commands on a row, and the label beside them"),

                    Card(VStack().WS().Children(
                        TextBlock("The rail a workspace app puts together: a logo, the workspace's name, and the rail's own controls as commands beside it. The brand carries the chat search and the way out, and the history sits under it. Change the name and the number of commands to watch the room the row keeps for them.").Secondary(),
                        HStack().WS().PT(8).Children(
                            ChoiceGroup("Workspace name").Choices(
                                Choice(SHORT_NAME).OnSelected(_ => workspaceName.Value = SHORT_NAME),
                                Choice(NAME).Selected().OnSelected(_ => workspaceName.Value = NAME),
                                Choice(LONG_NAME).OnSelected(_ => workspaceName.Value = LONG_NAME)),
                            ChoiceGroup("Commands").PL(32).Choices(
                                Choice("One").OnSelected(_ => commandCount.Value            = 1),
                                Choice("Two").Selected().OnSelected(_ => commandCount.Value = 2),
                                Choice("Three").OnSelected(_ => commandCount.Value          = 3))),
                        DeferSync(workspaceName, commandCount, (n, c) => WorkspaceRail(n, c)).PT(8))).SetTitle("The workspace rail")))
               .SeeAlso(typeof(SidebarShiftSample), typeof(SidebarSeparatorSample), typeof(SidenavSample), typeof(NavbarSample), typeof(MenuSample), typeof(BreadcrumbSample));
        }

        /// <summary>One rail with nothing in it but the brand row, so the row is the only thing to read.</summary>
        private static IComponent Rail(string title, SidebarButton brand) => VStack().PR(16).Children(
            TextBlock(title).XSmall().Secondary().PB(4),
            Sidebar().AddHeader(brand).H(96.px()));

        private static SidebarButton Brand(string id, string workspaceName, int commandCount) =>
            new SidebarButton($"brand-{id}", new ImageIcon(LOGO), workspaceName, CommandsFor(commandCount));

        /// <summary>
        /// A fresh set every time: a command renders in one place, so two rails cannot share one.
        /// </summary>
        private static SidebarCommand[] CommandsFor(int count)
        {
            var search = new SidebarCommand(UIcons.Search).Tooltip("Search your chats").OnClick(() => Toast().Information("Search your chats"));
            var leave  = new SidebarCommand(UIcons.AngleLeft).Tooltip("Leave the assistant").OnClick(() => Toast().Information("Leave the assistant"));
            var more   = new SidebarCommand(UIcons.MenuDots).Tooltip("More").OnClick(() => Toast().Information("More"));

            if (count <= 1) return new[] { search };
            if (count == 2) return new[] { search, leave };
            return new[] { search, leave, more };
        }

        private static IComponent WorkspaceRail(string workspaceName, int commandCount)
        {
            var rail = Sidebar();

            rail.AddHeader(Brand("workspace", workspaceName, commandCount).CommandsAlwaysVisible());

            rail.AddHeader(new SidebarButton("new-chat", UIcons.Edit, "New chat")
               .Primary()
               .Rounded()
               .OnClick(() => Toast().Success("New chat")));

            rail.AddContent(new SidebarSeparator("today", "Today"));
            rail.AddContent(new SidebarButton("chat-1", UIcons.Comment, "Phone will not power on"));
            rail.AddContent(new SidebarButton("chat-2", UIcons.Comment, "Battery drains overnight"));

            rail.AddContent(new SidebarSeparator("yesterday", "Yesterday"));
            rail.AddContent(new SidebarButton("chat-3", UIcons.Comment, "Screen flickers after update"));

            return rail.H(320.px());
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
