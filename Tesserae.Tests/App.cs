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
        private static Stack _MainStack;
        private static Sidebar _SideBar;
        private static Navbar _NavBar;

        public static void Main()
        {
            OnStartUp();
        }

        private static void OnStartUp()
        {
            _MainStack = Stack().Padding("16px")
                                .WidthStretch()
                                .MinHeightStretch();

            _SideBar = Sidebar();

            var page = new SplitView().Left(MainNav(), background: Theme.Default.Background)
                                      .Right(_MainStack, background: Theme.Secondary.Background)
                                      .LeftIsSmaller(SizeMode.Pixels, 300)
                                      .MinHeightStretch();

            _NavBar = Navbar().SetTop(Stack().Horizontal()
                                          .WidthStretch()
                                          .HeightStretch()
                                          .Children(SearchBox("Search for a template").WidthStretch().Underlined()))
                              .SetContent(page);

            _SideBar.IsVisible = false;
            _NavBar.IsVisible  = false;


            document.body.appendChild(_SideBar.Add(SidebarItem("... meow", "fal fa-cat").Large().NonSelectable())
                                              .Add(SidebarItem("Colorful sidebar", "fal fa-tint").OnSelect((s) => _SideBar.IsLight = false).Selected())
                                              .Add(SidebarItem("Light sidebar", "fal fa-tint-slash").OnSelect((s) => _SideBar.IsLight = true))
                                              .SetContent(_NavBar)
                                              .Render());
            document.body.style.overflow = "hidden";
        }

        public static IComponent MainNav()
        {
            return Stack().Padding(16).NoShrink().MinHeightStretch()
                          .Children(TextBlock("Tesserae Samples").MediumPlus().SemiBold().AlignCenter(),
                                    Nav().InlineContent(Label("Theme").Inline().SetContent(Toggle("Light", "Dark").Checked().OnChange((t, e) => { if (t.IsChecked) { Theme.Light(); } else { Theme.Dark(); } })))
                                         .InlineContent(Label("Navbar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { _NavBar.IsVisible = t.IsChecked; })))
                                         .InlineContent(Label("Sidebar").Inline().SetContent(Toggle("Show", "Hidden").OnChange((t, e) => { _SideBar.IsVisible = t.IsChecked; })))
                                         .Links(NavLink("Basic Inputs").Expanded()
                                                                       .SmallPlus()
                                                                       .SemiBold()
                                                                       .Links(NavLink("Button").OnSelected((s, e)      => Show(new ButtonSample())).Selected(),
                                                                              NavLink("CheckBox").OnSelected((s, e)    => Show(new CheckBoxSample())),
                                                                              NavLink("ChoiceGroup").OnSelected((s, e) => Show(new ChoiceGroupSample())),
                                                                              NavLink("Dropdown").OnSelected((s, e)    => Show(new DropdownSample())),
                                                                              NavLink("Label").OnSelected((s, e)       => Show(new LabelSample())),
                                                                              NavLink("TextBox").OnSelected((s, e)     => Show(new TextBoxSample())),
                                                                              NavLink("SearchBox").OnSelected((s, e)   => Show(new SearchBoxSample())),
                                                                              NavLink("Toggle").OnSelected((s, e)      => Show(new ToggleSample()))  
                                                ),
                                                NavLink("Progress").Expanded()
                                                                   .SmallPlus()
                                                                   .SemiBold()
                                                                   .Links(NavLink("Spinner").OnSelected((s, e) => Show(new SpinnerSample())))
                                                                   .Links(NavLink("Progress Indicator").OnSelected((s, e) => Show(new ProgressIndicatorSample()))),
                                                NavLink("Surfaces").Expanded()
                                                                   .SmallPlus()
                                                                   .SemiBold()
                                                                   .Links(NavLink("Dialog").OnSelected((s, e) => Show(new DialogSample())),
                                                                          NavLink("Modal").OnSelected((s, e)  => Show(new ModalSample())),
                                                                          NavLink("Panel").OnSelected((s, e)  => Show(new PanelSample())),
                                                                          NavLink("ContextMenu").OnSelected((s, e) => Show(new ContextMenuSample())),
                                                                          NavLink("ProgressModal").OnSelected((s, e) => Show(new ProgressModalSample())) 
                                                ),
                                                NavLink("Utilities").Expanded()
                                                                    .SmallPlus()
                                                                    .SemiBold()
                                                                    .Links(NavLink("Layer").OnSelected((s, e)         => Show(new LayerSample())),
                                                                           NavLink("Stack").OnSelected((s, e)         => Show(new StackSample())),
                                                                           NavLink("SectionStack").OnSelected((s, e) => Show(new SectionStackSample())),
                                                                           NavLink("TextBlock").OnSelected((s, e)     => Show(new TextBlockSample())),
                                                                           NavLink("Validator").OnSelected((s, e)     => Show(new ValidatorSample())),
                                                                           NavLink("Pivot").OnSelected((s, e)         => Show(new PivotSample())),
                                                                           NavLink("Defer").OnSelected((s, e)         => Show(new DeferSample()))
                                                ),
                                                NavLink("Nav Sample").Expanded()
                                                                     .SmallPlus()
                                                                     .SemiBold()
                                                                     .Links(NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                                                            NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                                                            NavLink("Level 1").LinksAsync(async () => { await Task.Delay(500); return new[] { NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4"))) }; })
                                                ))
                                         .InlineContent(Link("https://www.curiosity.ai", TextBlock("by curiosity.ai").XSmall().Primary().AlignEnd())
            ));
        }

        private static void Show(IComponent component)
        {
            _MainStack.Clear();
            _MainStack.Add(component);
            _MainStack.MinHeightStretch();
        }
    }
}