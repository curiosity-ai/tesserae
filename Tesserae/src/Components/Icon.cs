using static Tesserae.UI;
using static Transpose.Core.dom;
using System;

namespace Tesserae
{
    /// <summary>
    /// A single icon glyph from the bundled UIcons set, with configurable size, weight and color.
    /// </summary>
    [Transpose.Name("tss.Icon")]
    public class Icon : IComponent, IHasForegroundColor, ITextFormating
    {
        private readonly HTMLElement InnerElement;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Icon()
        {
            InnerElement = I(_("tss-icon "));
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Icon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, TextSize size = TextSize.Small)
        {
            var iconStr = $"{Transform(icon, weight)} {size}";

            InnerElement                 = I(_("tss-icon " + iconStr));
            InnerElement.dataset["icon"] = iconStr;
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Icon(Emoji icon, TextSize size = TextSize.Medium)
        {
            var iconStr = $"ec {icon} {size}";

            InnerElement                 = I(_("tss-icon " + iconStr));
            InnerElement.dataset["icon"] = iconStr;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public Icon SetIcon(Emoji icon, TextSize size = TextSize.Medium)
        {
            var iconStr = $"ec {icon} {size}";
            var current = InnerElement.dataset["icon"].As<string>();

            if (!string.IsNullOrWhiteSpace(current))
            {
                InnerElement.classList.remove(current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            InnerElement.classList.add(iconStr.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            InnerElement.dataset["icon"] = iconStr.ToString();
            return this;
        }

        /// <summary>
        /// Sets the icon of the component.
        /// </summary>
        public Icon SetIcon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, TextSize size = TextSize.Small)
        {
            var iconStr = $"{Transform(icon, weight)} {size}";

            var current = InnerElement.dataset["icon"].As<string>();

            if (!string.IsNullOrWhiteSpace(current))
            {
                InnerElement.classList.remove(current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            InnerElement.classList.add(iconStr.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            InnerElement.dataset["icon"] = iconStr.ToString();
            return this;
        }

        /// <summary>
        /// Configures the component to transform.
        /// </summary>
        public static string Transform(UIcons icon, UIconsWeight weight)
        {
            string v = icon.ToString();
            if (weight == UIconsWeight.Regular) return v;
            return weight + v.Substring(6);
        }

        /// <summary>
        /// Gets or sets the CSS color (foreground) of the component.
        /// </summary>
        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
        }

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the font weight of the component.
        /// </summary>
        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the component.
        /// </summary>
        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Center);
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public string Title
        {
            get => InnerElement.title;
            set => InnerElement.title = value;
        }

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public Icon SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => InnerElement;
    }
}