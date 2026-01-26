using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.Cursor)]
    public class ButtonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ButtonSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ButtonSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Buttons are best used to enable a user to commit a change or complete steps in a task. They are typically found inside forms, dialogs, panels or pages. An example of their usage is confirming the deletion of a file in a confirmation dialog."),
                    TextBlock("When considering their place in a layout, contemplate the order in which a user will flow through the UI. As an example, in a form, the individual will need to read and interact with the form fields before submiting the form. Therefore, as a general rule, the button should be placed at the bottom of the UI container (a dialog, panel, or page) which holds the related UI elements.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Buttons should clearly communicate what will happen when the user clicks them. Use concise, specific, self-explanatory labels, usually a single word. Default buttons should always perform safe operations. For example, a default button should never delete. Use only a single line of text in the label of the button. Expose only one or two buttons to the user at a time. Show only one primary button that inherits theme color at rest state. \"Submit\", \"OK\", and \"Apply\" buttons should always be styled as primary buttons."),
                    TextBlock("Avoid using generic labels like \"Ok\", especially in the case of an error. Do not place the default focus on a button that destroys data. Do not use a button to navigate to another place, use a link instead.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Buttons"),
                    HStack().Children(
                        Button().SetText("Standard").Tooltip("This is a standard button").OnClick(() => alert("Clicked!")),
                        Button().SetText("Primary").Tooltip("This is a primary button").Primary().OnClick(() => alert("Clicked!")),
                        Button().SetText("Link").Tooltip("This is a link button").Link().OnClick(() => alert("Clicked!"))
                    ),
                    SampleSubTitle("Icons and States"),
                    HStack().Children(
                        Button().SetText("Confirm").SetIcon(UIcons.Check).Success().OnClick(() => alert("Clicked!")),
                        Button().SetText("Delete").SetIcon(UIcons.Trash).Danger().OnClick(() => alert("Clicked!")),
                        Button().SetText("Disabled").SetIcon(UIcons.Lock).Disabled()
                    ),
                    SampleSubTitle("Loading States"),
                    HStack().Children(
                        Button().SetText("Click to Spin").OnClickSpinWhile(async () => await Task.Delay(2000)),
                        Button().SetText("With Loading Text").OnClickSpinWhile(async () => await Task.Delay(2000), "Processing..."),
                        Button().SetText("Error Simulation").OnClickSpinWhile(async () => { await Task.Delay(1000); throw new Exception("Action failed"); }, onError: (b, e) => b.SetText("Try again: " + e.Message).Danger())
                    ),
                    SampleSubTitle("Variations"),
                    HStack().Children(
                        ButtonAndIcon("Split Button", (m, i, ev) => Toast().Information("Icon clicked"), mainIcon: UIcons.Rocket, secondaryIcon: UIcons.AngleDown).OnClick((b, _) => Toast().Success("Main action")),
                        Button().SetText("No Padding").NoPadding().Primary(),
                        Button().SetText("No Border").NoBorder()
                    ),
                    SampleSubTitle("Themed Backgrounds"),
                    HStack().Children(
                        Button().SetText("Blue").Background(Theme.Colors.Blue500).OnClick(() => alert("Clicked!")),
                        Button().SetText("Lime").Background(Theme.Colors.Lime500).OnClick(() => alert("Clicked!")),
                        Button().SetText("Magenta").Background(Theme.Colors.Magenta500).OnClick(() => alert("Clicked!")),
                        Button().SetText("Yellow").Background(Theme.Colors.Yellow500).OnClick(() => alert("Clicked!"))
                    ),
                    SampleSubTitle("Rounded Buttons"),
                    HStack().Children(
                        Button().SetText("Small").Rounded(BorderRadius.Small).Primary(),
                        Button().SetText("Medium").Rounded(BorderRadius.Medium).Primary(),
                        Button().SetText("Full").Rounded(BorderRadius.Full).Primary()
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
