using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Transpose.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        public static partial class Theme
        {
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

                RaiseOnThemeChanged();
            }

            /// <summary>Deactivates the current custom theme and goes back to the default Tesserae look.</summary>
            public static Task ClearCustomTheme() => SetCustomTheme(null);

            private static void RaiseOnThemeChanged() => OnThemeChanged?.Invoke();
        }
    }
}
