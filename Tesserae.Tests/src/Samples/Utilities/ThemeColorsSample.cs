using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 10, Icon = LineAwesome.Palette)]
    public class ThemeColorsSample : IComponent, ISample
    {
        private IComponent _content;


        public ThemeColorsSample()
        {
            var currentTheme = Theme.IsLight;
            var primaryLight    = new SettableObservable<Color>();
            var backgroundLight = new SettableObservable<Color>();
            var primaryDark     = new SettableObservable<Color>();
            var backgroundDark  = new SettableObservable<Color>();

            var combined = new CombinedObservable<Color, Color, Color, Color>(primaryLight, primaryDark, backgroundLight, backgroundDark);

            var cpPrimaryLight    = ColorPicker().OnInput((cp, ev) => primaryLight.Value = cp.Color);
            var cpPrimaryDark     = ColorPicker().OnInput((cp, ev) => primaryDark.Value = cp.Color);
            var cpBackgroundLight = ColorPicker().OnInput((cp, ev) => backgroundLight.Value = cp.Color);
            var cpBackgroundDark  = ColorPicker().OnInput((cp, ev) => backgroundDark.Value = cp.Color);

            Theme.Light();
            window.setTimeout((_) =>
            {

                primaryLight.Value = Color.FromString(Color.EvalVar(Theme.Primary.Background));
                backgroundLight.Value = Color.FromString(Color.EvalVar(Theme.Default.Background));
                
                Theme.Dark();
                window.setTimeout((__) =>
                {
                    primaryDark.Value = Color.FromString(Color.EvalVar(Theme.Primary.Background));
                    backgroundDark.Value = Color.FromString(Color.EvalVar(Theme.Default.Background));
                    Theme.IsLight = currentTheme;


                    cpPrimaryLight.Color    = primaryLight.Value;
                    cpPrimaryDark.Color     = primaryDark.Value;
                    cpBackgroundLight.Color = backgroundLight.Value;
                    cpBackgroundDark.Color  = backgroundDark.Value;

                    combined.ObserveFutureChanges(v =>
                    {
                        Theme.SetPrimary(v.first, v.second);
                        Theme.SetBackground(v.third, v.forth);
                    });

                }, 1);
            }, 1);

            _content = SectionStack()
               .Title(SampleHeader(nameof(ThemeColorsSample)))
               .Section(
                    Stack().Children(
                        DetailsList<ColorListItem>(
                                DetailsListColumn(title: "ThemeName", width: 120.px()),
                                DetailsListColumn(title: "Background", width: 160.px()),
                                DetailsListColumn(title: "Foreground", width: 160.px()),
                                DetailsListColumn(title: "Border", width: 160.px()),
                                DetailsListColumn(title: "BackgroundActive", width: 160.px()),
                                DetailsListColumn(title: "BackgroundHover", width: 160.px()),
                                DetailsListColumn(title: "ForegroundActive", width: 160.px()),
                                DetailsListColumn(title: "ForegroundHover", width: 160.px()))
                           .Compact()
                           .Height(500.px())
                           .WithListItems(new[]
                            {
                                new ColorListItem("Default"),
                                new ColorListItem("Primary"),
                                new ColorListItem("Secondary"),
                                new ColorListItem("Success"),
                                new ColorListItem("Danger")
                            })
                           .SortedBy("Name"),
                            Label("Primary Light").Inline()   .SetContent(cpPrimaryLight    ),
                            Label("Primary Dark").Inline()    .SetContent(cpPrimaryDark     ),
                            Label("Background Light").Inline().SetContent(cpBackgroundLight ),
                            Label("Background Dark").Inline() .SetContent(cpBackgroundDark  )
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }


        public class ColorListItem : IDetailsListItem<ColorListItem>
        {
            public string ThemeName { get; }

            public static Dictionary<string, Dictionary<string, string>> Mapping = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "Default", new Dictionary<string, string>
                    {
                        {nameof(Theme.Default.Background), Theme.Default.Background},
                        {nameof(Theme.Default.Foreground), Theme.Default.Foreground},
                        {nameof(Theme.Default.Border), Theme.Default.Border},
                        {nameof(Theme.Default.BackgroundActive), Theme.Default.BackgroundActive},
                        {nameof(Theme.Default.BackgroundHover), Theme.Default.BackgroundHover},
                        {nameof(Theme.Default.ForegroundActive), Theme.Default.ForegroundActive},
                        {nameof(Theme.Default.ForegroundHover), Theme.Default.ForegroundHover},

                    }
                },
                {
                    "Primary", new Dictionary<string, string>
                    {
                        {nameof(Theme.Primary.Background), Theme.Primary.Background},
                        {nameof(Theme.Primary.Foreground), Theme.Primary.Foreground},
                        {nameof(Theme.Primary.Border), Theme.Primary.Border},
                        {nameof(Theme.Primary.BackgroundActive), Theme.Primary.BackgroundActive},
                        {nameof(Theme.Primary.BackgroundHover), Theme.Primary.BackgroundHover},
                        {nameof(Theme.Primary.ForegroundActive), Theme.Primary.ForegroundActive},
                        {nameof(Theme.Primary.ForegroundHover), Theme.Primary.ForegroundHover},
                    }
                },
                {
                    "Secondary", new Dictionary<string, string>()
                    {
                        {nameof(Theme.Secondary.Background), Theme.Secondary.Background},
                        {nameof(Theme.Secondary.Foreground), Theme.Secondary.Foreground},
//                {nameof(Theme.Secondary.Border), Theme.Secondary.Border},
//                {nameof(Theme.Secondary.BackgroundActive), Theme.Secondary.BackgroundActive},
//                {nameof(Theme.Secondary.BackgroundHover), Theme.Secondary.BackgroundHover},
//                {nameof(Theme.Secondary.ForegroundActive), Theme.Secondary.ForegroundActive},
//                {nameof(Theme.Secondary.ForegroundHover), Theme.Secondary.ForegroundHover},   
                    }
                },
                {
                    "Danger", new Dictionary<string, string>()
                    {
                        {nameof(Theme.Danger.Background), Theme.Danger.Background},
                        {nameof(Theme.Danger.Foreground), Theme.Danger.Foreground},
                        {nameof(Theme.Danger.Border), Theme.Danger.Border},
                        {nameof(Theme.Danger.BackgroundActive), Theme.Danger.BackgroundActive},
                        {nameof(Theme.Danger.BackgroundHover), Theme.Danger.BackgroundHover},
                        {nameof(Theme.Danger.ForegroundActive), Theme.Danger.ForegroundActive},
                        {nameof(Theme.Danger.ForegroundHover), Theme.Danger.ForegroundHover},
                    }
                },
                {
                    "Success", new Dictionary<string, string>
                    {
                        {nameof(Theme.Success.Background), Theme.Success.Background},
                        {nameof(Theme.Success.Foreground), Theme.Success.Foreground},
                        {nameof(Theme.Success.Border), Theme.Success.Border},
                        {nameof(Theme.Success.BackgroundActive), Theme.Success.BackgroundActive},
                        {nameof(Theme.Success.BackgroundHover), Theme.Success.BackgroundHover},
                        {nameof(Theme.Success.ForegroundActive), Theme.Success.ForegroundActive},
                        {nameof(Theme.Success.ForegroundHover), Theme.Success.ForegroundHover},
                    }
                }
            };

            public ColorListItem(string themeName)
            {
                ThemeName = themeName;
            }


            private static IComponent ColorSquare(string color)
            {
                if (string.IsNullOrWhiteSpace(color))
                {
                    return Raw(Div(_(styles: (s) =>
                    {
                        s.width = "50px";
                        s.height = "49px";
                        //                        s.boxShadow = "1px 1px 1px 1px lightgrey";
                    })));
                }

                return Raw(Div(_(styles: (s) =>
                {
                    s.width = "50px";
                    s.height = "49px";
                    s.backgroundColor = color;
                    s.color = color;
                    s.borderColor = color;
                    s.boxShadow = "1px 1px 1px 1px lightgrey";
                })));
            }

            public int CompareTo(ColorListItem other, string columnSortingKey)
            {
                return 0;
            }

            public bool EnableOnListItemClickEvent => false;

            public void OnListItemClick(int listItemIndex)
            {
                //                throw new NotImplementedException();
                //TODO pius: pn click copy color to clipboard

                Toast().Information(listItemIndex.ToString());
            }

            public IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> createGridCellExpression)
            {
                yield return createGridCellExpression(columns[0], () => TextBlock(ThemeName));
                yield return createGridCellExpression(columns[1], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("Background", "")));
                yield return createGridCellExpression(columns[2], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("Foreground", "")));
                yield return createGridCellExpression(columns[3], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("Border", "")));
                yield return createGridCellExpression(columns[4], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("BackgroundActive", "")));
                yield return createGridCellExpression(columns[5], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("BackgroundHover", "")));
                yield return createGridCellExpression(columns[6], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("ForegroundActive", "")));
                yield return createGridCellExpression(columns[7], () => ColorSquare(Mapping[ThemeName].GetValueOrDefault("ForegroundHover", "")));
            }
        }
    }
}