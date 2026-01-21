using static H5.Core.dom;
using static Tesserae.UI;
using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// A slider component that snaps to discrete steps.
    /// </summary>
    /// <typeparam name="T">The type of the steps.</typeparam>
    [H5.Name("tss.StepsSlider")]
    public sealed class StepsSlider<T> : IComponent where T : IEquatable<T>
    {
        private readonly T[]                  _steps;
        private readonly Slider               _slider;
        private          IEqualityComparer<T> _equalityComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepsSlider{T}"/> class.
        /// </summary>
        /// <param name="steps">The discrete steps.</param>
        public StepsSlider(params T[] steps)
        {
            _steps            = steps;
            _slider           = Slider(0, 0, _steps.Length - 1, 1);
            _equalityComparer = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Sets the equality comparer for the steps.
        /// </summary>
        /// <param name="equalityComparer">The equality comparer.</param>
        /// <returns>The current instance.</returns>
        public StepsSlider<T> Comparer(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
            return this;
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public T Value
        {
            get => _steps[_slider.Value];
            set => _slider.Value = Array.FindIndex(_steps, p => _equalityComparer.Equals(p, Value));
        }

        /// <summary>
        /// Gets or sets whether the slider is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _slider.IsEnabled;
            set => _slider.IsEnabled = value;
        }

        /// <summary>
        /// Gets or sets the orientation of the slider.
        /// </summary>
        public Slider.SliderOrientation Orientation
        {
            get => _slider.Orientation;
            set => _slider.Orientation = value;
        }


        /// <summary>
        /// Sets the value of the slider.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The current instance.</returns>
        public StepsSlider<T> SetValue(T val)
        {
            Value = val;
            return this;
        }

        /// <summary>
        /// Attaches a handler to the change event.
        /// </summary>
        /// <param name="onChange">The handler.</param>
        /// <returns>The current instance.</returns>
        public StepsSlider<T> OnChange(Action<T> onChange)
        {
            _slider.OnChange((s, e) =>
            {
                onChange?.Invoke(Value);
            });

            _slider.OnInput((s, e) =>
            {
                onChange?.Invoke(Value);
            });
            return this;
        }

        /// <summary>
        /// Sets whether the slider is disabled.
        /// </summary>
        /// <param name="value">Whether it's disabled.</param>
        /// <returns>The current instance.</returns>
        public StepsSlider<T> Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>Sets the orientation to horizontal.</summary>
        public StepsSlider<T> Horizontal()
        {
            Orientation = Slider.SliderOrientation.Horizontal;
            return this;
        }

        /// <summary>Sets the orientation to vertical.</summary>
        public StepsSlider<T> Vertical()
        {
            Orientation = Slider.SliderOrientation.Vertical;
            return this;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public HTMLElement Render() => _slider.Render();
    }
}