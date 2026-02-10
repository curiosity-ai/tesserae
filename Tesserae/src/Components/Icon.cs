using static Tesserae.UI;
using static H5.Core.dom;
using System;

namespace Tesserae
{
    [H5.Name("tss.Icon")]
    public class Icon : IComponent, IHasForegroundColor, ITextFormating
    {
        private readonly HTMLElement InnerElement;

        public Icon()
        {
            InnerElement = I(_("tss-icon "));
        }

        public Icon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, TextSize size = TextSize.Small)
        {
            var iconStr = $"{Transform(icon, weight)} {size}";

            InnerElement                 = I(_("tss-icon " + iconStr));
            InnerElement.dataset["icon"] = iconStr;
        }

        public Icon(Emoji icon, TextSize size = TextSize.Medium)
        {
            var iconStr = $"ec {icon} {size}";

            InnerElement                 = I(_("tss-icon " + iconStr));
            InnerElement.dataset["icon"] = iconStr;
        }

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

        public static string Transform(UIcons icon, UIconsWeight weight)
        {
            string v = icon.ToString();
            if (weight == UIconsWeight.Regular) return v;
            return weight + v.Substring(6);
        }

        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
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
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Center);
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public string Title
        {
            get => InnerElement.title;
            set => InnerElement.title = value;
        }

        public Icon SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public HTMLElement Render() => InnerElement;
    }
}