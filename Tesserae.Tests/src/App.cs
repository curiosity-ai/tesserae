using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using Tesserae.Tests.Samples;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    internal static class App
    {
        private static void Main()
        {
            var orderedComponents = new (string Name, Func<IComponent> Component)[]
            {
                ("Button", () => new ButtonSample()),
                ("ThemeColors", () => new ThemeColorsSample()),
                ("CheckBox", () => new CheckBoxSample()),
                ("ChoiceGroup", () => new ChoiceGroupSample()),
                ("Slider", () => new SliderSample()),
                ("Dropdown", () => new DropdownSample()),
                ("Label", () => new LabelSample()),
                ("EditableLabel", () => new EditableLabelSample()),
                ("HorizontalSeparator", () => new HorizontalSeparatorSample()),
                ("TextBox", () => new TextBoxSample()),
                ("ColorPicker", () => new ColorPickerSample()),
                ("DateTimePicker", () => new DateTimePickerSample()),
                ("DatePicker", () => new DatePickerSample()),
                ("SearchBox", () => new SearchBoxSample()),
                ("Toggle", () => new ToggleSample()),
                ("Spinner", () => new SpinnerSample()),
                ("ProgressIndicator", () => new ProgressIndicatorSample()),
                ("Dialog", () => new DialogSample()),
                ("Modal", () => new ModalSample()),
                ("TutorialModal", () => new TutorialModalSample()),
                ("Panel", () => new PanelSample()),
                ("ContextMenu", () => new ContextMenuSample()),
                ("ProgressModal", () => new ProgressModalSample()),
                ("ItemsList", () => new ItemsListSample()),
                ("InfiniteScrollingList", () => new InfiniteScrollingListSample()),
                ("VirtualizedList", () => new VirtualizedListSample()),
                ("SearchableList", () => new SearchableListSample()),
                ("SearchableGroupedList", () => new SearchableGroupedListSample()),
                ("DetailsList", () => new DetailsListSample()),
                ("Picker", () => new PickerSample()),
                ("Layer", () => new LayerSample()),
                ("Timeline", () => new TimelineSample()),
                ("Stack", () => new StackSample()),
                ("Masonry", () => new MasonrySample()),
                ("SectionStack", () => new SectionStackSample()),
                ("TextBlock", () => new TextBlockSample()),
                ("Validator", () => new ValidatorSample()),
                ("OverflowSet", () => new OverflowSetSample()),
                ("Breadcrumb", () => new BreadcrumbSample()),
                ("TextBreadcrumbs", () => new TextBreadcrumbsSample()),
                ("Pivot", () => new PivotSample()),
                ("Defer", () => new DeferSample()),
                ("Toast", () => new ToastSample()),
                ("Float", () => new FloatSample()),
                ("LineAwesomeIcons", () => new LineAwesomeSample()),
                ("FileSelector", () => new FileSelectorAndDropAreaSample()),
                ("GridPicker", () => new GridPickerSample())
            };

            var sideBar = Sidebar().Stretch();
            var navBar = Navbar().SetTop(HStack().S().Children(SearchBox("Search for a template").WidthStretch().Underlined()));
            sideBar.IsVisible = false;
            navBar.IsVisible = false;
            document.body.style.overflow = "hidden";


            document.body.appendChild(sideBar.Brand(SidebarItem("... meow", "las la-cat", href: "https://curiosity.ai").Large())
               .Add(SidebarItem("Colorful sidebar",                         "las la-tint").OnSelect((s) => sideBar.IsLight = false).Selected())
               .Add(SidebarItem("Light sidebar",                            "las la-tint-slash").OnSelect((s) => sideBar.IsLight = true))
               .Add(SidebarItem("Always Open",                              "las la-arrow-to-right").OnSelect((s) => sideBar.IsAlwaysOpen = true))
               .Add(SidebarItem("Open on Hover",                            "las la-arrows-alt-h").OnSelect((s) => sideBar.IsAlwaysOpen = false))
               .Add(SidebarItem("Small sidebar",                            "las la-minus-square").OnSelect((s) => sideBar.Width = Sidebar.Size.Small))
               .Add(SidebarItem("Medium sidebar",                           "las la-square").OnSelect((s) => sideBar.Width = Sidebar.Size.Medium))
               .Add(SidebarItem("Large sidebar",                            "las la-plus-square").OnSelect((s) => sideBar.Width = Sidebar.Size.Large))
               .SetContent(navBar)
               .Render());


            // We'll render the content in a DeferedComponent that updates itself whenever the "currentPage" observable's value changes - these changes will be triggered by the routing configured below
            var documentTitleBase = document.title;
            var currentPage = new SettableObservable<string>();
            var components = orderedComponents.ToDictionary(c => c.Name, c => c.Component);

            navBar.SetContent(
                Defer(
                        currentPage,
                        newlySelectedPage => ShowPage(newlySelectedPage).AsTask()
                    )
                   .Stretch()
            );

            // Configure routes for every component that we listed at the top of this method and one for home - whenever one of those routes is hit, the currentPage observable will have its value changed which will result in a navigation
            // - Note that the "home" route will set the currentPage observable to null and the ShowPage result of that is to show the content for the first component
            Router.Register("home", "/", _ => currentPage.Value = null);

            foreach (var (name, component) in orderedComponents)
            {
                var nameLocal = name;
                Router.Register(nameLocal, ToRoute(nameLocal), _ => currentPage.Value = nameLocal);
            }

            Router.Initialize();
            Router.Refresh(onDone: Router.ForceMatchCurrent); // We need to forcibly match the route at first loading since we want the just-registered routes to be matched against the current URL without us *changing* that URL

            string ToRoute(string name) => "/view/" + name;

            IComponent ShowPage(string componentRouteName)
            {
                if ((componentRouteName is null) || !components.ContainsKey(componentRouteName))
                    componentRouteName = components.Keys.First();
                else
                    Router.Push($"#/view/{componentRouteName}");

                document.title = documentTitleBase + " - " + componentRouteName;

                var links = orderedComponents.ToDictionary(
                    c => c.Name,
                    c => NavLink(c.Name).SelectedOrExpandedIf(c.Name == componentRouteName).OnSelected(s =>
                    {
                        console.log("Route to " + c.Name);
                        Router.Navigate("#" + ToRoute(c.Name));
                    })
                );
                var closePanelButton = Button().SetIcon(LineAwesome.ArrowLeft).Tooltip("Close panel");

                var component = components[componentRouteName]();

                var splitView = SplitView().NoSplitter()
                   .Left(Stack().Stretch().Children(MainNav(links, navBar, sideBar)).InvisibleScroll(), background: Theme.Default.Background)
                   .LeftIsSmaller(300.px())
                   .Stretch()
                   .Right(Stack().Stretch().Children(closePanelButton, component.WidthStretch()).ScrollY(), background: Theme.Secondary.Background);

                bool panelIsOpen = true;

                closePanelButton.OnClick((_, __) =>
                {
                    if (panelIsOpen)
                    {
                        panelIsOpen = false;
                        splitView.Close();
                        closePanelButton.SetIcon(LineAwesome.ArrowRight).Tooltip("Open panel");
                    }
                    else
                    {
                        panelIsOpen = true;
                        splitView.Open();
                        closePanelButton.SetIcon(LineAwesome.ArrowLeft).Tooltip("Close panel");
                    }
                });
                return splitView;
            }
        }

        private static IComponent MainNav(Dictionary<string, Nav.NavLink> links, Navbar navBar, Sidebar sideBar)
        {
            return Stack().Padding(16.px()).NoShrink().MinHeightStretch()
               .Children(TextBlock("Tesserae UI Toolkit").MediumPlus().SemiBold().AlignCenter(),
                    Stack().Horizontal().JustifyContent(ItemJustify.Center).PT(10.px()).PB(10.px()).Children(TextBlock("by").XSmall().PR(4.px()), Link("https://www.curiosity.ai", TextBlock("curiosity.ai").XSmall().Primary()).PR(4.px()), TextBlock("built with").XSmall().PR(4.px()), Link("https://h5.rocks", TextBlock("h5 🚀").XSmall().Primary())),
                    Nav().InlineContent(Label("Theme").Inline().SetContent(Toggle("Light", "Dark").Checked().OnChange((t, e) =>
                        {
                            if (t.IsChecked) { Theme.Light(); }
                            else { Theme.Dark(); }
                        })))
                       .InlineContent(Label("Navbar").Inline().SetContent(Toggle("Show",  "Hidden").OnChange((t, e) => { navBar.IsVisible = t.IsChecked; })))
                       .InlineContent(Label("Sidebar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { sideBar.IsVisible = t.IsChecked; })))
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