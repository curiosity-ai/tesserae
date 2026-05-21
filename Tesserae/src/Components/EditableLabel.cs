using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// An inline-editable single-line text surface that toggles between a read-only label and a textbox on click.
    /// </summary>
    [H5.Name("tss.EditableLabel")]
    public sealed class EditableLabel : ComponentBase<EditableLabel, HTMLInputElement>, ITextFormating, IObservableComponent<string>
    {
        private event SaveEditHandler Saved;
        public delegate bool          SaveEditHandler(EditableLabel sender, string newValue);

        private readonly HTMLDivElement             _container;
        private readonly HTMLSpanElement            _labelText;
        private readonly HTMLElement                _editIcon;
        private readonly HTMLElement                _cancelEditIcon;
        private readonly HTMLDivElement             _editView;
        private readonly HTMLDivElement             _labelView;
        private readonly SettableObservable<string> _observable;

        private bool _isCanceling = false;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public EditableLabel(string text = string.Empty)
        {
            _labelText = Span(_("tss-editablelabel-textspan", text: text, title: "Click to edit"));
            _editIcon  = I(_($"tss-editablelabel-edit-icon {UIcons.Pencil}"));
            _labelView = Div(_("tss-editablelabel-displaybox"), _labelText, _editIcon);

            InnerElement    = TextBox(_("tss-editablelabel-textbox", type: "text"));
            _cancelEditIcon = Div(_("tss-editablelabel-cancel-icon", title: "Cancel edit"), I(_(UIcons.Cross.ToString())));
            _editView       = Div(_("tss-editablelabel-editbox"),                           InnerElement, _cancelEditIcon);

            _container = Div(_("tss-editablelabel tss-fontcolor-default tss-fontsize-small tss-fontweight-regular"), _labelView, _editView);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            _labelView.addEventListener("click", BeginEditing);
            _cancelEditIcon.addEventListener("click", CancelEditing);

            OnKeyUp((_, e) =>
            {
                if (e.key == "Enter")
                    BeginSaveEditing();
                else if (e.key == "Escape")
                    CancelEditing();
            });

            OnBlur((_, __) => BeginSaveEditing());
            
            _observable = new SettableObservable<string>(text); //Avoid raising the event once
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
                    var labelRect = _labelText.getBoundingClientRect().As<DOMRect>();
                    var parentRect = _container.getBoundingClientRect().As<DOMRect>();
                    var targetWidth = parentRect.width > 100 ? Math.Min(labelRect.width * 1.2, parentRect.width - 48) : labelRect.width * 1.2;
                    InnerElement.style.minWidth = targetWidth + "px";
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
        public EditableLabel OnSave(SaveEditHandler onSave)
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

        private void BeginSaveEditing() => window.setTimeout(SaveEditing, 150); // We need to do this on a timeout, because clicking on the Cancel would trigger this method first, with no opportunity to cancel

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
        public EditableLabel SetText(string text)
        {
            if (IsEditingMode)
                InnerElement.value = text;
            else
                _labelText.textContent = text;

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
    }
}