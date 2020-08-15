using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class CheckBox : ComponentBase<CheckBox, HTMLInputElement>, IBindableComponent<bool>
    {
        private readonly HTMLSpanElement  _checkSpan;
        private readonly HTMLLabelElement _label;

        private SettableObservable<bool>           _observable;
        private ObservableEvent.ValueChanged<bool> valueGetter;
        private bool                               _observableReferenceUsed = false;

        public SettableObservable<bool> Observable
        {
            get
            {
                _observableReferenceUsed = true;
                return _observable;
            }
            set
            {
                if (_observableReferenceUsed)
                {
                    throw new ArgumentException("Can't set the observable after a reference of it has been used! (.AsObservable() might have been called before .Bind())");
                }

                if (_observable is object)
                    _observable.StopObserving(valueGetter);
                _observable = value;
                _observable.Observe(valueGetter);
            }
        }

        public CheckBox(string text = string.Empty)
        {

            
            InnerElement = CheckBox(_("tss-checkbox"));
            _checkSpan   = Span(_("tss-checkbox-mark"));
            _label       = Label(_("tss-checkbox-container", text: text), InnerElement, _checkSpan);
            
            valueGetter = v => IsChecked = v;
            Observable  = new SettableObservable<bool>();
            
            InnerElement.onchange += (e) =>
            {
                StopEvent(e);
                IsChecked = IsChecked;
                RaiseOnChange(ev: null);
            };

            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
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
    }
}