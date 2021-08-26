using System;
using System.Globalization;

namespace Tesserae
{
    public class DatePicker : MomentPickerBase<DatePicker, DateTimeOffset>
    {
        public DatePicker(DateTimeOffset? date = null)
            : base("date", date.HasValue ? FormatDateTime(date.Value) : string.Empty)
        {
        }

        public DateTimeOffset Date => Moment;

        /// <summary>
        /// Adds the pattern attribute to the underlying input element for graceful degradation when retrieving the user selected value on older browsers.
        /// </summary>
        /// <returns>
        /// The current instance of the type.
        /// </returns>
        public DatePicker WithBrowserFallback()
        {
            InnerElement.pattern = @"\d{4}-\d{2}-\d{2}";
            return this;
        }

        private static string FormatDateTime(DateTimeOffset date) => date.ToString("yyyy-MM-dd");

        protected override string FormatMoment(DateTimeOffset date) => FormatDateTime(date);

        protected override DateTimeOffset FormatMoment(string date)
        {
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, out var result))
            {
                return new DateTimeOffset(result);
            }

            return default;
        }
    }
}