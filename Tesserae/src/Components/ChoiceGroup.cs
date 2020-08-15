﻿using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class ChoiceGroup : ComponentBase<ChoiceGroup, HTMLDivElement>, IContainer<ChoiceGroup, ChoiceGroup.Choice>, IObservableComponent<ChoiceGroup.Choice>
    {
        private readonly TextBlock _header;

        private SettableObservable<Choice> _selectedOption;

        public IObservable<Choice> Observable => _selectedOption;

        public ChoiceGroup(string label = "Pick one")
        {
            _header = (new TextBlock(label)).SemiBold();
            var h = _header.Render();
            h.style.alignSelf = "baseline";
            InnerElement      = Div(_("tss-choice-group", styles: s => { s.flexDirection = "column"; }), h);

            _selectedOption = new SettableObservable<Choice>();
        }

        public Choice SelectedOption
        {
            get => _selectedOption.Value;
            private set => _selectedOption.Value = value;
        }

        public string Label
        {
            get => _header.Text;
            set => _header.Text = value;
        }

        public ChoiceGroupOrientation Orientation
        {
            get => InnerElement.style.flexDirection == "row" ? ChoiceGroupOrientation.Horizontal : ChoiceGroupOrientation.Vertical;
            set
            {
                if (value == ChoiceGroupOrientation.Horizontal) InnerElement.style.flexDirection = "row";
                else InnerElement.style.flexDirection                                            = "column";
            }
        }

        public bool IsRequired
        {
            get => _header.IsRequired;
            set => _header.IsRequired = value;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(Choice component)
        {
            ScrollBar.GetCorrectContainer(InnerElement).appendChild(component.Render());

            component.OnSelected(OnChoiceSelected);

            if (component.IsSelected)
                OnChoiceSelected(component);
        }

        public void Clear()
        {
            var container = ScrollBar.GetCorrectContainer(InnerElement);
            ClearChildren(container);
            ScrollBar.GetCorrectContainer(InnerElement).appendChild(_header.Render());
        }

        public void Replace(Choice newComponent, Choice oldComponent)
        {
            ScrollBar.GetCorrectContainer(InnerElement).replaceChild(newComponent.Render(), oldComponent.Render());
            newComponent.OnSelected(OnChoiceSelected);
        }

        public ChoiceGroup Choices(params ChoiceGroup.Choice[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        public ChoiceGroup Horizontal()
        {
            Orientation = ChoiceGroup.ChoiceGroupOrientation.Horizontal;
            return this;
        }

        public ChoiceGroup Vertical()
        {
            Orientation = ChoiceGroup.ChoiceGroupOrientation.Vertical;
            return this;
        }

        public ChoiceGroup Required()
        {
            IsRequired = true;
            return this;
        }

        private void OnChoiceSelected(Choice sender)
        {
            if (SelectedOption == sender)
                return;

            if (SelectedOption is object)
                SelectedOption.IsSelected = false;

            SelectedOption = sender;

            RaiseOnChange(ev: null);
        }

        public enum ChoiceGroupOrientation
        {
            Vertical,
            Horizontal
        }

        public sealed class Choice : ComponentBase<Choice, HTMLInputElement>
        {
            private event ComponentEventHandler<Choice> SelectedItem;

            private readonly HTMLSpanElement  _radioSpan;
            private readonly HTMLLabelElement _label;

            public Choice(string text)
            {
                InnerElement = RadioButton(_("tss-option"));
                _radioSpan   = Span(_("tss-option-mark"));
                _label       = Label(_("tss-option-container", text: text), InnerElement, _radioSpan);
                AttachClick();
                AttachChange();
                AttachFocus();
                AttachBlur();
                Changed += (s, e) =>
                {
                    if (IsSelected) SelectedItem?.Invoke(this);
                };
            }

            public bool IsEnabled
            {
                get { return !_label.classList.contains("tss-disabled"); }
                set
                {
                    if (value != IsEnabled)
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
            }

            public bool IsSelected
            {
                get { return InnerElement.@checked; }
                set
                {
                    if (value != IsSelected)
                    {
                        InnerElement.@checked = value;
                    }
                }
            }

            public string Text
            {
                get { return _label.innerText; }
                set { _label.innerText = value; }
            }

            public override HTMLElement Render()
            {
                return _label;
            }

            public Choice Disabled(bool value = true)
            {
                IsEnabled = !value;
                return this;
            }

            public Choice Selected()
            {
                IsSelected = true;
                return this;
            }

            public Choice SelectedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    IsSelected = true;
                }

                return this;
            }

            public Choice OnSelected(ComponentEventHandler<Choice> onSelected)
            {
                SelectedItem += onSelected;
                return this;
            }

            public Choice SetText(string text)
            {
                Text = text;
                return this;
            }
        }
    }
}