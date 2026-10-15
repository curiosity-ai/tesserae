using System;
using System.Globalization;

namespace Tesserae
{
    /// <summary>
    /// A TimePicker component that allows users to select a time using a native HTML time input.
    /// </summary>
    [Transpose.Name("tss.TimePicker")]
    public class TimePicker : MomentPickerBase<TimePicker, DateTimeOffset>
    {
        /// <summary>
        /// Initializes a new instance of the TimePicker class.
        /// </summary>
        /// <param name="time">The initial time value.</param>
        public TimePicker(DateTimeOffset? time = null)
            : base("time", time.HasValue ? FormatDateTime(time.Value) : string.Empty)
        {
        }

        /// <summary>
        /// Gets the selected time as a DateTimeOffset.
        /// </summary>
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

        //24-hour, as <input type="time"> reads and reports it; "hh" is the 12-hour clock and turned 14:30 into 02:30
        private static string FormatDateTime(DateTimeOffset time) => time.ToString("HH:mm");

        //The input reports seconds only when its step is under a minute
        private static readonly string[] _formats = { "HH:mm", "HH:mm:ss", "HH:mm:ss.fff" };

        protected override string FormatMoment(DateTimeOffset time) => FormatDateTime(time);

        protected override DateTimeOffset FormatMoment(string time)
        {
            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(time, format, DateTimeFormatInfo.InvariantInfo, out var result))
                {
                    return result;
                }
            }

            return default;
        }
    }
}