using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 10, Icon = UIcons.Palette)]
    public class ColorPaletteSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ColorPaletteSample()
        {
            var selectedColor = new SettableObservable<string>("#0078d4");

            var palette = ColorPalette()
                .Swatches(
                    ("Blue",    "#0078d4"),
                    ("Purple",  "#8764b8"),
                    ("Magenta", "#e3008c"),
                    ("Red",     "#d13438"),
                    ("Orange",  "#ca5010"),
                    ("Yellow",  "#ffaa44"),
                    ("Green",   "#107c10"),
                    ("Teal",    "#038387"),
                    ("Neutral", "#737373"),
                    ("Black",   "#000000")
                )
                .SetValue("#0078d4")
                .WithCustomColor()
                .OnChange(c =>
                {
                    selectedColor.Value = c;
                    Toast().Information($"Selected: {c}");
                });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ColorPaletteSample), UIcons.Palette, "A grid of named colour swatches")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ColorPalette displays a set of named swatches for picking from a predefined brand or theme palette. It's distinct from the raw ColorPicker which allows any colour — use ColorPalette when the set of valid choices is known in advance."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Define swatches using your design system's named tokens. Always include an accessible label for each swatch. If custom colours are allowed, add WithCustomColor()."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Brand Palette"),
                    palette,
                    HStack().MT(12).AlignItems(ItemAlign.Center).Children(
                        TextBlock("Selected:").Small(),
                        DeferSync(selectedColor, c =>
                        {
                            var swatch = Span(_());
                            swatch.style.display         = "inline-block";
                            swatch.style.width           = "20px";
                            swatch.style.height          = "20px";
                            swatch.style.background      = c;
                            swatch.style.borderRadius    = "50%";
                            swatch.style.border          = "1px solid rgba(0,0,0,0.15)";
                            return HStack().AlignItems(ItemAlign.Center).Gap(8).Children(
                                Raw(swatch),
                                TextBlock(c).SemiBold().Small()
                            );
                        })
                    ),
                    SampleSubTitle("Semantic Colours"),
                    ColorPalette()
                        .Swatches(
                            ("Success", Theme.Success.Background),
                            ("Warning", Theme.Warning.Background),
                            ("Danger",  Theme.Danger.Background),
                            ("Info",    Theme.Primary.Background)
                        )
                        .OnChange(c => Toast().Information($"Semantic color: {c}"))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
