using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Transpose.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        public static partial class Theme
        {
            /// <summary>
            /// The class on <c>document.body</c> while any custom theme is active. The colour setters
            /// (<see cref="SetPrimary"/>, <see cref="SetBackground"/>, <see cref="SetHighlight"/>, <see cref="Build"/>)
            /// restate their values under it as <c>!important</c>, so an app's explicit colours still win over
            /// the packaged theme it runs in. Without a custom theme the class is absent and nothing changes.
            /// </summary>
            public const string CustomThemeMarkerClass = "tss-custom-theme";

            private static ICustomTheme               _customTheme;
            private static int                        _customThemeVersion;
            private static readonly List<ICustomTheme> _knownCustomThemes = new List<ICustomTheme>();

            /// <summary>
            /// The custom theme currently selected with <see cref="SetCustomTheme"/>, or null when the
            /// default Tesserae look is in use.
            /// </summary>
            public static ICustomTheme CustomTheme => _customTheme;

            /// <summary>
            /// Makes <paramref name="theme"/> the one active custom theme, or goes back to the default look
            /// when it is null. The new theme's stylesheet is loaded before the previous theme is removed,
            /// so switching never shows the page unstyled in between. Raises <see cref="OnThemeChanged"/>.
            ///
            /// <para>
            /// Overlapping calls resolve to the last one: a theme whose stylesheet finishes loading after a
            /// later call has already picked something else is deactivated again instead of stacking on
            /// top of it.
            /// </para>
            /// </summary>
            public static async Task SetCustomTheme(ICustomTheme theme)
            {
                var version = ++_customThemeVersion;
                _customTheme = theme;

                if (theme is object)
                {
                    if (!_knownCustomThemes.Contains(theme)) _knownCustomThemes.Add(theme);

                    try
                    {
                        await theme.Activate();
                    }
                    catch (Exception e)
                    {
                        console.error($"Tesserae: could not activate the custom theme '{theme.Name}': {e}");

                        if (version == _customThemeVersion) _customTheme = null;
                        theme.Deactivate();
                        throw;
                    }
                }

                if (version != _customThemeVersion)
                {
                    // A later call won while this theme was loading.
                    if (theme is object && !ReferenceEquals(theme, _customTheme)) theme.Deactivate();
                    return;
                }

                foreach (var other in _knownCustomThemes)
                {
                    if (!ReferenceEquals(other, theme) && other.IsActive) other.Deactivate();
                }

                // Set last: deactivating the previous theme above takes the marker off with it.
                if (theme is object) document.body.classList.add(CustomThemeMarkerClass);
                else                 document.body.classList.remove(CustomThemeMarkerClass);

                RaiseOnThemeChanged();
            }

            /// <summary>Deactivates the current custom theme and goes back to the default Tesserae look.</summary>
            public static Task ClearCustomTheme() => SetCustomTheme(null);

            private static void RaiseOnThemeChanged() => OnThemeChanged?.Invoke();

            /// <summary>
            /// Appends to <paramref name="css"/> (a set of <c>:root</c> / <c>.tss-dark-mode</c> variable blocks) the same
            /// declarations as <c>!important</c> under <see cref="CustomThemeMarkerClass"/>. A custom theme sets the
            /// variables on the body, which beats anything declared on <c>:root</c> however specific; an important
            /// declaration on the body beats the theme's, and the dark block stays more specific than the light one.
            /// </summary>
            internal static string WithCustomThemeOverrides(string css)
            {
                var sb       = new StringBuilder(css);
                var inBlock  = false;

                foreach (var raw in css.Split('\n'))
                {
                    var line = raw.Trim();

                    if (line.EndsWith("{"))
                    {
                        var selector = line.Substring(0, line.Length - 1).Trim();
                        inBlock = true;

                        if (selector == ":root")               sb.Append("body.").Append(CustomThemeMarkerClass).AppendLine(" {");
                        else if (selector == ".tss-dark-mode") sb.Append("body.").Append(CustomThemeMarkerClass).AppendLine(".tss-dark-mode {");
                        else                                   inBlock = false;
                    }
                    else if (line == "}")
                    {
                        if (inBlock) sb.AppendLine("}");
                        inBlock = false;
                    }
                    else if (inBlock && line.StartsWith("--") && line.EndsWith(";"))
                    {
                        sb.Append("  ").Append(line.Substring(0, line.Length - 1)).AppendLine(" !important;");
                    }
                }

                return sb.ToString();
            }
        }
    }
}
