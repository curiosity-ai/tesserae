using static Tesserae.UI;
using static H5.Core.dom;
using System;

namespace Tesserae
{
    [H5.Name("tss.Icon")]
    public class Icon : IComponent, IHasForegroundColor, ITextFormating
    {
        private readonly HTMLElement InnerElement;

        public Icon(string icon)
        {
            InnerElement = I(_("tss-icon " + icon));
            InnerElement.dataset["icon"] = icon;
        }

        public Icon SetIcon(string icon)
        {
            var current = InnerElement.dataset["icon"].As<string>();

            if (!string.IsNullOrWhiteSpace(current))
            {
                InnerElement.classList.remove(current.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            if(!string.IsNullOrEmpty(icon))
            {
                InnerElement.classList.add(icon.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            InnerElement.dataset["icon"] = icon;
            return this;
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
            get =>  ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
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
