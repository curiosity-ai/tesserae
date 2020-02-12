using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae.Components;
using Tesserae.Tests.Samples;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    public class App
    {
        public static void Main()
        {
            var components = new (string Name, IComponent Component)[]
            {
                ("Button", new ButtonSample()),
                ("CheckBox", new CheckBoxSample()),
                ("ChoiceGroup", new ChoiceGroupSample()),
                ("Dropdown", new DropdownSample()),
                ("Label", new LabelSample()),
                ("EditableLabel", new EditableLabelSample()),
                ("HorizontalSeparator", new HorizontalSeparatorSample()),
                ("TextBox", new TextBoxSample()),
                ("SearchBox", new SearchBoxSample()),
                ("Toggle", new ToggleSample()),
                ("Spinner", new SpinnerSample()),
                ("ProgressIndicator", new ProgressIndicatorSample()),
                ("Dialog", new DialogSample()),
                ("Modal", new ModalSample()),
                ("Panel", new PanelSample()),
                ("ContextMenu", new ContextMenuSample()),
                ("ProgressModal", new ProgressModalSample()),
                ("Layer", new LayerSample()),
                ("Stack", new StackSample()),
                ("SectionStack", new SectionStackSample()),
                ("TextBlock", new TextBlockSample()),
                ("Validator", new ValidatorSample()),
                ("OverflowSet", new OverflowSetSample()),
                ("Breadcrumb", new BreadcrumbSample()),
                ("Pivot", new PivotSample()),
                ("Defer", new DeferSample()),
                ("Toast", new ToastSample()),
                ("FileSelector", new FileSelectorAndDropAreaSample())
            };

            var links = components.ToDictionary(
                component => component.Name,
                component => NavLink(component.Name).OnSelected((s, e) => Router.Navigate("#" + ToRoute(component.Name)))
            );

            var mainStack = Stack().Padding("16px")
                                .WidthStretch()
                                .MinHeightStretch();

            var sideBar = Sidebar();

            var navBar = Navbar().SetTop(Stack().Horizontal()
                                          .WidthStretch()
                                          .HeightStretch()
                                          .Children(SearchBox("Search for a template").WidthStretch().Underlined()));

            var page = new SplitView().Left(MainNav(links, navBar, sideBar), background: Theme.Default.Background)
                                      .Right(mainStack, background: Theme.Secondary.Background)
                                      .LeftIsSmaller(SizeMode.Pixels, 300)
                                      .MinHeightStretch();

            navBar.SetContent(page);

            sideBar.IsVisible = false;
            navBar.IsVisible  = false;

            document.body.appendChild(sideBar.Brand(SidebarItem("... meow", "las la-cat").Large().NonSelectable())
                                              .Add(SidebarItem("Colorful sidebar", "las la-tint").OnSelect((s) => sideBar.IsLight = false).Selected())
                                              .Add(SidebarItem("Light sidebar", "las la-tint-slash").OnSelect((s) => sideBar.IsLight = true))
                                              .Add(SidebarItem("Always Open", "las la-arrow-to-right").OnSelect((s) => sideBar.IsAlwaysOpen= true))
                                              .Add(SidebarItem("Open on Hover", "las la-arrows-alt-h").OnSelect((s) => sideBar.IsAlwaysOpen = false))
                                              .Add(SidebarItem("Small sidebar", "las la-minus-square").OnSelect((s) => sideBar.Width = Components.Sidebar.Size.Small))
                                              .Add(SidebarItem("Medium sidebar", "las la-square").OnSelect((s) => sideBar.Width = Components.Sidebar.Size.Medium))
                                              .Add(SidebarItem("Large sidebar", "las la-plus-square").OnSelect((s) => sideBar.Width = Components.Sidebar.Size.Large))
                                              .SetContent(navBar)
                                              .Render());
            document.body.style.overflow = "hidden";

            Router.Register("home", "/", p => Router.Navigate("#" + ToRoute(components.First().Name)));
            foreach (var (name, component) in components)
                Router.Register(name, ToRoute(name), p => Show(name, component));

            Router.Initialize();
            Router.Refresh((err, state) => Router.Navigate(window.location.hash, reload: false));

            string ToRoute(string name) => "/view/" + name;

            void Show(string route, IComponent component)
            {
                Router.Replace($"#/view/{route}");
                mainStack.Clear();
                mainStack.Add(component);
                mainStack.MinHeightStretch();
            }
        }

        private static IComponent MainNav(Dictionary<string, Nav.NavLink> links, Navbar navBar, Sidebar sideBar)
        {
            return Stack().Padding(Unit.Pixels, 16).NoShrink().MinHeightStretch()
                          .Children(TextBlock("Tesserae Samples").MediumPlus().SemiBold().AlignCenter(),
                                    Nav().InlineContent(Label("Theme").Inline().SetContent(Toggle("Light", "Dark").Checked().OnChange((t, e) => { if (t.IsChecked) { Theme.Light(); } else { Theme.Dark(); } })))
                                         .InlineContent(Label("Navbar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { navBar.IsVisible = t.IsChecked; })))
                                         .InlineContent(Label("Sidebar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { sideBar.IsVisible = t.IsChecked; })))
                                         .Links(NavLink("Basic Inputs").Expanded()
                                                                       .SmallPlus()
                                                                       .SemiBold()
                                                                       .Links(links["Button"],
                                                                              links["CheckBox"],
                                                                              links["ChoiceGroup"],
                                                                              links["Dropdown"] ,
                                                                              links["Label"],
                                                                              links["EditableLabel"],
                                                                              links["TextBox"] ,
                                                                              links["SearchBox"] ,
                                                                              links["Toggle"]),
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
                                                                          links["Panel"],
                                                                          links["ContextMenu"]),
                                                NavLink("Utilities").Expanded()
                                                                    .SmallPlus()
                                                                    .SemiBold()
                                                                    .Links(links["Layer"]       ,
                                                                           links["Stack"]       ,
                                                                           links["HorizontalSeparator"],
                                                                           links["SectionStack"],
                                                                           links["TextBlock"]   ,
                                                                           links["Validator"]   ,
                                                                           links["Breadcrumb"]  ,
                                                                           links["OverflowSet"]  ,
                                                                           links["Pivot"],
                                                                           links["Defer"],
                                                                           links["Toast"],
                                                                           links["FileSelector"],
                                                                           links["ProgressModal"]),
                                                NavLink("Nav Sample").Expanded()
                                                                     .SmallPlus()
                                                                     .SemiBold()
                                                                     .Links(NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                                                            NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                                                            NavLink("Async 1").LinksAsync(async () => { await Task.Delay(500); return new[] { NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4"))) }; })
                                                ))
                                         .InlineContent(Link("https://www.curiosity.ai", TextBlock("by curiosity.ai").XSmall().Primary().AlignEnd())
            ));
        }
    }
}