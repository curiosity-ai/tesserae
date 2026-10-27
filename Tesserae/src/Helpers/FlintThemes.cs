using System;
using System.Collections.Generic;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The <see cref="IFlintTheme"/>s for the <see href="https://github.com/microsoft/flint-chart">Flint</see> charts in
    /// Markdown (see <see cref="MarkdownBlock.Flint(IFlintTheme, bool)"/>): the presets Flint ships, a theme drawn from
    /// the page's Tesserae tokens, and one written as JSON.
    /// </summary>
    /// <remarks>
    /// <see cref="Tesserae"/> is the default. It takes the chart's colours and type from the tokens around the chart,
    /// so a chart follows light/dark mode and every custom theme the way the rest of the page does, and it is drawn
    /// again when the page's theme changes. A preset is a fixed house style with its own canvas colour, and looks the
    /// same in every Tesserae theme.
    /// </remarks>
    [Transpose.Name("tss.FlintThemes")]
    public static class FlintThemes
    {
        private static readonly Dictionary<string, IFlintTheme> _presets      = new Dictionary<string, IFlintTheme>();
        private static readonly Dictionary<string, IFlintTheme> _tesseraeOver = new Dictionary<string, IFlintTheme>();

        /// <summary>The page's own look: colours, fonts and series palette from the Tesserae tokens around the chart.</summary>
        public static IFlintTheme Tesserae { get; } = new TesseraeFlintTheme(null);

        /// <summary>No theme: Flint's own defaults.</summary>
        public static IFlintTheme None { get; } = new JsonFlintTheme(null);

        /// <summary>The New York Times.</summary>
        public static IFlintTheme Nyt         => Preset("nyt");
        /// <summary>The Economist: the red tab, the red zero rule, right-hand value axis.</summary>
        public static IFlintTheme Economist   => Preset("economist");
        /// <summary>Swiss / International Typographic Style: structural black rules, grotesk type.</summary>
        public static IFlintTheme Swiss       => Preset("swiss");
        /// <summary>Nature, the journal: a bare axis and quiet type.</summary>
        public static IFlintTheme Nature      => Preset("nature");
        /// <summary>McKinsey: horizontal bars and a restrained blue palette.</summary>
        public static IFlintTheme McKinsey    => Preset("mckinsey");
        /// <summary>Datawrapper.</summary>
        public static IFlintTheme Datawrapper => Preset("datawrapper");
        /// <summary>Power BI, dark canvas.</summary>
        public static IFlintTheme PowerBI     => Preset("powerbi");
        /// <summary>Power BI, light canvas.</summary>
        public static IFlintTheme PowerBILight => Preset("powerbi-light");
        /// <summary>Pop: bright, loud, playful.</summary>
        public static IFlintTheme Pop         => Preset("pop");
        /// <summary>Cartoon: rounded bar tops and a hand-drawn feel.</summary>
        public static IFlintTheme Cartoon     => Preset("cartoon");
        /// <summary>Apple.</summary>
        public static IFlintTheme Apple       => Preset("apple");
        /// <summary>De Stijl: primaries and black rules.</summary>
        public static IFlintTheme DeStijl     => Preset("de-stijl");
        /// <summary>The BBC.</summary>
        public static IFlintTheme Bbc         => Preset("bbc");
        /// <summary>Material Design 3.</summary>
        public static IFlintTheme Material    => Preset("material");

        /// <summary>The ids of the presets above, as Flint names them in a fence's <c>theme_spec</c>.</summary>
        public static readonly string[] PresetIds =
        {
            "nyt", "economist", "swiss", "nature", "mckinsey", "datawrapper", "powerbi", "powerbi-light",
            "pop", "cartoon", "apple", "de-stijl", "bbc", "material"
        };

        /// <summary>A preset Flint ships, by id (see <see cref="PresetIds"/>). An unknown id fails the chart with Flint's list of ids.</summary>
        public static IFlintTheme Preset(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A Flint preset id is required.", nameof(id));

            //One instance per id: Markdown tells two charts' themes apart by instance (see MarkdownCharts.KeyOf).
            if (!_presets.TryGetValue(id, out var theme))
            {
                theme       = new JsonFlintTheme(Script.Write<string>("JSON.stringify({0})", id));
                _presets[id] = theme;
            }

            return theme;
        }

        /// <summary>
        /// The page's Tesserae colours and type over a preset's structure: the preset decides the axes, gridlines,
        /// marks and spacing, the tokens decide every colour and font. <c>TesseraeOver("economist")</c> is the
        /// Economist's layout in the app's own palette.
        /// </summary>
        public static IFlintTheme TesseraeOver(string presetId)
        {
            if (string.IsNullOrWhiteSpace(presetId)) throw new ArgumentException("A Flint preset id is required.", nameof(presetId));

            if (!_tesseraeOver.TryGetValue(presetId, out var theme))
            {
                theme                    = new TesseraeFlintTheme(presetId);
                _tesseraeOver[presetId] = theme;
            }

            return theme;
        }

        /// <summary>
        /// A ThemeSpec written as JSON, e.g. <c>{ "extends": "nyt", "ink": { "accent": "#0b57d0" } }</c>, or a quoted
        /// preset id. See Flint's theme documentation for the fields; colours are hex.
        /// </summary>
        public static IFlintTheme FromJson(string themeSpecJson)
        {
            if (string.IsNullOrWhiteSpace(themeSpecJson)) throw new ArgumentException("The theme JSON is required.", nameof(themeSpecJson));
            return new JsonFlintTheme(themeSpecJson);
        }

        private sealed class JsonFlintTheme : IFlintTheme
        {
            private readonly string _json;

            public JsonFlintTheme(string json) => _json = json;

            public string ThemeSpecJson(HTMLElement host) => _json;
        }

        private sealed class TesseraeFlintTheme : IFlintTheme
        {
            private readonly string _extends;

            public TesseraeFlintTheme(string extendsPreset) => _extends = extendsPreset;

            public string ThemeSpecJson(HTMLElement host)
            {
                var css = new ComputedTokens(host);

                try
                {
                    var surface   = css.Surface;
                    var primary   = css.Color(Theme.Default.Foreground);
                    var secondary = css.Color(Theme.Secondary.Foreground);
                    var border    = css.Color(Theme.Default.Border);
                    var dark      = css.Color(Theme.Default.DarkBorder);
                    var family    = css.FontFamily("var(--tss-sansserif-font-family)");
                    var semibold  = css.FontWeight("var(--tss-font-weight-semibold)");

                    var series = new string[SeriesTokens.Length];
                    for (int i = 0; i < SeriesTokens.Length; i++) series[i] = css.Color(SeriesTokens[i]);

                    //"host": the frame around the chart draws the surface (a pattern, a translucent pane), so Markdown
                    //leaves the chart unpainted. The canvas colour is what Flint measures contrast against.
                    var spec = new FlintThemeSpec
                    {
                        extends = _extends,
                        id      = _extends is null ? "tesserae" : "tesserae-" + _extends,
                        label   = "Tesserae",
                        ink     = new FlintThemeInk
                        {
                            surface   = new FlintThemeSurface { source = "host", canvas = surface, plot = surface, panel = css.Color(Theme.Secondary.Background) },
                            text      = new FlintThemeText { primary = primary, secondary = secondary },
                            structure = new FlintThemeStructureInk { axis = dark, grid = border, frame = border, rule = border, zero = dark, connector = secondary },
                            series    = new FlintThemeSeries
                            {
                                single      = series[0],
                                categorical = series,
                                overflow    = secondary,
                                status      = new FlintThemeStatus
                                {
                                    positive = css.Color(Theme.Success.Background),
                                    negative = css.Color(Theme.Danger.Background),
                                    neutral  = secondary
                                }
                            },
                            accent = css.Color(Theme.Primary.Background)
                        },
                        type = new FlintThemeType
                        {
                            headline   = new FlintThemeTypeRole { family = family, weight = semibold, color = primary },
                            deck       = new FlintThemeTypeRole { family = family, color = secondary },
                            kicker     = new FlintThemeTypeRole { family = family, color = secondary },
                            axisLabel  = new FlintThemeTypeRole { family = family, color = secondary },
                            axisTitle  = new FlintThemeTypeRole { family = family, color = secondary },
                            valueLabel = new FlintThemeTypeRole { family = family },
                            keyLabel   = new FlintThemeTypeRole { family = family, color = secondary },
                            keyTitle   = new FlintThemeTypeRole { family = family, color = secondary },
                            annotation = new FlintThemeTypeRole { family = family, color = secondary },
                            footnote   = new FlintThemeTypeRole { family = family, color = secondary },
                            display    = new FlintThemeTypeRole { family = family, color = primary }
                        }
                    };

                    //Unset fields are null on the object literal, and Flint reads a present null as a stated value.
                    return Script.Write<string>("JSON.stringify({0}, function (k, v) { return v === null ? undefined : v; })", spec);
                }
                finally
                {
                    css.Dispose();
                }
            }

            //The palette the toolkit's own charts draw with (ChartBase.DefaultPalette): a custom theme recolours both.
            private static readonly string[] SeriesTokens =
            {
                SeriesToken(1, Theme.Colors.Blue600),
                SeriesToken(2, Theme.Colors.Green600),
                SeriesToken(3, Theme.Colors.Orange600),
                SeriesToken(4, Theme.Colors.Purple600),
                SeriesToken(5, Theme.Colors.Red600),
                SeriesToken(6, Theme.Colors.Teal600),
                SeriesToken(7, Theme.Colors.Yellow600),
                SeriesToken(8, Theme.Colors.Neutral600)
            };

            private static string SeriesToken(int index, string fallback) => "var(--tss-chart-series-" + index + ", " + fallback + ")";
        }

        /// <summary>
        /// Reads Tesserae tokens as they apply at one element, in the form Flint takes them. A token can be any CSS
        /// colour (<c>rgb()</c>, <c>oklch()</c>, <c>color-mix()</c>, translucent), and Flint parses hex only, so each
        /// is painted onto a one-pixel canvas over the chart's surface and read back.
        /// </summary>
        private sealed class ComputedTokens
        {
            private static HTMLCanvasElement _canvas;
            private static object            _context;

            private readonly HTMLElement _probe;

            public string Surface { get; }

            public ComputedTokens(HTMLElement host)
            {
                if (_canvas is null)
                {
                    _canvas        = (HTMLCanvasElement)document.createElement("canvas");
                    _canvas.width  = 1;
                    _canvas.height = 1;
                    _context       = Script.Write<object>("{0}.getContext('2d', { willReadFrequently: true })", _canvas);
                }

                //A probe inside the host sees the tokens the chart sits under (a sidebar or a card can restate them);
                //no transition, so a theme switch reads the colour it is going to, not one on the way there.
                _probe                  = (HTMLElement)document.createElement("span");
                _probe.style.display    = "none";
                _probe.style.transition = "none";
                host.appendChild(_probe);

                Surface = SurfaceBehind(host);
            }

            public string Color(string cssColor)
            {
                _probe.style.color = cssColor;
                return Paint(Surface, window.getComputedStyle(_probe).color);
            }

            public string FontFamily(string cssFontFamily)
            {
                _probe.style.fontFamily = cssFontFamily;
                return window.getComputedStyle(_probe).fontFamily;
            }

            public int FontWeight(string cssFontWeight)
            {
                _probe.style.fontWeight = cssFontWeight;
                return Script.Write<int>("parseInt({0}, 10) || 600", window.getComputedStyle(_probe).fontWeight);
            }

            public void Dispose() => _probe.remove();

            //The colour the chart is drawn on: the first opaque background from the host outwards, with every
            //translucent one in front of it laid over it (a frosted card over the page), over white at the end. A
            //background still transitioning (a theme switch fades cards and panes) counts as the colour it is going to.
            private static string SurfaceBehind(HTMLElement host)
                => Script.Write<string>(@"(function (el, ctx) {
                        var layers = [];
                        for (; el && el.nodeType === 1; el = el.parentElement) {
                            var bg = getComputedStyle(el).backgroundColor;
                            var running = el.getAnimations ? el.getAnimations() : [];
                            for (var j = 0; j < running.length; j++) {
                                if (running[j].transitionProperty !== 'background-color') continue;
                                var frames = running[j].effect.getKeyframes();
                                bg = frames[frames.length - 1].backgroundColor || bg;
                            }
                            ctx.clearRect(0, 0, 1, 1); ctx.fillStyle = bg; ctx.fillRect(0, 0, 1, 1);
                            var a = ctx.getImageData(0, 0, 1, 1).data[3];
                            if (a === 0) continue;
                            layers.push(bg);
                            if (a === 255) break;
                        }
                        ctx.clearRect(0, 0, 1, 1); ctx.fillStyle = '#ffffff'; ctx.fillRect(0, 0, 1, 1);
                        for (var i = layers.length - 1; i >= 0; i--) { ctx.fillStyle = layers[i]; ctx.fillRect(0, 0, 1, 1); }
                        var d = ctx.getImageData(0, 0, 1, 1).data;
                        return '#' + [d[0], d[1], d[2]].map(function (n) { return (n < 16 ? '0' : '') + n.toString(16); }).join('');
                    })({0}, {1})", host, _context);

            private static string Paint(string over, string color)
                => Script.Write<string>(@"(function (ctx, over, color) {
                        ctx.clearRect(0, 0, 1, 1);
                        ctx.fillStyle = over;  ctx.fillRect(0, 0, 1, 1);
                        ctx.fillStyle = color; ctx.fillRect(0, 0, 1, 1);
                        var d = ctx.getImageData(0, 0, 1, 1).data;
                        return '#' + [d[0], d[1], d[2]].map(function (n) { return (n < 16 ? '0' : '') + n.toString(16); }).join('');
                    })({0}, {1}, {2})", _context, over, color);
        }

        // The part of Flint's ThemeSpec the Tesserae theme states; field names are the JSON keys.

        [ObjectLiteral]
        private sealed class FlintThemeSpec
        {
            public string        extends;
            public string        id;
            public string        label;
            public FlintThemeInk  ink;
            public FlintThemeType type;
        }

        [ObjectLiteral]
        private sealed class FlintThemeInk
        {
            public FlintThemeSurface      surface;
            public FlintThemeText         text;
            public FlintThemeStructureInk structure;
            public FlintThemeSeries       series;
            public string                 accent;
        }

        [ObjectLiteral]
        private sealed class FlintThemeSurface
        {
            public string source;
            public string canvas;
            public string plot;
            public string panel;
        }

        [ObjectLiteral]
        private sealed class FlintThemeText
        {
            public string primary;
            public string secondary;
        }

        [ObjectLiteral]
        private sealed class FlintThemeStructureInk
        {
            public string axis;
            public string grid;
            public string frame;
            public string rule;
            public string zero;
            public string connector;
        }

        [ObjectLiteral]
        private sealed class FlintThemeSeries
        {
            public string           single;
            public string[]         categorical;
            public string           overflow;
            public FlintThemeStatus status;
        }

        [ObjectLiteral]
        private sealed class FlintThemeStatus
        {
            public string positive;
            public string negative;
            public string neutral;
        }

        [ObjectLiteral]
        private sealed class FlintThemeType
        {
            public FlintThemeTypeRole headline;
            public FlintThemeTypeRole deck;
            public FlintThemeTypeRole kicker;
            public FlintThemeTypeRole axisLabel;
            public FlintThemeTypeRole axisTitle;
            public FlintThemeTypeRole valueLabel;
            public FlintThemeTypeRole keyLabel;
            public FlintThemeTypeRole keyTitle;
            public FlintThemeTypeRole annotation;
            public FlintThemeTypeRole footnote;
            public FlintThemeTypeRole display;
        }

        [ObjectLiteral]
        private sealed class FlintThemeTypeRole
        {
            public string family;
            public int?   weight;
            public string color;
        }
    }
}
