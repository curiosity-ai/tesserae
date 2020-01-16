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
            //Require.LoadStyleAsync(/*"css/curiosity-bootstrap.css", */"css/fontawesome-all.min.css", "css/prototype-ui.css");
            //Require.LoadScriptAsync(OnStartUp, null, "css/curiosity-bootstrap.css");
            OnStartUp();
        }

        private static void OnStartUp()
        {
            _MainStack = Stack();
            var samples = Stack(StackOrientation.Horizontal).Children(MainNav().NoShrink().WidthPixels(300), _MainStack.WidthStretch());
            document.body.appendChild(samples.Render());
        }

        public static Nav MainNav()
        {
            return Nav()/*.OnChanged((e, s) =>
                {
                    var link = e as NavLink;
                    _MainStack.Clear();
                    if (link.Text.StartsWith("Stack Sample")) _MainStack.Add(StackSample());
                    else if (link.Text.StartsWith("Buttons Sample")) _MainStack.Add(ButtonSample());
                    else if (link.Text.StartsWith("TextBox Sample")) _MainStack.Add(TextBoxSample());
                    else if (link.Text.StartsWith("CheckBox Sample")) _MainStack.Add(CheckBoxSample());
                    else if (link.Text.StartsWith("Toggle Sample")) _MainStack.Add(ToggleSample());
                    else if (link.Text.StartsWith("ChoiceGroup Sample")) _MainStack.Add(ChoiceGroupSample());
                    else if (link.Text.StartsWith("Slider Sample")) _MainStack.Add(SliderSample());
                    else if (link.Text.StartsWith("Layer Sample")) _MainStack.Add(LayerSample());
                    else if (link.Text.StartsWith("Panel Sample")) _MainStack.Add(PanelSample());

                    else if (link.Text.StartsWith("Button")) _MainStack.Add(new Samples.ButtonSample());
                    else if (link.Text.StartsWith("CheckBox")) _MainStack.Add(new Samples.CheckBoxSample());
                    else if (link.Text.StartsWith("ChoiceGroup")) _MainStack.Add(new Samples.ChoiceGroupSample());
                    else if (link.Text.StartsWith("TextBox")) _MainStack.Add(new Samples.TextBoxSample());
                    else if (link.Text.StartsWith("Toggle")) _MainStack.Add(new Samples.ToggleSample());
                    else if (link.Text.StartsWith("TextBlock")) _MainStack.Add(new Samples.TextBlockSample());
                    else if (link.Text.StartsWith("Layer")) _MainStack.Add(new Samples.LayerSample());
                    else if (link.Text.StartsWith("Label")) _MainStack.Add(new Samples.LabelSample());
                    else if (link.Text.StartsWith("Stack")) _MainStack.Add(new Samples.StackSample());
                    else if (link.Text.StartsWith("Panel")) _MainStack.Add(new Samples.PanelSample());
                    else if (link.Text.StartsWith("Modal")) _MainStack.Add(new Samples.ModalSample());
                    else if (link.Text.StartsWith("Dialog")) _MainStack.Add(new Samples.DialogSample());
                })*/
            .Links(
                NavLink("Basic Inputs").Expanded().Links(
                    NavLink("Button").OnSelected((s, e)             => Show(new Samples.ButtonSample())).Selected(),
                    NavLink("CheckBox").OnSelected((s, e)           => Show(new Samples.CheckBoxSample())),
                    NavLink("ChoiceGroup").OnSelected((s, e)        => Show(new Samples.ChoiceGroupSample())),
                    NavLink("Label").OnSelected((s, e)              => Show(new Samples.LabelSample())),
                    NavLink("TextBox").OnSelected((s, e)            => Show(new Samples.TextBoxSample())),
                    NavLink("Toggle").OnSelected((s, e)             => Show(new Samples.ToggleSample()))
                ),
                NavLink("Surfaces").Expanded().Links(
                    NavLink("Dialog").OnSelected((s, e)             => Show(new Samples.DialogSample())),
                    NavLink("Modal").OnSelected((s, e)              => Show(new Samples.ModalSample())),
                    NavLink("Panel").OnSelected((s, e)              => Show(new Samples.PanelSample()))
                ),
                NavLink("Utilities").Expanded().Links(
                    NavLink("Layer").OnSelected((s, e)              => Show(new Samples.LayerSample())),
                    NavLink("Stack").OnSelected((s, e)              => Show(new Samples.StackSample())),
                    NavLink("TextBlock").OnSelected((s, e)          => Show(new Samples.TextBlockSample())),
                    NavLink("Validator").OnSelected((s, e)          => Show(new Samples.ValidatorSample())),
                    NavLink("Pivot").OnSelected((s, e)              => Show(new Samples.PivotSample()))
                ),
                NavLink("Nav Links").Expanded().Links(
                    NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                    NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4")))),
                    NavLink("Level 1").Links(NavLink("Level 2").Links(NavLink("Level 3").Links(NavLink("Level 4"))))
                ),
                NavLink("Deprecated Samples").Links(
                    NavLink("Stack Sample").OnSelected((s, e)       => Show(StackSample())),
                    NavLink("Buttons Sample").OnSelected((s, e)     => Show(ButtonSample())),
                    NavLink("TextBox Sample").OnSelected((s, e)     => Show(TextBoxSample())),
                    NavLink("CheckBox Sample").OnSelected((s, e)    => Show(CheckBoxSample())),
                    NavLink("Toggle Sample").OnSelected((s, e)      => Show(ToggleSample())),
                    NavLink("ChoiceGroup Sample").OnSelected((s, e) => Show(ChoiceGroupSample())),
                    NavLink("Slider Sample").OnSelected((s, e)      => Show(SliderSample())),
                    NavLink("Layer Sample").OnSelected((s, e)       => Show(LayerSample())),
                    NavLink("Panel Sample").OnSelected((s, e)       => Show(PanelSample()))
                )
            );
        }

        private static void Show(IComponent component)
        {
            _MainStack.Clear();
            _MainStack.Add(component);
        }

        public static IComponent StackSample()
        {
            return Stack().Children(
                TextBlock("Item Alignment").SemiBold(),
                Stack().Children(
                    TextBlock("Auto-aligned Item").AlignAuto(),
                    TextBlock("Stretch-aligned Item").AlignStretch(),
                    TextBlock("Baseline-aligned Item").AlignBaseline(),
                    TextBlock("Start-aligned Item").AlignStart(),
                    TextBlock("Center-aligned Item").AlignCenter(),
                    TextBlock("End-aligned Item").AlignEnd()
                )
            );
        }

        public static IComponent ButtonSample()
        {
            var btn1 = Button();
            var btn2 = Button();
            var btnStack = Stack();
            var btnStack2 = Stack();
            var addNameTextBox = TextBox();
            var addIconTextBox = TextBox();

            return Stack().AlignCenter().Children(
                        TextBlock("Buttons in Stack").SemiBold(),
                        btnStack.Horizontal().Children(
                            Button().Text("Horizontal").OnClicked((b, e) => btnStack.Horizontal()),
                            Button().Text("Vertical").OnClicked((b, e) => btnStack.Vertical()),
                            Button().Text("Enable/Disable Next Buttons").OnClicked((b, e) => { btn1.IsEnabled = !btn1.IsEnabled; btn2.IsEnabled = !btn2.IsEnabled; }),
                            btn1.Text("Button 1").Icon("far fa-plus").Disabled().Primary(),
                            btn2.Text("Button 2").Icon("far fa-minus").Disabled()
                        ),

                        TextBlock("Add Button Sample").SemiBold(),
                        btnStack2.Horizontal(),
                        Stack(StackOrientation.Horizontal).Children(
                            Stack().Children(TextBlock("Name:").XSmall(), addNameTextBox.Text("Button Name")),
                            Stack().Children(TextBlock("Icon:").XSmall(), addIconTextBox.Text("far fa-plus")),
                            Button("Add Button").HeightStretch().Primary().OnClicked((s, r) => btnStack2.Add(
                                    Button(addNameTextBox.Text).Icon(addIconTextBox.Text).OnClicked((e, b) => alert($"\"{b.Text}\" clicked!")))
                                ),
                            Button("Clear buttons").HeightStretch().OnClicked((e, b) => btnStack2.Clear())
                        )
                    );
        }

        public static IComponent TextBoxSample()
        {
            var txf = TextBox();
            var txt = TextBlock();
            var errorText = TextBlock();

            return Stack().Children(
                    TextBlock("Text Box Sample").SemiBold(),
                    Stack().Horizontal().Children(
                        Stack().Children(
                            TextBlock("Enter text:").XSmall(),
                            txf.Text("SomeText").OnInputed((t, e) => txt.Text(e.Text))
                        ),
                        Button().Text("Clear").OnClicked((b, e) => txf.Text(string.Empty)),
                        Button().Text("Change Size").OnClicked((b, e) =>
                        {
                            if (txt.Size == TextSize.SmallPlus)
                            {
                                txt.Size = TextSize.Tiny;
                                txt.Weight = TextWeight.Bold;
                            }
                            else
                            {
                                txt.Size = TextSize.SmallPlus;
                                txt.Weight = TextWeight.Regular;
                            }
                        })
                    ),
                    Stack(StackOrientation.Horizontal).Children(
                        TextBlock("Entered text:").Small(),
                        txt.Text("Some Text").SmallPlus()
                    ),
                    TextBlock("Standard validation error sample").SemiBold(),
                    Stack(StackOrientation.Horizontal).Children(
                        TextBlock("Enter \"Hello\":").Small(),
                        TextBox().OnInputed((e, s) =>
                        {
                            s.IsInvalid = s.Text != "Hello";
                            s.Error = s.IsInvalid ? "Please enter \"Hello\"" : "";
                        })
                    ),
                    TextBlock("Custom validation error sample").SemiBold(),
                    Stack(StackOrientation.Horizontal).Children(
                        TextBlock("Enter \"Hello\":").Small(),
                        TextBox().OnInputed((e, s) =>
                        {
                            s.IsInvalid = s.Text != "Hello";
                            errorText.Text = s.IsInvalid ? "Please enter \"Hello\"" : "";
                        }),
                        errorText.Small().Invalid()
                    ),
                    TextBlock("Required TextBox Sample").SemiBold(),
                    TextBox().Required().AlignStart()
            );
        }

        public static IComponent CheckBoxSample()
        {
            return Stack().Children(
                TextBlock("CheckBox Sample").SemiBold(),
                Stack(StackOrientation.Horizontal).Children(CheckBox("Check it").Checked().OnChanged((e, s) => alert(s.IsChecked ? "Check" : "Uncheck")), CheckBox("And it"), CheckBox("And it too"), CheckBox("Disabled").Disabled(), CheckBox("Disabled and Checked").Checked().Disabled())
             );
        }

        public static IComponent ChoiceGroupSample()
        {
            return Stack().Children(ChoiceGroup("Choises Sample Vertical (Required):").OnChanged((s, e) => alert(e.SelectedOption?.Text)).Vertical().Required().Options(
                Option("Option 1"),
                Option("Option 2").Selected(),
                Option("Option 3").Disabled(),
                Option("Option 4")
            ).AlignStart(), ChoiceGroup("Choises Sample Horizontal:").Horizontal().Options(
                Option("Option 1"),
                Option("Option 2"),
                Option("Option 3").Disabled(),
                Option("Option 4").Selected()
            ));
        }

        public static IComponent ToggleSample()
        {
            return Stack().Children(
                TextBlock("Toggle Sample").SemiBold(),
                Stack(StackOrientation.Horizontal).Children(Toggle("Check it").Checked().OnChanged((e, s) => alert(s.IsChecked ? "Check" : "Uncheck")), Toggle("And it"), Toggle("And it too"), Toggle("Disabled").Disabled(), Toggle("Disabled and Checked").Checked().Disabled(), Toggle(), Toggle(), Toggle())
            );
        }

        public static IComponent SliderSample()
        {
            return Stack().Children(
                TextBlock("Slider Sample").SemiBold(),
                Stack(StackOrientation.Horizontal).Children(Slider(), Slider(), Slider(50).Disabled()),
                TextBlock("Vertical Slider Sample").SemiBold(),
                Stack(StackOrientation.Horizontal).Children(Slider().Vertical(), Slider().Vertical(), Slider(50).Vertical().Disabled())
            );
        }

        public static IComponent LayerSample()
        {
            var layer1 = Layer();
            //var layer2 = Layer();
            //var htmlTest = HtmlUtil.Div(HtmlAttributes._("", text: "HTML Test", styles: (s) =>
            // {
            //     s.backgroundColor = "blue";
            //     s.color = "white";
            //     s.width = "100%";
            //     s.visibility = "visible";
            // }));
            var layerHost = LayerHost();

            return Stack().Children(
                TextBlock("Layer Sample").SemiBold(),
                layer1.Content(Stack(StackOrientation.Horizontal).Children(Toggle(), Toggle(), Toggle())),
                Toggle("Toggle Component Layer").OnChanged((e, t) => layer1.IsVisible = t.IsChecked),
                //layer2.Content(htmlTest),
                //Toggle("Toggle HTML Layer").OnChanged((e, t) => layer2.IsVisible = t.IsChecked),
                Toggle("Show on Host").OnChanged((e, t) => layer1.Host =/* layer2.Host = */t.IsChecked ? layerHost : null),
                layerHost
            );
        }

        public static IComponent PanelSample()
        {
            var panel = Panel();

            return Stack().Children(
                TextBlock("Panel Sample").SemiBold(),
                panel.LightDismiss().Content(Stack(StackOrientation.Horizontal).Children(Toggle(), Toggle(), Toggle()))
                    .Footer(Stack().Horizontal().Children(Button("Button 1").Primary(), Button("Button 2"))),
                Button("Toggle Panel").OnClicked((e, t) => panel.IsVisible = true));
        }
    }
}