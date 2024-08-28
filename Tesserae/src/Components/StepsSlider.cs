using static H5.Core.dom;
using static Tesserae.UI;
using System;
using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.StepsSlider")]
    public sealed class StepsSlider<T> : IComponent where T : IEquatable<T>
    {
        private readonly T[]                  _steps;
        private readonly Slider               _slider;
        private          IEqualityComparer<T> _equalityComparer;
        public StepsSlider(params T[] steps)
        {
            _steps            = steps;
            _slider           = Slider(0, 0, _steps.Length - 1, 1);
            _equalityComparer = EqualityComparer<T>.Default;
        }

        public StepsSlider<T> Comparer(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;
            return this;
        }

        public T Value
        {
            get => _steps[_slider.Value];
            set => _slider.Value = Array.FindIndex(_steps, p => _equalityComparer.Equals(p, Value));
        }

        public bool IsEnabled
        {
            get => _slider.IsEnabled;
            set => _slider.IsEnabled = value;
        }

        public Slider.SliderOrientation Orientation
        {
            get => _slider.Orientation;
            set => _slider.Orientation = value;
        }


        public StepsSlider<T> SetValue(T val)
        {
            Value = val;
            return this;
        }

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

        public StepsSlider<T> Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public StepsSlider<T> Horizontal()
        {
            Orientation = Slider.SliderOrientation.Horizontal;
            return this;
        }

        public StepsSlider<T> Vertical()
        {
            Orientation = Slider.SliderOrientation.Vertical;
            return this;
        }

        public HTMLElement Render() => _slider.Render();
    }
}