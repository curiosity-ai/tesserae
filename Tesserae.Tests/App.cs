using System.Threading.Tasks;
using Tesserae;
using Tesserae.Components;
using Tesserae.HTML;
using Tesserae.Tests.Samples;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests
{
    public class App
    {
        private static Stack _MainStack;

        public static void Main()
        {
            OnStartUp();
        }

        private static void OnStartUp()
        {
            _MainStack = Stack().Background("#faf9f8")
                                .Padding("16px")
                                .WidthStretch()
                                .MinHeightStretch();

            var samples = Stack(StackOrientation.Horizontal).MinHeightStretch()
                                                            .Children(MainNav().NoShrink()
                                                                               .WidthPixels(300)
                                                                               .MinHeightStretch(),
                                                                     _MainStack);
            document.body.appendChild(samples.Render());
            document.body.InvisibleScroll();
        }

        public static Nav MainNav()
        {
            return Nav().Links(NavLink("Basic Inputs").Expanded()
                                                      .SmallPlus()
                                                      .SemiBold()
                                                      .Links(NavLink("Button").OnSelected((s, e)      => Show(new ButtonSample())).Selected(),
                                                             NavLink("CheckBox").OnSelected((s, e)    => Show(new CheckBoxSample())),
                                                             NavLink("ChoiceGroup").OnSelected((s, e) => Show(new ChoiceGroupSample())),
                                                             NavLink("Label").OnSelected((s, e)       => Show(new LabelSample())),
                                                             NavLink("TextBox").OnSelected((s, e)     => Show(new TextBoxSample())),
                                                             NavLink("Toggle").OnSelected((s, e)      => Show(new ToggleSample()))
                               ),
                               NavLink("Surfaces").Expanded()
                                                  .SmallPlus()
                                                  .SemiBold()
                                                  .Links(NavLink("Dialog").OnSelected((s, e) => Show(new DialogSample())),
                                                         NavLink("Modal").OnSelected((s, e)  => Show(new ModalSample())),
                                                         NavLink("Panel").OnSelected((s, e)  => Show(new PanelSample()))
                               ),
                               NavLink("Utilities").Expanded()
                                                   .SmallPlus()
                                                   .SemiBold()
                                                   .Links(NavLink("Layer").OnSelected((s, e)         => Show(new LayerSample())),
                                                          NavLink("Stack").OnSelected((s, e)         => Show(new StackSample())),
                                                          NavLink("Section Stack").OnSelected((s, e) => Show(new SectionStackSample())),
                                                          NavLink("TextBlock").OnSelected((s, e)     => Show(new TextBlockSample())),
                                                          NavLink("Validator").OnSelected((s, e)     => Show(new ValidatorSample())),
                                                          NavLink("Pivot").OnSelected((s, e)         => Show(new PivotSample()))
                               ),
                               NavLink("Nav Sample").Expanded()
                                                    .SmallPlus()
                                                    .SemiBold()
                                                    .Links(NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                                           NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                                                           NavLink("Level 1").LinksAsync(async () => { await Task.Delay(500); return new[] { NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4"))) }; })
                               )
            );
        }

        private static void Show(IComponent component)
        {
            _MainStack.Clear();
            _MainStack.Add(component);
        }
    }
}