using H5;
using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ProgressIndicator")]
    public class ProgressIndicator : IComponent, IHasForegroundColor
    {
        private readonly HTMLElement InnerElement;
        private readonly HTMLElement BarElement;

        public string Foreground
        {
            get => BarElement.style.background;
            set
            {
                if (BarElement.classList.contains("tss-progressindicator-bar-indeterminate"))
                {
                    BarElement.style.background = $"linear-gradient(to right, var(--tss-progress-background-color) 0%, {value} 50%, var(--tss-progress-background-color) 100%)";
                }
                else
                {
                    BarElement.style.background = value;
                }
            }
        }

        public ProgressIndicator()
        {
            BarElement   = Div(_("tss-progressindicator-bar"));
            InnerElement = Div(_("tss-progressindicator"), BarElement);
        }

        public ProgressIndicator Progress(int position, int total) => Progress(100f * position / total);

        public ProgressIndicator Progress(float percent)
        {
            if (!BarElement.classList.contains("tss-progressindicator-bar"))
            {
                BarElement.classList.add("tss-progressindicator-bar");
                BarElement.classList.remove("tss-progressindicator-bar-indeterminate");
            }
            percent                = Math.Max(0f, Math.Min(100f, percent));
            BarElement.style.width = $"{percent}%";
            return this;
        }

        public ProgressIndicator Indeterminated()
        {
            if (!BarElement.classList.contains("tss-progressindicator-bar-indeterminate"))
            {
                BarElement.classList.remove("tss-progressindicator-bar");
                BarElement.classList.add("tss-progressindicator-bar-indeterminate");
            }
            BarElement.style.width = "100%";
            return this;
        }

        public HTMLElement Render()
        {
            return InnerElement;
        }
    }
}