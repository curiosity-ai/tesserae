using System;
using System.Linq;

namespace Tesserae
{
    public class MonthPicker : MomentPickerBase<MonthPicker, (int year, int month)>
    {
        public MonthPicker((int year, int month)? monthAndYear)
            : base("month", monthAndYear.HasValue ? FormatMonth(monthAndYear.Value) : string.Empty)
        {
        }

        public (int year, int month) Month => Moment;

        /// <summary>
        /// Adds the pattern attribute to the underlying input element for graceful degradation when retrieving the user selected value on older browsers.
        /// </summary>
        /// <returns>
        /// The current instance of the type.
        /// </returns>
        public MonthPicker WithBrowserFallback()
        {
            InnerElement.pattern = @"[0-9]{4}-[0-9]{2}";
            return this;
        }

        private static string FormatMonth((int year, int month) monthAndYear) => $"{monthAndYear.year}-{monthAndYear.month}";

        protected override string FormatMoment((int year, int month) monthAndYear) => FormatMonth(monthAndYear);

        protected override (int year, int month) FormatMoment(string monthAndYear)
        {
            var monthAndYearSplit = monthAndYear.Split(new []{ '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (!monthAndYearSplit.Any() || monthAndYearSplit.Any(string.IsNullOrWhiteSpace))
            {
                return (DateTime.Today.Year, 1);
            }

            var year  = monthAndYearSplit[0];
            var month = monthAndYearSplit[1];

            if (!int.TryParse(year, out var yearParsed) || !int.TryParse(month, out var monthParsed))
            {
                return (DateTime.Today.Year, 1);
            }

            return (yearParsed, monthParsed);
        }
    }
}
