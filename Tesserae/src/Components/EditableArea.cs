using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An inline-editable multi-line text surface that toggles between a read-only label and a textarea on click.
    /// </summary>
    [Transpose.Name("tss.EditableArea")]
    public sealed class EditableArea : ComponentBase<EditableArea, HTMLTextAreaElement>, ITextFormating, IBindableComponent<string>
    {
        private event SaveEditHandler Saved;
        public delegate bool          SaveEditHandler(EditableArea sender, string newValue);

        private readonly HTMLDivElement             _container;
        private readonly HTMLSpanElement            _labelText;
        private readonly HTMLDivElement             _editView;
        private readonly HTMLDivElement             _labelView;
        private readonly SettableObservable<string> _observable;

        private readonly HTMLElement _editIcon;
        private readonly HTMLElement _cancelEditIcon;

        private bool _isCanceling = false;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public EditableArea(string text = string.Empty)
        {
            _labelText = Span(Att("tss-editablelabel-textspan tss-fontcolor-default tss-fontsize-small tss-fontweight-regular", text: text, title: "Click to edit"));
            _editIcon  = I(Att($"tss-editablelabel-edit-icon {UIcons.Pencil}"));
            _labelView = Div(Att("tss-editablelabel-displaybox"), _labelText, _editIcon);


            InnerElement = UI.TextArea(Att("tss-editablelabel-textbox tss-fontcolor-default tss-fontsize-small tss-fontweight-regular", type: "text"));
            _cancelEditIcon = Div(Att("tss-editablelabel-cancel-icon",                                                                  title: "Cancel edit"), I(Att(UIcons.Cross.ToString())));
            _editView       = Div(Att("tss-editablelabel-editbox"),                                                                                            InnerElement, _cancelEditIcon);

            _container = Div(Att("tss-editablelabel "), _labelView, _editView);


            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            _observable = new SettableObservable<string>(text);

            _labelView.addEventListener("click", BeginEditing);
            _cancelEditIcon.addEventListener("click", CancelEditing);

            OnKeyUp((_, e) =>
            {
                if (e.key == "Escape")
                {
                    CancelEditing();
                }
            });

            OnBlur((_, __) => window.setTimeout(SaveEditing, 150)); // We need to do this on a timeout, because clicking on the Cancel would trigger this method first, with no opportunity to cancel
            
        }

        /// <summary>
        /// Gets or sets the size of the component.
        /// </summary>
        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                string current = Size.ToString();
                InnerElement.classList.remove(current);
                _labelText.classList.remove(current);
                _editIcon.classList.remove(current);
                _cancelEditIcon.classList.remove(current);

                string newValue = value.ToString();
                InnerElement.classList.add(newValue);
                _labelText.classList.add(newValue);
                _editIcon.classList.add(newValue);
                _cancelEditIcon.classList.add(newValue);
            }
        }

        /// <summary>
        /// Gets or sets the font weight of the component.
        /// </summary>
        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                var current  = Weight.ToString();
                var newValue = value.ToString();

                InnerElement.classList.remove(current);
                _labelText.classList.remove(current);
                InnerElement.classList.add(newValue);
                _labelText.classList.add(newValue);
            }
        }

        /// <summary>
        /// Gets or sets the text alignment of the component.
        /// </summary>
        public TextAlign TextAlign
        {
            get
            {
                return ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Left);
            }
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Returns a value indicating whether the component is editing mode.
        /// </summary>
        public bool IsEditingMode
        {
            get => _container.classList.contains("tss-editing");
            set
            {
                if (value)
                {
                    var labelRect = (DOMRect)_labelText.getBoundingClientRect();
                    InnerElement.style.minWidth  = (labelRect.width  * 1.2) + "px";
                    InnerElement.style.minHeight = (labelRect.height * 1.2) + "px";
                    _container.classList.add("tss-editing");
                }
                else
                {
                    _container.classList.remove("tss-editing");
                }
            }
        }

        /// <summary>
        /// Registers a callback invoked when the save event fires.
        /// </summary>
        public EditableArea OnSave(SaveEditHandler onSave)
        {
            Saved += onSave;
            return this;
        }

        private void BeginEditing()
        {
            InnerElement.value = _labelText.textContent;
            IsEditingMode      = true;
            _isCanceling       = false;
            InnerElement.focus();
        }

        private void CancelEditing()
        {
            _isCanceling  = true;
            IsEditingMode = false;
            InnerElement.blur();
        }

        private void SaveEditing(object e)
        {
            if (_isCanceling) return;

            var newValue = InnerElement.value;

            if (newValue != _labelText.textContent)
            {
                if (Saved is null || Saved(this, newValue))
                {
                    _labelText.textContent = newValue;
                    _observable.Value      = newValue;
                    IsEditingMode          = false;
                }
                else
                {
                    InnerElement.focus();
                }
            }
        }

        /// <summary>
        /// Sets the text of the component.
        /// </summary>
        public EditableArea SetText(string text)
        {
            if (IsEditingMode)
            {
                InnerElement.value = text;
            }
            else
            {
                _labelText.textContent = text;
            }

            _observable.Value = text;

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => _container;

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<string> AsObservable() => _observable;

        /// <summary>
        /// Programmatically updates the text as part of a two-way binding.
        /// </summary>
        public void SetBoundValue(string value) => SetText(value);
    }
}