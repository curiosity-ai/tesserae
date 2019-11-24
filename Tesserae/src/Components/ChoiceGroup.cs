using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Option : ComponentBase<Option, HTMLInputElement>
    {
        #region Fields

        private HTMLSpanElement _RadioSpan;
        private HTMLLabelElement _Label;

        #endregion

        #region Events

        public event EventHandler<Option> OnSelect;

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get { return !_Label.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
                {
                    if (value)
                    {
                        _Label.classList.remove("disabled");
                    }
                    else
                    {
                        _Label.classList.add("disabled");
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
                    //if (value) OnSelect?.Invoke(this, this);
                    InnerElement.@checked = value;
                }
            }
        }

        public string Text
        {
            get { return _Label.innerText; }
            set { _Label.innerText = value; }
        }

        #endregion

        public Option(string text)
        {
            InnerElement = CheckBox(_("mss-option"));
            _RadioSpan = Span(_("mss-option-mark"));
            _Label = Label(_("m-1 mss-option-container", text: text), InnerElement, _RadioSpan);
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();

            OnChange += (s, e) => OnSelect?.Invoke(this, this);
        }

        public override HTMLElement Render()
        {
            return _Label;
        }
    }


    public enum ChoiceGroupOrientation
    {
        Vertical,
        Horizontal
    }

    public class ChoiceGroup : ComponentBase<ChoiceGroup, HTMLDivElement>, IContainer<Stack>
    {
        #region Fields

        private TextBlock _Header;

        #endregion

        #region Properties

        public Option SelectedOption { get; private set; }

        public string Label
        {
            get { return _Header.Text; }
            set { _Header.Text = value; }
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
            get { return _Header.IsRequired; }
            set { _Header.IsRequired = value; }
        }

        #endregion

        public ChoiceGroup(string label)
        {
            _Header = (new TextBlock(label)).SemiBold();
            InnerElement = Div(_("m-1 mss-choice-group", styles: s => { s.flexDirection = "row"; }), _Header.Render());
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public void Add(IComponent component)
        {
            InnerElement.appendChild(component.Render());
            var choice = component as Option;
            choice.OnSelect += OnChoiceSelected;

            if (choice.IsSelected) OnChoiceSelected(null, choice);
        }

        private void OnChoiceSelected(object sender, Option e)
        {
            if (SelectedOption != null) SelectedOption.IsSelected = false;
            RaiseOnChange(e);
            SelectedOption = e;
        }

        public void Clear()
        {
            ClearChildren(InnerElement);
            InnerElement.appendChild(_Header.Render());
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
            var choice = oldComponent as Option;
            choice.OnSelect += OnChoiceSelected;
        }
    }

    public static class ChoiceExtensions
    {
        public static ChoiceGroup Options(this ChoiceGroup container, params Option[] children)
        {
            children.ForEach(x => container.Add(x));
            return container;
        }

        public static ChoiceGroup Horizontal(this ChoiceGroup container)
        {
            container.Orientation = ChoiceGroupOrientation.Horizontal;
            return container;
        }
        public static ChoiceGroup Vertical(this ChoiceGroup container)
        {
            container.Orientation = ChoiceGroupOrientation.Vertical;
            return container;
        }

        public static ChoiceGroup Required(this ChoiceGroup container)
        {
            container.IsRequired = true;
            return container;
        }
        public static Option Disabled(this Option option)
        {
            option.IsEnabled = false;
            return option;
        }

        public static Option Selected(this Option option)
        {
            option.IsSelected = true;
            return option;
        }
    }
}
