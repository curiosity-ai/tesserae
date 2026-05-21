using System.Collections.Generic;
using System.Text;
using static H5.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        public static partial class Theme
        {
            /// <summary>
            /// A fluent builder that lets you set every color CSS variable the toolkit exposes,
            /// independently for light and dark mode, and then atomically apply the resulting stylesheet.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Use <see cref="ThemeBuilder"/> when you need full control over the visual palette — for
            /// example to brand a multi-tenant SaaS, to match an existing design system, or to ship a
            /// product-wide light/dark theme overhaul. For lightweight one-off changes prefer the focused
            /// helpers <see cref="SetPrimary"/> and <see cref="SetBackground"/>, which leave the rest of
            /// the palette untouched.
            /// </para>
            /// <para>
            /// Every setter on the builder accepts a <em>light</em> color and a <em>dark</em> color. The
            /// "dark" color is applied only when the document has the <c>.tss-dark-mode</c> class
            /// (toggled by <see cref="Dark"/> / <see cref="Light"/>); the "light" color applies otherwise.
            /// Anything you do not set keeps its default value from <c>tss.common.css</c>.
            /// </para>
            /// <example>
            /// <code>
            /// UI.Theme.Build()
            ///     .Primary(Color.FromString("#0078d4"), Color.FromString("#2899f5"))
            ///     .DefaultBackground(Color.FromString("#ffffff"), Color.FromString("#1f1f1f"))
            ///     .DefaultForeground(Color.FromString("#1f1f1f"), Color.FromString("#f3f2f1"))
            ///     .Danger(Color.FromString("#dc3545"), Color.FromString("#ff5162"))
            ///     .Success(Color.FromString("#22c55e"), Color.FromString("#4ade80"))
            ///     .Link  (Color.FromString("#0078d4"), Color.FromString("#55b3fb"))
            ///     .Apply();
            /// </code>
            /// </example>
            /// </remarks>
            [H5.Name("tss.ThemeBuilder")]
            public sealed class ThemeBuilder
            {
                private static HTMLStyleElement _appliedStyleElement;

                private readonly Dictionary<string, string> _light = new Dictionary<string, string>();
                private readonly Dictionary<string, string> _dark  = new Dictionary<string, string>();

                internal ThemeBuilder() { }

                // ---------- Default surface (the neutral background of the application) ----------

                /// <summary>Sets the main background color (the canvas behind everything).</summary>
                public ThemeBuilder DefaultBackground(Color light, Color dark) => Set("default-background-color", light, dark);

                /// <summary>Sets the background used when the user hovers over a default control.</summary>
                public ThemeBuilder DefaultBackgroundHover(Color light, Color dark) => Set("default-background-hover-color", light, dark);

                /// <summary>Sets the background used while a default control is being pressed / activated.</summary>
                public ThemeBuilder DefaultBackgroundActive(Color light, Color dark) => Set("default-background-active-color", light, dark);

                /// <summary>Sets the default foreground (text) color.</summary>
                public ThemeBuilder DefaultForeground(Color light, Color dark) => Set("default-foreground-color", light, dark);

                /// <summary>Sets the default hovered foreground (text) color.</summary>
                public ThemeBuilder DefaultForegroundHover(Color light, Color dark) => Set("default-foreground-hover-color", light, dark);

                /// <summary>Sets the default active foreground (text) color.</summary>
                public ThemeBuilder DefaultForegroundActive(Color light, Color dark) => Set("default-foreground-active-color", light, dark);

                /// <summary>Sets the default border color (input borders, dividers, card outlines).</summary>
                public ThemeBuilder DefaultBorder(Color light, Color dark) => Set("default-border-color", light, dark);

                /// <summary>Sets the darker variant of the default border, used for headers/footers and stronger separators.</summary>
                public ThemeBuilder DarkBorder(Color light, Color dark) => Set("dark-border-color", light, dark);

                /// <summary>Sets the color of horizontal separators (Stack dividers, section breaks).</summary>
                public ThemeBuilder DefaultSeparator(Color light, Color dark) => Set("default-separator-color", light, dark);

                /// <summary>Sets the color of the invalid border drawn around form fields that fail validation.</summary>
                public ThemeBuilder InvalidBorder(Color light, Color dark) => Set("invalid-border-color", light, dark);

                // ---------- Primary (the brand color used for primary buttons, focus rings, links) ----------

                /// <summary>Sets the primary brand color (primary button background, focus ring, link color).</summary>
                public ThemeBuilder Primary(Color light, Color dark)
                {
                    Set("primary-background-color",        light, dark);
                    Set("primary-background-hover-color",  light, dark);
                    Set("primary-background-active-color", light, dark);
                    Set("primary-border-color",            light, dark);
                    Set("primary-shadow-color",            light, dark);
                    Set("link-color",                      light, dark);
                    return this;
                }

                /// <summary>Sets just the primary background color (without touching hover/active/border).</summary>
                public ThemeBuilder PrimaryBackground(Color light, Color dark) => Set("primary-background-color", light, dark);

                /// <summary>Sets the primary background color used on hover.</summary>
                public ThemeBuilder PrimaryBackgroundHover(Color light, Color dark) => Set("primary-background-hover-color", light, dark);

                /// <summary>Sets the primary background color used while pressed / active.</summary>
                public ThemeBuilder PrimaryBackgroundActive(Color light, Color dark) => Set("primary-background-active-color", light, dark);

                /// <summary>Sets the primary border color (focus outlines, primary-button outlines).</summary>
                public ThemeBuilder PrimaryBorder(Color light, Color dark) => Set("primary-border-color", light, dark);

                /// <summary>Sets the foreground color used on top of the primary background.</summary>
                public ThemeBuilder PrimaryForeground(Color light, Color dark) => Set("primary-foreground-color", light, dark);

                /// <summary>Sets the primary foreground color used on hover.</summary>
                public ThemeBuilder PrimaryForegroundHover(Color light, Color dark) => Set("primary-foreground-hover-color", light, dark);

                /// <summary>Sets the primary foreground color used while pressed / active.</summary>
                public ThemeBuilder PrimaryForegroundActive(Color light, Color dark) => Set("primary-foreground-active-color", light, dark);

                /// <summary>Sets the color used to seed the primary shadow (typically the same as the primary background).</summary>
                public ThemeBuilder PrimaryShadow(Color light, Color dark) => Set("primary-shadow-color", light, dark);

                // ---------- Secondary surface ----------

                /// <summary>Sets the secondary background color (cards, panels, raised surfaces).</summary>
                public ThemeBuilder SecondaryBackground(Color light, Color dark) => Set("secondary-background-color", light, dark);

                /// <summary>Sets the secondary foreground color (subdued text on secondary surfaces).</summary>
                public ThemeBuilder SecondaryForeground(Color light, Color dark) => Set("secondary-foreground-color", light, dark);

                // ---------- Sidebar ----------

                /// <summary>Sets the background color of the application sidebar.</summary>
                public ThemeBuilder SidebarBackground(Color light, Color dark) => Set("sidebar-background-color", light, dark);

                /// <summary>Sets the foreground (text + icon) color used inside the sidebar.</summary>
                public ThemeBuilder SidebarForeground(Color light, Color dark) => Set("sidebar-foreground-color", light, dark);

                // ---------- Disabled ----------

                /// <summary>Sets the background color used by disabled controls.</summary>
                public ThemeBuilder DisabledBackground(Color light, Color dark) => Set("disabled-background-color", light, dark);

                /// <summary>Sets the foreground (text) color used by disabled controls.</summary>
                public ThemeBuilder DisabledForeground(Color light, Color dark) => Set("disabled-foreground-color", light, dark);

                // ---------- Danger ----------

                /// <summary>Sets the full set of "danger" colors in one call.</summary>
                public ThemeBuilder Danger(Color light, Color dark)
                {
                    Set("danger-background-color",        light, dark);
                    Set("danger-background-hover-color",  light, dark);
                    Set("danger-background-active-color", light, dark);
                    Set("danger-border-color",            light, dark);
                    return this;
                }

                /// <summary>Sets the danger background color.</summary>
                public ThemeBuilder DangerBackground(Color light, Color dark) => Set("danger-background-color", light, dark);

                /// <summary>Sets the danger background hover color.</summary>
                public ThemeBuilder DangerBackgroundHover(Color light, Color dark) => Set("danger-background-hover-color", light, dark);

                /// <summary>Sets the danger background active color.</summary>
                public ThemeBuilder DangerBackgroundActive(Color light, Color dark) => Set("danger-background-active-color", light, dark);

                /// <summary>Sets the danger border color.</summary>
                public ThemeBuilder DangerBorder(Color light, Color dark) => Set("danger-border-color", light, dark);

                /// <summary>Sets the danger foreground color.</summary>
                public ThemeBuilder DangerForeground(Color light, Color dark) => Set("danger-foreground-color", light, dark);

                /// <summary>Sets the danger foreground hover color.</summary>
                public ThemeBuilder DangerForegroundHover(Color light, Color dark) => Set("danger-foreground-hover-color", light, dark);

                /// <summary>Sets the danger foreground active color.</summary>
                public ThemeBuilder DangerForegroundActive(Color light, Color dark) => Set("danger-foreground-active-color", light, dark);

                // ---------- Success ----------

                /// <summary>Sets the full set of "success" colors in one call.</summary>
                public ThemeBuilder Success(Color light, Color dark)
                {
                    Set("success-background-color",        light, dark);
                    Set("success-background-hover-color",  light, dark);
                    Set("success-background-active-color", light, dark);
                    Set("success-border-color",            light, dark);
                    return this;
                }

                /// <summary>Sets the success background color.</summary>
                public ThemeBuilder SuccessBackground(Color light, Color dark) => Set("success-background-color", light, dark);

                /// <summary>Sets the success background hover color.</summary>
                public ThemeBuilder SuccessBackgroundHover(Color light, Color dark) => Set("success-background-hover-color", light, dark);

                /// <summary>Sets the success background active color.</summary>
                public ThemeBuilder SuccessBackgroundActive(Color light, Color dark) => Set("success-background-active-color", light, dark);

                /// <summary>Sets the success border color.</summary>
                public ThemeBuilder SuccessBorder(Color light, Color dark) => Set("success-border-color", light, dark);

                /// <summary>Sets the success foreground color.</summary>
                public ThemeBuilder SuccessForeground(Color light, Color dark) => Set("success-foreground-color", light, dark);

                /// <summary>Sets the success foreground hover color.</summary>
                public ThemeBuilder SuccessForegroundHover(Color light, Color dark) => Set("success-foreground-hover-color", light, dark);

                /// <summary>Sets the success foreground active color.</summary>
                public ThemeBuilder SuccessForegroundActive(Color light, Color dark) => Set("success-foreground-active-color", light, dark);

                // ---------- Warning ----------

                /// <summary>Sets the warning background color.</summary>
                public ThemeBuilder WarningBackground(Color light, Color dark) => Set("warning-background-color", light, dark);

                /// <summary>Sets the warning foreground color.</summary>
                public ThemeBuilder WarningForeground(Color light, Color dark) => Set("warning-foreground-color", light, dark);

                // ---------- Link ----------

                /// <summary>Sets the color used for hyperlinks.</summary>
                public ThemeBuilder Link(Color light, Color dark) => Set("link-color", light, dark);

                // ---------- Tooltip ----------

                /// <summary>Sets the background color of tooltip and popover surfaces (Tippy-based components).</summary>
                public ThemeBuilder TooltipBackground(Color light, Color dark) => Set("tooltip-background-color", light, dark);

                /// <summary>Sets the foreground color of tooltip and popover content.</summary>
                public ThemeBuilder TooltipForeground(Color light, Color dark) => Set("tooltip-foreground-color", light, dark);

                // ---------- Slider ----------

                /// <summary>Sets the color of the slider track.</summary>
                public ThemeBuilder Slider(Color light, Color dark) => Set("slider-color", light, dark);

                /// <summary>Sets the color of the slider track for the active (selected) portion.</summary>
                public ThemeBuilder SliderActive(Color light, Color dark) => Set("slider-active-color", light, dark);

                /// <summary>Sets the color of the slider track when disabled.</summary>
                public ThemeBuilder SliderDisabled(Color light, Color dark) => Set("slider-disabled-color", light, dark);

                // ---------- Progress ----------

                /// <summary>Sets the background color used as the empty track of progress indicators.</summary>
                public ThemeBuilder ProgressBackground(Color light, Color dark) => Set("progress-background-color", light, dark);

                // ---------- Set a fully custom variable -----------------------------------

                /// <summary>
                /// Sets an arbitrary CSS color variable by name (without the leading <c>--tss-</c> prefix).
                /// Use this escape hatch for colors that the typed setters above do not cover, or for
                /// future variables added to <c>tss.common.css</c>. The value is emitted as
                /// <c>rgb(var(--tss-<paramref name="name"/>-root))</c> when applied.
                /// </summary>
                /// <param name="name">The variable name without the <c>--tss-</c> prefix (e.g. <c>"my-accent-color"</c>).</param>
                /// <param name="light">The color to use in light mode.</param>
                /// <param name="dark">The color to use in dark mode.</param>
                public ThemeBuilder SetVariable(string name, Color light, Color dark) => Set(name, light, dark);

                // ---------- Apply ---------------------------------------------------------

                /// <summary>
                /// Removes any &lt;style&gt; element previously installed by <see cref="Apply"/>.
                /// Used by <see cref="UI.Theme.ResetBuild"/>.
                /// </summary>
                internal static void ResetApplied()
                {
                    if (_appliedStyleElement is object)
                    {
                        _appliedStyleElement.remove();
                        _appliedStyleElement = null;
                    }
                }

                /// <summary>
                /// Renders the configured palette into a <c>&lt;style&gt;</c> element and attaches it
                /// to the document head. A previous theme installed by <see cref="Apply"/> is removed first,
                /// so calls are idempotent. Raises <see cref="OnThemeChanged"/> on success.
                /// </summary>
                public void Apply()
                {
                    if (_appliedStyleElement is object)
                    {
                        _appliedStyleElement.remove();
                        _appliedStyleElement = null;
                    }

                    var css = ToCss();

                    _appliedStyleElement      = (HTMLStyleElement)document.createElement("style");
                    _appliedStyleElement.type = "text/css";
                    _appliedStyleElement.appendChild(document.createTextNode(css));

                    var head = document.getElementsByTagName("head")[0];
                    head.appendChild(_appliedStyleElement);

                    OnThemeChanged?.Invoke();
                }

                /// <summary>
                /// Returns the generated CSS without attaching it to the document. Useful for
                /// server-rendering, persistence (e.g. saving a tenant's theme), or in-app theme previews.
                /// </summary>
                public string ToCss()
                {
                    var sb = new StringBuilder();
                    EmitBlock(sb, ":root",          _light);
                    EmitBlock(sb, ".tss-dark-mode", _dark);
                    return sb.ToString();
                }

                private static void EmitBlock(StringBuilder sb, string selector, Dictionary<string, string> values)
                {
                    if (values.Count == 0) return;
                    sb.Append(selector).AppendLine(" {");
                    foreach (var kvp in values)
                    {
                        // Emit both the "root" RGB triple AND the rgb(var(...)) wrapper so a single Apply()
                        // call works even for variables that are normally derived (the wrapper is what
                        // component CSS references).
                        sb.Append("  --tss-").Append(kvp.Key).Append("-root: ").Append(kvp.Value).AppendLine(";");
                        sb.Append("  --tss-").Append(kvp.Key).Append(": rgb(var(--tss-").Append(kvp.Key).AppendLine("-root));");
                    }
                    sb.AppendLine("}");
                }

                private ThemeBuilder Set(string name, Color light, Color dark)
                {
                    if (light is object) _light[name] = light.ToRGBvar();
                    if (dark  is object) _dark[name]  = dark.ToRGBvar();
                    return this;
                }
            }

            /// <summary>
            /// Begins building a custom theme. Chain calls to set individual color variables for light
            /// and dark mode and finish with <see cref="ThemeBuilder.Apply"/> to install the theme.
            /// </summary>
            /// <example>
            /// <code>
            /// UI.Theme.Build()
            ///     .Primary(Color.FromString("#0078d4"), Color.FromString("#2899f5"))
            ///     .DefaultBackground(Color.FromString("#ffffff"), Color.FromString("#1f1f1f"))
            ///     .Apply();
            /// </code>
            /// </example>
            public static ThemeBuilder Build() => new ThemeBuilder();

            /// <summary>
            /// Removes any custom theme previously installed via <see cref="ThemeBuilder.Apply"/>,
            /// returning all CSS color variables to the defaults declared in <c>tss.common.css</c>.
            /// Raises <see cref="OnThemeChanged"/>.
            /// </summary>
            public static void ResetBuild()
            {
                ThemeBuilder.ResetApplied();
                OnThemeChanged?.Invoke();
            }
        }
    }
}
