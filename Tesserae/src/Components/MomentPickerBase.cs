namespace Tesserae.Components
{
    public abstract class MomentPickerBase<TMomentPicker, TMoment> : Input<TMomentPicker>
        where TMomentPicker : MomentPickerBase<TMomentPicker, TMoment>
    {
        protected MomentPickerBase(string type, string defaultText = null)
            : base(type, defaultText)
        {
        }

        protected TMoment Moment => FormatMoment(Text);

        public TMoment Max
        {
            get => FormatMoment(InnerElement.max);
            set => InnerElement.max = FormatMoment(value);
        }

        public TMoment Min
        {
            get => FormatMoment(InnerElement.min);
            set => InnerElement.min = FormatMoment(value);
        }

        public int Step
        {
            get => int.Parse(InnerElement.step);
            set => InnerElement.step = value.ToString();
        }

        public TMomentPicker SetMax(TMoment max)
        {
            Max = max;
            return (TMomentPicker)this;
        }

        public TMomentPicker SetMin(TMoment min)
        {
            Min = min;
            return (TMomentPicker)this;
        }

        public TMomentPicker SetStep(int step)
        {
            Step = step;
            return (TMomentPicker)this;
        }

        protected abstract string FormatMoment(TMoment moment);

        protected abstract TMoment FormatMoment(string moment);
    }
}
