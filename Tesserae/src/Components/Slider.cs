﻿using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class Slider : ComponentBase<Slider, HTMLInputElement>, IBindableComponent<int>
    {
        private readonly HTMLLabelElement _outerLabel;
        private readonly HTMLDivElement   _outerDiv;
        private readonly HTMLDivElement   _fakeDiv;


        private SettableObservable<int>           _observable;
        private ObservableEvent.ValueChanged<int> valueGetter;
        private bool                              _observableReferenceUsed = false;

        public SettableObservable<int> Observable
        {
            get
            {
                _observableReferenceUsed = true;
                return _observable;
            }
            set
            {
                if (_observableReferenceUsed)
                {
                    throw new ArgumentException("Can't set the observable after a reference of it has been used! (.AsObservable() might have been called before .Bind())");
                }

                if (_observable is object)
                    _observable.StopObserving(valueGetter);
                _observable = value;
                _observable.Observe(valueGetter);
            }
        }


        public Slider(int val = 0, int min = 0, int max = 100, int step = 10)
        {
            InnerElement           = document.createElement("input") as HTMLInputElement;
            InnerElement.className = "tss-slider";
            InnerElement.value     = val.ToString();
            InnerElement.min       = min.ToString();
            InnerElement.max       = max.ToString();
            InnerElement.step      = step.ToString();
            InnerElement.type      = "range";

            valueGetter = v => Value = v;
            Observable  = new SettableObservable<int>();

            AttachClick();
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();

            if (navigator.userAgent.IndexOf("AppleWebKit") != -1)
            {
                _fakeDiv = Div(_("tss-slider-fake-progress"));
                double percent = ((double) (val - min) / (max - min)) * 100.0;
                _fakeDiv.style.width = $"{percent:0.##}%";
                InputUpdated += (e, s) =>
                {
                    percent              = ((double) (Value - Min) / (Max - Min)) * 100.0;
                    _fakeDiv.style.width = $"{percent:0.##}%";
                };
                _outerLabel = Label(_("tss-slider-container"), InnerElement, Div(_("tss-slider-fake-background")), _fakeDiv);
                InnerElement.classList.add("tss-fake");
            }
            else
            {
                _outerLabel = Label(_("tss-slider-container"), InnerElement);
            }

            _outerDiv = Div(_("tss-slider-div"), _outerLabel);

            OnChange((_, __) => _observable.Value = Value);
            OnInput((_,  __) => _observable.Value = Value);
        }

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

        private void SetBarWidth()
        {
            double percent = ((double) (Value - Min) / (Max - Min)) * 100.0;
            _fakeDiv.style.width = $"{percent:0.##}%";
        }

        public int Value
        {
            get => int.Parse(InnerElement.value);
            set
            {
                InnerElement.value = value.ToString();
                RaiseOnInput(null);
            }
        }

        public int Min
        {
            get => int.Parse(InnerElement.min);
            set => InnerElement.min = value.ToString();
        }

        public int Max
        {
            get => int.Parse(InnerElement.max);
            set => InnerElement.max = value.ToString();
        }

        public int Step
        {
            get => int.Parse(InnerElement.step);
            set => InnerElement.step = value.ToString();
        }

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

        public override HTMLElement Render()
        {
            return _outerDiv;
        }

        public Slider SetValue(int val)
        {
            Value = val;
            return this;
        }

        public Slider SetMin(int min)
        {
            Min = min;
            return this;
        }

        public Slider SetMax(int max)
        {
            Max = max;
            return this;
        }

        public Slider SetStep(int step)
        {
            Step = step;
            return this;
        }

        public Slider Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public Slider Horizontal()
        {
            Orientation = Slider.SliderOrientation.Horizontal;
            return this;
        }

        public Slider Vertical()
        {
            Orientation = Slider.SliderOrientation.Vertical;
            return this;
        }

        public enum SliderOrientation
        {
            Vertical,
            Horizontal
        }
    }
}