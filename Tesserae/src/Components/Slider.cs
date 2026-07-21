using static Transpose.Core.dom;
using static Tesserae.UI;
using System.Linq;

namespace Tesserae
{

    /// <summary>
    /// A slider component.
    /// </summary>
    [Transpose.Name("tss.Slider")]
    public sealed class Slider : ComponentBase<Slider, HTMLInputElement>, IBindableComponent<int>
    {
        private readonly HTMLLabelElement        _outerLabel;
        private readonly HTMLDivElement          _outerDiv;
        private readonly HTMLDivElement          _fakeDiv;
        private readonly SettableObservable<int> _observable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Slider"/> class.
        /// </summary>
        /// <param name="val">The initial value.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <param name="step">The step size.</param>
        public Slider(int val = 0, int min = 0, int max = 100, int step = 10)
        {
            InnerElement           = document.createElement("input") as HTMLInputElement;
            InnerElement.className = "tss-slider";
            InnerElement.value     = val.ToString();
            InnerElement.min       = min.ToString();
            InnerElement.max       = max.ToString();
            InnerElement.step      = step.ToString();
            InnerElement.type      = "range";
            InnerElement.setAttribute("role",          "slider");
            InnerElement.setAttribute("aria-valuemin", min.ToString());
            InnerElement.setAttribute("aria-valuemax", max.ToString());
            InnerElement.setAttribute("aria-valuenow", val.ToString());

            _observable = new SettableObservable<int>(val);

            AttachClick();
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();

            // Subscribe to the underlying events so the observable (and the
            // exposed aria value) stay in sync on user input, not just when the
            // value is set programmatically.
            Changed += (_, __) => _observable.Value = Value;
            InputUpdated += (_, __) =>
            {
                InnerElement.setAttribute("aria-valuenow", Value.ToString());
                _observable.Value = Value;
            };

            if (navigator.userAgent.IndexOf("AppleWebKit") != -1)
            {
                _fakeDiv = Div(Att("tss-slider-fake-progress"));
                double percent = ((double)(val - min) / (max - min)) * 100.0;
                _fakeDiv.style.width = $"{percent:0.##}%";

                InputUpdated += (e, s) =>
                {
                    percent = UpdateFakeProgress();
                };

                _outerLabel = Label(Att("tss-slider-container"), InnerElement, Div(Att("tss-slider-fake-background")), _fakeDiv);
                InnerElement.classList.add("tss-fake");
            }
            else
            {
                _outerLabel               = Label(Att("tss-slider-container"), InnerElement);
                InnerElement.style.height = "8px";
            }

            _outerDiv = Div(Att("tss-slider-div"), _outerLabel);
        }

        private double UpdateFakeProgress()
        {
            double percent = ((double)(Value - Min) / (Max - Min)) * 100.0;

            if (Orientation == SliderOrientation.Vertical)
            {
                // The fill grows along the vertical axis; let the CSS drive the
                // track thickness and clear any horizontal value left over from a
                // previous orientation.
                _fakeDiv.style.height = $"{percent:0.##}%";
                _fakeDiv.style.width  = "";
            }
            else
            {
                _fakeDiv.style.width  = $"{percent:0.##}%";
                _fakeDiv.style.height = "";
            }
            return percent;
        }

        /// <summary>
        /// Gets or sets the orientation of the slider.
        /// </summary>
        public SliderOrientation Orientation
        {
            get => _outerLabel.classList.contains("tss-vertical") ? SliderOrientation.Vertical : SliderOrientation.Horizontal;
            set
            {
                if (value == SliderOrientation.Vertical)
                {
                    _outerLabel.classList.add("tss-vertical");
                    _outerDiv.classList.add("tss-slider-div-vertical");
                    InnerElement.setAttribute("aria-orientation", "vertical");
                }
                else
                {
                    _outerLabel.classList.remove("tss-vertical");
                    _outerDiv.classList.remove("tss-slider-div-vertical");
                    InnerElement.setAttribute("aria-orientation", "horizontal");
                }

                if (_fakeDiv is object)
                {
                    // WebKit/Chromium path: the fake fill overlay draws the track.
                    // Re-project the current value onto the new axis (this also
                    // clears the inline size on the axis the CSS now controls).
                    UpdateFakeProgress();
                }
                else
                {
                    // Firefox native path: the constructor pins an inline height to
                    // give the horizontal track its thickness. Clear it when going
                    // vertical so the CSS class can size the native track, and
                    // restore it when returning to horizontal.
                    InnerElement.style.height = value == SliderOrientation.Vertical ? "" : "8px";
                }
            }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public int Value
        {
            get => int.Parse(InnerElement.value);
            set
            {
                if (Value != value)
                {
                    InnerElement.value = value.ToString();
                    InnerElement.setAttribute("aria-valuenow", value.ToString());
                    if (_fakeDiv is object) UpdateFakeProgress();
                    _observable.Value = value;
                }
            }
        }

        /// <summary>
        /// Returns an observable that tracks the slider's value.
        /// </summary>
        public IObservable<int> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the slider value as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(int value) => Value = value;

        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public int Min
        {
            get => int.Parse(InnerElement.min);
            set => InnerElement.min = value.ToString();
        }
        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public int Max
        {
            get => int.Parse(InnerElement.max);
            set => InnerElement.max = value.ToString();
        }

        /// <summary>
        /// Gets or sets the step size.
        /// </summary>
        public int Step
        {
            get => int.Parse(InnerElement.step);
            set => InnerElement.step = value.ToString();
        }

        /// <summary>
        /// Gets or sets whether the slider is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    InnerElement.classList.remove("tss-disabled");
                }
                else
                {
                    InnerElement.classList.add("tss-disabled");
                }
            }
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render()
        {
            return _outerDiv;
        }

        /// <summary>
        /// Sets the value of the slider.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The current instance.</returns>
        public Slider SetValue(int val)
        {
            Value = val;
            return this;
        }

        /// <summary>
        /// Sets the minimum value of the slider.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <returns>The current instance.</returns>
        public Slider SetMin(int min)
        {
            Min = min;
            return this;
        }

        /// <summary>
        /// Sets the maximum value of the slider.
        /// </summary>
        /// <param name="max">The maximum value.</param>
        /// <returns>The current instance.</returns>
        public Slider SetMax(int max)
        {
            Max = max;
            return this;
        }

        /// <summary>
        /// Sets the step size of the slider.
        /// </summary>
        /// <param name="step">The step size.</param>
        /// <returns>The current instance.</returns>
        public Slider SetStep(int step)
        {
            Step = step;
            return this;
        }

        /// <summary>
        /// Sets whether the slider is disabled.
        /// </summary>
        /// <param name="value">Whether it's disabled.</param>
        /// <returns>The current instance.</returns>
        public Slider Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Sets the orientation to horizontal.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Slider Horizontal()
        {
            Orientation = Slider.SliderOrientation.Horizontal;
            return this;
        }

        /// <summary>
        /// Sets the orientation to vertical.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Slider Vertical()
        {
            Orientation = Slider.SliderOrientation.Vertical;
            return this;
        }

        /// <summary>
        /// Represents the orientation of a slider.
        /// </summary>
        public enum SliderOrientation
        {
            /// <summary>Vertical orientation.</summary>
            Vertical,
            /// <summary>Horizontal orientation.</summary>
            Horizontal
        }
    }
}