using static H5.Core.dom;
using static Tesserae.UI;
using System.Linq;

namespace Tesserae
{

    /// <summary>
    /// A slider component.
    /// </summary>
    [H5.Name("tss.Slider")]
    public sealed class Slider : ComponentBase<Slider, HTMLInputElement>
    {
        private readonly HTMLLabelElement _outerLabel;
        private readonly HTMLDivElement   _outerDiv;
        private readonly HTMLDivElement   _fakeDiv;

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

            AttachClick();
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();

            if (navigator.userAgent.IndexOf("AppleWebKit") != -1)
            {
                _fakeDiv = Div(_("tss-slider-fake-progress"));
                double percent = ((double)(val - min) / (max - min)) * 100.0;
                _fakeDiv.style.width = $"{percent:0.##}%";

                InputUpdated += (e, s) =>
                {
                    percent = UpdateFakeProgress();
                };

                _outerLabel = Label(_("tss-slider-container"), InnerElement, Div(_("tss-slider-fake-background")), _fakeDiv);
                InnerElement.classList.add("tss-fake");
            }
            else
            {
                _outerLabel               = Label(_("tss-slider-container"), InnerElement);
                InnerElement.style.height = "8px";
            }

            _outerDiv = Div(_("tss-slider-div"), _outerLabel);
        }

        private double UpdateFakeProgress()
        {
            double percent = ((double)(Value - Min) / (Max - Min)) * 100.0;
            _fakeDiv.style.width = $"{percent:0.##}%";
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
                }
                else
                {
                    _outerLabel.classList.remove("tss-vertical");
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
                    if (_fakeDiv is object) UpdateFakeProgress();
                }
            }
        }

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