﻿using Tesserae;
using Tesserae.Components;
using Tesserae.HTML;
using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests
{
    public class App
    {
        private static Stack _MainStack;

        public static void Main()
        {
            Require.LoadStyleAsync("css/curiosity-bootstrap.css", "css/fontawesome-all.min.css", "css/prototype-ui.css");
            Require.LoadScriptAsync(OnStartUp, null, "css/curiosity-bootstrap.css");
        }

        private static void OnStartUp()
        {
            _MainStack = Stack();
            var samples = Stack(StackOrientation.Horizontal).Children(MainNav().WidthPixels(300), _MainStack.WidthStretch());
            document.body.appendChild(samples.Render());
        }

        public static Nav MainNav()
        {
            return Nav().OnChanged((e, s) =>
                {
                    var link = e as NavLink;
                    _MainStack.Clear();
                    if (link.Text.StartsWith("Stack")) _MainStack.Add(StackSample());
                    else if (link.Text.StartsWith("Buttons")) _MainStack.Add(ButtonSample());
                    else if (link.Text.StartsWith("TextBox")) _MainStack.Add(TextBoxSample());
                    else if (link.Text.StartsWith("CheckBox")) _MainStack.Add(CheckBoxSample());
                    else if (link.Text.StartsWith("Toggle")) _MainStack.Add(ToggleSample());
                    else if (link.Text.StartsWith("ChoiceGroup")) _MainStack.Add(ChoiceGroupSample());
                    else if (link.Text.StartsWith("Slider")) _MainStack.Add(SliderSample());
                    else if (link.Text.StartsWith("Layer")) _MainStack.Add(LayerSample());
                })
            .Links(
                NavLink("Sample 1").Expanded().Links(
                    NavLink("Stack Sample"),
                    NavLink("Buttons Sample"),
                    NavLink("TextBox Sample"),
                    NavLink("CheckBox Sample"),
                    NavLink("Toggle Sample"),
                    NavLink("ChoiceGroup Sample"),
                    NavLink("Slider Sample"),
                    NavLink("Layer Sample")
                    ),
                NavLink("Sample 2").Selected()
            );
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

            return Stack().Children(
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
                            Button("Add Button").Primary().OnClicked((s, r) => btnStack2.Add(
                                    Button(addNameTextBox.Text).Icon(addIconTextBox.Text).OnClicked((e, b) => alert($"\"{b.Text}\" clicked!")))
                                ),
                            Button("Clear buttons").OnClicked((e, b) => btnStack2.Clear())
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
                            s.ErrorText = s.IsInvalid ? "Please enter \"Hello\"" : "";
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
            return Stack().Children(ChoiceGroup("Choises Sample Vertical (Required):").Vertical().Required().Choices(
                Choice("Option 1"),
                Choice("Option 2").Selected(),
                Choice("Option 3").Disabled(),
                Choice("Option 4")
            ).AlignStart(), ChoiceGroup("Choises Sample Horizontal:").Horizontal().Choices(
                Choice("Option 1"),
                Choice("Option 2"),
                Choice("Option 3").Disabled(),
                Choice("Option 4").Selected()
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
            var layer2 = Layer();
            var htmlTest = HtmlUtil.Div(HtmlAttributes._("", text: "HTML Test", styles: (s) =>
             {
                 s.backgroundColor = "blue";
                 s.color = "white";
                 s.width = "100%";
                 s.visibility = "visible";
             }));
            var layerHost = LayerHost();

            return Stack().Children(
                TextBlock("Layer Sample").SemiBold(),
                layer1.Content(Stack(StackOrientation.Horizontal).Children(Toggle(), Toggle(), Toggle())),
                Toggle("Toggle Component Layer").OnChanged((e, t) => layer1.IsVisible = t.IsChecked),
                layer2.Content(htmlTest),
                Toggle("Toggle HTML Layer").OnChanged((e, t) => layer2.IsVisible = t.IsChecked),
                Toggle("Show on Host").OnChanged((e, t) => layer1.Host = layer2.Host = t.IsChecked ? layerHost : null),
                layerHost
            );
        }
    }
}