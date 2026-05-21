using System;

namespace Tesserae
{
    /// <summary>
    /// Helpers for rendering a <see cref="DateTime"/> range as a compact human-readable string.
    /// </summary>
    internal static class DateTimeRangeRenderer
    {
        /// <summary>
        /// Renders the time.
        /// </summary>
        public static string RenderTime(DateTime date)
        {
            return date.TimeOfDay.TotalMilliseconds == 0 ? date.ToString("d") : date.ToString("g");
        }

        /// <summary>
        /// Renders the range.
        /// </summary>
        public static string RenderRange(DateTime from, DateTime to, Func<DateTime, string> renderTime = null)
        {
            var hasCustomRenderTime = renderTime != null;
            renderTime = renderTime ?? RenderTime;

            if (to < from)
            {
                (from, to) = (to, from);
            }

            if (from == to)
            {
                return renderTime(from) ?? "";
            }

            if (hasCustomRenderTime)
            {
                return RenderCustomRange(from, to, renderTime);
            }

            return RenderDefaultRange(from, to);
        }

        private static string RenderCustomRange(DateTime from, DateTime to, Func<DateTime, string> renderTime)
        {
            var fromText = renderTime(from) ?? "";
            var toText   = renderTime(to) ?? "";

            if (fromText == toText)
            {
                return fromText;
            }

            if (from.Date == to.Date)
            {
                var compactToText = RemoveSharedDatePrefix(toText, from);

                if (compactToText != toText)
                {
                    return $"{fromText} - {compactToText}";
                }
            }

            return $"{fromText} - {toText}";
        }

        private static string RenderDefaultRange(DateTime from, DateTime to)
        {
            var fromHasTime = HasVisibleTime(from);
            var toHasTime   = HasVisibleTime(to);

            if (from.Date == to.Date)
            {
                if (!fromHasTime && !toHasTime)
                {
                    return from.ToString("d");
                }

                return $"{from:d} {FormatTime(from)} - {FormatTime(to)}";
            }

            if (!fromHasTime && !toHasTime)
            {
                if (from.Year == to.Year && from.Month == to.Month)
                {
                    return $"{from:MMM d} - {to.Day}, {to:yyyy}";
                }

                if (from.Year == to.Year)
                {
                    return $"{from:MMM d} - {to:MMM d, yyyy}";
                }

                return $"{from:MMM d, yyyy} - {to:MMM d, yyyy}";
            }

            if (from.Year == to.Year && from.Month == to.Month)
            {
                return $"{from:MMM d} {FormatTime(from)} - {to.Day} {FormatTime(to)}, {to:yyyy}";
            }

            if (from.Year == to.Year)
            {
                return $"{from:MMM d} {FormatTime(from)} - {to:MMM d} {FormatTime(to)}, {to:yyyy}";
            }

            return $"{from:MMM d, yyyy} {FormatTime(from)} - {to:MMM d, yyyy} {FormatTime(to)}";
        }

        private static string RemoveSharedDatePrefix(string text, DateTime date)
        {
            var candidates = new[]
            {
                date.ToString("MMMM d, yyyy"),
                date.ToString("MMM d, yyyy"),
                date.ToString("MMMM d"),
                date.ToString("MMM d"),
                date.ToString("D"),
                date.ToString("d")
            };

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && text.StartsWith(candidate))
                {
                    return TrimRangePrefixSeparators(text.Substring(candidate.Length));
                }
            }

            return text;
        }

        private static string TrimRangePrefixSeparators(string text)
        {
            while (text.Length > 0 && (text[0] == ' ' || text[0] == ',' || text[0] == '-'))
            {
                text = text.Substring(1);
            }

            return text;
        }

        private static bool HasVisibleTime(DateTime date)
        {
            return date.TimeOfDay.TotalMilliseconds != 0;
        }

        private static string FormatTime(DateTime date)
        {
            if (date.Millisecond != 0)
            {
                return date.ToString("HH:mm:ss.fff");
            }

            if (date.Second != 0)
            {
                return date.ToString("HH:mm:ss");
            }

            return date.ToString("HH:mm");
        }
    }
}