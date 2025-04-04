﻿using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.TextArea")]
    public sealed class TextArea : ComponentBase<TextArea, HTMLTextAreaElement>, ICanValidate<TextArea>, IObservableComponent<string>, ITabIndex
    {
        private readonly HTMLDivElement             _container;
        private readonly HTMLSpanElement            _errorSpan;
        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        public TextArea(string text = string.Empty)
        {
            InnerElement = TextArea(_("tss-textbox tss-textarea", type: "text", value: text));
            _errorSpan   = Span(_("tss-textbox-error"));
            _container   = Div(_("tss-textbox-container"), InnerElement, _errorSpan);

            //TODO: Need to make container display:flex, and use flex-grow to have correct sizing with _errorSpan
            InnerElement.style.width  = "100%";
            InnerElement.style.height = "100%";

            AttachChange();
            AttachInput();
            AttachKeys();
            AttachFocus();
            AttachBlur();

            OnChange((_, __) => _observable.Value = Text);

            OnInput((_, __) => _observable.Value = Text);
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
            get => !InnerElement.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    InnerElement.classList.remove("tss-disabled");
                }
                else
                {
                    InnerElement.classList.add("tss-disabled");
                }
            }
        }

        public bool IsReadOnly
        {
            get => InnerElement.hasAttribute("readonly");
            set
            {
                if (value) InnerElement.setAttribute("readonly", "");
                else InnerElement.removeAttribute("readonly");
            }
        }

        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value;
                _observable.Value  = value;
                RaiseOnInput(null);
            }
        }

        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        public string Error
        {
            get => _errorSpan.innerText;
            set => _errorSpan.innerText = value;
        }

        public int MaxLength
        {
            get => InnerElement.maxLength;
            set => InnerElement.maxLength = value;
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

        public bool IsRequired
        {
            get => _container.classList.contains("tss-required");
            set
            {
                if (value)
                {
                    _container.classList.add("tss-required");
                }
                else
                {
                    _container.classList.remove("tss-required");
                }
            }
        }

        public override HTMLElement Render()
        {
            return _container;
        }

        public void Attach(ComponentEventHandler<TextArea> handler)
        {
            InputUpdated += (s, _) => handler(s);
        }

        public TextArea SetText(string text)
        {
            Text = text;
            return this;
        }

        public TextArea ClearText()
        {
            SetText(string.Empty);
            return this;
        }

        public TextArea SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        public TextArea Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public TextArea ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        public TextArea NoSpellCheck()
        {
            InnerElement.spellcheck = false;
            return this;
        }

        public TextArea Required()
        {
            IsRequired = true;
            return this;
        }

        public TextArea Focus()
        {
            DomObserver.WhenMounted(InnerElement, () =>
            {
                try
                {
                    InnerElement.scrollIntoViewIfNeeded();
                }
                catch
                {
                    InnerElement.scrollIntoView();
                }

                InnerElement.focus();
            });
            return this;
        }

        public IObservable<string> AsObservable() => _observable;
    }
}