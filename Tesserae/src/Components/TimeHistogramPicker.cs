using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single bucket (time slice) inside the <see cref="TimeHistogramPicker"/>.
    /// </summary>
    [Transpose.Name("tss.TimeHistogramBucket")]
    public sealed class TimeHistogramBucket
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TimeHistogramBucket()
        {
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TimeHistogramBucket(DateTime start, DateTime end, int count)
        {
            Start = start;
            End   = end;
            Count = count;
        }

        /// <summary>
        /// Starts the component's operation.
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        public DateTime End { get; set; }
        /// <summary>
        /// Gets the number of items in the component.
        /// </summary>
        public int Count { get; set; }
    }

    [Transpose.Name("tss.TimeHistogramPicker")]
    public sealed class TimeHistogramPicker : IComponent, IBindableComponent<(DateTime from, DateTime to)>
    {
        private const double Millisecond = 1;
        private const double Second      = 1000 * Millisecond;
        private const double Minute      = 60 * Second;
        private const double Hour        = 60 * Minute;
        private const double Day         = 24 * Hour;
        private const double Month       = 30 * Day;
        private const double Year        = 365 * Day;

        // Mirrors the bars layout in tss.timehistogrampicker.css: each bar is at least
        // MinBarWidth wide with BarGap between bars, and the bars container is inset by
        // HorizontalInset on each side. Used to work out how many bars actually fit.
        private const double MinBarWidth     = 2;
        private const double BarGap          = 2;
        private const double HorizontalInset = 8;

        private readonly HTMLDivElement            _container;
        private readonly HTMLDivElement            _histogram;
        private readonly HTMLDivElement            _track;
        private readonly HTMLDivElement            _selection;
        private readonly HTMLDivElement            _leftThumb;
        private readonly HTMLDivElement            _rightThumb;
        private readonly HTMLDivElement            _emptyMessage;
        private readonly HTMLDivElement            _axis;
        private readonly HTMLSpanElement           _axisStart;
        private readonly HTMLSpanElement           _axisEnd;
        private readonly List<TimeHistogramBucket> _sourceBuckets = new List<TimeHistogramBucket>();
        private readonly List<TimeHistogramBucket> _buckets       = new List<TimeHistogramBucket>();
        private readonly List<HTMLDivElement>      _bars          = new List<HTMLDivElement>();
        private readonly ResizeObserver            _resizeObserver;

        private DateTime[]                      _values = new DateTime[0];
        private int                             _appliedFit = -1;
        private int                             _maxBuckets;
        private int                             _selectedStartIndex;
        private int                             _selectedEndIndex;
        private bool                            _isEnabled = true;
        private bool                            _usesPrecomputedBuckets;
        private bool                            _showBucketTooltipOnHover = true;
        private Func<DateTime, string>          _renderTime               = DateTimeRangeRenderer.RenderTime;
        private bool                            _usesCustomTimeRender;
        private Action<DateTime, DateTime, int> _rangeChanged;
        private Action                          _hideBarTooltip;

        private readonly SettableObservable<(DateTime from, DateTime to)> _observable
            = new SettableObservable<(DateTime from, DateTime to)>((default(DateTime), default(DateTime)));

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TimeHistogramPicker(DateTime[] values, int maxBuckets = 80)
        {
            _maxBuckets = Math.Max(1, maxBuckets);

            _histogram    = Div(Att("tss-time-histogram-picker-bars"));
            _emptyMessage = Div(Att("tss-time-histogram-picker-empty"), Span(Att(text: "No values")));

            _selection  = Div(Att("tss-time-histogram-picker-selection"));
            _leftThumb  = Div(Att("tss-time-histogram-picker-thumb tss-time-histogram-picker-thumb-left"));
            _rightThumb = Div(Att("tss-time-histogram-picker-thumb tss-time-histogram-picker-thumb-right"));
            _track      = Div(Att("tss-time-histogram-picker-track"), _selection, _leftThumb, _rightThumb);

            _axisStart = Span(Att("tss-time-histogram-picker-axis-label"));
            _axisEnd   = Span(Att("tss-time-histogram-picker-axis-label"));
            _axis      = Div(Att("tss-time-histogram-picker-axis"), _axisStart, _axisEnd);

            _container = Div(Att("tss-time-histogram-picker"), _histogram, _emptyMessage, _track, _axis);
            HookDragEvents(_leftThumb,  true);
            HookDragEvents(_rightThumb, false);
            HookKeyboardEvents(_leftThumb,  true);
            HookKeyboardEvents(_rightThumb, false);

            SetValues(values);

            // Re-bucket once the real width is known and whenever it changes, so the number of
            // rendered bars is always small enough to fit the chart instead of overflowing it.
            _resizeObserver = new ResizeObserver((entries, obs) => RefitToWidth());
            _resizeObserver.observe(_histogram);
            DomObserver.WhenRemoved(_container, () => _resizeObserver.unobserve(_histogram));
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TimeHistogramPicker(TimeHistogramBucket[] buckets)
            : this((DateTime[])null)
        {
            SetBuckets(buckets);
        }

        /// <summary>
        /// Gets or sets the selected from.
        /// </summary>
        public DateTime SelectedFrom => HasSelection ? _buckets[_selectedStartIndex].Start : default;
        /// <summary>
        /// Gets or sets the selected to.
        /// </summary>
        public DateTime SelectedTo => HasSelection ? _buckets[_selectedEndIndex].End : default;
        /// <summary>
        /// Gets or sets the selected count.
        /// </summary>
        public int SelectedCount => HasSelection ? CountSelectedValues() : 0;

        private bool HasSelection => _buckets.Count > 0 && _selectedStartIndex >= 0 && _selectedEndIndex >= _selectedStartIndex;

        /// <summary>
        /// Sets the values of the component.
        /// </summary>
        public TimeHistogramPicker SetValues(DateTime[] values)
        {
            _usesPrecomputedBuckets = false;
            _values                 = values?.ToArray() ?? new DateTime[0];
            Array.Sort(_values);
            BuildBuckets();
            ResetSelection();
            return this;
        }

        /// <summary>
        /// Sets the buckets of the component.
        /// </summary>
        public TimeHistogramPicker SetBuckets(TimeHistogramBucket[] buckets)
        {
            _usesPrecomputedBuckets = true;
            _values                 = new DateTime[0];
            _sourceBuckets.Clear();

            foreach (var bucket in (buckets ?? new TimeHistogramBucket[0]).Where(b => b != null).OrderBy(b => b.Start).ThenBy(b => b.End))
            {
                _sourceBuckets.Add(new TimeHistogramBucket(bucket.Start, bucket.End, Math.Max(0, bucket.Count)));
            }

            ResetSelection();
            return this;
        }

        /// <summary>
        /// Sets the range of the component.
        /// </summary>
        public TimeHistogramPicker SetRange(DateTime from, DateTime to)
        {
            if (_buckets.Count == 0)
            {
                return this;
            }

            if (to < from)
            {
                var tmp = from;
                from = to;
                to   = tmp;
            }

            _selectedStartIndex = FindBucketIndex(from, true);
            _selectedEndIndex   = FindBucketIndex(to,   false);

            if (_selectedEndIndex < _selectedStartIndex)
            {
                _selectedEndIndex = _selectedStartIndex;
            }

            UpdateSelection(true);
            return this;
        }

        /// <summary>
        /// Configures the max buckets on the component.
        /// </summary>
        public TimeHistogramPicker MaxBuckets(int maxBuckets)
        {
            _maxBuckets = Math.Max(1, maxBuckets);

            if (!_usesPrecomputedBuckets)
            {
                BuildBuckets();
            }

            RebuildDisplayBuckets();
            _selectedStartIndex = _buckets.Count == 0 ? -1 : Math.Min(_selectedStartIndex,                              _buckets.Count - 1);
            _selectedEndIndex   = _buckets.Count == 0 ? -1 : Math.Min(Math.Max(_selectedEndIndex, _selectedStartIndex), _buckets.Count - 1);
            RenderBuckets();
            UpdateSelection(true);
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the range changed event fires.
        /// </summary>
        public TimeHistogramPicker OnRangeChanged(Action<DateTime, DateTime, int> handler)
        {
            _rangeChanged += handler;
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given custom time render.
        /// </summary>
        public TimeHistogramPicker WithCustomTimeRender(Func<DateTime, string> renderTime)
        {
            _renderTime           = renderTime ?? DateTimeRangeRenderer.RenderTime;
            _usesCustomTimeRender = renderTime != null;
            RenderBuckets();
            UpdateSelection(false);
            return this;
        }

        /// <summary>
        /// Shows the count on hover.
        /// </summary>
        public TimeHistogramPicker ShowCountOnHover(bool value = true)
        {
            return ShowBucketTooltipOnHover(value);
        }

        /// <summary>
        /// Shows the bucket tooltip on hover.
        /// </summary>
        public TimeHistogramPicker ShowBucketTooltipOnHover(bool value = true)
        {
            _showBucketTooltipOnHover = value;
            RenderBuckets();
            UpdateSelection(false);
            return this;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public TimeHistogramPicker Disabled(bool value = true)
        {
            _isEnabled = !value;
            _container.UpdateClassIf(value, "tss-disabled");
            _leftThumb.tabIndex  = _isEnabled ? 0 : -1;
            _rightThumb.tabIndex = _isEnabled ? 0 : -1;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;

        private void ResetSelection()
        {
            RebuildDisplayBuckets();
            _selectedStartIndex = _buckets.Count == 0 ? -1 : 0;
            _selectedEndIndex   = _buckets.Count - 1;
            RenderBuckets();
            UpdateSelection(false);
        }

        // Aggregates the full set of source buckets down to a number of display buckets that
        // actually fits the chart, widening each bucket's time span by merging adjacent ones.
        private void RebuildDisplayBuckets()
        {
            _buckets.Clear();

            var fit = ComputeFitBucketCount();
            _appliedFit = fit;

            if (_sourceBuckets.Count == 0)
            {
                return;
            }

            if (_sourceBuckets.Count <= fit)
            {
                foreach (var bucket in _sourceBuckets)
                {
                    _buckets.Add(new TimeHistogramBucket(bucket.Start, bucket.End, bucket.Count));
                }
                return;
            }

            var groupSize = (int)Math.Ceiling(_sourceBuckets.Count / (double)fit);

            for (var i = 0; i < _sourceBuckets.Count; i += groupSize)
            {
                var end   = Math.Min(i + groupSize, _sourceBuckets.Count);
                var count = 0;

                for (var j = i; j < end; j++)
                {
                    count += _sourceBuckets[j].Count;
                }

                _buckets.Add(new TimeHistogramBucket(_sourceBuckets[i].Start, _sourceBuckets[end - 1].End, count));
            }
        }

        // How many bars fit the bars container at its current width; falls back to the configured
        // maximum while the element has no layout yet (e.g. before it is mounted).
        private int ComputeFitBucketCount()
        {
            var available = _histogram.clientWidth - (2 * HorizontalInset);

            if (available <= 0)
            {
                return _maxBuckets;
            }

            var fit = (int)Math.Floor((available + BarGap) / (MinBarWidth + BarGap));
            return Clamp(fit, 1, _maxBuckets);
        }

        // Re-aggregates the buckets to the available width while keeping the current selection
        // (mapped back by date), invoked by the ResizeObserver as the chart is sized / resized.
        private void RefitToWidth()
        {
            if (_sourceBuckets.Count == 0)
            {
                return;
            }

            if (ComputeFitBucketCount() == _appliedFit)
            {
                return;
            }

            var hadSelection = HasSelection;
            var wasFullRange = hadSelection && _selectedStartIndex == 0 && _selectedEndIndex == _buckets.Count - 1;
            var from         = hadSelection ? SelectedFrom : default;
            var to           = hadSelection ? SelectedTo   : default;

            RebuildDisplayBuckets();

            if (_buckets.Count == 0)
            {
                _selectedStartIndex = -1;
                _selectedEndIndex   = -1;
            }
            else if (hadSelection && !wasFullRange)
            {
                _selectedStartIndex = FindBucketIndex(from, true);
                _selectedEndIndex   = FindBucketIndex(to,   false);

                if (_selectedEndIndex < _selectedStartIndex)
                {
                    _selectedEndIndex = _selectedStartIndex;
                }
            }
            else
            {
                _selectedStartIndex = 0;
                _selectedEndIndex   = _buckets.Count - 1;
            }

            RenderBuckets();
            UpdateSelection(false);
        }

        private void BuildBuckets()
        {
            _sourceBuckets.Clear();

            if (_values.Length == 0)
            {
                return;
            }

            var min = _values[0];
            var max = _values[_values.Length - 1];

            if (_values.Length == 1 || min == max)
            {
                _sourceBuckets.Add(new TimeHistogramBucket(min, max, _values.Length));
                return;
            }

            var totalMilliseconds = Math.Max(1, (max - min).TotalMilliseconds);
            var bucketWidth       = ChooseBucketWidth(totalMilliseconds);
            var bucketCount       = Math.Max(1, (int)Math.Ceiling(totalMilliseconds / bucketWidth));

            if (bucketCount > _maxBuckets)
            {
                bucketCount = _maxBuckets;
                bucketWidth = totalMilliseconds / bucketCount;
            }

            for (var i = 0; i < bucketCount; i++)
            {
                var start = min.AddMilliseconds(i * bucketWidth);
                var end   = i == bucketCount - 1 ? max : min.AddMilliseconds((i + 1) * bucketWidth);
                _sourceBuckets.Add(new TimeHistogramBucket(start, end, 0));
            }

            var bucketIndex = 0;

            for (var i = 0; i < _values.Length; i++)
            {
                while (bucketIndex < _sourceBuckets.Count - 1 && _values[i] >= _sourceBuckets[bucketIndex].End)
                {
                    bucketIndex++;
                }

                _sourceBuckets[bucketIndex].Count++;
            }
        }

        private double ChooseBucketWidth(double totalMilliseconds)
        {
            var niceWidths = new[]
            {
                1 * Second, 5 * Second, 10 * Second, 30 * Second,
                1 * Minute, 5 * Minute, 15 * Minute, 30 * Minute,
                1 * Hour, 3 * Hour, 6 * Hour, 12 * Hour,
                1 * Day, 2 * Day, 7 * Day, 14 * Day,
                1 * Month, 3 * Month, 6 * Month,
                1 * Year, 2 * Year, 5 * Year, 10 * Year
            };

            foreach (var width in niceWidths)
            {
                if (Math.Ceiling(totalMilliseconds / width) <= _maxBuckets)
                {
                    return width;
                }
            }

            return totalMilliseconds / _maxBuckets;
        }

        private void RenderBuckets()
        {
            HideBarTooltip();
            _histogram.RemoveChildElements();
            _bars.Clear();

            _container.UpdateClassIf(_buckets.Count == 0, "tss-time-histogram-picker-empty-state");
            _leftThumb.tabIndex  = _isEnabled && _buckets.Count > 0 ? 0 : -1;
            _rightThumb.tabIndex = _isEnabled && _buckets.Count > 0 ? 0 : -1;

            if (_buckets.Count == 0)
            {
                _axisStart.textContent = "";
                _axisEnd.textContent   = "";
                return;
            }

            var maxCount = Math.Max(1, _buckets.Max(b => b.Count));

            for (var i = 0; i < _buckets.Count; i++)
            {
                var bar       = Div(Att("tss-time-histogram-picker-bar"));
                var height    = _buckets[i].Count == 0 ? 0 : Math.Max(2, (_buckets[i].Count / (double)maxCount) * 100);
                var rangeText = RenderRange(_buckets[i].Start, _buckets[i].End);
                var countText = _buckets[i].Count.ToString("n0");
                bar.style.height = $"{height:0.##}%";
                bar.setAttribute("aria-label", $"{rangeText}, {countText}");

                if (_showBucketTooltipOnHover)
                {
                    HookBarTooltip(bar, rangeText, countText);
                }
                _histogram.appendChild(bar);
                _bars.Add(bar);
            }

            _axisStart.textContent = RenderTime(_buckets[0].Start);
            _axisEnd.textContent   = RenderTime(_buckets[_buckets.Count - 1].End);
        }

        private void HookBarTooltip(HTMLElement bar, string rangeText, string countText)
        {
            bar.onmouseenter = _ => ShowBarTooltip(bar, rangeText, countText);
            bar.onmouseleave = _ => HideBarTooltip();
        }

        private void ShowBarTooltip(HTMLElement bar, string rangeText, string countText)
        {
            HideBarTooltip();

            var tooltip = Div(Att("tss-time-histogram-picker-tooltip"),
                Span(Att("tss-time-histogram-picker-tooltip-range", text: rangeText)),
                Span(Att("tss-time-histogram-picker-tooltip-count", text: countText)));
            Tippy.ShowFor(bar, tooltip, out _hideBarTooltip, placement: TooltipPlacement.Top, delayShow: 0, delayHide: 0, maxWidth: 260, arrow: true);
        }

        private void HideBarTooltip()
        {
            _hideBarTooltip?.Invoke();
            _hideBarTooltip = null;
        }

        private void UpdateSelection(bool raiseChanged)
        {
            if (!HasSelection)
            {
                _selection.style.left  = "0%";
                _selection.style.width = "0%";
                _leftThumb.style.left  = "0%";
                _rightThumb.style.left = "0%";
                return;
            }

            var leftPercent  = StartIndexToPercent(_selectedStartIndex);
            var rightPercent = EndIndexToPercent(_selectedEndIndex);
            _selection.style.left  = $"{leftPercent:0.##}%";
            _selection.style.width = $"{Math.Max(0, rightPercent - leftPercent):0.##}%";
            _leftThumb.style.left  = $"{leftPercent:0.##}%";
            _rightThumb.style.left = $"{rightPercent:0.##}%";
            _leftThumb.setAttribute("aria-valuemin",  "0");
            _leftThumb.setAttribute("aria-valuemax",  (_buckets.Count - 1).ToString());
            _leftThumb.setAttribute("aria-valuenow",  _selectedStartIndex.ToString());
            _leftThumb.setAttribute("aria-valuetext", RenderTime(SelectedFrom));
            _leftThumb.title = RenderTime(SelectedFrom);
            _rightThumb.setAttribute("aria-valuemin",  "0");
            _rightThumb.setAttribute("aria-valuemax",  (_buckets.Count - 1).ToString());
            _rightThumb.setAttribute("aria-valuenow",  _selectedEndIndex.ToString());
            _rightThumb.setAttribute("aria-valuetext", RenderTime(SelectedTo));
            _rightThumb.title = RenderTime(SelectedTo);

            for (var i = 0; i < _bars.Count; i++)
            {
                _bars[i].UpdateClassIf(i >= _selectedStartIndex && i <= _selectedEndIndex, "tss-selected");
            }

            if (raiseChanged)
            {
                _observable.Value = (SelectedFrom, SelectedTo);
                _rangeChanged?.Invoke(SelectedFrom, SelectedTo, SelectedCount);
            }
        }

        /// <summary>
        /// Returns an observable that tracks the currently-selected (from, to) range.
        /// </summary>
        public IObservable<(DateTime from, DateTime to)> AsObservable() => _observable;

        /// <summary>
        /// Programmatically sets the selected range as part of a two-way binding.
        /// </summary>
        public void SetBoundValue((DateTime from, DateTime to) value) => SetRange(value.from, value.to);

        private double StartIndexToPercent(int index)
        {
            if (_buckets.Count == 0)
            {
                return 0;
            }

            return (index / (double)_buckets.Count) * 100;
        }

        private double EndIndexToPercent(int index)
        {
            if (_buckets.Count == 0)
            {
                return 0;
            }

            return ((index + 1) / (double)_buckets.Count) * 100;
        }

        private void HookDragEvents(HTMLDivElement thumb, bool isLeft)
        {
            DOMRect rect = null;

            thumb.onmousedown += me =>
            {
                if (!_isEnabled || _buckets.Count == 0)
                {
                    return;
                }

                rect               =  _track.getBoundingClientRect().As<DOMRect>();
                window.onmousemove += Move;
                window.onmouseup   += Stop;
                StopEvent(me);
            };

            void Move(MouseEvent me)
            {
                if (rect == null)
                {
                    return;
                }

                SetThumbFromClientX(me.clientX, rect, isLeft);
                StopEvent(me);
            }

            void Stop(MouseEvent me)
            {
                window.onmousemove -= Move;
                window.onmouseup   -= Stop;
                rect               =  null;
                StopEvent(me);
            }
        }

        private void HookKeyboardEvents(HTMLDivElement thumb, bool isLeft)
        {
            thumb.tabIndex = 0;
            thumb.setAttribute("role",       "slider");
            thumb.setAttribute("aria-label", isLeft ? "Start time" : "End time");

            thumb.addEventListener("keydown", ev =>
            {
                if (!_isEnabled || _buckets.Count == 0)
                {
                    return;
                }

                var keyEvent = ev.As<KeyboardEvent>();
                var delta    = keyEvent.key == "ArrowLeft" || keyEvent.key == "ArrowDown" ? -1 : keyEvent.key == "ArrowRight" || keyEvent.key == "ArrowUp" ? 1 : 0;

                if (delta == 0)
                {
                    return;
                }

                if (isLeft)
                {
                    _selectedStartIndex = Clamp(_selectedStartIndex + delta, 0, _selectedEndIndex);
                }
                else
                {
                    _selectedEndIndex = Clamp(_selectedEndIndex + delta, _selectedStartIndex, _buckets.Count - 1);
                }

                UpdateSelection(true);
                StopEvent(ev);
            });
        }

        private void SetThumbFromClientX(double clientX, DOMRect rect, bool isLeft)
        {
            var width   = Math.Max(1, rect.width);
            var percent = Math.Max(0, Math.Min(1, (clientX - rect.left) / width));
            var index   = isLeft ? (int)Math.Round(percent * _buckets.Count) : (int)Math.Round(percent * _buckets.Count) - 1;

            if (isLeft)
            {
                _selectedStartIndex = Clamp(index, 0, _selectedEndIndex);
            }
            else
            {
                _selectedEndIndex = Clamp(index, _selectedStartIndex, _buckets.Count - 1);
            }

            UpdateSelection(true);
        }

        private int FindBucketIndex(DateTime value, bool preferStart)
        {
            if (value <= _buckets[0].Start)
            {
                return 0;
            }

            for (var i = 0; i < _buckets.Count; i++)
            {
                if (value >= _buckets[i].Start && value <= _buckets[i].End)
                {
                    return i;
                }

                if (preferStart && value < _buckets[i].Start)
                {
                    return i;
                }
            }

            return _buckets.Count - 1;
        }

        private int CountSelectedValues()
        {
            var count = 0;

            for (var i = _selectedStartIndex; i <= _selectedEndIndex; i++)
            {
                count += _buckets[i].Count;
            }

            return count;
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private string RenderTime(DateTime date)
        {
            return _renderTime(date) ?? "";
        }

        private string RenderRange(DateTime from, DateTime to)
        {
            return DateTimeRangeRenderer.RenderRange(from, to, _usesCustomTimeRender ? _renderTime : null);
        }
    }
}