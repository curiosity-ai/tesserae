using Tesserae.Components;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ColorPickerSample : IComponent
    {
        private readonly IComponent _content;

        public ColorPickerSample()
        {
            _content = SectionStack()
            .Title(SampleHeader(nameof(ColorPickerSample)))
            .Section(Stack().Children(
                SampleTitle("Overview"),
                TextBlock("The ColorPicker allows users to pick a color from a native browser widget. Unless specified, black is the default color upon render of the component"), Link("https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input/color", "Please see here for further information.")))
            .Section(Stack().Children(
                SampleTitle("Usage"),
                TextBlock("Basic ColorPicker").Medium(),
                Stack().Width(40.percent()).Children(
                    Label("Standard").SetContent(HStack().Stretch().Children(ColorPicker().Width(10.percent()).Var(out var colorPicker1), Button().SetText("Click me!").Var(out var button1))),
                    Label("With preset color").SetContent(ColorPicker(Color.FromString("#0078d4"))).Width(10.percent()),
                    Label("Disabled").Disabled().SetContent(ColorPicker().Disabled()).Width(10.percent()),
                    Label("Required").Required().SetContent(ColorPicker()).Width(10.percent()), ColorPicker().Required().Width(10.percent()),
                    Label("With error message").SetContent(ColorPicker().Error("Error message").IsInvalid()).Width(10.percent()),
                    Label("With validation for light color").SetContent(ColorPicker().Validation(Validation.LightColor)).Width(10.percent()),
                    Label("With validation for dark color").SetContent(ColorPicker().Validation(Validation.DarkColor)).Width(10.percent()))));

            colorPicker1.OnChange((_, __) => button1.Background = colorPicker1.Text);
            button1.OnClick((_, __) => window.alert($"{colorPicker1.Text}, {colorPicker1.Color.ToHex()}"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
