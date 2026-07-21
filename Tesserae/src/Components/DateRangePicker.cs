using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A composite picker for selecting a contiguous range of dates ("from" → "to"). Internally composed
    /// of two <see cref="DatePicker"/> instances joined by a visual separator. The two pickers stay in
    /// sync: setting the "from" date raises the "to" picker's minimum, and vice-versa.
    /// </summary>
    /// <example>
    /// <code>
    /// var range = UI.DateRangePicker(DateTime.Today, DateTime.Today.AddDays(7));
    /// range.OnChange((s, _) =>
    /// {
    ///     Console.WriteLine($"{s.From:d} to {s.To:d}");
    /// });
    /// </code>
    /// </example>
    [Transpose.Name("tss.DateRangePicker")]
    public sealed class DateRangePicker : IComponent, IHasMarginPadding, IBindableComponent<(DateTime? from, DateTime? to)>
    {
        private readonly DatePicker                                         _from;
        private readonly DatePicker                                         _to;
        private readonly HTMLDivElement                                     _container;
        private readonly SettableObservable<(DateTime? from, DateTime? to)> _observable;
        private          Action<DateRangePicker>                            _onChange;
        private          bool                                               _syncing;

        /// <summary>Creates a new date-range picker, optionally initialised with a range.</summary>
        /// <param name="from">Initial "from" date, or <c>null</c> for an empty start.</param>
        /// <param name="to">Initial "to" date, or <c>null</c> for an empty end.</param>
        public DateRangePicker(DateTime? from = null, DateTime? to = null)
        {
            _from = new DatePicker(from);
            _to   = new DatePicker(to);

            var separator = Span(_("tss-daterange-separator", text: "—"));

            _container = Div(_("tss-daterange-picker"), _from.Render(), separator, _to.Render());

            _observable = new SettableObservable<(DateTime? from, DateTime? to)>((from, to));

            _from.OnChange((_, __) => OnFromChanged());
            _to.OnChange((_,   __) => OnToChanged());

            if (from.HasValue) _to.Min = from.Value;
            if (to.HasValue) _from.Max = to.Value;
        }

        /// <summary>Gets the currently selected "from" date, or <c>null</c> if no date has been picked.</summary>
        public DateTime? From => HasValue(_from) ? _from.Date : (DateTime?)null;

        /// <summary>Gets the currently selected "to" date, or <c>null</c> if no date has been picked.</summary>
        public DateTime? To => HasValue(_to) ? _to.Date : (DateTime?)null;

        /// <summary>Gets the inner picker used for the "from" date. Useful for advanced configuration (min/max bounds, validation).</summary>
        public DatePicker FromPicker => _from;

        /// <summary>Gets the inner picker used for the "to" date. Useful for advanced configuration (min/max bounds, validation).</summary>
        public DatePicker ToPicker => _to;

        /// <summary>Gets or sets the outer container's CSS margin.</summary>
        public string Margin { get => _container.style.margin; set => _container.style.margin = value; }

        /// <summary>Gets or sets the outer container's CSS padding.</summary>
        public string Padding { get => _container.style.padding; set => _container.style.padding = value; }

        /// <summary>Sets the "from" date programmatically.</summary>
        public DateRangePicker SetFrom(DateTime date)
        {
            _from.Text = date.ToString("yyyy-MM-dd");
            return this;
        }

        /// <summary>Sets the "to" date programmatically.</summary>
        public DateRangePicker SetTo(DateTime date)
        {
            _to.Text = date.ToString("yyyy-MM-dd");
            return this;
        }

        /// <summary>
        /// Registers a callback fired whenever either the "from" or "to" date changes.
        /// </summary>
        public DateRangePicker OnChange(Action<DateRangePicker> handler)
        {
            _onChange += handler;
            return this;
        }

        /// <summary>
        /// Registers a callback fired whenever either the "from" or "to" date changes. The two-argument
        /// overload mirrors the convention used elsewhere in the toolkit (sender + event args).
        /// </summary>
        public DateRangePicker OnChange(Action<DateRangePicker, Event> handler)
        {
            return OnChange(self => handler(self, null));
        }

        /// <summary>Renders the picker's container element.</summary>
        public HTMLElement Render() => _container;

        private void OnFromChanged()
        {
            if (_syncing) return;
            _syncing = true;

            try
            {
                if (HasValue(_from))
                {
                    _to.Min = _from.Date;

                    if (HasValue(_to) && _to.Date < _from.Date)
                    {
                        _to.Text = _from.Text;
                    }
                }
                _observable.Value = (From, To);
                _onChange?.Invoke(this);
            }
            finally { _syncing = false; }
        }

        private void OnToChanged()
        {
            if (_syncing) return;
            _syncing = true;

            try
            {
                if (HasValue(_to))
                {
                    _from.Max = _to.Date;

                    if (HasValue(_from) && _from.Date > _to.Date)
                    {
                        _from.Text = _to.Text;
                    }
                }
                _observable.Value = (From, To);
                _onChange?.Invoke(this);
            }
            finally { _syncing = false; }
        }

        private static bool HasValue(DatePicker p) => !string.IsNullOrWhiteSpace(p.Text);

        /// <summary>
        /// Returns an observable that tracks the selected (from, to) range.
        /// </summary>
        public IObservable<(DateTime? from, DateTime? to)> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the range as part of a two-way binding.
        /// A null component clears the corresponding picker.
        /// </summary>
        public void SetBoundValue((DateTime? from, DateTime? to) value)
        {
            _syncing = true;

            try
            {
                _from.Text = value.from.HasValue ? value.from.Value.ToString("yyyy-MM-dd") : string.Empty;
                _to.Text   = value.to.HasValue ? value.to.Value.ToString("yyyy-MM-dd") : string.Empty;
                if (value.from.HasValue) _to.Min = value.from.Value;
                if (value.to.HasValue) _from.Max = value.to.Value;
            }
            finally { _syncing = false; }
            _observable.Value = value;
        }
    }
}