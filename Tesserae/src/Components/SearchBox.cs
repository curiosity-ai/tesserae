using System;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class SearchBox : ComponentBase<SearchBox, HTMLInputElement>
    {
        private HTMLDivElement _container;
        private HTMLSpanElement _icon;
        private HTMLElement _iconContainer;
        private HTMLElement _paddingContainer;

        public event SearchEventHandler onSearch;
        public delegate void SearchEventHandler(SearchBox sender, string value);


        public SearchBox(string placeholder = string.Empty)
        {
            InnerElement = TextBox(_("tss-searchbox", type: "text", placeholder: placeholder));
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
                    onSearch?.Invoke(this, InnerElement.value);
                }
            });
        }

        public bool IsEnabled
        {
            get => !_container.classList.contains("disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("disabled");
                }
                else
                {
                    _container.classList.add("disabled");
                }
            }
        }

        public bool IsUnderlined
        {
            get => _container.classList.contains("underlined");
            set
            {
                if (value) _container.classList.add("underlined");
                else _container.classList.remove("underlined", "");
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
            get => _container.classList.contains("invalid");
            set
            {
                if (value)
                {
                    _container.classList.add("invalid");
                }
                else
                {
                    _container.classList.remove("invalid");
                }
            }
        }

        public override HTMLElement Render()
        {
            return _container;
        }

        public void Attach(EventHandler<Event> handler, Validation.Mode mode)
        {
            if (mode == Validation.Mode.OnBlur)
            {
                onChange += (s,e) => handler(s,e);
            }
            else
            {
                onInput += (s, e) => handler(s, e);
            }
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

        public SearchBox Disabled()
        {
            IsEnabled = false;
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
            _container.classList.add("noicon");
            return this;
        }

        public SearchBox SearchAsYouType()
        {
            OnKeyUp((s, e) =>
            {
                onSearch?.Invoke(this, InnerElement.value);
            });
            return this;
        }

        public SearchBox OnSearch(SearchEventHandler onSearch)
        {
            this.onSearch += onSearch;
            return this;
        }
    }
}
