using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Transpose.Core;
using Tesserae;
using Tesserae.Tests.Samples;
using Tesserae.Tests;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    internal static class App
    {
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

            // Not sortable: the sidebar's order is the one SampleGroup.InDisplayOrder and each
            // sample's Order declare, and it is the same order the landing page reads top to bottom.
            // Dragging an entry out of it made the two disagree, and made a shared link to a sample
            // land somewhere else in the list than the person who sent it saw.
            var sidebar = Sidebar();

            // On mobile we use navbar mode: horizontal bar + full-screen sliding drawer.
            // This is evaluated once at startup; the CSS (tss-mobile class) handles visual switches
            // for subsequent resize events.
            if (Theme.IsMobileMode)
            {
                sidebar.AsNavbar();
            }

            sidebar.AddHeader(new SidebarText("header", "Tesserae", "TSS", textSize: TextSize.XLarge, textWeight: TextWeight.Bold));

            var searchBox = new SidebarSearchBox("search", "Search...");
            searchBox.OnSearch((term) => sidebar.Search(term));
            sidebar.AddHeader(searchBox);

            //Important: Reflection will only properly work here if reflection metadata is emitted inline with the javascript, instead of in a separate .meta.js file
            //           i.e. in the tps.json file, we need:      "reflection": { "disabled": false, "target":  "inline" },

            // Built before the content area, because the landing page it shows when no sample is
            // selected is the same list, drawn as cards.
            var samples = typeof(ISample).Assembly.GetTypes().Where(t => typeof(ISample).IsAssignableFrom(t) && !t.IsInterface)
               .Select(sampleType =>
               {
                   var sg = sampleType.GetCustomAttributes(typeof(SampleDetailsAttribute), true).FirstOrDefault() as SampleDetailsAttribute;
                   var group = sg is object ? sg.Group : "Others";
                   int order = sg is object ? sg.Order : 0;
                   UIcons icon = sg is object ? sg.Icon : UIcons.Circle;
                   string description = sg is object ? sg.Description : null;
                   return new Sample(sampleType.Name, Sample.FormatSampleName(sampleType), group, order, icon, description, async () => await Activator.CreateInstanceAsync(sampleType) as IComponent);
               })
               .ToDictionary(s => s.Name, s => s);

            var contentArea = Defer(currentPage, async page => page is null
                ? (IComponent)VStack().S().ScrollY().Children(new LandingPage(samples.Values).WS())
                : VStack().S().ScrollY().Children((await page.ContentGenerator()).WS().MinHeight(100.percent())));

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

            // Groups are laid out in SampleGroup.InDisplayOrder, not alphabetically: the sidebar
            // reads top-down from the containers a page is built out of to the helpers that render
            // nothing on their own, and alphabetical ordering would scatter that.
            foreach (var group in samples.Values.GroupBy(s => s.Group).OrderBy(g => SampleGroup.DisplayIndex(g.Key)).ThenBy(g => g.Key))
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
                        // Push asks the OnBeforeNavigate handler registered below and returns false
                        // when it refuses, so a sample holding unsaved changes isn't swapped out from
                        // under the dialog. The guard re-issues the navigation once the user decides,
                        // and the route registered below is what shows the new sample then.
                        if (!Router.Push($"#/view/{item.Name}")) return;

                        currentPage.Value = item;
                    });


                    sidebar.AddContent(sidebarItem);
                    allSidebarItems.Add(sidebarItem);
                    sampleToSidebarItem[item] = sidebarItem;
                }
            }


            // One handler covers every way out of a sample: the browser's back/forward buttons,
            // Router.Navigate, and the sidebar's Router.Push. (Closing or reloading the browser tab
            // is handled by the guard's own beforeunload listener.) Router keeps a single handler,
            // so a sample needing its own before-navigate logic has to call CanNavigateAway from it.
            Router.OnBeforeNavigate((toState, fromState, isBack) => UnsavedChangesGuard.CanNavigateAway(toState, fromState));

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
