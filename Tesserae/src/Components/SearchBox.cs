using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SearchBox")]
    public class SearchBox : ComponentBase<SearchBox, HTMLInputElement>, ITextFormating, IHasBackgroundColor, ITabIndex
    {
        private readonly HTMLDivElement  _container;
        private readonly HTMLSpanElement _icon;
        private readonly HTMLElement     _iconContainer;
        private readonly HTMLElement     _paddingContainer;

        protected event SearchEventHandler Searched;
        public delegate void               SearchEventHandler(SearchBox sender, string value);

        public SearchBox(string placeholder = string.Empty)
        {
            InnerElement      = TextBox(_("tss-searchbox tss-fontsize-small tss-fontweight-regular", type: "text", placeholder: placeholder));
            _icon             = Span(_(UIcons.Search.ToString()));
            _iconContainer    = Div(_("tss-searchbox-icon"), _icon);
            _paddingContainer = Div(_("tss-searchbox-padding"));
            _container        = Div(_("tss-searchbox-container"), _iconContainer, InnerElement, _paddingContainer);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            OnKeyPress((s, e) =>
            {
                if (e.key == "Enter")
                {
                    Searched?.Invoke(this, InnerElement.value);
                }
            });
        }

        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
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
                else _container.classList.remove("tss-underlined");
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

        public SearchBox NotUnderlined()
        {
            IsUnderlined = false;
            return this;
        }

        public SearchBox SetIcon(UIcons icon)
        {
            _icon.className = icon.ToString();
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
            DomObserver.WhenMounted(InnerElement, () => window.setTimeout((_) => InnerElement.focus(), 500));
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

        public SearchBox Height(UnitSize unitSize)
        {
            var h = unitSize.ToString();
            InnerElement.style.height     = h;
            InnerElement.style.lineHeight = h;
            _container.style.height       = h;
            return this;
        }

        public SearchBox H(int unitSize) => Height(unitSize.px());
    }
}