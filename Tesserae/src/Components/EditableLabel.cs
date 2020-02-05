using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class EditableLabel : ComponentBase<EditableLabel, HTMLInputElement>, IHasTextSize
    {
        protected readonly HTMLDivElement _container;

        protected readonly HTMLSpanElement _labelText;

        protected          HTMLElement    _editIcon;
        protected          HTMLElement    _cancelEditIcon;
        protected readonly HTMLDivElement _editView;
        protected readonly HTMLDivElement _labelView;

        public delegate bool SaveEdit(EditableLabel sender, string newValue);

        public event SaveEdit onSave;

        private bool _isCanceling = false;

        public TextSize Size
        {
            get
            {
                return TextSizeExtensions.FromClassList(InnerElement, TextSize.Small);
            }
            set
            {
                InnerElement.classList.remove(Size.ToClassName());
                InnerElement.classList.add(value.ToClassName());
                _editIcon.classList.remove(Size.ToClassName());
                _editIcon.classList.add(value.ToClassName());
                _cancelEditIcon.classList.remove(Size.ToClassName());
                _cancelEditIcon.classList.add(value.ToClassName());
            }
        }

        public TextWeight Weight
        {
            get
            {
                return TextSizeExtensions.FromClassList(InnerElement, TextWeight.Regular);
            }
            set
            {
                InnerElement.classList.remove(Weight.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public EditableLabel(string text = string.Empty)
        {
            _labelText  = Span(_("tss-editablelabel-textspan", text: text, title: "Click to edit"));
            _editIcon   = I(_("tss-editablelabel-edit-icon far fa-edit"));
            _labelView  = Div(_("tss-editablelabel-displaybox"), _labelText, _editIcon);

            InnerElement     = TextBox(_("tss-editablelabel-textbox", type: "text", value: text));
            _cancelEditIcon  = Div(_("tss-editablelabel-cancel-icon", title:"Cancel edit"), I(_("far fa-times")));
            _editView        = Div(_("tss-editablelabel-editbox"), InnerElement, _cancelEditIcon);

            _container = Div(_("tss-editablelabel"), _labelView, _editView);

            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
            AttachKeys();

            _labelView.addEventListener("click",      BeginEditing);
            _cancelEditIcon.addEventListener("click", CancelEditing);

            OnKeyUp(KeyUp);
            OnBlur(BeginSaveEditing);
        }

        private void KeyUp(EditableLabel sender, KeyboardEvent e)
        {
            if(e.key == "Enter")
            {
                BeginSaveEditing(sender, e);
            }
            else if(e.key == "Escape")
            {
                CancelEditing(sender);
            }
        }

        public bool IsEditingMode
        {
            get { return _container.classList.contains("editing"); }
            set
            {
                if (value)
                {
                    _container.classList.add("editing");
                }
                else
                {
                    _container.classList.remove("editing");
                }
            }
        }

        private HTMLDivElement ShownElement()
        {
            return _editView.hidden ? _labelView : _editView;
        }

        public EditableLabel OnSave(SaveEdit onSave)
        {
            this.onSave += onSave;
            return this;
        }

        protected void BeginEditing(object sender)
        {
            IsEditingMode = true;
            _isCanceling = false;
            InnerElement.focus();
        }

        protected void CancelEditing(object sender)
        {
            _isCanceling = true;
            IsEditingMode = false;
            InnerElement.blur();
        }

        private void BeginSaveEditing(EditableLabel sender, Event e)
        {
            //We need to do this on a timeout, because clicking on the Cancel would trigger this method first, 
            //with no opportunity to cancel
            window.setTimeout(SaveEditing, 150);
        }

        private void SaveEditing(object e)
        {
            if (_isCanceling) return;
            
            var newValue = InnerElement.value;

            if(onSave is null || onSave(this, newValue))
            {
                _labelText.textContent = newValue;
                IsEditingMode = false;
            }
            else
            {
                InnerElement.focus();
            }
        }

        public override HTMLElement Render()
        {
            return _container;
        }
    }
}