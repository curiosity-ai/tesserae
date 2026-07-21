using System;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// Abstract base class shared by the date, time and date-time pickers. Adds Min/Max/Step support on top of the
    /// typed-input <see cref="Input{TInput}"/> base.
    /// </summary>
    [Transpose.Name("tss.MomentPickerBase")]
    public abstract class MomentPickerBase<TMomentPicker, TMoment>
        : Input<TMomentPicker>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, IBindableComponent<TMoment?>
        where TMomentPicker : MomentPickerBase<TMomentPicker, TMoment>
        where TMoment : struct
    {
        private readonly SettableObservable<TMoment?> _momentObservable;

        protected MomentPickerBase(string type, string defaultText = null) : base(type, defaultText)
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
            InnerElement.style.alignItems = "center";

            _momentObservable = new SettableObservable<TMoment?>(ParseOrNull(defaultText));

            // Mirror the text-level events that Input&lt;TInput&gt; already wires up so the typed
            // observable stays in sync with user input.
            Changed      += (_, __) => UpdateMomentObservable();
            InputUpdated += (_, __) => UpdateMomentObservable();
        }

        private TMoment? ParseOrNull(string text) => string.IsNullOrEmpty(text) ? (TMoment?)null : FormatMoment(text);

        private void UpdateMomentObservable() => _momentObservable.Value = ParseOrNull(Text);

        /// <summary>
        /// Returns an observable that tracks the typed value of the picker
        /// (null when the input is empty).
        /// </summary>
        /// <remarks>
        /// The picker inherits a <see cref="IObservable{T}"/> of <see cref="string"/> from
        /// <see cref="Input{TInput}"/> via the inherited <c>AsObservable()</c>; this explicit
        /// interface implementation exposes the typed view used when binding to a
        /// <c>SettableObservable&lt;TMoment?&gt;</c>.
        /// </remarks>
        IObservable<TMoment?> IObservableComponent<TMoment?>.AsObservable() => _momentObservable;

        /// <summary>
        /// Programmatically updates the picker as part of a two-way binding.
        /// A <c>null</c> value clears the input.
        /// </summary>
        void IBindableComponent<TMoment?>.SetBoundValue(TMoment? value)
        {
            Text                    = value.HasValue ? FormatMoment(value.Value) : string.Empty;
            _momentObservable.Value = value;
        }

        protected TMoment Moment => FormatMoment(Text);

        /// <summary>
        /// Gets or sets the maximum value accepted by the component.
        /// </summary>
        public TMoment Max
        {
            get => FormatMoment(InnerElement.max);
            set => InnerElement.max = FormatMoment(value);
        }

        /// <summary>
        /// Gets or sets the minimum value accepted by the component.
        /// </summary>
        public TMoment Min
        {
            get => FormatMoment(InnerElement.min);
            set => InnerElement.min = FormatMoment(value);
        }

        /// <summary>
        /// Gets or sets the step increment used by the component.
        /// </summary>
        public int Step
        {
            get => int.Parse(InnerElement.step);
            set => InnerElement.step = value.ToString();
        }

        /// <summary>
        /// Sets the max of the component.
        /// </summary>
        public TMomentPicker SetMax(TMoment max)
        {
            Max = max;
            return (TMomentPicker)this;
        }

        /// <summary>
        /// Sets the min of the component.
        /// </summary>
        public TMomentPicker SetMin(TMoment min)
        {
            Min = min;
            return (TMomentPicker)this;
        }

        /// <summary>
        /// Sets the step of the component.
        /// </summary>
        public TMomentPicker SetStep(int step)
        {
            Step = step;
            return (TMomentPicker)this;
        }

        protected abstract string FormatMoment(TMoment moment);

        protected abstract TMoment FormatMoment(string moment);

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the font weight of the component.
        /// </summary>
        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the component.
        /// </summary>
        public TextAlign TextAlign
        {
            get
            {
                return ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Left);
            }
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        /// <summary>
        /// Gets or sets the CSS color (foreground) of the component.
        /// </summary>
        public string Foreground { get => InnerElement.style.color; set => InnerElement.style.color = value; }

    }
}