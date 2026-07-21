using Transpose;
using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A horizontal progress bar with optional label and either determinate or indeterminate state.
    /// </summary>
    [Transpose.Name("tss.ProgressIndicator")]
    public class ProgressIndicator : IComponent, IHasForegroundColor
    {
        private readonly HTMLElement InnerElement;
        private readonly HTMLElement BarElement;

        /// <summary>
        /// Gets or sets the CSS color (foreground) of the component.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ProgressIndicator()
        {
            BarElement   = Div(Att("tss-progressindicator-bar"));
            InnerElement = Div(Att("tss-progressindicator"), BarElement);
        }

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
        public ProgressIndicator Progress(int position, int total) => Progress(100f * position / total);

        /// <summary>
        /// Configures the component to progress.
        /// </summary>
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

        /// <summary>
        /// Configures the component to indeterminated.
        /// </summary>
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

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            return InnerElement;
        }
    }
}