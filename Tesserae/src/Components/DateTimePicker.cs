﻿using System;
using System.Globalization;

namespace Tesserae
{
    [H5.Name("tss.DateTimePicker")]
    public class DateTimePicker : MomentPickerBase<DateTimePicker, DateTime>
    {
        public DateTimePicker(DateTime? dateTime = null)
            : base("datetime-local", dateTime.HasValue ? FormatDateTime(dateTime.Value) : string.Empty)
        {
        }

        public DateTime DateTime => Moment;

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

        private static string FormatDateTime(DateTime dateTime) => dateTime.ToString("yyyy-MM-ddTHH:mm");

        protected override string FormatMoment(DateTime dateTime) => FormatDateTime(dateTime);

        protected override DateTime FormatMoment(string dateTime)
        {
            if (System.DateTime.TryParseExact(dateTime, "yyyy-MM-ddTHH:mm", DateTimeFormatInfo.InvariantInfo, out var result))
            {
                return result;
            }

            return default;
        }
    }
}