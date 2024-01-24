using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using H5.Core;
using Tesserae;
using Tesserae.Tests.Samples;
using Tesserae.Tests;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    internal static class App
    {
        private static void Main()
        {
            document.body.style.overflow = "hidden";

            var allSidebarItems     = new List<ISidebarItem>();
            var sampleToSidebarItem = new Dictionary<Sample, ISidebarItem>();

            void SelectSidebar(ISidebarItem toSelect)
            {
                allSidebarItems.ForEach(i => i.IsSelected = i == toSelect);
            }

            var currentPage = new SettableObservable<Sample>(null);

            currentPage.Observe(selected =>
            {
                if (selected is object && sampleToSidebarItem.TryGetValue(selected, out var item))
                {
                    SelectSidebar(item);
                }
            });

            var sidebar = Sidebar();

            sidebar.AddHeader(new SidebarText("tesserae", "TSS", textSize: TextSize.Large, textWeight: TextWeight.Bold).PT(16).PB(16).PL(12));

            var pageContent = HStack().Children(sidebar.HS(), DeferSync(currentPage, page => page is null ? (IComponent)CenteredCardWithBackground(TextBlock("Select an item")) : VStack().S().ScrollY().Children(page.ContentGenerator().WS())).HS().W(1).Grow()).S();

            document.body.appendChild(pageContent.Render());

            //Important: Reflection will only properly work here if reflection metadata is emitted inline with the javascript, instead of in a separate .meta.js file
            //           i.e. in the h5.json file, we need:      "reflection": { "disabled": false, "target":  "inline" },

            var samples = typeof(ISample).Assembly.GetTypes().Where(t => typeof(ISample).IsAssignableFrom(t) && !t.IsInterface)
               .Select(sampleType =>
                {
                    var    sg    = sampleType.GetCustomAttributes(typeof(SampleDetailsAttribute), true).FirstOrDefault() as SampleDetailsAttribute;
                    var    group = sg is object ? sg.Group : "Others";
                    int    order = sg is object ? sg.Order : 0;
                    UIcons icon  = sg is object ? sg.Icon : UIcons.Circle;
                    return new Sample(sampleType.Name, sampleType.Name.Replace("Sample", ""), group, order, icon, () => Activator.CreateInstance(sampleType) as IComponent);
                })
               .ToDictionary(s => s.Name, s => s);

            sidebar.AddHeader(new SidebarButton(Emoji.House, "Source Code", new SidebarCommand(UIcons.ArrowUpRightFromSquare).Tooltip("Open repository on GitHub")
                   .OnClick(() => window.open("https://github.com/curiosity-ai/tesserae", "_blank")))
               .CommandsAlwaysVisible()
               .OnOpenIconClick(() => Toast().Success("You clicked on the icon")));

            var openClose = new SidebarCommand(UIcons.AngleLeft).Tooltip("Close Sidebar");

            openClose.OnClick(() =>
            {
                sidebar.Toggle();

                if (sidebar.IsClosed)
                {
                    openClose.SetIcon(UIcons.AngleRight).Tooltip("Open Sidebar");
                }
                else
                {
                    openClose.SetIcon(UIcons.AngleLeft).Tooltip("Close Sidebar");
                }
            });

            var reorderSidebarToggle = new SidebarCommand(UIcons.ArrowsCross);

            if (sidebar.IsInReorderMode)
            {
                reorderSidebarToggle.SetIcon(UIcons.AppsSort, weight: UIconsWeight.Solid).Tooltip("Stop reordering Sidebar");
            }
            else
            {
                reorderSidebarToggle.SetIcon(UIcons.AppsSort, weight: UIconsWeight.Regular).Tooltip("Reoder Sidebar");
            }

            reorderSidebarToggle.OnClick(() =>
            {

                sidebar.IsInReorderMode = !sidebar.IsInReorderMode;

                if (sidebar.IsInReorderMode)
                {
                    reorderSidebarToggle.SetIcon(UIcons.AppsSort, weight: UIconsWeight.Solid).Tooltip("Stop reordering Sidebar");
                }
                else
                {
                    reorderSidebarToggle.SetIcon(UIcons.AppsSort, weight: UIconsWeight.Regular).Tooltip("Reoder Sidebar");
                }
                sidebar.Refresh();

            });


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

            var commands = new SidebarCommands(lightDark, toast, pizza, cheese);


            var fireworks = new SidebarCommand(Emoji.ConfettiBall).Tooltip("Confetti !").OnClick(() => Toast().Success("🎊"));
            var happy     = new SidebarCommand(Emoji.Smile).Tooltip("I like this !").OnClick(() => Toast().Success("Thanks for your feedback"));
            var sad       = new SidebarCommand(Emoji.Disappointed).Tooltip("I don't like this!").OnClick(() => Toast().Success("Thanks for your feedback"));

            var dotsMenu = new SidebarCommand(UIcons.MenuDots).OnClickMenu(() => new ISidebarItem[]
            {
                new SidebarButton(UIcons.User, "Manage Account"),
                new SidebarButton(UIcons.Settings, "Preferences"),
                new SidebarButton(UIcons.Trash, "Delete Account"),
                new SidebarCommands(new SidebarCommand(Emoji.Smile), new SidebarCommand(Emoji.Disappointed), new SidebarCommand(Emoji.Angry)),
                new SidebarCommands(new SidebarCommand(UIcons.Plus).Primary(), new SidebarCommand(UIcons.Trash).Danger()).AlignEnd(),
                new SidebarButton(UIcons.SignOutAlt, "Sign Out"),
            });

            var commandsEndAligned = new SidebarCommands(fireworks, dotsMenu).AlignEnd();

            var commandSidebarconfig = new SidebarCommands(openClose, reorderSidebarToggle);


            sidebar.AddFooter(new SidebarNav(Emoji.MailboxWithNoMail, "Empty Nav", true).ShowDotIfEmpty().OnOpenIconClick((e, m) => Toast().Success("You clicked on the icon!")));

            sidebar.AddFooter(commands);
            sidebar.AddFooter(commandsEndAligned);
            sidebar.AddFooter(commandSidebarconfig);

            sidebar.AddFooter(new SidebarButton(
                new ImageIcon("https://curiosity.ai/media/cat-color-square-64.png"),
                "By Curiosity",
                new SidebarBadge("+3").Foreground(Theme.Primary.Foreground).Background(Theme.Primary.Background),
//                new SidebarCommand("+3", Theme.Primary.Background, Theme.Primary.Foreground),
                new SidebarCommand(UIcons.ArrowUpRightFromSquare).OnClick(() => window.open("https://github.com/curiosity-ai/tesserae", "_blank"))).Tooltip("Made with ❤ by Curiosity").OnClick(() => window.open("https://curiosity.ai", "_blank")));


            var groupIndex = 0;

            foreach (var group in samples.Values.GroupBy(s => s.Group))
            {
                var groupKey = group.Key + groupIndex++;

                var nav = new SidebarNav(UIcons.Box, group.Key, false).OnClick(n => n.Toggle());
                nav.Identifier = groupKey;
                allSidebarItems.Add(nav);
                sidebar.AddContent(nav);

                var itemIndex = 0;

                foreach (var item in group.OrderBy(s => s.Order).ThenBy(s => s.Name.ToLower()))
                {
                    var sidebarItem = new SidebarButton(item.Icon, item.Name, new SidebarCommand(UIcons.SquareTerminal).Tooltip("Show sample code").OnClick(() => SamplesHelper.ShowSampleCode(item.Name)),
                        new SidebarCommand(UIcons.ArrowUpRightFromSquare).Tooltip("Open in new tab").OnClick(() => window.open($"#/view/{item.Name}", "_blank")));

                    sidebarItem.OnClick(() =>
                    {
                        Router.Push($"#/view/{item.Name}");
                        currentPage.Value = item;
                    });

                    sidebarItem.GroupIdentifier = groupKey;
                    sidebarItem.Identifier      = item.Name + itemIndex++;

                    nav.Add(sidebarItem);
                    allSidebarItems.Add(sidebarItem);
                    sampleToSidebarItem[item] = sidebarItem;
                }
            }

            Router.Register("home", "/", _ => currentPage.Value = null);


            // We'll render the content in a DeferedComponent that updates itself whenever the "currentPage" observable's value changes - these changes will be triggered by the routing configured below
            var documentTitleBase = document.title;

            foreach (var kv in samples)
            {
                Router.Register($"#/view/{kv.Key}", _ => currentPage.Value = kv.Value);
            }

            Router.Initialize();
            Router.Refresh(onDone: Router.ForceMatchCurrent); // We need to forcibly match the route at first loading since we want the just-registered routes to be matched against the current URL without us *changing* that URL
        }

        private static BackgroundArea CenteredCardWithBackground(IComponent content)
        {
            var card = Card(content).NoAnimation().Padding(32.px());
            card.Render().style.maxHeight = "calc(100% - 32px)";
            return BackgroundArea(card).S();
        }
    }
}