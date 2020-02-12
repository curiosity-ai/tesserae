﻿using System.Collections.Generic;
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
        private static Stack _mainStack;
        private static Sidebar _sideBar;
        private static Navbar _navBar;

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

            _mainStack = Stack().Padding("16px")
                                .WidthStretch()
                                .MinHeightStretch();

            _sideBar = Sidebar();

            var page = new SplitView().Left(MainNav(links), background: Theme.Default.Background)
                                      .Right(_mainStack, background: Theme.Secondary.Background)
                                      .LeftIsSmaller(SizeMode.Pixels, 300)
                                      .MinHeightStretch();

            _navBar = Navbar().SetTop(Stack().Horizontal()
                                          .WidthStretch()
                                          .HeightStretch()
                                          .Children(SearchBox("Search for a template").WidthStretch().Underlined()))
                              .SetContent(page);

            _sideBar.IsVisible = false;
            _navBar.IsVisible  = false;

            document.body.appendChild(_sideBar.Add(SidebarItem("... meow", "fal fa-cat").Large().NonSelectable())
                                              .Add(SidebarItem("Colorful sidebar", "fal fa-tint").OnSelect((s) => _sideBar.IsLight = false).Selected())
                                              .Add(SidebarItem("Light sidebar", "fal fa-tint-slash").OnSelect((s) => _sideBar.IsLight = true))
                                              .Add(SidebarItem("Small sidebar", "fal fa-minus-square").OnSelect((s) => _sideBar.IsSmall = true))
                                              .Add(SidebarItem("Normal sidebar", "fal fa-plus-square").OnSelect((s) => _sideBar.IsSmall = false))
                                              .SetContent(_navBar)
                                              .Render());
            document.body.style.overflow = "hidden";

            Router.Register("home", "/", p => Router.Navigate("#" + ToRoute(components.First().Name)));
            foreach (var (name, component) in components)
                Router.Register(name, ToRoute(name), p => { console.log($"TODO: View component '{name}'"); Show(name, component); });

            Router.Initialize();
            Router.Refresh((err, state) => Router.Navigate(window.location.hash, reload: false));

            string ToRoute(string name) => "/view/" + name;
        }

        public static IComponent MainNav(Dictionary<string, Nav.NavLink> links)
        {
            return Stack().Padding(Unit.Pixels, 16).NoShrink().MinHeightStretch()
                          .Children(TextBlock("Tesserae Samples").MediumPlus().SemiBold().AlignCenter(),
                                    Nav().InlineContent(Label("Theme").Inline().SetContent(Toggle("Light", "Dark").Checked().OnChange((t, e) => { if (t.IsChecked) { Theme.Light(); } else { Theme.Dark(); } })))
                                         .InlineContent(Label("Navbar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { _navBar.IsVisible = t.IsChecked; })))
                                         .InlineContent(Label("Sidebar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { _sideBar.IsVisible = t.IsChecked; })))
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

        private static void Show(string route, IComponent component)
        {
            Router.Replace($"#/view/{route}");

            _mainStack.Clear();
            _mainStack.Add(component);
            _mainStack.MinHeightStretch();
        }

        private class LowerCaseComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Equals(x, y, System.StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.ToLower().GetHashCode();
            }
        }
    }
}