using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

            var allSidebarItems = new List<ISidebarItem>();
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
            
            var sidebar     = Sidebar();
            var pageContent = HStack().Children(sidebar.HS(), Defer(currentPage, page => page is null ? CenteredCardWithBackground(TextBlock("Select an item")).AsTask() : VStack().S().ScrollY().Children(page.ContentGenerator().WS()).AsTask()).HS().W(1).Grow()).S();

            document.body.appendChild(pageContent.Render());

            //Important: Reflection will only properly work here if reflection metadata is emitted inline with the javascript, instead of in a separate .meta.js file
            //           i.e. in the h5.json file, we need:      "reflection": { "disabled": false, "target":  "inline" },

            var samples = typeof(ISample).Assembly.GetTypes().Where(t => typeof(ISample).IsAssignableFrom(t) && !t.IsInterface)
                            .Select(sampleType =>
                            {
                                var sg = sampleType.GetCustomAttributes(typeof(SampleDetailsAttribute), true).FirstOrDefault() as SampleDetailsAttribute;
                                var group = sg is object ? sg.Group : "Others";
                                int order = sg is object ? sg.Order : 0;
                                LineAwesome icon = sg is object ? sg.Icon : LineAwesome.Circle;
                                return new Sample(sampleType.Name, sampleType.Name.Replace("Sample", ""), group, order, icon, () => Activator.CreateInstance(sampleType) as IComponent);
                            })
                            .ToDictionary(s => s.Name, s => s);

            sidebar.AddHeader(new SidebarButton(Emoji.House, "Tesserae", new SidebarCommand(LineAwesome.ExternalLinkAlt).Tooltip("View on GitHub").OnClick(() => window.open("https://github.com/curiosity-ai/tesserae","_blank"))).CommandsAlwaysVisible());

            var openClose = new SidebarButton(LineAwesome.ChevronLeft, "").Tooltip("Close Sidebar");

            openClose.OnClick(() =>
            {
                sidebar.Toggle();

                if (sidebar.IsClosed)
                {
                    openClose.SetIcon(LineAwesome.ChevronRight).Tooltip("Open Sidebar");
                }
                else
                {
                    openClose.SetIcon(LineAwesome.ChevronLeft).Tooltip("Close Sidebar");
                }
            });


            var lightDark = new SidebarCommand(LineAwesome.Sun).Tooltip("Light Mode");

            lightDark.OnClick(() =>
            {
                if (Theme.IsDark)
                {
                    Theme.Light();
                    lightDark.SetIcon(LineAwesome.Sun).Tooltip("Light Mode");
                }
                else
                {
                    Theme.Dark();
                    lightDark.SetIcon(LineAwesome.Moon).Tooltip("Dark Mode");
                }
            });

            var toast  = new SidebarCommand(Emoji.Bread).Tooltip("Toast !").OnClick(() => Toast().Success("Here is your toast 🍞"));
            var pizza  = new SidebarCommand(Emoji.Pizza).Tooltip("Pizza!").OnClick(() => Toast().Success("Here is your pizza 🍕"));
            var cheese = new SidebarCommand(Emoji.Cheese).Tooltip("Cheese !").OnClick(() => Toast().Success("Here is your cheese 🧀"));

            var commands = new SidebarCommands(lightDark, toast, pizza, cheese);


            var fireworks = new SidebarCommand(Emoji.ConfettiBall).Tooltip("Confetti !").OnClick(() => Toast().Success("🎊"));
            var happy = new SidebarCommand(Emoji.Smile).Tooltip("I like this !").OnClick(() => Toast().Success("Thanks for your feedback"));
            var sad   = new SidebarCommand(Emoji.Disappointed).Tooltip("I don't like this!").OnClick(() => Toast().Success("Thanks for your feedback"));

            var dotsMenu = new SidebarCommand(LineAwesome.EllipsisH).OnClickMenu(() => new ISidebarItem[] 
            {
                new SidebarButton(LineAwesome.User, "Manage Account"),
                new SidebarButton(LineAwesome.Cog, "Preferences"),
                new SidebarButton(LineAwesome.TrashAlt, "Delete Account"),
                new SidebarCommands(new SidebarCommand(Emoji.Smile), new SidebarCommand(Emoji.Disappointed), new SidebarCommand(Emoji.Angry)),
                new SidebarCommands(new SidebarCommand(LineAwesome.Plus).Primary(), new SidebarCommand(LineAwesome.TrashAlt).Danger()).AlignEnd(),
                new SidebarButton(LineAwesome.SignOutAlt, "Sign Out"),
            });

            var commandsEndAligned = new SidebarCommands(fireworks, dotsMenu).AlignEnd();

            sidebar.AddFooter(new SidebarNav(Emoji.MailboxWithNoMail, "Empty Nav", true).ShowDotIfEmpty().OnOpenIconClick((e,m) => Toast().Success("You clicked on the icon!")));

            sidebar.AddFooter(commands);
            sidebar.AddFooter(commandsEndAligned);
            sidebar.AddFooter(openClose);
            sidebar.AddFooter(new SidebarButton(new ImageIcon("https://curiosity.ai/media/cat-color-square-64.png"), "By Curiosity").Tooltip("Made with ❤ by Curiosity").OnClick(() => window.open("https://curiosity.ai", "_blank")));


            foreach (var group in samples.Values.GroupBy(s => s.Group))
            {
                var nav = new SidebarNav(LineAwesome.Box, group.Key, false).OnClick(n => n.Toggle());
                allSidebarItems.Add(nav);
                sidebar.AddContent(nav);

                foreach (var item in group.OrderBy(s => s.Order).ThenBy(s => s.Name.ToLower()))
                {
                    var sidebarItem = new SidebarButton(item.Icon, item.Name, new SidebarCommand(LineAwesome.Code).Tooltip("Show sample code").OnClick(() => SamplesHelper.ShowSampleCode(item.Name)),
                                                                              new SidebarCommand(LineAwesome.ExternalLinkAlt).Tooltip("Open in new tab").OnClick(() => window.open($"#/view/{item.Name}","_blank")));

                    sidebarItem.OnClick(() =>
                    {
                        Router.Push($"#/view/{item.Name}");
                        currentPage.Value = item;
                    });

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


        private static IComponent MainNav(Dictionary<string, Nav.NavLink> links)
        {
            return Stack().Padding(16.px()).NoShrink().MinHeightStretch()
               .Children(TextBlock("Tesserae UI Toolkit").MediumPlus().SemiBold().AlignCenter(),
                    HStack().JustifyContent(ItemJustify.Center).PT(10.px()).PB(10.px()).Children(TextBlock("by").XSmall().PR(4.px()), Link("https://www.curiosity.ai", TextBlock("curiosity.ai").XSmall().Primary()).PR(4.px()), TextBlock("built with").XSmall().PR(4.px()), Link("https://h5.rocks", TextBlock("h5 🚀").XSmall().Primary())),
                    Nav().InlineContent(Label("Theme").Inline().SetContent(Toggle("Light", "Dark").Checked().OnChange((t, e) =>
                        {
                            if (t.IsChecked) { Theme.Light(); }
                            else { Theme.Dark(); }
                        })))
                       .Links(NavLink("Basic Inputs").Expanded()
                               .SmallPlus()
                               .SemiBold()
                               .Links(links["Button"],
                                    links["CheckBox"],
                                    links["ChoiceGroup"],
                                    links["Slider"],
                                    links["Dropdown"],
                                    links["Label"],
                                    links["EditableLabel"],
                                    links["TextBox"],
                                    links["SearchBox"],
                                    links["Toggle"],
                                    links["Picker"],
                                    links["ColorPicker"],
                                    links["DatePicker"],
                                    links["DateTimePicker"],
                                    links["GridPicker"]
                                    ),
                            NavLink("Progress").Expanded()
                               .SmallPlus()
                               .SemiBold()
                               .Links(links["Spinner"],
                                    links["ProgressIndicator"]),
                            NavLink("Surfaces").Expanded()
                               .SmallPlus()
                               .SemiBold()
                               .Links(links["Dialog"],
                                    links["Modal"],
                                    links["TutorialModal"],
                                    links["Panel"],
                                    links["ContextMenu"]),
                            NavLink("Utilities").Expanded()
                               .SmallPlus()
                               .SemiBold()
                               .Links(links["Layer"],
                                    links["Stack"],
                                    links["Masonry"],
                                    links["HorizontalSeparator"],
                                    links["SectionStack"],
                                    links["TextBlock"],
                                    links["Validator"],
                                    links["Breadcrumb"],
                                    links["TextBreadcrumbs"],
                                    links["OverflowSet"],
                                    links["Pivot"],
                                    links["Defer"],
                                    links["Toast"],
                                    links["Float"],
                                    links["FileSelector"],
                                    links["LineAwesomeIcons"],
                                    links["ProgressModal"],
                                    links["ThemeColors"]
                                ),
                            NavLink("Collections").Expanded()
                               .SmallPlus()
                               .SemiBold()
                               .Links(links["ItemsList"],
                                    links["VirtualizedList"],
                                    links["InfiniteScrollingList"],
                                    links["SearchableList"],
                                    links["SearchableGroupedList"],
                                    links["DetailsList"],
                                    links["Timeline"]),
                            NavLink("Nav Sample").Expanded()
                               .SmallPlus()
                               .SemiBold()
                               .Links(NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                    NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                    NavLink("Async 1").LinksAsync(async () =>
                                    {
                                        await Task.Delay(500);
                                        return new[] { NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4"))) };
                                    })
                                ))
                );
        }
    }
}