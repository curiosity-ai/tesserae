using System;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// A WeekPicker component that allows users to select a week using a native HTML week input.
    /// </summary>
    [H5.Name("tss.WeekPicker")]
    public class WeekPicker : MomentPickerBase<WeekPicker, (int year, int weekNumber)>
    {
        /// <summary>
        /// Initializes a new instance of the WeekPicker class.
        /// </summary>
        /// <param name="week">The initial week value (year and week number).</param>
        public WeekPicker((int year, int weekNumber)? week)
            : base("week", week.HasValue ? FormatWeek(week.Value) : string.Empty)
        {
        }

        /// <summary>
        /// Gets the selected week (year and week number).
        /// </summary>
        public (int year, int weekNumber) Week => Moment;

        private static string FormatWeek((int year, int weekNumber) week) => $"{week.year}-W{week.weekNumber}";

        protected override string FormatMoment((int year, int weekNumber) week) => FormatWeek(week);

        protected override (int year, int weekNumber) FormatMoment(string week)
        {
            var weekSplit = week.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (!weekSplit.Any() || weekSplit.Any(string.IsNullOrWhiteSpace))
            {
                return (DateTime.Today.Year, 1);
            }

            var year       = weekSplit[0];
            var weekNumber = weekSplit[1].ToUpper().Replace("W", string.Empty);

            if (!int.TryParse(year, out var yearParsed) || !int.TryParse(weekNumber, out var weekNumberParsed))
            {
                return (DateTime.Today.Year, 1);
            }

            return (yearParsed, weekNumberParsed);
        }
    }
}