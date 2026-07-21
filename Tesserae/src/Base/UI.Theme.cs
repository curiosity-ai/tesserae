using System;
using System.Text;
using static Transpose.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        /// <summary>
        /// Provides methods to manage the application theme (Dark/Light) and custom colors.
        /// </summary>
        public static partial class Theme
        {
            private static HTMLStyleElement _primaryStyleElement;
            private static HTMLStyleElement _backgroundStyleElement;

            /// <summary>
            /// Raised when on theme changed occurs.
            /// </summary>
            public static event Action OnThemeChanged;
            /// <summary>
            /// Enables dark mode.
            /// </summary>
            public static void Dark()
            {
                if (!IsDark)
                {
                    document.body.classList.add("tss-dark-mode");
                    OnThemeChanged?.Invoke();
                }
            }

            /// <summary>
            /// Enables light mode.
            /// </summary>
            public static void Light()
            {
                if (IsDark)
                {
                    document.body.classList.remove("tss-dark-mode");
                    OnThemeChanged?.Invoke();
                }
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
            /// Returns a value indicating whether the component is mobile.
            /// </summary>
            public static bool IsMobile
            {
                get
                {
                    try
                    {
                        return navigator.userAgentData.mobile;
                    }
                    catch
                    {
                        //Ignore, probably userAgentData is not supported
                    }
                    return !window.matchMedia("(any-pointer:fine)").matches;
                }
            }

            /// <summary>
            /// Raised when on mobile mode changed occurs.
            /// </summary>
            public static event Action OnMobileModeChanged;

            /// <summary>
            /// Enables mobile mode by adding the tss-mobile class to the document body.
            /// </summary>
            public static void Mobile()
            {
                if (!IsMobileMode)
                {
                    document.body.classList.add("tss-mobile");
                    OnMobileModeChanged?.Invoke();
                }
            }

            /// <summary>
            /// Disables mobile mode by removing the tss-mobile class from the document body.
            /// </summary>
            public static void NonMobile()
            {
                if (IsMobileMode)
                {
                    document.body.classList.remove("tss-mobile");
                    OnMobileModeChanged?.Invoke();
                }
            }

            /// <summary>
            /// Gets or sets whether mobile mode is active (tss-mobile class on body).
            /// </summary>
            public static bool IsMobileMode
            {
                get => document.body.classList.contains("tss-mobile");
                set
                {
                    if (value) Mobile();
                    else NonMobile();
                }
            }

            /// <summary>
            /// Enables automatic mobile detection based on viewport width, adding or removing the tss-mobile
            /// class from the document body whenever the viewport changes. Also fires OnMobileModeChanged.
            /// </summary>
            /// <param name="breakpoint">The max viewport width (in pixels) considered mobile. Default is 768.</param>
            public static void EnableMobileDetection(int breakpoint = 768)
            {
                void Check()
                {
                    // Try navigator.userAgentData.mobile first (reliable on modern browsers
                    // and does not depend on viewport width, so it survives browser zoom).
                    try
                    {
                        if (navigator.userAgentData.mobile)
                        {
                            Mobile();
                            return;
                        }
                    }
                    catch { }

                    // Fall back to viewport width (covers desktop browsers that don't support
                    // the User-Agent Client Hints API).
                    if (window.innerWidth <= breakpoint)
                    {
                        Mobile();
                    }
                    else
                    {
                        NonMobile();
                    }
                }

                Check();

                window.addEventListener("resize", (Action<Event>)(_ => Check()));
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
                /// <summary>
                /// CSS variable reference for the default surface background color.
                /// </summary>
                public const string Background       = "var(--tss-default-background-color)";
                /// <summary>
                /// CSS variable reference for the default surface foreground (text) color.
                /// </summary>
                public const string Foreground       = "var(--tss-default-foreground-color)";
                /// <summary>
                /// CSS variable reference for the default surface border color.
                /// </summary>
                public const string Border           = "var(--tss-default-border-color)";
                /// <summary>
                /// CSS variable reference for the default surface darker border color.
                /// </summary>
                public const string DarkBorder       = "var(--tss-dark-border-color)";
                /// <summary>
                /// CSS variable reference for the default surface invalid-state border color.
                /// </summary>
                public const string InvalidBorder    = "var(--tss-invalid-border-color)";
                /// <summary>
                /// CSS variable reference for the default surface background color on hover.
                /// </summary>
                public const string BackgroundHover  = "var(--tss-default-background-hover-color)";
                /// <summary>
                /// CSS variable reference for the default surface foreground color on hover.
                /// </summary>
                public const string ForegroundHover  = "var(--tss-default-foreground-hover-color)";
                /// <summary>
                /// CSS variable reference for the default surface background color while pressed / active.
                /// </summary>
                public const string BackgroundActive = "var(--tss-default-background-active-color)";
                /// <summary>
                /// CSS variable reference for the default surface foreground color while pressed / active.
                /// </summary>
                public const string ForegroundActive = "var(--tss-default-foreground-active-color)";

                /// <summary>
                /// CSS variable reference for the default surface track color.
                /// </summary>
                public const string Slider         = "var(--tss-slider-color)";
                /// <summary>
                /// CSS variable reference for the default surface active-track color.
                /// </summary>
                public const string SliderActive   = "var(--tss-slider-active-color)";
                /// <summary>
                /// CSS variable reference for the default surface disabled-track color.
                /// </summary>
                public const string SliderDisabled = "var(--tss-slider-disabled-color)";
                /// <summary>
                /// CSS variable reference for the default surface light overlay color.
                /// </summary>
                public const string OverlayLight   = "var(--tss-overlay-light)";
                /// <summary>
                /// CSS variable reference for the default surface dark overlay color.
                /// </summary>
                public const string OverlayDark    = "var(--tss-overlay-dark)";
                /// <summary>
                /// CSS variable reference for the default surface card shadow style.
                /// </summary>
                public const string CardShadow     = "var(--tss-card-shadow, 0 0.3px 0.9px 0 rgba(0,0,0,0.108))";
            }

            public static class Sidebar
            {
                /// <summary>
                /// CSS variable reference for the sidebar surface background color.
                /// </summary>
                public const string Background = "var(--tss-sidebar-background-color)";
                /// <summary>
                /// CSS variable reference for the sidebar surface foreground (text) color.
                /// </summary>
                public const string Foreground = "var(--tss-sidebar-foreground-color)";
            }

            public static class Secondary
            {
                /// <summary>
                /// CSS variable reference for the secondary surface background color.
                /// </summary>
                public const string Background = "var(--tss-secondary-background-color)";
                /// <summary>
                /// CSS variable reference for the secondary surface foreground (text) color.
                /// </summary>
                public const string Foreground = "var(--tss-secondary-foreground-color)";
            }
            public static class Disabled
            {
                /// <summary>
                /// CSS variable reference for the disabled state background color.
                /// </summary>
                public const string Background = "var(--tss-disabled-background-color)";
                /// <summary>
                /// CSS variable reference for the disabled state foreground (text) color.
                /// </summary>
                public const string Foreground = "var(--tss-disabled-foreground-color)";
            }

            public static class Primary
            {
                /// <summary>
                /// CSS variable reference for the primary (brand) surface background color.
                /// </summary>
                public const string Background       = "var(--tss-primary-background-color)";
                /// <summary>
                /// CSS variable reference for the primary (brand) surface foreground (text) color.
                /// </summary>
                public const string Foreground       = "var(--tss-primary-foreground-color)";
                /// <summary>
                /// CSS variable reference for the primary (brand) surface border color.
                /// </summary>
                public const string Border           = "var(--tss-primary-border-color)";
                /// <summary>
                /// CSS variable reference for the primary (brand) surface background color on hover.
                /// </summary>
                public const string BackgroundHover  = "var(--tss-primary-background-hover-color)";
                /// <summary>
                /// CSS variable reference for the primary (brand) surface foreground color on hover.
                /// </summary>
                public const string ForegroundHover  = "var(--tss-primary-foreground-hover-color)";
                /// <summary>
                /// CSS variable reference for the primary (brand) surface background color while pressed / active.
                /// </summary>
                public const string BackgroundActive = "var(--tss-primary-background-active-color)";
                /// <summary>
                /// CSS variable reference for the primary (brand) surface foreground color while pressed / active.
                /// </summary>
                public const string ForegroundActive = "var(--tss-primary-foreground-active-color)";
            }

            public static class Danger
            {
                /// <summary>
                /// CSS variable reference for the danger / error tone background color.
                /// </summary>
                public const string Background       = "var(--tss-danger-background-color)";
                /// <summary>
                /// CSS variable reference for the danger / error tone foreground (text) color.
                /// </summary>
                public const string Foreground       = "var(--tss-danger-foreground-color)";
                /// <summary>
                /// CSS variable reference for the danger / error tone border color.
                /// </summary>
                public const string Border           = "var(--tss-danger-border-color)";
                /// <summary>
                /// CSS variable reference for the danger / error tone background color on hover.
                /// </summary>
                public const string BackgroundHover  = "var(--tss-danger-background-hover-color)";
                /// <summary>
                /// CSS variable reference for the danger / error tone foreground color on hover.
                /// </summary>
                public const string ForegroundHover  = "var(--tss-danger-foreground-hover-color)";
                /// <summary>
                /// CSS variable reference for the danger / error tone background color while pressed / active.
                /// </summary>
                public const string BackgroundActive = "var(--tss-danger-background-active-color)";
                /// <summary>
                /// CSS variable reference for the danger / error tone foreground color while pressed / active.
                /// </summary>
                public const string ForegroundActive = "var(--tss-danger-foreground-active-color)";
            }

            public static class Success
            {
                /// <summary>
                /// CSS variable reference for the success tone background color.
                /// </summary>
                public const string Background       = "var(--tss-success-background-color)";
                /// <summary>
                /// CSS variable reference for the success tone foreground (text) color.
                /// </summary>
                public const string Foreground       = "var(--tss-success-foreground-color)";
                /// <summary>
                /// CSS variable reference for the success tone border color.
                /// </summary>
                public const string Border           = "var(--tss-success-border-color)";
                /// <summary>
                /// CSS variable reference for the success tone background color on hover.
                /// </summary>
                public const string BackgroundHover  = "var(--tss-success-background-hover-color)";
                /// <summary>
                /// CSS variable reference for the success tone foreground color on hover.
                /// </summary>
                public const string ForegroundHover  = "var(--tss-success-foreground-hover-color)";
                /// <summary>
                /// CSS variable reference for the success tone background color while pressed / active.
                /// </summary>
                public const string BackgroundActive = "var(--tss-success-background-active-color)";
                /// <summary>
                /// CSS variable reference for the success tone foreground color while pressed / active.
                /// </summary>
                public const string ForegroundActive = "var(--tss-success-foreground-active-color)";
            }

            public static class Gradients
            {
                /// <summary>
                /// CSS variable reference: Lime.
                /// </summary>
                public const string Lime = "var(--tss-gradient-lime)";
                /// <summary>
                /// CSS variable reference: Red.
                /// </summary>
                public const string Red = "var(--tss-gradient-red)";
                /// <summary>
                /// CSS variable reference: Orange.
                /// </summary>
                public const string Orange = "var(--tss-gradient-orange)";
                /// <summary>
                /// CSS variable reference: Yellow.
                /// </summary>
                public const string Yellow = "var(--tss-gradient-yellow)";
                /// <summary>
                /// CSS variable reference: Green.
                /// </summary>
                public const string Green = "var(--tss-gradient-green)";
                /// <summary>
                /// CSS variable reference: Teal.
                /// </summary>
                public const string Teal = "var(--tss-gradient-teal)";
                /// <summary>
                /// CSS variable reference: Blue.
                /// </summary>
                public const string Blue = "var(--tss-gradient-blue)";
                /// <summary>
                /// CSS variable reference: Purple.
                /// </summary>
                public const string Purple = "var(--tss-gradient-purple)";
                /// <summary>
                /// CSS variable reference: Magenta.
                /// </summary>
                public const string Magenta = "var(--tss-gradient-magenta)";
                /// <summary>
                /// CSS variable reference: AI.
                /// </summary>
                public const string AI = "var(--tss-gradient-ai)";
            }

            public static class Colors
            {
                /// <summary>
                /// CSS variable reference: Lime100.
                /// </summary>
                public const string Lime100 = "var(--tss-colors-lime-100)";
                /// <summary>
                /// CSS variable reference: Lime200.
                /// </summary>
                public const string Lime200 = "var(--tss-colors-lime-200)";
                /// <summary>
                /// CSS variable reference: Lime250.
                /// </summary>
                public const string Lime250 = "var(--tss-colors-lime-250)";
                /// <summary>
                /// CSS variable reference: Lime300.
                /// </summary>
                public const string Lime300 = "var(--tss-colors-lime-300)";
                /// <summary>
                /// CSS variable reference: Lime400.
                /// </summary>
                public const string Lime400 = "var(--tss-colors-lime-400)";
                /// <summary>
                /// CSS variable reference: Lime500.
                /// </summary>
                public const string Lime500 = "var(--tss-colors-lime-500)";
                /// <summary>
                /// CSS variable reference: Lime600.
                /// </summary>
                public const string Lime600 = "var(--tss-colors-lime-600)";
                /// <summary>
                /// CSS variable reference: Lime700.
                /// </summary>
                public const string Lime700 = "var(--tss-colors-lime-700)";
                /// <summary>
                /// CSS variable reference: Lime800.
                /// </summary>
                public const string Lime800 = "var(--tss-colors-lime-800)";
                /// <summary>
                /// CSS variable reference: Lime850.
                /// </summary>
                public const string Lime850 = "var(--tss-colors-lime-850)";
                /// <summary>
                /// CSS variable reference: Lime900.
                /// </summary>
                public const string Lime900 = "var(--tss-colors-lime-900)";
                /// <summary>
                /// CSS variable reference: Lime1000.
                /// </summary>
                public const string Lime1000 = "var(--tss-colors-lime-1000)";
                /// <summary>
                /// CSS variable reference: Red100.
                /// </summary>
                public const string Red100 = "var(--tss-colors-red-100)";
                /// <summary>
                /// CSS variable reference: Red200.
                /// </summary>
                public const string Red200 = "var(--tss-colors-red-200)";
                /// <summary>
                /// CSS variable reference: Red250.
                /// </summary>
                public const string Red250 = "var(--tss-colors-red-250)";
                /// <summary>
                /// CSS variable reference: Red300.
                /// </summary>
                public const string Red300 = "var(--tss-colors-red-300)";
                /// <summary>
                /// CSS variable reference: Red400.
                /// </summary>
                public const string Red400 = "var(--tss-colors-red-400)";
                /// <summary>
                /// CSS variable reference: Red500.
                /// </summary>
                public const string Red500 = "var(--tss-colors-red-500)";
                /// <summary>
                /// CSS variable reference: Red600.
                /// </summary>
                public const string Red600 = "var(--tss-colors-red-600)";
                /// <summary>
                /// CSS variable reference: Red700.
                /// </summary>
                public const string Red700 = "var(--tss-colors-red-700)";
                /// <summary>
                /// CSS variable reference: Red800.
                /// </summary>
                public const string Red800 = "var(--tss-colors-red-800)";
                /// <summary>
                /// CSS variable reference: Red850.
                /// </summary>
                public const string Red850 = "var(--tss-colors-red-850)";
                /// <summary>
                /// CSS variable reference: Red900.
                /// </summary>
                public const string Red900 = "var(--tss-colors-red-900)";
                /// <summary>
                /// CSS variable reference: Red1000.
                /// </summary>
                public const string Red1000 = "var(--tss-colors-red-1000)";
                /// <summary>
                /// CSS variable reference: Orange100.
                /// </summary>
                public const string Orange100 = "var(--tss-colors-orange-100)";
                /// <summary>
                /// CSS variable reference: Orange200.
                /// </summary>
                public const string Orange200 = "var(--tss-colors-orange-200)";
                /// <summary>
                /// CSS variable reference: Orange250.
                /// </summary>
                public const string Orange250 = "var(--tss-colors-orange-250)";
                /// <summary>
                /// CSS variable reference: Orange300.
                /// </summary>
                public const string Orange300 = "var(--tss-colors-orange-300)";
                /// <summary>
                /// CSS variable reference: Orange400.
                /// </summary>
                public const string Orange400 = "var(--tss-colors-orange-400)";
                /// <summary>
                /// CSS variable reference: Orange500.
                /// </summary>
                public const string Orange500 = "var(--tss-colors-orange-500)";
                /// <summary>
                /// CSS variable reference: Orange600.
                /// </summary>
                public const string Orange600 = "var(--tss-colors-orange-600)";
                /// <summary>
                /// CSS variable reference: Orange700.
                /// </summary>
                public const string Orange700 = "var(--tss-colors-orange-700)";
                /// <summary>
                /// CSS variable reference: Orange800.
                /// </summary>
                public const string Orange800 = "var(--tss-colors-orange-800)";
                /// <summary>
                /// CSS variable reference: Orange850.
                /// </summary>
                public const string Orange850 = "var(--tss-colors-orange-850)";
                /// <summary>
                /// CSS variable reference: Orange900.
                /// </summary>
                public const string Orange900 = "var(--tss-colors-orange-900)";
                /// <summary>
                /// CSS variable reference: Orange1000.
                /// </summary>
                public const string Orange1000 = "var(--tss-colors-orange-1000)";
                /// <summary>
                /// CSS variable reference: Yellow100.
                /// </summary>
                public const string Yellow100 = "var(--tss-colors-yellow-100)";
                /// <summary>
                /// CSS variable reference: Yellow200.
                /// </summary>
                public const string Yellow200 = "var(--tss-colors-yellow-200)";
                /// <summary>
                /// CSS variable reference: Yellow250.
                /// </summary>
                public const string Yellow250 = "var(--tss-colors-yellow-250)";
                /// <summary>
                /// CSS variable reference: Yellow300.
                /// </summary>
                public const string Yellow300 = "var(--tss-colors-yellow-300)";
                /// <summary>
                /// CSS variable reference: Yellow400.
                /// </summary>
                public const string Yellow400 = "var(--tss-colors-yellow-400)";
                /// <summary>
                /// CSS variable reference: Yellow500.
                /// </summary>
                public const string Yellow500 = "var(--tss-colors-yellow-500)";
                /// <summary>
                /// CSS variable reference: Yellow600.
                /// </summary>
                public const string Yellow600 = "var(--tss-colors-yellow-600)";
                /// <summary>
                /// CSS variable reference: Yellow700.
                /// </summary>
                public const string Yellow700 = "var(--tss-colors-yellow-700)";
                /// <summary>
                /// CSS variable reference: Yellow800.
                /// </summary>
                public const string Yellow800 = "var(--tss-colors-yellow-800)";
                /// <summary>
                /// CSS variable reference: Yellow850.
                /// </summary>
                public const string Yellow850 = "var(--tss-colors-yellow-850)";
                /// <summary>
                /// CSS variable reference: Yellow900.
                /// </summary>
                public const string Yellow900 = "var(--tss-colors-yellow-900)";
                /// <summary>
                /// CSS variable reference: Yellow1000.
                /// </summary>
                public const string Yellow1000 = "var(--tss-colors-yellow-1000)";
                /// <summary>
                /// CSS variable reference: Green100.
                /// </summary>
                public const string Green100 = "var(--tss-colors-green-100)";
                /// <summary>
                /// CSS variable reference: Green200.
                /// </summary>
                public const string Green200 = "var(--tss-colors-green-200)";
                /// <summary>
                /// CSS variable reference: Green250.
                /// </summary>
                public const string Green250 = "var(--tss-colors-green-250)";
                /// <summary>
                /// CSS variable reference: Green300.
                /// </summary>
                public const string Green300 = "var(--tss-colors-green-300)";
                /// <summary>
                /// CSS variable reference: Green400.
                /// </summary>
                public const string Green400 = "var(--tss-colors-green-400)";
                /// <summary>
                /// CSS variable reference: Green500.
                /// </summary>
                public const string Green500 = "var(--tss-colors-green-500)";
                /// <summary>
                /// CSS variable reference: Green600.
                /// </summary>
                public const string Green600 = "var(--tss-colors-green-600)";
                /// <summary>
                /// CSS variable reference: Green700.
                /// </summary>
                public const string Green700 = "var(--tss-colors-green-700)";
                /// <summary>
                /// CSS variable reference: Green800.
                /// </summary>
                public const string Green800 = "var(--tss-colors-green-800)";
                /// <summary>
                /// CSS variable reference: Green850.
                /// </summary>
                public const string Green850 = "var(--tss-colors-green-850)";
                /// <summary>
                /// CSS variable reference: Green900.
                /// </summary>
                public const string Green900 = "var(--tss-colors-green-900)";
                /// <summary>
                /// CSS variable reference: Green1000.
                /// </summary>
                public const string Green1000 = "var(--tss-colors-green-1000)";
                /// <summary>
                /// CSS variable reference: Teal100.
                /// </summary>
                public const string Teal100 = "var(--tss-colors-teal-100)";
                /// <summary>
                /// CSS variable reference: Teal200.
                /// </summary>
                public const string Teal200 = "var(--tss-colors-teal-200)";
                /// <summary>
                /// CSS variable reference: Teal250.
                /// </summary>
                public const string Teal250 = "var(--tss-colors-teal-250)";
                /// <summary>
                /// CSS variable reference: Teal300.
                /// </summary>
                public const string Teal300 = "var(--tss-colors-teal-300)";
                /// <summary>
                /// CSS variable reference: Teal400.
                /// </summary>
                public const string Teal400 = "var(--tss-colors-teal-400)";
                /// <summary>
                /// CSS variable reference: Teal500.
                /// </summary>
                public const string Teal500 = "var(--tss-colors-teal-500)";
                /// <summary>
                /// CSS variable reference: Teal600.
                /// </summary>
                public const string Teal600 = "var(--tss-colors-teal-600)";
                /// <summary>
                /// CSS variable reference: Teal700.
                /// </summary>
                public const string Teal700 = "var(--tss-colors-teal-700)";
                /// <summary>
                /// CSS variable reference: Teal800.
                /// </summary>
                public const string Teal800 = "var(--tss-colors-teal-800)";
                /// <summary>
                /// CSS variable reference: Teal850.
                /// </summary>
                public const string Teal850 = "var(--tss-colors-teal-850)";
                /// <summary>
                /// CSS variable reference: Teal900.
                /// </summary>
                public const string Teal900 = "var(--tss-colors-teal-900)";
                /// <summary>
                /// CSS variable reference: Teal1000.
                /// </summary>
                public const string Teal1000 = "var(--tss-colors-teal-1000)";
                /// <summary>
                /// CSS variable reference: Blue100.
                /// </summary>
                public const string Blue100 = "var(--tss-colors-blue-100)";
                /// <summary>
                /// CSS variable reference: Blue200.
                /// </summary>
                public const string Blue200 = "var(--tss-colors-blue-200)";
                /// <summary>
                /// CSS variable reference: Blue250.
                /// </summary>
                public const string Blue250 = "var(--tss-colors-blue-250)";
                /// <summary>
                /// CSS variable reference: Blue300.
                /// </summary>
                public const string Blue300 = "var(--tss-colors-blue-300)";
                /// <summary>
                /// CSS variable reference: Blue400.
                /// </summary>
                public const string Blue400 = "var(--tss-colors-blue-400)";
                /// <summary>
                /// CSS variable reference: Blue500.
                /// </summary>
                public const string Blue500 = "var(--tss-colors-blue-500)";
                /// <summary>
                /// CSS variable reference: Blue600.
                /// </summary>
                public const string Blue600 = "var(--tss-colors-blue-600)";
                /// <summary>
                /// CSS variable reference: Blue700.
                /// </summary>
                public const string Blue700 = "var(--tss-colors-blue-700)";
                /// <summary>
                /// CSS variable reference: Blue800.
                /// </summary>
                public const string Blue800 = "var(--tss-colors-blue-800)";
                /// <summary>
                /// CSS variable reference: Blue850.
                /// </summary>
                public const string Blue850 = "var(--tss-colors-blue-850)";
                /// <summary>
                /// CSS variable reference: Blue900.
                /// </summary>
                public const string Blue900 = "var(--tss-colors-blue-900)";
                /// <summary>
                /// CSS variable reference: Blue1000.
                /// </summary>
                public const string Blue1000 = "var(--tss-colors-blue-1000)";
                /// <summary>
                /// CSS variable reference: Purple100.
                /// </summary>
                public const string Purple100 = "var(--tss-colors-purple-100)";
                /// <summary>
                /// CSS variable reference: Purple200.
                /// </summary>
                public const string Purple200 = "var(--tss-colors-purple-200)";
                /// <summary>
                /// CSS variable reference: Purple250.
                /// </summary>
                public const string Purple250 = "var(--tss-colors-purple-250)";
                /// <summary>
                /// CSS variable reference: Purple300.
                /// </summary>
                public const string Purple300 = "var(--tss-colors-purple-300)";
                /// <summary>
                /// CSS variable reference: Purple400.
                /// </summary>
                public const string Purple400 = "var(--tss-colors-purple-400)";
                /// <summary>
                /// CSS variable reference: Purple500.
                /// </summary>
                public const string Purple500 = "var(--tss-colors-purple-500)";
                /// <summary>
                /// CSS variable reference: Purple600.
                /// </summary>
                public const string Purple600 = "var(--tss-colors-purple-600)";
                /// <summary>
                /// CSS variable reference: Purple700.
                /// </summary>
                public const string Purple700 = "var(--tss-colors-purple-700)";
                /// <summary>
                /// CSS variable reference: Purple800.
                /// </summary>
                public const string Purple800 = "var(--tss-colors-purple-800)";
                /// <summary>
                /// CSS variable reference: Purple850.
                /// </summary>
                public const string Purple850 = "var(--tss-colors-purple-850)";
                /// <summary>
                /// CSS variable reference: Purple900.
                /// </summary>
                public const string Purple900 = "var(--tss-colors-purple-900)";
                /// <summary>
                /// CSS variable reference: Purple1000.
                /// </summary>
                public const string Purple1000 = "var(--tss-colors-purple-1000)";
                /// <summary>
                /// CSS variable reference: Magenta100.
                /// </summary>
                public const string Magenta100 = "var(--tss-colors-magenta-100)";
                /// <summary>
                /// CSS variable reference: Magenta200.
                /// </summary>
                public const string Magenta200 = "var(--tss-colors-magenta-200)";
                /// <summary>
                /// CSS variable reference: Magenta250.
                /// </summary>
                public const string Magenta250 = "var(--tss-colors-magenta-250)";
                /// <summary>
                /// CSS variable reference: Magenta300.
                /// </summary>
                public const string Magenta300 = "var(--tss-colors-magenta-300)";
                /// <summary>
                /// CSS variable reference: Magenta400.
                /// </summary>
                public const string Magenta400 = "var(--tss-colors-magenta-400)";
                /// <summary>
                /// CSS variable reference: Magenta500.
                /// </summary>
                public const string Magenta500 = "var(--tss-colors-magenta-500)";
                /// <summary>
                /// CSS variable reference: Magenta600.
                /// </summary>
                public const string Magenta600 = "var(--tss-colors-magenta-600)";
                /// <summary>
                /// CSS variable reference: Magenta700.
                /// </summary>
                public const string Magenta700 = "var(--tss-colors-magenta-700)";
                /// <summary>
                /// CSS variable reference: Magenta800.
                /// </summary>
                public const string Magenta800 = "var(--tss-colors-magenta-800)";
                /// <summary>
                /// CSS variable reference: Magenta850.
                /// </summary>
                public const string Magenta850 = "var(--tss-colors-magenta-850)";
                /// <summary>
                /// CSS variable reference: Magenta900.
                /// </summary>
                public const string Magenta900 = "var(--tss-colors-magenta-900)";
                /// <summary>
                /// CSS variable reference: Magenta1000.
                /// </summary>
                public const string Magenta1000 = "var(--tss-colors-magenta-1000)";
                /// <summary>
                /// CSS variable reference: Neutral0.
                /// </summary>
                public const string Neutral0 = "var(--tss-colors-neutral-0)";
                /// <summary>
                /// CSS variable reference: Neutral100.
                /// </summary>
                public const string Neutral100 = "var(--tss-colors-neutral-100)";
                /// <summary>
                /// CSS variable reference: Neutral200.
                /// </summary>
                public const string Neutral200 = "var(--tss-colors-neutral-200)";
                /// <summary>
                /// CSS variable reference: Neutral300.
                /// </summary>
                public const string Neutral300 = "var(--tss-colors-neutral-300)";
                /// <summary>
                /// CSS variable reference: Neutral400.
                /// </summary>
                public const string Neutral400 = "var(--tss-colors-neutral-400)";
                /// <summary>
                /// CSS variable reference: Neutral500.
                /// </summary>
                public const string Neutral500 = "var(--tss-colors-neutral-500)";
                /// <summary>
                /// CSS variable reference: Neutral600.
                /// </summary>
                public const string Neutral600 = "var(--tss-colors-neutral-600)";
                /// <summary>
                /// CSS variable reference: Neutral700.
                /// </summary>
                public const string Neutral700 = "var(--tss-colors-neutral-700)";
                /// <summary>
                /// CSS variable reference: Neutral800.
                /// </summary>
                public const string Neutral800 = "var(--tss-colors-neutral-800)";
                /// <summary>
                /// CSS variable reference: Neutral900.
                /// </summary>
                public const string Neutral900 = "var(--tss-colors-neutral-900)";
                /// <summary>
                /// CSS variable reference: Neutral1000.
                /// </summary>
                public const string Neutral1000 = "var(--tss-colors-neutral-1000)";
                /// <summary>
                /// CSS variable reference: Neutral1100.
                /// </summary>
                public const string Neutral1100 = "var(--tss-colors-neutral-1100)";
                /// <summary>
                /// CSS variable reference: Neutral100Alpha.
                /// </summary>
                public const string Neutral100Alpha = "var(--tss-colors-neutral-100-alpha)";
                /// <summary>
                /// CSS variable reference: Neutral200Alpha.
                /// </summary>
                public const string Neutral200Alpha = "var(--tss-colors-neutral-200-alpha)";
                /// <summary>
                /// CSS variable reference: Neutral300Alpha.
                /// </summary>
                public const string Neutral300Alpha = "var(--tss-colors-neutral-300-alpha)";
                /// <summary>
                /// CSS variable reference: Neutral400Alpha.
                /// </summary>
                public const string Neutral400Alpha = "var(--tss-colors-neutral-400-alpha)";
                /// <summary>
                /// CSS variable reference: Neutral500Alpha.
                /// </summary>
                public const string Neutral500Alpha = "var(--tss-colors-neutral-500-alpha)";
                /// <summary>
                /// CSS variable reference: DarkNeutral0.
                /// </summary>
                public const string DarkNeutral0 = "var(--tss-colors-dark-neutral-0)";
                /// <summary>
                /// CSS variable reference: DarkNeutral100.
                /// </summary>
                public const string DarkNeutral100 = "var(--tss-colors-dark-neutral-100)";
                /// <summary>
                /// CSS variable reference: DarkNeutral200.
                /// </summary>
                public const string DarkNeutral200 = "var(--tss-colors-dark-neutral-200)";
                /// <summary>
                /// CSS variable reference: DarkNeutral300.
                /// </summary>
                public const string DarkNeutral300 = "var(--tss-colors-dark-neutral-300)";
                /// <summary>
                /// CSS variable reference: DarkNeutral400.
                /// </summary>
                public const string DarkNeutral400 = "var(--tss-colors-dark-neutral-400)";
                /// <summary>
                /// CSS variable reference: DarkNeutral500.
                /// </summary>
                public const string DarkNeutral500 = "var(--tss-colors-dark-neutral-500)";
                /// <summary>
                /// CSS variable reference: DarkNeutral600.
                /// </summary>
                public const string DarkNeutral600 = "var(--tss-colors-dark-neutral-600)";
                /// <summary>
                /// CSS variable reference: DarkNeutral700.
                /// </summary>
                public const string DarkNeutral700 = "var(--tss-colors-dark-neutral-700)";
                /// <summary>
                /// CSS variable reference: DarkNeutral800.
                /// </summary>
                public const string DarkNeutral800 = "var(--tss-colors-dark-neutral-800)";
                /// <summary>
                /// CSS variable reference: DarkNeutral900.
                /// </summary>
                public const string DarkNeutral900 = "var(--tss-colors-dark-neutral-900)";
                /// <summary>
                /// CSS variable reference: DarkNeutral1000.
                /// </summary>
                public const string DarkNeutral1000 = "var(--tss-colors-dark-neutral-1000)";
                /// <summary>
                /// CSS variable reference: DarkNeutral1100.
                /// </summary>
                public const string DarkNeutral1100 = "var(--tss-colors-dark-neutral-1100)";
                /// <summary>
                /// CSS variable reference: DarkNeutral100Alpha.
                /// </summary>
                public const string DarkNeutral100Alpha = "var(--tss-colors-dark-neutral-100-alpha)";
                /// <summary>
                /// CSS variable reference: DarkNeutral200Alpha.
                /// </summary>
                public const string DarkNeutral200Alpha = "var(--tss-colors-dark-neutral-200-alpha)";
                /// <summary>
                /// CSS variable reference: DarkNeutral300Alpha.
                /// </summary>
                public const string DarkNeutral300Alpha = "var(--tss-colors-dark-neutral-300-alpha)";
                /// <summary>
                /// CSS variable reference: DarkNeutral400Alpha.
                /// </summary>
                public const string DarkNeutral400Alpha = "var(--tss-colors-dark-neutral-400-alpha)";
                /// <summary>
                /// CSS variable reference: DarkNeutral500Alpha.
                /// </summary>
                public const string DarkNeutral500Alpha = "var(--tss-colors-dark-neutral-500-alpha)";
            }
        }
    }
}
