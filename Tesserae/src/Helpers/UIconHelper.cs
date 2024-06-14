using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;


namespace Tesserae
{
    public static class UIconHelper
    {
        public static UIcons WithDefaultUIcon(UIcons icon, UIcons defaultIcon) => string.IsNullOrWhiteSpace(icon.ToString()) ? defaultIcon : icon;
        public static UIcons WithDefaultUIcon(UIcons icon) => string.IsNullOrWhiteSpace(icon.ToString()) ? UIcons.Default : icon;

        public static Emoji  WithTextSize(this  Emoji  icon, TextSize  size)  => icon.WithCss(size.ToString());
        public static UIcons WithTextSize(this  UIcons icon, TextSize  size)  => icon.WithCss(size.ToString());
        public static UIcons WithTextColor(this UIcons icon, TextColor color) => icon.WithCss(color.ToString());


        public static bool TryGetUIcon(this string value, out UIcons icon)
        {
            if (value.StartsWith("fi"))
            {
                icon = value.AsUIcon();
                return true;
            }
            else
            {
                icon = default;
                return false;
            }
        }

        public static bool TryGetEmoji(this string value, out Emoji icon)
        {
            if (value.StartsWith("ec"))
            {
                icon = value.As<Emoji>();
                return true;
            }
            else
            {
                icon = default;
                return false;
            }
        }

        public static UIcons ParseUIcon(this string value, UIcons ifInvalid = UIcons.Default)
        {
            return !string.IsNullOrWhiteSpace(value) ? Enum.TryParse<UIcons>(value.Split(' ').FirstOrDefault(), out var icon) ? icon : ifInvalid : ifInvalid;
        }

        public static UIcons AsUIcon(this string value)
        {
            return value.As<UIcons>();
        }

        public static UIcons WithCss(this UIcons value, params string[] cssClasses)
        {
            return $"{value} {string.Join(" ", cssClasses)}".As<UIcons>();
        }

        public static Emoji WithCss(this Emoji value, params string[] cssClasses)
        {
            return $"{value} {string.Join(" ", cssClasses)}".As<Emoji>();
        }


        public static UIcons? AsUIconNullable(this string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? (UIcons?)null
                : value.As<UIcons>();
        }

        public static Emoji AsEmoji(this string value)
        {
            return value.As<Emoji>();
        }

        public static string GetUnicode(UIcons? icon)
        {
            return GetUnicode(icon ?? UIcons.Circle);
        }

        private static readonly Dictionary<UIcons, string> Icon2Unicode = new Dictionary<UIcons, string>();

        public static string GetUnicode(UIcons icon)
        {
            var iconStr = icon.ToString();

            if (string.IsNullOrWhiteSpace(iconStr)) return GetUnicode(UIcons.Circle);

            if (Icon2Unicode.TryGetValue(icon, out var unicode))
            {
                return unicode;
            }

            unicode            = GetContent(icon);
            unicode            = unicode ?? "0"; // the unicode codes for UIcons change with each version so we are just using a 0
            Icon2Unicode[icon] = unicode;
            return unicode;
        }

        private static string GetContent(UIcons icon)
        {
            var iconSelectorString = icon.ToString();

            foreach (var sheet in document.styleSheets)
            {
                var cssss = sheet.As<CSSStyleSheet>();

                try
                {
                    if (cssss.cssRules is object)
                    {
                        foreach (var rule in cssss.cssRules)
                        {
                            var cssRule = rule.As<CSSStyleRule>();

                            if (cssRule.selectorText.Contains(iconSelectorString))
                            {
                                return cssRule.style.content.Trim('"');
                            }
                        }
                    }
                }
                catch
                {
                    //Ignore, can happen for cross-domain css styles
                }
            }

            return null;
        }
    }
}