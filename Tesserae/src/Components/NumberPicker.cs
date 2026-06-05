namespace Tesserae
{
    /// <summary>
    /// A form input for entering a numeric value, with optional min/max bounds and step.
    /// </summary>
    [H5.Name("tss.NumberPicker")]
    public class NumberPicker : Input<NumberPicker>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, IBindableComponent<int>
    {
        private readonly SettableObservable<int> _intObservable;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public NumberPicker(int defaultValue = 0) : base("number", defaultValue.ToString())
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
            InnerElement.style.alignItems = "center";

            _intObservable = new SettableObservable<int>(defaultValue);

            // Keep the int-shaped observable in sync with the text-level events that
            // Input&lt;NumberPicker&gt; already wires up.
            Changed      += (_, __) => UpdateIntObservable();
            InputUpdated += (_, __) => UpdateIntObservable();
        }

        private void UpdateIntObservable()
        {
            if (int.TryParse(Text, out var parsed))
            {
                _intObservable.Value = parsed;
            }
        }

        /// <summary>
        /// Gets or sets the current value of the component.
        /// </summary>
        public int Value => int.Parse(Text);

        /// <summary>
        /// Returns an observable that tracks the numeric value of the component.
        /// </summary>
        /// <remarks>
        /// NumberPicker inherits a string observable from <see cref="Input{TInput}"/>, accessible via
        /// the inherited <c>AsObservable()</c> method. This explicit interface implementation provides
        /// an additional <c>IObservable&lt;int&gt;</c> view used when binding to a
        /// <c>SettableObservable&lt;int&gt;</c>.
        /// </remarks>
        IObservable<int> IObservableComponent<int>.AsObservable() => _intObservable;

        /// <summary>
        /// Programmatically updates the numeric value as part of a two-way binding.
        /// </summary>
        void IBindableComponent<int>.SetBoundValue(int value)
        {
            Text                 = value.ToString();
            _intObservable.Value = value;
        }

        /// <summary>
        /// Gets or sets the maximum value accepted by the component.
        /// </summary>
        public int Max
        {
            get => int.Parse(InnerElement.max);
            set => InnerElement.max = value.ToString();
        }

        /// <summary>
        /// Gets or sets the minimum value accepted by the component.
        /// </summary>
        public int Min
        {
            get => int.Parse(InnerElement.min);
            set => InnerElement.min = value.ToString();
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
        public NumberPicker SetMax(int max)
        {
            Max = max;
            return this;
        }

        /// <summary>
        /// Sets the min of the component.
        /// </summary>
        public NumberPicker SetMin(int min)
        {
            Min = min;
            return this;
        }

        /// <summary>
        /// Sets the step of the component.
        /// </summary>
        public NumberPicker SetStep(int step)
        {
            Step = step;
            return this;
        }

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
        public TextSize Size
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
        public TextWeight Weight
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