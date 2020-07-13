using System;
using System.Globalization;

namespace Tesserae.Components
{
    public class DateTimePicker : Input<DateTimePicker>
    {
        public DateTimePicker(DateTime? dateTime = null)
            : base("datetime-local", dateTime.HasValue ? FormatDateTime(dateTime.Value) : string.Empty)
        {
        }

        public DateTime DateTime => FormatDateTime(Text);

        public DateTime Max
        {
            get => FormatDateTime(InnerElement.max);
            set => InnerElement.max = FormatDateTime(value);
        }

        public DateTime Min
        {
            get => FormatDateTime(InnerElement.min);
            set => InnerElement.min = FormatDateTime(value);
        }

        public int Step
        {
            get => int.Parse(InnerElement.step);
            set => InnerElement.step = value.ToString();
        }

        public DateTimePicker SetMax(DateTime max)
        {
            Max = max;
            return this;
        }

        public DateTimePicker SetMin(DateTime min)
        {
            Min = min;
            return this;
        }

        public DateTimePicker SetStep(int step)
        {

            return this;
        }

        /// <summary>
        /// Adds the pattern attribute to the underlying input element for graceful degradation when retrieving the user selected value on older browsers.
        /// </summary>
        /// <returns>
        /// The current instance of the type.
        /// </returns>
        public DateTimePicker WithBrowserFallback()
        {
            InnerElement.pattern = @"[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}";
            return this;
        }

        private static string FormatDateTime(DateTime dateTime) => dateTime.ToString("s");

        private static DateTime FormatDateTime(string dateTime)
        {
            if (DateTime.TryParseExact(dateTime, "yyyy-MM-ddTHH:mm", DateTimeFormatInfo.InvariantInfo, out var result))
            {
                return result;
            }

            return default;
        }
    }
}
