using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.MenuDots)]
    public class SidebarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SidebarSample()
        {
            var sidebar = Sidebar();

            // A rounded, primary "pill" button (see .Rounded()), matching the rounded search box below.
            var newDocument = new SidebarButton("new-doc", UIcons.Plus, "New document")
                .Primary()
                .Rounded()
                .OnClick(() => Toast().Success("New document"));

            sidebar.AddHeader(newDocument);

            // A rounded search box with a keyboard shortcut chip (⌘K / Ctrl+K) that focuses it.
            var searchBox = new SidebarSearchBox("search", "Search docs, parts, records...")
                .Rounded()
                .SetKeyboardShortcut("Ctrl", "K")
                .OnSearch((term) => sidebar.Search(term));

            sidebar.AddHeader(searchBox);

            sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home"));
            sidebar.AddContent(new SidebarButton("profile", UIcons.User, "Profile"));

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


            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SidebarSample), UIcons.Apps, "A sidebar navigation component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A fully featured Sidebar with Search, Navigation, Buttons, and Separators. The header shows a rounded (pill) primary button and a rounded search box with a keyboard shortcut (⌘K / Ctrl+K) — enable both with .Rounded()."))).SetTitle("Overview"),
                    Card(VStack().WS().Children(
                        SplitView().WS().H(800).LeftIsSmaller(400.px()).Resizable()
                                   .Left(sidebar.S())
                                   .Right(CenteredCardWithBackground(Message("Your application content goes here")))
               )).SetTitle("Usage")));
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
