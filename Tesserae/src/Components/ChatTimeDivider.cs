using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
    /// <summary>
    /// A quiet divider across a chat transcript saying when the messages below it were written - a wavy hairline
    /// either side of the time ("6:07 PM", "Yesterday · 6:07 PM", "Mon · 6:07 PM", "Nov 3 · 6:07 PM"). A transcript
    /// puts one where the conversation picked up again after a pause; <see cref="IsGap"/> is the test for where.
    /// </summary>
    /// <remarks>
    /// The time is formatted in the browser's locale (12- or 24-hour as the user's system has it), relative to today.
    /// </remarks>
    [Transpose.Name("tss.ChatTimeDivider")]
    public sealed class ChatTimeDivider : IComponent, IHasMarginPadding
    {
        /// <summary>The pause after which a transcript usually draws a divider: one hour.</summary>
        public static readonly TimeSpan DefaultGap = TimeSpan.FromHours(1);

        private readonly HTMLElement _root;
        private readonly HTMLElement _label;
        private          DateTimeOffset _time;

        /// <summary>Initializes a divider for the given moment.</summary>
        /// <param name="time">When the messages below the divider start.</param>
        /// <param name="label">The text to show instead of the formatted time. None formats <paramref name="time"/>.</param>
        public ChatTimeDivider(DateTimeOffset time, string label = null)
        {
            _label = Div(Att("tss-chat-time-divider-label"));

            _root = Div(Att("tss-chat-time-divider"),
                        Div(Att("tss-chat-time-divider-line")),
                        _label,
                        Div(Att("tss-chat-time-divider-line")));

            _root.setAttribute("role", "separator");

            SetTime(time, label);
        }

        /// <inheritdoc />
        public string Margin  { get => _root.style.margin;  set => _root.style.margin  = value; }

        /// <inheritdoc />
        public string Padding { get => _root.style.padding; set => _root.style.padding = value; }

        /// <summary>The moment the divider stands for.</summary>
        public DateTimeOffset Time => _time;

        /// <summary>Changes the moment, and with it the text, of a divider already on screen.</summary>
        /// <param name="time">When the messages below the divider start.</param>
        /// <param name="label">The text to show instead of the formatted time. None formats <paramref name="time"/>.</param>
        public ChatTimeDivider SetTime(DateTimeOffset time, string label = null)
        {
            _time = time;

            var text = string.IsNullOrEmpty(label) ? Format(time) : label;

            _label.textContent = text;
            _root.setAttribute("aria-label", text);
            _root.title = FormatFull(time);

            return this;
        }

        /// <inheritdoc />
        public HTMLElement Render() => _root;

        /// <summary>
        /// Whether a transcript should draw a divider between a message written at <paramref name="previous"/> and the
        /// next one, written at <paramref name="next"/>: when the pause between them is longer than <paramref name="gap"/>
        /// (<see cref="DefaultGap"/> when none is given), or when the two fall on different days.
        /// </summary>
        public static bool IsGap(DateTimeOffset previous, DateTimeOffset next, TimeSpan? gap = null)
        {
            var threshold = gap ?? DefaultGap;

            if (threshold <= TimeSpan.Zero) return false;

            return (next - previous) > threshold || previous.LocalDateTime.Date != next.LocalDateTime.Date;
        }

        /// <summary>
        /// The divider's text for <paramref name="time"/>: the time alone today, "Yesterday", the weekday within the last
        /// week, and the date (with the year when it is not this year's) before that.
        /// </summary>
        public static string Format(DateTimeOffset time)
        {
            var local = time.LocalDateTime;
            var today = DateTime.Now.Date;
            var clock = LocaleTime(time);

            if (local.Date == today)              return clock;
            if (local.Date == today.AddDays(-1))  return "Yesterday".t() + " · " + clock;
            if (local.Date > today.AddDays(-7))   return Weekday(time)      + " · " + clock;
            if (local.Year == today.Year)         return MonthDay(time)     + " · " + clock;

            return MonthDayYear(time) + " · " + clock;
        }

        private static string FormatFull(DateTimeOffset time)
        {
            var ms = (double)time.ToUnixTimeMilliseconds();
            return Transpose.Script.Write<string>("new Date({0}).toLocaleString(undefined, { dateStyle: 'full', timeStyle: 'short' })", ms);
        }

        private static string LocaleTime(DateTimeOffset time)
        {
            var ms = (double)time.ToUnixTimeMilliseconds();
            return Transpose.Script.Write<string>("new Date({0}).toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' })", ms);
        }

        private static string Weekday(DateTimeOffset time)
        {
            var ms = (double)time.ToUnixTimeMilliseconds();
            return Transpose.Script.Write<string>("new Date({0}).toLocaleDateString(undefined, { weekday: 'short' })", ms);
        }

        private static string MonthDay(DateTimeOffset time)
        {
            var ms = (double)time.ToUnixTimeMilliseconds();
            return Transpose.Script.Write<string>("new Date({0}).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })", ms);
        }

        private static string MonthDayYear(DateTimeOffset time)
        {
            var ms = (double)time.ToUnixTimeMilliseconds();
            return Transpose.Script.Write<string>("new Date({0}).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })", ms);
        }
    }
}
