using System;
using System.Globalization;

namespace Tesserae
{
    /// <summary>
    /// A form input for picking a single calendar date, backed by the browser's native date input.
    /// </summary>
    [Transpose.Name("tss.DatePicker")]
    public class DatePicker : MomentPickerBase<DatePicker, DateTime>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public DatePicker(DateTime? date = null)
            : base("date", date.HasValue ? FormatDateTime(date.Value) : string.Empty)
        {
        }

        /// <summary>
        /// Gets or sets the date value of the component.
        /// </summary>
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