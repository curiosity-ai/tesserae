using System.Collections.Generic;
using System.Threading.Tasks;
using Tesserae;
using Tesserae.Components;
using Tesserae.HTML;
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
        private static Dictionary<string, Nav.NavLink> _links = new Dictionary<string, Nav.NavLink>(new LowerCaseComparer());
        
        public static void Main()
        {
            _mainStack = Stack().Padding("16px")
                                .WidthStretch()
                                .MinHeightStretch();

            _sideBar = Sidebar();

            var page = new SplitView().Left(MainNav(), background: Theme.Default.Background)
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

            Router.Initialize();
            Router.Register("home", "/", (p) => _links["Button"].Selected());
            Router.Register("view", "/view/:component", (p) => _links[p["component"]].Selected());
            Router.Refresh((err, state) => Router.Navigate(window.location.hash, reload: false));
        }

        public static IComponent MainNav()
        {
            _links["Button"              ] = NavLink("Button")              .OnSelected((s, e) => Show("Button"           ,     new ButtonSample()));
            _links["CheckBox"            ] = NavLink("CheckBox")            .OnSelected((s, e) => Show("CheckBox"         ,     new CheckBoxSample()));
            _links["ChoiceGroup"         ] = NavLink("ChoiceGroup")         .OnSelected((s, e) => Show("ChoiceGroup"      ,     new ChoiceGroupSample()));
            _links["Dropdown"            ] = NavLink("Dropdown")            .OnSelected((s, e) => Show("Dropdown"         ,     new DropdownSample()));
            _links["Label"               ] = NavLink("Label")               .OnSelected((s, e) => Show("Label"            ,     new LabelSample()));
            _links["EditableLabel"       ] = NavLink("EditableLabel")       .OnSelected((s, e) => Show("EditableLabel"    ,     new EditableLabelSample()));
            _links["HorizontalSeparator" ] = NavLink("HorizontalSeparator") .OnSelected((s, e) => Show("HorizontalSeparator",   new HorizontalSeparatorSample()));
            _links["TextBox"             ] = NavLink("TextBox")             .OnSelected((s, e) => Show("TextBox"          ,     new TextBoxSample()));
            _links["SearchBox"           ] = NavLink("SearchBox")           .OnSelected((s, e) => Show("SearchBox"        ,     new SearchBoxSample()));
            _links["Toggle"              ] = NavLink("Toggle")              .OnSelected((s, e) => Show("Toggle"           ,     new ToggleSample()));
            _links["Spinner"             ] = NavLink("Spinner")             .OnSelected((s, e) => Show("Spinner"          ,     new SpinnerSample()));
            _links["ProgressIndicator"   ] = NavLink("ProgressIndicator")   .OnSelected((s, e) => Show("ProgressIndicator",     new ProgressIndicatorSample()));
            _links["Dialog"              ] = NavLink("Dialog")              .OnSelected((s, e) => Show("Dialog"           ,     new DialogSample()));
            _links["Modal"               ] = NavLink("Modal")               .OnSelected((s, e) => Show("Modal"            ,     new ModalSample()));
            _links["Panel"               ] = NavLink("Panel")               .OnSelected((s, e) => Show("Panel"            ,     new PanelSample()));
            _links["ContextMenu"         ] = NavLink("ContextMenu")         .OnSelected((s, e) => Show("ContextMenu"      ,     new ContextMenuSample()));
            _links["ProgressModal"       ] = NavLink("ProgressModal")       .OnSelected((s, e) => Show("ProgressModal"    ,     new ProgressModalSample()));
            _links["Layer"               ] = NavLink("Layer")               .OnSelected((s, e) => Show("Layer"            ,     new LayerSample()));
            _links["Stack"               ] = NavLink("Stack")               .OnSelected((s, e) => Show("Stack"            ,     new StackSample()));
            _links["SectionStack"        ] = NavLink("SectionStack")        .OnSelected((s, e) => Show("SectionStack"     ,     new SectionStackSample()));
            _links["TextBlock"           ] = NavLink("TextBlock")           .OnSelected((s, e) => Show("TextBlock"        ,     new TextBlockSample()));
            _links["Validator"           ] = NavLink("Validator")           .OnSelected((s, e) => Show("Validator"        ,     new ValidatorSample()));
            _links["OverflowSet"         ] = NavLink("OverflowSet")         .OnSelected((s, e) => Show("OverflowSet"      ,     new OverflowSetSample()));
            _links["Breadcrumb"          ] = NavLink("Breadcrumb")          .OnSelected((s, e) => Show("Breadcrumb"       ,     new BreadcrumbSample()));
            _links["Pivot"               ] = NavLink("Pivot").OnSelected((s, e) => Show("Pivot"            ,     new PivotSample()));
            _links["Defer"               ] = NavLink("Defer")               .OnSelected((s, e) => Show("Defer"            ,     new DeferSample()));
            _links["Toast"               ] = NavLink("Toast")               .OnSelected((s, e) => Show("Toast"            ,     new ToastSample()));
            

            return Stack().Padding(Unit.Pixels, 16).NoShrink().MinHeightStretch()
                          .Children(TextBlock("Tesserae Samples").MediumPlus().SemiBold().AlignCenter(),
                                    Nav().InlineContent(Label("Theme").Inline().SetContent(Toggle("Light", "Dark").Checked().OnChange((t, e) => { if (t.IsChecked) { Theme.Light(); } else { Theme.Dark(); } })))
                                         .InlineContent(Label("Navbar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { _navBar.IsVisible = t.IsChecked; })))
                                         .InlineContent(Label("Sidebar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { _sideBar.IsVisible = t.IsChecked; })))
                                         .Links(NavLink("Basic Inputs").Expanded()
                                                                       .SmallPlus()
                                                                       .SemiBold()
                                                                       .Links(_links["Button"],
                                                                              _links["CheckBox"],
                                                                              _links["ChoiceGroup"],
                                                                              _links["Dropdown"] ,
                                                                              _links["Label"],
                                                                              _links["EditableLabel"],
                                                                              _links["TextBox"] ,
                                                                              _links["SearchBox"] ,
                                                                              _links["Toggle"]),
                                                NavLink("Progress").Expanded()
                                                                   .SmallPlus()
                                                                   .SemiBold()
                                                                   .Links(_links["Spinner"], 
                                                                          _links["ProgressIndicator"]),
                                                NavLink("Surfaces").Expanded()
                                                                   .SmallPlus()
                                                                   .SemiBold()
                                                                   .Links(_links["Dialog"] ,
                                                                          _links["Modal"] ,
                                                                          _links["Panel"] ,
                                                                          _links["ContextMenu"]),
                                                NavLink("Utilities").Expanded()
                                                                    .SmallPlus()
                                                                    .SemiBold()
                                                                    .Links(_links["Layer"]       ,
                                                                           _links["Stack"]       ,
                                                                           _links["HorizontalSeparator"],
                                                                           _links["SectionStack"],
                                                                           _links["TextBlock"]   ,
                                                                           _links["Validator"]   ,
                                                                           _links["Breadcrumb"]  ,
                                                                           _links["OverflowSet"]  ,
                                                                           _links["Pivot"],
                                                                           _links["Defer"],
                                                                           _links["Toast"],
                                                                           _links["ProgressModal"]),
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