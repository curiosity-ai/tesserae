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

            // Chrome on Android applies the phone's text size as a page zoom, which shrinks the
            // viewport under a px-sized layout; this opts out (it scales the root font size instead).
            if (document.head.querySelector("meta[name='text-scale']") is null)
            {
                var textScaleMeta = document.createElement("meta");
                textScaleMeta["name"]    = "text-scale";
                textScaleMeta["content"] = "scale";
                document.head.appendChild(textScaleMeta);
            }

            // Enable automatic mobile detection — adds/removes the tss-mobile class on body
            // whenever the viewport is 768px or narrower (or when the device reports a coarse pointer).
            Theme.EnableMobileDetection(breakpoint: 768);

            var allSidebarItems      = new List<ISidebarItem>();
            var sampleToSidebarItems = new Dictionary<Sample, List<ISidebarItem>>();

            var currentPage = new SettableObservable<Sample>(null);

            currentPage.Observe(selected =>
            {
                var toSelect = selected is object && sampleToSidebarItems.TryGetValue(selected, out var items) ? items : new List<ISidebarItem>();
                allSidebarItems.ForEach(i => i.IsSelected = toSelect.Contains(i));
            });

            // Not sortable: the sidebar's order is the one SampleGroup.InDisplayOrder and each
            // sample's Order declare, and it is the same order the landing page reads top to bottom.
            // Dragging an entry out of it made the two disagree, and made a shared link to a sample
            // land somewhere else in the list than the person who sent it saw.
            var sidebar = Sidebar();

            sidebar.AddHeader(new SidebarText("header", "Tesserae", "TSS", textSize: TextSize.XLarge, textWeight: TextWeight.Bold));

            var searchTerm = "";
            var searchBox  = new SidebarSearchBox("search", "Search...");

            searchBox.OnSearch((term) =>
            {
                searchTerm = term;
                sidebar.Search(term);
            });

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

            // On a phone the sidebar is a page (Sidebar.AsPage): it and the content take turns filling the
            // screen, and this bar is how the content gets back to it. It collapses itself on a desktop.
            var showCode = Button().SetIcon(UIcons.SquareTerminal).Tooltip("Show sample code").OnClick(() =>
            {
                if (currentPage.Value is object) SamplesHelper.ShowSampleCode(currentPage.Value.Type);
            });

            var pageBar = SidebarPageBar(sidebar)
               .Brand(TextBlock("TSS").Bold().Foreground(Theme.Primary.Background))
               .Commands(showCode);

            currentPage.Observe(page =>
            {
                pageBar.SetTitle(page is object ? page.Name : "Components");

                if (page is object) showCode.Show();
                else                showCode.Collapse();
            });

            // The shell is a row on both layouts - sidebar left, content right - and it is the sidebar
            // that changes: a rail beside the content on a desktop, a page of its own on a phone, where
            // it hides the content while it is on screen and hides itself once a sample is picked.
            //
            // The sidebar itself is never given an inline width: .tss-sidebar's own 250px is what
            // sizes it on desktop, and page mode fills the row on a phone.
            var content     = VStack().Children(pageBar, contentArea.WS().H(1).Grow());
            var pageContent = HStack().S().Children(sidebar.HS(), content.HS().W(1).Grow());

            MountToBody(pageContent);

            sidebar.AddHeader(new SidebarButton("SOURCE_CODE", Emoji.House, "Source Code", new SidebarCommand(UIcons.ArrowUpRightFromSquare).Tooltip("Open repository on GitHub")
                   .OnClick(() => window.open("https://github.com/curiosity-ai/tesserae", "_blank")))
               .CommandsAlwaysVisible()
               .OnOpenIconClick(() => Toast().Success("You clicked on the icon")));

            // A page is always open, so the command that closes the rail is hidden with the brand's own
            var openClose = new SidebarCommand(UIcons.AngleLeft).Tooltip("Close Sidebar").Class("tss-sidebar-close-command");

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

            SidebarButton SampleButton(Sample item, string identifier)
            {
                var sidebarItem = new SidebarButton(identifier, item.Icon, item.Name, new SidebarCommand(UIcons.SquareTerminal).Tooltip("Show sample code").OnClick(() => SamplesHelper.ShowSampleCode(item.Type)),
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

                allSidebarItems.Add(sidebarItem);

                if (!sampleToSidebarItems.TryGetValue(item, out var items))
                {
                    items = new List<ISidebarItem>();
                    sampleToSidebarItems[item] = items;
                }

                items.Add(sidebarItem);
                return sidebarItem;
            }

            // Two sets of rows over the same samples, because the two layouts group them differently: a desktop
            // lists every sample under a separator per category, and a phone - where the sidebar is a page and a
            // hundred rows is a long way to scroll with a thumb - shows one row per category, which opens its
            // samples as a panel over the sidebar.
            var railContent  = new List<ISidebarItem>();
            var pageContents = new List<ISidebarItem>();
            var groupIndex   = 0;

            // Groups are laid out in SampleGroup.InDisplayOrder, not alphabetically: the sidebar
            // reads top-down from the containers a page is built out of to the helpers that render
            // nothing on their own, and alphabetical ordering would scatter that.
            foreach (var group in samples.Values.GroupBy(s => s.Group).OrderBy(g => SampleGroup.DisplayIndex(g.Key)).ThenBy(g => g.Key))
            {
                var groupKey = group.Key + groupIndex++;

                railContent.Add(new SidebarSeparator(groupKey, group.Key));

                var nav = new SidebarNav("page-" + groupKey, SampleGroup.IconFor(group.Key), group.Key, initiallyCollapsed: true);
                pageContents.Add(nav);

                var itemIndex = 0;

                foreach (var item in group.OrderBy(s => s.Order).ThenBy(s => s.Name.ToLower()))
                {
                    var identifier = item.Name + itemIndex++;

                    railContent.Add(SampleButton(item, identifier));
                    nav.Add(SampleButton(item, identifier));
                }
            }

            bool? showingPageContents = null;

            // Points the shell at the layout the current mode calls for. Called once below and then
            // on every OnMobileModeChanged, so narrowing or widening the window switches the whole
            // shell rather than leaving one layout for the mobile stylesheet to reshape.
            void ApplyLayoutMode(bool isMobile)
            {
                if (showingPageContents != isMobile)
                {
                    showingPageContents = isMobile;

                    sidebar.ClearContent();
                    (isMobile ? pageContents : railContent).ForEach(i => sidebar.AddContent(i));
                    sidebar.Search(searchTerm);
                }

                sidebar.AsPage(isMobile);

                if (!isMobile)
                {
                    // Back on desktop, the sidebar is a rail again, so restore the user's own
                    // open/closed preference.
                    var sidebarOpenState = bool.TryParse(localStorage.getItem(_sidebarOpenStateKey), out var v) ? v : true;
                    sidebar.Closed(!sidebarOpenState);

                    openClose.SetIcon(sidebarOpenState ? UIcons.AngleLeft : UIcons.AngleRight)
                       .Tooltip(sidebarOpenState ? "Close Sidebar" : "Open Sidebar");
                }
            }

            ApplyLayoutMode(Theme.IsMobileMode);
            Theme.OnMobileModeChanged += () => ApplyLayoutMode(Theme.IsMobileMode);

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
                Router.Register($"#/view/{kv.Key.Replace(" ", "%20")}", _ =>
                {
                    currentPage.Value = kv.Value;
                    sidebar.ShowContent(); //a link to a sample opens on the sample, not on the sidebar a phone would otherwise show first
                });
            }

            Router.Initialize();
            Router.Refresh(onDone: Router.ForceMatchCurrent); // We need to forcibly match the route at first loading since we want the just-registered routes to be matched against the current URL without us *changing* that URL
        }
    }
}
