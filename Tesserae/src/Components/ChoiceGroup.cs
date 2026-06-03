using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A group of radio-style choices of which exactly one may be selected at a time.
    /// </summary>
    [H5.Name("tss.ChoiceGroup")]
    public sealed class ChoiceGroup : ComponentBase<ChoiceGroup, HTMLDivElement>, IContainer<ChoiceGroup, ChoiceGroup.Choice>, IBindableComponent<ChoiceGroup.Choice>
    {
        private readonly string                     _name;
        private readonly TextBlock                  _header;
        private readonly SettableObservable<Choice> _selectedOption;
        private static   int                        _count = 0;
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ChoiceGroup(string label = "Pick one")
        {
            _count++;
            _name = $"choice-group-{_count}";
            var headerId = $"choice-group-header-{_count}";
            _header = (new TextBlock(label)).SemiBold().Id(headerId);
            var h = _header.Render();
            h.style.alignSelf = "baseline";
            _selectedOption   = new SettableObservable<Choice>();
            InnerElement      = Div(_("tss-choice-group tss-default-component-margin", role: "radiogroup", ariaLabelledBy: headerId, styles: s => { s.flexDirection = "column"; }), h);
        }

        /// <summary>
        /// Gets or sets the selected option.
        /// </summary>
        public Choice SelectedOption { get => _selectedOption.Value; private set => _selectedOption.Value = value; }

        /// <summary>
        /// Gets or sets the label shown by the component.
        /// </summary>
        public string Label
        {
            get => _header.Text;
            set => _header.Text = value;
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        public ChoiceGroupOrientation Orientation
        {
            get => InnerElement.style.flexDirection == "row" ? ChoiceGroupOrientation.Horizontal : ChoiceGroupOrientation.Vertical;
            set
            {
                if (value == ChoiceGroupOrientation.Horizontal) InnerElement.style.flexDirection = "row";
                else InnerElement.style.flexDirection                                            = "column";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the component is required for form submission.
        /// </summary>
        public bool IsRequired
        {
            get => _header.IsRequired;
            set => _header.IsRequired = value;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(Choice component)
        {
            component.Name(_name);
            InnerElement.appendChild(component.Render());

            component.OnSelected(OnChoiceSelected);

            if (component.IsSelected)
                OnChoiceSelected(component);
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public void Clear()
        {
            var container = InnerElement;
            ClearChildren(container);
            InnerElement.appendChild(_header.Render());
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(Choice newComponent, Choice oldComponent)
        {
            newComponent.Name(_name);
            InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
            newComponent.OnSelected(OnChoiceSelected);
        }

        /// <summary>
        /// Configures the component to choices.
        /// </summary>
        public ChoiceGroup Choices(params ChoiceGroup.Choice[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        /// <summary>
        /// Configures the component to horizontal.
        /// </summary>
        public ChoiceGroup Horizontal()
        {
            Orientation = ChoiceGroup.ChoiceGroupOrientation.Horizontal;
            return this;
        }
        /// <summary>
        /// Configures the component to vertical.
        /// </summary>
        public ChoiceGroup Vertical()
        {
            Orientation = ChoiceGroup.ChoiceGroupOrientation.Vertical;
            return this;
        }

        /// <summary>
        /// Marks the component as required.
        /// </summary>
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

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<Choice> AsObservable() => _selectedOption;

        /// <summary>
        /// Programmatically selects a choice as part of a two-way binding.
        /// Choices not in this group are ignored.
        /// </summary>
        public void SetBoundValue(Choice value)
        {
            if (value == null) return;
            if (SelectedOption == value) return;
            value.IsSelected = true;
            // Choice raises its SelectedItem event from the DOM 'change' handler, which fires only
            // on real user input. Programmatically toggling @checked doesn't echo, so funnel the
            // selection back into the group here.
            OnChoiceSelected(value);
        }

        public enum ChoiceGroupOrientation
        {
            Vertical,
            Horizontal
        }

        public sealed class Choice : ComponentBase<Choice, HTMLInputElement>, ITextFormating
        {
            private event ComponentEventHandler<Choice> SelectedItem;

            private readonly HTMLSpanElement  _radioSpan;
            private readonly HTMLLabelElement _label;
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Choice(string text)
            {
                InnerElement = RadioButton(_("tss-option"));
                _radioSpan   = Span(_("tss-option-mark"));
                _label       = Label(_("tss-option-container tss-default-component-margin tss-fontcolor-default tss-fontsize-small tss-fontweight-regular", text: text), InnerElement, _radioSpan);
                AttachClick();
                AttachChange();
                AttachFocus();
                AttachBlur();

                Changed += (s, e) =>
                {
                    if (IsSelected) SelectedItem?.Invoke(this);
                };
            }

            /// <summary>
            /// Gets or sets a value indicating whether the component is interactive (enabled).
            /// </summary>
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

            /// <summary>
            /// Gets or sets a value indicating whether the component is selected.
            /// </summary>
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

            /// <summary>
            /// Gets or sets the text shown in the component.
            /// </summary>
            public string Text
            {
                get { return _label.innerText; }
                set { _label.innerText = value; }
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
            public Choice Disabled(bool value = true)
            {
                IsEnabled = !value;
                return this;
            }

            /// <summary>
            /// Marks the component as selected.
            /// </summary>
            public Choice Selected()
            {
                IsSelected = true;
                return this;
            }

            /// <summary>
            /// Configures the selected if on the component.
            /// </summary>
            public Choice SelectedIf(bool shouldSelect)
            {
                if (shouldSelect)
                {
                    IsSelected = true;
                }
                return this;
            }

            /// <summary>
            /// Registers a callback invoked when the selected event fires.
            /// </summary>
            public Choice OnSelected(ComponentEventHandler<Choice> onSelected)
            {
                SelectedItem += onSelected;
                return this;
            }

            /// <summary>
            /// Sets the text of the component.
            /// </summary>
            public Choice SetText(string text)
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

            internal void Name(string name)
            {
                InnerElement.name = name;
            }
        }
    }
}