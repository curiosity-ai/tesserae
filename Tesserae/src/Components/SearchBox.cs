﻿using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SearchBox : ComponentBase<SearchBox, HTMLInputElement>,  ITextFormating, IHasBackgroundColor
    {
        private readonly HTMLDivElement _container;
        private readonly HTMLSpanElement _icon;
        private readonly HTMLElement _iconContainer;
        private readonly HTMLElement _paddingContainer;

        protected event SearchEventHandler Searched;
        public delegate void SearchEventHandler(SearchBox sender, string value);

        public SearchBox(string placeholder = string.Empty)
        {
            InnerElement = TextBox(_("tss-searchbox tss-fontsize-small tss-fontweight-regular", type: "text", placeholder: placeholder));
            _icon = Span(_("las la-search"));
            _iconContainer = Div(_("tss-searchbox-icon"), _icon);
            _paddingContainer = Div(_("tss-searchbox-padding"));
            _container = Div(_("tss-searchbox-container"), _iconContainer, InnerElement, _paddingContainer);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            OnKeyPress((s, e) =>
            {
                if(e.key == "Enter")
                {
                    Searched?.Invoke(this, InnerElement.value);
                }
            });
        }

        public bool IsEnabled
        {
            get => !_container.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-disabled");
                }
                else
                {
                    _container.classList.add("tss-disabled");
                }
            }
        }

        public bool IsUnderlined
        {
            get => _container.classList.contains("tss-underlined");
            set
            {
                if (value) _container.classList.add("tss-underlined");
                else _container.classList.remove("tss-underlined", "");
            }
        }

        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value;
                RaiseOnInput(null);
            }
        }

        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        public string Icon
        {
            get => _icon.className;
            set
            {
                _icon.className = value;
            }
        }

        public bool IsInvalid
        {
            get => _container.classList.contains("tss-invalid");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-invalid");
                }
                else
                {
                    _container.classList.remove("tss-invalid");
                }
            }
        }

        public TextSize Size
        {
            get => InnerElement.GetTextSize().textSize ?? TextSize.Small;
            set
            {
                var (textSize, textSizeCssClass) = InnerElement.GetTextSize();

                InnerElement.RemoveClassIf(textSize.HasValue, textSizeCssClass);

                InnerElement.classList.add($"tss-fontsize-{value.ToString().ToLower()}");
            }
        }

        public TextWeight Weight
        {
            get => InnerElement.GetTextWeight().textWeight ?? TextWeight.Regular;
            set
            {
                var (textWeight, textWeightCssClass) = InnerElement.GetTextWeight();

                InnerElement.RemoveClassIf(textWeight.HasValue, textWeightCssClass);

                InnerElement.classList.add($"tss-fontweight-{value.ToString().ToLower()}");
            }
        }

        public TextAlign TextAlign
        {
            get => InnerElement.GetTextAlign().textAlign ?? TextAlign.Center;
            set
            {
                var (textAlign, textAlignCssClass) = InnerElement.GetTextAlign();

                InnerElement.RemoveClassIf(textAlign.HasValue, textAlignCssClass);

                InnerElement.classList.add($"tss-textalign-{value.ToString().ToLower()}");
            }
        }
        public string Background { get => _container.style.background; set => _container.style.background = value; }

        public override HTMLElement Render()
        {
            return _container;
        }

        public void Attach(ComponentEventHandler<SearchBox> handler)
        {
            InputUpdated += (s, _) => handler(s);
        }

        public SearchBox SetText(string text)
        {
            Text = text;
            return this;
        }

        public SearchBox SetPlaceholder(string error)
        {
            Placeholder = error;
            return this;
        }

        public SearchBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public SearchBox Underlined()
        {
            IsUnderlined = true;
            return this;
        }

        public SearchBox SetIcon(string icon)
        {
            Icon = icon;
            return this;
        }

        public SearchBox NoIcon()
        {
            _container.classList.add("tss-noicon");
            return this;
        }

        public SearchBox Focus()
        {
            // 2020-12-29 DWR: Seems like this setTimeout is required then the element is rendered within a container that uses "simplebar" scrolling - without the delay, if the element getting focus is out of view then it will not be
            // scrolled into view (even though it has successfully received focus)
            DomObserver.WhenMounted(InnerElement, () => window.setTimeout((_) =>  InnerElement.focus(), 500));
            return this;
        }

        public SearchBox SearchAsYouType()
        {
            OnKeyUp((s, e) =>
            {
                Searched?.Invoke(this, InnerElement.value);
            });
            return this;
        }

        public SearchBox OnSearch(SearchEventHandler onSearch)
        {
            Searched += onSearch;
            return this;
        }
    }
}
