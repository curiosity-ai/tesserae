using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class ChoiceGroup : ComponentBase<ChoiceGroup, HTMLDivElement>, IContainer<ChoiceGroup, ChoiceGroup.Option>
    {
        private readonly TextBlock _header;

        public ChoiceGroup(string label = "Pick one")
        {
            _header = (new TextBlock(label)).SemiBold();
            var h = _header.Render();
            h.style.alignSelf = "baseline";
            InnerElement = Div(_("tss-choice-group", styles: s => { s.flexDirection = "column"; }), h);
        }

        public Option SelectedOption { get; private set; }

        public string Label
        {
            get { return _header.Text; }
            set { _header.Text = value; }
        }

        public ChoiceGroupOrientation Orientation
        {
            get
            {
                return InnerElement.style.flexDirection == "row"
                    ? ChoiceGroupOrientation.Horizontal
                    : ChoiceGroupOrientation.Vertical;
            }
            set
            {
                if (value == ChoiceGroupOrientation.Horizontal) InnerElement.style.flexDirection = "row";
                else InnerElement.style.flexDirection = "column";
            }
        }

        public bool IsRequired
        {
            get { return _header.IsRequired; }
            set { _header.IsRequired = value; }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(Option component)
        {
            ScrollBar.GetCorrectContainer(InnerElement).appendChild(component.Render());
            component.OnSelect += OnChoiceSelected;

            if (component.IsSelected) OnChoiceSelected(null, component);
        }

        public void Clear()
        {
            var container = ScrollBar.GetCorrectContainer(InnerElement);
            ClearChildren(container);
            ScrollBar.GetCorrectContainer(InnerElement).appendChild(_header.Render());
        }

        public void Replace(Option newComponent, Option oldComponent)
        {
            ScrollBar.GetCorrectContainer(InnerElement).replaceChild(newComponent.Render(), oldComponent.Render());
            newComponent.OnSelect += OnChoiceSelected;
        }
        public ChoiceGroup Options(params ChoiceGroup.Option[] children)
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

        private void OnChoiceSelected(object sender, Option e)
        {
            if (SelectedOption == e) return;
            if (SelectedOption != null) SelectedOption.IsSelected = false;
            SelectedOption = e;
            RaiseOnChange(e);
        }

        public enum ChoiceGroupOrientation
        {
            Vertical,
            Horizontal
        }

        public class Option : ComponentBase<Option, HTMLInputElement>
        {
            private readonly HTMLSpanElement _radioSpan;
            private readonly HTMLLabelElement _label;

            public event EventHandler<Option> OnSelect;

            public Option(string text)
            {
                InnerElement = RadioButton(_("tss-option"));
                _radioSpan = Span(_("tss-option-mark"));
                _label = Label(_("tss-option-container", text: text), InnerElement, _radioSpan);
                AttachClick();
                AttachChange();
                AttachFocus();
                AttachBlur();
                onChange += (s, e) =>
                {
                    if (IsSelected) OnSelect?.Invoke(this, this);
                };
            }

            public bool IsEnabled
            {
                get { return !_label.classList.contains("disabled"); }
                set
                {
                    if (value != IsEnabled)
                    {
                        if (value)
                        {
                            _label.classList.remove("disabled");
                        }
                        else
                        {
                            _label.classList.add("disabled");
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
            public Option Disabled()
            {
                IsEnabled = false;
                return this;
            }

            public Option Selected()
            {
                IsSelected = true;
                return this;
            }

            public Option OnSelected(EventHandler<ChoiceGroup.Option> onSelected)
            {
                OnSelect += onSelected;
                return this;
            }

            public Option SetText(string text)
            {
                Text = text;
                return this;
            }
        }
    }
}
