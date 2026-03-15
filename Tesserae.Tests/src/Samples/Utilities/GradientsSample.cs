using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 11, Icon = UIcons.Palette)]
    public class GradientsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public GradientsSample()
        {
            var allGradients = new (string Name, string Value)[]
            {
                (nameof(Theme.Gradients.Lime), Theme.Gradients.Lime),
                (nameof(Theme.Gradients.Red), Theme.Gradients.Red),
                (nameof(Theme.Gradients.Orange), Theme.Gradients.Orange),
                (nameof(Theme.Gradients.Yellow), Theme.Gradients.Yellow),
                (nameof(Theme.Gradients.Green), Theme.Gradients.Green),
                (nameof(Theme.Gradients.Teal), Theme.Gradients.Teal),
                (nameof(Theme.Gradients.Blue), Theme.Gradients.Blue),
                (nameof(Theme.Gradients.Purple), Theme.Gradients.Purple),
                (nameof(Theme.Gradients.Magenta), Theme.Gradients.Magenta),
                (nameof(Theme.Gradients.AI), Theme.Gradients.AI),
            };

            var grid = Grid(1.fr(), 1.fr(), 1.fr()).Gap(8.px());

            void Render()
            {
                grid.Children(
                    VStack().Children(
                        SampleTitle("Linear Gradients"),
                        VStack().Children(allGradients.Select(g => RenderGradientStack(g.Name, g.Value)).ToArray())
                    )
                );
            }
            Render();

            Theme.OnThemeChanged += () => window.setTimeout(_ => Render(), 1);

            _content = SectionStack()
                .Title(SampleHeader(nameof(GradientsSample)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Tesserae provides a comprehensive set of predefined gradients that are part of the theme. These gradients are accessible via the 'Theme.Gradients' class and are designed to provide a consistent visual language across the application, with support for both light and dark modes.")))
                .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Prefer using these predefined gradients over hardcoded linear-gradient functions to ensure your application remains consistent with the theme. Click on any gradient name below to copy its C# constant name.")))
                .Section(Stack().Children(
                    SampleTitle("Usage"),
                    grid));
        }

        private IComponent RenderGradientStack(string gradientName, string gradientVar)
        {
            var textColor = "white"; // Can be adjusted dynamically if needed, but white usually contrasts well with medium/dark gradients
            return Stack().Children(
                HStack().NoWrap().Background(gradientVar).Children(
                    Button(gradientName).Foreground(textColor).NoBackground().W(10).Grow().OnClick(() =>
                    {
                        Clipboard.Copy($"Theme.Gradients.{gradientName}");
                    })
                )
            ).MB(8);
        }

        public HTMLElement Render() => _content.Render();
    }
}