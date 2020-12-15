using System;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class Link : IComponent, ITextFormating
    {
        private readonly HTMLAnchorElement _anchor;
        public Link(string url, IComponent component, bool noUnderline = false)
        {
            _anchor = A(_(href: url), component.Render());
            if(component is Button && !noUnderline)
            {
                _anchor.classList.add("tss-link-btn");
            }
        }

        public string Target
        {
            get => _anchor.target;
            set => _anchor.target = value;
        }

        public string URL
        {
            get => _anchor.href;
            set => _anchor.href = value;
        }

        public HTMLElement Render()
        {
            return _anchor;
        }

        public Link OpenInNewTab()
        {
            Target = "_blank";
            return this;
        }

        public Link OnClick(Action onClicked)
        {
            if (onClicked is null)
            {
                throw new ArgumentNullException(nameof(onClicked));
            }

            _anchor.onclick = (e) =>
            {
                StopEvent(e);
                onClicked();
            };

            return this;
        }

        public TextSize Size
        {
            get => _anchor.GetTextSize().textSize ?? TextSize.Small;
            set
            {
                var (textSize, textSizeCssClass) = _anchor.GetTextSize();

                _anchor.RemoveClassIf(textSize.HasValue, textSizeCssClass);

                _anchor.classList.add($"tss-fontsize-{value.ToString().ToLower()}");
            }
        }

        public TextWeight Weight
        {
            get => _anchor.GetTextWeight().textWeight ?? TextWeight.Regular;
            set
            {
                var (textWeight, textWeightCssClass) = _anchor.GetTextWeight();

                _anchor.RemoveClassIf(textWeight.HasValue, textWeightCssClass);

                _anchor.classList.add($"tss-fontweight-{value.ToString().ToLower()}");
            }
        }

        public TextAlign TextAlign
        {
            get => _anchor.GetTextAlign().textAlign ?? TextAlign.Center;
            set
            {
                var (textAlign, textAlignCssClass) = _anchor.GetTextAlign();

                _anchor.RemoveClassIf(textAlign.HasValue, textAlignCssClass);

                _anchor.classList.add($"tss-textalign-{value.ToString().ToLower()}");
            }
        }
    }
}
