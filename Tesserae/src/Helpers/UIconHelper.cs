using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;


namespace Tesserae
{
    /// <summary>
    /// Provides helper methods for working with UIcons and Emojis, including parsing, styling, and metadata retrieval.
    /// </summary>
    public static class UIconHelper
    {
        /// <summary>Returns the icon, or a default value if the icon is not specified.</summary>
        public static UIcons WithDefaultUIcon(UIcons icon, UIcons defaultIcon) => string.IsNullOrWhiteSpace(icon.ToString()) ? defaultIcon : icon;
        /// <summary>Returns the icon, or UIcons.Default if the icon is not specified.</summary>
        public static UIcons WithDefaultUIcon(UIcons icon) => string.IsNullOrWhiteSpace(icon.ToString()) ? UIcons.Default : icon;

        /// <summary>Applies a text size to an Emoji.</summary>
        public static Emoji  WithTextSize(this  Emoji  icon, TextSize  size)  => icon.WithCss(size.ToString());
        /// <summary>Applies a text size to a UIcons.</summary>
        public static UIcons WithTextSize(this  UIcons icon, TextSize  size)  => icon.WithCss(size.ToString());
        /// <summary>Applies a text color to a UIcons.</summary>
        public static UIcons WithTextColor(this UIcons icon, TextColor color) => icon.WithCss(color.ToString());

        /// <summary>Attempts to parse a string value into a UIcons icon.</summary>
        public static bool TryGetUIcon(string value, out UIcons icon)
        {
            if (value.StartsWith("fi"))
            {
                icon = AsUIcon(value);
                return true;
            }
            else
            {
                icon = default;
                return false;
            }
        }

        /// <summary>Attempts to parse a string value into an Emoji icon.</summary>
        public static bool TryGetEmoji(string value, out Emoji icon)
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

        /// <summary>Parses a string into a UIcons icon, returning a default value if invalid.</summary>
        public static UIcons ParseUIcon(string value, UIcons ifInvalid = UIcons.Default)
        {
            return !string.IsNullOrWhiteSpace(value) ? Enum.TryParse<UIcons>(value.Split(' ').FirstOrDefault(), out var icon) ? icon : ifInvalid : ifInvalid;
        }

        /// <summary>Casts a string value to a UIcons icon.</summary>
        public static UIcons AsUIcon(string value)
        {
            return value.As<UIcons>();
        }

        /// <summary>Applies CSS classes to a UIcons icon.</summary>
        public static UIcons WithCss(this UIcons value, params string[] cssClasses)
        {
            return $"{value} {string.Join(" ", cssClasses)}".As<UIcons>();
        }

        /// <summary>Applies CSS classes to an Emoji icon.</summary>
        public static Emoji WithCss(this Emoji value, params string[] cssClasses)
        {
            return $"{value} {string.Join(" ", cssClasses)}".As<Emoji>();
        }


        /// <summary>Converts a string to a UIcons icon, or returns a default value.</summary>
        public static UIcons ToIconOrDefault(string value, UIcons defaultIcon)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultIcon : value.As<UIcons>();
        }

        /// <summary>Converts a string to an Emoji icon, or returns a default value.</summary>
        public static Emoji ToEmojiOrDefault(string value, Emoji defaultEmoji)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultEmoji : value.As<Emoji>();
        }

        /// <summary>Casts a string value to an Emoji icon.</summary>
        public static Emoji AsEmoji(string value)
        {
            return value.As<Emoji>();
        }

        private static readonly Dictionary<UIcons, string> Icon2Unicode = new Dictionary<UIcons, string>();

        /// <summary>Retrieves the Unicode character associated with the specified UIcons icon from the stylesheets.</summary>
        public static string GetUnicode(UIcons icon)
        {
            var iconStr = icon.ToString();

            if (string.IsNullOrWhiteSpace(iconStr)) return GetUnicode(UIcons.Default);

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
                var cssStyleSheet = sheet.As<CSSStyleSheet>();

                try
                {
                    if (cssStyleSheet.cssRules is object)
                    {
                        foreach (var rule in cssStyleSheet.cssRules)
                        {
                            var cssRule = rule.As<CSSStyleRule>();

                            var ruleText = cssRule.selectorText;

                            if (ruleText is object && ruleText.Length > 0)
                            {
                                var l = ruleText.Length;

                                if (ruleText.Contains(iconSelectorString))
                                {
                                    var isMatch = ruleText
                                       .Substring(0, l - 8) // strip "::before"
                                       .Substring(1, l - 1) // strip leading "."
                                       .Equals(iconSelectorString);

                                    if (isMatch)
                                    {
                                        return cssRule.style.content.Trim('"');
                                    }
                                }
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

        /// <summary>Returns the font family name for the specified icon and weight.</summary>
        public static string GetFontText(UIcons icon, UIconsWeight weight)
        {
            if (icon.ToString().Contains("-brands-")) return "uicons-brands";

            switch (weight)
            {
                case UIconsWeight.Regular:         return "uicons-regular-rounded";
                case UIconsWeight.Solid:           return "uicons-solid-rounded";
                case UIconsWeight.Bold:            return "uicons-bold-rounded";
                case UIconsWeight.Thin:            return "uicons-thin-rounded";
                case UIconsWeight.RegularStraight: return "uicons-regular-straight";
                case UIconsWeight.SolidStraight:   return "uicons-solid-straight";
                case UIconsWeight.BoldStraight:    return "uicons-bold-straight";
                case UIconsWeight.ThinStraight:    return "uicons-thin-straight";
                default:                           throw new ArgumentOutOfRangeException(nameof(weight), weight, null);
            }
        }

    }
}