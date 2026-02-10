using System;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Link")]
    public class Link : IComponent, ITextFormating
    {
        private readonly HTMLAnchorElement _anchor;
        private          string            _features;

        public Link(string url, IComponent component, bool noUnderline = false)
        {
            _anchor = A(_(href: url), component.Render());

            if (noUnderline)
            {
                _anchor.classList.add("tss-link-no-underline");
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

        public Link AsWindow(string features = null)
        {
            if (string.IsNullOrEmpty(features))
            {
                int left = (int)((window.screen.availWidth  - 900) / 2);
                int top  = (int)((window.screen.availHeight - 600) / 2);
                _features = $"scrollbars=yes,resizable=yes,toolbar=no,status=no,menubar=no,width=900,height=600,left={left},top={top}";
            }
            else
            {
                _features = features;
            }

            return OnClick(() =>
            {
                window.open(_anchor.href, _anchor.target, _features);
            });
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
            get => ITextFormatingExtensions.FromClassList(_anchor, TextSize.Small);
            set
            {
                _anchor.classList.remove(Size.ToString());
                _anchor.classList.add(value.ToString());
            }
        }

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(_anchor, TextWeight.Regular);
            set
            {
                _anchor.classList.remove(Weight.ToString());
                _anchor.classList.add(value.ToString());
            }
        }

        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(_anchor, TextAlign.Center);
            set
            {
                _anchor.classList.remove(TextAlign.ToString());
                _anchor.classList.add(value.ToString());
            }
        }
    }
}