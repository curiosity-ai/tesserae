using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public abstract class Input<TInput> : ComponentBase<TInput, HTMLInputElement>, ICanValidate<TInput>, IObservableComponent<string> where TInput : Input<TInput>
    {
        private readonly HTMLDivElement _container;
        private readonly HTMLSpanElement _errorSpan;
        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        protected Input(string type, string defaultText = null)
        {
            InnerElement = TextBox(_("tss-textbox", type: type, value: defaultText));

            _errorSpan = Span(_("tss-textbox-error"));
            _container = Div(_("tss-textbox-container"), InnerElement, _errorSpan);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            // TODO: 27/06/20 - MB - calling virtual member within a constructor is a bit of a no-no.
            OnChange((_, __) => _observable.Value = Text);
            OnInput((_, __) => _observable.Value = Text);
        }

        /// <summary>
        /// This will reset the input to a blank state but it will NOT trigger the InputUpdated event because this should be used when a form is being programmatically reset, as opposed to when the User has set the field to blank - the important difference is
        /// that if a form is reset then it should not immediately be covered in validation warnings until the User starts to interact with the reset form (and firing InputUpdated will cause any validator that this component is registered with to revalidate)
        /// </summary>
        public void Reset()
        {
            InnerElement.value = "";
            _observable.Value = "";
        }

        public string Text
        {
            get => InnerElement.value;
            set
            {
                InnerElement.value = value;
                _observable.Value = value;
                RaiseOnInput(null);
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

        public string Error
        {
            get => _errorSpan.innerText;
            set => _errorSpan.innerText = value;
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

        public void Attach(ComponentEventHandler<TInput> handler)
        {
            InputUpdated += (s, _) => handler(s);
        }

        public TInput SetText(string text)
        {
            Text = text;
            return (TInput)this;
        }

        public TInput ClearText()
        {
            SetText(string.Empty);
            return (TInput)this;
        }

        public TInput Disabled(bool value = true)
        {
            IsEnabled = !value;
            return (TInput)this;
        }

        public TInput NoSpellCheck()
        {
            InnerElement.spellcheck = false;
            return (TInput)this;
        }

        public TInput Required()
        {
            IsRequired = true;
            return (TInput)this;
        }

        public TInput Focus()
        {
            DomObserver.WhenMounted(InnerElement, () => window.setTimeout((_) => InnerElement.focus(), 500));
            return (TInput)this;
        }

        public IObservable<string> AsObservable() => _observable;

        public override HTMLElement Render() => _container;
    }
}