using System.Text;
using static H5.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        /// <summary>
        /// Provides methods to manage the application theme (Dark/Light) and custom colors.
        /// </summary>
        public static class Theme
        {
            private static HTMLStyleElement _primaryStyleElement;
            private static HTMLStyleElement _backgroundStyleElement;

            /// <summary>
            /// Enables dark mode.
            /// </summary>
            public static void Dark()
            {
                document.body.classList.add("tss-dark-mode");
            }

            /// <summary>
            /// Enables light mode.
            /// </summary>
            public static void Light()
            {
                document.body.classList.remove("tss-dark-mode");
            }

            /// <summary>
            /// Gets or sets a value indicating whether the theme is dark.
            /// </summary>
            public static bool IsDark
            {
                get
                {
                    return document.body.classList.contains("tss-dark-mode");
                }
                set
                {
                    if (value)
                    {
                        Dark();
                    }
                    else
                    {
                        Light();
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the theme is light.
            /// </summary>
            public static bool IsLight
            {
                get
                {
                    return !IsDark;
                }
                set
                {
                    IsDark = !value;
                }
            }


            /// <summary>
            /// Sets the background colors for both light and dark modes.
            /// </summary>
            public static void SetBackground(Color defaultLight, Color defaultDark)
            {
                if (_backgroundStyleElement is object)
                {
                    _backgroundStyleElement.remove();
                    _backgroundStyleElement = null;
                }

                var secondaryLight = (HSLColor)defaultLight;
                var sidebarLight   = (HSLColor)defaultLight;
                var hoverLight     = (HSLColor)defaultLight;
                var activeLight    = (HSLColor)defaultLight;
                var progressLight  = (HSLColor)defaultLight;

                var secondaryDark = (HSLColor)defaultDark;
                var hoverDark     = (HSLColor)defaultDark;
                var activeDark    = (HSLColor)defaultDark;
                var progressDark  = (HSLColor)defaultDark;


                secondaryLight.Luminosity -= 4;
                hoverLight.Luminosity     -= 12;
                activeLight.Luminosity    -= 9;
                progressLight.Luminosity  -= 9;

                secondaryDark.Luminosity += 7;
                hoverDark.Luminosity     += 2;
                activeDark.Luminosity    += 13;
                progressDark.Luminosity  += 13;


                var sb = new StringBuilder();
                sb.AppendLine(":root {");
                //sb.Append("  --tss-default-background-color-root: ").Append(defaultLight.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-secondary-background-color-root: ").Append(secondaryLight.ToRGBvar()).AppendLine(";");
                //sb.Append("  --tss-default-background-hover-color-root: ").Append(hoverLight.ToRGBvar()).AppendLine(";");
                //sb.Append("  --tss-default-background-active-color-root: ").Append(activeLight.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-progress-background-color-root: ").Append(progressLight.ToRGBvar()).AppendLine(";");

                //We need to redefine the variables again here otherwise the values won't change as they're derived twice from variables
                sb.Append(@"
    --tss-default-background-color: rgb(var(--tss-default-background-color-root ));
    --tss-secondary-background-color: rgb(var(--tss-secondary-background-color-root ));
    --tss-default-background-hover-color: rgb(var(--tss-default-background-hover-color-root ));
    --tss-default-background-active-color: rgb(var(--tss-default-background-active-color-root ));
    --tss-progress-background-color: rgb(var(--tss-progress-background-color-root ));
");

                sb.AppendLine("}");

                sb.AppendLine(".tss-dark-mode {");
                //sb.Append("  --tss-default-background-color-root: ").Append(defaultDark.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-secondary-background-color-root: ").Append(secondaryDark.ToRGBvar()).AppendLine(";");
                //sb.Append("  --tss-default-background-hover-color-root: ").Append(hoverDark.ToRGBvar()).AppendLine(";");
                //sb.Append("  --tss-default-background-active-color-root: ").Append(activeDark.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-progress-background-color-root: ").Append(progressDark.ToRGBvar()).AppendLine(";");

                //We need to redefine the variables again here otherwise the values won't change as they're derived twice from variables
                sb.Append(@"
    --tss-default-background-color: rgb(var(--tss-default-background-color-root ));
    --tss-secondary-background-color: rgb(var(--tss-secondary-background-color-root ));
    --tss-default-background-hover-color: rgb(var(--tss-default-background-hover-color-root ));
    --tss-default-background-active-color: rgb(var(--tss-default-background-active-color-root ));
    --tss-progress-background-color: rgb(var(--tss-progress-background-color-root ));
");

                sb.AppendLine("}");

                _backgroundStyleElement      = (HTMLStyleElement)document.createElement("style");
                _backgroundStyleElement.type = "text/css";
                _backgroundStyleElement.appendChild(document.createTextNode(sb.ToString()));

                var head = document.getElementsByTagName("head")[0];
                head.appendChild(_backgroundStyleElement);
            }


            /// <summary>
            /// Sets the primary colors for both light and dark modes.
            /// </summary>
            public static void SetPrimary(Color primaryLightColor, Color primaryDarkColor)
            {
                if (_primaryStyleElement is object)
                {
                    _primaryStyleElement.remove();
                    _primaryStyleElement = null;
                }

                var borderColorLight      = (HSLColor)primaryLightColor;
                var borderColorDark       = (HSLColor)primaryDarkColor;
                var backgroundActiveLight = (HSLColor)primaryLightColor;
                var backgroundActiveDark  = (HSLColor)primaryDarkColor;

                // rgb(0, 120, 212)  = hsl(206, 100, 41.6)
                // rgb(16, 110, 190) = hsl(208, 85.5, 40.4)
                // rgb(0, 90, 158)   = hsl(206, 100, 31)

                borderColorLight.Luminosity -= (100  - 85.5); //Uses the same delta as in the current template
                borderColorLight.Saturation -= (41.6 - 40.4); //TODO: get real values instead using Color.EvalVar
                borderColorLight.Hue        -= (206  - 208); // Main problem is just how to handle the .tss-dark-mode eval, as it will change the return value

                borderColorDark.Luminosity -= (100  - 85.5);
                borderColorDark.Saturation -= (41.6 - 40.4);
                borderColorDark.Hue        -= (206  - 208);

                backgroundActiveLight.Luminosity -= (100  - 100);
                backgroundActiveLight.Saturation -= (41.6 - 31);
                backgroundActiveLight.Hue        -= (206  - 206);

                backgroundActiveDark.Luminosity -= (100  - 100);
                backgroundActiveDark.Saturation -= (41.6 - 31);
                backgroundActiveDark.Hue        -= (206  - 206);

                var sb = new StringBuilder();
                sb.AppendLine(":root {");
                sb.Append("  --tss-primary-background-color-root: ").Append(primaryLightColor.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-link-color-root: ").Append(primaryLightColor.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-border-color-root: ").Append(borderColorLight.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-background-hover-color-root: ").Append(borderColorLight.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-background-active-color-root: ").Append(backgroundActiveLight.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-shadow-color-root: ").Append(primaryLightColor.ToRGBvar()).AppendLine(";");

                //We need to redefine the variables again here otherwise the values won't change as they're derived twice from variables
                sb.Append(@"
    --tss-primary-background-color: rgb(var(--tss-primary-background-color-root ));
    --tss-link-color: rgb(var(--tss-link-color-root ));
    --tss-primary-border-color: rgb(var(--tss-primary-border-color-root ));
    --tss-primary-background-hover-color: rgb(var(--tss-primary-background-hover-color-root ));
    --tss-primary-background-active-color: rgb(var(--tss-primary-background-active-color-root ));
    --tss-primary-shadow: 0 1.6px 3.6px 0 rgba(var(--tss-primary-shadow-color-root),0.132), 0 0.3px 0.9px 0 rgba(var(--tss-primary-shadow-color-root),0.108);
");

                sb.AppendLine("}");

                sb.AppendLine(".tss-dark-mode {");
                sb.Append("  --tss-primary-background-color-root: ").Append(primaryDarkColor.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-link-color-root: ").Append(primaryDarkColor.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-border-color-root: ").Append(borderColorDark.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-background-hover-color-root: ").Append(borderColorDark.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-background-active-color-root: ").Append(backgroundActiveDark.ToRGBvar()).AppendLine(";");
                sb.Append("  --tss-primary-shadow-color-root: ").Append(primaryDarkColor.ToRGBvar()).AppendLine(";");

                //We need to redefine the variables again here otherwise the values won't change as they're derived twice from variables
                sb.Append(@"
    --tss-primary-background-color: rgb(var(--tss-primary-background-color-root ));
    --tss-link-color: rgb(var(--tss-link-color-root ));
    --tss-primary-border-color: rgb(var(--tss-primary-border-color-root ));
    --tss-primary-background-hover-color: rgb(var(--tss-primary-background-hover-color-root ));
    --tss-primary-background-active-color: rgb(var(--tss-primary-background-active-color-root ));
    --tss-primary-shadow: 0 3.6px 5.6px 0 rgba(var(--tss-primary-shadow-color-root),0.132), 2px 2.3px 5.9px 0 rgba(var(--tss-primary-shadow-color-root),0.108);
");


                sb.AppendLine("}");

                _primaryStyleElement      = (HTMLStyleElement)document.createElement("style");
                _primaryStyleElement.type = "text/css";
                _primaryStyleElement.appendChild(document.createTextNode(sb.ToString()));

                var head = document.getElementsByTagName("head")[0];
                head.appendChild(_primaryStyleElement);
            }

            //Variables from tesserae.common.css
            public static class Default
            {
                public const string Background       = "var(--tss-default-background-color)";
                public const string Foreground       = "var(--tss-default-foreground-color)";
                public const string Border           = "var(--tss-default-border-color)";
                public const string DarkBorder       = "var(--tss-dark-border-color)";
                public const string InvalidBorder    = "var(--tss-invalid-border-color)";
                public const string BackgroundHover  = "var(--tss-default-background-hover-color)";
                public const string ForegroundHover  = "var(--tss-default-foreground-hover-color)";
                public const string BackgroundActive = "var(--tss-default-background-active-color)";
                public const string ForegroundActive = "var(--tss-default-foreground-active-color)";

                public const string Slider         = "var(--tss-slider-color)";
                public const string SliderActive   = "var(--tss-slider-active-color)";
                public const string SliderDisabled = "var(--tss-slider-disabled-color)";
                public const string OverlayLight   = "var(--tss-overlay-light)";
                public const string OverlayDark    = "var(--tss-overlay-dark)";
                public const string CardShadow     = "var(--tss-card-shadow, 0 0.3px 0.9px 0 rgba(0,0,0,0.108))";
            }

            public static class Sidebar
            {
                public const string Background = "var(--tss-sidebar-background-color)";
                public const string Foreground = "var(--tss-sidebar-foreground-color)";
            }

            public static class Secondary
            {
                public const string Background = "var(--tss-secondary-background-color)";
                public const string Foreground = "var(--tss-secondary-foreground-color)";
            }
            public static class Disabled
            {
                public const string Background = "var(--tss-disabled-background-color)";
                public const string Foreground = "var(--tss-disabled-foreground-color)";
            }

            public static class Primary
            {
                public const string Background       = "var(--tss-primary-background-color)";
                public const string Foreground       = "var(--tss-primary-foreground-color)";
                public const string Border           = "var(--tss-primary-border-color)";
                public const string BackgroundHover  = "var(--tss-primary-background-hover-color)";
                public const string ForegroundHover  = "var(--tss-primary-foreground-hover-color)";
                public const string BackgroundActive = "var(--tss-primary-background-active-color)";
                public const string ForegroundActive = "var(--tss-primary-foreground-active-color)";
            }

            public static class Danger
            {
                public const string Background       = "var(--tss-danger-background-color)";
                public const string Foreground       = "var(--tss-danger-foreground-color)";
                public const string Border           = "var(--tss-danger-border-color)";
                public const string BackgroundHover  = "var(--tss-danger-background-hover-color)";
                public const string ForegroundHover  = "var(--tss-danger-foreground-hover-color)";
                public const string BackgroundActive = "var(--tss-danger-background-active-color)";
                public const string ForegroundActive = "var(--tss-danger-foreground-active-color)";
            }

            public static class Success
            {
                public const string Background       = "var(--tss-success-background-color)";
                public const string Foreground       = "var(--tss-success-foreground-color)";
                public const string Border           = "var(--tss-success-border-color)";
                public const string BackgroundHover  = "var(--tss-success-background-hover-color)";
                public const string ForegroundHover  = "var(--tss-success-foreground-hover-color)";
                public const string BackgroundActive = "var(--tss-success-background-active-color)";
                public const string ForegroundActive = "var(--tss-success-foreground-active-color)";
            }

            public static class Colors
            {
                public const string Lime100 = "var(--tss-colors-lime-100)";
                public const string Lime200 = "var(--tss-colors-lime-200)";
                public const string Lime250 = "var(--tss-colors-lime-250)";
                public const string Lime300 = "var(--tss-colors-lime-300)";
                public const string Lime400 = "var(--tss-colors-lime-400)";
                public const string Lime500 = "var(--tss-colors-lime-500)";
                public const string Lime600 = "var(--tss-colors-lime-600)";
                public const string Lime700 = "var(--tss-colors-lime-700)";
                public const string Lime800 = "var(--tss-colors-lime-800)";
                public const string Lime850 = "var(--tss-colors-lime-850)";
                public const string Lime900 = "var(--tss-colors-lime-900)";
                public const string Lime1000 = "var(--tss-colors-lime-1000)";
                public const string Red100 = "var(--tss-colors-red-100)";
                public const string Red200 = "var(--tss-colors-red-200)";
                public const string Red250 = "var(--tss-colors-red-250)";
                public const string Red300 = "var(--tss-colors-red-300)";
                public const string Red400 = "var(--tss-colors-red-400)";
                public const string Red500 = "var(--tss-colors-red-500)";
                public const string Red600 = "var(--tss-colors-red-600)";
                public const string Red700 = "var(--tss-colors-red-700)";
                public const string Red800 = "var(--tss-colors-red-800)";
                public const string Red850 = "var(--tss-colors-red-850)";
                public const string Red900 = "var(--tss-colors-red-900)";
                public const string Red1000 = "var(--tss-colors-red-1000)";
                public const string Orange100 = "var(--tss-colors-orange-100)";
                public const string Orange200 = "var(--tss-colors-orange-200)";
                public const string Orange250 = "var(--tss-colors-orange-250)";
                public const string Orange300 = "var(--tss-colors-orange-300)";
                public const string Orange400 = "var(--tss-colors-orange-400)";
                public const string Orange500 = "var(--tss-colors-orange-500)";
                public const string Orange600 = "var(--tss-colors-orange-600)";
                public const string Orange700 = "var(--tss-colors-orange-700)";
                public const string Orange800 = "var(--tss-colors-orange-800)";
                public const string Orange850 = "var(--tss-colors-orange-850)";
                public const string Orange900 = "var(--tss-colors-orange-900)";
                public const string Orange1000 = "var(--tss-colors-orange-1000)";
                public const string Yellow100 = "var(--tss-colors-yellow-100)";
                public const string Yellow200 = "var(--tss-colors-yellow-200)";
                public const string Yellow250 = "var(--tss-colors-yellow-250)";
                public const string Yellow300 = "var(--tss-colors-yellow-300)";
                public const string Yellow400 = "var(--tss-colors-yellow-400)";
                public const string Yellow500 = "var(--tss-colors-yellow-500)";
                public const string Yellow600 = "var(--tss-colors-yellow-600)";
                public const string Yellow700 = "var(--tss-colors-yellow-700)";
                public const string Yellow800 = "var(--tss-colors-yellow-800)";
                public const string Yellow850 = "var(--tss-colors-yellow-850)";
                public const string Yellow900 = "var(--tss-colors-yellow-900)";
                public const string Yellow1000 = "var(--tss-colors-yellow-1000)";
                public const string Green100 = "var(--tss-colors-green-100)";
                public const string Green200 = "var(--tss-colors-green-200)";
                public const string Green250 = "var(--tss-colors-green-250)";
                public const string Green300 = "var(--tss-colors-green-300)";
                public const string Green400 = "var(--tss-colors-green-400)";
                public const string Green500 = "var(--tss-colors-green-500)";
                public const string Green600 = "var(--tss-colors-green-600)";
                public const string Green700 = "var(--tss-colors-green-700)";
                public const string Green800 = "var(--tss-colors-green-800)";
                public const string Green850 = "var(--tss-colors-green-850)";
                public const string Green900 = "var(--tss-colors-green-900)";
                public const string Green1000 = "var(--tss-colors-green-1000)";
                public const string Teal100 = "var(--tss-colors-teal-100)";
                public const string Teal200 = "var(--tss-colors-teal-200)";
                public const string Teal250 = "var(--tss-colors-teal-250)";
                public const string Teal300 = "var(--tss-colors-teal-300)";
                public const string Teal400 = "var(--tss-colors-teal-400)";
                public const string Teal500 = "var(--tss-colors-teal-500)";
                public const string Teal600 = "var(--tss-colors-teal-600)";
                public const string Teal700 = "var(--tss-colors-teal-700)";
                public const string Teal800 = "var(--tss-colors-teal-800)";
                public const string Teal850 = "var(--tss-colors-teal-850)";
                public const string Teal900 = "var(--tss-colors-teal-900)";
                public const string Teal1000 = "var(--tss-colors-teal-1000)";
                public const string Blue100 = "var(--tss-colors-blue-100)";
                public const string Blue200 = "var(--tss-colors-blue-200)";
                public const string Blue250 = "var(--tss-colors-blue-250)";
                public const string Blue300 = "var(--tss-colors-blue-300)";
                public const string Blue400 = "var(--tss-colors-blue-400)";
                public const string Blue500 = "var(--tss-colors-blue-500)";
                public const string Blue600 = "var(--tss-colors-blue-600)";
                public const string Blue700 = "var(--tss-colors-blue-700)";
                public const string Blue800 = "var(--tss-colors-blue-800)";
                public const string Blue850 = "var(--tss-colors-blue-850)";
                public const string Blue900 = "var(--tss-colors-blue-900)";
                public const string Blue1000 = "var(--tss-colors-blue-1000)";
                public const string Purple100 = "var(--tss-colors-purple-100)";
                public const string Purple200 = "var(--tss-colors-purple-200)";
                public const string Purple250 = "var(--tss-colors-purple-250)";
                public const string Purple300 = "var(--tss-colors-purple-300)";
                public const string Purple400 = "var(--tss-colors-purple-400)";
                public const string Purple500 = "var(--tss-colors-purple-500)";
                public const string Purple600 = "var(--tss-colors-purple-600)";
                public const string Purple700 = "var(--tss-colors-purple-700)";
                public const string Purple800 = "var(--tss-colors-purple-800)";
                public const string Purple850 = "var(--tss-colors-purple-850)";
                public const string Purple900 = "var(--tss-colors-purple-900)";
                public const string Purple1000 = "var(--tss-colors-purple-1000)";
                public const string Magenta100 = "var(--tss-colors-magenta-100)";
                public const string Magenta200 = "var(--tss-colors-magenta-200)";
                public const string Magenta250 = "var(--tss-colors-magenta-250)";
                public const string Magenta300 = "var(--tss-colors-magenta-300)";
                public const string Magenta400 = "var(--tss-colors-magenta-400)";
                public const string Magenta500 = "var(--tss-colors-magenta-500)";
                public const string Magenta600 = "var(--tss-colors-magenta-600)";
                public const string Magenta700 = "var(--tss-colors-magenta-700)";
                public const string Magenta800 = "var(--tss-colors-magenta-800)";
                public const string Magenta850 = "var(--tss-colors-magenta-850)";
                public const string Magenta900 = "var(--tss-colors-magenta-900)";
                public const string Magenta1000 = "var(--tss-colors-magenta-1000)";
                public const string Neutral0 = "var(--tss-colors-neutral-0)";
                public const string Neutral100 = "var(--tss-colors-neutral-100)";
                public const string Neutral200 = "var(--tss-colors-neutral-200)";
                public const string Neutral300 = "var(--tss-colors-neutral-300)";
                public const string Neutral400 = "var(--tss-colors-neutral-400)";
                public const string Neutral500 = "var(--tss-colors-neutral-500)";
                public const string Neutral600 = "var(--tss-colors-neutral-600)";
                public const string Neutral700 = "var(--tss-colors-neutral-700)";
                public const string Neutral800 = "var(--tss-colors-neutral-800)";
                public const string Neutral900 = "var(--tss-colors-neutral-900)";
                public const string Neutral1000 = "var(--tss-colors-neutral-1000)";
                public const string Neutral1100 = "var(--tss-colors-neutral-1100)";
                public const string Neutral100A = "var(--tss-colors-neutral-100-alpha)";
                public const string Neutral200A = "var(--tss-colors-neutral-200-alpha)";
                public const string Neutral300A = "var(--tss-colors-neutral-300-alpha)";
                public const string Neutral400A = "var(--tss-colors-neutral-400-alpha)";
                public const string Neutral500A = "var(--tss-colors-neutral-500-alpha)";
                public const string DarkNeutralMinus100 = "var(--tss-colors-dark-neutral-minus-100)";
                public const string DarkNeutral0 = "var(--tss-colors-dark-neutral-0)";
                public const string DarkNeutral100 = "var(--tss-colors-dark-neutral-100)";
                public const string DarkNeutral200 = "var(--tss-colors-dark-neutral-200)";
                public const string DarkNeutral250 = "var(--tss-colors-dark-neutral-250)";
                public const string DarkNeutral300 = "var(--tss-colors-dark-neutral-300)";
                public const string DarkNeutral350 = "var(--tss-colors-dark-neutral-350)";
                public const string DarkNeutral400 = "var(--tss-colors-dark-neutral-400)";
                public const string DarkNeutral500 = "var(--tss-colors-dark-neutral-500)";
                public const string DarkNeutral600 = "var(--tss-colors-dark-neutral-600)";
                public const string DarkNeutral700 = "var(--tss-colors-dark-neutral-700)";
                public const string DarkNeutral800 = "var(--tss-colors-dark-neutral-800)";
                public const string DarkNeutral900 = "var(--tss-colors-dark-neutral-900)";
                public const string DarkNeutral1000 = "var(--tss-colors-dark-neutral-1000)";
                public const string DarkNeutral1100 = "var(--tss-colors-dark-neutral-1100)";
                public const string DarkNeutralMinus100A = "var(--tss-colors-dark-neutral-minus-100-alpha)";
                public const string DarkNeutral100A = "var(--tss-colors-dark-neutral-100-alpha)";
                public const string DarkNeutral200A = "var(--tss-colors-dark-neutral-200-alpha)";
                public const string DarkNeutral250A = "var(--tss-colors-dark-neutral-250-alpha)";
                public const string DarkNeutral300A = "var(--tss-colors-dark-neutral-300-alpha)";
                public const string DarkNeutral350A = "var(--tss-colors-dark-neutral-350-alpha)";
                public const string DarkNeutral400A = "var(--tss-colors-dark-neutral-400-alpha)";
                public const string DarkNeutral500A = "var(--tss-colors-dark-neutral-500-alpha)";
            }
        }
    }
}
