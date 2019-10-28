using Tesserae;
using Tesserae.Components;

using static Retyped.dom;
using static Tesserae.Components.UI;

namespace Tesserae.Tests
{
    public class App
    {
        public static void Main()
        {
            Require.LoadStyleAsync("css/curiosity-bootstrap.css", "css/fontawesome-all.min.css", "css/prototype-ui.css");
            Require.LoadScriptAsync(OnStartUp, null, "css/curiosity-bootstrap.css");
        }

        private static void OnStartUp()
        {
            var btn1 = Button();
            var btn2 = Button();
            var btnStack = Stack();
            var btnStack2 = Stack();
            var txf = TextBox();
            var txt = TextBlock();
            var addNameTextBox = TextBox();
            var addIconTextBox = TextBox();

            var stack = Stack().Children(
                TextBlock("Buttons in Stack").SemiBold(),
                btnStack.Horizontal().Children(
                    Button().Text("Horizontal").OnClicked((b, e) => btnStack.Horizontal()),
                    Button().Text("Vertical").OnClicked((b, e) => btnStack.Vertical()),
                    Button().Text("Enable/Disable Next Buttons").OnClicked((b, e) => { btn1.IsEnabled = !btn1.IsEnabled; btn2.IsEnabled = !btn2.IsEnabled; }),
                    btn1.Text("Button 1").Icon("far fa-plus").Disabled().Primary(),
                    btn2.Text("Button 2").Icon("far fa-minus").Disabled()
                ),

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

                TextBlock("Add Button Sample").SemiBold(),
                btnStack2.Horizontal(),
                Stack(StackOrientation.Horizontal).Children(
                    Stack().Children(TextBlock("Name:").XSmall(), addNameTextBox.Text("Button Name")),
                    Stack().Children(TextBlock("Icon:").XSmall(), addIconTextBox.Text("far fa-plus")),
                    Button("Add Button").Primary().OnClicked((s, r) => btnStack2.Add(
                            Button(addNameTextBox.Text).Icon(addIconTextBox.Text).OnClicked((e, b) => alert($"\"{b.Text}\" clicked!")))
                        ),
                    Button("Clear buttons").OnClicked((e, b) => btnStack2.Clear())
                ),
                TextBlock("CheckBox Sample").SemiBold(),
                Stack(StackOrientation.Horizontal).Children(CheckBox("Check it").Checked().OnChanged((e, s) => alert(s.IsChecked ? "Check" : "Uncheck")), CheckBox("And it"), CheckBox("And it too"), CheckBox("Disabled").Disabled(), CheckBox("Disabled and Checked").Checked().Disabled())
            );
            document.body.appendChild(stack.Render());
        }
    }
}