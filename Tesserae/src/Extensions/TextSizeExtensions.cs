using System;
using System.Linq;
using static Retyped.dom;

namespace Tesserae.Components
{
    public static class TextSizeExtensions
    {
        public static T Tiny<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.Tiny;
            return hasTextSize;
        }

        public static T XSmall<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.XSmall;
            return hasTextSize;
        }
        public static T Small<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.Small;
            return hasTextSize;
        }

        public static T SmallPlus<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.SmallPlus;
            return hasTextSize;
        }

        public static T Medium<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.Medium;
            return hasTextSize;
        }
        public static T MediumPlus<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.MediumPlus;
            return hasTextSize;
        }

        public static T Large<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.Large;
            return hasTextSize;
        }
        public static T XLarge<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.XLarge;
            return hasTextSize;
        }

        public static T XXLarge<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.XXLarge;
            return hasTextSize;
        }

        public static T Mega<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Size = TextSize.Mega;
            return hasTextSize;
        }

        public static T Regular<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Weight = TextWeight.Regular;
            return hasTextSize;
        }

        public static T SemiBold<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Weight = TextWeight.SemiBold;
            return hasTextSize;
        }

        public static T Bold<T>(this T hasTextSize) where T : IHasTextSize
        {
            hasTextSize.Weight = TextWeight.Bold;
            return hasTextSize;
        }

        internal static TextSize FromClassList(HTMLElement element, TextSize defaultValue)
        {
            var curFontSize = element.classList.FirstOrDefault(t => t.StartsWith("tss-fontsize-"));
            if (curFontSize is object && Enum.TryParse<TextSize>(curFontSize.Substring("tss-fontsize-".Length), true, out var result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static string ToClassName(this TextSize textSize)
        {
            return $"tss-fontsize-{textSize.ToString().ToLower()}";
        }

        internal static TextWeight FromClassList(HTMLElement element, TextWeight defaultValue)
        {
            var curFontSize = element.classList.FirstOrDefault(t => t.StartsWith("tss-fontweight-"));
            if (curFontSize is object && Enum.TryParse<TextWeight>(curFontSize.Substring("tss-fontweight-".Length), true, out var result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static string ToClassName(this TextWeight textWeight)
        {
            return $"tss-fontweight-{textWeight.ToString().ToLower()}";
        }
    }
}
