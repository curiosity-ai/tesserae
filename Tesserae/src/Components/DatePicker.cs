using System;
using System.Globalization;

namespace Tesserae
{
    [H5.Name("tss.DatePicker")]
    public class DatePicker : MomentPickerBase<DatePicker, DateTime>
    {
        public DatePicker(DateTime? date = null)
            : base("date", date.HasValue ? FormatDateTime(date.Value) : string.Empty)
        {
        }

        public DateTime Date => Moment;

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

        private static string FormatDateTime(DateTime date) => date.ToString("yyyy-MM-dd");

        protected override string FormatMoment(DateTime date) => FormatDateTime(date);

        protected override DateTime FormatMoment(string date)
        {
            if (DateTime.TryParseExact(date, "yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo, out var result))
            {
                return result;
            }

            return default;
        }
    }
}