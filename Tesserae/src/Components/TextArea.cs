using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A multi-line text input component.
    /// </summary>
    [H5.Name("tss.TextArea")]
    public sealed class TextArea : ComponentBase<TextArea, HTMLTextAreaElement>, ICanValidate<TextArea>, IObservableComponent<string>, ITabIndex, IRoundedStyle
    {
        private readonly HTMLDivElement             _container;
        private readonly HTMLSpanElement            _errorSpan;
        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TextArea"/> class.
        /// </summary>
        /// <param name="text">The initial text.</param>
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

        /// <summary>Gets or sets the tab index.</summary>
        public int TabIndex
        {
            set
            {
                InnerElement.tabIndex = value;
            }
        }

        /// <summary>Gets or sets whether the component is enabled.</summary>
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

        /// <summary>Gets or sets whether the component is read-only.</summary>
        public bool IsReadOnly
        {
            get => InnerElement.hasAttribute("readonly");
            set
            {
                if (value) InnerElement.setAttribute("readonly", "");
                else InnerElement.removeAttribute("readonly");
            }
        }

        /// <summary>Gets or sets the text in the text area.</summary>
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

        /// <summary>Gets or sets the placeholder text.</summary>
        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        /// <summary>Gets or sets the error message.</summary>
        public string Error
        {
            get => _errorSpan.innerText;
            set => _errorSpan.innerText = value;
        }

        /// <summary>Gets or sets the maximum length of the text.</summary>
        public int MaxLength
        {
            get => InnerElement.maxLength;
            set => InnerElement.maxLength = value;
        }

        /// <summary>Gets or sets whether the component is in an invalid state.</summary>
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

        /// <summary>Gets or sets whether the component is required.</summary>
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
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render()
        {
            return _container;
        }

        /// <summary>
        /// Attaches a handler to the input updated event.
        /// </summary>
        /// <param name="handler">The handler.</param>
        public void Attach(ComponentEventHandler<TextArea> handler)
        {
            InputUpdated += (s, _) => handler(s);
        }

        /// <summary>
        /// Sets the text of the text area.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The current instance.</returns>
        public TextArea SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Clears the text.
        /// </summary>
        /// <returns>The current instance.</returns>
        public TextArea ClearText()
        {
            SetText(string.Empty);
            return this;
        }

        /// <summary>
        /// Sets the placeholder text.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        /// <returns>The current instance.</returns>
        public TextArea SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary>
        /// Sets whether the component is disabled.
        /// </summary>
        /// <param name="value">Whether it's disabled.</param>
        /// <returns>The current instance.</returns>
        public TextArea Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>Sets the component as read-only.</summary>
        public TextArea ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        /// <summary>Disables spell check.</summary>
        public TextArea NoSpellCheck()
        {
            InnerElement.spellcheck = false;
            return this;
        }

        /// <summary>Sets the component as required.</summary>
        public TextArea Required()
        {
            IsRequired = true;
            return this;
        }

        /// <summary>Sets focus to the text area.</summary>
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

        /// <summary>Returns an observable of the text.</summary>
        public IObservable<string> AsObservable() => _observable;
    }
}