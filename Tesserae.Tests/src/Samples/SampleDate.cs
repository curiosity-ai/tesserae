using System;

namespace Tesserae.Tests.Samples
{
    /// <summary>
    /// The instant the samples pretend "now" is. A sample that renders a date — a pre-filled picker,
    /// a chart's time axis, a histogram of events — reads as stale if it is pinned to a date in the
    /// past, but reads differently on every run if it is pinned to the clock, which is noise in any
    /// diff of the gallery. This is the compromise: a fixed day of the *current* year, so the page
    /// never looks out of date and still renders the same text all year.
    /// </summary>
    /// <remarks>
    /// Only the values a sample *displays* are anchored here. Validation rules that judge what the
    /// user just typed (<c>Validation.NotInThePast</c> and friends) stay on the real clock — they
    /// are about the person using the page, and they render nothing until someone interacts.
    /// </remarks>
    public static class SampleDate
    {
        /// <summary>May 25th of the current year, at midnight.</summary>
        public static DateTime Today => new DateTime(DateTime.Today.Year, 5, 25);

        /// <summary>May 25th of the current year, at 09:41.</summary>
        public static DateTime Now => Today.AddHours(9).AddMinutes(41);

        /// <summary>The given moment as seconds since the Unix epoch, for charts on a time axis.</summary>
        public static double UnixSeconds(DateTime value) => (value - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
