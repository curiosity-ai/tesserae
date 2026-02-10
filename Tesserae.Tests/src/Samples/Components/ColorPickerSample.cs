using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.Palette)]
    public class ColorPickerSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public ColorPickerSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ColorPickerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The ColorPicker allows users to select a color using the browser's native color selection widget. It returns the selected color as both a hex string and a Color object."),
                    TextBlock("This component is useful for personalization settings, drawing applications, or any interface where color customization is required.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use the ColorPicker when users need to select a precise color that isn't covered by a predefined set of options. If you only need a few specific colors, consider using a ChoiceGroup with custom styling or a Dropdown instead. Always provide a default color that makes sense for the context. Ensure the picked color is validated if certain constraints apply (e.g., must be a dark color for text readability).")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic ColorPicker"),
                    VStack().Children(
                        Label("Pick a color").SetContent(
                            HStack().Children(
                                ColorPicker().Var(out var cp1).Width(50.px()),
                                Button("Apply Color").Var(out var btn1)
                            )
                        ),
                        Label("With default color (Blue)").SetContent(ColorPicker(Color.FromString("#0078d4")).Width(50.px())),
                        Label("Disabled state").Disabled().SetContent(ColorPicker().Disabled().Width(50.px()))
                    ),
                    SampleSubTitle("Validation"),
                    VStack().Children(
                        Label("Light color required").SetContent(ColorPicker().Validation(Validation.LightColor).Width(50.px())),
                        Label("Dark color required").SetContent(ColorPicker().Validation(Validation.DarkColor).Width(50.px()))
                    ),
                    SampleSubTitle("Interactive Example"),
                    TextBlock("Changing the color picker below will update the button's background.")
                ));

            cp1.OnChange((_, __) => btn1.Background = cp1.Text);
            btn1.OnClick((_, __) => Toast().Information($"Selected Color: {cp1.Text} (Hex: {cp1.Color.ToHex()})"));
        }

        public HTMLElement Render() => _content.Render();
    }
}
