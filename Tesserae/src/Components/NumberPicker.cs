namespace Tesserae
{
    [H5.Name("tss.NumberPicker")]
    public class NumberPicker : Input<NumberPicker>, ITextFormating, IHasBackgroundColor, IHasForegroundColor
    {
        public NumberPicker(int defaultValue = 0) : base("number", defaultValue.ToString())
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
            InnerElement.style.alignItems = "center";
        }

        public int Value => int.Parse(Text);

        public int Max
        {
            get => int.Parse(InnerElement.max);
            set => InnerElement.max = value.ToString();
        }

        public int Min
        {
            get => int.Parse(InnerElement.min);
            set => InnerElement.min = value.ToString();
        }

        public int Step
        {
            get => int.Parse(InnerElement.step);
            set => InnerElement.step = value.ToString();
        }

        public NumberPicker SetMax(int max)
        {
            Max = max;
            return this;
        }

        public NumberPicker SetMin(int min)
        {
            Min = min;
            return this;
        }

        public NumberPicker SetStep(int step)
        {
            Step = step;
            return this;
        }

        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

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

        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        public string Foreground { get => InnerElement.style.color; set => InnerElement.style.color = value; }
    }
}