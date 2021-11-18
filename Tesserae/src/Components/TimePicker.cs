using System;
using System.Globalization;

namespace Tesserae
{
    [H5.Name("tss.TimePicker")]
    public class TimePicker : MomentPickerBase<TimePicker, DateTimeOffset>
    {
        public TimePicker(DateTimeOffset? time = null)
            : base("time", time.HasValue ? FormatDateTime(time.Value) : string.Empty)
        {
        }

        public DateTimeOffset Time => Moment;

        /// <summary>
        /// Adds the pattern attribute to the underlying input element for graceful degradation when retrieving the user selected value on older browsers.
        /// </summary>
        /// <returns>
        /// The current instance of the type.
        /// </returns>
        public TimePicker WithBrowserFallback()
        {
            InnerElement.pattern = @"[0-9]{2}:[0-9]{2}";
            return this;
        }

        private static string FormatDateTime(DateTimeOffset time) => time.ToString("hh:mm:ss");

        protected override string FormatMoment(DateTimeOffset time) => FormatDateTime(time);

        protected override DateTimeOffset FormatMoment(string time)
        {
            if (DateTime.TryParseExact(time, "hh:mm:ss", DateTimeFormatInfo.InvariantInfo, out var result))
            {
                return result;
            }

            return default;
        }
    }
}