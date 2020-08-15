﻿using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>, IBindableComponent<bool>
    {
        private readonly HTMLElement _checkElement;
        private readonly HTMLElement _onOffSpan;
        private readonly HTMLElement _container;
        private readonly IComponent  _offText;
        private readonly IComponent  _onText;

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


        public Toggle(IComponent onText = null, IComponent offText = null)
        {
            _onText       = onText  ?? TextBlock("On");
            _offText      = offText ?? TextBlock("Off");
            InnerElement  = CheckBox(_("tss-checkbox"));
            _checkElement = Div(_("tss-toggle-mark"));
            _onOffSpan    = Div(_("tss-toggle-text"),      _offText.Render());
            _container    = Div(_("tss-toggle-container"), InnerElement, _checkElement, _onOffSpan);

            _checkElement.onclick += (e) =>
            {
                StopEvent(e);
                IsChecked = !IsChecked;
                OnToggleChanged();
                RaiseOnChange(ev: null);
            };

            valueGetter = v => IsChecked = v;
            _observable = new SettableObservable<bool>();

            OnChange((s, e) => OnToggleChanged());
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        /// <summary>
        /// Gets or sets toggle text
        /// </summary>
        public string Text
        {
            get => _container.innerText;
            set
            {
                _container.innerText = value;
                if (string.IsNullOrEmpty(value)) _onOffSpan.style.display = "";
                else _onOffSpan.style.display                             = "none";
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is enabled
        /// </summary>
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

        /// <summary>
        /// Gets or sets whenever Toggle is checked
        /// </summary>
        public bool IsChecked
        {
            get => InnerElement.@checked;
            set
            {
                InnerElement.@checked = value;
                _observable.Value     = value;
                ClearChildren(_onOffSpan);
                if (value)
                {
                    _onOffSpan.appendChild(_onText.Render());
                }
                else
                {
                    _onOffSpan.appendChild(_offText.Render());
                }
            }
        }

        public override HTMLElement Render()
        {
            return _container;
        }

        private void OnToggleChanged()
        {
            ClearChildren(_onOffSpan);
            if (IsChecked)
            {
                _onOffSpan.appendChild(_onText.Render());
            }
            else
            {
                _onOffSpan.appendChild(_offText.Render());
            }

            _observable.Value = IsChecked;
        }

        public Toggle SetText(string text)
        {
            Text = text;
            return this;
        }

        public Toggle Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public Toggle Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }
    }
}