using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.EditableLabel")]
    public sealed class EditableLabel : ComponentBase<EditableLabel, HTMLInputElement>, ITextFormating, IObservableComponent<string>
    {
        private event SaveEditHandler Saved;
        public delegate bool SaveEditHandler(EditableLabel sender, string newValue);

        private readonly HTMLDivElement _container;
        private readonly HTMLSpanElement _labelText;
        private readonly HTMLElement _editIcon;
        private readonly HTMLElement _cancelEditIcon;
        private readonly HTMLDivElement _editView;
        private readonly HTMLDivElement _labelView;
        private readonly SettableObservable<string> _observable = new SettableObservable<string>();

        private bool _isCanceling = false;

        public EditableLabel(string text = string.Empty)
        {
            _labelText = Span(_("tss-editablelabel-textspan", text: text, title: "Click to edit"));
            _editIcon = I(_("tss-editablelabel-edit-icon las la-edit"));
            _labelView = Div(_("tss-editablelabel-displaybox"), _labelText, _editIcon);

            InnerElement = TextBox(_("tss-editablelabel-textbox", type: "text"));
            _cancelEditIcon = Div(_("tss-editablelabel-cancel-icon", title: "Cancel edit"), I(_("las la-times")));
            _editView = Div(_("tss-editablelabel-editbox"), InnerElement, _cancelEditIcon);

            _container = Div(_("tss-editablelabel tss-fontsize-small"), _labelView, _editView);

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
        }

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

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                var current = Weight.ToString(); 
                var newValue = value.ToString();

                InnerElement.classList.remove(current);
                _labelText.classList.remove(current);
                InnerElement.classList.add(newValue);
                _labelText.classList.add(newValue);
            }
        }

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

        public bool IsEditingMode
        {
            get => _container.classList.contains("tss-editing");
            set
            {
                if (value)
                {
                    var labelRect = (DOMRect)_labelText.getBoundingClientRect();
                    InnerElement.style.minWidth = (labelRect.width * 1.2) + "px";
                    _container.classList.add("tss-editing");
                }
                else
                {
                    _container.classList.remove("tss-editing");
                }
            }
        }

        public EditableLabel OnSave(SaveEditHandler onSave)
        {
            Saved += onSave;
            return this;
        }

        private void BeginEditing()
        {
            InnerElement.value = _labelText.textContent;
            IsEditingMode = true;
            _isCanceling = false;
            InnerElement.focus();
        }

        private void CancelEditing()
        {
            _isCanceling = true;
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
                    _observable.Value = newValue;
                    IsEditingMode = false;
                }
                else
                {
                    InnerElement.focus();
                }
            }
        }

        public EditableLabel SetText(string text)
        {
            if (IsEditingMode)
                InnerElement.value = text;
            else
                _labelText.textContent = text;

            _observable.Value = text;

            return this;
        }

        public override HTMLElement Render() => _container;

        public IObservable<string> AsObservable() => _observable;
    }
}