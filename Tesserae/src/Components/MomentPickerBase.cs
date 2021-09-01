using System;
using System.Linq;

namespace Tesserae
{
    public abstract class MomentPickerBase<TMomentPicker, TMoment> : Input<TMomentPicker>, ITextFormating, IHasBackgroundColor, IHasForegroundColor where TMomentPicker : MomentPickerBase<TMomentPicker, TMoment>
    {
        protected MomentPickerBase(string type, string defaultText = null) : base(type, defaultText)
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
            InnerElement.style.alignItems = "center";
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
            return (TMomentPicker) this;
        }

        public TMomentPicker SetMin(TMoment min)
        {
            Min = min;
            return (TMomentPicker) this;
        }

        public TMomentPicker SetStep(int step)
        {
            Step = step;
            return (TMomentPicker) this;
        }

        protected abstract string FormatMoment(TMoment moment);

        protected abstract TMoment FormatMoment(string moment);

        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public TextAlign TextAlign
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                if (curFontSize is object && Enum.TryParse<TextAlign>(curFontSize.Substring("tss-textalign-".Length), true, out var result))
                {
                    return result;
                }
                else
                {
                    return TextAlign.Left;
                }
            }
            set
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"tss-textalign-{value.ToString().ToLower()}");
            }
        }

        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        public string Foreground { get => InnerElement.style.color; set => InnerElement.style.color = value; }

    }
}