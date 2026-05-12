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
        private const string _sidebarOrderKey     = "tss-sample-sidebar-order";
        private const string _sidebarOpenStateKey = "tss-sample-sidebar-open-close";

        private static void Main()
        {
            document.body.style.overflow = "hidden";

            // Ensure the viewport meta tag is present so that mobile browsers use the device
            // width instead of rendering at a desktop width and scaling down.
            if (document.head.querySelector("meta[name='viewport']") is null)
            {
                var viewportMeta = document.createElement("meta");
                viewportMeta["name"]    = "viewport";
                viewportMeta["content"] = "width=device-width, initial-scale=1.0, maximum-scale=5.0";
                document.head.appendChild(viewportMeta);
            }

            // Enable automatic mobile detection — adds/removes the tss-mobile class on body
            // whenever the viewport is 768px or narrower (or when the device reports a coarse pointer).
            Theme.EnableMobileDetection(breakpoint: 768);

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

            var sidebar = Sidebar(sortable: true);

            // On mobile we use navbar mode: horizontal bar + full-screen sliding drawer.
            // This is evaluated once at startup; the CSS (tss-mobile class) handles visual switches
            // for subsequent resize events.
            if (Theme.IsMobileMode)
            {
                sidebar.AsNavbar();
            }

            var sortingTimeout = 0d;

            sidebar.OnSortingChanged(itemOrder =>
            {
                window.clearTimeout(sortingTimeout);

                sortingTimeout = window.setTimeout(_ =>
                {
                    var itemOrderObject = new { };

                    foreach (var item in itemOrder)
                    {
                        itemOrderObject[item.Key] = item.Value;
                    }
                    var sorting = new { itemOrder = itemOrderObject };

                    localStorage.setItem(_sidebarOrderKey, es5.JSON.stringify(sorting));
                    console.log("saved sorting", sorting);
                }, 1000);
            });


            sidebar.AddHeader(new SidebarText("header", "Tesserae", "TSS", textSize: TextSize.XLarge, textWeight: TextWeight.Bold));

            var searchBox = new SidebarSearchBox("search", "Search...");
            searchBox.OnSearch((term) => sidebar.Search(term));
            sidebar.AddHeader(searchBox);

            var contentArea = DeferSync(currentPage, page => page is null
                ? (IComponent)CenteredCardWithBackground(Message("Welcome to Tesserae", "Select a component to see more details").Icon(UIcons.Search))
                : VStack().S().ScrollY().Children(page.ContentGenerator().WS().MinHeight(100.percent())));

            // On mobile the sidebar is a fixed top navbar, so the layout is vertical (sidebar then content).
            // On desktop the layout is horizontal (sidebar left, content right).
            Stack pageContent;

            if (Theme.IsMobileMode)
            {
                pageContent = VStack().Class("tss-page-layout").S().Children(sidebar.WS(), contentArea.WS().H(1).Grow());
            }
            else
            {
                pageContent = HStack().Class("tss-page-layout").S().Children(sidebar.HS(), contentArea.HS().W(1).Grow());
            }

            MountToBody(pageContent);

            //Important: Reflection will only properly work here if reflection metadata is emitted inline with the javascript, instead of in a separate .meta.js file
            //           i.e. in the h5.json file, we need:      "reflection": { "disabled": false, "target":  "inline" },

            var samples = typeof(ISample).Assembly.GetTypes().Where(t => typeof(ISample).IsAssignableFrom(t) && !t.IsInterface)
               .Select(sampleType =>
               {
                   var sg = sampleType.GetCustomAttributes(typeof(SampleDetailsAttribute), true).FirstOrDefault() as SampleDetailsAttribute;
                   var group = sg is object ? sg.Group : "Others";
                   int order = sg is object ? sg.Order : 0;
                   UIcons icon = sg is object ? sg.Icon : UIcons.Circle;
                   return new Sample(sampleType.Name, Sample.FormatSampleName(sampleType), group, order, icon, () => Activator.CreateInstance(sampleType) as IComponent);
               })
               .ToDictionary(s => s.Name, s => s);

            sidebar.AddHeader(new SidebarButton("SOURCE_CODE", Emoji.House, "Source Code", new SidebarCommand(UIcons.ArrowUpRightFromSquare).Tooltip("Open repository on GitHub")
                   .OnClick(() => window.open("https://github.com/curiosity-ai/tesserae", "_blank")))
               .CommandsAlwaysVisible()
               .OnOpenIconClick(() => Toast().Success("You clicked on the icon")));

            var openClose = new SidebarCommand(UIcons.AngleLeft).Tooltip("Close Sidebar");

            // In mobile/navbar mode the drawer always starts closed (hamburger opens it).
            // In desktop mode, restore the user's last open/closed preference from localStorage.
            if (!Theme.IsMobileMode)
            {
                var sidebarOpenState = bool.TryParse(localStorage.getItem(_sidebarOpenStateKey), out var v) ? v : true;
                sidebar.Closed(!sidebarOpenState);
            }

            openClose.OnClick(() =>
            {
                sidebar.Toggle();

                if (sidebar.IsClosed)
                {
                    openClose.SetIcon(UIcons.AngleRight).Tooltip("Open Sidebar");
                    localStorage.setItem(_sidebarOpenStateKey, false.ToString());
                }
                else
                {
                    openClose.SetIcon(UIcons.AngleLeft).Tooltip("Close Sidebar");
                    localStorage.setItem(_sidebarOpenStateKey, true.ToString());
                }
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

            var commandSidebarconfig = new SidebarCommands("CONFIG", lightDark, openClose);
            sidebar.AddFooter(commandSidebarconfig);

            var groupIndex = 0;

            foreach (var group in samples.Values.GroupBy(s => s.Group).OrderBy(g => g.Key))
            {
                var groupKey = group.Key + groupIndex++;

                var separator = new SidebarSeparator(groupKey, group.Key);
                sidebar.AddContent(separator);

                var itemIndex = 0;

                foreach (var item in group.OrderBy(s => s.Order).ThenBy(s => s.Name.ToLower()))
                {
                    var sidebarItem = new SidebarButton(item.Name + itemIndex++, item.Icon, item.Name, new SidebarCommand(UIcons.SquareTerminal).Tooltip("Show sample code").OnClick(() => SamplesHelper.ShowSampleCode(item.Type)),
                        new SidebarCommand(UIcons.ArrowUpRightFromSquare).Tooltip("Open in new tab").OnClick(() => window.open($"#/view/{item.Name}", "_blank")));

                    sidebarItem.OnClick(() =>
                    {
                        Router.Push($"#/view/{item.Name}");
                        currentPage.Value = item;
                    });


                    sidebar.AddContent(sidebarItem);
                    allSidebarItems.Add(sidebarItem);
                    sampleToSidebarItem[item] = sidebarItem;
                }
            }


            var sidebarOrderJson = localStorage.getItem(_sidebarOrderKey);

            if (sidebarOrderJson is object)
            {
                var sidebarOrderObj = es5.JSON.parse(sidebarOrderJson);
                console.log("loaded sorting", sidebarOrderObj);


                var itemOrderObj = sidebarOrderObj["itemOrder"].As<object>();
                if (itemOrderObj is null) return;
                var itemOrder = new Dictionary<string, string[]>();

                foreach (var identifier in GetOwnPropertyNames(itemOrderObj))
                {
                    itemOrder[identifier] = itemOrderObj[identifier].As<string[]>();

                }
                sidebar.LoadSorting(itemOrder);
            }

            Router.Register("home", "/", _ => currentPage.Value = null);


            // We'll render the content in a DeferedComponent that updates itself whenever the "currentPage" observable's value changes - these changes will be triggered by the routing configured below
            var documentTitleBase = document.title;

            foreach (var kv in samples)
            {
                Router.Register($"#/view/{kv.Key.Replace(" ", "%20")}", _ => currentPage.Value = kv.Value);
            }

            Router.Initialize();
            Router.Refresh(onDone: Router.ForceMatchCurrent); // We need to forcibly match the route at first loading since we want the just-registered routes to be matched against the current URL without us *changing* that URL
        }
    }
}
