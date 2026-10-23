using System;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Which way a value moved.
    /// </summary>
    [Enum(Emit.StringName)]
    public enum TrendDirection
    {
        Up,
        Down,
        Flat
    }

    /// <summary>
    /// Whether a movement is good news. The direction and the sentiment are independent: a falling cost is
    /// <see cref="Good"/>, a falling revenue is <see cref="Bad"/>, and a figure with no target to judge it
    /// against is <see cref="Neutral"/>.
    /// </summary>
    [Enum(Emit.StringName)]
    public enum TrendSentiment
    {
        Neutral,
        Good,
        Bad
    }

    /// <summary>
    /// A small pill saying how a figure moved - an arrow and the size of the change ("↓ £31", "↑ 1.7pp") -
    /// colored by whether that movement is good for the metric rather than by its direction: green is
    /// good news, red is bad news, grey is a figure with no good direction.
    /// </summary>
    /// <remarks>
    /// Use <see cref="For"/> to derive both from the sign of the change and which way is better.
    /// </remarks>
    [Transpose.Name("tss.TrendBadge")]
    public sealed class TrendBadge : ComponentBase<TrendBadge, HTMLElement>
    {
        private readonly HTMLElement _arrow;
        private readonly HTMLElement _text;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TrendBadge(string text, TrendDirection direction = TrendDirection.Flat, TrendSentiment sentiment = TrendSentiment.Neutral)
        {
            _arrow = I(Att("tss-trendbadge-arrow"));
            _text  = Span(Att("tss-trendbadge-text", text: text ?? string.Empty));

            InnerElement = Span(Att("tss-trendbadge"), _arrow, _text);

            Direction(direction);
            Sentiment(sentiment);
        }

        /// <summary>
        /// Builds a badge from the signed size of a change: the arrow follows its sign, and the color says
        /// whether that is good news given which way is better for the metric.
        /// </summary>
        /// <param name="change">The signed change; zero draws a flat, neutral badge.</param>
        /// <param name="text">The text to show - the formatted change, typically without its sign.</param>
        /// <param name="higherIsBetter">False for a metric where less is better (cost, churn, errors).</param>
        public static TrendBadge For(double change, string text, bool higherIsBetter = true)
        {
            if (change == 0)
            {
                return new TrendBadge(text, TrendDirection.Flat, TrendSentiment.Neutral);
            }

            var up = change > 0;

            return new TrendBadge(text, up ? TrendDirection.Up : TrendDirection.Down, up == higherIsBetter ? TrendSentiment.Good : TrendSentiment.Bad);
        }

        /// <summary>
        /// Sets the text of the badge.
        /// </summary>
        public TrendBadge SetText(string text)
        {
            _text.innerText = text ?? string.Empty;

            return this;
        }

        /// <summary>
        /// Sets which way the arrow points.
        /// </summary>
        public TrendBadge Direction(TrendDirection direction)
        {
            var icon = direction == TrendDirection.Up   ? UIcons.ArrowSmallUp
                     : direction == TrendDirection.Down ? UIcons.ArrowSmallDown
                     :                                    UIcons.MinusSmall;

            _arrow.className = "tss-trendbadge-arrow " + Tesserae.Icon.Transform(icon, UIconsWeight.Regular);

            return this;
        }

        /// <summary>
        /// Sets whether the movement is good or bad news, which decides the color.
        /// </summary>
        public TrendBadge Sentiment(TrendSentiment sentiment)
        {
            InnerElement.UpdateClassIf(sentiment == TrendSentiment.Good, "tss-trendbadge-good");
            InnerElement.UpdateClassIf(sentiment == TrendSentiment.Bad,  "tss-trendbadge-bad");

            return this;
        }

        /// <summary>The movement is good news.</summary>
        public TrendBadge Good() => Sentiment(TrendSentiment.Good);

        /// <summary>The movement is bad news.</summary>
        public TrendBadge Bad() => Sentiment(TrendSentiment.Bad);

        /// <summary>The movement is neither: a figure with no target or no good direction.</summary>
        public TrendBadge Neutral() => Sentiment(TrendSentiment.Neutral);

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
