using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ChecBox")]
    public class CheckBox : ComponentBase<CheckBox, HTMLInputElement>, IObservableComponent<bool>
    {
        private readonly HTMLSpanElement          _checkSpan;
        private readonly HTMLLabelElement         _label;
        private readonly SettableObservable<bool> _observable;

        public CheckBox(string text = string.Empty)
        {
            InnerElement = CheckBox(_("tss-checkbox"));
            _checkSpan   = Span(_("tss-checkbox-mark"));
            _label       = Label(_("tss-checkbox-container tss-default-component-margin tss-fontcolor-default tss-fontsize-small tss-fontweight-regular", text: text), InnerElement, _checkSpan);

            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();

            _observable = new SettableObservable<bool>();
            _observable.Value = InnerElement.@checked;

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

        public override HTMLElement Render()
        {
            return _label;
        }

        public CheckBox Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public CheckBox Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }

        public CheckBox SetText(string text)
        {
            Text = text;
            return this;
        }

        public IObservable<bool> AsObservable()
        {
            return _observable;
        }
    }
}