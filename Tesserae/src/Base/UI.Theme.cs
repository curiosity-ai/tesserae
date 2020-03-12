using static Retyped.dom;

namespace Tesserae
{
    public static partial class UI
    {
        public static class Theme
        {
            public static void Dark()
            {
                document.body.classList.add("tss-dark");
            }

            public static void Light()
            {
                document.body.classList.remove("tss-dark");
            }

            public static bool IsDark()
            {
                return document.body.classList.contains("tss-dark");
            }

            //Variables from tesserae.common.css
            public static class Default
            {
                public const string Background       = "var(--tss-default-background-color)";
                public const string Foreground       = "var(--tss-default-foreground-color)";
                public const string Border           = "var(--tss-default-border-color)";
                public const string InvalidBorder    = "var(--tss-invalid-border-color)";
                public const string BackgroundHover  = "var(--tss-default-background-hover-color)";
                public const string ForegroundHover  = "var(--tss-default-foreground-hover-color)";
                public const string BackgroundActive = "var(--tss-default-background-active-color)";
                public const string ForegroundActive = "var(--tss-default-foreground-active-color)";

                public const string Slider           = "var(--tss-slider-color)";
                public const string SliderActive     = "var(--tss-slider-active-color)";
                public const string SliderDisabled   = "var(--tss-slider-disabled-color)";
                public const string OverlayLight     = "var(--tss-overlay-light)";
                public const string OverlayDark      = "var(--tss-overlay-dark)";
                public const string CardShadow       = "var(--tss-card-shadow, 0 0.3px 0.9px 0 rgba(0,0,0,0.108))";
            }

            public static class Secondary
            {
                public const string Background       = "var(--tss-secondary-background-color)";
                public const string Foreground       = "var(--tss-secondary-foreground-color)";
            }
            public static class Disabled
            {
                public const string Background       = "var(--tss-disabled-background-color)";
                public const string Foreground       = "var(--tss-disabled-foreground-color)";
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
        }
    }
}
