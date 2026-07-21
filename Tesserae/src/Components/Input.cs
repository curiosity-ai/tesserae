using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Abstract base class for input components that wrap a single <see cref="HTMLInputElement"/>.
    /// Provides shared behaviour for text-like inputs (value, validation, focus, observable binding).
    /// </summary>
    /// <typeparam name="TInput">The concrete input type (CRTP self-reference for fluent return types).</typeparam>
    [Transpose.Name("tss.Input")]
    public abstract class Input<TInput> : ComponentBase<TInput, HTMLInputElement>, ITabIndex, ICanValidate<TInput>, IBindableComponent<string> where TInput : Input<TInput>
    {
        private readonly HTMLDivElement             _container;
        private readonly HTMLSpanElement            _errorSpan;
        private readonly SettableObservable<string> _observable;

        /// <summary>
        /// Initializes a new instance of the <see cref="Input{TInput}"/> class.
        /// </summary>
        /// <param name="type">The HTML <c>input</c> <c>type</c> attribute (e.g. <c>text</c>, <c>date</c>, <c>number</c>).</param>
        /// <param name="defaultText">Optional initial value for the input.</param>
        protected Input(string type, string defaultText = null)
        {
            InnerElement = UI.TextBox(Att("tss-textbox", type: type, value: defaultText));
            _errorSpan = Span(Att("tss-textbox-error"));
            _container = Div(Att("tss-textbox-container tss-default-component-margin"), InnerElement, _errorSpan);

            _observable = new SettableObservable<string>(defaultText);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();
            AttachClick();

            // Subscribe to the underlying events directly rather than calling the virtual
            // OnChange / OnInput methods, which would dispatch to derived overrides before
            // the derived constructor has finished running (a classic virtual-in-constructor
            // antipattern). This achieves the same observable-update behaviour safely.
            Changed       += (_, __) => _observable.Value = Text;
            InputUpdated  += (_, __) => _observable.Value = Text;
        }

        /// <summary>
        /// This will reset the input to a blank state but it will NOT trigger the InputUpdated event because this should be used when a form is being programmatically reset, as opposed to when the User has set the field to blank - the important difference is
        /// that if a form is reset then it should not immediately be covered in validation warnings until the User starts to interact with the reset form (and firing InputUpdated will cause any validator that this component is registered with to revalidate)
        /// </summary>
        public void Reset()
        {
            InnerElement.value = "";
            _observable.Value  = "";
        }

        /// <summary>
        /// Gets or sets the text shown in the component.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the component is interactive (enabled).
        /// </summary>
        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    InnerElement.classList.remove("tss-disabled");
                    InnerElement.disabled = false;
                }
                else
                {
                    InnerElement.classList.add("tss-disabled");
                    InnerElement.disabled = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the validation error message displayed beneath the component.
        /// </summary>
        public string Error
        {
            get => _errorSpan.innerText;
            set => _errorSpan.innerText = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is currently in an invalid state.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the component is required for form submission.
        /// </summary>
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

        /// <summary>
        /// Sets the keyboard tab order of the component.
        /// </summary>
        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
        }

        /// <summary>
        /// Attaches a handler to the component's value-changed event.
        /// </summary>
        public void Attach(ComponentEventHandler<TInput> handler)
        {
            InputUpdated += (s, _) => handler(s);
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public TInput SetText(string text)
        {
            Text = text;
            return this.As<TInput>();
        }

        /// <summary>
        /// Clears the text.
        /// </summary>
        public TInput ClearText()
        {
            SetText(string.Empty);
            return this.As<TInput>();
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public TInput Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this.As<TInput>();
        }

        /// <summary>
        /// Removes / disables the spell check on the component.
        /// </summary>
        public TInput NoSpellCheck()
        {
            InnerElement.spellcheck = false;
            return this.As<TInput>();
        }

        /// <summary>
        /// Marks the component as required.
        /// </summary>
        public TInput Required()
        {
            IsRequired = true;
            return this.As<TInput>();
        }

        /// <summary>
        /// Moves keyboard focus to the component.
        /// </summary>
        public TInput Focus()
        {
            // 2020-12-29 DWR: Seems like this setTimeout is required then the element is rendered within a container that uses "simplebar" scrolling - without the delay, if the element getting focus is out of view then it will not be
            // scrolled into view (even though it has successfully received focus)
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
            return this.As<TInput>();
        }

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<string> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the input as part of a two-way binding (DOM input event is not raised).
        /// </summary>
        public void SetBoundValue(string value)
        {
            InnerElement.value = value;
            _observable.Value  = value;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => _container;
    }
}