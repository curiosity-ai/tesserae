using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A two-state form control (checked / unchecked) used for boolean values.
    /// </summary>
    [Transpose.Name("tss.ChecBox")]
    public class CheckBox : ComponentBase<CheckBox, HTMLInputElement>, IBindableComponent<bool>, IRoundedStyle, ITextFormating
    {
        private readonly HTMLSpanElement          _checkSpan;
        private readonly HTMLLabelElement         _label;
        private readonly SettableObservable<bool> _observable;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CheckBox(string text = string.Empty)
        {
            InnerElement = CheckBox(_("tss-checkbox"));
            _checkSpan   = Span(_("tss-checkbox-mark"));
            _label = Label(_("tss-checkbox-container tss-default-component-margin tss-fontcolor-default tss-fontsize-small tss-fontweight-regular", text: text), InnerElement, _checkSpan);

            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();

            _observable = new SettableObservable<bool>(InnerElement.@checked);

            InnerElement.addEventListener("change", _ =>
            {
                _observable.Value = InnerElement.@checked;
            });
        }

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get => _label.innerText;
            set => _label.innerText = value;
        }

        /// <summary>
        /// Gets or sets whenever CheckBox is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => !_label.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _label.classList.remove("tss-disabled");
                }
                else
                {
                    _label.classList.add("tss-disabled");
                }
            }
        }

        /// <summary>
        /// Gets or sets whenever CheckBox is checked
        /// </summary>
        public bool IsChecked
        {
            get => InnerElement.@checked;
            set
            {
                InnerElement.@checked = value;
                _observable.Value     = value;
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return _label;
        }

        /// <summary>
        /// Disables the component.
        /// </summary>
        public CheckBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Marks the component as checked.
        /// </summary>
        public CheckBox Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public CheckBox SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>Gets or sets the text size.</summary>
        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(_label, TextSize.Small);
            set
            {
                _label.classList.remove(Size.ToString());
                _label.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text weight.</summary>
        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(_label, TextWeight.Regular);
            set
            {
                _label.classList.remove(Weight.ToString());
                _label.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text alignment.</summary>
        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(_label, TextAlign.Left);
            set
            {
                _label.classList.remove(TextAlign.ToString());
                _label.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<bool> AsObservable()
        {
            return _observable;
        }

        /// <summary>
        /// Programmatically updates the checkbox as part of a two-way binding (no DOM change-event echo).
        /// </summary>
        public void SetBoundValue(bool value) => IsChecked = value;
    }
}