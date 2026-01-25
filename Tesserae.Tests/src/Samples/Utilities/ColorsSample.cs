using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 20, Icon = UIcons.Palette)]
    public class ColorsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ColorsSample()
        {
            var allColors = new (string Name, string Value)[]
            {
                (nameof(Theme.Colors.Lime100), Theme.Colors.Lime100),
                (nameof(Theme.Colors.Lime200), Theme.Colors.Lime200),
                (nameof(Theme.Colors.Lime250), Theme.Colors.Lime250),
                (nameof(Theme.Colors.Lime300), Theme.Colors.Lime300),
                (nameof(Theme.Colors.Lime400), Theme.Colors.Lime400),
                (nameof(Theme.Colors.Lime500), Theme.Colors.Lime500),
                (nameof(Theme.Colors.Lime600), Theme.Colors.Lime600),
                (nameof(Theme.Colors.Lime700), Theme.Colors.Lime700),
                (nameof(Theme.Colors.Lime800), Theme.Colors.Lime800),
                (nameof(Theme.Colors.Lime850), Theme.Colors.Lime850),
                (nameof(Theme.Colors.Lime900), Theme.Colors.Lime900),
                (nameof(Theme.Colors.Lime1000), Theme.Colors.Lime1000),
                (nameof(Theme.Colors.Red100), Theme.Colors.Red100),
                (nameof(Theme.Colors.Red200), Theme.Colors.Red200),
                (nameof(Theme.Colors.Red250), Theme.Colors.Red250),
                (nameof(Theme.Colors.Red300), Theme.Colors.Red300),
                (nameof(Theme.Colors.Red400), Theme.Colors.Red400),
                (nameof(Theme.Colors.Red500), Theme.Colors.Red500),
                (nameof(Theme.Colors.Red600), Theme.Colors.Red600),
                (nameof(Theme.Colors.Red700), Theme.Colors.Red700),
                (nameof(Theme.Colors.Red800), Theme.Colors.Red800),
                (nameof(Theme.Colors.Red850), Theme.Colors.Red850),
                (nameof(Theme.Colors.Red900), Theme.Colors.Red900),
                (nameof(Theme.Colors.Red1000), Theme.Colors.Red1000),
                (nameof(Theme.Colors.Orange100), Theme.Colors.Orange100),
                (nameof(Theme.Colors.Orange200), Theme.Colors.Orange200),
                (nameof(Theme.Colors.Orange250), Theme.Colors.Orange250),
                (nameof(Theme.Colors.Orange300), Theme.Colors.Orange300),
                (nameof(Theme.Colors.Orange400), Theme.Colors.Orange400),
                (nameof(Theme.Colors.Orange500), Theme.Colors.Orange500),
                (nameof(Theme.Colors.Orange600), Theme.Colors.Orange600),
                (nameof(Theme.Colors.Orange700), Theme.Colors.Orange700),
                (nameof(Theme.Colors.Orange800), Theme.Colors.Orange800),
                (nameof(Theme.Colors.Orange850), Theme.Colors.Orange850),
                (nameof(Theme.Colors.Orange900), Theme.Colors.Orange900),
                (nameof(Theme.Colors.Orange1000), Theme.Colors.Orange1000),
                (nameof(Theme.Colors.Yellow100), Theme.Colors.Yellow100),
                (nameof(Theme.Colors.Yellow200), Theme.Colors.Yellow200),
                (nameof(Theme.Colors.Yellow250), Theme.Colors.Yellow250),
                (nameof(Theme.Colors.Yellow300), Theme.Colors.Yellow300),
                (nameof(Theme.Colors.Yellow400), Theme.Colors.Yellow400),
                (nameof(Theme.Colors.Yellow500), Theme.Colors.Yellow500),
                (nameof(Theme.Colors.Yellow600), Theme.Colors.Yellow600),
                (nameof(Theme.Colors.Yellow700), Theme.Colors.Yellow700),
                (nameof(Theme.Colors.Yellow800), Theme.Colors.Yellow800),
                (nameof(Theme.Colors.Yellow850), Theme.Colors.Yellow850),
                (nameof(Theme.Colors.Yellow900), Theme.Colors.Yellow900),
                (nameof(Theme.Colors.Yellow1000), Theme.Colors.Yellow1000),
                (nameof(Theme.Colors.Green100), Theme.Colors.Green100),
                (nameof(Theme.Colors.Green200), Theme.Colors.Green200),
                (nameof(Theme.Colors.Green250), Theme.Colors.Green250),
                (nameof(Theme.Colors.Green300), Theme.Colors.Green300),
                (nameof(Theme.Colors.Green400), Theme.Colors.Green400),
                (nameof(Theme.Colors.Green500), Theme.Colors.Green500),
                (nameof(Theme.Colors.Green600), Theme.Colors.Green600),
                (nameof(Theme.Colors.Green700), Theme.Colors.Green700),
                (nameof(Theme.Colors.Green800), Theme.Colors.Green800),
                (nameof(Theme.Colors.Green850), Theme.Colors.Green850),
                (nameof(Theme.Colors.Green900), Theme.Colors.Green900),
                (nameof(Theme.Colors.Green1000), Theme.Colors.Green1000),
                (nameof(Theme.Colors.Teal100), Theme.Colors.Teal100),
                (nameof(Theme.Colors.Teal200), Theme.Colors.Teal200),
                (nameof(Theme.Colors.Teal250), Theme.Colors.Teal250),
                (nameof(Theme.Colors.Teal300), Theme.Colors.Teal300),
                (nameof(Theme.Colors.Teal400), Theme.Colors.Teal400),
                (nameof(Theme.Colors.Teal500), Theme.Colors.Teal500),
                (nameof(Theme.Colors.Teal600), Theme.Colors.Teal600),
                (nameof(Theme.Colors.Teal700), Theme.Colors.Teal700),
                (nameof(Theme.Colors.Teal800), Theme.Colors.Teal800),
                (nameof(Theme.Colors.Teal850), Theme.Colors.Teal850),
                (nameof(Theme.Colors.Teal900), Theme.Colors.Teal900),
                (nameof(Theme.Colors.Teal1000), Theme.Colors.Teal1000),
                (nameof(Theme.Colors.Blue100), Theme.Colors.Blue100),
                (nameof(Theme.Colors.Blue200), Theme.Colors.Blue200),
                (nameof(Theme.Colors.Blue250), Theme.Colors.Blue250),
                (nameof(Theme.Colors.Blue300), Theme.Colors.Blue300),
                (nameof(Theme.Colors.Blue400), Theme.Colors.Blue400),
                (nameof(Theme.Colors.Blue500), Theme.Colors.Blue500),
                (nameof(Theme.Colors.Blue600), Theme.Colors.Blue600),
                (nameof(Theme.Colors.Blue700), Theme.Colors.Blue700),
                (nameof(Theme.Colors.Blue800), Theme.Colors.Blue800),
                (nameof(Theme.Colors.Blue850), Theme.Colors.Blue850),
                (nameof(Theme.Colors.Blue900), Theme.Colors.Blue900),
                (nameof(Theme.Colors.Blue1000), Theme.Colors.Blue1000),
                (nameof(Theme.Colors.Purple100), Theme.Colors.Purple100),
                (nameof(Theme.Colors.Purple200), Theme.Colors.Purple200),
                (nameof(Theme.Colors.Purple250), Theme.Colors.Purple250),
                (nameof(Theme.Colors.Purple300), Theme.Colors.Purple300),
                (nameof(Theme.Colors.Purple400), Theme.Colors.Purple400),
                (nameof(Theme.Colors.Purple500), Theme.Colors.Purple500),
                (nameof(Theme.Colors.Purple600), Theme.Colors.Purple600),
                (nameof(Theme.Colors.Purple700), Theme.Colors.Purple700),
                (nameof(Theme.Colors.Purple800), Theme.Colors.Purple800),
                (nameof(Theme.Colors.Purple850), Theme.Colors.Purple850),
                (nameof(Theme.Colors.Purple900), Theme.Colors.Purple900),
                (nameof(Theme.Colors.Purple1000), Theme.Colors.Purple1000),
                (nameof(Theme.Colors.Magenta100), Theme.Colors.Magenta100),
                (nameof(Theme.Colors.Magenta200), Theme.Colors.Magenta200),
                (nameof(Theme.Colors.Magenta250), Theme.Colors.Magenta250),
                (nameof(Theme.Colors.Magenta300), Theme.Colors.Magenta300),
                (nameof(Theme.Colors.Magenta400), Theme.Colors.Magenta400),
                (nameof(Theme.Colors.Magenta500), Theme.Colors.Magenta500),
                (nameof(Theme.Colors.Magenta600), Theme.Colors.Magenta600),
                (nameof(Theme.Colors.Magenta700), Theme.Colors.Magenta700),
                (nameof(Theme.Colors.Magenta800), Theme.Colors.Magenta800),
                (nameof(Theme.Colors.Magenta850), Theme.Colors.Magenta850),
                (nameof(Theme.Colors.Magenta900), Theme.Colors.Magenta900),
                (nameof(Theme.Colors.Magenta1000), Theme.Colors.Magenta1000),
                (nameof(Theme.Colors.Neutral0), Theme.Colors.Neutral0),
                (nameof(Theme.Colors.Neutral100), Theme.Colors.Neutral100),
                (nameof(Theme.Colors.Neutral200), Theme.Colors.Neutral200),
                (nameof(Theme.Colors.Neutral300), Theme.Colors.Neutral300),
                (nameof(Theme.Colors.Neutral400), Theme.Colors.Neutral400),
                (nameof(Theme.Colors.Neutral500), Theme.Colors.Neutral500),
                (nameof(Theme.Colors.Neutral600), Theme.Colors.Neutral600),
                (nameof(Theme.Colors.Neutral700), Theme.Colors.Neutral700),
                (nameof(Theme.Colors.Neutral800), Theme.Colors.Neutral800),
                (nameof(Theme.Colors.Neutral900), Theme.Colors.Neutral900),
                (nameof(Theme.Colors.Neutral1000), Theme.Colors.Neutral1000),
                (nameof(Theme.Colors.Neutral1100), Theme.Colors.Neutral1100),
                (nameof(Theme.Colors.DarkNeutral0), Theme.Colors.DarkNeutral0),
                (nameof(Theme.Colors.DarkNeutral100), Theme.Colors.DarkNeutral100),
                (nameof(Theme.Colors.DarkNeutral200), Theme.Colors.DarkNeutral200),
                (nameof(Theme.Colors.DarkNeutral300), Theme.Colors.DarkNeutral300),
                (nameof(Theme.Colors.DarkNeutral400), Theme.Colors.DarkNeutral400),
                (nameof(Theme.Colors.DarkNeutral500), Theme.Colors.DarkNeutral500),
                (nameof(Theme.Colors.DarkNeutral600), Theme.Colors.DarkNeutral600),
                (nameof(Theme.Colors.DarkNeutral700), Theme.Colors.DarkNeutral700),
                (nameof(Theme.Colors.DarkNeutral800), Theme.Colors.DarkNeutral800),
                (nameof(Theme.Colors.DarkNeutral900), Theme.Colors.DarkNeutral900),
                (nameof(Theme.Colors.DarkNeutral1000), Theme.Colors.DarkNeutral1000),
                (nameof(Theme.Colors.DarkNeutral1100), Theme.Colors.DarkNeutral1100),
            };

            //TODO: Add alpha colors over an image or background
            //(nameof(Theme.Colors.Neutral100Alpha), Theme.Colors.Neutral100Alpha),
            //(nameof(Theme.Colors.Neutral200Alpha), Theme.Colors.Neutral200Alpha),
            //(nameof(Theme.Colors.Neutral300Alpha), Theme.Colors.Neutral300Alpha),
            //(nameof(Theme.Colors.Neutral400Alpha), Theme.Colors.Neutral400Alpha),
            //(nameof(Theme.Colors.Neutral500Alpha), Theme.Colors.Neutral500Alpha),

            //(nameof(Theme.Colors.DarkNeutral100Alpha), Theme.Colors.DarkNeutral100Alpha),
            //(nameof(Theme.Colors.DarkNeutral200Alpha), Theme.Colors.DarkNeutral200Alpha),
            //(nameof(Theme.Colors.DarkNeutral300Alpha), Theme.Colors.DarkNeutral300Alpha),
            //(nameof(Theme.Colors.DarkNeutral400Alpha), Theme.Colors.DarkNeutral400Alpha),
            //(nameof(Theme.Colors.DarkNeutral500Alpha), Theme.Colors.DarkNeutral500Alpha),

            var groups = allColors.GroupBy(GetGroupName);
            
            var grid = Grid(1.fr(), 1.fr(), 1.fr()).Gap(8.px());

            void Render()
            {
                grid.Children(
                        groups.OrderBy(g => g.Key == "Neutral" ? 0 : g.Key == "DarkNeutral" ? 1 : 2).ThenBy(g => g.Key)
                            .Select(g =>
                                VStack().Children(
                                    SampleTitle(g.Key),
                                    VStack().Children(g.Select(c => RenderColorStack(c.Name, c.Value)).ToArray())
                                )
                        ).ToArray()
                    );
            }
            Render();
            
            Theme.OnThemeChanged += () => window.setTimeout(_ => Render(), 1);

            _content = SectionStack()
                .Title(SampleHeader(nameof(ColorsSample)))
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Tesserae provides a comprehensive set of predefined colors that are part of the theme. These colors are accessible via the 'Theme.Colors' class and are designed to provide a consistent visual language across the application, with support for both light and dark modes.")))
                .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Prefer using these predefined colors over hardcoded hex values to ensure your application remains consistent with the theme. Use specific color ranges (e.g., Red for errors, Green for success) to convey semantic meaning. Click on any color name below to copy its C# constant name, or use the icons to copy its RGB or Hex values.")))
                .Section(Stack().Children(
                    SampleTitle("Usage"),
                    grid));
        }

        private string GetGroupName((string Name, string Value) color)
        {
            if (color.Name.StartsWith("DarkNeutral")) return "DarkNeutral";
            if (color.Name.StartsWith("Neutral")) return "Neutral";

            return new string(color.Name.TakeWhile(char.IsLetter).ToArray());
        }

        private IComponent RenderColorStack(string colorName, string colorVar)
        {
            var color = Color.FromString(colorVar);
            var hsl= new HSLColor(color);
            var textColor = "black";
            if(hsl.Luminosity < 100)
            {
                textColor = "white";
            }
            return Stack().Children(
                HStack().NoWrap().Background(colorVar).Children(
                    Button(colorName).Foreground(textColor).NoBackground().W(10).Grow().OnClick(() =>
                    {
                        Clipboard.Copy($"Theme.Colors.{colorName}");
                    }),
                    Button().SetIcon(UIcons.Copy, color:textColor).Tooltip(color.ToRGB()).OnClick(() =>
                    {
                        Clipboard.Copy(color.ToRGB());
                    }).Tooltip($"Copy RGB Value: {color.ToRGB()}"),
                    Button().SetIcon(UIcons.Hashtag, color:textColor).OnClick(() =>
                    {
                        Clipboard.Copy(color.ToHex());
                    }).Tooltip($"Copy Hex Value: {color.ToHex()}")
                )
            ).MB(8);
        }

        public HTMLElement Render() => _content.Render();
    }
}
