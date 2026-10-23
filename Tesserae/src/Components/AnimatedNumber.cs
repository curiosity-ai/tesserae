using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>How an <see cref="AnimatedNumber"/> moves from one value to the next.</summary>
    [Transpose.Name("tss.AnimatedNumberStyle")]
    public enum AnimatedNumberStyle
    {
        /// <summary>Counts through the values in between, fast then settling: a total, a percentage, a price.</summary>
        Count,
        /// <summary>The new value slides in, from below when it went up and from above when it went down: a counter, a character count.</summary>
        Tick
    }

    /// <summary>
    /// A number that animates when it changes, so the change is seen rather than missed: a total recalculated after
    /// a selection, an upload percentage, a like count, characters left. Digits are tabular, so the text does not
    /// shift as they change. Under reduced motion the new value simply appears.
    /// </summary>
    [Transpose.Name("tss.AnimatedNumber")]
    public sealed class AnimatedNumber : ComponentBase<AnimatedNumber, HTMLSpanElement>, IBindableComponent<double>
    {
        private readonly HTMLSpanElement            _valueSpan;
        private readonly HTMLSpanElement            _outgoingSpan;
        private readonly SettableObservable<double> _observable;

        private string              _format;
        private string              _prefix = "";
        private string              _suffix = "";
        private int                 _durationMs = 650;
        private AnimatedNumberStyle _style = AnimatedNumberStyle.Count;
        private double              _shown;
        private int                 _frameGeneration;

        /// <summary>
        /// Creates an animated number.
        /// </summary>
        /// <param name="value">The starting value, shown without animation.</param>
        /// <param name="format">A .NET numeric format string ("N0", "0.0", "P0", ...).</param>
        public AnimatedNumber(double value = 0, string format = "N0")
        {
            _format     = format ?? "N0";
            _shown      = value;
            _observable = new SettableObservable<double>(value);
            _valueSpan    = Span(Att("tss-animatednumber-value"));
            _outgoingSpan = Span(Att("tss-animatednumber-outgoing"));
            _outgoingSpan.setAttribute("aria-hidden", "true");
            InnerElement  = Span(Att("tss-animatednumber"), _valueSpan, _outgoingSpan);
            Paint(value);
        }

        /// <summary>Gets or sets the value; setting it animates to the new value.</summary>
        public double Value
        {
            get => _observable.Value;
            set => SetValue(value);
        }

        /// <summary>Animates to <paramref name="value"/>.</summary>
        public AnimatedNumber SetValue(double value)
        {
            var previous = _observable.Value;
            _observable.Value = value;
            AnimateTo(previous, value);
            return this;
        }

        /// <inheritdoc />
        public IObservable<double> AsObservable() => _observable;

        /// <inheritdoc />
        public void SetBoundValue(double value) => SetValue(value);

        /// <summary>Follows <paramref name="source"/>: every value it takes is animated to.</summary>
        public AnimatedNumber Bind(IObservable<double> source)
        {
            source.Observe(v => SetValue(v));
            return this;
        }

        /// <summary>Sets the .NET numeric format string used to print the value.</summary>
        public AnimatedNumber Format(string format)
        {
            _format = format ?? "N0";
            Paint(_shown);
            return this;
        }

        /// <summary>Text printed before the number (a currency sign).</summary>
        public AnimatedNumber Prefix(string prefix)
        {
            _prefix = prefix ?? "";
            Paint(_shown);
            return this;
        }

        /// <summary>Text printed after the number (a unit, "%").</summary>
        public AnimatedNumber Suffix(string suffix)
        {
            _suffix = suffix ?? "";
            Paint(_shown);
            return this;
        }

        /// <summary>How long a <see cref="AnimatedNumberStyle.Count"/> takes, in milliseconds.</summary>
        public AnimatedNumber Duration(int milliseconds)
        {
            _durationMs = Math.Max(0, milliseconds);
            return this;
        }

        /// <summary>Counts through the values in between (the default).</summary>
        public AnimatedNumber Count()
        {
            _style = AnimatedNumberStyle.Count;
            return this;
        }

        /// <summary>Slides the new value in instead of counting to it.</summary>
        public AnimatedNumber Tick()
        {
            _style = AnimatedNumberStyle.Tick;
            return this;
        }

        private void AnimateTo(double from, double to)
        {
            var generation = ++_frameGeneration;

            if (from == to || !InnerElement.IsMounted() || MicroInteractionExtensions.PrefersReducedMotion || _durationMs == 0)
            {
                Paint(to);
                return;
            }

            if (_style == AnimatedNumberStyle.Tick)
            {
                // The old value leaves the way the number moved (up when it grew) while the new one arrives behind
                // it: two class flips, the motion itself is CSS (tss.animatednumber.css).
                _outgoingSpan.textContent = _valueSpan.textContent;
                Paint(to);
                var up = to > from;
                _valueSpan.classList.remove("tss-animatednumber-up", "tss-animatednumber-down");
                _outgoingSpan.classList.remove("tss-animatednumber-leave-up", "tss-animatednumber-leave-down");
                MicroInteractionExtensions.Play(_valueSpan, up ? "tss-animatednumber-up" : "tss-animatednumber-down");
                MicroInteractionExtensions.Play(_outgoingSpan, up ? "tss-animatednumber-leave-up" : "tss-animatednumber-leave-down");
                return;
            }

            // Count from what is on screen, so a value that changes mid-count carries on from there.
            var start     = _shown;
            var startTime = performance.now();

            FrameRequestCallback frame = null;

            frame = now =>
            {
                if (generation != _frameGeneration) return;

                var t     = Math.Min(1, (now - startTime) / _durationMs);
                var eased = 1 - Math.Pow(1 - t, 3);
                Paint(t >= 1 ? to : start + (to - start) * eased);

                if (t < 1) window.requestAnimationFrame(frame);
            };

            window.requestAnimationFrame(frame);
        }

        private void Paint(double value)
        {
            _shown = value;
            _valueSpan.textContent = _prefix + value.ToString(_format) + _suffix;
        }

        /// <summary>Renders the component's root HTML element.</summary>
        public override HTMLElement Render() => InnerElement;
    }
}
