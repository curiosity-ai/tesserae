using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public enum SliderOrientation
    {
        Vertical,
        Horizontal
    }
    public class Slider : ComponentBase<Slider, HTMLInputElement>
    {
        #region Fields

        private HTMLLabelElement _OuterLabel;
        private HTMLDivElement _OuterDiv;
        private HTMLDivElement _FakeDiv;
        private HTMLSpanElement _ValueSpan;

        #endregion

        #region Properties

        public SliderOrientation Orientation
        {
            get { return _OuterLabel.classList.contains("vertical") ? SliderOrientation.Vertical : SliderOrientation.Horizontal; }
            set
            {
                if (value != Orientation)
                {
                    if (value == SliderOrientation.Vertical)
                    {
                        _OuterLabel.classList.add("vertical");
                    }
                    else
                    {
                        _OuterLabel.classList.remove("vertical");
                    }
                }
            }
        }

        public int Value
        {
            get { return int.Parse(InnerElement.value); }
            set { InnerElement.value = value.ToString(); }
        }

        public int Min
        {
            get { return int.Parse(InnerElement.min); }
            set { InnerElement.min = value.ToString(); }
        }
        public int Max
        {
            get { return int.Parse(InnerElement.max); }
            set { InnerElement.max = value.ToString(); }
        }

        public int Step
        {
            get { return int.Parse(InnerElement.step); }
            set { InnerElement.step = value.ToString(); }
        }

        public bool IsEnabled
        {
            get { return !InnerElement.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
                {
                    if (value)
                    {
                        InnerElement.classList.remove("disabled");
                    }
                    else
                    {
                        InnerElement.classList.add("disabled");
                    }
                }
            }
        }

        #endregion

        public Slider(int val = 0, int min = 0, int max = 100, int step = 10)
        {
            InnerElement = document.createElement("input") as HTMLInputElement;
            InnerElement.className = "mss-slider";
            InnerElement.value = val.ToString();
            InnerElement.min = min.ToString();
            InnerElement.max = max.ToString();
            InnerElement.step = step.ToString();
            InnerElement.type = "range";

            _ValueSpan = Span(_("m-1", text: val.ToString()));

            AttachClick();
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();

            if (navigator.userAgent.IndexOf("AppleWebKit") != -1)
            {
                _FakeDiv = Div(_("mss-slider-fake-progress"));
                double percent = ((double)(val - min) / (double)(max - min)) * 100.0;
                _FakeDiv.style.width = $"{percent.ToString("0.##")}%";
                OnInput += (e, s) =>
                {
                    percent = ((double)(Value - Min) / (double)(Max - Min)) * 100.0;
                    _FakeDiv.style.width = $"{percent.ToString("0.##")}%";
                };
                _OuterLabel = Label(_("mss-slider-container"), InnerElement, Div(_("mss-slider-fake-background")), _FakeDiv);
                InnerElement.classList.add("fake");
            }
            else
            {
                _OuterLabel = Label(_("mss-slider-container"), InnerElement);
            }

            _OuterDiv = Div(_("mss-slider-div"), _OuterLabel);
        }

        public override HTMLElement Render()
        {
            return _OuterDiv;
        }
    }

    public static class SliderExtensions
    {
        public static Slider Value(this Slider slider, int val)
        {
            slider.Value = val;
            return slider;
        }
        public static Slider Min(this Slider slider, int min)
        {
            slider.Min = min;
            return slider;
        }
        public static Slider Max(this Slider slider, int max)
        {
            slider.Max = max;
            return slider;
        }
        public static Slider Step(this Slider slider, int step)
        {
            slider.Step = step;
            return slider;
        }

        public static Slider Disabled(this Slider slider)
        {
            slider.IsEnabled = false;
            return slider;
        }

        public static Slider Horizontal(this Slider slider)
        {
            slider.Orientation= SliderOrientation.Horizontal;
            return slider;
        }

        public static Slider Vertical(this Slider slider)
        {
            slider.Orientation = SliderOrientation.Vertical;
            return slider;
        }
    }
}
